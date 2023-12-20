using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.OnlineForms;

using Kentico.Builder.Web.Mvc;
using Kentico.Forms.Web.Mvc;

namespace Kentico.Forms.Web.Attributes.Http
{
    /// <summary>
    /// Specifies that access to an API controller or action method is restricted to users obtained via virtual context, who meet the authorization requirement.
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
        /// Initializes new instance of <see cref="AuthorizeFormVirtualContextAttribute"/> for authorizing requests to an API controller.
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
        /// Indicates whether the specified control is authorized.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <returns><c>true</c> if the control is authorized via virtual context; otherwise, <c>false</c>.</returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!VirtualContext.IsInitialized)
            {
                throw new InvalidOperationException("Virtual context is not initialized");
            }

            var isUserAuthorizedPerResource = UserInfoProvider.GetUserInfo(RequestContext.UserName)?.IsAuthorizedPerResource(ResourceName, PermissionName) ?? false;
            
            return isUserAuthorizedPerResource && IsUserAllowedToManageForm(actionContext);
        }


        /// <summary>
        /// Processes requests that fail authorization.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var message = ResHelper.GetStringFormat("general.permissionresource", PermissionName, ResourceName);

            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
        }


        private static bool IsUserAllowedToManageForm(HttpActionContext actionContext)
        {
            var formId = 0;
            if (actionContext.ControllerContext.RouteData.Values.ContainsKey(FormBuilderConstants.ROUTE_NAME))
            {
                formId = ValidationHelper.GetInteger(actionContext.ControllerContext.RouteData.Values[FormBuilderConstants.ROUTE_NAME], 0);
            }

            if (formId == 0 && actionContext.ControllerContext.Request.Headers.Contains(BuilderConstants.INSTANCE_HEADER_NAME))
            {
                int.TryParse(actionContext.ControllerContext.Request.Headers.GetValues(BuilderConstants.INSTANCE_HEADER_NAME).FirstOrDefault(), out formId);
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