using System;
using System.Collections.Generic;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Provides access to information about CSS Stylesheet.
    /// </summary>
    public class CssStylesheetInfoProvider : AbstractInfoProvider<CssStylesheetInfo, CssStylesheetInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// CSS stylesheet directory path.
        /// </summary>
        private const string CSS_STYLESHEETS_DIRECTORY = "~/CMSVirtualFiles/CSSStylesheets";


        /// <summary>
        /// The static collection of CssPreprocessor objects.
        /// </summary>
        private static SafeDictionary<string, CssPreprocessor> preprocessorCollection;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether CSS style sheets should be stored externally
        /// </summary>
        public static bool StoreCSSStyleSheetsInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStoreCSSStylesheetsInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStoreCSSStylesheetsInFS", value);
            }
        }


        /// <summary>
        /// Gets the CSS stylesheet directory path.
        /// </summary>
        public static string CSSStylesheetsDirectory
        {
            get
            {
                return CSS_STYLESHEETS_DIRECTORY;
            }
        }


        /// <summary>
        /// Represents the collection of CSS preprocessors.
        /// </summary>
        public static SafeDictionary<string, CssPreprocessor> CssPreprocessors
        {
            get
            {
                if (preprocessorCollection == null)
                {
                    preprocessorCollection = new SafeDictionary<string, CssPreprocessor>();
                }

                return preprocessorCollection;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CssStylesheetInfoProvider()
            : base(CssStylesheetInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true
            })
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns CSS style sheet info for specified path
        /// </summary>
        /// <param name="path">Path</param>
        public static CssStylesheetInfo GetVirtualObject(string path)
        {
            List<string> prefixes = new List<string>();
            // Get layout code name and web part code name
            string cssName = VirtualPathHelper.GetVirtualObjectName(path, CSSStylesheetsDirectory, ref prefixes);
            return GetCssStylesheetInfo(cssName);
        }


        /// <summary>
        /// Returns all CSS stylesheets.
        /// </summary>
        public static ObjectQuery<CssStylesheetInfo> GetCssStylesheets()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets or updates specified CssStylesheetInfo.
        /// </summary>
        /// <param name="stylesheet">Css stylesheet info object</param>
        public static void SetCssStylesheetInfo(CssStylesheetInfo stylesheet)
        {
            ProviderObject.SetInfo(stylesheet);
        }


        /// <summary>
        /// Returns CssStylesheetInfo object for specified CssStylesheetID.
        /// </summary>
        /// <param name="cssStylesheetId">Stylesheet ID</param>
        public static CssStylesheetInfo GetCssStylesheetInfo(int cssStylesheetId)
        {
            return ProviderObject.GetInfoById(cssStylesheetId);
        }


        /// <summary>
        /// Returns CssStylesheetInfo object for specified CssStylesheetName.
        /// </summary>
        /// <param name="cssStylesheetName">Stylesheet name</param>
        public static CssStylesheetInfo GetCssStylesheetInfo(string cssStylesheetName)
        {
            return ProviderObject.GetInfoByCodeName(cssStylesheetName);
        }


        /// <summary>
        /// Deletes specified CssStyleSheet.
        /// </summary>
        /// <param name="cssObject">CssStylesheet object to delete</param>
        public static void DeleteCssStylesheetInfo(CssStylesheetInfo cssObject)
        {
            ProviderObject.DeleteInfo(cssObject);

            // Clear sites hashtable
            ProviderHelper.ClearHashtables(SiteInfo.OBJECT_TYPE, true);
        }


        /// <summary>
        /// Deletes specified CssStyleSheet.
        /// </summary>
        /// <param name="cssStyleSheetId">CssStylesheetId to delete</param>
        public static void DeleteCssStylesheetInfo(int cssStyleSheetId)
        {
            CssStylesheetInfo infoObj = GetCssStylesheetInfo(cssStyleSheetId);
            DeleteCssStylesheetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified CssStyleSheet.
        /// </summary>
        /// <param name="cssStyleSheetName">CssStylesheetName to delete</param>
        public static void DeleteCssStylesheetInfo(string cssStyleSheetName)
        {
            // Get the stylesheet
            CssStylesheetInfo csi = GetCssStylesheetInfo(cssStyleSheetName);
            DeleteCssStylesheetInfo(csi);
        }


        /// <summary>
        /// Returns full CSS stylesheet name from the given virtual CSS stylesheet path.
        /// </summary>
        /// <param name="url">Virtual CSS stylesheet path</param>
        public static string GetCSSStylesheetName(string url)
        {
            if (HttpContext.Current != null)
            {
                string physicalPath = HttpContext.Current.Request.MapPath(url);
                string physicalPathVirtDir = HttpContext.Current.Request.MapPath(CSSStylesheetsDirectory);

                // gets the path behind CSS stylesheet directory
                string newPath = physicalPath.Remove(0, physicalPathVirtDir.Length + 1);
                newPath = newPath.Replace("\\", ".");
                newPath = URLHelper.RemoveFirstPart(newPath, ".");

                // gets file name from the specified path without extension
                if (newPath.EndsWithCSafe(".css"))
                {
                    newPath = newPath.Substring(0, newPath.Length - 5);
                }

                return newPath;
            }
            return "";
        }


        /// <summary>
        /// Use this method to register custom CSS dynamic language and its parser.
        /// </summary>
        /// <param name="name">Dynamic language name</param>
        /// <param name="extension">File extension associated with the dynamic language</param>
        /// <param name="displayName">Dynamic language name used for displaying in the user interface</param>
        /// <param name="callback">Callback function which the source CSS code is passed to process</param>
        /// <param name="registerClientCompilationScripts">Method that ensures registration of client scripts required for client-side compilation</param>
        /// <param name="getErrorDescription">Method used to get formatted error message</param>
        public static void RegisterCssPreprocessor(string name, string extension, string displayName, Func<CssEventArgs, string> callback, Action registerClientCompilationScripts = null, Func<string, string> getErrorDescription = null)
        {
            // Check whether preprocessor with the same name is already registered. If so keep the firstly registered one.
            CssPreprocessor prep = GetCssPreprocessor(name);
            if ((prep == null) && !String.IsNullOrEmpty(name))
            {
                prep = new CssPreprocessor
                {
                    Name = name,
                    Extension = extension,
                    DisplayName = displayName,
                    Callback = callback,
                    RegisterClientCompilationScripts = registerClientCompilationScripts,
                    GetErrorDescription = getErrorDescription
                };

                CssPreprocessors.Add(name.ToLowerCSafe(), prep);
            }
        }


        /// <summary>
        /// The method processes the CSS code provided as the first argument with CSS preprocessor associated with dynamic language provided as the second argument.
        /// </summary>
        /// <param name="source">Source CSS code</param>
        /// <param name="lang">Dynamic language name</param>
        public static string RunCssPreprocessor(string source, string lang)
        {
            string output = "";

            if (!String.IsNullOrEmpty(lang))
            {
                // Get preprocessor by the given dynamic language if registered
                CssPreprocessor p = GetCssPreprocessor(lang);
                if (p != null)
                {
                    CssEventArgs cssArgs = new CssEventArgs();
                    cssArgs.Code = source;

                    // Run CSS preprocessor via the registered callback and pass CssEventArgs object as an argument. After execution that object contains error and warning message if any occurred.
                    if (p.Callback != null)
                    {
                        output = p.Callback(cssArgs);
                    }

                    // Assume that no error occurred if error message returned by CSS preprocessor is empty.
                    if (!String.IsNullOrEmpty(cssArgs.ErrorMessage))
                    {
                        throw new Exception(cssArgs.ErrorMessage);
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// Internally calls preprocessor parser and returns error message. 
        /// </summary>
        /// <param name="code">Input code.</param>
        /// <param name="parserName">Preprocessor name.</param>
        /// <param name="output">Resulting CSS code.</param>
        public static string TryParseCss(string code, string parserName, out string output)
        {
            string error = String.Empty;
            output = String.Empty;

            // Running CSS preprocessor may produce an exception
            try
            {
                output = RunCssPreprocessor(code, parserName);
            }
            catch (Exception e)
            {
                error = e.Message;
                if (StoreCSSStyleSheetsInExternalStorage)
                {
                    output = error;
                }
            }

            // Assume that parsing succeeded if error message is empty
            return error;
        }


        /// <summary>
        /// Returns CssPreprocessor object by the preprocessor language provided as the first argument if such exists.
        /// </summary>
        /// <param name="lang">Preprocessor language</param>
        public static CssPreprocessor GetCssPreprocessor(string lang)
        {
            CssPreprocessor p = null;
            if (!String.IsNullOrEmpty(lang))
            {
                CssPreprocessors.TryGetValue(lang.ToLowerCSafe(), out p);
            }
            return p;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(CssStylesheetInfo info)
        {
            if (info != null)
            {
                // Change version GUID for VPP only if the code changed
                bool codeChanged = info.ItemChanged(CssStylesheetInfo.EXTERNAL_COLUMN_CSS);
                if (codeChanged || String.IsNullOrEmpty(info.StylesheetVersionGUID))
                {
                    info.StylesheetVersionGUID = Guid.NewGuid().ToString();
                }

                // Ensure that stylesheet dynamic language is registered; if not then convert the stylesheet to plain CSS
                if (!info.StylesheetDynamicLanguage.EqualsCSafe(CssStylesheetInfo.PLAIN_CSS, true) && (GetCssPreprocessor(info.StylesheetDynamicLanguage) == null))
                {
                    info.StylesheetDynamicLanguage = CssStylesheetInfo.PLAIN_CSS;
                }

                if (!info.IsPlainCss())
                {
                    // Get data straightly from the DataClass if external storage used in order to obtain correct data for special external column
                    var code = StoreCSSStyleSheetsInExternalStorage ? info.StylesheetDynamicCodeInternal : info.StylesheetDynamicCode;

                    if (!String.IsNullOrEmpty(code))
                    {
                        // Try get already processed value
                        if (!String.IsNullOrEmpty(CssPreprocessor.CurrentCompiledValue))
                        {
                            info.StylesheetText = CssPreprocessor.CurrentCompiledValue;
                        }
                        // Run CSS preprocessor 
                        else if (info.ItemChanged("StylesheetDynamicCode"))
                        {
                            info.StylesheetText = RunCssPreprocessor(code, info.StylesheetDynamicLanguage);
                        }
                    }
                }
                else
                {
                    info.StylesheetDynamicCode = null;
                }

                // Set CSS info
                base.SetInfo(info);
            }
        }

        #endregion
    }
}