using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Base;
using CMS.Globalization;

[assembly: RegisterModule(typeof(GlobalizationModule))]

namespace CMS.Globalization
{
    /// <summary>
    /// Represents the Globalization module.
    /// </summary>
    public class GlobalizationModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public GlobalizationModule()
            : base(new GlobalizationModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Register Countries object to MacroEngine
            Extend<IMacroRoot>.WithStaticProperty<CountriesMacroContainer>("Countries").AsSingleton();

            ClassHelper.RegisterAliasFor<TimeZoneTypeEnum>("CMS.SiteProvider", "CMS.SiteProvider.TimeZoneTypeEnum");
        }
    }
}