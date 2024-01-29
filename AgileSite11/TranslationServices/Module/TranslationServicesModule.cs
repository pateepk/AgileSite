using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Base;
using CMS.TranslationServices;

[assembly: RegisterModule(typeof(TranslationServicesModule))]

namespace CMS.TranslationServices
{
    /// <summary>
    /// Represents the Translation Services module.
    /// </summary>
    public class TranslationServicesModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for translation services.
        /// </summary>
        public const string TRANSLATION_SERVICES_EMAIL_TEMPLATE_TYPE_NAME = "translationservices";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public TranslationServicesModule()
            : base(new TranslationServicesModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            TranslationServicesHandlers.Init();
            ImportSpecialActions.Init();

            ExtendList<MacroResolverStorage, MacroResolver>.With("TranslationServicesResolver").WithLazyInitialization(() => TranslationServicesResolvers.TranslationServicesResolver);
        }

        #endregion
    }
}