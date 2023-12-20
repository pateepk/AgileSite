using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Url processing methods.
    /// </summary>
    public class URLHelper
    {
        #region "Constants"

        /// <summary>
        /// Used in system requests of live site pages, it prevents the system to log statistics or activities, e.g. when dynamic newsletter is generated.
        /// </summary>
        public const string SYSTEM_QUERY_PARAMETER = "systemrequest";

        // Ampersand macro used for replacement in UrlEncodeQueryString() method.
        private const string AMPERSAND = "##AMP##";

        // Equals macro used for replacement in UrlEncodeQueryString() method.
        private const string EQUALS = "##EQ##";

        private const string DEFAULT_DOCUMENT_RELATIVE_PATH = "/default.aspx";

        #endregion


        #region "Public Constants"

        /// <summary>
        /// Constant for HTTP port.
        /// </summary>
        public const int DEFAULT_HTTP_PORT = 80;

        /// <summary>
        /// Constant for HTTPS port.
        /// </summary>
        public const int DEFAULT_HTTPS_PORT = 443;

        #endregion


        #region "Variables"

        private string mDomainName;
        private static string mPortalTemplatePage;
        private static string mPortalTemplatePageCheck;
        private static Regex mProtocolRegex;

        #endregion


        #region "Delegates"

        /// <summary>
        /// Handler to modify the existing given path
        /// </summary>
        /// <param name="path">Original path</param>
        public delegate string PathModifierHandler(string path);

        #endregion


        #region "Events"

        /// <summary>
        /// Method handling an event fired for all parts of the URL (splitted by '/' character) to get a safe URL (without forbidden characters)
        /// </summary>
        /// <param name="part">Part of the URL without </param>
        /// <param name="siteName">Site name</param>
        /// <param name="e">EventArgs</param>
        /// <returns>Returns false if original method should not be used.</returns>
        public delegate bool OnBeforeGetSafeUrlPartEventHandler(ref string part, string siteName, EventArgs e);


        /// <summary>
        /// Occurs when the GetSafeUrlPart method is called, returns false if the original method ensuring safe URL part should not be used.
        /// </summary>
        public static event OnBeforeGetSafeUrlPartEventHandler OnBeforeGetSafeUrlPart;

        #endregion


        #region "Static variables"

        private static StringSafeDictionary<Regex> mParamRegEx;
        private static Regex mRegExMakeLinksAbsolute;
        internal static string mForbiddenURLValues;
        private static string mLanguageParameterName;
        private static string mAliasPathParameterName;
        private static string mDefaultTheme;
        private static string mCustomTheme;
        private static bool? mUseTrailingSlashOnlyForExtensionLess;
        private static int mUrlPort = -1;
        private static int mSSLUrlPort = -1;
        private static bool? mLimitURLReplacements;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regex used to check or remove protocol
        /// </summary>
        private static Regex ProtocolRegex
        {
            get
            {
                // http://tools.ietf.org/html/rfc3986#page-17
                return mProtocolRegex ?? (mProtocolRegex = RegexHelper.GetRegex("(?<protocol>^[a-z][a-z0-9.+-]*)://", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }


        /// <summary>
        /// Page used as the portal template base page
        /// </summary>
        public static string PortalTemplatePage
        {
            get
            {
                return mPortalTemplatePage ?? (mPortalTemplatePage = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSPortalTemplatePage"], "~/CMSPages/PortalTemplate.aspx"));
            }
            set
            {
                mPortalTemplatePage = value;
                PortalTemplatePageCheck = null;
            }
        }


        /// <summary>
        /// Page used for check of the portal template page
        /// </summary>
        private static string PortalTemplatePageCheck
        {
            get
            {
                return mPortalTemplatePageCheck ?? (mPortalTemplatePageCheck = PortalTemplatePage.TrimStart('~').ToLowerCSafe());
            }
            set
            {
                mPortalTemplatePageCheck = value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether multiple, starting and trailing replacements should be limited.
        /// </summary>
        private static bool UseLimitURLReplacements
        {
            get
            {
                if (mLimitURLReplacements == null)
                {
                    mLimitURLReplacements = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLimitUrlReplacements"], true);
                }
                return mLimitURLReplacements.Value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether trailing slash handling
        /// should be processed for all requests or only for extension less requests
        /// </summary>
        public static bool UseTrailingSlashOnlyForExtensionLess
        {
            get
            {
                if (mUseTrailingSlashOnlyForExtensionLess == null)
                {
                    mUseTrailingSlashOnlyForExtensionLess = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseTrailingSlashOnlyForExtensionLess"], false);
                }

                return mUseTrailingSlashOnlyForExtensionLess.Value;
            }
        }


        /// <summary>
        /// Parameter regular expression [name] -> [RegEx]
        /// </summary>
        private static StringSafeDictionary<Regex> ParamRegEx
        {
            get
            {
                return mParamRegEx ?? (mParamRegEx = new StringSafeDictionary<Regex>());
            }
        }


        /// <summary>
        /// Make links absolute regular expression.
        /// </summary>
        public static Regex RegExMakeLinksAbsolute
        {
            get
            {
                // Expression groups:                                                              (1:prefix                )      (quote    )(2:lnk)      (quote     )(3:link   )
                return mRegExMakeLinksAbsolute ?? (mRegExMakeLinksAbsolute = RegexHelper.GetRegex("(\\s(?:src|href)\\s*=\\s*)(?:(?:(?<quote>')([^']*)')|(?:(?<quote>\")([^\"]*)\"))"));
            }
            set
            {
                mRegExMakeLinksAbsolute = value;
            }
        }


        /// <summary>
        /// Language query parameter name.
        /// </summary>
        public static string LanguageParameterName
        {
            get
            {
                return mLanguageParameterName ?? (mLanguageParameterName = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSLanguageParameterName"], "lang"));
            }
        }


        /// <summary>
        /// Alias path query parameter name.
        /// </summary>
        public static string AliasPathParameterName
        {
            get
            {
                return mAliasPathParameterName ?? (mAliasPathParameterName = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAliasPathParameterName"], "aliaspath"));
            }
        }


        /// <summary>
        /// Default theme name.
        /// </summary>
        public static string DefaultTheme
        {
            get
            {
                return mDefaultTheme ?? (mDefaultTheme = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSDefaultTheme"], "Default"));
            }
            set
            {
                mDefaultTheme = value;
            }
        }


        /// <summary>
        /// Custom theme name.
        /// </summary>
        public static string CustomTheme
        {
            get
            {
                return mCustomTheme ?? (mCustomTheme = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSCustomTheme"], DefaultTheme));
            }
            set
            {
                mCustomTheme = value;
            }
        }


        /// <summary>
        /// Application's URL port.
        /// </summary>
        public static int UrlPort
        {
            get
            {
                if (mUrlPort == -1)
                {
                    mUrlPort = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSUrlPort"], 0);
                }

                return mUrlPort;
            }
            set
            {
                mUrlPort = value;
            }
        }


        /// <summary>
        /// Application's SSL URL port.
        /// </summary>
        public static int SSLUrlPort
        {
            get
            {
                if (mSSLUrlPort == -1)
                {
                    mSSLUrlPort = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSSLUrlPort"], 0);
                }
                return mSSLUrlPort;
            }
            set
            {
                mSSLUrlPort = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if domain is IP address or top level domain e.g localhost
        /// </summary>
        /// <param name="domain">domain without protocol</param>
        public static bool IsTldDomain(string domain)
        {
            IPAddress addr;

            // Check IP addresses (IPv4/IPv6) or TLD e.g. localhost
            if (IPAddress.TryParse(domain, out addr) || !domain.Contains("."))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns regular expression for allowed URL characters.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetAllowedURLCharacters(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSAllowedURLCharacters"].ToString("");
        }


        /// <summary>
        /// Returns value that indicates whether lang prefix should be used for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UseLangPrefixForUrls(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSUseLangPrefixForUrls"].ToBoolean(false);
        }


        /// <summary>
        /// Returns string containing all forbidden characters that cannot be used in node alias and in the file system.
        /// </summary>
        public static string ForbiddenURLCharacters(string siteName)
        {
            // Initialize the default forbidden values
            if (mForbiddenURLValues == null)
            {
                mForbiddenURLValues = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSForbiddenURLValues"], "\\/:*?\"<>|&%.'#[]+ \t=„“");
            }

            // Get the settings value
            if (siteName == null)
            {
                return mForbiddenURLValues;
            }

            return mForbiddenURLValues + CoreServices.Settings[siteName + ".CMSForbiddenURLCharacters"].ToString("");
        }


        /// <summary>
        /// Returns string containing replacement for forbidden characters (chars that can't be used in node alias and in the file system).
        /// </summary>
        public static char ForbiddenCharactersReplacement(string siteName)
        {
            string replacement = CoreServices.Settings[siteName + ".CMSForbiddenCharactersReplacement"].ToString("");
            return (replacement == string.Empty) ? '_' : replacement[0];
        }


        /// <summary>
        /// Returns the physical file path that corresponds to the specified virtual path on the web server.
        /// </summary>
        /// <param name="path">File virtual path</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains illegal characters</exception>
        public static string GetPhysicalPath(string path)
        {
            if (path == null)
            {
                return null;
            }

            var context = CMSHttpContext.Current;
            if (context == null)
            {
                path = SystemContext.WebApplicationPhysicalPath + Path.EnsureBackslashes(GetVirtualPath(path).TrimStart('~'));
            }
            else
            {
                path = context.Server.MapPath(path);
            }

            return path;
        }


        /// <summary>
        /// Returns the safe equivalent to the given file name (with extension).
        /// </summary>
        /// <param name="fileName">Original file name</param>
        /// <param name="siteName">Site name</param>
        public static string GetSafeFileName(string fileName, string siteName)
        {
            return GetSafeFileName(fileName, siteName, true);
        }


        /// <summary>
        /// Returns the safe equivalent to the given file name.
        /// </summary>
        /// <param name="fileName">Original file name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="extensionIncluded">Indicates if given fileName contains extension</param>
        public static string GetSafeFileName(string fileName, string siteName, bool extensionIncluded)
        {
            // If fileName not specified, do not process
            if (fileName == null)
            {
                return "";
            }

            // Remove diacritics from the file name
            fileName = TextHelper.RemoveDiacritics(fileName.Trim());

            string forbiddenChars = ForbiddenURLCharacters(siteName);
            char replacement = ForbiddenCharactersReplacement(siteName);

            // Allow some of the characters that may appear within the URL
            forbiddenChars = forbiddenChars.Replace(".", "");

            // Replace the forbidden characters
            for (int i = 0; i <= forbiddenChars.Length - 1; i++)
            {
                fileName = fileName.Replace(forbiddenChars[i], replacement);
            }

            // Special treatment for '.' (dot) in the file name
            string urlDir = String.IsNullOrEmpty(fileName) ? "" : Path.EnsureSlashes(Path.GetDirectoryName(fileName));
            urlDir = (!string.IsNullOrEmpty(urlDir) ? urlDir.Replace('.', replacement) + '/' : "");

            string name;
            string ext = String.Empty;


            // Preserve extension '.' dot if included
            if (extensionIncluded)
            {
                name = Path.GetFileNameWithoutExtension(fileName);
                name = name.Replace('.', replacement);
                ext = Path.GetExtension(fileName);
                ext = ext.TrimStart('.');
                fileName = urlDir + name;
            }
            else
            {
                name = Path.GetFileName(fileName);
                name = name.Replace('.', replacement);
                fileName = urlDir + name;
            }

            // Try get allowed URL characters
            string allowedRegex = GetAllowedURLCharacters(siteName);
            if (!String.IsNullOrEmpty(allowedRegex))
            {
                if (!String.IsNullOrEmpty(fileName))
                {
                    fileName = Regex.Replace(fileName, "[^" + allowedRegex + "]+", replacement.ToString());
                }

                if (!String.IsNullOrEmpty(ext))
                {
                    ext = Regex.Replace(ext, "[^" + allowedRegex + "]+", replacement.ToString());
                }
            }

            // Limit replacements in filename
            if (!String.IsNullOrEmpty(fileName))
            {
                fileName = LimitURLReplacements(fileName, replacement);
            }

            // Limit replacements in extension
            if (!String.IsNullOrEmpty(ext))
            {
                ext = LimitURLReplacements(ext, replacement);
            }

            if ((extensionIncluded) && (!String.IsNullOrEmpty(ext)))
            {
                fileName += "." + ext;
            }

            return fileName;
        }


        /// <summary>
        /// Returns the safe part of URL. This method doesn't process complete URL it handles only part of URL.
        /// </summary>
        /// <param name="part">String with URL part which may contain URL-unsafe characters. Enter only part of URL without any '/' characters</param>
        /// <param name="siteName">Site name from where forbidden characters should be taken</param>
        /// <param name="removeDiacritics">Indicates if diacritics should be removed from the URL part</param>
        /// <param name="limitReplacements">Indicates if replacements should be aggregated into single replacement character if found multiple invalid characters in row</param>
        /// <param name="additionalAllowedCharacters">Additional allowed characters for the URL part</param>
        public static string GetSafeUrlPart(string part, string siteName, bool removeDiacritics = true, bool limitReplacements = true, string additionalAllowedCharacters = null)
        {
            // If URL is not specified, do not process
            if (part == null)
            {
                return "";
            }

            // Custom URL path settings
            if (OnBeforeGetSafeUrlPart != null)
            {
                if (!OnBeforeGetSafeUrlPart(ref part, siteName, null))
                {
                    return part;
                }
            }

            // Get site forbidden character and replacement
            string forbiddenChars = ForbiddenURLCharacters(siteName);
            char replacement = ForbiddenCharactersReplacement(siteName);
            string replacementString = replacement.ToString();

            // Remove additional allowed characters from the forbidden ones
            if (additionalAllowedCharacters != null)
            {
                for (int i = 0; i <= additionalAllowedCharacters.Length - 1; i++)
                {
                    forbiddenChars = forbiddenChars.Replace(additionalAllowedCharacters[i].ToString(), string.Empty);
                }
            }

            // Trim the white spaces from the beginning and the ned of the part
            part = part.Trim();

            // Remove diacritics from the URL
            if (removeDiacritics)
            {
                part = TextHelper.RemoveDiacritics(part);
            }

            // Remove special characters never acceptable in the path
            part = part.Replace("/", replacementString);
            part = part.Replace("\\", replacementString);

            // Replace the forbidden characters
            for (int i = 0; i <= forbiddenChars.Length - 1; i++)
            {
                part = part.Replace(forbiddenChars[i], replacement);
            }

            // Try get allowed URL characters
            string allowedRegex = GetAllowedURLCharacters(siteName);
            if (!String.IsNullOrEmpty(allowedRegex))
            {
                // Add additional allowed characters
                allowedRegex += additionalAllowedCharacters;
                part = Regex.Replace(part, "[^" + allowedRegex + "]+", replacementString);
            }

            // Limit replacements
            if (limitReplacements)
            {
                part = LimitURLReplacements(part, replacement);
            }

            return part;
        }


        /// <summary>
        /// Limits multiple replacements, starting and trailing replacements from URL. Can be disabled by app key: CMSLimitUrlReplacement.
        /// </summary>
        /// <param name="url">Input URL path</param>
        /// <param name="replacement">Replacement character</param>
        public static string LimitURLReplacements(string url, char replacement)
        {
            // If URL is not specified or is '/', do not process
            if (string.IsNullOrEmpty(url) || (url == "/"))
            {
                return url;
            }

            string replacementString = replacement.ToString();

            // Limit replacements if it's required
            if (UseLimitURLReplacements)
            {
                url = Regex.Replace(url, "[" + replacementString + "]+", replacementString);

                bool addStratingSlash = false;

                // Remove starting path in URL path
                if (url.StartsWithCSafe("/"))
                {
                    addStratingSlash = true;
                    url = url.Substring(1);
                }

                string trimmedUrl = url.Trim(replacement);

                url = !String.IsNullOrEmpty(trimmedUrl) ? trimmedUrl : replacementString;

                // Add starting slash if this slash was removed
                if (addStratingSlash)
                {
                    url = "/" + url;
                }
            }

            return url;
        }


        /// <summary>
        /// Returns true if the given domain names match.
        /// </summary>
        /// <param name="first">First domain name</param>
        /// <param name="second">Second domain name</param>
        /// <param name="checkPort">If true, port is checked</param>
        public static bool DomainMatch(string first, string second, bool checkPort)
        {
            // Trim www if present
            first = RemoveWWW(first);
            second = RemoveWWW(second);

            if (checkPort)
            {
                // Trim port 80 (default)
                if (first.EndsWithCSafe(":80"))
                {
                    first = first.Substring(first.Length - 3);
                }
                if (second.EndsWithCSafe(":80"))
                {
                    second = second.Substring(second.Length - 3);
                }
            }
            else
            {
                // Trim port
                first = RemovePort(first);
                second = RemovePort(second);
            }

            // Compare
            return first.Equals(second, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Removes the port part from the domain name.
        /// </summary>
        /// <param name="domain">Domain name</param>
        public static string RemovePort(string domain)
        {
            // Do not remove port if current domain is in IPv6 format without port
            if (!domain.EndsWith("]", StringComparison.Ordinal))
            {
                int portIndex = domain.LastIndexOf(":", StringComparison.Ordinal);
                if (portIndex >= 0)
                {
                    domain = domain.Substring(0, portIndex);
                }
            }
            return domain;
        }


        /// <summary>
        /// Removes the port part from the whole URL.
        /// </summary>
        /// <param name="url">URL</param>
        public static string RemovePortFromURL(string url)
        {
            if ((url != null) && (url.Contains(":")))
            {
                Uri uri = new Uri(url);
                return uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.UriEscaped);
            }
            return url;
        }


        /// <summary>
        /// Returns domain or domain with port if present from specific URL.
        /// </summary>
        /// <param name="url">Input URL in format http(s)://domain</param>
        public static string GetDomain(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return string.Empty;
            }

            // Return domain and append port only if is not considered as default (http:80, https: 443)
            return uri.Host + (((uri.Port < 0) || uri.IsDefaultPort) ? String.Empty : ":" + uri.Port);
        }


        /// <summary>
        /// Returns name (e.g. localhost) without port, www. prefix and application path.
        /// </summary>
        /// <param name="domain">Domain.</param>
        public static string GetDomainName(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return domain;
            }

            domain = RemoveWWW(domain);

            // Remove application path
            int slashIndex = domain.IndexOfCSafe('/');
            if (slashIndex > 0)
            {
                domain = domain.Substring(0, slashIndex);
            }

            return RemovePort(domain);
        }


        /// <summary>
        /// Returns the application URL (absolute), without trailing slashes.
        /// </summary>
        public static string GetApplicationUrl(string domain)
        {
            string result = RequestContext.CurrentScheme + "://" + domain;

            // Add application URL if domain does not contain application path
            if (!domain.Contains("/"))
            {
                result += "/" + SystemContext.ApplicationPath.Trim('/');
            }

            return result.Trim('/');
        }


        /// <summary>
        /// Returns current application URL (absolute), without trailing slashes.
        /// </summary>
        public static string GetApplicationUrl()
        {
            if (CMSHttpContext.Current == null)
            {
                return null;
            }

            return GetApplicationUrl(RequestContext.CurrentDomain);
        }


        /// <summary>
        /// Adds parameter to specified URL.
        /// </summary>
        /// <param name="url">URL to be modified</param>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="parameterValue">Parameter value</param>
        public static string AddParameterToUrl(string url, string parameterName, string parameterValue)
        {
            if (url == null)
            {
                return null;
            }

            // Get query
            var query = string.Empty;

            // Cut off query string
            var queryIndex = url.IndexOf("?", StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                query = url.Substring(queryIndex);
                url = url.Substring(0, queryIndex);
            }

            // Add the parameter
            if (!String.IsNullOrEmpty(parameterName))
            {
                query = AddUrlParameter(query, parameterName.ToLowerInvariant(), parameterValue);
            }

            return url + query;
        }


        /// <summary>
        /// Returns current application URL (absolute) with port, without trailing slashes.
        /// </summary>
        /// <param name="domainName">Domain name, if not supplied <see cref="RequestContext.FullDomain"/> is used instead</param>
        public static string GetFullApplicationUrl(string domainName = null)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                domainName = RequestContext.FullDomain;
            }
            if (string.IsNullOrEmpty(domainName))
            {
                return null;
            }

            var url = domainName.TrimEnd('/');
            if (!domainName.Contains(SystemContext.ApplicationPath))
            {
                url = url + "/" + SystemContext.ApplicationPath.Trim('/');
            }

            if (string.IsNullOrEmpty(GetProtocol(url)))
            {
                url = RequestContext.CurrentScheme + "://" + url;
            }

            return url.Trim('/');
        }


        /// <summary>
        /// Returns the absolute URL (including port) representation for the given relative URL, uses given domain and application path.
        /// </summary>
        /// <param name="relativeUrl">Relative URL to translate</param>
        /// <param name="domainName">Domain name</param>
        public static string GetAbsoluteUrl(string relativeUrl, string domainName)
        {
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return relativeUrl;
            }

            // If no context present, or no URL given, return null
            if (CMSHttpContext.Current == null)
            {
                if (relativeUrl.StartsWith("~", StringComparison.Ordinal))
                {
                    string fullUrl = GetFullApplicationUrl(domainName);
                    if (!string.IsNullOrEmpty(fullUrl))
                    {
                        return fullUrl + relativeUrl.Substring(1);
                    }
                }
                return relativeUrl;
            }

            if (string.IsNullOrEmpty(domainName))
            {
                domainName = RequestContext.FullDomain;
            }

            return GetAbsoluteUrl(relativeUrl, domainName, GetFullApplicationUrl(domainName), RequestContext.URL.AbsolutePath);
        }


        /// <summary>
        /// Returns the absolute URL (including port) representation for the given relative URL, uses current domain and application path.
        /// </summary>
        /// <param name="relativeUrl">Relative URL to translate</param>
        public static string GetAbsoluteUrl(string relativeUrl)
        {
            return GetAbsoluteUrl(relativeUrl, null);
        }


        /// <summary>
        /// Returns the absolute URL for the given relative URL based on the given paths.
        /// </summary>
        /// <param name="relativeUrl">Relative URL to translate to absolute</param>
        /// <param name="domainUrl">Domain (server) URL, for /xxx URLs</param>
        /// <param name="applicationUrl">Application URL (including server), for ~/ URLs</param>
        /// <param name="currentPath">Current execution path, for directory relative URLs</param>
        public static string GetAbsoluteUrl(string relativeUrl, string domainUrl, string applicationUrl, string currentPath)
        {
            if (relativeUrl == null)
            {
                return null;
            }

            // Don't make absolute link from absolute URL
            if (ContainsProtocol(relativeUrl) || relativeUrl.StartsWithCSafe("#") || relativeUrl.StartsWithCSafe("?"))
            {
                return relativeUrl;
            }

            string url = relativeUrl;

            // If server relative, append to domain name
            if (relativeUrl.StartsWithCSafe("/"))
            {
                if (domainUrl != null)
                {
                    url = domainUrl.TrimEnd('/') + relativeUrl;
                }
            }
            // If application relative, append to application URL
            else if (relativeUrl.StartsWithCSafe("~/"))
            {
                if (applicationUrl == null)
                {
                    applicationUrl = domainUrl.TrimEnd('/') + SystemContext.ApplicationPath;
                }
                url = applicationUrl.TrimEnd('/') + relativeUrl.TrimStart('~');
            }
            // Else append to current request directory (if not starting with www.)
            else if (!relativeUrl.StartsWithCSafe("www.", true))
            {
                if (currentPath != null)
                {
                    // Get the current request directory
                    string result = currentPath;
                    int lastSlash = result.LastIndexOfCSafe("/");
                    if (lastSlash >= 0)
                    {
                        result = result.Substring(0, lastSlash);
                    }
                    result = result.Trim('/');
                    url = domainUrl + "/" + result + "/" + relativeUrl;
                }
                else
                {
                    throw new Exception("[URLHelper.GetAbsoluteUrl]: Current execution path could not be null.");
                }
            }

            // If URL contains protocol, return the URL
            if (ContainsProtocol(url))
            {
                return url;
            }

            // Add protocol
            if ((CMSHttpContext.Current == null) || !RequestContext.IsSSL)
            {
                // ... add http://
                url = "http://" + url;
            }
            else
            {
                // ... add https://
                url = "https://" + url;
            }

            // Return absolute URL
            return url;
        }


        /// <summary>
        /// Returns true if the URL contains the protocol part.
        /// </summary>
        /// <param name="url">URL to check</param>
        public static bool ContainsProtocol(string url)
        {
            return GetProtocol(url) != null;
        }


        /// <summary>
        /// Add parameter to query string, queryStringValue MUST start with "?".
        /// </summary>
        /// <param name="queryStringValue">Query string value</param>
        /// <param name="newKeyName">Key name</param>
        /// <param name="newKeyValue">Key value</param>
        /// <returns>return query string with new value</returns>
        public static string AddUrlParameter(string queryStringValue, string newKeyName, string newKeyValue)
        {
            string paramString = newKeyName + "=" + newKeyValue;

            if (String.IsNullOrEmpty(queryStringValue))
            {
                return "?" + paramString;
            }

            // Remove the parameter
            queryStringValue = RemoveUrlParameter(queryStringValue, newKeyName);
            if (queryStringValue == "")
            {
                return "?" + paramString;
            }

            // Add the new value
            if (!queryStringValue.StartsWith("?", StringComparison.Ordinal))
            {
                queryStringValue = "?" + queryStringValue + "&" + paramString;
            }
            else
            {
                queryStringValue += "&" + paramString;
            }

            return queryStringValue;
        }


        /// <summary>
        /// Remove parameter from query string.
        /// </summary>
        /// <param name="queryStringValue">Query string value</param>
        /// <param name="keyName">Key name</param>
        /// <returns>return query string without query string value</returns>
        public static string RemoveUrlParameter(string queryStringValue, string keyName)
        {
            if (String.IsNullOrEmpty(queryStringValue))
            {
                return "";
            }

            // If key not even found, do not remove
            if (queryStringValue.IndexOfCSafe(keyName, true) < 0)
            {
                return queryStringValue;
            }

            var paramRegEx = GetParamRegEx(keyName);

            // Remove the existing parameter
            queryStringValue = paramRegEx.Replace(queryStringValue, "");
            if (queryStringValue == "")
            {
                return queryStringValue;
            }

            // Remove the '&' at the start of query string
            queryStringValue = queryStringValue.TrimStart('&');

            // Ensure the '?'
            if (!queryStringValue.StartsWithCSafe("?"))
            {
                return "?" + queryStringValue;
            }
            else
            {
                return queryStringValue;
            }
        }


        /// <summary>
        /// Removes the given URL parameters
        /// </summary>
        /// <param name="url">URL to modify</param>
        /// <param name="names">Parameter names to remove</param>
        public static string RemoveUrlParameters(string url, params string[] names)
        {
            foreach (var name in names)
            {
                url = RemoveUrlParameter(url, name);
            }

            return url;
        }


        /// <summary>
        /// Propagates the URL parameters from current URL to the given URL
        /// </summary>
        /// <param name="url">URL to modify</param>
        /// <param name="parameters">Parameters to propagate</param>
        public static string PropagateUrlParameters(string url, params string[] parameters)
        {
            // Process all parameters
            foreach (var name in parameters)
            {
                // If the parameter is present, add to the URL
                var value = QueryHelper.GetString(name, null);
                if (value != null)
                {
                    url = AddParameterToUrl(url, name, value);
                }
            }

            return url;
        }


        /// <summary>
        /// Gets the query string value for specified key name.
        /// </summary>
        /// <param name="url">Input URL</param>
        /// <param name="keyName">Query string key name</param>
        /// <param name="keyExists">Indicates whether query key exists</param>
        public static string GetQueryValue(string url, string keyName, out bool keyExists)
        {
            var paramRegEx = GetParamRegEx(keyName);

            keyExists = false;

            var match = paramRegEx.Match(url);
            if (match.Success)
            {
                keyExists = true;
                return match.Groups[1].Value;
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets the regular expression for the query string parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        private static Regex GetParamRegEx(string name)
        {
            // Get regex from hashtable
            var paramRegEx = ParamRegEx[name];

            if (paramRegEx == null)
            {
                // Regular expression groups:                               (value       )
                paramRegEx = RegexHelper.GetRegex("(?:[?]|&)" + name + "(?:=(?<val>[^&]*))?(?=&|$)", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);
                ParamRegEx[name] = paramRegEx;
            }
            return paramRegEx;
        }


        /// <summary>
        /// Returns protocol from url.
        /// e.g. http, https, ftp, ...
        /// If no protocol found, null is returned.
        /// </summary>
        /// <param name="url">Url that may contain any protocol in valid format</param>
        public static string GetProtocol(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            var match = ProtocolRegex.Match(url);
            return match.Success ? match.Groups["protocol"].Value : null;
        }


        /// <summary>
        /// Gets the query string value for specified key name.
        /// </summary>
        /// <param name="url">Input URL</param>
        /// <param name="keyName">Query string key name</param>
        public static string GetQueryValue(string url, string keyName)
        {
            bool exists;

            return GetQueryValue(url, keyName, out exists);
        }


        /// <summary>
        /// Removes specified parameters from the given URL.
        /// </summary>
        /// <param name="url">URL to be modified</param>
        /// <param name="parameters">Parameters to remove</param>
        public static string RemoveParametersFromUrl(string url, params string[] parameters)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            // Remove all parameters
            foreach (var parameter in parameters)
            {
                url = RemoveParameterFromUrl(url, parameter);
            }

            return url;
        }


        /// <summary>
        /// Removes specified parameters from the given URL.
        /// </summary>
        /// <param name="url">URL to be modified</param>
        /// <param name="parameterName">Parameter name</param>
        public static string RemoveParameterFromUrl(string url, string parameterName)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            // Get query
            string query = "";

            int queryIndex = url.IndexOf("?", StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                query = url.Substring(queryIndex);
                url = url.Substring(0, queryIndex);
            }

            // Remove the parameter
            if (!String.IsNullOrEmpty(parameterName))
            {
                query = RemoveUrlParameter(query, parameterName);
            }

            return url + query;
        }


        /// <summary>
        /// Appends the given query string to the URL. If the URL already contains query string, merges both.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="newQuery">Query to append</param>
        public static string AppendQuery(string url, string newQuery)
        {
            if (!string.IsNullOrEmpty(newQuery))
            {
                newQuery = newQuery.TrimStart(new[] { '?', '&' });

                if (url.Contains("?"))
                {
                    url += "&" + newQuery;
                }
                else
                {
                    url += "?" + newQuery;
                }
            }

            return url;
        }


        /// <summary>
        /// Checks if page with specified relative path is excluded from system paths and returns the category of the excluded page.
        /// </summary>
        /// <param name="relativePath">Page relative path</param>
        /// <param name="checkPhysicalPath">Indicates whether physical path should be checked. Returns Physical path type</param>
        public static ExcludedSystemEnum IsExcludedSystemEnum(string relativePath, bool checkPhysicalPath = true)
        {
            if (!string.IsNullOrEmpty(relativePath))
            {
                relativePath = relativePath.ToLowerCSafe();

                // Get current extension
                string extension = Path.GetExtension(relativePath);
                bool fileSpecificUrlChecked = false;
                ExcludedSystemEnum fileSpecificExcludeType = ExcludedSystemEnum.Unknown;

                // Exclude non-aspx physical files automatically
                bool aspxPage = extension.EqualsCSafe(".aspx");
                if (!aspxPage)
                {
                    // AppThemes folder
                    if (relativePath.StartsWithCSafe("/app_themes/"))
                    {
                        return ExcludedSystemEnum.AppThemes;
                    }

                    // GetResource.ashx
                    bool ashx = extension.EqualsCSafe(".ashx");
                    if (ashx && relativePath.StartsWithCSafe("/cmspages/getresource.ashx"))
                    {
                        return ExcludedSystemEnum.GetResource;
                    }

                    // Fast evaluation for the script files
                    if (extension.EqualsCSafe(".axd") ||
                        extension.EqualsCSafe(".asmx") ||
                        extension.EqualsCSafe(".svc") ||
                        ashx
                        )
                    {
                        return ExcludedSystemEnum.Excluded;
                    }

                    // Check whether file handler path is requested.
                    // Must be called before the check for physical path due to performance impact on environments with external storage
                    fileSpecificExcludeType = GetExcludedStateForFileSpecificUrls(relativePath, ref fileSpecificUrlChecked);
                    if (fileSpecificExcludeType != ExcludedSystemEnum.Unknown)
                    {
                        return fileSpecificExcludeType;
                    }

                    if (checkPhysicalPath)
                    {
                        // Check if file exists
                        if (!String.IsNullOrEmpty(extension))
                        {
                            string physicalPath = SystemContext.WebApplicationPhysicalPath + Path.EnsureBackslashes(HttpUtility.UrlDecode(relativePath));
                            physicalPath = RemoveQuery(physicalPath);
                            if (File.Exists(physicalPath))
                            {
                                // Existing physical file
                                return ExcludedSystemEnum.PhysicalFile;
                            }
                        }
                    }
                }
                else if (relativePath.EqualsCSafe("/default.aspx") ||
                         relativePath.EqualsCSafe("/"))
                {
                    // Default page is never excluded
                    return ExcludedSystemEnum.NotExcluded;
                }

                // relative path with trailing slash must be used for administration interface
                // due to changes in IIS 7 and higher which can process directory URL without trailing slash
                string relativePathWithTrailingSlash = relativePath.TrimEnd('/') + "/";

                // Check whether file handler path is requested
                fileSpecificExcludeType = GetExcludedStateForFileSpecificUrls(relativePath, ref fileSpecificUrlChecked);
                if (fileSpecificExcludeType != ExcludedSystemEnum.Unknown)
                {
                    return fileSpecificExcludeType;
                }

                // Fast evaluation for the CMSPages files
                if (relativePath.StartsWithCSafe("/cmspages/"))
                {
                    if (relativePath.EqualsCSafe("/cmspages/getfile.aspx") ||
                        relativePath.EqualsCSafe("/cmspages/getmetafile.aspx") ||
                        relativePath.EqualsCSafe("/cmspages/getmediafile.aspx"))
                    {
                        // GetFile pages
                        return ExcludedSystemEnum.GetFileHandler;
                    }

                    if (relativePath.EqualsCSafe(PortalTemplatePageCheck))
                    {
                        // Portal template
                        return ExcludedSystemEnum.PortalTemplate;
                    }

                    if (relativePath.EqualsCSafe("/cmspages/logon.aspx"))
                    {
                        // Logon page
                        return ExcludedSystemEnum.LogonPage;
                    }

                    // Standard excluded pages
                    return ExcludedSystemEnum.Excluded;
                }

                // Special directories and files
                if (relativePath.StartsWithCSafe("/app_") ||
                    relativePath.StartsWithCSafe("/testingpages/"))
                {
                    // Standard excluded pages
                    return ExcludedSystemEnum.Excluded;
                }

                // Administration UI
                if (relativePathWithTrailingSlash.StartsWithCSafe("/admin/") ||
                    relativePathWithTrailingSlash.StartsWithCSafe("/" + AdministrationUrlHelper.GetAdministrationPath() + "/"))
                {
                    return ExcludedSystemEnum.Administration;
                }

                // CMS directories
                if (relativePath.StartsWithCSafe("/cms"))
                {
                    if (relativePath.StartsWithCSafe("/cms/getdoc/"))
                    {
                        // Authorized Document page - Not excluded
                        return ExcludedSystemEnum.NotExcluded;
                    }

                    if (relativePath.StartsWithCSafe("/cms/getattachment/"))
                    {
                        // Authorized attachment page
                        return ExcludedSystemEnum.GetFileHandler;
                    }

                    if (relativePath.StartsWithCSafe("/cms/dialogs/"))
                    {
                        // CMS dialogs - Require authentication
                        return ExcludedSystemEnum.CMSDialog;
                    }

                    if (relativePathWithTrailingSlash.StartsWithCSafe("/cmsedit/") ||
                        IsSystemPath(relativePath, true, true, true, false))
                    {
                        // Standard excluded pages
                        return ExcludedSystemEnum.Excluded;
                    }

                    if (relativePathWithTrailingSlash.StartsWithCSafe("/cms/files/"))
                    {
                        // WebDAV request
                        return ExcludedSystemEnum.WebDAV;
                    }

                    if (relativePathWithTrailingSlash.StartsWithCSafe("/cmsapi/"))
                    {
                        // Web Api requests
                        return ExcludedSystemEnum.Excluded;
                    }
                }
            }

            return ExcludedSystemEnum.NotExcluded;
        }


        /// <summary>
        /// Returns <see cref="ExcludedSystemEnum"/> for relative path related to the file handled by the GetFileHandler.
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="checkedPredicate">Indicates whether the path was already evaluated and if not process the method and cahnge the checked state.</param>
        /// <returns>
        /// Returns <see cref="ExcludedSystemEnum.Unknown"/> if path does not match; otherwise specific enum type.
        /// </returns>
        private static ExcludedSystemEnum GetExcludedStateForFileSpecificUrls(string relativePath, ref bool checkedPredicate)
        {
            if (!checkedPredicate)
            {
                checkedPredicate = true;

                // Fast evaluation for special paths for files
                if (relativePath.StartsWithCSafe("/get"))
                {
                    if (relativePath.StartsWithCSafe("/getavatar/"))
                    {
                        return ExcludedSystemEnum.Excluded;
                    }

                    if (relativePath.StartsWithCSafe("/getfile/") ||
                        relativePath.StartsWithCSafe("/getmetafile/") ||
                        relativePath.StartsWithCSafe("/getmedia/") ||
                        relativePath.StartsWithCSafe("/getimage/"))
                    {
                        return ExcludedSystemEnum.GetFileHandler;
                    }

                    if (relativePath.StartsWithCSafe("/getattachment/"))
                    {
                        // Exclude only "/getattachment/{guid}/" URL paterns
                        // URL pattern: "/getattachment/{node}/{alias}/{path}/{filename}" cannot be excluded as it needs to be processed by URLRewriter.
                        string guidPart = relativePath.Substring(15);
                        int slashAfterGuidIndex = guidPart.IndexOf('/');
                        if (slashAfterGuidIndex != -1)
                        {
                            guidPart = guidPart.Remove(slashAfterGuidIndex);
                            if (ValidationHelper.IsGuid(guidPart))
                            {
                                return ExcludedSystemEnum.GetFileHandler;
                            }
                        }
                    }
                }
            }

            return ExcludedSystemEnum.Unknown;
        }


        /// <summary>
        /// Returns true if the given path is a system path. The path is expected to be lowercase and start with slash, e.g. "/cmsmodules/..."
        /// </summary>
        /// <param name="path">path to check</param>
        /// <param name="codeFolders">Check code folders</param>
        /// <param name="virtualFolders">Check virtual folders</param>
        /// <param name="specialFolders">Special folders</param>
        /// <param name="cmsPages">Special CMS pages</param>
        public static bool IsSystemPath(string path, bool codeFolders, bool virtualFolders, bool specialFolders, bool cmsPages)
        {
            // System path must start with /cms
            if (!path.StartsWithCSafe("/cms"))
            {
                return false;
            }

            // Shorten the path to provide faster match
            path = path.Substring(4);

            // Check code folders
            if (codeFolders)
            {
                if (path.StartsWithCSafe("modules/") ||
                    path.StartsWithCSafe("templates/") ||
                    path.StartsWithCSafe("formcontrols/") ||
                    path.StartsWithCSafe("inlinecontrols/") ||
                    path.StartsWithCSafe("admincontrols/") ||
                    path.StartsWithCSafe("messages/") ||
                    path.StartsWithCSafe("webparts/") ||
                    path.StartsWithCSafe("controlsexamples/") ||
                    path.StartsWithCSafe("apiexamples/") ||
                    path.StartsWithCSafe("install/") ||
                    path.StartsWithCSafe("globalfiles/"))
                {
                    return true;
                }
            }

            // Check other than code folders
            if (virtualFolders)
            {
                if (path.StartsWithCSafe("templatelayouts/") ||
                    path.StartsWithCSafe("layouts/") ||
                    path.StartsWithCSafe("transformations/"))
                {
                    return true;
                }
            }

            // Check special folders
            if (virtualFolders)
            {
                if (path.StartsWithCSafe("help/") ||
                    path.StartsWithCSafe("scripts/") ||
                    path.StartsWithCSafe("siteutils/") ||
                    path.StartsWithCSafe("resources/"))
                {
                    return true;
                }
            }

            // Check CMS pages
            if (cmsPages)
            {
                if (path.StartsWithCSafe("pages/"))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true for url's starting with hash (#) character.
        /// </summary>
        /// <param name="url">URL to process</param>
        public static bool IsAnchor(string url)
        {
            return url.StartsWith("#", StringComparison.Ordinal);
        }


        /// <summary>
        /// Checks if page with specified relative path is excluded from system paths.
        /// </summary>
        /// <param name="relativePath">Page relative path</param>
        public static bool IsExcludedSystem(string relativePath)
        {
            return (IsExcludedSystemEnum(relativePath) != ExcludedSystemEnum.NotExcluded);
        }


        /// <summary>
        /// Checks if page with specified relative path is excluded from excluded paths.
        /// </summary>
        /// <param name="relativePath">Page relative path</param>
        /// <param name="excludedPaths">Excluded paths</param>
        public static bool IsExcluded(string relativePath, string excludedPaths)
        {
            if (IsExcludedSystem(relativePath))
            {
                return true;
            }

            return IsExcludedCustom(relativePath, excludedPaths);
        }


        /// <summary>
        /// Checks if page with specified relative path is excluded from excluded paths.
        /// </summary>
        /// <param name="relativePath">Page relative path</param>
        /// <param name="excludedPaths">Excluded paths</param>
        public static bool IsExcludedCustom(string relativePath, string excludedPaths)
        {
            if (String.IsNullOrEmpty(excludedPaths))
            {
                return false;
            }

            return IsExcluded(relativePath, excludedPaths.Split(';'));
        }


        /// <summary>
        /// Checks if page with specified relative path is excluded from excluded paths.
        /// </summary>
        /// <param name="relativePath">Page relative path</param>
        /// <param name="excludedPaths">Excluded paths</param>
        public static bool IsExcluded(string relativePath, string[] excludedPaths)
        {
            if ((excludedPaths == null) || (excludedPaths.Length <= 0) || string.IsNullOrEmpty(relativePath))
            {
                return false;
            }

            relativePath = relativePath.ToLowerCSafe();

            // Check excluded paths
            foreach (string path in excludedPaths)
            {
                if (path != "")
                {
                    if (relativePath.StartsWithCSafe(path.ToLowerCSafe()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Indicates whether <paramref name="relativePath"/> starting from application root identifies a default document within application root.
        /// The default document is identified by '/default.aspx', '/' or empty string <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Path relative to the application root.</param>
        /// <returns>Returns true if <paramref name="relativePath"/> identifies a default document, otherwise returns false.</returns>
        /// <seealso cref="SystemContext.ApplicationPath"/>
        public static bool IsRootDefaultDocumentRelativePath(string relativePath)
        {
            return String.IsNullOrEmpty(relativePath) || relativePath.Equals("/", StringComparison.OrdinalIgnoreCase) || relativePath.Equals(DEFAULT_DOCUMENT_RELATIVE_PATH, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Resolves given path to be absolute (combines with given paths).
        /// </summary>
        /// <param name="path">Relative path</param>
        /// <param name="separator">Path separator</param>
        /// <param name="webSitePath">Web site path</param>
        /// <param name="serverPath">Server path</param>
        public static string CombinePath(string path, char separator, string webSitePath, string serverPath)
        {
            // If server path not given, assign the web site path
            if (serverPath == null)
            {
                serverPath = webSitePath;
            }

            // Server based path
            if (path.StartsWithCSafe(separator.ToString()))
            {
                path = serverPath.Trim(separator) + path;
            }
            // Web site based path
            else if (path.StartsWithCSafe("~" + separator))
            {
                path = webSitePath.TrimEnd(separator) + separator + path.Substring(2);
            }
            // Other - web site based relative path
            else
            {
                path = webSitePath.TrimEnd(separator) + separator + path;
            }

            return path;
        }


        /// <summary>
        /// Returns the string with first selector removed.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="delimiter">Selector delimiter</param>
        public static string RemoveFirstPart(string expression, string delimiter)
        {
            if (expression == null)
            {
                return null;
            }

            // Get the index of delimiter
            int delimiterIndex = expression.IndexOfCSafe(delimiter);
            if (delimiterIndex < 0)
            {
                // No delimiter, delete all
                return "";
            }
            else
            {
                // Remove the part before the delimiter and the delimiter itself
                return expression.Substring(delimiterIndex + delimiter.Length);
            }
        }


        /// <summary>
        /// Removes the query from URL.
        /// </summary>
        /// <param name="url">URL to process</param>
        public static string RemoveQuery(string url)
        {
            if (url == null)
            {
                return null;
            }

            int queryIndex = url.IndexOfCSafe("?");
            if (queryIndex >= 0)
            {
                return url.Substring(0, queryIndex);
            }

            return url;
        }


        /// <summary>
        /// Gets the query from URL.
        /// </summary>
        /// <param name="url">URL to process</param>
        public static string GetQuery(string url)
        {
            if (url == null)
            {
                return null;
            }

            int queryIndex = url.IndexOfCSafe("?");
            if (queryIndex >= 0)
            {
                return url.Substring(queryIndex);
            }

            return String.Empty;
        }


        /// <summary>
        /// Converts the physical file path to virtual path within the web application.
        /// </summary>
        /// <param name="physicalPath">Physical path to convert</param>
        public static string UnMapPath(string physicalPath)
        {
            if (physicalPath.StartsWithCSafe(SystemContext.WebApplicationPhysicalPath, true))
            {
                physicalPath = "~" + Path.EnsureSlashes(physicalPath.Substring(SystemContext.WebApplicationPhysicalPath.Length));
            }

            return physicalPath;
        }


        /// <summary>
        /// Converts the absolute path to the virtual.
        /// </summary>
        /// <param name="path">Path to process (</param>
        public static string GetVirtualPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.StartsWithCSafe("~/"))
            {
                // Already virtual
                return path;
            }

            path = RemoveQuery(path);
            path = RemoveProtocolAndDomain(path);
            path = RemoveApplicationPath(path);

            return "~/" + path.TrimStart('/');
        }


        /// <summary>
        /// Un-resolves the given URL (replaces application path with ~).
        /// </summary>
        /// <param name="url">URL to un-resolve</param>
        /// <param name="applicationPath">Application path</param>
        public static string UnResolveUrl(string url, string applicationPath)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            // Don't process relative URLs
            if (url.StartsWith("/", StringComparison.Ordinal) && !url.StartsWith("//", StringComparison.Ordinal))
            {
                // Reflect root installation
                if (applicationPath.Equals("/", StringComparison.Ordinal))
                {
                    url = "~/" + url.TrimStart('/');
                }
                else if (url.StartsWith(applicationPath + "/", StringComparison.InvariantCultureIgnoreCase)
                    // Support URL in format <application_path>?<query_string> (e.g. /CMS?boo=foo)
                    || url.StartsWith(applicationPath + "?", StringComparison.InvariantCultureIgnoreCase))
                {
                    url = "~/" + url.Substring(applicationPath.Length).TrimStart('/');
                }
                else if (applicationPath.Equals(url, StringComparison.InvariantCultureIgnoreCase))
                {
                    url = "~/";
                }
            }

            return url;
        }


        /// <summary>
        /// Removes the potential extension from the given path.
        /// </summary>
        /// <param name="path">Path to process</param>
        public static string RemoveExtension(string path)
        {
            int dotIndex = path.LastIndexOfCSafe(".");
            int slashIndex = path.LastIndexOfCSafe("/");

            // If dot is after the slash (in last part of the path), remove the extension
            if (dotIndex > slashIndex)
            {
                path = path.Substring(0, dotIndex);
            }

            return path;
        }


        /// <summary>
        /// Adds HTTP protocol to URL if it's not contain HTTP or HTTPS.
        /// </summary>
        /// <param name="url">URL for which HTTP is added.</param>
        public static string AddHTTPToUrl(string url)
        {
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            return url;
        }


        /// <summary>
        /// Removes the protocol part from the URL.
        /// </summary>
        /// <param name="url">URL to process (in format protocol://...)</param>
        public static string RemoveProtocol(string url)
        {
            Match protocol;

            // Check whether URL is defined and starts with protocol
            if (!String.IsNullOrEmpty(url) && (protocol = ProtocolRegex.Match(url)).Success)
            {
                return url.Substring(protocol.Length);
            }

            return url;
        }


        /// <summary>
        /// Removes the protocol and domain part from the URL.
        /// </summary>
        /// <param name="url">URL to process (in format protocol://domain/...)</param>
        public static string RemoveProtocolAndDomain(string url)
        {
            if (url == null)
            {
                return null;
            }

            url = RemoveProtocol(url);

            // Remove domain (and port)
            int rootIndex = url.IndexOfCSafe("/");
            if (rootIndex >= 0)
            {
                url = url.Substring(rootIndex);
            }

            return url;
        }


        /// <summary>
        /// Removes the Application path from the URL.
        /// </summary>
        /// <param name="url">URL to process (in format /ApplicationPath/...)</param>
        public static string RemoveApplicationPath(string url)
        {
            if (url == null)
            {
                return null;
            }

            string appPath = SystemContext.ApplicationPath;

            if (!string.IsNullOrEmpty(appPath) && url.StartsWithCSafe(appPath, true))
            {
                url = "/" + url.Substring(appPath.Length).TrimStart('/');
            }

            return url;
        }


        /// <summary>
        /// Returns the relative path for specified URL.
        /// </summary>
        public static string GetAppRelativePath(Uri url)
        {
            // No URL given, no result
            if (url == null)
            {
                return null;
            }

            // Process the path
            string result = url.PathAndQuery;

            result = RemoveQuery(result);

            result = RemoveApplicationPath(result);

            return result;
        }


        /// <summary>
        /// Resolves an application relative URL to the absolute form.
        /// </summary>
        /// <param name="url">Virtual application root relative URL</param>
        /// <param name="ensurePrefix">If true, the current URL prefix is ensured for the URL</param>
        /// <returns>Absolute URL</returns>
        /// <exception cref="ArgumentException">Throws when <paramref name="url" /> uses a leading .. to exit above the top directory.</exception>
        public static string ResolveUrl(string url, bool ensurePrefix = false)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            // Resolve only virtual ( e.g. '/hello.aspx') or relative (e.g. '~/hello.aspx') urls
            if (!url.StartsWithAny(StringComparison.Ordinal, "~", "/"))
            {
                return url;
            }

            url = RemoveDotSegments(url);

            string appPath = SystemContext.ApplicationPath;

            // If the relative path is exactly '~', url equals appPath
            if (url.Equals("~", StringComparison.Ordinal))
            {
                url = appPath;
            }

            // If the relative path starts with '~/' replace with the appPath
            if (url.StartsWith("~/", StringComparison.Ordinal))
            {
                url = $"{appPath.TrimEnd('/')}/{url.Substring(2)}";
            }

            // Ensure the virtual URL prefix
            if (ensurePrefix)
            {
                string virtualPrefix = VirtualContext.CurrentURLPrefix;
                if (!String.IsNullOrEmpty(virtualPrefix) && !VirtualContext.ContainsVirtualContextPrefix(url))
                {
                    // Ensure custom prefix for preview link URLs
                    PathModifierHandler previewHash = null;
                    if (VirtualContext.IsPreviewLinkInitialized)
                    {
                        previewHash = VirtualContext.AddPathHash;
                    }

                    // Ensure the virtual prefix
                    url = EnsureURLPrefix(url, appPath, virtualPrefix, previewHash);
                }
            }

            return url;
        }


        /// <summary>
        /// Removes '.' and '..' segments in the <paramref name="path"/>.
        /// As side effect it fix double/triple/etc... slashes in path.
        /// </summary>
        /// <remarks>
        /// From virtual path '/Level1/../hello.aspx', '/./hello.aspx' or '//hello.aspx' it becomes in all three cases '/hello.aspx'
        /// From relative path '~/Level1/../hello.aspx', '~/./hello.aspx' or '~//hello.aspx' it becomes in all three cases '~/hello.aspx'
        /// </remarks>
        /// <param name="path">Path.</param>
        /// <returns>Processed path.</returns>
        /// <exception cref="ArgumentException">Throws when <paramref name="path" /> uses a leading .. to exit above the top directory.</exception>
        private static string RemoveDotSegments(string path)
        {
            var pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var trailingSlash = path.EndsWith("/", StringComparison.Ordinal);
            var startingSlash = path.StartsWith("/", StringComparison.Ordinal);

            // Remove all segments which contains only '.' char
            pathSegments.RemoveAll(s => s.Equals(".", StringComparison.Ordinal));

            // Process segments which contains '..' string
            var index = pathSegments.IndexOf("..");
            while (index > -1)
            {
                // Throw exception when path uses a leading .. to exit above the top directory
                if ((index == 0) || ((index == 1) && pathSegments[0].Equals("~", StringComparison.Ordinal)))
                {
                    throw new ArgumentException("Cannot use a leading .. to exit above the top directory.");
                }

                // Remove segment containing '..' string and parent segment
                pathSegments.RemoveAt(index);
                pathSegments.RemoveAt(index - 1);

                index = pathSegments.IndexOf("..");
            }

            // Reconstruct the path
            var returnPath = new StringBuilder();

            if (startingSlash)
            {
                returnPath.Append("/");
            }

            foreach (var item in pathSegments)
            {
                returnPath.Append(item)
                          .Append("/");
            }

            return (trailingSlash) ? returnPath.ToString() : returnPath.ToString().TrimEnd('/');
        }


        /// <summary>
        /// Resolves and combine client URL with base URL.
        /// </summary>
        /// <param name="baseUrl">Base URL.</param>
        /// <param name="relativeUrl">Client URL.</param>
        public static string ResolveClientUrl(string baseUrl, string relativeUrl)
        {
            // Do nothing for empty client URL
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return relativeUrl;
            }

            // Schema-agnostic client URL is considered as absolute and should not be resolved
            if (relativeUrl.StartsWith("//", StringComparison.Ordinal))
            {
                return relativeUrl;
            }

            if (relativeUrl.StartsWith("~/", StringComparison.Ordinal))
            {
                relativeUrl = ResolveUrl(relativeUrl);
            }

            // Return potentially resolved client URL if base URL is not defined
            if (string.IsNullOrEmpty(baseUrl))
            {
                return relativeUrl;
            }

            if (baseUrl.StartsWith("~/", StringComparison.Ordinal))
            {
                baseUrl = ResolveUrl(baseUrl);
            }


            // If client URL is in absolute form, no combining with base path is required
            Uri cssUri = null;
            if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out cssUri))
            {
                return cssUri.AbsoluteUri;
            }

            // Schema-agnostic base URL must be considered as absolute URL. Schema is temporarily injected and will be removed later
            bool schemaLessSourceUrl = false;
            if (baseUrl.StartsWith("//", StringComparison.Ordinal))
            {
                baseUrl = "http:" + baseUrl;
                schemaLessSourceUrl = true;
            }

            try
            {
                // Combine absolute base path with relative URL path
                Uri baseUri = null;
                if (Uri.TryCreate(baseUrl, UriKind.Absolute, out baseUri))
                {
                    Uri uri = new Uri(baseUri, relativeUrl);

                    return (schemaLessSourceUrl) ?
                        uri.AbsoluteUri.Substring(uri.Scheme.Length + 1) :
                        uri.AbsoluteUri;
                }

                // Check if relative path is pointing outside the application, if yes throw exception -> original value is returned"
                RemoveDotSegments($"{baseUrl.Substring(0, baseUrl.LastIndexOf('/'))}/{relativeUrl}");

                // Combine relative base path and relative URL part
                // UriBuilder is used for absolute path and temporal injection of http scheme (see above)
                UriBuilder builder = new UriBuilder { Path = baseUrl };
                return new Uri(builder.Uri, relativeUrl).PathAndQuery;
            }
            catch
            {
                return relativeUrl;
            }
        }


        /// <summary>
        /// Gets the extension for the given URL. Returns the extension with the dot, e.g. ".gif"
        /// </summary>
        /// <param name="url">URL to analyze</param>
        public static string GetUrlExtension(string url)
        {
            // Get the index of the last slash
            int slashIndex = url.LastIndexOfCSafe('/');
            if (slashIndex < 0)
            {
                slashIndex = 0;
            }

            // Get the index of the last dot
            int dotIndex = url.LastIndexOfCSafe('.');
            if (dotIndex >= slashIndex)
            {
                return url.Substring(dotIndex);
            }

            return "";
        }


        /// <summary>
        /// Ensures that the given URLs has the given prefix
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="prefix">URL prefix to ensure</param>
        /// <param name="urlModifier">Callback for preview hash</param>
        public static string EnsureURLPrefix(string url, string applicationPath, string prefix, PathModifierHandler urlModifier)
        {
            if ((url != null) && (prefix != null) && !url.StartsWithCSafe(prefix))
            {
                // Special URL handling in virtual context
                if (VirtualContext.IsInitialized)
                {
                    // Get relative URL without query string
                    string originalUrl = RemoveQuery(url);
                    originalUrl = RemoveApplicationPath(originalUrl);

                    var excludedEnum = IsExcludedSystemEnum(originalUrl);
                    switch (excludedEnum)
                    {
                        // Do not ensure prefix for preview link
                        case ExcludedSystemEnum.Excluded:
                        case ExcludedSystemEnum.Administration:
                            if (VirtualContext.IsPreviewLinkInitialized)
                            {
                                return url;
                            }
                            break;

                        // Never ensure prefix for physical files and app themes
                        case ExcludedSystemEnum.PhysicalFile:
                        case ExcludedSystemEnum.AppThemes:
                            return url;
                    }
                }

                // Position of prefix
                int position = -1;

                // Relative path
                if (url.StartsWithCSafe("~/"))
                {
                    position = 1;
                }
                else if (url.StartsWithCSafe(applicationPath, true))
                {
                    position = applicationPath.TrimEnd('/').Length;
                }

                // Check if prefix included
                if (!IsURLPrefixIncluded(url, prefix, position))
                {
                    // Add validation hash
                    if (urlModifier != null)
                    {
                        url = urlModifier(url);
                    }

                    // Return URL with prefix
                    return url.Insert(position, prefix);
                }
            }

            return url;
        }


        /// <summary>
        /// Checks if the prefix is included in given URL.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="prefix">URL prefix</param>
        /// <param name="position">Starting index of relative URL</param>
        private static bool IsURLPrefixIncluded(string url, string prefix, int position)
        {
            if (position >= 0)
            {
                // Check if the prefix is present
                return (url.IndexOfCSafe(prefix, true) == position);
            }

            return false;
        }


        /// <summary>
        /// Trim www if input text starts with it.
        /// </summary>
        public static string RemoveWWW(string inputText)
        {
            if (!string.IsNullOrEmpty(inputText))
            {
                if (inputText.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                {
                    return inputText.Remove(0, 4);
                }
            }

            return inputText;
        }


        /// <summary>
        /// Update parameter in specified URL.
        /// </summary>
        /// <param name="url">Raw URL</param>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="parameterValue">Parameter value</param>
        public static string UpdateParameterInUrl(string url, string parameterName, string parameterValue)
        {
            return AddParameterToUrl(url, parameterName, parameterValue);
        }


        /// <summary>
        /// Merges two query strings. Query string parameters should (but don't have to) start with question mark
        /// but they definitely must omit path part of URL.
        /// </summary>
        /// <param name="originalQuery">Query string to be merged with newQuery parameter.</param>
        /// <param name="newQuery">Query string to be merged with originalQuery parameter.</param>
        /// <param name="preferOriginal">If true, parameters from originalQuery takes first priority</param>
        public static string MergeQueryStrings(string originalQuery, string newQuery, bool preferOriginal)
        {
            // If one of query strings is empty
            if (string.IsNullOrEmpty(originalQuery) || string.IsNullOrEmpty(newQuery))
            {
                // Concatenation of both of them can be safely returned
                return originalQuery + newQuery;
            }
            // Get non-preferred query string
            string result = preferOriginal ? newQuery : originalQuery;

            // Rewrite parameters using preferred collection
            NameValueCollection preferred = HttpUtility.ParseQueryString(preferOriginal ? originalQuery : newQuery);
            foreach (string param in preferred)
            {
                result = UpdateParameterInUrl(result, param, HttpUtility.UrlEncode(preferred[param]));
            }
            return result;
        }


        /// <summary>
        /// Returns first of the friendly extensions from settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetFriendlyExtension(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSFriendlyURLExtension"].ToString("").Split(';')[0];
        }


        /// <summary>
        /// Returns string with all friendly URL extensions separated by semicolon.
        /// </summary>
        public static string GetFriendlyExtensions(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSFriendlyURLExtension"].ToString("");
        }


        /// <summary>
        /// Refreshes the current page by doing a redirect to self
        /// </summary>
        public static void RefreshCurrentPage()
        {
            Redirect(RequestContext.URL.ToString());
        }


        /// <summary>
        /// If true, permanent redirection (301) is used instead of regular (302) redirection.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UsePermanentRedirect(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSUsePermanentRedirect"].ToBoolean(false);
        }


        /// <summary>
        /// Redirects the client to another URL using the GET method (303 H).
        /// </summary>
        /// <param name="url">Destination URL</param>
        public static void SeeOther(string url)
        {
            var response = CMSHttpContext.Current.Response;
            response.StatusCode = (int)HttpStatusCode.SeeOther;
            response.RedirectLocation = ResolveUrl(url);
            response.End();
        }


        /// <summary>
        /// Redirects the response using specified URL (302 HTTP status code).
        /// </summary>
        /// <param name="url">URL for redirection</param>
        public static void Redirect(string url)
        {
            ResponseRedirect(url);
        }


        /// <summary>
        /// Redirects the response using specified URL using permanent redirection using 301.
        /// </summary>
        /// <param name="url">URL for redirection</param>
        /// <param name="siteName">Site name</param>
        public static void RedirectPermanent(string url, string siteName)
        {
            // Use response redirect if permanent redirect is not allowed
            if (!UsePermanentRedirect(siteName))
            {
                Redirect(url);
            }

            RedirectPermanentInternal(url);
        }


        /// <summary>
        /// Redirects the response using specified URL using permanent redirection using 301.
        /// </summary>
        /// <param name="url">URL for redirection</param>
        internal static void RedirectPermanentInternal(string url)
        {
            // Process URL
            url = ResolveRedirectUrl(url);

            // Log the redirect
            RequestDebug.LogRequestOperation("Redirect301", url, 1);

            var context = CMSHttpContext.CurrentStandard;
            var response = context.Response;

            // Check whether no cache should be used
            if (RequestContext.UseNoCacheForRedirect)
            {
                context.Response.Cache.SetCacheability(Base.HttpCacheability.NoCache);
            }

            response.RedirectPermanent(url, true);
        }


        /// <summary>
        /// Redirects the response using specified URL using standard redirection with Response.Redirect.
        /// </summary>
        /// <param name="url">URL for redirection</param>
        /// <param name="endResponse">True to terminate the current process</param>
        /// <remarks>If calling this method with endResponse = false and the server
        /// redirect is performed, the request can be  ended using
        /// HttpApplication.CompleteRequest() method.</remarks>
        public static void ResponseRedirect(string url, bool endResponse = true)
        {
            // Log the redirect
            RequestDebug.LogRequestOperation("Redirect302", url, 1);

#if NETFULLFRAMEWORK
            // If request is callback, set status and url of response and end response
            if (RequestHelper.IsCallback())
            {
                ClientRedirect(url);
                CMSHttpContext.Current.Response.End();
            }
            else
            {
                ServerRedirect(url, endResponse);
            }
#else
            ServerRedirect(url, endResponse);
#endif
        }


        /// <summary>
        /// Performs client redirect.
        /// </summary>
        /// <param name="url">URL to be redirected to.</param>
        public static void ClientRedirect(string url)
        {
            var response = CMSHttpContext.Current.Response;

            response.StatusCode = 302;
            response.RedirectLocation = ResolveRedirectUrl(url);
        }


        /// <summary>
        /// Performs server redirect.
        /// </summary>
        /// <param name="url">URL to be redirected to.</param>
        /// <param name="endResponse">True to terminate the current process</param>
        /// <remarks>If calling this method with endResponse = false, the request can be
        /// ended using HttpApplication.CompleteRequest() method.</remarks>
        private static void ServerRedirect(string url, bool endResponse = true)
        {
#pragma warning disable BH1008
            CMSHttpContext.Current.Response.Redirect(ResolveRedirectUrl(url), endResponse);
#pragma warning restore BH1008
        }


        /// <summary>
        /// Resolves URL against current virtual path.
        /// </summary>
        /// <param name="url">URL to be resolved.</param>
        private static string ResolveRedirectUrl(string url)
        {
            url = UnResolveUrl(url, SystemContext.ApplicationPath);

            // Keep context prefix for redirection
            return ResolveUrl(url, ensurePrefix: true);
        }


        /// <summary>
        /// Checks the URL prefix. If the prefix is present, returns true and removes the prefix from the URL if removePrefix is true.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <param name="prefix">Prefix to check</param>
        /// <param name="removePrefix">Remove the prefix if matches</param>
        public static bool CheckPrefix(ref string path, string prefix, bool removePrefix)
        {
            if (path.StartsWithCSafe(prefix, true))
            {
                if (removePrefix)
                {
                    path = path.Substring(prefix.Length);
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Checks the URL prefixes. If one of the the prefixes is present, returns true and removes the prefix from the URL if removePrefix is true.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <param name="prefixes">Prefix to check</param>
        /// <param name="removePrefix">Remove the prefix if matches</param>
        public static bool CheckPrefixes(ref string path, string[] prefixes, bool removePrefix)
        {
            // Check all prefixes
            foreach (string prefix in prefixes)
            {
                if (CheckPrefix(ref path, prefix, removePrefix))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns value of query parameter from URL.
        /// </summary>
        /// <param name="url">Input URL</param>
        /// <param name="parameterName">Name of query parameter</param>
        public static string GetUrlParameter(string url, string parameterName)
        {
            if (!String.IsNullOrEmpty(url) && !String.IsNullOrEmpty(parameterName))
            {
                Regex param = RegexHelper.GetRegex("(?:[?]|&amp;|&)" + parameterName + "=([^&]*)?(?=&|$)", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);
                Match m = param.Match(url);
                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
            }
            return null;
        }


        /// <summary>
        /// Removes trailing slash from URL.
        /// </summary>
        /// <param name="url">URL</param>
        public static string RemoveTrailingSlashFromURL(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            // Get URL query
            string query = GetQuery(url);

            // Get URL without query
            url = RemoveQuery(url);

            // Remove trailing slash and add original query string values
            return url.TrimEnd('/') + query;
        }


        /// <summary>
        /// Gets the settings for the URL trailing slash rules.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static TrailingSlashEnum UseURLsWithTrailingSlash(string siteName)
        {
            string value = CoreServices.Settings[siteName + ".CMSUseURLsWithTrailingSlash"].ToString("");
            switch (value.ToLowerCSafe())
            {
                // Always
                case "always":
                    return TrailingSlashEnum.Always;

                // Never
                case "never":
                    return TrailingSlashEnum.Never;

                // Don't care
                default:
                    return TrailingSlashEnum.DontCare;
            }
        }


        /// <summary>
        /// Gets the settings for the URL case rules.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static CaseRedirectEnum GetCaseRedirectEnum(string siteName)
        {
            string value = CoreServices.Settings[siteName + ".CMSRedirectInvalidCasePages"].ToString("");
            switch (value.ToLowerCSafe())
            {
                case "exact":
                    return CaseRedirectEnum.Exact;

                case "lowercase":
                    return CaseRedirectEnum.LowerCase;

                case "uppercase":
                    return CaseRedirectEnum.UpperCase;

                default:
                    return CaseRedirectEnum.None;
            }
        }


        /// <summary>
        /// Replaces {hash} macro by hash code for given query string parameters.
        /// </summary>
        /// <param name="url">URL</param>
        public static string EnsureHashToQueryParameters(string url)
        {
            // Check whether URL is defined and contains query string
            if (!String.IsNullOrEmpty(url) && url.Contains("?"))
            {
                string hashValues = QueryHelper.GetString("hashvalues", String.Empty);
                string hashString = String.Empty;

                if (String.IsNullOrEmpty(hashValues))
                {
                    hashString = GetQueryValue(url, "dashboardname") + "|" + GetQueryValue(url, "templatename");
                }
                else
                {
                    foreach (string value in hashValues.Split(';'))
                    {
                        hashString += GetQueryValue(value, String.Empty) + "|";
                    }

                    hashString = hashString.TrimEnd('|');
                }

                url = url.Replace("{hash}", "hash=" + ValidationHelper.GetHashString(hashString, new HashSettings("")));
            }

            return url;
        }


        /// <summary>
        /// URL encodes query string in given URL. Needs HttpContext.
        /// </summary>
        /// <param name="url">URL to update.</param>
        public static string UrlEncodeQueryString(string url)
        {
            // Get query from URL
            string query = GetQuery(url);
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Substring(1);
                string path = RemoveQuery(url);

                query = query.Replace("=", EQUALS);
                query = query.Replace("&", AMPERSAND);

                // First, decode query string in case that query string is already encoded. Then, encode query.
                query = HttpUtility.UrlEncode(HttpUtility.UrlDecode(query));

                // Decode back control characters "=" and "&"
                query = query.Replace(HttpUtility.UrlEncode(EQUALS), "=");
                query = query.Replace(HttpUtility.UrlEncode(AMPERSAND), "&");

                return path + "?" + query;
            }

            return url;
        }


        /// <summary>
        /// Ensures URL without forbidden characters (%, &amp;, #).
        /// </summary>
        /// <param name="url">URL to replace special characters</param>
        public static string EscapeSpecialCharacters(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            return url.Replace("%", "%25").Replace("&", "%26").Replace("#", "%23");
        }


        /// <summary>
        /// Redirects (302 HTTP status code) the response using specified URL in case it is local,
        /// otherwise redirects to <see cref="SystemContext.ApplicationPath"></see>
        /// </summary>
        /// <remarks>Use this method when the specified URL comes as user input (query string, POST form data, etc.)
        /// in order to prevent unvalidated redirects.</remarks>
        /// <param name="url">URL within same domain.</param>
        public static void LocalRedirect(string url)
        {
            if (IsLocalUrl(url))
            {
                Redirect(url);
            }
            else
            {
                Redirect(GetApplicationUrl());
            }
        }


        /// <summary>
        /// Returns true if the given URL is local.
        /// </summary>
        /// <param name="url">URL to check.</param>
        public static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // The url might not be encoded and might just contain a url-encoded query string.
            if (!url.Contains("/") && url.Contains("%"))
            {
                url = HttpUtility.UrlDecode(url);
            }

            return (url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || // "/" or "/foo" but not "//" or "/\"
                   (url.Length > 1 && url[0] == '~' && url[1] == '/'); // "~/" or "~/foo"
        }


        /// <summary>
        /// Sets proper port according to SSL option and custom port.
        /// </summary>
        /// <param name="isSSL">SSL option</param>
        /// <param name="uri">Uri to check</param>
        public static Uri EnsureProperPort(bool isSSL, Uri uri)
        {
            // Port from web.config is equal to request port or is not set. In both cases send request Url value.
            if ((UrlPort == uri.Port) || (SSLUrlPort == uri.Port) || ((UrlPort == 0) && (SSLUrlPort == 0)))
            {
                // Keep the original request URL
                return uri;
            }

            string result = uri.Scheme + "://" + uri.Host;

            // Do not insert default port to URL
            if ((UrlPort != DEFAULT_HTTP_PORT) || (SSLUrlPort != DEFAULT_HTTPS_PORT))
            {
                int port = (isSSL) ? SSLUrlPort : UrlPort;

                result += (port == 0) ? String.Empty : ":" + port;
            }

            return new Uri(result + uri.PathAndQuery);
        }

#endregion


#region "Make links absolute"

        /// <summary>
        /// Returns the absolute representation of the HTML links (image paths ...).
        /// </summary>
        /// <param name="text">Input HTML</param>
        /// <param name="domainName">Domain name</param>
        public string MakeLinksAbsolute(string text, string domainName)
        {
            mDomainName = domainName;
            return RegExMakeLinksAbsolute.Replace(text, SiteLinkMatch);
        }


        /// <summary>
        /// Returns the absolute representation of the HTML links (image paths ...).
        /// </summary>
        /// <param name="text">Input HTML</param>
        public static string MakeLinksAbsolute(string text)
        {
            return RegExMakeLinksAbsolute.Replace(text, LinkMatch);
        }


        /// <summary>
        /// Link match evaluation function.
        /// </summary>
        /// <param name="m">Regular expression match</param>
        private static string LinkMatch(Match m)
        {
            string link = m.Groups[2].Value + m.Groups[3].Value;
            link = MakeAbsolute(link);

            return m.Groups[1].Value + m.Groups["quote"].Value + link + m.Groups["quote"].Value;
        }


        /// <summary>
        /// Link match for site evaluation function.
        /// </summary>
        /// <param name="m">Regular expression match</param>
        private string SiteLinkMatch(Match m)
        {
            string link = m.Groups[2].Value + m.Groups[3].Value;
            link = MakeSiteAbsolute(link);

            return m.Groups[1].Value + m.Groups["quote"].Value + link + m.Groups["quote"].Value;
        }


        /// <summary>
        /// Returns the absolute URL representation for the given relative URL.
        /// </summary>
        /// <param name="relativeUrl">Relative URL to convert to absolute</param>
        private static string MakeAbsolute(string relativeUrl)
        {
            // Validate the URL and check if the protocol is present
            if (!relativeUrl.Contains("://") && ValidationHelper.IsURL(relativeUrl))
            {
                relativeUrl = GetAbsoluteUrl(relativeUrl);
            }

            return relativeUrl;
        }


        /// <summary>
        /// Returns the absolute URL representation for the given relative URL for site.
        /// </summary>
        /// <param name="relativeUrl">Relative URL to convert to absolute</param>
        private string MakeSiteAbsolute(string relativeUrl)
        {
            // Validate the URL and check if the protocol is present
            if (!relativeUrl.Contains("://") && ValidationHelper.IsURL(relativeUrl))
            {
                relativeUrl = GetAbsoluteUrl(relativeUrl, mDomainName);
            }

            return relativeUrl;
        }

#endregion
    }
}