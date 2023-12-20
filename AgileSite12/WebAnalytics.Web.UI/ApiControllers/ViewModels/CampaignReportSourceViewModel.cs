using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model for part of campaign conversion report specific for one source (UTM source).
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CampaignReportSourceViewModel
    {
        /// <summary>
        /// Name of the source corresponding to UTM source.
        /// </summary>
        [JsonProperty("name")]
        public string SourceName
        {
            get; set;
        }


        /// <summary>
        /// Name of the content corresponding to UTM content.
        /// </summary>
        [JsonProperty("content")]
        public string ContentName
        {
            get; set;
        }


        /// <summary>
        /// Number of conversion coming from the source.
        /// </summary>
        [JsonProperty("hits")]
        public int SourceHitsCount
        {
            get; set;
        }
    }
}
