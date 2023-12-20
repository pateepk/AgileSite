using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods from DebugHelper namespace in the MacroEngine.
    /// </summary>
    internal class DebugSettingsMethods : MacroMethodContainer
    {
        /// <summary>
        /// Checks if the specified debug is enabled.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Checks if the specified debug is enabled.", 1)]
        [MacroMethodParam(0, "debugName", typeof(string), "Name of the debug.")]
        public static object IsDebugEnabled(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    var name = GetStringParam(parameters[0]);
                    return DebugHelper.IsDebugEnabled(name);

                default:
                    // No other overload is supported
                    throw new NotSupportedException();
            }
        }
    }
}
