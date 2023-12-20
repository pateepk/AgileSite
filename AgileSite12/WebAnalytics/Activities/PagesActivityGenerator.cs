using System.ComponentModel;

using CMS.Activities;
using CMS.Core;
using CMS.DocumentEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Contains methods for generating sample activities data.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PagesActivityGenerator
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Generates page visit <see cref="IActivityInfo"/> for given <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page to generate activity for</param>
        /// <param name="contactId">Contact to generate activity for</param>
        /// <param name="siteId">Site to generate activity in</param>
        public void GeneratePageVisitActivity(TreeNode page, int contactId, int siteId)
        {
            var activityInitializer = new PageVisitActivityInitializer(page.GetDocumentName(), page)
                                            .WithContactId(contactId)
                                            .WithSiteId(siteId);

            mActivityLogService.LogWithoutModifiersAndFilters(activityInitializer);
        }


        /// <summary>
        /// Generates internal search <see cref="IActivityInfo"/> for given <paramref name="page"/>.
        /// </summary>
        /// <param name="keyword">Keyword searched for</param>
        /// <param name="page">Page to generate activity for</param>
        /// <param name="contactId">Contact to generate activity for</param>
        /// <param name="siteId">Site to generate activity in</param>
        public void GenerateInternalSearchActivity(string keyword, TreeNode page, int contactId, int siteId)
        {
            var activityInitializer = new InternalSearchActivityInitializer(keyword, page)
                                            .WithContactId(contactId)
                                            .WithSiteId(siteId);

            mActivityLogService.LogWithoutModifiersAndFilters(activityInitializer);
        }
    }
}
