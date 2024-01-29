using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Base;
using CMS.WebFarmSync;

[assembly: RegisterModule(typeof(WebFarmSyncModule))]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Represents the Web Farm Synchronization module.
    /// </summary>
    public class WebFarmSyncModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebFarmSyncModule()
            : base(new WebFarmSyncModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            Service.Use<IWebFarmService, WebFarmService>();
            DebugHelper.RegisterDebug(WebFarmDebug.Settings);
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Register web farm tasks
            SystemTasks.Init();

            ImportSpecialActions.Init();

            WebFarmSyncHandlers.Init();
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            WebFarmContext.Clear(logTasks);
        }
    }
}