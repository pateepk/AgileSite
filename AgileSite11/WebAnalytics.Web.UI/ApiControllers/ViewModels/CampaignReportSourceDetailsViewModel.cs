using System.ComponentModel;

using Newtonsoft.Json;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model for campaing conversion containing utm source and utm content that were used in campaign along with link to email report details.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CampaignReportSourceDetailsViewModel
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
        /// Link to email report details.
        /// </summary>
        [JsonProperty("details")]
        public EmailLinkDetailViewModel EmailLinkDetail
        {
            get;
            set;
        }
    }
}
