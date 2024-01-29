using Newtonsoft.Json;

using CMS.Activities;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model for <see cref="ActivityTypeInfo"/>
    /// </summary>
    public class ActivityTypeViewModel
    {
        /// <summary>
        /// Type of activity, e.g. pagevisit.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get;
            set;
        }


        /// <summary>
        /// Activity type display name.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName
        {
            get;
            set;
        }
    }
}
