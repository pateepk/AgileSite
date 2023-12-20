using CMS.SiteProvider;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Sample online marketing data generator providing sample data for Dancing Goat demo site.
    /// </summary>
    public sealed class OnlineMarketingSampleDataGenerator : ISampleDataGenerator
    {
        /// <summary>
        /// First name prefix of contacts generated for sample campaigns.
        /// </summary>
        private const string CONTACT_FIRST_NAME_PREFIX = "GeneratedCampaignContact";

        /// <summary>
        /// Last name prefix of contacts generated for sample campaigns.
        /// </summary>
        private const string CONTACT_LAST_NAME_PREFIX = "GeneratedCampaignContactLastName";

        /// <summary>
        /// Generates sample online marketing data. Suitable only for Dancing Goat demo site.
        /// </summary>
        /// <param name="siteID">ID of the site to generate sample data for.</param>
        public void Generate(int siteID)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteID);

            GenerateOnlineMarketingData(site);
        }


        private void GenerateOnlineMarketingData(SiteInfo site)
        {
            new CampaignContactsDataGenerator(CONTACT_FIRST_NAME_PREFIX, CONTACT_LAST_NAME_PREFIX).Generate();

            new CampaignDataGenerator(site, CONTACT_FIRST_NAME_PREFIX).Generate();

            new ABTestAndConversionDataGenerator(site).Generate();

            new OnlineMarketingDataGenerator(site).Generate();

            new ContactGroupSubscribersDataGenerator(site).Generate();

            new ScoringWithRulesGenerator(site).Generate();

            new NewslettersDataGenerator(site).Generate();

            new PersonaWithRulesGenerator(site).Generate();

            new WebAnalyticsDataGenerator(site).Generate();
        }
    }
}
