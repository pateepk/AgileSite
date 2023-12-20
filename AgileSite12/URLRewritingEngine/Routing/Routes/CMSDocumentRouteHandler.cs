using System;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;

using CMS.DocumentEngine;
using CMS.Helpers;

using RequestContext = CMS.Helpers.RequestContext;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Route handler for the portal pages.
    /// </summary>
    internal sealed class CMSDocumentRouteHandler : IRouteHandler
    {
        /// <summary>
        /// Gets the MVC Http handler
        /// </summary>
        /// <param name="requestContext">Request context</param>
        public IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestContext)
        {
            var pageInfo = DocumentContext.CurrentPageInfo;
            if (pageInfo == null)
            {
                throw new InvalidOperationException("Missing current page info to handle route request.");
            }

            RequestStatusEnum status = RequestStatusEnum.PathRewritten;
            
            // Set current status and request properties
            RequestContext.CurrentStatus = status;

            // Process actions after rewriting
            RequestContext.LogPageHit = true;

            // Set current status and request properties
            URLRewritingContext.CurrentRouteData = requestContext.RouteData;

            // Get the handler for the given path if not provided previously
            var handler = BuildManager.CreateInstanceFromVirtualPath(URLHelper.PortalTemplatePage, typeof(Page)) as IHttpHandler;
            if (handler == null)
            {
                throw new InvalidOperationException("Unable to retrieve handler for portal page route configuration.");
            }

            return handler;
        }
    }
}