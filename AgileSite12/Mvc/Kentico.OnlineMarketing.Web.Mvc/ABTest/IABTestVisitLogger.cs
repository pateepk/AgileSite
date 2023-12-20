using CMS;
using CMS.Core;
using CMS.DocumentEngine;

using Kentico.OnlineMarketing.Web.Mvc;

[assembly: RegisterImplementation(typeof(IABTestVisitLogger), typeof(ABTestVisitLogger), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Defines method for logging A/B test visits.
    /// </summary>
    internal interface IABTestVisitLogger
    {
        /// <summary>
        /// Log first or return visit of A/B test on given <paramref name="page"/>.
        /// </summary>
        /// <remarks>
        /// First visit is logged when user visit A/B test for the first time. Return visit is logged when user visit A/B test for a first time in a scope of current session. Any subsequent visit is not logged.
        /// </remarks>
        /// <param name="page">Page containing visited A/B test.</param>
        /// <param name="isFirstVisit">Whether to log first or return visit.</param>
        void LogVisit(TreeNode page, bool isFirstVisit);
    }
}
