using System;

using CMS;
using CMS.Core;
using CMS.HealthMonitoring;

[assembly: RegisterModule(typeof(HealthMonitoringModule))]

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Health monitoring module
    /// </summary>
    public class HealthMonitoringModule : ModuleEntry
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HealthMonitoringModule()
            : base(new HealthMonitoringModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            // Initialize the performance counter
            Service.Use<IPerformanceCounter, CMSPerformanceCounter>(transient: true);
        }


        /// <summary>
        /// Initializes the module
        /// </summary>    
        protected override void OnInit()
        {
            base.OnInit();

            DefaultCounters.RegisterPerformanceCounters();

            HealthMonitoringHandlers.InitHandlers();
        }
    }
}
