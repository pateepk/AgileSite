using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Base;
using CMS.WorkflowEngine;

[assembly: RegisterModule(typeof(WorkflowModule))]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Represents the Workflow module.
    /// </summary>
    public class WorkflowModule : Module
    {
        #region "Constants"

        internal const string AUTOMATION = "##AUTOMATION##";
        internal const string ONLINEMARKETING = "##ONLINEMARKETING##";

        internal const string WORKFLOW = "##WORKFLOW##";


        /// <summary>
        /// Name of email template type for workflow.
        /// </summary>
        public const string WORKFLOW_EMAIL_TEMPLATE_TYPE_NAME = "workflow";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public WorkflowModule()
            : base(new WorkflowModuleMetadata())
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
            ExportSpecialActions.Init();
            ExportWorkflow.Init();

            ExtendList<MacroResolverStorage, MacroResolver>.With("WorkflowBaseResolver").WithLazyInitialization(() => WorkflowResolvers.WorkflowBaseResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("WorkflowSimpleResolver").WithLazyInitialization(() => WorkflowResolvers.WorkflowSimpleResolver);
        }

        #endregion
    }
}