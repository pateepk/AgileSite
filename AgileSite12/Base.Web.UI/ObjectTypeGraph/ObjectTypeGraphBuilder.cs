using System.Linq;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Provides functionality for building the <see cref="GraphData"/> of vis.js graphs.
    /// </summary>
    internal class ObjectTypeGraphBuilder
    {
        private readonly GraphData mData = new GraphData();
        private readonly GraphNode mMainNode;

        private int mLevelOverflowThreshold = int.MaxValue;
        private int mNodeLabelMaxLength = 0;

        // Counter for graph nodes that can overflow to a lower hierarchy level
        private int mNodesWithLevelOverflow = 0;


        /// <summary>
        /// Constructor that initializes the main graph node.
        /// </summary>
        internal ObjectTypeGraphBuilder(string mainNodeLabel, string mainNodeGroup)
        {
            // Creates the main (initial) node of the object type graph
            mMainNode = new GraphNode
            {
                Id = mainNodeLabel,
                Label = mainNodeLabel,
                Title = mainNodeLabel,
                Level = 0,
                Group = mainNodeGroup,
                Fixed = true
            };

            Data.Nodes.Add(mMainNode);
        }


        /// <summary>
        /// Gets the current <see cref="GraphData"/>.
        /// </summary>
        public GraphData Data
        {
            get
            {
                return mData;
            }
        }


        /// <summary>
        /// Gets the <see cref="GraphNode"/> object representing the main (initial) graph node.
        /// </summary>
        public GraphNode MainNode
        {
            get
            {
                return mMainNode;
            }
        }


        /// <summary>
        /// Graph nodes created using the AddNodeWithLevelOverflow method have an increased hierarchy level if their number exceeds this value.
        /// </summary>
        public int LevelOverflowThreshold
        {
            get
            {
                return mLevelOverflowThreshold;
            }
            set
            {
                mLevelOverflowThreshold = value;
            }
        }


        /// <summary>
        /// Limits the maximum number of characters in graph node labels.
        /// </summary>
        public int NodeLabelMaxLength
        {
            get
            {
                return mNodeLabelMaxLength;
            }
            set
            {
                mNodeLabelMaxLength = value;
            }
        }


        /// <summary>
        /// Adds a node to the graph data.
        /// </summary>
        /// <param name="label">The node's label shown in the graph. Also used as the node ID (if unique) and in the tooltip.</param>
        /// <param name="group">The node's formatting group. Processed in the JavaScript configuration of the vis.js graph.</param>
        /// <param name="level">The node's level in the graph hierarchy.</param>
        /// <param name="multipleNodeOccurrences">
        /// The number of nodes in the graph with the same label. Used to generate a unique ID suffix.
        /// If 0, conflicting nodes are not allowed and the existing node is returned.
        /// </param>
        public GraphNode AddNode(string label, string group, int level, int multipleNodeOccurrences = 0)
        {
            string nodeId = label;

            // Generates ID suffixes for nodes that occur multiple times
            if (multipleNodeOccurrences > 0)
            {
                nodeId = nodeId + "_" + multipleNodeOccurrences;
            }
            else
            {
                // Returns the existing node if multiple node occurrences are not allowed
                var existing = Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (existing != null)
                {
                    return existing;
                }
            }

            var node = new GraphNode
            {
                Id = nodeId,
                Label = label,
                Title = label,
                Level = level,
                Group = group,
                Fixed = false
            };

            node.Label = TrimLabelLength(node.Label);
            Data.Nodes.Add(node);

            return node;
        }


        /// <summary>
        /// Adds a node to the graph data. Nodes created using this method have their hierarchy level incremented if their count exceeds the LevelOverflowThreshold value.
        /// </summary>
        /// <param name="label">The node's label shown in the graph. Also used as the node ID (if unique) and in the tooltip.</param>
        /// <param name="group">The node's formatting group. Processed in the JavaScript configuration of the vis.js graph.</param>
        /// <param name="level">The node's level in the graph hierarchy. Incremented by one if the number of created nodes exceeds the LevelOverflowThreshold value.</param>
        /// <param name="multipleNodeOccurrences">
        /// The number of nodes in the graph with the same label. Used to generate a unique ID suffix. 
        /// If 0, conflicting nodes are not allowed and the existing node is returned.
        /// </param>
        public GraphNode AddNodeWithLevelOverflow(string label, string group, int level, int multipleNodeOccurrences = 0)
        {
            if (mNodesWithLevelOverflow >= LevelOverflowThreshold)
            {
                ++level;
            }

            var node = AddNode(label, group, level, multipleNodeOccurrences);
            ++mNodesWithLevelOverflow;

            return node;
        }


        /// <summary>
        /// Adds a graph node without a label that branches into other nodes.
        /// </summary>
        /// <param name="id">Identifier of the branch node.</param>
        /// <param name="group">The node's formatting group. Processed in the JavaScript configuration of the vis.js graph.</param>
        /// <param name="level">The node's level in the graph hierarchy.</param>
        /// <param name="multipleBranchOccurrences">The number of nodes in the graph with the same id. Used to generate a unique ID suffix.</param>
        public GraphNode AddBranchNode(string id, string group, int level, int multipleBranchOccurrences = 0)
        {
            if (multipleBranchOccurrences > 0)
            {
                id = id + "_" + multipleBranchOccurrences;
            }

            var node = new GraphNode
            {
                Id = id,
                Level = level,
                Group = group,
                Fixed = true
            };

            Data.Nodes.Add(node);

            return node;
        }


        /// <summary>
        /// Adds an edge connecting two other nodes in the graph.
        /// </summary>
        /// <param name="from">The graph node that is the edge's source.</param>
        /// <param name="to">The graph node that is the edge's target.</param>
        /// <param name="color">CSS color value applied to the edge.</param>
        /// <param name="dashes">Indicates if the edge line is dashed.</param>
        /// <param name="arrowFormat">Determines the format of the edge's arrows. All arrows = "to;from;middle", no arrows = "".</param>
        public void AddEdge(GraphNode from, GraphNode to, string color, bool dashes = false, string arrowFormat = "to")
        {
            var existing = Data.Edges.FirstOrDefault(e => (e.From == from.Id && e.To == to.Id));
            if (existing != null)
            {
                return;
            }
            Data.Edges.Add(new GraphEdge
            {
                From = from.Id,
                To = to.Id,
                Color = color,
                Dashes = dashes,
                Arrows = arrowFormat,
            });
        }


        private string TrimLabelLength(string label)
        {
            // Trims the label length if needed
            if ((NodeLabelMaxLength > 0) && (label.Length > NodeLabelMaxLength))
            {
                label = label.Substring(0, NodeLabelMaxLength) + "..";
            }

            return label;
        }
    }
}
