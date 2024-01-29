using Newtonsoft.Json;

using CMS.SiteProvider;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model for campaign conversions.
    /// </summary>
    public class CampaignConversionViewModel
    {
        /// <summary>
        /// Conversion ID.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int ID
        {
            get;
            set;
        }


        /// <summary>
        /// Conversion name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Activity name.
        /// </summary>
        [JsonProperty(PropertyName = "activityName")]
        public string ActivityName
        {
            get;
            set;
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        [JsonProperty(PropertyName = "activityType")]
        public string ActivityType
        {
            get;
            set;
        }


        /// <summary>
        /// ID of campaign this conversion belongs to. Used when creating or updating conversion.
        /// </summary>
        [JsonProperty(PropertyName = "campaignID")]
        public int CampaignID
        {
            get;
            set;
        }


        /// <summary>
        /// ID of object which is monitored by conversion. For example nodeid for <c>pagevisit</c> <see cref="ActivityType"/>.
        /// </summary>
        [JsonProperty(PropertyName = "itemID")]
        public int? ItemID
        {
            get;
            set;
        }


        /// <summary>
        /// Order of conversion in the campaign.
        /// </summary>
        [JsonProperty(PropertyName = "order")]
        public int Order
        {
            get;
            set;
        }


        /// <summary>
        /// Boolean flag if conversion is part of funnel.
        /// </summary>
        [JsonProperty(PropertyName = "isFunnelStep")]
        public bool IsFunnelStep
        {
            get;
            set;
        }


        /// <summary>
        /// Page visit URL. Used if a campaign is running on content only site <see cref="SiteInfo.SiteIsContentOnly"/>.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url
        {
            get;
            set;
        }
    }
}