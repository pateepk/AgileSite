using System.Web.SessionState;
using System.Web;

using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Base.Web.UI;

namespace CMS.Membership.Web.UI
{
    class SignOutRequestHandler : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the System.Web.IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements
        ///     the System.Web.IHttpHandler interface.
        /// </summary>
        /// <param name="context">An System.Web.HttpContext object that provides references to the intrinsic
        ///     server objects (for example, Request, Response, Session, and Server) used
        ///     to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            ProcessSignOutRequest(context, QueryHelper.GetString("redirecturl", null));
            
            URLHelper.Redirect(UIHelper.GetSignOutUrl(SiteContext.CurrentSite));

            RequestHelper.CompleteRequest();
        }


        /// <summary>
        /// Handles SignOut request emerged from Javascript SignOut.
        /// Returns a URL to which user should be redirected after SignOut.
        /// If null is returned, no SignOut has been performed and no redirection is required.
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="redirectUrl">URL to which signout should redirect</param>
        private void ProcessSignOutRequest(HttpContext context, string redirectUrl)
        {
            if ((QueryHelper.GetInteger("signout", 0) > 0) && QueryHelper.ValidateHash("hash", "aliaspath"))
            {
                // Prevent browser form keeping sensitive information
                context.Response.Cache.SetNoStore();

                // Sign out from CMS
                AuthenticationHelper.SignOut(redirectUrl);
            }
        }
    }
}
