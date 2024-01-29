using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;

[assembly: RegisterModule(typeof(ModulesModule))]


namespace CMS.Modules
{
    /// <summary>
    /// Represents the Modules module.
    /// </summary>
    public class ModulesModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ModulesModule()
            : base(new ModulesModuleMetadata())
        {
        }
        

        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Import/Export handlers
            ModuleImport.Init();
            ModuleExport.Init();

            ImportSpecialActions.Init();
            ExportSpecialActions.Init();

            // Virtual modules must be registered after PreInit state and before Initialized state
            // This dependency is required by upgrade procedure (all modules must be loaded)
            VirtualModuleManager.RegisterVirtualModules();

            ApplicationEvents.Initialized.Execute += ProcessInstallableModuleChanges;
        }


        /// <summary>
        /// Processes changes in installable modules.
        /// </summary>
        private void ProcessInstallableModuleChanges(object sender, EventArgs e)
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                // Do not perform any changes unless running as a regular web
                var moduleInstaller = ModuleInstaller.Current;
                moduleInstaller.ProcessInstallation();

                // Restart application if any processed module needs it
                if (moduleInstaller.RestartRequired)
                {
                    if (!SystemHelper.RestartApplication(SystemContext.WebApplicationPhysicalPath))
                    {
                        CoreServices.EventLog.LogEvent("E", "ModuleInstaller", "RESTART", "The module installer required application restart, but automatic restart failed. Installed modules might not work until application is restarted. You should restart the application manually.");
                    }
                    else
                    {
                        CoreServices.EventLog.LogEvent("I", "ModuleInstaller", "RESTART", "Application will now restart (due to module installation).");
                    }
                }
            }
        }


        /// <summary>
        /// <para>
        /// Cleans out uninstallation tokens for all modules, including associated data (i.e. module uninstallation files).
        /// This allows for module re-installation when the database has been lost, but the modules are still present
        /// in code base (e.g. after dropping the database and installing a new one using the wizard).
        /// </para>
        /// <para>
        /// This method must be called after application pre-initialization phase has finished (to reflect changes in custom path mapping).
        /// When called after the instance has already been initialized (and module installation processed), the module re-installation will occur upon next startup.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Reset of module uninstallation tokens allows for re-installation of modules which had already been processed by this instance,
        /// but have not been uninstalled (i.e. have not been removed from the code base). It is designed for situations when you want need
        /// to install all modules again (e.g. after database has been recreated to its initial state).
        /// If you need to allow for re-installation of some particular module, you should simply uninstall it (i.e. remove from code base) and then install again.
        /// </para>
        /// <para>
        /// If you reset uninstallation tokens when modules are installed (i.e. they are present in the database and code base), you prevent them from being uninstalled by this instance.
        /// However, the instance will recover the missing tokens (and associated data) for installed modules upon its next startup.
        /// </para>
        /// </remarks>
        public static void ResetUninstallationTokensOfInstallableModules()
        {
            ModuleInstaller.Current.ResetUninstallationTokens();
        }
    }
}