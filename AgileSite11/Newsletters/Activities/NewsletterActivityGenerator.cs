using System.ComponentModel;

using CMS.Activities;
using CMS.Core;

namespace CMS.Newsletters
{
    /// <summary>
    /// Contains methods for generating sample activities data.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NewsletterActivityGenerator
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Generates newsletter subscribe <see cref="IActivityInfo"/> for given <paramref name="newsletter"/>.
        /// </summary>
        /// <param name="newsletter">Newsletter the activity is logged for</param>
        /// <param name="subscriberId">Subscriber to generate activity for</param>
        /// <param name="contactId">Contact to generate activity for</param>
        /// <param name="siteId">Site to generate activity in</param>
        public void GenerateNewsletterSubscribeActivity(NewsletterInfo newsletter, int subscriberId, int contactId, int siteId)
        {
            var activityInitializer = new NewsletterSubscribingActivityInitializer(newsletter, subscriberId)
                                            .WithContactId(contactId)
                                            .WithSiteId(siteId);

            mActivityLogService.LogWithoutModifiersAndFilters(activityInitializer);
        }
    }
}
