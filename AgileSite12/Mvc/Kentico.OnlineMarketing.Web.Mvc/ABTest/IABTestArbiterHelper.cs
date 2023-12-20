using System;

using CMS.OnlineMarketing;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Defines helper methods for A/B test arbiter.
    /// </summary>
    interface IABTestArbiterHelper
    {
        /// <summary>
        /// Determines whether or not is the current user subjected to A/B testing.
        /// </summary>
        /// <param name="abTest">A/B test against which to test the current user.</param>
        /// <returns>True, if the current user is subjected to A/B testing defined by <paramref name="abTest"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="abTest"/> is null.</exception>
        bool IsCurrentUserABTested(ABTestInfo abTest);
    }
}
