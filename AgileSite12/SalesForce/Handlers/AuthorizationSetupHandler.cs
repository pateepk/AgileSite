using System;
using System.Linq;
using System.Text;
using System.Web;

namespace CMS.SalesForce
{
    /// <summary>
    /// Provides JSON-P service for SalesForce authorization process.
    /// </summary>
    public class AuthorizationSetupHandler : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the System.Web.IHttpHandler interface.
        /// </summary>
        /// <param name="context">An System.Web.HttpContext object that provides references to the intrinsic server objects used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            string content = String.Format("{0}('{{enabled:true}}');", context.Request.Params["callback"]);
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(content);
        }

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

    }
}
