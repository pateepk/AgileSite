using System;
using System.Linq;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Core;
using CMS.Helpers;
using CMS.OnlineMarketing.Web.UI;
using CMS.SiteProvider;

[assembly: RegisterLiveTileModelProvider(ModuleName.ABTEST, "ABTestListing", typeof(AbTestLiveTileModelProvider))]

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Provides live tile model for the AB test dashboard tile.
    /// </summary>
    internal class AbTestLiveTileModelProvider : ILiveTileModelProvider
    {
        /// <summary>
        /// Loads total number of currently running AB tests.
        /// </summary>
        /// <param name="liveTileContext">Context of the live tile. Contains information about the user and the site the model is requested for</param>
        /// <exception cref="ArgumentNullException"><paramref name="liveTileContext"/> is null</exception>
        /// <returns>Live tile model</returns>
        public LiveTileModel GetModel(LiveTileContext liveTileContext)
        {
            if (liveTileContext == null)
            {
                throw new ArgumentNullException("liveTileContext");
            }

            if (!ABTestInfoProvider.ABTestingEnabled(liveTileContext.SiteInfo.SiteName))
            {
                return null;
            }

            var runningTestsCount = GetRunningTestsCount(liveTileContext.SiteInfo);

            return new LiveTileModel
            {
                Value = runningTestsCount,
                Description = ResHelper.GetString("abtest.livetiledescription")
            };
        }


        /// <summary>
        /// Gets number of running tests.
        /// </summary>
        /// <param name="site">Tests site</param>
        /// <returns>Number of tests</returns>
        private static int GetRunningTestsCount(SiteInfo site)
        {
            var runningTestsCount = ABCachedObjects
                .GetTests()
                .Where(test => test.ABTestSiteID == site.SiteID)
                .Where(ABTestStatusEvaluator.ABTestIsRunning)
                .Count();
            return runningTestsCount;
        }
    }
}