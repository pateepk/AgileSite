using CMS;
using CMS.CustomTables;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;
using CMS.Base;

[assembly: RegisterModule(typeof(CustomTableModule))]

namespace CMS.CustomTables
{
    /// <summary>
    /// Represents the Custom table module.
    /// </summary>
    public class CustomTableModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomTableModule()
            : base(new CustomTableModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            InfoProviderLoader.LoadProvider += AbstractProvider_LoadProvider;

            // Init custom table info
            DataClassInfo.ReplaceWith<CustomTableInfo>().WhenColumnValue(DataClassInfo.TYPEINFO.ObjectClassName, "ClassIsCustomTable", v => ValidationHelper.GetBoolean(v, false));

            SearchIndexers.RegisterIndexer<CustomTableSearchIndexer>(CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE);
        }


        private static void AbstractProvider_LoadProvider(object sender, LoadProviderEventArgs e)
        {
            var objectType = e.ObjectType;
            if (CustomTableItemProvider.IsCustomTableItemObjectType(objectType))
            {
                // Check if Custom table exists
                var dataClass = DataClassInfoProvider.GetDataClassInfo(CustomTableItemProvider.GetClassName(objectType));
                e.Provider = (dataClass != null) ? CustomTableItemProvider.LoadProviderInternal(objectType) : null;
                e.ProviderLoaded = true;
            }
        }


        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            CustomTableSynchronization.Init();

            // Init event handlers
            CustomTableHandlers.Init();
            ContinuousIntegrationHandlers.Init();

            // Import export handlers
            CustomTableExport.Init();
            CustomTableImport.Init();
        }


        /// <summary>
        /// Gets the object created from the given DataRow.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public override BaseInfo GetObject(string objectType)
        {
            if (objectType != null)
            {
                objectType = objectType.ToLowerCSafe();

                // Custom table item object type
                if (CustomTableItemProvider.IsCustomTableItemObjectType(objectType))
                {
                    DataClassInfo dataClass = DataClassInfoProvider.GetDataClassInfo(CustomTableItemProvider.GetClassName(objectType));

                    // Check if Custom table exists
                    if (dataClass != null)
                    {
                        return CustomTableItem.New(dataClass.ClassName);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            CustomTableItemProvider.Clear(logTasks);
        }
    }
}