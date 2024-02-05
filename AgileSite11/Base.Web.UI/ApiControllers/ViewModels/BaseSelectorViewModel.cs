using Newtonsoft.Json;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base view model class for <see cref="ISelectorController{TViewModel}"/> controllers.
    /// Every view model must have <c>id</c> and <c>text</c> property.
    /// </summary>
    public class BaseSelectorViewModel
    {
        /// <summary>
        /// View model ID.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int ID
        {
            get;
            set;
        }


        /// <summary>
        /// View model text.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text
        {
            get;
            set;
        }
    }
}
