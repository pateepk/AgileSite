using System;

using CMS;
using CMS.Core;
using CMS.Base;

[assembly: RegisterModule(typeof(BaseModule))]

namespace CMS.Base
{
    /// <summary>
    /// Represents the Settings Provider module.
    /// </summary>
    public class BaseModule : ModuleEntry
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseModule()
            : base(new BaseModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            DebugHelper.RegisterDebug(HandlersDebug.Settings);

            InitSettingsServices();

            ObjectFactory<IModuleUsageCounter>.SetObjectTypeTo<DefaultModuleUsageCounter>(true);
            ObjectFactory<IModuleUsageDataSourceContainer>.SetObjectTypeTo<DefaultModuleUsageDataSourceContainer>(true);
            ObjectFactory<IModuleUsageDataCollection>.SetObjectTypeTo<DefaultModuleUsageDataCollection>(true);
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            BaseSynchronization.Init();
        }


        /// <summary>
        /// Initializes the settings services
        /// </summary>
        public static void InitSettingsServices()
        {
            Service.Use<IAppSettingsService, AppSettingsService>();
            Service.Use<IConnectionStringService, ConnectionStringService>();
        }
    }
}
