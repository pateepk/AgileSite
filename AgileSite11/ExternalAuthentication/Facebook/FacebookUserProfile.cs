using System;
using System.ComponentModel.DataAnnotations;

using CMS.Membership;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Represents partial information from Facebook user profile.
    /// </summary>
    /// <remarks>
    /// This class is intended for updating users with information from their Facebook profile.
    /// As a result it contains only a a limited number of properties available.
    /// </remarks>
    public class FacebookUserProfile
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets user's full name.
        /// </summary>
        [Display(Name = "{$fb.profile.facebookname$}")]
        [Facebook("name")]
        public string FacebookName
        {
            get;
            set;
        } 



        /// <summary>
        /// Gets or sets user's first name.
        /// </summary>
        [Display(Name = "{$fb.profile.firstname$}")]
        [Facebook("first_name")]
        public string FirstName
        {
            get;
            set;
        } 


        /// <summary>
        /// Gets or sets user's middle name.
        /// </summary>
        [Display(Name = "{$fb.profile.middlename$}")]
        [Facebook("middle_name")]
        public string MiddleName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets user's last name.
        /// </summary>
        [Display(Name = "{$fb.profile.lastname$}")]
        [Facebook("last_name")]
        public string LastName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets user's gender.
        /// </summary>
        [Display(Name = "{$fb.profile.gender$}")]
        [Facebook("gender")]
        public UserGenderEnum Gender
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets user's birthday.
        /// </summary>
        [Display(Name = "{$fb.profile.birthday$}")]
        [Facebook("birthday", PermissionScopeName = "user_birthday")]
        public DateTime? Birthday
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets user's e-mail address.
        /// </summary>
        [Display(Name = "{$fb.profile.email$}")]
        [Facebook("email", PermissionScopeName = "email")]
        public string Email
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets user's current city.
        /// </summary>
        [Display(Name = "{$fb.profile.city$}")]
        [Facebook("location", PermissionScopeName = "user_location")]
        public string City
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets user's website.
        /// </summary>
        [Display(Name = "{$fb.profile.website$}")]
        [Facebook("website", PermissionScopeName = "user_website")]
        public string Website
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if user's Facebook account is verified.
        /// </summary>
        [Display(Name = "{$fb.profile.isverified$}")]
        [Facebook("verified")]
        public bool IsVerified
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets user's locale containing the ISO Language Code and ISO Country Code.
        /// </summary>
        [Display(Name = "{$fb.profile.locale$}")]
        [Facebook("locale", ValueConverterType=typeof(EntityCultureAttributeValueConverter))]
        public string Locale
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the link to the user's Facebook profile page.
        /// </summary>
        [Display(Name = "{$fb.profile.linktoprofile$}")]
        [Facebook("link")]
        public string LinkToFacebookProfile
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the timestamp of the last profile cahnge.
        /// </summary>
        [Display(Name = "{$fb.profile.profilechange$}")]
        [Facebook("updated_time")]
        public DateTime? LastProfileChange
        {
            get;
            set;
        }

        #endregion
    }
}