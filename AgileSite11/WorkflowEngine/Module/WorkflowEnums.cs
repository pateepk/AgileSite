using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Base;
using CMS.WorkflowEngine;

[assembly: RegisterExtension(typeof(WorkflowEnums), typeof(EnumsNamespace))]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Wrapper class to provide workflow enumerations.
    /// </summary>
    internal class WorkflowEnums : MacroFieldContainer
    {
        /// <summary>
        /// Registers the workflow enumerations.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            // Register enumerations
            RegisterField(new MacroField("WorkflowStepTypeEnum", () => new EnumDataContainer(typeof(WorkflowStepTypeEnum))));
            RegisterField(new MacroField("WorkflowTypeEnum", () => new EnumDataContainer(typeof(WorkflowTypeEnum))));
        }
    }
}