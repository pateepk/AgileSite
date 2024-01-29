using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(BaseMacroMethods), typeof(UtilNamespace))]

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base macro methods - wrapping methods for macro resolver.
    /// </summary>
    public class BaseMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Escapes the given string to be used in the JavaScript string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Escapes the given string to be used in the JavaScript string.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be processed.")]
        public static object JSEscape(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return ScriptHelper.GetString(GetStringParam(parameters[0], context.Culture), false);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
