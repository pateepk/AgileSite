using CMS;
using CMS.DataEngine;
using CMS.Synchronization;

[assembly: RegisterModule(typeof(SynchronizationModule))]

namespace CMS.Synchronization
{
    /// <summary>
    /// Represents the Synchronization module.
    /// </summary>
    public class SynchronizationModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SynchronizationModule()
            : base(new SynchronizationModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            Synchronization.Init();
            SynchronizationHandlers.Init();

            VersioningHandlers.Init();
        }

        #endregion
    }
}