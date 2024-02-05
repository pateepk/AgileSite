namespace CMS.SalesForce
{

    /// <summary>
    /// Represents an entity attribute.
    /// </summary>
    public sealed class EntityAttribute
    {

        #region "Private members"

        private EntityAttributeModel mModel;
        private object mValue;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the attribute model.
        /// </summary>
        public EntityAttributeModel Model
        {
            get
            {
                return mModel;
            }
        }

        /// <summary>
        /// Gets or sets the attribute value.
        /// </summary>
        public object Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (value == null)
                {
                    mValue = null;
                }
                else
                {
                    mValue = mModel.ConvertValue(value);
                }
            }
        }

        #endregion

        #region "Constructors"

        internal EntityAttribute(EntityAttributeModel model)
        {
            mModel = model;
        }

        #endregion

    }

}