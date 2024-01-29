using CMS.Core;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Represents the Import/Export module metadata.
    /// </summary>
    public class ImportExportModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ImportExportModuleMetadata()
            : base(ModuleName.IMPORTEXPORT)
        {
            RootPath = "~/CMSModules/ImportExport/";
        }
    }
}