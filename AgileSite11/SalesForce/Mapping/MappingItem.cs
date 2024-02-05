namespace CMS.SalesForce
{

    /// <summary>
    /// Represents an item of the mapping of CMS objects to SalesForce entities.
    /// </summary>
    public sealed class MappingItem
    {

        #region "Private members"

        private string mAttributeName;
        private string mAttributeLabel;
        private string mSourceName;
        private string mSourceLabel;
        private MappingItemSourceTypeEnum mSourceType;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the name of the SalesForce entity attribute.
        /// </summary>
        public string AttributeName
        {
            get
            {
                return mAttributeName;
            }
        }

        /// <summary>
        /// Gets the label of the SalesForce entity attribute.
        /// </summary>
        public string AttributeLabel
        {
            get
            {
                return mAttributeLabel;
            }
        }

        /// <summary>
        /// Gets the name of the mapping item source.
        /// </summary>
        public string SourceName
        {
            get
            {
                return mSourceName;
            }
        }

        /// <summary>
        /// Gets the label of the mapping item source.
        /// </summary>
        public string SourceLabel
        {
            get
            {
                return mSourceLabel;
            }
        }

        /// <summary>
        /// Gets the type of the mapping item source.
        /// </summary>
        public MappingItemSourceTypeEnum SourceType
        {
            get
            {
                return mSourceType;
            }
        }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the MappingItem class.
        /// </summary>
        /// <param name="attributeModel">The SalesForce entity attribute model.</param>
        /// <param name="sourceName">The name of the mapping item source.</param>
        /// <param name="sourceLabel">The label of the mapping item source.</param>
        /// <param name="sourceType">The type of the mapping item source.</param>
        public MappingItem(EntityAttributeModel attributeModel, string sourceName, string sourceLabel, MappingItemSourceTypeEnum sourceType)
        {
            mAttributeName = attributeModel.Name;
            mAttributeLabel = attributeModel.Label;
            mSourceName = sourceName;
            mSourceLabel = sourceLabel;
            mSourceType = sourceType;
        }

        /// <summary>
        /// Initializes a new instance of the MappingItem class.
        /// </summary>
        /// <param name="attributeName">The name of the SalesForce entity attribute.</param>
        /// <param name="attributeLabel">The label of the SalesForce entity attribute.</param>
        /// <param name="sourceName">The name of the mapping item source.</param>
        /// <param name="sourceLabel">The label of the mapping item source.</param>
        /// <param name="sourceType">The type of the mapping item source.</param>
        public MappingItem(string attributeName, string attributeLabel, string sourceName, string sourceLabel, MappingItemSourceTypeEnum sourceType)
        {
            mAttributeName = attributeName;
            mAttributeLabel = attributeLabel;
            mSourceName = sourceName;
            mSourceLabel = sourceLabel;
            mSourceType = sourceType;
        }

        #endregion

    }

}