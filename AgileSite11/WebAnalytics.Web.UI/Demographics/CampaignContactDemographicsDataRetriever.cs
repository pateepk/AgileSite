using System;
using System.Collections.Specialized;

using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;

namespace CMS.WebAnalytics.Web.UI
{
    internal class CampaignContactDemographicsDataRetriever : IContactDemographicsDataRetriever
    {
        private readonly ILocalizationService mLocalizationService;
        private readonly ICampaignActivitiesQueryBuilder mCampaignActivitiesQueryBuilder;
        

        public CampaignContactDemographicsDataRetriever(ILocalizationService localizationService, ICampaignActivitiesQueryBuilder campaignActivitiesQueryBuilder)
        {
            mLocalizationService = localizationService;
            mCampaignActivitiesQueryBuilder = campaignActivitiesQueryBuilder;
        }


        public ObjectQuery<ContactInfo> GetContactObjectQuery(NameValueCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var campaignConversion = GetCampaignConversion(parameters);
            string utmSource = parameters.Get("utmSource");
            string utmContent = parameters.Get("utmContent");

            return ContactInfoProvider.GetContacts()
                                      .WhereIn("ContactID", mCampaignActivitiesQueryBuilder.GetActivitiesQuery(campaignConversion, utmSource, utmContent)
                                                                                           .Column("ActivityContactID"));
        }


        private static CampaignConversionInfo GetCampaignConversion(NameValueCollection parameters)
        {
            int campaignConversionID = parameters.Get("campaignConversionID").ToInteger(0);
            if (campaignConversionID == 0)
            {
                throw new ArgumentException("campaignConversionID parameter has to be specified");
            }

            var campaignConversion = CampaignConversionInfoProvider.GetCampaignConversionInfo(campaignConversionID);
            if (campaignConversion == null)
            {
                throw new InvalidOperationException($"No campaign conversion found for ID ({campaignConversionID})");
            }
            return campaignConversion;
        }


        public string GetCaption()
        {
            return mLocalizationService.GetString("campaign.demographics.conversion");
        }
    }
}