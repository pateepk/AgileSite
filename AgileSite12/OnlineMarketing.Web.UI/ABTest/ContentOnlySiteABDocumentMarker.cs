using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class used to mark documents on <see cref="DocumentEvents.GetDocumentMark"/> event. Generates HTML of images that are to be added to
    /// <see cref="DocumentMarkEventArgs.MarkContent"/> event arguments property.
    /// </summary>
    /// <remarks>
    /// Implementation is designed for content only sites.
    /// </remarks>
    internal class ContentOnlySiteABDocumentMarker : IABDocumentMarker
    {
        private readonly string mPath;
        private readonly string mSiteName;
        private readonly string mCulture;
        private readonly IDateTimeNowService mDateTimeService;
        private readonly ILocalizationService mLocalizationService;


        /// <summary>
        /// Initializes a new instance of <see cref="ContentOnlySiteABDocumentMarker"/>.
        /// </summary>
        /// <param name="path">Path (<see cref="TreeNode.NodeAliasPath"/>) of a document</param>
        /// <param name="siteName">Name of the site that the document is on</param>
        /// <param name="culture">Culture of the document</param>
        public ContentOnlySiteABDocumentMarker(string path, string siteName, string culture)
            : this(path, siteName, culture, Service.Resolve<IDateTimeNowService>(), Service.Resolve<ILocalizationService>())
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ContentOnlySiteABDocumentMarker"/>.
        /// </summary>
        /// <param name="path">Path (<see cref="TreeNode.NodeAliasPath"/>) of a document</param>
        /// <param name="siteName">Name of the site that the document is on</param>
        /// <param name="culture">Culture of the document</param>
        /// <param name="dateTimeService">Instance of <see cref="IDateTimeNowService"/></param>
        /// <param name="localizationService">Instance of <see cref="ILocalizationService"/></param>
        internal ContentOnlySiteABDocumentMarker(string path, string siteName, string culture, IDateTimeNowService dateTimeService, ILocalizationService localizationService)
        {
            mPath = path;
            mSiteName = siteName;
            mCulture = culture;
            mDateTimeService = dateTimeService;
            mLocalizationService = localizationService;
        }


        /// <summary>
        /// Returns HTML code of all A/B icons to mark the document specified in constructor.
        /// </summary>
        /// <returns>HTML of all icons marking the specified document</returns>
        public string GetIcons()
        {
            var createdTests = CacheHelper.Cache(GetCreatedTests, new CacheSettings(ABHandlers.CACHE_MINUTES, "CreatedABTestsForIcons", mSiteName, mCulture)
            {
                CacheDependency = CacheHelper.GetCacheDependency(new[]
                {
                    CacheHelper.GetCacheItemName(null, ABTestInfo.OBJECT_TYPE, "all")
                })
            });

            var runningTests = CacheHelper.Cache(GetRunningTests, new CacheSettings(ABHandlers.CACHE_MINUTES, "RunningABTestsForIcons", mSiteName, mCulture)
            {
                CacheDependency = CacheHelper.GetCacheDependency(new[]
                {
                    CacheHelper.GetCacheItemName(null, ABTestInfo.OBJECT_TYPE, "all")
                })
            });

            var finishedTests = CacheHelper.Cache(GetFinishedTests, new CacheSettings(ABHandlers.CACHE_MINUTES, "FinishedABTestsForIcons", mSiteName, mCulture)
            {
                CacheDependency = CacheHelper.GetCacheDependency(new[]
                {
                    CacheHelper.GetCacheItemName(null, ABTestInfo.OBJECT_TYPE, "all")
                })
            });

            var test = GetTest(createdTests, runningTests, finishedTests);
            if (test == null)
            {
                return null;
            }

            var color = GetColor(runningTests, finishedTests);
            var tooltip = GetTooltip(test);

            return UIHelper.GetAccessibleIconTag($"NodeLink icon-two-squares-line tn {color}", tooltip);
        }


        private ABTestInfo GetTest(IEnumerable<ABTestInfo> createdTests, IEnumerable<ABTestInfo> runningTests, IEnumerable<ABTestInfo> finishedTests)
        {
            var test = (finishedTests.FirstOrDefault(i => String.Equals(i.ABTestOriginalPage, mPath, StringComparison.OrdinalIgnoreCase))
                       ?? runningTests.FirstOrDefault(i => String.Equals(i.ABTestOriginalPage, mPath, StringComparison.OrdinalIgnoreCase)))
                       ?? createdTests.FirstOrDefault(i => String.Equals(i.ABTestOriginalPage, mPath, StringComparison.OrdinalIgnoreCase));

            return test;
        }


        private string GetColor(IEnumerable<ABTestInfo> runningTests, IEnumerable<ABTestInfo> finishedTests)
        {
            if (finishedTests.Select(i => i.ABTestOriginalPage).Distinct().ToHashSetCollection(StringComparer.OrdinalIgnoreCase).Contains(mPath))
            {
                return "color-orange-80";
            }

            if (runningTests.Select(i => i.ABTestOriginalPage).Distinct().ToHashSetCollection(StringComparer.OrdinalIgnoreCase).Contains(mPath))
            {
                return "color-blue-70";
            }

            return "color-gray-50";
        }


        private string GetTooltip(ABTestInfo test)
        {
            var tooltip = mLocalizationService.GetString("ABTesting.DocumentContainsTest");
            return string.Format(tooltip, test.ABTestDisplayName);
        }


        private ICollection<ABTestInfo> GetFinishedTests()
        {
            var currentDateTime = mDateTimeService.GetDateTimeNow();
            return GetTests().WhereLessThan("ABTestOpenTo", currentDateTime).ToList();

        }


        private ICollection<ABTestInfo> GetRunningTests()
        {
            var currentDateTime = mDateTimeService.GetDateTimeNow();
            return GetTests().WhereLessThan("ABTestOpenFrom", currentDateTime).ToList();

        }


        private ICollection<ABTestInfo> GetCreatedTests()
        {
            var currentDateTime = mDateTimeService.GetDateTimeNow();

            return GetTests().Where(new WhereCondition().WhereNull("ABTestOpenFrom").Or().WhereGreaterThan("ABTestOpenFrom", currentDateTime))
                             .And()
                             .Where(new WhereCondition().WhereNull("ABTestOpenTo").Or().WhereGreaterThan("ABTestOpenTo", currentDateTime))
                             .ToList();
        }


        private ObjectQuery<ABTestInfo> GetTests()
        {
            return ABTestInfoProvider.GetABTests()
                                     .WhereEquals("ABTestCulture", mCulture)
                                     .WhereNull("ABTestWinnerGUID")
                                     .OnSite(mSiteName)
                                     .Columns("ABTestOriginalPage", "ABTestDisplayName");
        }
    }
}
