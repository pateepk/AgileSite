using System;

using CMS;
using CMS.DocumentEngine;
using CMS.MacroEngine;
using CMS.TranslationServices;

[assembly: RegisterExtension(typeof(TranslationServicesTreeNodeMethods), typeof(TreeNode))]

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation services methods - wrapping methods for macro resolver.
    /// </summary>
    public class TranslationServicesTreeNodeMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if there is at least one translation submission item with target XLIFF.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if there is at least one translation submission item with target XLIFF for given page and submission is marked as ready.", 1)]
        [MacroMethodParam(0, "document", typeof (TreeNode), "Page to check.")]
        public static object IsTranslationReady(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TranslationServicesTransformationFunctions.IsTranslationReady(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}