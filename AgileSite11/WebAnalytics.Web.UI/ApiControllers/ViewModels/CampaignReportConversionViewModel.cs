using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model of campaign conversion report.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CampaignReportConversionViewModel
    {
        /// <summary>
        /// Human-readable name of the conversion.
        /// </summary>
        [JsonProperty("name")]
        public string ConversionName
        {
            get; set;
        }


        /// <summary>
        /// Human-readable name of the conversion type.
        /// </summary>
        [JsonProperty("typeName")]
        public string ConversionTypeName
        {
            get;
            internal set;
        }


        /// <summary>
        /// Number of occurrences of the conversion.
        /// </summary>
        [JsonProperty("hits")]
        public int ConversionHitsCount
        {
            get; set;
        }


        /// <summary>
        /// Collection of sources for the conversion.
        /// </summary>
        [JsonProperty("sources")]
        public IEnumerable<CampaignReportSourceViewModel> ConversionSources
        {
            get; set;
        }


        /// <summary>
        /// Boolean flag if conversion is part of funnel.
        /// </summary>
        [JsonProperty(PropertyName = "isFunnelStep")]
        public bool IsFunnelStep
        {
            get; set;
        }


        /// <summary>
        /// ID of related campaign conversion object.
        /// </summary>
        [JsonProperty(PropertyName = "campaignConversionID")]
        public int CampaignConversionID
        {
            get;
            set;
        }
    }
}
