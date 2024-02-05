using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class WorkflowResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mWorkflowBaseResolver = null;
        private static MacroResolver mWorkflowSimpleResolver = null;

        #endregion


        /// <summary>
        /// Returns base empty workflow macro resolver.
        /// </summary>
        public static MacroResolver WorkflowBaseResolver
        {
            get
            {
                if (mWorkflowBaseResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("CurrentUser", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));

                    mWorkflowBaseResolver = resolver;
                }

                return mWorkflowBaseResolver;
            }
        }


        /// <summary>
        /// Returns simple empty workflow macro resolver.
        /// </summary>
        public static MacroResolver WorkflowSimpleResolver
        {
            get
            {
                if (mWorkflowSimpleResolver == null)
                {
                    MacroResolver resolver = WorkflowBaseResolver.CreateChild();

                    resolver.SetNamedSourceData("CurrentStep", ModuleManager.GetReadOnlyObject(WorkflowStepInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Workflow", ModuleManager.GetReadOnlyObject(WorkflowInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("ActionDefinition", ModuleManager.GetReadOnlyObject(WorkflowActionInfo.OBJECT_TYPE));

                    mWorkflowSimpleResolver = resolver;
                }

                return mWorkflowSimpleResolver;
            }
        }
    }
}