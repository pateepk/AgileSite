using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(ApplicationDashboardModule))]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Represents the Online Marketing module.
    /// </summary>
    internal class ApplicationDashboardModule : Module
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
        public ApplicationDashboardModule()
            : base(new ApplicationDashboardModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Init events handlers
            ApplicationDashboardModuleHandlers.Init();
        }
    }
}