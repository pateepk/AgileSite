using CMS;
using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.IO;
using CMS.Synchronization;

[assembly: RegisterModule(typeof(FormModule))]

namespace CMS.FormEngine
{
    /// <summary>
    /// Represents the Form module.
    /// </summary>
    public class FormModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormModule()
            : base(new FormModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Pre-initialize module.
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            ObjectFactory<DataDefinition>.SetObjectTypeTo<FormInfo>(true);
        }


        /// <summary>
        /// Initialize module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Alternative form virtual object
            VirtualPathHelper.RegisterVirtualPath(AlternativeFormInfoProvider.FormLayoutsDirectory, AlternativeFormVirtualObject.GetVirtualFileObject);

            FormHandlers.Init();

            // Exclude those system hidden keys from synchronization and Import/Export
            SynchronizationHelper.AddExcludedSettingKey("CMSStoreAltFormLayoutsInFS", "CMSStoreFormLayoutsInFS");

            ImportExportHelper.AddExcludedSettingKey("CMSStoreAltFormLayoutsInFS", "CMSStoreFormLayoutsInFS");

            // Import export handlers
            FormControlExport.Init();
            FormControlImport.Init();
            ImportSpecialActions.Init();
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            FormHelper.Clear(logTasks);
        }

        #endregion
    }
}