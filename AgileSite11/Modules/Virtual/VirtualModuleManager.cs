using System;
using CMS.IO;

using CMS.Base;
using CMS.Core;

namespace CMS.Modules
{

    /// <summary>
    /// Provides a registration of virtual modules.
    /// </summary>
    /// <remarks>
    /// Virtual modules are represented only by a folder in the CMSModules folder, there is no corresponding class derived from the Module class.
    /// </remarks>
    public class VirtualModuleManager
    {

        /// <summary>
        /// Registers virtual modules.
        /// </summary>
        /// <remarks>
        /// Virtual modules are registered only if the application is a web application.
        /// </remarks>
        public static void RegisterVirtualModules()
        {
            // Do not register virtual modules if the application is not a web application.
            if (!SystemContext.IsWebSite)
            {
                return;
            }

            string moduleRootFolderPath = Path.Combine(SystemContext.WebApplicationPhysicalPath, "CMSModules");
            if (Directory.Exists(moduleRootFolderPath))
            {
                foreach (var moduleFolderPath in Directory.GetDirectories(moduleRootFolderPath))
                {
                    string moduleFolderName = Path.GetFileName(moduleFolderPath);
                    string moduleName = moduleFolderName;

                    // Module CMS.Form resides in the BizForms folder.
                    if (moduleFolderName.Equals("BizForms", StringComparison.InvariantCultureIgnoreCase))
                    {
                        moduleName = ModuleName.BIZFORM;
                    }
                    // Module CMS.License resides in the Licenses folder and his name is both Licenses and CMS.License.
                    else if (moduleFolderName.Equals("Licenses", StringComparison.InvariantCultureIgnoreCase))
                    {
                        moduleName = "Licenses";
                    }
                    // If a name of the module vendor is missing, the module belongs to Kentico (vendor name CMS).
                    // Module CMS is an exception, there is no vendor prefix.
                    else if (moduleName.IndexOf('.') < 0 && !moduleName.Equals("CMS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        moduleName = String.Format("CMS.{0}", moduleName);
                    }

                    if (!ModuleEntryManager.IsModuleLoaded(moduleName))
                    {
                        string moduleRootPath = String.Format("~/CMSModules/{0}", moduleFolderName);
                        RegisterVirtualModule(moduleName, moduleRootPath);
                    }
                }
                // Module CMS must be registered, it represents the Kentico platform.
                if (!ModuleEntryManager.IsModuleLoaded("CMS"))
                {
                    RegisterVirtualModule("CMS", "~/CMSModules/CMS");
                }
            }
        }


        /// <summary>
        /// Registers a virtual module with the specified properties.
        /// </summary>
        /// <param name="name">A module name.</param>
        /// <param name="rootPath">A relative path to the module folder (for example "~/CMSModules/Forums").</param>
        private static void RegisterVirtualModule(string name, string rootPath)
        {
            var module = new ModuleInfo(name, rootPath);
            ModuleEntryManager.RegisterModule(module);
        }

    }

}