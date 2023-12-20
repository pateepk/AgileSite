using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

using CMS.DataEngine;

namespace Kentico.Content.Web.Mvc.Attributes
{
    /// <summary>
    /// Checks that the incoming request is authenticated via the virtual context and the user has a given permission for the current page.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.AuthorizeAttribute" />
    internal sealed class CheckPagePermissionsAttribute : AuthorizeAttribute
    {
        private readonly IPageSecurityChecker checker;


        /// <summary>
        /// Gets or sets the permission to check.
        /// </summary>
        private PermissionsEnum Permission { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CheckPagePermissionsAttribute"/> class.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        public CheckPagePermissionsAttribute(PermissionsEnum permission)
            : this(permission, new PageSecurityChecker(new VirtualContextPageRetriever()))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CheckPagePermissionsAttribute" /> class.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <param name="checker">The page security checker.</param>
        /// <exception cref="ArgumentNullException"><paramref name="checker"/> is null.</exception>
        internal CheckPagePermissionsAttribute(PermissionsEnum permission, IPageSecurityChecker checker)
        {
            this.checker = checker ?? throw new ArgumentNullException(nameof(checker));

            Permission = permission;
        }


        /// <summary>
        /// Provides an entry point for custom authorization checks.
        /// </summary>
        /// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns>
        /// true if the user is authorized; otherwise, false.
        /// </returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return checker.Check(Permission);
        }


        /// <summary>
        /// Processes HTTP requests that fail authorization.
        /// </summary>
        /// <param name="filterContext">Encapsulates the information for using <see cref="T:System.Web.Mvc.AuthorizeAttribute" />. The <paramref name="filterContext" /> object contains the controller, HTTP context, request context, action result, and route data.</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
    }
}