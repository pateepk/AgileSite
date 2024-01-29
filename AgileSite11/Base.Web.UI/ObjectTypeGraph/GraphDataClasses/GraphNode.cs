using Newtonsoft.Json;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Represents a graph node in data for vis.js object type graphs.
    /// </summary>    
    [JsonObject]
    internal class GraphNode
    {
        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }


        [JsonProperty("label")]
        public string Label
        {
            get;
            set;
        }


        [JsonProperty("title")]
        public string Title
        {
            get;
            set;
        }


        [JsonProperty("level")]
        public int Level
        {
            get;
            set;
        }


        [JsonProperty("group")]
        public string Group
        {
            get;
            set;
        }


        [JsonProperty("fixed")]
        public bool Fixed
        {
            get;
            set;
        }
    }
}
