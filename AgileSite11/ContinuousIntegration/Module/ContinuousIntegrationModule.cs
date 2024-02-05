using CMS;
using CMS.CMSImportExport;
using CMS.ContinuousIntegration;
using CMS.ContinuousIntegration.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Synchronization;


[assembly: RegisterModule(typeof(ContinuousIntegrationModule))]

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Represents the Serialization module.
    /// </summary>
    internal class ContinuousIntegrationModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ContinuousIntegrationModule()
            : base(new ModuleMetadata(ModuleName.CONTINUOUSINTEGRATION))
        {
        }

        
        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            ContinuousIntegrationHandlers.Init();
            ContinuousIntegrationRepositoryHandlers.Init();
            ContinuousIntegrationCustomClassHandlers.Init();

            DatabaseObjectsEnumeratorFactory.RegisterObjectEnumerator(DataClassInfo.OBJECT_TYPE, (objectType, where) => new ObjectQuery(objectType)
            .WhereEqualsOrNull("ClassIsDocumentType", false)
            .WhereEqualsOrNull("ClassShowAsSystemTable", false)
            .WhereEqualsOrNull("ClassIsForm", false)
            .WhereEqualsOrNull("ClassIsCustomTable", false)
            .Where(where));

            InitImportExport();
            InitSynchronization();
        }


        private void InitImportExport()
        {
            ImportExportHelper.AddExcludedSettingKey(ContinuousIntegrationHelper.ENABLED_CI_KEY);
        }


        private void InitSynchronization()
        {
            SynchronizationHelper.AddExcludedSettingKey(ContinuousIntegrationHelper.ENABLED_CI_KEY);

            // Set API deletion based on CI setting
            ObjectEvents.RequireEventHandling.Execute += DeleteWithApi_Execute;
        }


        private void DeleteWithApi_Execute(object sender, ObjectTypeInfoEventArgs e)
        {
            e.Result |= ContinuousIntegrationHelper.IsSupportedForObjectType(e.TypeInfo);
        }
    }
}