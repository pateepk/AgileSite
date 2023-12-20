using System;
using System.Web;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Ensures redirection to the live URL of page based on culture name (for example en-us) and node ID.
    /// </summary>
    public class PresentationUrlRedirect : IHttpHandler
    {
        #region "Properties"

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

        #endregion


        #region "Handler methods"

        /// <summary>
        /// Ensures redirection to the live URL of node based on provided culture and node ID.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            // Validate the request hash
            var hashSettings = new HashSettings("")
            {
                Redirect = false
            };

            if (!QueryHelper.ValidateHash("hash", null, hashSettings))
            {
                RequestHelper.Respond403();
            }
            
            var cultureName = QueryHelper.GetString("culturename", "");
            var nodeID = QueryHelper.GetInteger("nodeid", 0);
            var nodeUrl = "";

            if (String.IsNullOrEmpty(cultureName))
            {
                RequestHelper.Respond400(ResHelper.GetString("content.presentationurlhandler.invalidculture"));
            }

            if (nodeID < 1)
            {
                RequestHelper.Respond400(ResHelper.GetString("content.presentationurlhandler.invalidnodeid"));
            }

            var tree = new TreeProvider();
            // Include coupled data and returns latest (edited) version
            var node = DocumentHelper.GetDocument(nodeID, cultureName, tree);

            if (node == null)
            {
                RequestHelper.Respond404();
            }

            try
            {
                nodeUrl = DocumentURLProvider.GetPresentationUrl(node);
            }
            catch (Exception exception)
            {
                // Log the exception
                EventLogProvider.LogException("PresentationUrlRedirect", "GetPresentationUrl", exception);
                RequestHelper.Respond500();
            }

            nodeUrl = URLHelper.AppendQuery(nodeUrl, RequestContext.CurrentQueryString);
            nodeUrl = RemoveHandlerQueryParameters(nodeUrl);

            context.Response.ContentType = "text/plain";
            URLHelper.ResponseRedirect(nodeUrl);
        }


        /// <summary>
        /// Removes query string parameters used by handler.
        /// </summary>
        private string RemoveHandlerQueryParameters(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return "";
            }

            return URLHelper.RemoveParametersFromUrl(url, "culturename", "hash", "nodeid");
        }

        #endregion
    }
}