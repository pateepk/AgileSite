namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Represents view model for the contact details component.
    /// </summary>
    public class ContactDetailsViewModel
    {
        /// <summary>
        /// Gets or sets localized caption of the detail item.
        /// </summary>
        public string FieldCaption
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets name of the detail item.
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value of the detail item.
        /// </summary>
        public object FieldValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets type of the <see cref="FieldValue"/>.
        /// </summary>
        public string FieldType
        {
            get;
            set;
        }
    }
}
