using Newtonsoft.Json;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Represents a graph edge in data for vis.js object type graphs.
    /// </summary>
    [JsonObject]
    internal class GraphEdge
    {
        [JsonProperty("from")]
        public string From
        {
            get;
            set;
        }


        [JsonProperty("to")]
        public string To
        {
            get;
            set;
        }


        [JsonProperty("arrows")]
        public string Arrows
        {
            get;
            set;
        }


        [JsonProperty("dashes")]
        public bool Dashes
        {
            get;
            set;
        }


        [JsonProperty("color")]
        public string Color
        {
            get;
            set;
        }
    }
}
