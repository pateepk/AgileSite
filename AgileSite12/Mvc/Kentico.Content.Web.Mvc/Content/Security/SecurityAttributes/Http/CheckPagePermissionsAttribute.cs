using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

using CMS.DataEngine;

namespace Kentico.Content.Web.Mvc.HttpAttributes
{
    /// <summary>
    /// Checks that the incoming request is authenticated via the virtual context and the user has a given permission for the current page.
    /// </summary>
    /// <seealso cref="System.Web.Http.AuthorizeAttribute" />
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
        /// Indicates whether the specified control is authorized.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return checker.Check(Permission);
        }


        /// <summary>
        /// Processes requests that fail authorization.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Forbidden: Access is denied.");
        }
    }
}