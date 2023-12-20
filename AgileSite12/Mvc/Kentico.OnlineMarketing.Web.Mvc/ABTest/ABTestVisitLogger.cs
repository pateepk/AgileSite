using System;

using CMS.DocumentEngine;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;
using CMS.WebAnalytics;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Class for logging of A/B test visits.
    /// </summary>
    internal class ABTestVisitLogger : IABTestVisitLogger
    {
        internal const string VISIT_TYPE_PREFIX = "abvisit";
        internal const string VISIT_TYPE_FIRST = "first";
        internal const string VISIT_TYPE_RETURN = "return";

        private readonly ICachedABTestManager abTestManager;
        private readonly IABUserStateManagerFactory abUserStateManagerFactory;

        
        /// <summary>
        /// Initializes instance of <see cref="ABTestVisitLogger"/> class with given <paramref name="abTestManager"/> and <paramref name="abUserStateManagerFactory"/>.
        /// </summary>
        public ABTestVisitLogger(ICachedABTestManager abTestManager, IABUserStateManagerFactory abUserStateManagerFactory)
        {
            this.abTestManager = abTestManager ?? throw new ArgumentNullException(nameof(abTestManager));
            this.abUserStateManagerFactory = abUserStateManagerFactory ?? throw new ArgumentNullException(nameof(abUserStateManagerFactory));
        }


        /// <summary>
        /// Log first or return visit of A/B test on given <paramref name="page"/>.
        /// </summary>
        /// <remarks>
        /// First visit is logged when user visit A/B test for the first time. Return visit is logged when user visit A/B test for a first time in a scope of current session. Any subsequent visit is not logged.
        /// </remarks>
        /// <param name="page">Page containing visited A/B test.</param>
        /// <param name="isFirstVisit">Whether to log first or return visit.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public void LogVisit(TreeNode page, bool isFirstVisit)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var abTest = abTestManager.GetRunningABTest(page);
            if (abTest == null)
            {
                return;
            }

            var stateManager = abUserStateManagerFactory.Create<Guid?>(abTest.ABTestName);
            var variantIdentifier = stateManager.GetVariantIdentifier();

            // Try set visit. This method have to be called for all visitors (incl. excluded)  due to output caching reasons.
            var abVisitSet = stateManager.SetVisit();

            // Do not log if user is excluded from test or variant has not yet been selected or user already visited this page in current session
            if (stateManager.IsExcluded || !variantIdentifier.HasValue || !abVisitSet)
            {
                return;
            }

            // Whether this is first visit in first session ever, or first visit in another session
            var visitType = isFirstVisit ? VISIT_TYPE_FIRST : VISIT_TYPE_RETURN;
            var hitName = $"{VISIT_TYPE_PREFIX}{visitType};{abTest.ABTestName};{variantIdentifier.Value.ToString()}";

            HitLogProvider.LogHit(hitName, page.Site.SiteName, page.DocumentCulture, page.NodeAliasPath, 0);
        }
    }
}
