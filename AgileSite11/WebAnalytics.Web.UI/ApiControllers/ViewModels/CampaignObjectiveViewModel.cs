
using Newtonsoft.Json;


namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model for campaign objective.
    /// </summary>
    public class CampaignObjectiveViewModel
    {
        /// <summary>
        /// Objective ID.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int ID
        {
            get;
            set;
        }


        /// <summary>
        /// Objective value.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public int Value
        {
            get;
            set;
        }


        /// <summary>
        /// Objective campaign ID.
        /// </summary>
        [JsonProperty(PropertyName = "campaignID")]
        public int CampaignID
        {
            get;
            set;
        }


        /// <summary>
        /// Objective conversion ID.
        /// </summary>
        [JsonProperty(PropertyName = "conversionID")]
        public int ConversionID
        {
            get;
            set;
        }
    }
}
