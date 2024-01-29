using CMS.DataEngine;
using CMS.WebAnalytics.Web.UI.Internal;

namespace CMS.Newsletters.Web.UI.Internal
{
    internal class NewsletterCampaignConversionItemFilter : ICampaignConversionItemFilter
    {
        public ObjectQuery Filter(ObjectQuery objectQuery)
        {
            return objectQuery.WhereEquals("NewsletterType", EmailCommunicationTypeEnum.Newsletter);
        }
    }
}
