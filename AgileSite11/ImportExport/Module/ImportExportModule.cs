using CMS;
using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(ImportExportModule))]

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Represents the Import/Export module.
    /// </summary>
    public class ImportExportModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ImportExportModule()
            : base(new ImportExportModuleMetadata())
        {
        }


        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ImportExportSynchronization.Init();
            SpecialActions.Init();
            ImportExportUIInitializer.InitDefaultSettingsControls();
        }
    }
}