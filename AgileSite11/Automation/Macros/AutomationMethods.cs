using System;

using CMS;
using CMS.Automation;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(AutomationMethods), typeof(AutomationStateInfo))]

namespace CMS.Automation
{
    /// <summary>
    /// Automation methods - wrapping methods for macro resolver.
    /// </summary>
    public class AutomationMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if process passed through one/all of the selected automation actions in history.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if process passed through specified actions.", 2, Name = "PassedThroughAutomationActions")]
        [MacroMethodParam(0, "state", typeof (AutomationStateInfo), "Process instance to check.")]
        [MacroMethodParam(1, "actions", typeof (string), "Action names separated with a semicolon.")]
        [MacroMethodParam(2, "allActions", typeof (string), "If true, process must have passed through all specified actions. One of them is sufficient otherwise.")]
        public static object PassedThroughActions(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return AutomationTransformationFunctions.PassedThroughActions(parameters[0], ValidationHelper.GetString(parameters[1], null), false);

                case 3:
                    return AutomationTransformationFunctions.PassedThroughActions(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if process passed through one/all of the selected automation steps in history.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (bool), "Returns true if process passed through specified tags.", 2, Name = "PassedThroughAutomationSteps")]
        [MacroMethodParam(0, "state", typeof (AutomationStateInfo), "Process instance to check.")]
        [MacroMethodParam(1, "steps", typeof (string), "Step names separated with a semicolon.")]
        [MacroMethodParam(2, "allSteps", typeof (bool), "If true, process must have passed through all specified steps. One of them is sufficient otherwise.")]
        public static object PassedThroughSteps(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return AutomationTransformationFunctions.PassedThroughSteps(parameters[0], ValidationHelper.GetString(parameters[1], null), false);

                case 3:
                    return AutomationTransformationFunctions.PassedThroughSteps(parameters[0], ValidationHelper.GetString(parameters[1], null), ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}