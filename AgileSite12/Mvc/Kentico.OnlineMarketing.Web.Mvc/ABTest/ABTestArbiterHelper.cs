using System;

using CMS;
using CMS.Core;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

using Kentico.OnlineMarketing.Web.Mvc;

[assembly:RegisterImplementation(typeof(IABTestArbiterHelper), typeof(ABTestArbiterHelper), Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Defines helper methods for A/B test arbiter.
    /// </summary>
    internal class ABTestArbiterHelper : IABTestArbiterHelper
    {
        /// <summary>
        /// Determines whether or not is the current user subjected to A/B testing.
        /// </summary>
        /// <param name="abTest">A/B test against which to test the current user.</param>
        /// <returns>True, if the current user is subjected to A/B testing defined by <paramref name="abTest"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="abTest"/> is null.</exception>
        public bool IsCurrentUserABTested(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException(nameof(abTest));
            }

            return !ABSegmentationEvaluator.CheckUserIsEditor()
                 && ABSegmentationEvaluator.CheckIncludedTrafficCondition(abTest)
                 && ABSegmentationEvaluator.CheckVisitorTargetingMacro(abTest);
        }
    }
}
