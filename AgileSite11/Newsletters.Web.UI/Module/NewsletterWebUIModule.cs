using CMS;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Newsletters.Web.UI;
using CMS.Newsletters.Web.UI.Internal;
using CMS.WebAnalytics.Web.UI.Internal;

[assembly: RegisterModule(typeof(NewsletterWebUIModule))]

namespace CMS.Newsletters.Web.UI
{
    internal class NewsletterWebUIModule : Module
    {
        public NewsletterWebUIModule()
            : base(new ModuleMetadata("CMS.Newsletters.Web.UI"))
        {
        }


        protected override void OnInit()
        {
            base.OnInit();

            RegisterNewsletterCampaignConversionItemFilter();
            RegisterNewsletterReportContactDemographicsDataRetrievers();
        }


        private void RegisterNewsletterCampaignConversionItemFilter()
        {
            ICampaignConversionItemFilterContainer campaignConversionItemFilterContainer = Service.Resolve<ICampaignConversionItemFilterContainer>();

            campaignConversionItemFilterContainer.RegisterFilter(NewsletterInfo.OBJECT_TYPE, new NewsletterCampaignConversionItemFilter());
        }


        private void RegisterNewsletterReportContactDemographicsDataRetrievers()
        {
            IContactDemographicsDataRetrieverFactory contactDemographicsDataRetrieverFactory = Service.Resolve<IContactDemographicsDataRetrieverFactory>();

            contactDemographicsDataRetrieverFactory.Register("openedEmail", typeof(OpenedEmailContactDemographicsDataRetriever));
            contactDemographicsDataRetrieverFactory.Register("unsubscription", typeof(UnsubscribedContactDemographicsDataRetriever));
            contactDemographicsDataRetrieverFactory.Register("clickedLink", typeof(ClickedEmailContactDemographicsDataRetriever));
        }
    }
}