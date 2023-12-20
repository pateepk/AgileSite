using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents data from the body of HTTP post made by Page builder feature.
    /// </summary>
    internal sealed class PageBuilderPostData
    {
        /// <summary>
        /// String representation of JSON data.
        /// </summary>
        [JsonProperty("postedData")]
        [JsonConverter(typeof(JsonConverterObjectToString))]
        public string Data
        {
            get;
            set;
        }


        /// <summary>
        /// Page identifier representing the context of a page.
        /// </summary>
        [JsonProperty("pageIdentifier")]
        [JsonRequired]
        public int PageIdentifier
        {
            get;
            set;
        }
    }
}
