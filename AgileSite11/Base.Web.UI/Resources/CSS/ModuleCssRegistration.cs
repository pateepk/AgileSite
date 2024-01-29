using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.IO;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Provides methods for module CSS stylesheet registration.
    /// </summary>
    public class ModuleCssRegistration
    {
        /// <summary>
        /// Registers links to all stylesheets of the specified module with the <see cref="System.Web.UI.Page"/> object.
        /// </summary>
        /// <param name="page">The page to register stylesheets with.</param>
        /// <param name="moduleName">The code name of the module.</param>
        public static void RegisterModuleStylesheets(Page page, string moduleName)
        {
            if (String.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentException("Module name is not specified.", "moduleName");
            }

            if (page == null)
            {
                return;
            }

            string relativeFolderPath;
            if (TryGetModuleStylesheetRelativeFolderPath(moduleName, out relativeFolderPath))
            {
                var folderPath = Path.Combine(SystemContext.WebApplicationPhysicalPath, relativeFolderPath);
                foreach (var stylesheetFilePath in Directory.GetFiles(folderPath, "*.css"))
                {
                    var stylesheetFileName = Path.GetFileName(stylesheetFilePath);
                    var stylesheetRelativeUrl = String.Format("~/{0}/{1}", relativeFolderPath, stylesheetFileName);

                    CssRegistration.AddExternalCssToPageHeader(page, stylesheetRelativeUrl);
                }
            }
        }


        /// <summary>
        /// Registers link to a stylesheet of the specified module with the <see cref="System.Web.UI.Page"/> object.
        /// </summary>
        /// <param name="page">The page to register the stylesheet with.</param>
        /// <param name="moduleName">The code name of the module.</param>
        /// <param name="stylesheetFileName">The stylesheet file name.</param>
        public static void RegisterModuleStylesheet(Page page, string moduleName, string stylesheetFileName)
        {
            if (String.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentException("Module name is not specified.", "moduleName");
            }

            if (String.IsNullOrEmpty(stylesheetFileName))
            {
                throw new ArgumentException("Stylesheet file name is not specified.", "stylesheetFileName");
            }

            if (page == null)
            {
                return;
            }

            string relativeFolderPath;
            if (TryGetModuleStylesheetRelativeFolderPath(moduleName, out relativeFolderPath))
            {
                var stylesheetRelativeUrl = String.Format("~/{0}/{1}", relativeFolderPath, stylesheetFileName);
                CssRegistration.AddExternalCssToPageHeader(page, stylesheetRelativeUrl);
            }
        }


        /// <summary>
        /// Gets a relative path to the folder with stylesheets of the specified module.
        /// </summary>
        /// <param name="moduleName">The code name of the module.</param>
        /// <param name="relativeFolderPath">A relative path to the folder with stylesheets, if found; otherwise, the value is not specified.</param>
        /// <returns>True, if the folder with stylesheets exists; otherwise, false.</returns>
        private static bool TryGetModuleStylesheetRelativeFolderPath(string moduleName, out string relativeFolderPath)
        {
            relativeFolderPath = null;
            const string relativeFolderPathFormat = "CMSModules/{0}/Content";

            foreach (var folderName in GetModuleFolderNames(moduleName))
            {
                relativeFolderPath = String.Format(relativeFolderPathFormat, folderName);
                var folderPath = Path.Combine(SystemContext.WebApplicationPhysicalPath, relativeFolderPath);

                if (Directory.Exists(folderPath))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns an enumerable collection of folder names for the specified module.
        /// </summary>
        /// <param name="moduleName">The code name of the module.</param>
        /// <returns>An enumerable collection of folder names for the specified module.</returns>
        /// <remarks>
        /// A folder with module files must have the same name as the module code name.
        /// However, there is one exception from this naming convention.
        /// Kentico modules do not require the CMS prefix in folder names, it is optional.
        /// </remarks>
        private static IEnumerable<string> GetModuleFolderNames(string moduleName)
        {
            const string kenticoNamespacePrefix = "CMS.";

            if (moduleName.StartsWith(kenticoNamespacePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new string[]
                {
                    moduleName,
                    moduleName.Remove(0, kenticoNamespacePrefix.Length)
                };
            }

            return new string[]
            {
                moduleName
            };
        }
    }
}