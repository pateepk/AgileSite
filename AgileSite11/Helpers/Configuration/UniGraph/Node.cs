using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace CMS.Helpers.UniGraphConfig
{
    /// <summary>
    /// Configuration class for JSON serialization.
    /// </summary>
    [DataContract(Name="Node", Namespace="CMS.Helpers.UniGraphConfig")]
    public class Node
    {
        #region "Variables"

        /// <summary>
        /// Source points on node.
        /// </summary>
        private List<SourcePoint> mSourcePoints = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of HTML element representing node.
        /// </summary>
        [DataMember]
        public string ID
        {
            get;
            set;
        }


        /// <summary>
        /// Name of node.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Whether or not is name of node resource string.
        /// </summary>
        [DataMember]
        public bool IsNameLocalized 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Content of node.
        /// </summary>
        [DataMember]
        public string Content 
        {
            get; 
            set; 
        }


        /// <summary>
        /// Absolute position of node.
        /// </summary>
        [DataMember]
        public Point Position 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Graphic type of node.
        /// </summary>
        [DataMember]
        public NodeTypeEnum Type 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Extra css classes of node.
        /// </summary>
        [DataMember]
        public string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Source points node will have defined.
        /// </summary>
        [DataMember]
        public List<SourcePoint> SourcePoints 
        { 
            get
            {
                if (mSourcePoints == null)
                {
                    mSourcePoints = GetDefaultSourcePoints();
                }
                return mSourcePoints;
            }
            set
            {
                mSourcePoints = value;
            }
        }


        /// <summary>
        /// Whether or not item in state defined by this node should continue after defined amount of time.  
        /// </summary>
        [DataMember]
        public bool HasTimeout 
        { 
            get;
            set; 
        }


        /// <summary>
        /// Indicates if node can be deleted.
        /// Default value = true
        /// </summary>
        [DataMember]
        public bool IsDeletable
        { 
            get; 
            set; 
        }


        /// <summary>
        /// URL to thumbnail image of node.
        /// </summary>
        [DataMember]
        public string ThumbnailImageUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Thumbnail CSS class name of node.
        /// </summary>
        [DataMember]
        public string ThumbnailClass
        {
            get;
            set;
        }


        /// <summary>
        /// URL to icon image of node.
        /// </summary>
        [DataMember]
        public string IconImageUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Icon CSS class name of node.
        /// </summary>
        [DataMember]
        public string IconClass
        {
            get;
            set;
        }


        /// <summary>
        /// Type as string used for better granularity.
        /// </summary>
        [DataMember]
        public string TypeName
        {
            get;
            set;
        }


        /// <summary>
        /// Whether step has target point.
        /// </summary>
        [DataMember]
        public bool HasTargetPoint
        {
            get;
            set;
        }


        /// <summary>
        /// Prefix of resources of given type.
        /// </summary>
        public string TypeResourceStringPrefix
        {
            get;
            set;
        }


        /// <summary>
        /// Whether the node has the explicit timeout source point.
        /// </summary>
        public bool HasTimeoutSourcePoint
        {
            get
            {
                return HasTimeout && 
                    (this.Type != NodeTypeEnum.Action) && 
                    (this.Type != NodeTypeEnum.Multichoice);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public Node() 
        {
            IsDeletable = true;
            HasTargetPoint = true;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Method returning default list of source points.
        /// </summary>
        /// <returns>List of source points</returns>
        protected virtual List<SourcePoint> GetDefaultSourcePoints()
        {
            return new List<SourcePoint>();
        }

        #endregion
    }
}
