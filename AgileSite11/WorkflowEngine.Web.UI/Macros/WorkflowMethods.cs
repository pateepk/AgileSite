using System;

using CMS;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.WorkflowEngine.Web.UI;

[assembly: RegisterExtension(typeof(WorkflowMethods), typeof(TreeNode))]

namespace CMS.WorkflowEngine.Web.UI
{
    /// <summary>
    /// Workflow methods - wrapping methods for macro resolver.
    /// </summary>
    public class WorkflowMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if document passed through one/all of the selected workflow actions in history.
        /// </summary>
        /// <param name="parameters">
        /// Document to be checked;
        /// Workflow action names separated with a semicolon;
        /// Indicator whether all of the selected action should be required.
        /// </param>
        [MacroMethod(typeof (bool), "Returns true if process passed through specified actions.", 2)]
        [MacroMethodParam(0, "document", typeof (TreeNode), "Process instance to check.")]
        [MacroMethodParam(1, "actions", typeof (string), "Action names separated with a semicolon.")]
        [MacroMethodParam(2, "allActions", typeof (string), "If true, process must have passed through all specified actions. One of them is sufficient otherwise.")]
        public static object PassedThroughActions(params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return WorkflowTransformationFunctions.PassedThroughActions(parameters[0], ValidationHelper.GetString(parameters[1], null), false);

                case 3:
                    return WorkflowTransformationFunctions.PassedThroughActions(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if document passed through one/all of the selected workflow steps in history.
        /// </summary>
        /// <param name="parameters">
        /// Document to be checked;
        /// Workflow step names separated with a semicolon;
        /// Indicator whether all of the selected steps should be required.
        /// </param>
        [MacroMethod(typeof (bool), "Returns true if process passed through specified tags.", 2)]
        [MacroMethodParam(0, "document", typeof (TreeNode), "Process instance to check.")]
        [MacroMethodParam(1, "steps", typeof (string), "Step names separated with a semicolon.")]
        [MacroMethodParam(2, "allSteps", typeof (bool), "If true, process must have passed through all specified steps. One of them is sufficient otherwise.")]
        public static object PassedThroughSteps(params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return WorkflowTransformationFunctions.PassedThroughSteps(parameters[0], ValidationHelper.GetString(parameters[1], null), false);

                case 3:
                    return WorkflowTransformationFunctions.PassedThroughSteps(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}