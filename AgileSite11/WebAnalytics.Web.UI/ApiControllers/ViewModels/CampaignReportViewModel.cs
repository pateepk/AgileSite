using System;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model containing data for campaign report.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CampaignReportViewModel
    {
        /// <summary>
        /// Human-readable name of the campaign.
        /// </summary>
        [JsonProperty("name")]
        public string CampaignName
        {
            get; set;
        }


        /// <summary>
        /// Human-readable description of the campaign.
        /// </summary>
        [JsonProperty("description")]
        public string CampaignDescription
        {
            get; set;
        }


        /// <summary>
        /// Status of the campaign.
        /// </summary>
        [JsonProperty("status")]
        public string CampaignStatus
        {
            get; set;
        }


        /// <summary>
        /// Date and time of the campaign launch or null when campaign does not have one.
        /// </summary>
        [JsonProperty("launchDate")]
        public DateTime? CampaignLaunchDate
        {
            get; set;
        }


        /// <summary>
        /// Date and time of the campaign finish or null when campaign does not have one.
        /// </summary>
        [JsonProperty("finishDate")]
        public DateTime? CampaignFinishDate
        {
            get; set;
        }


        /// <summary>
        /// Date and time of the last update of the campaign report.
        /// </summary>
        [JsonProperty("updateDate")]
        public DateTime? CampaignReportUpdated
        {
            get; set;
        }


        /// <summary>
        /// Collection of conversions tracked within campaign.
        /// </summary>
        [JsonProperty("conversions")]
        public IEnumerable<CampaignReportConversionViewModel> CampaignConversions
        {
            get; set;
        }


        /// <summary>
        /// Collection of campaign conversion source details.
        /// </summary>
        [JsonProperty("sourceDetails")]
        public IEnumerable<CampaignReportSourceDetailsViewModel> CampaignSourceDetails
        {
            get; set;
        }


        /// <summary>
        /// Campaign goal details.
        /// </summary>
        [JsonProperty("objective")]
        public object CampaignObjective
        {
            get; set;
        }
    }
}
