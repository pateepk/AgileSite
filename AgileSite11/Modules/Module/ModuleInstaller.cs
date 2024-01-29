using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using CMS.Modules.Internal;


namespace CMS.Modules
{
    /// <summary>
    /// Class responsible for modules installation from the high level perspective.
    /// </summary>
    internal class ModuleInstaller
    {
        #region "Fields"
        
        private static readonly ModuleInstaller mCurrent = new ModuleInstaller();
        private static readonly object syncRoot = new object();

        // Indicates if some module from current installation run needs a restart
        private bool mRestartRequired;

        // Indicates if instance is after restart.
        private bool mAfterRestart = true;
       
        #endregion


        #region "Properties"
        
        /// <summary>
        /// Gets current instance.
        /// </summary>
        public static ModuleInstaller Current
        {
            get
            {
                return mCurrent;
            }
        }


        /// <summary>
        /// Tells you whether module installer has processed any module requiring application restart.
        /// Once set to true the flag is not cleared until application is restarted.
        /// </summary>
        public bool RestartRequired
        {
            get
            {
                return mRestartRequired;
            }
        }


        /// <summary>
        /// Path to application root.
        /// </summary>
        private string RootPath
        {
            get
            {
                return SystemContext.WebApplicationPhysicalPath;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Processes installation changes in modules.
        /// </summary>
        /// <returns>Returns true if the installation changes finished successfully, false otherwise.</returns>
        /// <remarks>
        /// The installation changes stop on first failing module.
        /// </remarks>
        public bool ProcessInstallation()
        {
            // Installation in parallel is currently not supported
            lock (syncRoot)
            {
                var installableModulesManager = InstallableModulesManager.Current;

                try
                {
                    if (mAfterRestart)
                    {
                        // When called for the first time, update state of modules which needed a restart
                        mAfterRestart = false;
                        installableModulesManager.RestartPerformed();
                    }

                    var installableModulesState = installableModulesManager.GetCurrentState();

                    IUserInfo cachedInstallationUser = null;
                    Func<IUserInfo> installationUser = () => cachedInstallationUser ?? (cachedInstallationUser = ModuleInstallerConfiguration.GetInstallationUser());

                    // Retry finish actions for installed modules
                    var modulesWithoutToken = installableModulesManager.GetInstalledModulesWithoutUninstallationToken(installableModulesState);
                    foreach (var module in modulesWithoutToken)
                    {
                        FinishModuleInstallation(module, new ModuleInstallationFileResolver(RootPath, module.Name, module.Version));
                    }

                    // Install new modules
                    var modulesForInstallation = installableModulesManager.GetModulesToBeInstalled(installableModulesState);
                    foreach (var module in modulesForInstallation)
                    {
                        InstallModule(module, installationUser());
                    }

                    // Update existing modules
                    var modulesForUpdate = installableModulesManager.GetModulesToBeUpdated(installableModulesState);
                    foreach (var modulePair in modulesForUpdate)
                    {
                        UpdateModule(modulePair.Item1, modulePair.Item2, installationUser());
                    }

                    // Retry finish actions for uninstalled modules
                    var modulesWithRedundantToken = installableModulesManager.GetUninstalledModulesWithUninstallationToken(installableModulesState);
                    foreach (var module in modulesWithRedundantToken)
                    {
                        FinishModuleUninstallation(module, new ModuleInstallationFileResolver(RootPath, module.Name, module.Version));
                    }

                    // Uninstall modules
                    var modulesForUninstallation = installableModulesManager.GetModulesToBeUninstalled(installableModulesState);
                    foreach (var module in modulesForUninstallation)
                    {
                        UninstallModule(module);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    CoreServices.EventLog.LogException("ModuleInstaller", "FAILURE", ex);

                    return false;
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
        public void ResetUninstallationTokens()
        {
            var uninstallationTokens = InstallableModulesManager.Current.GetUninstallationTokens();
            foreach (var uninstallationToken in uninstallationTokens)
            {
                FinishModuleUninstallationInternal(uninstallationToken, new ModuleInstallationFileResolver(RootPath, uninstallationToken.Name, uninstallationToken.Version));
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Installs a new module to the system.
        /// </summary>
        /// <param name="basicModuleInstallationMetaData">Basic module installation meta data.</param>
        /// <param name="userInfo">User info to be used for module installation.</param>
        /// <exception cref="ModuleInstallationException">Thrown when installation fails.</exception>
        internal void InstallModule(BasicModuleInstallationMetaData basicModuleInstallationMetaData, IUserInfo userInfo)
        {
            try
            {
                bool restartNeeded; 
                var fileResolver = new ModuleInstallationFileResolver(RootPath, basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version);

                using (var transaction = new CMSTransactionScope())
                {
                    // Execute before.sql
                    ExecuteSqlScript(fileResolver.InstallationBeforeSqlPath);

                    // Import .zip package with export data
                    ModuleExportPackageImporter importer = new ModuleExportPackageImporter(userInfo);
                    importer.Import(fileResolver.InstallationExportPackagePath);

                    // Execute after.sql
                    ExecuteSqlScript(fileResolver.InstallationAfterSqlPath);

                    restartNeeded = IsRestartNeeded(basicModuleInstallationMetaData);

                    // Update installed modules meta data
                    InstallableModulesManager.Current.MarkModuleAsInstalled(basicModuleInstallationMetaData, restartNeeded);

                    // Commit the module installation if the transaction succeeded
                    transaction.Commit();
                }

                // Perform finish actions
                FinishModuleInstallation(basicModuleInstallationMetaData, fileResolver);

                SetRestartFlag(restartNeeded);

                CoreServices.EventLog.LogEvent("I", "ModuleInstaller", "MODULEINSTALLED", String.Format("Module '{0}' in version {1} has been installed (restart is {2}required).",
                    basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version, restartNeeded ? String.Empty : "not "));
            }
            catch (Exception ex)
            {
                throw new ModuleInstallationException(String.Format("Installation of module '{0}' in version {1} failed.",
                        basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version), basicModuleInstallationMetaData.Name, "INSTALL", basicModuleInstallationMetaData.Version, ex);
            }
        }


        /// <summary>
        /// Updates a module installed in the system.
        /// </summary>
        /// <param name="installedModuleVersion">Basic module installation meta data of currently installed module version.</param>
        /// <param name="updatedModuleVersion">Basic module installation meta data of newer module version.</param>
        /// <param name="userInfo">User info to be used for module update.</param>
        /// <exception cref="ModuleInstallationException">Thrown when module update fails.</exception>
        internal void UpdateModule(BasicModuleInstallationMetaData installedModuleVersion, BasicModuleInstallationMetaData updatedModuleVersion, IUserInfo userInfo)
        {
            try
            {
                bool restartNeeded;
                var updatedFileResolver = new ModuleInstallationFileResolver(RootPath, updatedModuleVersion.Name, updatedModuleVersion.Version);
                var installedFileResolver = new ModuleInstallationFileResolver(RootPath, installedModuleVersion.Name, installedModuleVersion.Version);

                using (var transaction = new CMSTransactionScope())
                {
                    var queryParameters = new QueryDataParameters();
                    queryParameters.Add("@FromVersion", installedModuleVersion.Version);

                    // Execute before.sql
                    ExecuteSqlScript(updatedFileResolver.UpdateBeforeSqlPath, queryParameters);

                    // Import .zip package with export data
                    ModuleExportPackageImporter importer = new ModuleExportPackageImporter(userInfo);
                    importer.Import(updatedFileResolver.InstallationExportPackagePath);

                    // Execute after.sql
                    ExecuteSqlScript(updatedFileResolver.UpdateAfterSqlPath, queryParameters);

                    restartNeeded = IsRestartNeeded(updatedModuleVersion);

                    // Update installed modules meta data
                    InstallableModulesManager.Current.MarkModuleAsInstalled(updatedModuleVersion, restartNeeded);

                    // Commit the module installation if the transaction succeeded
                    transaction.Commit();
                }

                // Perform finish actions
                FinishModuleUninstallation(installedModuleVersion, installedFileResolver);
                FinishModuleInstallation(updatedModuleVersion, updatedFileResolver);

                SetRestartFlag(restartNeeded);

                CoreServices.EventLog.LogEvent("I", "ModuleInstaller", "MODULEUPDATED", String.Format("Module '{0}' has been updated from version {1} to version {2} (restart is {3}required).",
                    updatedModuleVersion.Name, installedModuleVersion.Version, updatedModuleVersion.Version, restartNeeded ? String.Empty : "not "));
            }
            catch (Exception ex)
            {
                throw new ModuleInstallationException(String.Format("Update of module '{0}' from version {1} to version {2} failed.",
                        updatedModuleVersion.Name, installedModuleVersion.Version, updatedModuleVersion.Version), updatedModuleVersion.Name, "UPDATE", updatedModuleVersion.Version, ex);
            }
        }


        /// <summary>
        /// Uninstall a module from the system.
        /// </summary>
        /// <param name="basicModuleInstallationMetaData">Basic module installation meta data.</param>
        /// <exception cref="ModuleInstallationException">Thrown when uninstallation fails.</exception>
        internal void UninstallModule(BasicModuleInstallationMetaData basicModuleInstallationMetaData)
        {
            try
            {
                var fileResolver = new ModuleInstallationFileResolver(RootPath, basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version);

                // Uninstall module from database
                using (var transaction = new CMSTransactionScope())
                {
                    // Execute before.sql
                    ExecuteSqlScript(fileResolver.UninstallationBeforeSqlRepositoryPath);

                    // Remove all module data from database
                    RemoveInstalledModuleData(basicModuleInstallationMetaData.Name);

                    // Execute after.sql
                    ExecuteSqlScript(fileResolver.UninstallationAfterSqlRepositoryPath);

                    // Commit the module uninstallation if the transaction succeeded
                    transaction.Commit();
                }

                // Perform finish actions
                FinishModuleUninstallation(basicModuleInstallationMetaData, fileResolver);

                CoreServices.EventLog.LogEvent("I", "ModuleInstaller", "MODULEUNINSTALLED", String.Format("Module '{0}' in version {1} has been uninstalled.",
                    basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version));
            }
            catch (Exception ex)
            {
                throw new ModuleInstallationException(String.Format("Uninstallation of module '{0}' in version {1} failed.",
                        basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version), basicModuleInstallationMetaData.Name, "UNINSTALL", basicModuleInstallationMetaData.Version, ex);
            }
        }


        /// <summary>
        /// Performs finish actions after module installation. Writes to event log in case of failure.
        /// Failure to perform the actions has no impact on module functionality. However, such module
        /// can not be uninstalled until finish actions are successfully performed.
        /// </summary>
        /// <param name="basicModuleInstallationMetaData">Basic module installation meta data.</param>
        /// <param name="fileResolver">Resolver for module paths.</param>
        private void FinishModuleInstallation(BasicModuleInstallationMetaData basicModuleInstallationMetaData, ModuleInstallationFileResolver fileResolver)
        {
            try
            {
                // Copy files to repository
                CopyUninstallationFiles(fileResolver);

                // Token goes last
                InstallableModulesManager.Current.EnsureModuleUninstallationToken(basicModuleInstallationMetaData);
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogEvent("W", "ModuleInstaller", "FINISHINSTALLATION", String.Format("Finish actions for module '{0}' in version {1} have failed. This should have no impact on module functionality. However, the module can not be uninstalled until the issue is resolved. The instance will retry to complete the finish actions for this module installation upon next startup. {2}",
                    basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version, ex));
            }
        }


        /// <summary>
        /// Performs finish actions after module uninstallation. Writes to event log in case of failure.
        /// Failure to perform the actions has not direct impact on module uninstallation. However, such module
        /// can not be installed again until finish actions are successfully performed.
        /// </summary>
        /// <param name="basicModuleInstallationMetaData">Basic module installation meta data.</param>
        /// <param name="fileResolver">Resolver for module paths.</param>
        private void FinishModuleUninstallation(BasicModuleInstallationMetaData basicModuleInstallationMetaData, ModuleInstallationFileResolver fileResolver)
        {
            try
            {
                FinishModuleUninstallationInternal(basicModuleInstallationMetaData, fileResolver);
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogEvent("W", "ModuleInstaller", "FINISHUNINSTALLATION", String.Format("Finish actions for module '{0}' in version {1} have failed. This should have no direct impact on module uninstallation. However, the module can not be installed again until the issue is resolved. The instance will retry to complete the finish actions for this module uninstallation upon next startup. {2}",
                    basicModuleInstallationMetaData.Name, basicModuleInstallationMetaData.Version, ex));
            }
        }


        /// <summary>
        /// Performs finish actions after module uninstallation. Does not handle exceptions.
        /// Failure to perform the actions has not direct impact on module uninstallation. However, such module
        /// can not be installed again until finish actions are successfully performed.
        /// </summary>
        /// <param name="basicModuleInstallationMetaData">Basic module installation meta data.</param>
        /// <param name="fileResolver">Resolver for module paths.</param>
        private void FinishModuleUninstallationInternal(BasicModuleInstallationMetaData basicModuleInstallationMetaData, ModuleInstallationFileResolver fileResolver)
        {
            // Token goes first, if the repository removal fails, the uninstallation data may be incomplete and uninstallation must be prohibited
            InstallableModulesManager.Current.RemoveModuleUninstallationToken(basicModuleInstallationMetaData);

            // Remove repository
            RemoveInstalledModuleRepository(fileResolver);
        }


        /// <summary>
        /// Copies module files, which are necessary for uninstallation, to repository.
        /// </summary>
        /// <param name="fileResolver">Resolver for module paths.</param>
        private void CopyUninstallationFiles(ModuleInstallationFileResolver fileResolver)
        {
            CopyUninstallationFile(fileResolver.UninstallationBeforeSqlPath, fileResolver.UninstallationBeforeSqlRepositoryPath);

            CopyUninstallationFile(fileResolver.UninstallationAfterSqlPath, fileResolver.UninstallationAfterSqlRepositoryPath);
        }


        /// <summary>
        /// Copies uninstallation file if it exists. Ensures the <paramref name="targetFilePath"/> existence before copying.
        /// Overwrites existing file.
        /// </summary>
        /// <param name="sourceFilePath">Source path.</param>
        /// <param name="targetFilePath">Target path.</param>
        private void CopyUninstallationFile(string sourceFilePath, string targetFilePath)
        {
            if (File.Exists(sourceFilePath))
            {
                DirectoryHelper.EnsureDiskPath(targetFilePath, RootPath);

                File.Copy(sourceFilePath, targetFilePath, true);
            }
        }


        /// <summary>
        /// Sets restart flag if <paramref name="restartNeeded"/> is true.
        /// Otherwise does nothing.
        /// </summary>
        /// <param name="restartNeeded">True if restart is needed, false otherwise.</param>
        /// <remarks>
        /// Modules having a DLL are omitted from the initialization and therefore
        /// are not fully ready until application is restarted.
        /// </remarks>
        private void SetRestartFlag(bool restartNeeded)
        {
            // Once set, it is not cleared until application restarts
            mRestartRequired |= restartNeeded;
        }


        /// <summary>
        /// Tells you whether module needs restart after making installation changes.
        /// </summary>
        /// <param name="basicModuleInstallationMetaData">Basic module installation meta data.</param>
        /// <returns>True if module needs restart, false otherwise.</returns>
        private bool IsRestartNeeded(BasicModuleInstallationMetaData basicModuleInstallationMetaData)
        {
            // Restart the application if the module has an assembly (exists in the file system and is not virtual)
            var moduleInfo = ModuleEntryManager.GetModuleInfo(basicModuleInstallationMetaData.Name);

            return (moduleInfo != null) && (moduleInfo.Module != null);
        }


        /// <summary>
        /// Executes SQL script from a file.
        /// </summary>
        /// <param name="sqlScriptFilePath">SQL script file path.</param>
        /// <param name="parameters">Query parameters.</param>
        private void ExecuteSqlScript(string sqlScriptFilePath, QueryDataParameters parameters = null)
        {
            if (!File.Exists(sqlScriptFilePath))
            {
                return;
            }

            // Read the script with default encoding
            string sqlQuery = File.ReadAllText(sqlScriptFilePath);
            ConnectionHelper.ExecuteNonQuery(sqlQuery, parameters, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Removes all installed module meta data.
        /// </summary>
        /// <param name="fileResolver">File resolver used for resolving module meta file paths.</param>
        private void RemoveInstalledModuleRepository(ModuleInstallationFileResolver fileResolver)
        {
            string repository = fileResolver.RepositoryPath;
            if (Directory.Exists(repository))
            {
                Directory.Delete(repository, true);
            }
        }


        /// <summary>
        /// Removes all module's data from the database.
        /// </summary>
        /// <param name="moduleName">Module code name.</param>
        internal void RemoveInstalledModuleData(string moduleName)
        {
            ResourceInfo resource = ResourceInfoProvider.GetResourceInfo(moduleName);
            if (resource == null)
            {
                return;
            }

            // Delete all resource classes (class is not deleted with resource)
            var dataClasses = DataClassInfoProvider.GetClasses().WhereEquals("ClassResourceID", resource.ResourceID);
            foreach (var dataClass in dataClasses)
            {
                DataClassInfoProvider.DeleteDataClassInfo(dataClass);
            }

            var moduleDataProvider = new ModuleDataProvider(resource);
            foreach (var objectType in moduleDataProvider.SupportedObjectTypes)
            {
                var objects = moduleDataProvider.GetModuleObjects(objectType);
                foreach (var obj in objects)
                {
                    obj.Delete();
                }
            }

            resource.Delete();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Prepares the only instance of module installer.
        /// </summary>
        private ModuleInstaller()
        {
            
        }

        #endregion
    }
}
