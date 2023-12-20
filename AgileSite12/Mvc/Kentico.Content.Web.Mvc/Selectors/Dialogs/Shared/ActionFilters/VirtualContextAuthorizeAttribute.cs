using System;
using System.Web;
using System.Web.Mvc;

using CMS.Helpers;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Action filter attribute to check initialization of virtual context.
    /// </summary>
    internal sealed class VirtualContextAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Provides an entry point for custom authorization checks.
        /// </summary>
        /// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns>
        /// <returns><c>true</c> if the user is authorized via virtual context; otherwise, <c>false</c>.</returns>
        /// </returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!VirtualContext.IsInitialized)
            {
                throw new InvalidOperationException("Virtual context is not initialized");
            }

            return true;
        }
    }
}
