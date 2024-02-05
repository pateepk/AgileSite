using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;

[assembly: RegisterModule(typeof(EventLogModule))]

namespace CMS.EventLog
{
    /// <summary>
    /// Represents the Event Log module.
    /// </summary>
    public class EventLogModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EventLogModule()
            : base(new EventLogModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            Service.Use<IEventLogService, EventLogService>();

            EventLogHandlers.Init();
            EventLogSourceHelper.RegisterDefaultEventLogListener();
        }
    }
}