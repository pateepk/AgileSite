using CMS;
using CMS.Base;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.WebAnalytics.Internal;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterModule(typeof(WebAnalyticsWebUIModule))]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Represents the Web Analytics Web UI module.
    /// </summary>
    public class WebAnalyticsWebUIModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebAnalyticsWebUIModule()
            : base(new WebAnalyticsWebUIModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Init events handlers          
            WebAnalyticsWebUIHandlers.Init();
            WebAnalyticsServiceScriptsRenderer.Init();

            RegisterContactDetailCampaignResolver();

            IContactDemographicsDataRetrieverFactory contactObjectQueryRetrieverFactory = Service.Resolve<IContactDemographicsDataRetrieverFactory>();
            contactObjectQueryRetrieverFactory.Register("campaign", typeof(CampaignContactDemographicsDataRetriever));
        }


        private void RegisterContactDetailCampaignResolver()
        {
            var siteService = Service.Resolve<ISiteService>();
            var campaignLinkService = Service.Resolve<ICampaignLinkService>();

            var contactDetailCampaignResolver = new ContactDetailsCampaignResolver(siteService, campaignLinkService);
            var contactDetailControllerService = Service.Resolve<IContactDetailsControllerService>();

            contactDetailControllerService.RegisterContactDetailsFieldResolver("ContactCampaign", contactDetailCampaignResolver);
        }
    }
}