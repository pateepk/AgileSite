namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Represents an item of an entity mapping.
    /// </summary>
    public sealed class EntityMappingItem
    {
        #region "Variables"

        /// <summary>
        /// The name of the entity attribute that is the target of this mapping item.
        /// </summary>
        private string mAttributeName;


        /// <summary>
        /// The name of the form field that is the source of this mapping item.
        /// </summary>
        private string mFieldName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the name of the entity attribute that is the target of this mapping item.
        /// </summary>
        public string AttributeName
        {
            get
            {
                return mAttributeName;
            }
        }


        /// <summary>
        /// Gets the name of the form field that is the source of this mapping item.
        /// </summary>
        public string FieldName
        {
            get
            {
                return mFieldName;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityMappingItem class with the specified entity attribute and form field name.
        /// </summary>
        /// <param name="attributeName">The name of the entity attribute of this mapping item.</param>
        /// <param name="fieldName">The name of the form field of this mapping item.</param>
        internal EntityMappingItem(string attributeName, string fieldName)
        {
            mAttributeName = attributeName;
            mFieldName = fieldName;
        }

        #endregion
    }

}