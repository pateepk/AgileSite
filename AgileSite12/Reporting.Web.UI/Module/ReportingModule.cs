using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Reporting.Web.UI;

[assembly: RegisterModule(typeof(ReportingModule))]

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Represents the Reporting module.
    /// </summary>
    public class ReportingModule : Module
    {
        /// <summary>
        /// Identifier of <see cref="ReportingModule"/>.
        /// </summary>
        public const string MODULE_NAME = "CMS.ReportingModule.Web.UI";


        /// <summary>
        /// Creates an instance of <see cref="ReportingModule"/> class.
        /// </summary>
        public ReportingModule()
            : base(MODULE_NAME)
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ClassHelper.RegisterAliasFor<ReportSubscriptionSender>("CMS.Reporting", "CMS.Reporting.ReportSubscriptionSender");
        }
    }
}