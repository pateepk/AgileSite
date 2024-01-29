using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.IO;


namespace CMS.Modules
{
    /// <summary>
    /// Class responsible for import of export package shipped with installable module.
    /// </summary>
    internal class ModuleExportPackageImporter
    {
        #region "Fields"

        private readonly IUserInfo mUserInfo;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Imports package residing in <paramref name="exportPackagePath"/>.
        /// </summary>
        /// <param name="exportPackagePath">Path to export package.</param>
        public void Import(string exportPackagePath)
        {
            if (!File.Exists(exportPackagePath))
            {
                throw new ArgumentException("[ModuleExportPackageImporter.Import] File on given path '" + exportPackagePath + "' does not exist.", "exportPackagePath");
            }

            var settings = CreateImportSettings(exportPackagePath);

            ImportManager importManager = new ImportManager(settings);
            importManager.Import(null);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates new settings object for import and sets its basic properties.
        /// </summary>
        /// <param name="sourceFilePath">Path to the zip for importing.</param>
        /// <returns>Import settings.</returns>
        private SiteImportSettings CreateImportSettings(string sourceFilePath)
        {
            SiteImportSettings result = new SiteImportSettings(mUserInfo);
            result.SourceFilePath = sourceFilePath;
            result.WebsitePath = SystemContext.WebApplicationPhysicalPath;

            result.ImportType = ImportTypeEnum.AllForced;
            result.CopyFiles = false;
            result.CopyCodeFiles = false;
            result.DefaultProcessObjectType = ProcessObjectEnum.All;
            result.LogSynchronization = false;
            result.RefreshMacroSecurity = true;

            return result;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new instance of module package importer.
        /// </summary>
        /// <param name="userInfo">User info to be used in the import process.</param>
        public ModuleExportPackageImporter(IUserInfo userInfo)
        {
            mUserInfo = userInfo;
        }

        #endregion
    }
}
