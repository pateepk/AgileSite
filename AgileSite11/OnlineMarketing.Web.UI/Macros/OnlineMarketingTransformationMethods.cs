using System;

using CMS;
using CMS.ContactManagement;
using CMS.DocumentEngine.Web.UI;
using CMS.MacroEngine;
using CMS.OnlineMarketing.Web.UI;

[assembly: RegisterExtension(typeof(OnlineMarketingTransformationMethods), typeof(TransformationNamespace))]

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// On-line marketing methods - wrapping methods for macro resolver.
    /// </summary>
    internal class OnlineMarketingTransformationMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns e-mail domain name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns e-mail domain name.", 1)]
        [MacroMethodParam(0, "email", typeof (string), "E-mail address.")]
        public static object GetEmailDomain(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return ContactHelper.GetEmailDomain(Convert.ToString(parameters[0]));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}