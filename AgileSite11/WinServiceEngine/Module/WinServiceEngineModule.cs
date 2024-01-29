using CMS;
using CMS.DataEngine;
using CMS.WinServiceEngine;

[assembly: RegisterModule(typeof(WinServiceEngineModule))]

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Represents the Windows service engine module.
    /// </summary>
    public class WinServiceEngineModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WinServiceEngineModule()
            : base(new WinServiceEngineModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            WinServiceEngineHandlers.Init();
        }

        #endregion
    }
}