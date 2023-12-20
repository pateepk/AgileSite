using System;
using System.Runtime.Serialization;

namespace CMS.ExternalAuthentication.LinkedIn
{
    /// <summary>
    /// Represents a LinkedIn profile according to the LinkedIn API.
    /// </summary>
    [Serializable, DataContract]
    public class LinkedInProfile
    {
        /// <summary>
        /// The unique identifier for the given member.
        /// </summary>
        [DataMember(Name = "id", IsRequired = true)]
        public string Id
        {
            get;
            set;
        }


        /// <summary>
        /// Localized first name of the member.
        /// </summary>
        [DataMember(Name = "localizedFirstName")]
        public string LocalizedFirstName
        {
            get;
            set;
        }


        /// <summary>
        /// Localized last name of the member.
        /// </summary>
        [DataMember(Name = "localizedLastName")]
        public string LocalizedLastName
        {
            get;
            set;
        }


        /// <summary>
        /// Birth date of the member
        /// </summary>
        [DataMember(Name = "birthDate")]
        public LinkedInDateObject BirthDate
        {
            get;
            set;
        }
    }
}
