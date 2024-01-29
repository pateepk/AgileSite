using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using CMS.Base;

[assembly: RegisterModule(typeof(DataEngineModule))]

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents the Data Engine module.
    /// </summary>
    public class DataEngineModule : Module
    {       
        /// <summary>
        /// Default constructor
        /// </summary> 
        public DataEngineModule()
            : base(new DataEngineModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            InitServices();

            SqlDebug.Init();

            Extend<string>.WithMetadata<IStringMetadata>();

            // Use default implementation of GeneralInfo so that the system can work with general BaseInfo collections
            ObjectFactory<BaseInfo>.SetObjectTypeTo<GeneralInfo>(true);
            ObjectFactory<IDataClass>.SetObjectTypeTo<SimpleDataClass>(true, false);
            ObjectFactory<IInfoObjectCollection>.SetObjectTypeTo<InfoObjectCollection>(true);
        }


        /// <summary>
        /// Initializes the data engine services
        /// </summary>
        private static void InitServices()
        {
            // Ensure settings service
            Service.Use<ISettingsService, SettingsService>();

            // Ensure generic serialization service
            Service.Use(typeof(IDataContractSerializerService<>), typeof(DataContractSerializerService<>));
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Init handlers
            DataEngineHandlers.Init();

            // Init connection helper
            ConnectionHelper.Init();

            // Force object type initialization
            ObjectTypeManager.EnsureObjectTypes();

            // Register web farm tasks
            DataTasks.Init();
            StorageTasks.Init();

            // Class form virtual object
            VirtualPathHelper.RegisterVirtualPath(DataClassInfoProvider.FormLayoutsDirectory, DataClassFormVirtualObject.GetVirtualFileObject);
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);
            ClassStructureInfo.Clear(false);
        }
    }
}