using CMS.Membership;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Represents view model for the contact component.
    /// </summary>
    public class ContactProfileViewModel
    {
        /// <summary>
        /// Gets or sets contact ID.
        /// </summary>
        public int ContactID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets full name of the contact.
        /// </summary>
        public string ContactName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets email of the contact.
        /// </summary>
        public string ContactEmail
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value specifying the contact type based on license.
        /// </summary>
        public ContactTypeEnum ContactType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value specifying the contact address.
        /// </summary>
        public string ContactAddress
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value specifying the contact age.
        /// </summary>
        public int? ContactAge
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value specifying the contact gender.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public UserGenderEnum ContactGender
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value specifying the contact notes.
        /// </summary>
        public string ContactNotes
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets url that leads to contacts edit page.
        /// </summary>
        public string EditUrl
        {
            get;
            set;
        }

        
        /// <summary>
        /// Gets or sets whether the contact is in relation with some user.
        /// </summary>
        public bool IsCustomer
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether the contact is in relation with some user.
        /// </summary>
        public bool IsUser
        {
            get;
            set;
        }
    }
}