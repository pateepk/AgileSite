using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Contains A/B test helper methods.
    /// </summary>
    public class ABTestHelper : AbstractHelper<ABTestHelper>
    {
        /// <summary>
        /// Gets the latest version of a page for the given <paramref name="abTest"/>.
        /// Site and culture of the A/B test has to match site and culture of the page.
        /// </summary>
        /// <param name="abTest">A/B test to get the page for.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="abTest"/> is null.</exception>
        /// <returns>Found page.</returns>
        public static TreeNode GetABTestPage(ABTestInfo abTest)
        {
            return HelperObject.GetABTestPageInternal(abTest);
        }


        /// <summary>
        /// Gets names of the current user's A/B tests.
        /// </summary>
        public static IEnumerable<string> GetUsersTests()
        {
            return HelperObject.GetUsersTestsInternal();
        }


        /// <summary>
        /// Returns collection with variant identifiers of A/B tests, which are running and in which the current user is included.
        /// </summary>
        public static IEnumerable<Guid> GetValidVariants()
        {
            return HelperObject.GetValidVariantsInternal();
        }


        /// <summary>
        /// Returns whether <paramref name="abTest"/> has it's variants materialized.
        /// </summary>
        internal static bool HasABTestVariantsMaterialized(ABTestInfo abTest)
        {
            return ABVariantDataInfoProvider.GetVariantsData().WhereEquals("ABVariantTestID", abTest.ABTestID).GetCount() > 0;
        }


        /// <summary>
        /// Gets the latest version of a page for given <paramref name="abTest"/>.
        /// Site and culture of the A/B test has to match site and culture of the page.
        /// </summary>
        /// <param name="abTest">A/B test to get the page for.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="abTest"/> is null.</exception>
        /// <returns>Found page.</returns>
        protected virtual TreeNode GetABTestPageInternal(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException(nameof(abTest));
            }

            return new DocumentQuery()
                .OnSite(abTest.ABTestSiteID)
                .Culture(abTest.ABTestCulture)
                .Path(abTest.ABTestOriginalPage)
                .LatestVersion(true)
                .Published(false)
                .FirstOrDefault();
        }


        /// <summary>
        /// Gets names of the current user's A/B tests.
        /// </summary>
        protected virtual IEnumerable<string> GetUsersTestsInternal()
        {
            // Get names of all user's cookies
            var cookieNames = Service.Resolve<IABResponseCookieProvider>().GetDistinctCookieNames();

            // Return A/B test names
            return cookieNames.Where(c => c.StartsWith(ABTestConstants.AB_COOKIE_PREFIX, StringComparison.OrdinalIgnoreCase))
                              .Select(c => c.Substring(ABTestConstants.AB_COOKIE_PREFIX.Length));
        }


        /// <summary>
        /// Returns collection with variant identifiers of A/B tests, which are running and in which the current user is included.
        /// </summary>
        protected virtual IEnumerable<Guid> GetValidVariantsInternal()
        {
            var userStateManagerFactory = Service.Resolve<IABUserStateManagerFactory>();
            var siteName = Service.Resolve<ISiteService>().CurrentSite?.SiteName;

            foreach (var abTestName in GetUsersTests())
            {
                var abTestInfo = ABTestInfoProvider.GetABTestInfo(abTestName, siteName);
                if (abTestInfo == null || !ABTestStatusEvaluator.ABTestIsRunning(abTestInfo))
                {
                    continue;
                }

                var manager = userStateManagerFactory.Create<Guid>(abTestInfo.ABTestName);
                if (manager.IsExcluded)
                {
                    continue;
                }

                yield return manager.GetVariantIdentifier();
            }
        }
    }
}
