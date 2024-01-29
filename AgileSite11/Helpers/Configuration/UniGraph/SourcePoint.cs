using System.Runtime.Serialization;

namespace CMS.Helpers.UniGraphConfig
{
    /// <summary>
    /// Configuration class for JSON serialization.
    /// </summary>
    [DataContract(Name="SourcePoint", Namespace="CMS.Helpers.UniGraphConfig")]
    public class SourcePoint
    {
        #region "Properties"

        /// <summary>
        /// ID of source point (not rendered to HTML).
        /// </summary>
        [DataMember]
        public string ID 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Type of source point.
        /// </summary>
        [DataMember]
        public SourcePointTypeEnum Type 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Label of source point.
        /// </summary>
        [DataMember]
        public string Label 
        { 
            get; 
            set;
        }


        /// <summary>
        /// Whether or not is label of source point localized.
        /// </summary>
        [DataMember]
        public bool IsLabelLocalized 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Label of source point.
        /// </summary>
        [DataMember]
        public string Tooltip
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public SourcePoint()
        {
        }

        #endregion
    }
}
