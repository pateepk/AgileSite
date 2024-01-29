using System.Collections.Generic;
using Newtonsoft.Json;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Defines the data structure for vis.js object type graphs.
    /// </summary>
    [JsonObject(Id = "data")]
    internal class GraphData
    {
        private readonly List<GraphNode> mNodes = new List<GraphNode>();
        private readonly List<GraphEdge> mEdges = new List<GraphEdge>();


        [JsonProperty("nodes")]
        public List<GraphNode> Nodes
        {
            get
            {
                return mNodes;
            }
        }


        [JsonProperty("edges")]
        public List<GraphEdge> Edges
        {
            get
            {
                return mEdges;
            }
        }
    }
}