using CMS.Core;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Represents the Workflow module metadata.
    /// </summary>
    public class WorkflowModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WorkflowModuleMetadata()
            : base(ModuleName.WORKFLOWENGINE)
        {
            RootPath = "~/CMSModules/Workflows/";
        }
    }
}