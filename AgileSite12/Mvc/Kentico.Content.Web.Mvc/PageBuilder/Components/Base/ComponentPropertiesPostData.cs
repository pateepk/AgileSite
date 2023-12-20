using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents data from the body of HTTP post made by the component properties form.
    /// </summary>
    internal sealed class ComponentPropertiesPostData
    {
        /// <summary>
        /// String representation of the component properties JSON.
        /// </summary>
        [JsonProperty("properties")]
        [JsonConverter(typeof(JsonConverterObjectToString))]
        [JsonRequired]
        public string Properties
        {
            get;
            set;
        }
    }
}
