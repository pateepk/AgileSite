using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Authentication event arguments
    /// </summary>
    public class AuthenticationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Authenticated user.
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Password.
        /// </summary>
        public string Password
        {
            get;
            set;
        }


        /// <summary>
        /// User name.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }


        /// <summary>
        /// Passcdoe for multifactor authentication.
        /// </summary>
        public string Passcode
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the site to which the user tries to autenticate.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }
    }
}