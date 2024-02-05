using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using CMS.Modules.NuGetPackages;

using SystemIO = System.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Allows for creating a NuGet package containing all installable module files.
    /// </summary>
    public class ModulePackageBuilder
    {
        #region "Fields"

        private const string MODULES_DATA_PATH = @"App_Data\CMSModules\Modules\";
        private const string README_SOURCE_PATH = MODULES_DATA_PATH + "readme.txt";
        private const string PACKAGE_TOOLS_SOURCE_PATH = MODULES_DATA_PATH + @"tools\";

        private readonly ResourceInfo mModule;
        private readonly ModuleFileResolver mFileResolver;
        private readonly string mRootPath = SystemContext.WebApplicationPhysicalPath;
        private readonly ModuleExportPackageBuilder mExportBuilder;
        private readonly ModuleInstallationMetaDataBuilder mInstallationMetaDataBuilder;

        #endregion


        #region "Properties"

        /// <summary>
        /// The installation package contains module objects of types enumerated by this property.
        /// The module object type itself ("cms.resource") is not present in the enumeration.
        /// </summary>
        /// <returns>Enumeration of object types</returns>
        /// <seealso cref="GetModuleObjects"/>
        public IEnumerable<string> IncludedObjectTypes
        {
            get
            {
                return mExportBuilder.IncludedObjectTypes;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new ModulePackageBuilder for given ResourceInfo (module).
        /// Builder can be created only for modules which are in development (<see cref="ResourceInfo.ResourceIsInDevelopment"/> is set to true).
        /// Builder can not be created for <see cref="ModuleName.CUSTOMSYSTEM"/>.
        /// </summary>
        /// <param name="module">Resource info specifying module for which the package is created.</param>
        public ModulePackageBuilder(ResourceInfo module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }
            if (!module.ResourceIsInDevelopment)
            {
                throw new ArgumentException("Installation package can be created only when module is currently in development.", "module");
            }
            if (module.ResourceName.EqualsCSafe(ModuleName.CUSTOMSYSTEM, true))
            {
                throw new ArgumentException("Installation package can not be created for module " + ModuleName.CUSTOMSYSTEM, "module");
            }

            mModule = module;
            mFileResolver = new ModuleFileResolver(mModule, mRootPath);
            mExportBuilder = new ModuleExportPackageBuilder(mModule, CMSActionContext.CurrentUser);
            mInstallationMetaDataBuilder = new ModuleInstallationMetaDataBuilder(mModule);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Builds the module installation package to the given stream.
        /// </summary>
        /// <param name="stream">Stream to be used.</param>
        public void BuildPackage(SystemIO.Stream stream)
        {
            try
            {
                mExportBuilder.BuildExportPackage(mFileResolver.ExportPackageFolderPhysicalPath, mFileResolver.ExportPackageFileName, mFileResolver.ExportPackageTempFolderPhysicalPath);
                mInstallationMetaDataBuilder.BuildModuleDescription(mFileResolver.InstallationMetaDataTempFolderPhysicalPath, mFileResolver.InstallationMetaDataModuleDescriptionFileName);

                NuSpecBuilder nuSpec = CreateNuSpecBuilder();
                using (var nuSpecStream = new SystemIO.MemoryStream())
                {
                    nuSpec.BuildToStream(nuSpecStream);
                    nuSpecStream.Position = 0;
                    NuGetPackageBuilder packageBuilder = new NuGetPackageBuilder(nuSpecStream, mRootPath);
                    packageBuilder.BuildToStream(stream);
                }
            }
            finally
            {
                CleanupTempFiles();
            }
        }


        /// <summary>
        /// Builds the module installation package to the file specified by given path.
        /// </summary>
        /// <param name="filePath">Path to installation package.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when necessary permissions are missing for the given path.</exception>
        public void BuildPackage(string filePath)
        {
            // Ensure existence of target folder and check permissions
            DirectoryHelper.EnsureDiskPath(filePath, SystemContext.WebApplicationPhysicalPath);
            if (!DirectoryHelper.CheckPermissions(filePath))
            {
                throw new UnauthorizedAccessException(
                    String.Format("[ModulePackageBuilder.BuildPackage]: Missing write permissions necessary for target package path ('{0}').", filePath));
            }

            using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                BuildPackage(fileStream);
            }
        }


        /// <summary>
        /// Gets object query for module objects of given type which are included in the installation package.
        /// <paramref name="objectType"/> must be one of those enumerated in <see cref="IncludedObjectTypes"/>, otherwise returns null.
        /// </summary>
        /// <param name="objectType">Type of object to return object query for</param>
        /// <returns>Object query for given object type, or null.</returns>
        /// <seealso cref="IncludedObjectTypes"/>
        public ObjectQuery GetModuleObjects(string objectType)
        {
            return mExportBuilder.GetModuleObjects(objectType);
        }


        /// <summary>
        /// Gets List of relative paths to files that will be included in the installation package.
        /// </summary>
        /// <returns>List of relative paths to files that will be included in the installation package.</returns>
        public IEnumerable<string> GetModuleFiles()
        {
            List<string> result = new List<string>();
            result.AddRange(mFileResolver.GetLibraryFiles().Select(x => x.SourceRelativePath));
            result.AddRange(mFileResolver.GetContentFiles().Select(x => x.SourceRelativePath));
            result.AddRange(mFileResolver.GetMetaDataFiles().Select(x => x.SourceRelativePath));

            return result;
        } 


        /// <summary>
        /// Gets module's package metadata.
        /// </summary>
        /// <returns>Module's package metadata.</returns>
        public ModulePackageMetadata GetModuleMetadata()
        {
            return new ModulePackageMetadata()
            {
                Id = mModule.ResourceName,
                Title = mModule.ResourceDisplayName,
                Version = mModule.ResourceVersion,
                Description = String.IsNullOrWhiteSpace(mModule.ResourceDescription) ? "No description provided." : mModule.ResourceDescription,
                Authors = String.IsNullOrWhiteSpace(mModule.ResourceAuthor) ? "Unknown" : mModule.ResourceAuthor,
            };
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates and initializes NuSpecBuilder accordingly to the exported module.
        /// </summary>
        /// <returns>Initializes NuSpecBuilder instance.</returns>
        private NuSpecBuilder CreateNuSpecBuilder()
        {
            NuSpecBuilder nuSpecBuilder = new NuSpecBuilder(GetModuleMetadata())
            {
                ReadmeFilePath = README_SOURCE_PATH,
            };
            AddModuleFiles(nuSpecBuilder);
            AddMetaFiles(nuSpecBuilder);
            AddPackageTools(nuSpecBuilder);
            
            return nuSpecBuilder;
        }


        /// <summary>
        /// Adds all module files to the NuSpec manifest.
        /// </summary>
        /// <param name="nuSpecBuilder">NuSpec manifest builder that is used.</param>
        private void AddModuleFiles(NuSpecBuilder nuSpecBuilder)
        {
            foreach (var contentPath in mFileResolver.GetContentFiles())
            {
                nuSpecBuilder.AddContent(contentPath.SourceRelativePath, contentPath.TargetRelativePath);
            }

            foreach (var contentPath in mFileResolver.GetLibraryFiles())
            {
                nuSpecBuilder.AddLibrary(contentPath.SourceRelativePath, contentPath.TargetRelativePath);
            }
        }


        /// <summary>
        /// Adds module meta data files to the NuSpec manifest.
        /// </summary>
        /// <param name="nuSpecBuilder">NuSpec manifest builder that is used.</param>
        private void AddMetaFiles(NuSpecBuilder nuSpecBuilder)
        {
            foreach (var path in mFileResolver.GetMetaDataFiles())
            {
                nuSpecBuilder.AddContent(path.SourceRelativePath, path.TargetRelativePath);
            }
        }


        /// <summary>
        /// Adds package tools to the NuSpec manifest.
        /// </summary>
        /// <param name="nuSpecBuilder">NuSpec manifest builder that is used.</param>
        private void AddPackageTools(NuSpecBuilder nuSpecBuilder)
        {
            nuSpecBuilder.AddTools(PACKAGE_TOOLS_SOURCE_PATH, null);
        }


        /// <summary>
        /// Cleans up all possible temporary files after building a module package.
        /// </summary>
        private void CleanupTempFiles()
        {
            DirectoryHelper.DeleteDirectory(mFileResolver.TemporaryFilesPath, true);
        }

        #endregion
    }
}
