using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using CMS.Membership;

namespace CMS.WebApi
{
    /// <summary>
    /// Restrict access for authorized user with given resource name and permission.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class IsAuthorizedPerResourceAttribute : AuthorizationFilterAttribute
    {
        private const string DEFAULT_ERROR_MESSAGE = "You are not authorized per resource {0}";

        private readonly string mResourceName;
        private readonly string mPermissionName;
        private readonly string mMessage;


        /// <summary>
        /// Creates permission attribute. Permission will be checked similarly to <see cref="UserInfo.IsAuthorizedPerResource(string, string)"/>.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name to check</param>
        /// <param name="message">Error message, if null or empty <see cref="DEFAULT_ERROR_MESSAGE"/> is used</param>
        public IsAuthorizedPerResourceAttribute(string resourceName, string permissionName, string message = "")
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentException("[IsAuthorizedPerResourceAttribute.Constructor]: Cannot be null or empty", "resourceName");
            }

            if (string.IsNullOrEmpty(permissionName))
            {
                throw new ArgumentException("[IsAuthorizedPerResourceAttribute.Constructor]: Cannot be null or empty", "permissionName");
            }

            mResourceName = resourceName;
            mPermissionName = permissionName;
            mMessage = string.IsNullOrEmpty(message) ? DEFAULT_ERROR_MESSAGE : message;
        }


        /// <summary>
        /// Restrict access for authorized user with given resource name and permission.
        /// </summary>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (MembershipContext.AuthenticatedUser == null || !IsAuthorized(MembershipContext.AuthenticatedUser))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, string.Format(mMessage, mResourceName));
            }
        }


        private bool IsAuthorized(UserInfo authenticatedUser)
        {
            return authenticatedUser.IsAuthorizedPerResource(mResourceName, mPermissionName);
        }
    }
}