namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Represents view model for the contact account details field.
    /// </summary>
    public class ContactDetailsAccountViewModel
    {
        /// <summary>
        /// Gets or sets URL leading to the account.
        /// </summary>
        public string Url
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets text representing the account.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the role contact has in account.
        /// </summary>
        public string ContactRole
        {
            get;
            set;
        }
    }
}