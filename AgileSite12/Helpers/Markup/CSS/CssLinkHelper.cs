using System;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Utility methods for CSS url manipulation.
    /// </summary>
    public class CssLinkHelper
    {
        /// <summary>
        /// CSS Link Prefix
        /// </summary>
        public const string CSS_LINK_PREFIX = "LinkCSS_";


        /// <summary>
        /// If true, the current request links are minified.
        /// </summary>
        public static bool MinifyCurrentRequest
        {
            get
            {
                return WebLanguagesContext.MinifyCurrentRequest;
            }
            set
            {
                WebLanguagesContext.MinifyCurrentRequest = value;
            }
        }


        /// <summary>
        /// Gets if stylesheet minification is enabled.
        /// </summary>        
        public static bool StylesheetMinificationEnabled
        {
            get
            {
                return CoreServices.Settings["CMSStylesheetMinificationEnabled"].ToBoolean(false);
            }
        }


        /// <summary>
        /// Gets the URL for the Theme CSS file. Based in ~/App_Themes/[Theme] or ~/App_Themes/Default.
        /// </summary>
        /// <param name="theme">Stylesheet theme</param>
        /// <param name="fileName">Name of the stylesheet file</param>
        /// <param name="useOnlySpecifiedTheme">If true, then return the url only for the specified theme, if the file not found return null</param>
        public static string GetThemeCssUrl(string theme, string fileName, bool useOnlySpecifiedTheme)
        {
            // Need to specify name of stylesheet
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            // Return CSS file for a specific theme if exists
            if (!string.IsNullOrEmpty(theme))
            {
                string themeUrl = string.Format("~/App_Themes/{0}/{1}", theme, fileName);
                if (FileHelper.FileExists(themeUrl))
                {
                    return themeUrl;
                }
            }

            if (!useOnlySpecifiedTheme)
            {
                // If a specific theme does not exist, use a custom theme
                string customThemeUrl = string.Format("~/App_Themes/{0}/{1}", URLHelper.CustomTheme, fileName);
                if (FileHelper.FileExists(customThemeUrl))
                {
                    return customThemeUrl;
                }

                // If a specific theme does not exist, use a default theme
                string defaultThemeUrl = string.Format("~/App_Themes/Default/{0}", fileName);
                if (FileHelper.FileExists(defaultThemeUrl))
                {
                    return defaultThemeUrl;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the URL for the Theme CSS file. Based in ~/App_Themes/[Theme] or ~/App_Themes/Default.
        /// </summary>
        /// <param name="theme">Stylesheet theme</param>
        /// <param name="fileName">Name of the stylesheet file</param>
        public static string GetThemeCssUrl(string theme, string fileName)
        {
            return GetThemeCssUrl(theme, fileName, false);
        }


        /// <summary>
        /// Gets the URL used to retrieve external physical stylesheet.
        /// </summary>
        /// <param name="url">URL to the stylesheet file</param>
        /// <returns>URL to stylesheet file</returns>
        public static string GetPhysicalCssUrl(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return null;
            }


            // Exclude GetCSS and GetResource handlers from URL transformation
            if (StylesheetMinificationEnabled &&
                MinifyCurrentRequest &&
                !url.Contains("GetResource.ashx") &&
                !url.Contains("GetCSS.aspx") &&
                FileHelper.FileExists(url))
            {
                return GetCssUrl("?stylesheetfile=" + URLHelper.ResolveUrl(url));
            }
            else
            {
                return url;
            }
        }


        /// <summary>
        /// Gets the URL for the Theme CSS file. Based in ~/App_Themes/[Theme] or ~/App_Themes/Default.
        /// </summary>
        /// <param name="theme">Stylesheet theme</param>
        /// <param name="fileName">Name of the stylesheet file</param>
        public static string GetPhysicalCssUrl(string theme, string fileName)
        {
            string url = GetThemeCssUrl(theme, fileName);
            return GetPhysicalCssUrl(url);
        }


        /// <summary>
        /// Returns path to the stylesheet handler
        /// </summary>
        /// <param name="parameters">Querystring arguments for the request, including the leading question mark</param>        
        /// <returns>URL to the stylesheet, using the querystring arguments</returns>
        public static string GetCssUrl(string parameters)
        {
            // If no parameters specified, return null
            if (string.IsNullOrEmpty(parameters) || (parameters == "?"))
            {
                return null;
            }

            string handlerUrl = StylesheetMinificationEnabled ? "~/CMSPages/GetResource.ashx" : "~/CMSPages/GetCSS.aspx";
            return handlerUrl + parameters;
        }


        /// <summary>
        /// Returns virtual path to the stylesheet file.
        /// </summary>
        /// <param name="stylesheetName">Stylesheet name</param>
        public static string GetStylesheetUrl(string stylesheetName)
        {
            return GetCssUrl("?stylesheetname=" + stylesheetName);
        }


        /// <summary>
        /// Returns the HTML link element that links to external stylesheet.
        /// </summary>
        /// <param name="url">URL pointing to an external stylesheet</param>
        /// <param name="media">What device the linked document will be displayed on, or null</param>
        /// <returns>HTML link to a stylesheet</returns>
        public static string GetCssFileLink(string url, string media)
        {
            return HTMLHelper.GetLink(URLHelper.ResolveUrl(url), "text/css", "stylesheet", media, null);
        }


        /// <summary>
        /// Sets the CSS link as already registered.
        /// </summary>
        /// <param name="url">URL of the CSS</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static void SetCssLinkRegistered(string url)
        {
            string key = CSS_LINK_PREFIX + URLHelper.ResolveUrl(url);

            WebLanguagesContext.CurrentCSSLinks[key] = true;
        }


        /// <summary>
        /// Returns the HTML link element that links to external stylesheet.
        /// </summary>
        /// <param name="url">URL pointing to an external stylesheet</param>
        /// <returns>HTML link to a stylesheet</returns>
        public static string GetCssFileLink(string url)
        {
            return HTMLHelper.GetLink(URLHelper.ResolveUrl(url), "text/css", "stylesheet", null, null);
        }


        /// <summary>
        /// Gets if the CSS link is already registered.
        /// </summary>
        /// <param name="url">URL of the CSS</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static bool IsCssLinkRegistered(string url)
        {
            string key = CSS_LINK_PREFIX + URLHelper.ResolveUrl(url);

            return (WebLanguagesContext.CurrentCSSLinks[key] != null);
        }
    }
}