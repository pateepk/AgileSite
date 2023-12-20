using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Authorization handler
    /// </summary>
    public class AuthorizationHandler : SimpleHandler<AuthorizationHandler, AuthorizationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="resourceName">Resource name to authorize</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="authorized">Current (system) authorization status</param>
        public AuthorizationEventArgs StartResourceEvent(UserInfo userInfo, string resourceName, string permissionName, ref bool authorized)
        {
            var e = new AuthorizationEventArgs
                {
                    User = userInfo,
                    ResourceName = resourceName,
                    PermissionName = permissionName,
                    Authorized = authorized
                };

            StartEvent(e);

            authorized = e.Authorized;

            return e;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="className">Class name to authorize</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="authorized">Current (system) authorization status</param>
        public AuthorizationEventArgs StartClassEvent(UserInfo userInfo, string className, string permissionName, ref bool authorized)
        {
            var e = new AuthorizationEventArgs
                {
                    User = userInfo,
                    ClassName = className,
                    PermissionName = permissionName,
                    Authorized = authorized
                };

            StartEvent(e);

            authorized = e.Authorized;

            return e;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="userInfo">User info object</param>
        /// <param name="resourceName">Resource name to authorize</param>
        /// <param name="elementName">UI element name</param>
        /// <param name="authorized">Current (system) authorization status</param>
        public AuthorizationEventArgs StartUIElementEvent(UserInfo userInfo, string resourceName, string elementName, ref bool authorized)
        {
            var e = new AuthorizationEventArgs
                {
                    User = userInfo,
                    ResourceName = resourceName,
                    ElementName = elementName,
                    Authorized = authorized
                };

            StartEvent(e);

            authorized = e.Authorized;

            return e;
        }
    }
}