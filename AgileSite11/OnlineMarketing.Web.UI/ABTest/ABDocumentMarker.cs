using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class used to mark documents on <see cref="DocumentEvents.GetDocumentMark"/> event. Generates HTML of images that are to be added to
    /// <see cref="DocumentMarkEventArgs.MarkContent"/> event arguments property.
    /// </summary>
    internal class ABDocumentMarker
    {
        #region "Variables"

        private readonly string mPath;
        private readonly string mSiteName;
        private readonly string mCulture;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Path (<see cref="TreeNode.NodeAliasPath"/>) of a document</param>
        /// <param name="siteName">Name of the site that the document is on</param>
        /// <param name="culture">Culture of the document</param>
        public ABDocumentMarker(string path, string siteName, string culture)
        {
            mPath = path;
            mSiteName = siteName;
            mCulture = culture;
        }


        /// <summary>
        /// Returns HTML code of all A/B icons to mark the document specified in constructor.
        /// </summary>
        /// <returns>HTML of all icons marking the specified document</returns>
        public string GetIcons()
        {
            var sb = new StringBuilder();

            AppendRunningTestIcon(sb);

            return sb.ToString();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Appends icon representing running AB test if the document belongs to a variant that's in unfinished A/B test. 
        /// Unfinished AB test means that <see cref="ABTestInfo.ABTestOpenTo"/> is empty or greater than <see cref="DateTime.Now"/>.
        /// </summary>
        private void AppendRunningTestIcon(StringBuilder sb)
        {
            // Get cached list with alias paths that have unfinished test on them
            ILookup<string, string> testsByAliasPaths = CacheHelper.Cache(() => GetAliasPathsForUnfinishedTests(mSiteName, mCulture), new CacheSettings(ABHandlers.CACHE_MINUTES, "nodealiaspathswithunfinishedtest", mSiteName, mCulture)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[]
                {
                    ABTestInfo.OBJECT_TYPE + "|all",
                    ABVariantInfo.OBJECT_TYPE + "|all"
                })
            });

            if ((testsByAliasPaths != null) && testsByAliasPaths.Contains(mPath))
            {
                string tooltip = CoreServices.Localization.GetString("ABTesting.DocumentContainsTest");

                tooltip = string.Format(tooltip, string.Join(", ", testsByAliasPaths[mPath]));

                // Add A/B mark image as font icon (italic tag with appropriate class)
                sb.AppendFormat(UIHelper.GetAccessibleIconTag("NodeLink icon-two-squares-line tn", tooltip));
            }
        }


        /// <summary>
        /// Returns alias paths of documents included in unfinished tests.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture</param>
        private static ILookup<string, string> GetAliasPathsForUnfinishedTests(string siteName, string culture)
        {
            // Get variants paths with test names of unfinished tests
            var variantPaths = ABVariantInfoProvider.GetVariants()
                                                    .Columns("ABVariantPath", "ABTestDisplayName")
                                                    .Source(s => s.Join<ABTestInfo>("ABVariantTestID", "ABTestID"))
                                                    // Unfilled date is considered to be in the future too
                                                    .Where(w => w.Where("ABTestOpenTo", QueryOperator.LargerThan, DateTime.Now)
                                                                 .Or()
                                                                 .WhereEmpty("ABTestOpenTo"))
                                                    // All cultures or specified culture
                                                    .Where(w => w.WhereEmpty("ABTestCulture")
                                                                 .Or()
                                                                 .WhereEquals("ABTestCulture", culture))
                                                    .OnSite(siteName)
                                                    .Result;

            if (DataHelper.DataSourceIsEmpty(variantPaths))
            {
                return null;
            }

            return variantPaths.Tables[0].Rows.Cast<DataRow>().ToLookup(row => DataHelper.GetStringValue(row, "ABVariantPath"), row => ResHelper.LocalizeString(DataHelper.GetStringValue(row, "ABTestDisplayName")));
        }

        #endregion
    }
}