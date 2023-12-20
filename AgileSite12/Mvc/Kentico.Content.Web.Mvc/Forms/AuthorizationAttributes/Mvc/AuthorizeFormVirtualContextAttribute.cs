using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.OnlineForms;

using Kentico.Forms.Web.Mvc;

namespace Kentico.Forms.Web.Attributes.Mvc
{
    /// <summary>
    /// Specifies that access to an MVC controller or action method is restricted to users obtained via virtual context, who meet the authorization requirement.
    /// Use when user is obtained via the virtual context. Omits Owin authorization.
    /// </summary>
    internal sealed class AuthorizeFormVirtualContextAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Gets name of the resource which needs access authorization.
        /// </summary>
        /// <seealso cref="ResourceInfo"/>
        public string ResourceName { get; }


        /// <summary>
        /// Gets permission name needed for access within given <see cref="ResourceName"/>.
        /// </summary>
        /// <seealso cref="PermissionNameInfo"/>
        public string PermissionName { get; }


        /// <summary>
        /// Initializes new instance of <see cref="AuthorizeFormVirtualContextAttribute"/> for authorizing requests to an MVC controller.
        /// </summary>
        /// <param name="resourceName">Name of the resource which needs access authorization.</param>
        /// <param name="permissionName">Permission name needed for access within given <paramref name="resourceName"/>.</param>
        /// <seealso cref="ResourceInfo"/>
        /// <seealso cref="PermissionNameInfo"/>
        public AuthorizeFormVirtualContextAttribute(string resourceName, string permissionName)
        {
            if (String.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(resourceName));
            }

            if (String.IsNullOrEmpty(permissionName))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(permissionName));
            }

            ResourceName = resourceName;
            PermissionName = permissionName;
        }


        /// <summary>
        /// Entry point for custom authorization check.
        /// </summary>
        /// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns><c>true</c> if the user is authorized via virtual context; otherwise, <c>false</c>.</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!VirtualContext.IsInitialized)
            {
                throw new InvalidOperationException("Virtual context is not initialized");
            }

            var isUserAuthorizedPerResource = UserInfoProvider.GetUserInfo(RequestContext.UserName)?.IsAuthorizedPerResource(ResourceName, PermissionName) ?? false;
            
            return isUserAuthorizedPerResource && IsUserAllowedToManageForm(httpContext);
        }


        /// <summary>
        /// Processes HTTP requests that fail authorization.
        /// </summary>
        /// <param name="filterContext">
        /// Encapsulates the information for using System.Web.Mvc.AuthorizeAttribute.
        /// The filterContext object contains the controller, HTTP context, request context, action result, and route data.
        /// </param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var message = ResHelper.GetStringFormat("general.permissionresource", PermissionName, ResourceName);

            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, message);
        }


        private static bool IsUserAllowedToManageForm(HttpContextBase httpContext)
        {
            var formId = 0;
            if (httpContext.Request.RequestContext.RouteData.Values.ContainsKey(FormBuilderConstants.ROUTE_NAME))
            {
                formId = ValidationHelper.GetInteger(httpContext.Request.RequestContext.RouteData.Values[FormBuilderConstants.ROUTE_NAME], 0);
            }

            if (formId == 0 && httpContext.Request.Form.Get(FormBuilderConstants.ROUTE_NAME) != null)
            {
                int.TryParse(httpContext.Request.Form.Get(FormBuilderConstants.ROUTE_NAME), out formId);
            }

            var form = BizFormInfoProvider.GetBizFormInfo(formId);
            if (form == null)
            {
                return true;
            }

            var currentUser = Service.Resolve<IAuthenticationService>().CurrentUser;
            var currentSite = Service.Resolve<ISiteService>().CurrentSite;

            return form.IsFormAllowedForUser(currentUser?.UserName, currentSite?.SiteName);
        }
    }
}