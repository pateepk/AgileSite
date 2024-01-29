namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for location associated with a number of contacts from the location.
    /// </summary>
    public class ContactsGroupedByLocationViewModel
    {
        /// <summary>
        /// Code of the location.
        /// </summary>
        public string LocationKey
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts from the location.
        /// </summary>
        public int NumberOfContacts
        {
            get;
            set;
        }
    }
}
