namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Read-only version of the form engine user control
    /// </summary>
    public abstract class ReadOnlyFormEngineUserControl : FormEngineUserControl
    {
        /// <summary>
        /// Gets or sets field value. You need to override this method to make the control work properly with the form.
        /// </summary>
        public override object Value
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Returns true if the control has value, if false, the value from the control should not be used within the form to update the data
        /// </summary>
        public override bool HasValue
        {
            get
            {
                return false;
            }
        }
    }
}
