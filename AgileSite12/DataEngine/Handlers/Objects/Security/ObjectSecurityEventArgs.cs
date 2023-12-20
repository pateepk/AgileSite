using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectSecurityEventArgs : ObjectSecurityEventArgs<BaseInfo>
    {
    }


    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectSecurityEventArgs<TObject> : ObjectEventArgs<TObject>
    {
        /// <summary>
        /// Result of the security check. If true, the security check is allowed
        /// </summary>
        public AuthorizationResultEnum Result
        {
            get;
            set;
        }


        /// <summary>
        /// User for which the permissions is checked
        /// </summary>
        public IUserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Site name for which the security check is performed. Doesn't necessarily have to be the same as the object site name.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }

        
        /// <summary>
        /// Permission to check
        /// </summary>
        public PermissionsEnum Permission
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectSecurityEventArgs()
        {
            Result = AuthorizationResultEnum.Insignificant;
        }
    }
}