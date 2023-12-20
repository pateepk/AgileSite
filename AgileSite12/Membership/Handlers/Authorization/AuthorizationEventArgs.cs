using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Authorization event arguments
    /// </summary>
    public class AuthorizationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// User to check
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Resource name
        /// </summary>
        public string ResourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName
        {
            get;
            set;
        }


        /// <summary>
        /// Permission name
        /// </summary>
        public string PermissionName
        {
            get;
            set;
        }


        /// <summary>
        /// UI element name
        /// </summary>
        public string ElementName
        {
            get;
            set;
        }


        /// <summary>
        /// Authorization result
        /// </summary>
        public bool Authorized
        {
            get;
            set;
        }
    }
}