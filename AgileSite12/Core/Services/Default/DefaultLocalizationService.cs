using System;

namespace CMS.Core
{
    /// <summary>
    /// Default localization service
    /// </summary>
    internal class DefaultLocalizationService : ILocalizationService
    {
        /// <summary>
        /// Returns original resource key
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public string GetString(string resourceKey, string culture = null, bool useDefaultCulture = true)
        {
            return resourceKey;
        }


        /// <summary>
        /// Returns original resource key
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <param name="culture">Culture</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public string GetFileString(string resourceKey, string culture = null, bool useDefaultCulture = true)
        {
            return resourceKey;
        }


        /// <summary>
        /// Returns specified string from the resource files.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value to return in case no string found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exists</param>
        public string GetFileString(string stringName, string culture, string defaultValue, bool useDefaultCulture = true)
        {
            return stringName;
        }


        /// <summary>
        /// Returns specified string.
        /// </summary>
        /// <param name="stringName">Key of the string</param>
        /// <param name="culture">Culture</param>
        /// <param name="defaultValue">Default value</param>
        public string GetAPIString(string stringName, string culture, string defaultValue)
        {
            return defaultValue;
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
        public string LocalizeExpression(string expression, string culture = null, bool encode = false, Func<string, string, bool, string> getStringMethod = null, bool useDefaultCulture = true)
        {
            return expression;
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings using given culture.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        /// <param name="culture">Culture</param>
        /// <param name="encode">If true, translations (replacements) are HTML encoded</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public string LocalizeString(string inputText, string culture = null, bool encode = false, bool useDefaultCulture = true)
        {
            return inputText;
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions</param>
        public string LocalizeFileString(string inputText)
        {
            return inputText;
        }


        /// <summary>
        /// Clears the cached resource strings and forces them to load again on next localization request.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public void ClearCache(bool logTasks)
        {
            // Does nothing by default
        }
    }
}
