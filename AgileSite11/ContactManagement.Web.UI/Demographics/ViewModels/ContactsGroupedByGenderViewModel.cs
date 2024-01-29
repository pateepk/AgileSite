using CMS.Membership;

using Newtonsoft.Json;

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for number of contacts in <see cref="Gender"/>
    /// </summary>
    public class ContactsGroupedByGenderViewModel
    {
        /// <summary>
        /// Contacts' gender.
        /// </summary>
        [JsonConverter(typeof(LocalizedGenderJsonConverter))]
        public UserGenderEnum Gender
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts in <see cref="Gender"/>.
        /// </summary>
        public int NumberOfContacts
        {
            get;
            set;
        }
    }
}
