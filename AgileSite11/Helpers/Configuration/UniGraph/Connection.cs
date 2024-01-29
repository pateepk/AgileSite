using System.Runtime.Serialization;

namespace CMS.Helpers.UniGraphConfig
{
    /// <summary>
    /// Configuration class for JSON serialization.
    /// </summary>
    [DataContract(Name="Connection", Namespace="CMS.Helpers.UniGraphConfig")]
    public class Connection
    {
        #region "Properties"

        /// <summary>
        /// ID of connection (not rendered to HTML).
        /// </summary>
        [DataMember]
        public string ID 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// ID of source point from which connection starts.
        /// </summary>
        [DataMember]
        public string SourcePointID 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// ID of HTML element representing node from which connection starts.
        /// </summary>
        [DataMember]
        public string SourceNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// ID of HTML element representing node to which connection leads.
        /// </summary>
        [DataMember]
        public string TargetNodeID 
        { 
            get; 
            set;
        }

        #endregion
    }
}
