using System.ComponentModel;

using Newtonsoft.Json;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model for campaing conversion containing link to email report details.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class EmailLinkDetailViewModel
    {
        /// <summary>
        /// Text to display.
        /// </summary>
        [JsonProperty("text")]
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Url to email report.
        /// </summary>
        [JsonProperty("url")]
        public string Url
        {
            get;
            set;
        }
    }
}
