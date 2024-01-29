namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a SalesForce picklist entry.
    /// </summary>
    public sealed class PicklistEntry
    {

        #region "Private members"

        private WebServiceClient.PicklistEntry mSource;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the value indicating whether this entry is active.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return mSource.active;
            }
        }

        /// <summary>
        /// Gets the value indicating whether this entry is default.
        /// </summary>
        public bool IsDefault
        {
            get
            {
                return mSource.defaultValue;
            }
        }

        /// <summary>
        /// Gets the label of this entry.
        /// </summary>
        public string Label
        {
            get
            {
                return mSource.label;
            }
        }

        /// <summary>
        /// Gets the value of this entry.
        /// </summary>
        public string Value
        {
            get
            {
                return mSource.value;
            }
        }

        #endregion

        #region "Constructors"

        internal PicklistEntry(WebServiceClient.PicklistEntry source)
        {
            mSource = source;
        }

        #endregion

    }

}