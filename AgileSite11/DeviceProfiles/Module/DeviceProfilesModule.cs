using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.MacroEngine;

[assembly: RegisterModule(typeof(DeviceProfilesModule))]

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Represents the Device profiles module.
    /// </summary>
    public class DeviceProfilesModule : Module
    {
        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        public DeviceProfilesModule()
            : base(new DeviceProfilesModuleMetadata())
        {
        }
        
        
        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            
            RegisterContext<DeviceContext>("DeviceContext");

            InitMacros();

            //// Import export handlers
            InitImportExport();
        }
        

        /// <summary>
        /// Initializes import/export handlers
        /// </summary>
        private static void InitImportExport()
        {
            DeviceProfileImport.Init();
        }


        /// <summary>
        /// Initializes the device profiles macros
        /// </summary>
        private static void InitMacros()
        {
            ExtendList<MacroResolverStorage, MacroResolver>.With("DeviceProfilesResolver").WithLazyInitialization(() => DeviceProfilesResolvers.DeviceProfilesResolver);
        }

        #endregion
    }
}