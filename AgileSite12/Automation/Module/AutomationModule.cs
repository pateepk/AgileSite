using CMS;
using CMS.Automation;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(AutomationModule))]

namespace CMS.Automation
{
    /// <summary>
    /// Represents the automation module.
    /// </summary>
    public class AutomationModule : Module
    {
        #region "Constants"

        internal const string AUTOMATION = "##AUTOMATION##";
        internal const string ONLINEMARKETING = "##ONLINEMARKETING##";


        /// <summary>
        /// Name of email template type for automation.
        /// </summary>
        public const string AUTOMATION_EMAIL_TEMPLATE_TYPE_NAME = "automation";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public AutomationModule()
            : base(new AutomationModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ImportSpecialActions.Init();
            AutomationHandlers.Init();
        }

        #endregion
    }
}