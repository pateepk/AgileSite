using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;

[assembly: RegisterModule(typeof(LocalizationModule))]

namespace CMS.Localization
{
    /// <summary>
    /// Represents the Localization module.
    /// </summary>
    public class LocalizationModule : Module
    {
        internal const string CULTURE = "##CULTURE##";


        /// <summary>
        /// Default constructor
        /// </summary>
        public LocalizationModule()
            : base(new LocalizationModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            // Init the core localization service
            Service.Use<ILocalizationService, FileLocalizationService>();
            Service.Use<ICultureService, CultureService>();
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Init the database localization service
            Service.Use<ILocalizationService, LocalizationService>();

            RegisterContext<LocalizationContext>("LocalizationContext");
        }
    }
}