using System;

using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Provides method for resolving campaigns in contact detail component.
    /// </summary>
    internal class ContactDetailsCampaignResolver : IContactDetailsFieldResolver
    {
        private readonly ISiteService mSiteService;
        private readonly ICampaignLinkService mCampaignLinkService;


        /// <summary>
        /// Instantiates new instance of <see cref="ContactDetailsCampaignResolver"/>.
        /// </summary>
        /// <param name="siteService">Service for obtaining current site information</param>
        /// <param name="campaignLinkService">Provides methods for obtaining link to single campaign object</param>
        public ContactDetailsCampaignResolver(ISiteService siteService, ICampaignLinkService campaignLinkService)
        {
            mSiteService = siteService;
            mCampaignLinkService = campaignLinkService;
        }


        /// <summary>
        /// Resolves campaign detail field for given <paramref name="contact"/>. 
        /// </summary>
        /// <param name="contact">Contact the campaign detail field is resolved for</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c></exception>
        /// <returns><c>null</c> if no suitable <see cref="CampaignInfo"/> is found; otherwise, new instance of <see cref="ContactDetailsCampaignViewModel"/> 
        /// containing <see cref="CampaignInfo.CampaignDisplayName"/> and URL leading to the campaign</returns>
        public object ResolveField(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            if (!HasCampaign(contact))
            {
                return null;
            }

            var campaign = GetCampaignForContact(contact);
            if (campaign == null)
            {
                return null;
            }

            return BuildResultViewModel(campaign);
        }


        private static bool HasCampaign(ContactInfo contact)
        {
            return !string.IsNullOrEmpty(contact.ContactCampaign);
        }


        private CampaignInfo GetCampaignForContact(ContactInfo contact)
        {
            return CampaignInfoProvider.GetCampaignInfo(contact.ContactCampaign, mSiteService.CurrentSite.SiteName);
        }


        private ContactDetailsCampaignViewModel BuildResultViewModel(CampaignInfo campaign)
        {
            return new ContactDetailsCampaignViewModel
            {
                Text = campaign.CampaignDisplayName,
                Url = mCampaignLinkService.GetCampaignLink(campaign)
            };
        }
    }
}