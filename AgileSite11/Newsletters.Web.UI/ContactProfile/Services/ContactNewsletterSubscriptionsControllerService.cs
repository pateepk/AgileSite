using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.Newsletters.Web.UI.Internal;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.SiteProvider;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactNewsletterSubscriptionsController"/>.
    /// </summary>
    internal class ContactNewsletterSubscriptionsControllerService : IContactNewsletterSubscriptionsControllerService
    {
        private readonly ISubscriptionService mSubscriptionService;
        private readonly IUILinkProvider mUILinkProvider;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactNewsletterSubscriptionsControllerService"/>.
        /// </summary>
        /// <param name="subscriptionService">Handles all work with subscriptions and unsubscriptions</param>
        /// <param name="uiLinkProvider">Provides methods for generating links to access single objects within the module. (e.g. single Site).</param>
        public ContactNewsletterSubscriptionsControllerService(ISubscriptionService subscriptionService, IUILinkProvider uiLinkProvider)
        {
            mSubscriptionService = subscriptionService;
            mUILinkProvider = uiLinkProvider;
        }


        /// <summary>
        /// Gets collection of <see cref="ContactNewsletterSubscriptionViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the collection is obtained for</param>
        /// <returns>Collection of <see cref="ContactNewsletterSubscriptionViewModel"/> for the given <paramref name="contactID"/></returns>
        public IEnumerable<ContactNewsletterSubscriptionViewModel> GetContactNewsletterSubscriptionViewModels(int contactID)
        {
            return NewsletterInfoProvider.GetNewsletters()
                                         .Columns("NewsletterID", "NewsletterDisplayName", "NewsletterSiteID")
                                         .WhereIn("NewsletterID", mSubscriptionService.GetAllActiveSubscriptions(contactID).Column("NewsletterID"))
                                         .ToList()
                                         .Select(newsletter => new ContactNewsletterSubscriptionViewModel
                                         {
                                             NewsletterName = newsletter.NewsletterDisplayName,
                                             NewsletterUrl = GetNewsletterUrl(newsletter)
                                         }).ToList().OrderBy(newsletter => newsletter.NewsletterName);
        }


        private string GetNewsletterUrl(NewsletterInfo newsletter)
        {
            var site = SiteInfoProvider.GetSiteInfo(newsletter.NewsletterSiteID);
            var objectDetailLinkParameters = new ObjectDetailLinkParameters
            {
                AllowNavigationToListing = true,
                ObjectIdentifier = newsletter.NewsletterID 
            };
            var newsletterLink = mUILinkProvider.GetSingleObjectLink(ModuleName.NEWSLETTER, "EditNewsletterProperties", objectDetailLinkParameters);
            return URLHelper.GetAbsoluteUrl(newsletterLink, site.DomainName);
        }
    }
}
