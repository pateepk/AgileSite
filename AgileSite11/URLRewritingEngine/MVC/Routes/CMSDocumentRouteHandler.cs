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
    /// Route handler for the portal pages
    /// </summary>
    public class CMSDocumentRouteHandler : IRouteHandler
    {
        #region "Variables"

        /// <summary>
        /// Site name for the current handler.
        /// </summary>
        protected string mSiteName = null;

        /// <summary>
        /// Alias path for the current handler
        /// </summary>
        protected string mAliasPath = null;

        /// <summary>
        /// Node ID for the current handler
        /// </summary>
        protected int mNodeId = 0;

        /// <summary>
        /// Culture for the current handler
        /// </summary>
        protected string mCulture = null;

        /// <summary>
        /// Url path
        /// </summary>
        protected string mUrlPath = null;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteName">Site name for matching of the requests</param>
        /// <param name="aliasPath">Alias path of the matching document</param>
        /// <param name="nodeId">Node ID of the matching document</param>
        /// <param name="culture">Culture of the matching document</param>
        /// <param name="urlPath">URL path</param>
        public CMSDocumentRouteHandler(string siteName, string aliasPath, int nodeId, string culture, string urlPath)
        {
            mSiteName = siteName;
            mAliasPath = aliasPath;
            mNodeId = nodeId;
            mCulture = culture;
            mUrlPath = urlPath;
        }


        /// <summary>
        /// Gets the MVC Http handler
        /// </summary>
        /// <param name="requestContext">Request context</param>
        public IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestContext)
        {
            IHttpHandler handler = null;

            PageInfo pageInfo = DocumentContext.CurrentPageInfo;
            if (pageInfo != null)
            {
                // Get the file path
                string path = URLHelper.PortalTemplatePage;

                RequestStatusEnum status = RequestStatusEnum.PathRewritten;
            
                // Get the correct MVC handler for the given page info
                handler = CMSMvcHelper.GetMVCPageHandler(pageInfo, requestContext, ref status);

                // Set current status and request properties
                RequestContext.CurrentStatus = status;

                // Process actions after rewriting
                RequestContext.LogPageHit = true;

                // Set current status and request properties
                URLRewritingContext.CurrentRouteData = requestContext.RouteData;

                // Get the handler for the given path if not provided previously
                if (handler == null)
                {
                    handler = BuildManager.CreateInstanceFromVirtualPath(path, typeof(Page)) as IHttpHandler;
                }
            }

            return handler;
        }

        #endregion
    }
}