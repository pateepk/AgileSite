using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Base;

[assembly: RegisterModule(typeof(LicenseModule))]

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Represents the License module.
    /// </summary>
    public class LicenseModule : Module
    {
        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        public LicenseModule()
            : base(new LicenseModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module.
        /// </summary>
        protected override void OnPreInit()
        {
            ObjectFactory<ILicenseService>.SetObjectTypeTo<LicenseService>();
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            InitMacros();

            LicenseHandlers.Init();
        }


        /// <summary>
        /// Initializes the macros with license module values
        /// </summary>
        private static void InitMacros()
        {
            MacroContext.GlobalResolver.SetHiddenNamedSourceData("LicenseHelper", (x) => LicenseHelperNamespace.Instance);
            MacroContext.GlobalResolver.SetHiddenNamedSourceData("FeatureEnum", (x) => new EnumDataContainer(typeof (FeatureEnum)));
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            LicenseKeyInfoProvider.Clear();
        }

        #endregion
    }
}