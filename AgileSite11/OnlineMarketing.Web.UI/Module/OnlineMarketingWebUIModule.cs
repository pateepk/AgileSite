using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineMarketing.Web.UI;

[assembly: RegisterModule(typeof(OnlineMarketingWebUIModule))]

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Represents the On-line Marketing module.
    /// </summary>
    internal class OnlineMarketingWebUIModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for scoring.
        /// </summary>
        public const string SCORING_EMAIL_TEMPLATE_TYPE_NAME = "scoring";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public OnlineMarketingWebUIModule()
            : base(new ModuleMetadata("CMS.OnlineMarketing.Web.UI"))
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ABHandlers.Init();
            MVTHandlers.Init();
        }
    }
}