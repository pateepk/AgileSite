using Newtonsoft.Json;

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for number of contacts in <see cref="Category"/>
    /// </summary>
    public class ContactsGroupedByAgeViewModel
    {
        /// <summary>
        /// Gets or sets age category (25–34, 34–44 etc.)
        /// </summary>
        [JsonConverter(typeof(LocalizedAgeCategoryJsonConverter))]
        public AgeCategoryEnum Category
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts in <see cref="Category"/>.
        /// </summary>
        public int NumberOfContacts
        {
            get;
            set;
        }
    }
}