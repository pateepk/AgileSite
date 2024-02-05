using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class WorkflowResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mWorkflowBaseDocumentResolver = null;
        private static MacroResolver mWorkflowSimpleDocumentResolver = null;
        private static MacroResolver mWorkflowEmailResolver = null;

        #endregion


        /// <summary>
        /// Returns workflow e-mail template macro resolver.
        /// </summary>
        public static MacroResolver WorkflowResolver
        {
            get
            {
                if (mWorkflowEmailResolver == null)
                {
                    MacroResolver resolver = WorkflowEngine.WorkflowResolvers.WorkflowSimpleResolver.CreateChild();

                    resolver.SetNamedSourceData("Document", TreeNode.New(SystemDocumentTypes.Root));
                    resolver.SetNamedSourceData("OriginalStep", ModuleManager.GetReadOnlyObject(WorkflowStepInfo.OBJECT_TYPE));

                    RegisterStringValues(resolver, new[] { "ApplicationURL", "ApprovedBy", "ApprovedWhen", "OriginalStepName", "CurrentStepName", "Comment", "FirstName", "LastName", "UserName", "Email", "FullName", "DocumentPreviewUrl", "DocumentEditUrl", "DocumentActionName" });

                    // Prioritized properties
                    resolver.PrioritizeProperty("ApplicationURL");
                    resolver.PrioritizeProperty("Comment");
                    resolver.PrioritizeProperty("DocumentPreviewUrl");
                    resolver.PrioritizeProperty("DocumentEditUrl");
                    resolver.PrioritizeProperty("DocumentActionName");

                    mWorkflowEmailResolver = resolver;
                }

                return mWorkflowEmailResolver;
            }
        }


        /// <summary>
        /// Returns base empty workflow macro resolver with document field.
        /// </summary>
        public static MacroResolver WorkflowBaseDocumentResolver
        {
            get
            {
                if (mWorkflowBaseDocumentResolver == null)
                {
                    MacroResolver resolver = WorkflowEngine.WorkflowResolvers.WorkflowBaseResolver.CreateChild();

                    resolver.SetNamedSourceData("Document", TreeNode.New(SystemDocumentTypes.Root));

                    mWorkflowBaseDocumentResolver = resolver;
                }

                return mWorkflowBaseDocumentResolver;
            }
        }


        /// <summary>
        /// Returns simple workflow macro resolver with document field.
        /// </summary>
        public static MacroResolver WorkflowSimpleDocumentResolver
        {
            get
            {
                if (mWorkflowSimpleDocumentResolver == null)
                {
                    MacroResolver resolver = WorkflowEngine.WorkflowResolvers.WorkflowSimpleResolver.CreateChild();

                    resolver.SetNamedSourceData("Document", TreeNode.New(SystemDocumentTypes.Root));

                    mWorkflowSimpleDocumentResolver = resolver;
                }

                return mWorkflowSimpleDocumentResolver;
            }
        }
    }
}