using System;

using CMS;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using CMS.LicenseProvider;
using CMS.DataEngine;

[assembly: RegisterImplementation(typeof(ICampaignPersistentStorage), typeof(CookieCampaignPersistentStorage), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to the campaign and its source name stored in the cookies.
    /// </summary>
    internal class CookieCampaignPersistentStorage : ICampaignPersistentStorage
    {
        /// <summary>
        /// Gets or sets the visitor's campaign from/to a cookie.
        /// </summary>
        /// <remarks>
        /// The cookie is saved when a visitor comes to the page through URL that contains campaign parameter. Is stored for fifty years and is used for linking campaigns to conversions.
        /// </remarks>
        public string CampaignUTMCode
        {
            get
            {
                var campaignCode = CookieHelper.GetValue(CookieName.Campaign);
                if (campaignCode != null && !EnsureCampaign(campaignCode))
                {
                    RemoveCampaignCookies();
                    return null;
                }
                return campaignCode;
            }
            set
            {
                CookieHelper.SetValue(CookieName.Campaign, value, DateTime.Now.AddYears(50));
            }
        }


        /// <summary>
        /// Gets or sets the visitor's source from/to a cookie.
        /// </summary>
        /// <remarks>
        /// The cookie is saved when a visitor comes to the page through URL that contains source parameter. Is stored for fifty years and is used for linking channels to conversions.
        /// </remarks>
        public string SourceName
        {
            get
            {
                var sourceName = CookieHelper.GetValue(CookieName.Source);

                // Campaign getter performs validation of the cookie and deletes all campaign cookies
                // in case the campaign is finished or does not exist
                return string.IsNullOrEmpty(CampaignUTMCode) ? null : sourceName;
            }
            set
            {
                CookieHelper.SetValue(CookieName.Source, value, DateTime.Now.AddYears(50));
            }
        }


        /// <summary>
        /// Gets or sets the visitor's source from/to a cookie.
        /// </summary>
        /// <remarks>
        /// The cookie is saved when a visitor comes to the page through URL that contains content parameter. Is stored for fifty years and is used for linking channels to conversions.
        /// </remarks>
        public string CampaignUTMContent
        {
            get
            {
                var content = CookieHelper.GetValue(CookieName.Content);

                // Campaign getter performs validation of the cookie and deletes all campaign cookies
                // in case the campaign is finished or does not exist
                return string.IsNullOrEmpty(SourceName) ? null : content;
            }
            set
            {
                CookieHelper.SetValue(CookieName.Content, value, DateTime.Now.AddYears(50));
            }
        }


        /// <summary>
        /// Ensures that given <paramref name="campaignCode"/> represents existing <see cref="CampaignInfo"/> and the related <see cref="CampaignInfo"/> is not finished.
        /// </summary>
        /// <param name="campaignCode">Campaign UTM code.</param>
        /// <returns>True if campaign exists and has not finished yet; otherwise, false</returns>
        private static bool EnsureCampaign(string campaignCode)
        {
            // Check if sufficient license is present
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.CampaignAndConversions))
            {
                return false;
            }

            var campaign = CampaignInfoProvider.GetCampaignByUTMCode(campaignCode, SiteContext.CurrentSiteName);
            return (campaign != null) && (campaign.GetCampaignStatus(DateTime.Now) != CampaignStatusEnum.Finished);
        }


        /// <summary>
        /// Removes all cookies related to the campaign.
        /// </summary>
        private static void RemoveCampaignCookies()
        {
            CookieHelper.Remove(CookieName.Campaign);
            CookieHelper.Remove(CookieName.Source);
            CookieHelper.Remove(CookieName.Content);
        }
    }
}
