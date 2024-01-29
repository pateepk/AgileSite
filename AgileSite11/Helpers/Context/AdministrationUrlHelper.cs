using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

using CMS.Base;
using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// UI helper methods.
    /// </summary>
    public static class AdministrationUrlHelper
    {
        #region "Constants"

        /// <summary>
        /// General access denied page
        /// </summary>
        public const string ACCESSDENIED_PAGE = "~/CMSMessages/accessdenied.aspx";

        /// <summary>
        /// Administration UI path
        /// </summary>
        public const string DEFAULT_ADMINISTRATION_PATH = "admin";

        /// <summary>
        /// Access denied page for administration
        /// </summary>
        public const string ADMIN_ACCESSDENIED_PAGE = "~/CMSModules/Admin/accessdenied.aspx";

        /// <summary>
        /// General error page
        /// </summary>
        public const string ERROR_PAGE = "~/CMSMessages/Error.aspx";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets URL for the access denied page which displays specified localized string.
        /// </summary>
        /// <param name="resourceString">Resource string to display</param>
        /// <returns>Access Denied page URL</returns>
        public static string GetAccessDeniedUrl(string resourceString)
        {
            string url = string.Format("{0}?resstring={1}", ACCESSDENIED_PAGE, HttpUtility.UrlEncode(resourceString));

            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));
            url = AddDevelopmentDebugInfo(url);

            return url;
        }


        /// <summary>
        /// Gets URL for the access denied page which displays specified message.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <returns>Access Denied page URL</returns>
        public static string GetAccessDeniedUrlWithMessage(string message)
        {
            string url = string.Format("{0}?message={1}", ACCESSDENIED_PAGE, HttpUtility.UrlEncode(message));

            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));
            url = AddDevelopmentDebugInfo(url);

            return url;
        }


        /// <summary>
        /// Gets UI image path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetImageUrl(string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            string path = GetImagePath(URLHelper.CustomTheme, imagePath, isLiveSite, ensureDefaultTheme);

            return ResolveImageUrl(path);
        }


        /// <summary>
        /// Gets UI image path for given theme.
        /// </summary>
        /// <param name="theme">Theme</param>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetImagePath(string theme, string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            // No path is given
            if (String.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            // Given path contains protocol, return original path
            if (URLHelper.ContainsProtocol(imagePath))
            {
                return imagePath;
            }

            // Handle the complete relative path
            if (imagePath.StartsWithCSafe("~/"))
            {
                // Try to find as mapped file
                if (Path.FindExistingFile(ref imagePath, true))
                {
                    return imagePath;
                }

                return null;
            }

            // Make sure that the path doesn't start with '/'
            imagePath = imagePath.TrimStart('/');

            string defaultTheme = URLHelper.DefaultTheme;

            string defaultRootPath = String.Format("~/App_Themes/{0}/Images/{1}", defaultTheme, imagePath);
            string customRootPath = String.Format("~/App_Themes/{0}/Images/{1}", theme, imagePath);

            // Path doesn't contain file, return resolved folder
            if (!Path.HasExtension(imagePath))
            {
                // Try to get mapped custom path
                if (!ensureDefaultTheme && Path.FindExistingDirectory(ref customRootPath, true))
                {
                    // Custom root path is already mapped
                    imagePath = customRootPath;
                }
                else
                {
                    imagePath = defaultRootPath;

                    // Get mapped root path
                    Path.GetMappedPath(ref imagePath);
                }

                return imagePath;
            }

            // Trim the extension
            int dotIndex = imagePath.LastIndexOfCSafe(".");
            string extensionlessPath = imagePath.Substring(0, dotIndex);
            string extension = Path.GetExtension(imagePath);
            bool forceGif = (extension.ToLowerCSafe() == ".gif");

            // Path pattern
            string pathPattern = "~/App_Themes/{0}/Images/{1}" + extensionlessPath + "{2}";

            // Initialize themes
            string[] themes;
            if (!ensureDefaultTheme && (theme.ToLowerCSafe() != defaultTheme.ToLowerCSafe()))
            {
                themes = new[] { theme, defaultTheme };
            }
            else
            {
                themes = new[] { defaultTheme };
            }

            // Initialize directions
            string[] directions;
            if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft && !imagePath.ToLowerCSafe().StartsWithCSafe("rtl"))
            {
                directions = new[] { "RTL/", "" };
            }
            else
            {
                directions = new[] { "" };
            }

            // Initialize extensions
            string[] extensions;
            // Explicitly find GIF image
            if (forceGif)
            {
                extensions = new[] { ".gif" };
            }
            else
            {
                extensions = new[] { ".png" };
            }

            string finalPath = GetBestImagePath(pathPattern, themes, directions, extensions);

            return (finalPath ?? defaultRootPath);
        }


        /// <summary>
        /// Returns path to the icon image for the specified document type.
        /// </summary>
        /// <param name="theme">Current theme</param>
        /// <param name="className">Name of the class</param>
        /// <param name="iconSet">Name of the subfolder where icon images are located</param>
        /// <param name="checkFile">Indicates if the required icon exists in the file system</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public static string GetDocumentTypeIconPath(string theme, string className, string iconSet = null, bool checkFile = true, bool ensureDefaultTheme = true)
        {
            var fileName = GetFileName(className);

            // Get images directory and extension
            var imageDirectory = "DocumentTypeIcons/" + iconSet;

            // Get PNG image full path
            var imagePath = GetImagePath(theme, imageDirectory.TrimEnd('/') + "/" + fileName + ".png", false, ensureDefaultTheme);

            // File is not physically present, try to use GIF image
            if (checkFile && !File.Exists(URLHelper.GetPhysicalPath(imagePath)))
            {
                // Get GIF image full path
                imagePath = GetImagePath(theme, imageDirectory.TrimEnd('/') + "/" + fileName + ".gif", false, ensureDefaultTheme);
            }

            return imagePath;
        }


        /// <summary>
        /// Resolves the path to an image
        /// </summary>
        /// <param name="path">Image path</param>
        public static string ResolveImageUrl(string path)
        {
            path = StorageHelper.GetImageUrl(path);

            return URLHelper.ResolveUrl(path);
        }


        /// <summary>
        /// Gets custom administration path, or empty string if not set.
        /// </summary>
        /// <returns>Custom administration path.</returns>
        public static string GetCustomAdministrationPath()
        {
            return ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAdministrationPath"], String.Empty);
        }


        /// <summary>
        /// Gets administration path - custom, or default (if custom not set).
        /// </summary>
        /// <returns>Administration path.</returns>
        public static string GetAdministrationPath()
        {
            string path = GetCustomAdministrationPath();
            return (String.IsNullOrEmpty(path)) ? DEFAULT_ADMINISTRATION_PATH : path;
        }


        /// <summary>
        /// Get administration Url (starts with ~/) - custom, or default (if custom not set)
        /// </summary>
        public static string GetAdministrationUrl()
        {
            return String.Format("~/{0}", GetAdministrationPath());
        }


        /// <summary>
        /// Gets URL for the info page which displays specified message.
        /// </summary>
        /// <param name="resourceString">Resource string to display</param>
        public static string GetInformationUrl(string resourceString)
        {
            string url = "~/CMSMessages/Information.aspx?resstring=" + HttpUtility.UrlEncode(resourceString);

            // Add hash to URL in order to prevent modifying the resource string
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));

            url = AddDevelopmentDebugInfo(url);

            return url;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets best image path.
        /// </summary>
        /// <param name="filePattern">Image path pattern</param>
        /// <param name="themes">Array of themes</param>
        /// <param name="directions">Array of language directions</param>
        /// <param name="extensions">Array of file extensions</param>
        private static string GetBestImagePath(string filePattern, IEnumerable<string> themes, IEnumerable<string> directions, IEnumerable<string> extensions)
        {
            // Try all themes
            foreach (string theme in themes)
            {
                // Try all directions
                foreach (string direction in directions)
                {
                    // Try all extensions
                    foreach (string extension in extensions)
                    {
                        // Check if the given file exists
                        string filePath = String.Format(filePattern, theme, direction, extension);

                        if (Path.FindExistingFile(ref filePath, true))
                        {
                            return filePath;
                        }
                    }
                }
            }

            return null;
        }


        private static string GetFileName(string className)
        {
            if (String.IsNullOrEmpty(className))
            {
                return "default";
            }

            return className.Replace(".", "_");
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Gets the URL to Access denied page
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        /// <param name="uiElementName">UI element name that failed</param>
        /// <param name="message">Custom message to display</param>
        /// <param name="pageUrl">Custom page URL to be used</param>
        public static string GetAccessDeniedUrl(string resourceName, string permissionName, string uiElementName, string message = null, string pageUrl = ACCESSDENIED_PAGE)
        {
            string url = pageUrl;

            // Resource name given
            if (!String.IsNullOrEmpty(resourceName))
            {
                url = URLHelper.AddParameterToUrl(url, "resource", HttpUtility.UrlEncode(resourceName));
                if (!String.IsNullOrEmpty(permissionName))
                {
                    url = URLHelper.AddParameterToUrl(url, "permission", HttpUtility.UrlEncode(permissionName));
                }
                else if (!String.IsNullOrEmpty(uiElementName))
                {
                    url = URLHelper.AddParameterToUrl(url, "uielement", HttpUtility.UrlEncode(uiElementName));
                }
            }

            // Custom message
            if (!String.IsNullOrEmpty(message))
            {
                url = URLHelper.AddParameterToUrl(url, "message", HttpUtility.UrlEncode(message));
            }

            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));
            url = AddDevelopmentDebugInfo(url);

            return url;
        }


        /// <summary>
        /// Adds debug info if development mode is enabled.
        /// </summary>
        /// <param name="url">URL where will be added the debug info.</param>
        /// <returns>URL with debug info if development mode is enabled, otherwise original <paramref name="url"/>.</returns>
        private static string AddDevelopmentDebugInfo(string url)
        {
            if (SystemContext.DevelopmentMode)
            {
                url = URLHelper.AddParameterToUrl(url, "requestguid", DebugContext.CurrentRequestLogs.RequestGUID.ToString());
            }

            return url;
        }


        /// <summary>
        /// Gets URL for the error page which displays specified message.
        /// </summary>
        /// <param name="titleResourceString">Resource string used as a title</param>
        /// <param name="textResourceString">Resource string used as a text</param>
        /// <param name="isDialog">Set to true if the error page should be displayed in the dialog</param>
        /// <param name="pageUrl">Custom page URL to be used</param>
        /// <returns>URL for the error page</returns>
        public static string GetErrorPageUrl(string titleResourceString, string textResourceString, bool isDialog = false, string pageUrl = ERROR_PAGE)
        {
            string url = pageUrl;

            // Resource name given
            if (!String.IsNullOrEmpty(titleResourceString))
            {
                url = URLHelper.AddParameterToUrl(url, "title", HttpUtility.UrlEncode(titleResourceString));
            }

            if (!String.IsNullOrEmpty(textResourceString))
            {
                url = URLHelper.AddParameterToUrl(url, "text", HttpUtility.UrlEncode(textResourceString));
            }

            // Add hash to URL in order to prevent modifying the resource string
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));

            if (isDialog)
            {
                // If it is a dialog, display Cancel button
                url = URLHelper.AddParameterToUrl(url, "cancel", "1");
            }

            return url;
        }

        #endregion
    }
}
