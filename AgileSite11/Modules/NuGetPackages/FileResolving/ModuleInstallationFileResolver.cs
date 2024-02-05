using System;
using System.Linq;
using System.Text;

using CMS.IO;


namespace CMS.Modules
{
    /// <summary>
    /// Provides paths to all module files and folders which are needed during package installation.
    /// </summary>
    internal class ModuleInstallationFileResolver : ModuleFileResolverBase
    {
        #region "Constants"

        /// <summary>
        /// Relative path to directory containing all meta files of installed modules (with trailing slash).
        /// The directory serves as a repository. Once the module is removed via NuGet, the meta files have to be still available.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// Substitution '{1}' is replaced by the module version.
        /// </remarks>
        private const string INSTALLED_NUGET_MODULES_REPOSITORY = InstallableModulesManager.INSTALLED_NUGET_MODULES_META_FILES_PATH + InstallableModulesManager.MODULE_DESCRIPTION_PATTERN + @"\";


        /// <summary>
        /// Relative path to directory containing files of installed modules which are necessary for proper uninstallation (with trailing slash).
        /// The directory serves as a repository. Once the module is removed via NuGet, the uninstallation files have to be still available.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// Substitution '{1}' is replaced by the module version.
        /// </remarks>
        private const string INSTALLED_NUGET_MODULES_UNINSTALLATION_FILES_REPOSITORY = INSTALLED_NUGET_MODULES_REPOSITORY + @"Uninstall\";

        #endregion


        #region "Fields"

        private readonly string mModuleName;
        private readonly string mModuleVersion;
        private readonly string mRootPath;
        private string mRootedInstallationPath;
        private string mRootedUpdatePath;
        private string mRootedUninstallationPath;
        private string mRootedUninstallationRepositoryPath;
        private string mRootedRepositoryPath;

        #endregion


        #region "Properties"

        /// <summary>
        /// Path to module import/export package.
        /// </summary>
        public string InstallationExportPackagePath
        {
            get
            {
                return Path.Combine(RootedInstallationPath, SubstitutePath(EXPORT_PACKAGE_FILE_NAME_PATTERN, mModuleName, mModuleVersion));
            }
        }


        /// <summary>
        /// Path to SQL script to be executed before module package import.
        /// </summary>
        public string InstallationBeforeSqlPath
        {
            get
            {
                return Path.Combine(RootedInstallationPath, SQL_BEFORE_FILE_NAME);
            }
        }


        /// <summary>
        /// Path to SQL script to be executed after module package import.
        /// </summary>
        public string InstallationAfterSqlPath
        {
            get
            {
                return Path.Combine(RootedInstallationPath, SQL_AFTER_FILE_NAME);
            }
        }


        /// <summary>
        /// Path to SQL script to be executed before module DB data uninstallation.
        /// </summary>
        /// <remarks>
        /// Path points to NuGet package, but the script is backed-up in local repository
        /// since after module is removed via NuGet, the file would be inaccessible.
        /// </remarks>
        public string UninstallationBeforeSqlPath
        {
            get
            {
                return Path.Combine(RootedUninstallationPath, SQL_BEFORE_FILE_NAME);
            }
        }


        /// <summary>
        /// Path to SQL script to be executed after module DB data uninstallation.
        /// </summary>
        /// <remarks>
        /// Path points to NuGet package, but the script is backed-up in local repository
        /// since after module is removed via NuGet, the file would be inaccessible.
        /// </remarks>
        public string UninstallationAfterSqlPath
        {
            get
            {
                return Path.Combine(RootedUninstallationPath, SQL_AFTER_FILE_NAME);
            }
        }


        /// <summary>
        /// Path to SQL script to be executed before module DB data update.
        /// </summary>
        public string UpdateBeforeSqlPath
        {
            get
            {
                return Path.Combine(RootedUpdatePath, SQL_BEFORE_FILE_NAME);
            }
        }


        /// <summary>
        /// Path to SQL script to be executed after module DB data update.
        /// </summary>
        public string UpdateAfterSqlPath
        {
            get
            {
                return Path.Combine(RootedUpdatePath, SQL_AFTER_FILE_NAME);
            }
        }


        /// <summary>
        /// Path to SQL script to be executed before module DB data uninstallation.
        /// </summary>
        /// <remarks>
        /// Path points to repository which is available even after module is removed via NuGet.
        /// </remarks>
        public string UninstallationBeforeSqlRepositoryPath
        {
            get
            {
                return Path.Combine(RootedUninstallationRepositoryPath, SQL_BEFORE_FILE_NAME);
            }
        }


        /// <summary>
        /// Path to SQL script to be executed after module DB data uninstallation.
        /// </summary>
        /// <remarks>
        /// Path points to repository which is available even after module is removed via NuGet.
        /// </remarks>
        public string UninstallationAfterSqlRepositoryPath
        {
            get
            {
                return Path.Combine(RootedUninstallationRepositoryPath, SQL_AFTER_FILE_NAME);
            }
        }


        /// <summary>
        /// Rooted path to module meta data repository folder.
        /// </summary>
        /// <remarks>
        /// Path points to repository which is available even after module is removed via NuGet.
        /// </remarks>
        public string RepositoryPath
        {
            get
            {
                return mRootedRepositoryPath ?? (mRootedRepositoryPath = Path.Combine(mRootPath, SubstitutePath(INSTALLED_NUGET_MODULES_REPOSITORY, mModuleName, mModuleVersion)));
            }
        }

        /// <summary>
        /// Rooted path to module installation folder with performed substitutions.
        /// </summary>
        private string RootedInstallationPath
        {
            get
            {
                return mRootedInstallationPath ?? (mRootedInstallationPath = Path.Combine(mRootPath, SubstitutePath(INSTALLATION_DIRECTORY_PATH_PATTERN, mModuleName, mModuleVersion)));
            }
        }


        /// <summary>
        /// Rooted path to module update folder with performed substitutions.
        /// </summary>
        private string RootedUpdatePath
        {
            get
            {
                return mRootedUpdatePath ?? (mRootedUpdatePath = Path.Combine(mRootPath, SubstitutePath(UPDATE_DIRECTORY_PATH_PATTERN, mModuleName, mModuleVersion)));
            }
        }


        /// <summary>
        /// Rooted path to module uninstallation folder with performed substitutions.
        /// </summary>
        /// <remarks>
        /// Path points to NuGet package, but the path content is backed-up in local repository
        /// since after module is removed via NuGet, the file would be inaccessible.
        /// </remarks>
        /// <seealso cref="RootedUninstallationRepositoryPath"/>
        private string RootedUninstallationPath
        {
            get
            {
                return mRootedUninstallationPath ?? (mRootedUninstallationPath = Path.Combine(mRootPath, SubstitutePath(UNINSTALLATION_DIRECTORY_PATH_PATTERN, mModuleName, mModuleVersion)));
            }
        }


        /// <summary>
        /// Rooted path to module uninstallation repository folder.
        /// </summary>
        /// <remarks>
        /// Path points to repository which is available even after module is removed via NuGet.
        /// </remarks>
        /// <seealso cref="RootedUninstallationPath"/>
        private string RootedUninstallationRepositoryPath
        {
            get
            {
                return mRootedUninstallationRepositoryPath ?? (mRootedUninstallationRepositoryPath = Path.Combine(mRootPath, SubstitutePath(INSTALLED_NUGET_MODULES_UNINSTALLATION_FILES_REPOSITORY, mModuleName, mModuleVersion)));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new file path resolver for installation/update/uninstallation of modules.
        /// </summary>
        /// <param name="rootPath">Root physical path. All paths are relative to this path.</param>
        /// <param name="moduleName">Name of module for which paths are created.</param>
        /// <param name="moduleVersion">Version of module for which paths are created.</param>
        public ModuleInstallationFileResolver(string rootPath, string moduleName, string moduleVersion)
        {
            mRootPath = rootPath;
            mModuleName = moduleName;
            mModuleVersion = moduleVersion;
        }

        #endregion
    }
}
