using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Localization
{

    /// <summary>
    /// Resource manager class that retrieves localized strings from resources.
    /// </summary>
    public class LocalizationHelper : AbstractHelper<LocalizationHelper>
    {
        #region "Variables"

        private const string SITE_RESOURCE_ROOT_FOLDER = "App_Data";
        private const string SITE_RESOURCE_RESX_FOLDER = "Resources";
        private const int MAX_KEY_LENGTH = 50;
        private static FileResourceManager mEmptyManager;
        private static string mDefaultPathPrefix;
        private static string[] mDefaultResourceFiles;
        private static Regex mRegExResourceName;
        private static Regex mRegexSpecialMacro;
        private static readonly object lockObject = new object();

        private FileResourceManager mDefaultManager;
        private StringSafeDictionary<FileResourceManager> mFileManagers;

        // If true, SQL Resource manager if used as primary source to retrieve the strings.
        private readonly BoolAppSetting mUseSQLResourceManagerAsPrimary = new BoolAppSetting("CMSUseSQLResourceManagerAsPrimary", true);

        // Localization string sources to be used when sql source has higher priority.
        private readonly ILocalizationStringSource[] mSQLPrioritizedStringSources = { SQLLocalizationSource.Instance, FileLocalizationSource.Instance };

        // Localization string sources to be used when file resx source has higher priority.
        private readonly ILocalizationStringSource[] mFilePrioritizedStringSources = { FileLocalizationSource.Instance, SQLLocalizationSource.Instance };

        #endregion


        #region "Properties"

        /// <summary>
        /// <para>
        /// Default path prefix to be used when resolving path to CMSResources folder. Gets <see cref="SystemContext.WebApplicationPhysicalPath"/> unless explicitly set.
        /// Existing resource file managers must be cleared upon set by calling <see cref="Clear"/> for the change to take effect on them.
        /// </para>
        /// <para>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </para>
        /// </summary>
        public static string DefaultPathPrefix
        {
            get
            {
                return mDefaultPathPrefix ?? SystemContext.WebApplicationPhysicalPath;
            }
            set
            {
                mDefaultPathPrefix = value;
            }
        }


        /// <summary>
        /// List of the default resource files.
        /// </summary>
        public static string[] DefaultResourceFiles
        {
            get
            {
                return mDefaultResourceFiles ?? (mDefaultResourceFiles = new[] { "First", "Fast", "CMS_Install", "CMS", "Custom", "Hotfix", "Last" });
            }
            set
            {
                mDefaultResourceFiles = value;
            }
        }


        /// <summary>
        /// Resource name regular expression.
        /// </summary>
        public static Regex RegExResourceName
        {
            get
            {
                return mRegExResourceName ?? (mRegExResourceName = RegexHelper.GetRegex("^(?:\\$)(.*)(?:\\$)$"));
            }
            set
            {
                mRegExResourceName = value;
            }
        }


        /// <summary>
        /// If true, SQL Resource manager if used as primary source to retrieve the strings.
        /// </summary>
        public static bool UseSQLResourceManagerAsPrimary
        {
            get
            {
                return HelperObject.mUseSQLResourceManagerAsPrimary.Value;
            }
            set
            {
                HelperObject.mUseSQLResourceManagerAsPrimary.Value = value;
            }
        }


        /// <summary>
        /// Returns default file resource manager.
        /// </summary>
        public static FileResourceManager DefaultManager
        {
            get
            {
                return HelperObject.DefaultManagerInternal;
            }
        }


        /// <summary>
        /// Gets or sets String separator.
        /// </summary>
        public static char StringSeparator { get; set; } = '|';


        /// <summary>
        /// Gets or sets Culture separator.
        /// </summary>
        public static char CultureSeparator { get; set; } = '=';


        /// <summary>
        /// Gets Localization regular expression.
        /// </summary>
        private static Regex RegExLocalize
        {
            get
            {
                // Expressing groups:        (1:selector                             )
                return RegexHelper.GetRegex("(?:\\{\\$)((?:[^\\$]|(?:\\$(?=[^\\}])))*)(?:\\$\\})");
            }
        }


        /// <summary>
        /// Returns default file resource manager.
        /// </summary>
        private static FileResourceManager EmptyManager
        {
            get
            {
                return LockHelper.Ensure(ref mEmptyManager, lockObject);
            }
        }


        /// <summary>
        /// Regular expression for resolving special macro.
        /// </summary>
        private static Regex RegexSpecialMacro
        {
            get
            {
                return mRegexSpecialMacro ?? (mRegexSpecialMacro = RegexHelper.GetRegex("##(.+?)\\|(.*?)##"));
            }
        }


        /// <summary>
        /// Returns default file resource manager.
        /// </summary>
        protected FileResourceManager DefaultManagerInternal
        {
            get
            {
                return LockHelper.Ensure(ref mDefaultManager, GetDefaultManager, lockObject);
            }
            set
            {
                lock (lockObject)
                {
                    mDefaultManager = value;
                }
            }
        }


        /// <summary>
        /// File managers.
        /// </summary>
        private StringSafeDictionary<FileResourceManager> FileManagers
        {
            get
            {
                return LockHelper.Ensure(ref mFileManagers, lockObject);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default resource manager
        /// </summary>
        private static FileResourceManager GetDefaultManager()
        {
            // Load the default manager for default culture
            var filepath = GetResourceFilePaths(CultureHelper.DefaultUICultureCode);
            if (filepath != null)
            {
                return new FileResourceManager(filepath, CultureHelper.DefaultUICultureCode);
            }

            return EmptyManager;
        }


        /// <summary>
        /// Returns true if resource file exists for given culture.
        /// </summary>
        /// <param name="cultureCode">Culture code (e.g. "en-US")</param>
        /// <param name="pathPrefix">Path prefix used to locate the resource files</param>
        public static bool ResourceFileExistsForCulture(string cultureCode, string pathPrefix = null)
        {
            string path = GetCMSResourceFilePath(cultureCode, pathPrefix);
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            return File.Exists(path);
        }


        /// <summary>
        /// Checks if resource string key is unique.
        /// </summary>
        /// <param name="resKey">Resource key</param>
        private static bool IsResourceKeyUnique(string resKey)
        {
            return ResourceStringInfoProvider.GetResourceStringInfo(resKey) == null;
        }


        /// <summary>
        /// Returns unique resource string key from given plain text.
        /// </summary>
        /// <param name="resKeyPrefix">Prefix of the resource string that should be used</param>
        /// <param name="plainText">Plain text from which the string should be generated</param>
        /// <param name="maxKeyLength">Maximal resource string key length. Length of the resource string key is limited to 50 chars.</param>
        public static string GetUniqueResStringKey(string plainText, string resKeyPrefix = "", int maxKeyLength = MAX_KEY_LENGTH)
        {
            if (maxKeyLength <= 0 || maxKeyLength > MAX_KEY_LENGTH)
            {
                maxKeyLength = MAX_KEY_LENGTH;
            }

            // Trim resource string to max length if needed
            string resKey = resKeyPrefix + ValidationHelper.GetCodeName(plainText, maxKeyLength - resKeyPrefix.Length);

            string baseResKey = resKey;
            int i = 1;
            while (!IsResourceKeyUnique(resKey))
            {
                resKey = baseResKey;

                string suffixNumber = i.ToString();
                if (resKey.Length + suffixNumber.Length > maxKeyLength)
                {
                    resKey = resKey.Substring(0, maxKeyLength - suffixNumber.Length);
                }

                resKey += suffixNumber;
                i++;
            }

            return resKey;
        }


        /// <summary>
        /// Creates a new instance of file manager.
        /// </summary>
        /// <param name="culture">Culture</param>
        public static FileResourceManager CreateFileManager(string culture)
        {
            // Initialize the resource manager
            string filepath = GetResourceFilePaths(culture);
            if (filepath != null)
            {
                return new FileResourceManager(filepath, culture);
            }

            return null;
        }


        /// <summary>
        /// Clears the current instances of the file managers which can retrieve data from the resource files.
        /// This will ensure reloading the resource strings from the resource files.
        /// </summary>
        public static void Clear()
        {
            lock (lockObject)
            {
                HelperObject.mFileManagers = null;
            }
        }


        /// <summary>
        /// Returns the names of resource files (including their paths) for the specified culture.
        /// </summary>
        /// <param name="cultureCode">The culture name in the format <c>languagecode2-country/regioncode2</c>, or an empty string for the default localization.</param>
        /// <param name="pathPrefix">The path to the folder with resource files (optional).</param>
        /// <returns>A semi-colon separated list of the full names of resource files (including their paths) for the specified culture, or NULL if no resource files are found.</returns>
        public static string GetResourceFilePaths(string cultureCode, string pathPrefix = null)
        {
            var resourceFilePaths = new List<string>();

            if (String.IsNullOrEmpty(cultureCode) || cultureCode.Equals(CultureHelper.DefaultUICultureCode, StringComparison.InvariantCultureIgnoreCase))
            {
                cultureCode = String.Empty;
            }

            var applicationPath = String.IsNullOrEmpty(pathPrefix) ? DefaultPathPrefix : pathPrefix;
            if (!String.IsNullOrEmpty(applicationPath))
            {
                var basePath = Path.Combine(applicationPath, "CMSResources");
                if (Directory.Exists(basePath))
                {
                    resourceFilePaths.AddRange(GetDefaultResourcesFilePaths(basePath, cultureCode));
                    resourceFilePaths.AddRange(GetModulesResourcesFilePaths(basePath, cultureCode));
                }

                var sitesResourcesBasePath = Path.Combine(applicationPath, SITE_RESOURCE_ROOT_FOLDER);
                if (Directory.Exists(sitesResourcesBasePath))
                {
                    resourceFilePaths.AddRange(GetSitesResourcesFilePaths(sitesResourcesBasePath, cultureCode));
                }
            }

            return resourceFilePaths.Any() ? String.Join(";", resourceFilePaths.Distinct()) : null;
        }


        /// <summary>
        /// Returns an enumerable collection of the names of default resource files (including their paths) for the specified culture.
        /// </summary>
        /// <param name="baseFolderPath">The path to the folder with resource files.</param>
        /// <param name="cultureCode">The culture name in the format <c>languagecode2-country/regioncode2</c>, or an empty string for the default localization.</param>
        /// <returns>An enumerable collection of the full names of default resource files (including their paths) for the specified culture</returns>
        private static IEnumerable<string> GetDefaultResourcesFilePaths(string baseFolderPath, string cultureCode)
        {
            var resourceFilePaths = new List<string>();
            foreach (var resourceGroupName in DefaultResourceFiles)
            {
                var resourceGroupFileName = String.IsNullOrEmpty(cultureCode) ? String.Format("{0}.resx", resourceGroupName) : String.Format("{0}.{1}.resx", resourceGroupName, cultureCode);
                var resourceFilePath = Path.Combine(baseFolderPath, resourceGroupFileName);
                if (File.Exists(resourceFilePath))
                {
                    resourceFilePaths.Add(resourceFilePath);
                }
            }

            return resourceFilePaths;
        }


        /// <summary>
        /// Returns an enumerable collection of the names of modular resource files (including their paths) for the specified culture.
        /// </summary>
        /// <param name="baseFolderPath">The path to the folder with resource files.</param>
        /// <param name="cultureCode">The culture name in the format <c>languagecode2-country/regioncode2</c>, or an empty string for the default localization.</param>
        /// <returns>An enumerable collection of the full names of modular resource files (including their paths) for the specified culture</returns>
        private static IEnumerable<string> GetModulesResourcesFilePaths(string baseFolderPath, string cultureCode)
        {
            var resourceFilePaths = new List<string>();
            foreach (var moduleFolderPath in Directory.GetDirectories(baseFolderPath))
            {
                var paths = GetFolderResourcesFilePaths(moduleFolderPath, cultureCode);
                resourceFilePaths.AddRange(paths);
            }

            return resourceFilePaths;
        }


        /// <summary>
        /// Returns an enumerable collection of the names of site resource files (including their paths) for the specified culture.
        /// </summary>
        /// <param name="appDataFolderPath">The path to the site folder with resource files.</param>
        /// <param name="cultureCode">The culture name in the format <c>languagecode2-country/regioncode2</c>, or an empty string for the default localization.</param>
        /// <returns>An enumerable collection of the full names of site resource files (including their paths) for the specified culture</returns>
        private static IEnumerable<string> GetSitesResourcesFilePaths(string appDataFolderPath, string cultureCode)
        {
            var siteResourceFilePaths = new List<string>();
            foreach (var siteFolderPath in Directory.GetDirectories(appDataFolderPath))
            {
                // Search for resource files in the folder /App_Data/<siteName>/Resources/
                var siteResourcesFolderPath = Path.Combine(siteFolderPath, SITE_RESOURCE_RESX_FOLDER);
                if (Directory.Exists(siteResourcesFolderPath))
                {
                    // Collect all existing site resource files
                    var paths = GetFolderResourcesFilePaths(siteResourcesFolderPath, cultureCode);
                    siteResourceFilePaths.AddRange(paths);
                }
            }

            return siteResourceFilePaths;
        }


        /// <summary>
        /// Returns an enumerable collection of the names of resource files in the specified folder.
        /// </summary>
        /// <param name="folderPath">The path to the folder with resource files.</param>
        /// <param name="cultureCode">The culture name in the format <c>languagecode2-country/regioncode2</c>, or an empty string for the default localization.</param>
        /// <returns>An enumerable collection of the full names of resource files in the specified folder, or NULL if no resource files are found.</returns>
        public static IEnumerable<string> GetFolderResourcesFilePaths(string folderPath, string cultureCode)
        {
            if (String.IsNullOrEmpty(cultureCode))
            {
                return Directory.GetFiles(folderPath, "*.resx");
            }

            var cultureFolderPath = Path.Combine(folderPath, cultureCode);
            if (Directory.Exists(cultureFolderPath))
            {
                return Directory.GetFiles(cultureFolderPath, "*.resx");
            }

            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// Gets the path to the resource file.
        /// </summary>
        /// <param name="cultureCode">Culture to load</param>
        /// <param name="pathPrefix">Path prefix used to locate the resource files</param>
        /// <returns>(e.g for "cs-CZ" returns "...\CMSResources\CMS.cs-cz.resx")</returns>
        public static string GetCMSResourceFilePath(string cultureCode, string pathPrefix = null)
        {
            // If default culture, do not add culture suffix
            if (String.IsNullOrEmpty(cultureCode) || cultureCode.EqualsCSafe(CultureHelper.DefaultUICultureCode, true))
            {
                cultureCode = String.Empty;
            }
            else
            {
                cultureCode = "." + cultureCode;
            }

            cultureCode += ".resx";

            // Initialize the resource manager
            string appPath = String.IsNullOrEmpty(pathPrefix) ? DefaultPathPrefix : pathPrefix.TrimEnd('\\');

            if (!String.IsNullOrEmpty(appPath))
            {
                return appPath + "\\CMSResources\\CMS" + cultureCode;
            }

            return null;
        }


        /// <summary>
        /// Returns the file resource manager for given culture.
        /// </summary>
        public static FileResourceManager GetFileManager(string culture)
        {
            return HelperObject.GetFileManagerInternal(culture);
        }


        /// <summary>
        /// Gets the given string and formats it with the standard String.Format method
        /// </summary>
        /// <param name="stringName">String name</param>
        /// <param name="parameters">Parameters for the formatting</param>
        [Obsolete("Use CMS.Helpers.ResHelper.GetStringFormat instead.")]
        public static string GetStringFormat(string stringName, params object[] parameters)
        {
            return String.Format(GetStringCore(stringName), parameters);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        [Obsolete("Use CMS.Helpers.ResHelper.GetString instead.")]
        public static string GetString(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return GetStringCore(stringName, culture, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Value to return in case string not found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        /// <param name="allowMultiple">If true, multiple string variants are allowed</param>
        [Obsolete("Use CMS.Helpers.ResHelper.GetString instead.")]
        public static string GetString(string stringName, string culture, string defaultValue, bool useDefaultCulture = true, bool allowMultiple = true)
        {
            return GetStringCore(stringName, culture, defaultValue, useDefaultCulture, allowMultiple);
        }


        internal static string GetStringCore(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return GetStringCore(stringName, culture, stringName, useDefaultCulture);
        }


        internal static string GetStringCore(string stringName, string culture, string defaultValue, bool useDefaultCulture = true, bool allowMultiple = true)
        {
            return HelperObject.GetStringInternal(stringName, culture, defaultValue, useDefaultCulture, allowMultiple);
        }


        /// <summary>
        /// Finds and resolves special macros within localization string.
        /// </summary>
        /// <param name="input">Input string in which special macros are resolved</param>
        /// <param name="culture">Culture code</param>
        /// <param name="stringName">Resource string name</param>
        private static string ResolveSpecialMacros(string input, string culture, string stringName)
        {
            if (input != null)
            {
                // Check whether string contains special macro
                int startIndex = input.IndexOf("##", StringComparison.Ordinal);
                if (startIndex >= 0)
                {
                    string output = RegexSpecialMacro.Replace(input, m =>
                    {
                        if (m.Groups.Count == 3)
                        {
                            // Pass current localization values to event arguments
                            var args = new LocalizationEventArgs
                            {
                                CultureCode = culture,
                                MacroValue = m.Groups[2].Value,
                                ResourceStringKey = stringName,
                                MacroType = m.Groups[1].Value
                            };

                            LocalizationEvents.ResolveSubstitutionMacro.StartEvent(args);

                            if (args.IsMatch)
                            {
                                return args.MacroResult;
                            }
                        }

                        return m.Value;
                    }, 50, startIndex);

                    return output;
                }
            }

            return input;
        }


        /// <summary>
        /// Gets the string using the given string keys. Returns the first available item found.
        /// </summary>
        /// <param name="stringNames">String names</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        [Obsolete("Method is not intended to be used in custom code.")]
        public static string GetString(string[] stringNames, string culture, string defaultValue, bool useDefaultCulture = true)
        {
            return GetFirstString(stringNames, culture, defaultValue, useDefaultCulture);
        }


        /// <summary>
        /// Gets the string using the given string keys. Returns the first available item found.
        /// </summary>
        /// <param name="stringNames">String names</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        private static string GetFirstString(string[] stringNames, string culture, string defaultValue, bool useDefaultCulture = true)
        {
            if (stringNames == null)
            {
                return null;
            }

            // Set culture if not set
            if (String.IsNullOrEmpty(culture))
            {
                culture = Thread.CurrentThread.CurrentUICulture.ToString();
            }

            // Go through all the keys
            foreach (string key in stringNames)
            {
                if (!String.IsNullOrEmpty(key))
                {
                    string result = GetStringCore(key, culture, null, useDefaultCulture, false);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            // Return all the keys if not found
            return defaultValue;
        }


        /// <summary>
        /// Returns specified string from the resources.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        [Obsolete("Use CMS.Helpers.ResHelper.GetFileString instead.")]
        public static string GetFileString(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return GetFileStringCore(stringName, culture, stringName, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string from the resource files.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value to return in case no string found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        [Obsolete("Use CMS.Helpers.ResHelper.GetFileString instead.")]
        public static string GetFileString(string stringName, string culture, string defaultValue, bool useDefaultCulture = true)
        {
            return GetFileStringCore(stringName, culture, defaultValue, useDefaultCulture);
        }


        internal static string GetFileStringCore(string stringName, string culture = null, bool useDefaultCulture = true)
        {
            return GetFileStringCore(stringName, culture, stringName, useDefaultCulture);
        }


        internal static string GetFileStringCore(string stringName, string culture, string defaultValue, bool useDefaultCulture = true)
        {
            return HelperObject.GetFileStringInternal(stringName, culture, defaultValue, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string for the API usage (the default value is used when string is not found).
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="defaultValue">Value to return in case string not found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        [Obsolete("Use CMS.Helpers.ResHelper.GetAPIString instead.")]
        public static string GetAPIString(string stringName, string defaultValue, bool useDefaultCulture = true)
        {
            return GetStringCore(stringName, null, defaultValue, useDefaultCulture);
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Value to return in case string not found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        [Obsolete("Use CMS.Helpers.ResHelper.GetAPIString instead.")]
        public static string GetAPIString(string stringName, string culture, string defaultValue, bool useDefaultCulture = true)
        {
            return GetStringCore(stringName, culture, defaultValue, useDefaultCulture);
        }


        /// <summary>
        /// Localizes the given expression, handles two types of expressions:
        /// 
        /// stringkey - Simple localization
        /// 
        /// =default string|cs-cz=localized string - advanced localization
        /// </summary>
        /// <param name="expression">Expression to localize</param>
        /// <param name="culture">Culture to use for localization</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="getStringMethod">Method to get the resource string for localization</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        [Obsolete("Use CMS.Helpers.ResHelper.LocalizeExpression instead.")]
        public static string LocalizeExpression(string expression, string culture = null, bool encode = false, Func<string, string, bool, string> getStringMethod = null, bool useDefaultCulture = true)
        {
            return LocalizeExpressionCore(expression, culture, encode, getStringMethod, useDefaultCulture);
        }


        internal static string LocalizeExpressionCore(string expression, string culture = null, bool encode = false, Func<string, string, bool, string> getStringMethod = null, bool useDefaultCulture = true)
        {
            string result;
            if (expression.StartsWithCSafe(LocalizationHelper.CultureSeparator.ToString()))
            {
                // Load culture if not given
                if (culture == null)
                {
                    culture = Thread.CurrentThread.CurrentUICulture.ToString().ToLowerCSafe() + LocalizationHelper.CultureSeparator;
                }
                else
                {
                    culture = culture.ToLowerCSafe() + LocalizationHelper.CultureSeparator;
                }

                string[] parts = expression.Substring(1).Split(LocalizationHelper.StringSeparator);

                // Find the culture
                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i].ToLowerCSafe().StartsWithCSafe(culture))
                    {
                        return parts[i].Substring(culture.Length);
                    }
                }

                // Culture not found, return default string                
                result = parts[0];
            }
            else
            {
                if (getStringMethod == null)
                {
                    getStringMethod = GetStringCore;
                }

                result = getStringMethod(expression.Trim(), culture, useDefaultCulture);
            }

            if (encode)
            {
                result = HTMLHelper.HTMLEncode(result);
            }

            return result;
        }


        /// <summary>
        /// Trim starting and ending '$' chars, if both exists only. Returns trimmed string.
        /// </summary>
        /// <param name="value">String to process</param>
        public static string GetResourceName(string value)
        {
            string resourceName = value;

            if ((value != null) && (RegExResourceName.Match(value).Success))
            {
                resourceName = RegExResourceName.Replace(value, "$1");
            }

            return resourceName;
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings using given culture.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        /// <param name="culture">Culture</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        internal static string LocalizeString(string inputText, string culture = null, bool encode = false, bool useDefaultCulture = true)
        {
            return LocalizeString(inputText, GetStringCore, culture, useDefaultCulture, encode);
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        internal static string LocalizeFileString(string inputText)
        {
            return LocalizeString(inputText, GetFileStringCore);
        }


        /// <summary>
        /// Returns true, if the given text contains a localization macro
        /// </summary>
        /// <param name="inputText">Text to check</param>
        internal static bool ContainsLocalizationMacro(string inputText)
        {
            if (String.IsNullOrEmpty(inputText))
            {
                return false;
            }

            return inputText.Contains("{$");
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Value to return in case string not found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        /// <param name="allowMultiple">If true, multiple string variants are allowed</param>
        protected virtual string GetStringInternal(string stringName, string culture, string defaultValue, bool useDefaultCulture = true, bool allowMultiple = true)
        {
            // Do not process
            if (stringName == null)
            {
                return null;
            }

            // Handle multiple strings
            if (allowMultiple && (stringName.IndexOfCSafe('|') >= 0))
            {
                return GetFirstString(stringName.Split('|'), culture, defaultValue, useDefaultCulture);
            }

            // Set culture if not set
            if (String.IsNullOrEmpty(culture))
            {
                culture = Thread.CurrentThread.CurrentUICulture.ToString();
            }

            // Get localization string sources with correct order
            var prioritizedSources = UseSQLResourceManagerAsPrimary ? mSQLPrioritizedStringSources : mFilePrioritizedStringSources;
            // Get string from sources
            var result = GetStringFromLocalizationSource(stringName, culture, useDefaultCulture, prioritizedSources);

            // If still not found, return default value
            result = result == null ? defaultValue : result;

            // Resolve special macros contained in the localization string (only if not disabled via action context)
            if (LocalizationActionContext.CurrentResolveSubstitutionMacros)
            {
                result = ResolveSpecialMacros(result, culture, stringName);
            }

            return result;
        }


        /// <summary>
        /// Returns specified string from the resource files.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value to return in case no string found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        protected virtual string GetFileStringInternal(string stringName, string culture, string defaultValue, bool useDefaultCulture = true)
        {
            if (culture == null)
            {
                culture = Thread.CurrentThread.CurrentUICulture.ToString();
            }

            // Get the manager
            var manager = GetFileManagerInternal(culture);

            // Get the string
            string result = manager.GetString(stringName);

            // If not found, get default language string
            if ((result == null) && useDefaultCulture && (manager.Culture != DefaultManagerInternal.Culture))
            {
                result = DefaultManagerInternal.GetString(stringName);
            }

            // If result still not found, return the key
            return result ?? defaultValue;
        }


        /// <summary>
        /// Returns the file resource manager for given culture.
        /// </summary>
        protected virtual FileResourceManager GetFileManagerInternal(string culture)
        {
            // Prepare the culture
            culture = culture ?? CultureHelper.DefaultUICultureCode;

            // Get the manager from the hashtable
            var result = FileManagers[culture];
            if (result == null)
            {
                // Load the manager
                if (culture == CultureHelper.DefaultUICultureCode)
                {
                    result = DefaultManagerInternal;
                }
                else
                {
                    result = CreateFileManager(culture) ?? DefaultManagerInternal;
                }

                // Save it
                FileManagers[culture] = result;
            }

            return result;
        }


        /// <summary>
        /// Returns specified localized string from given sources. Priority of the sources is given by their order.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        /// <param name="sources">Localization string sources</param>
        private string GetStringFromLocalizationSource(string stringName, string culture, bool useDefaultCulture, IEnumerable<ILocalizationStringSource> sources)
        {
            // Get requested culture value
            var result = sources.Select(source => source.GetString(stringName, culture)).FirstOrDefault(s => s != null);

            // Try to load the default culture value.
            if ((result == null) && useDefaultCulture)
            {
                result = sources.Select(source => source.GetString(stringName, source.DefaultCulture)).FirstOrDefault(s => s != null);
            }

            return result;
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings. 
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        /// <param name="getStringMethod">Method to get the resource string for localization</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        internal static string LocalizeString(string inputText, Func<string, string, bool, string> getStringMethod = null, string culture = null, bool useDefaultCulture = true, bool encode = false)
        {
            if (String.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            if (!ContainsLocalizationMacro(inputText))
            {
                return inputText;
            }

            if (getStringMethod == null)
            {
                getStringMethod = GetStringCore;
            }

            return RegExLocalize.Replace(inputText, m => LocalizedStringMatch(m, culture, encode, getStringMethod, useDefaultCulture));
        }


        /// <summary>
        /// Match evaluator for the string localization.
        /// </summary>
        /// <param name="m">Regular expression match</param>
        /// <param name="culture">Culture to use for localization</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="getStringMethod">Method to provide the resource string in case of localization</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        private static string LocalizedStringMatch(Match m, string culture, bool encode, Func<string, string, bool, string> getStringMethod, bool useDefaultCulture)
        {
            string expression = m.Groups[1].ToString();

            return LocalizeExpressionCore(expression, culture, encode, getStringMethod, useDefaultCulture);
        }

        #endregion
    }
}
