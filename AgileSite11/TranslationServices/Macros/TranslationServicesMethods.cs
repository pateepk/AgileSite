using System;

using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation services methods - wrapping methods for macro resolver.
    /// </summary>
    public class TranslationServicesMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns translation priority text.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns priority name from priority integer constant.", 1)]
        [MacroMethodParam(0, "priority", typeof (int), "Priority integer constant.")]
        public static string GetTranslationPriority(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TranslationServiceHelper.GetPriorityText(ValidationHelper.GetInteger(parameters[0], 0));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}