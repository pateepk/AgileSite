using System;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Fires when there is a change in campaigns UTM parameter.
    /// </summary>
    public class CampaignUTMChangedHandler : SimpleHandler<CampaignUTMChangedHandler, CMSEventArgs<CampaignUTMChangedData>>
    {
    }
}