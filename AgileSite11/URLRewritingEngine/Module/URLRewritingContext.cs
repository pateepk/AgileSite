using System;
using System.Web;
using System.Web.Routing;

using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Request URL rewriting values
    /// </summary>
    public class URLRewritingContext : AbstractContext<URLRewritingContext>, INotCopyThreadItem
    {
        private RouteData mCurrentRouteData;

        /// <summary>
        /// Gets or sets the value that indicates whether page not found was set
        /// </summary>
        internal static bool PageNotFoundHandled
        {
            get
            {
                return RequestItems.CurrentItems["URLRewritingContextPageNotFoundHandled"] as bool? ?? false;
            }
            set
            {
                RequestItems.CurrentItems["URLRewritingContextPageNotFoundHandled"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the redirect URL for time when the full request context is available
        /// </summary>
        internal static string PlannedRedirectUrl
        {
            get
            {
                return Convert.ToString(RequestItems.CurrentItems["URLRewritingContextPlannedRedirectUrl"]);
            }
            set
            {
                RequestItems.CurrentItems["URLRewritingContextPlannedRedirectUrl"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the current page info source for live site requests
        /// </summary>
        public static PageInfoSource CurrentPageInfoSource
        {
            get
            {
                PageInfo pi = DocumentContext.CurrentPageInfo;
                if (pi != null)
                {
                    return pi.PageResult.PageSource;
                }

                return PageInfoSource.Unknown;
            }
            set
            {
                PageInfo pi = DocumentContext.CurrentPageInfo;
                if (pi != null)
                {
                    pi.PageResult.PageSource = value;
                }
            }
        }

        /// <summary>
        /// Http handler to use within current request. If set, the handler is automatically applied during the PostMapRequestHandler event
        /// </summary>
        public static IHttpHandler HttpHandler
        {
            get
            {
                return RequestItems.CurrentItems["URLRewritingContextHttpHandler"] as IHttpHandler;
            }
            set
            {
                RequestItems.CurrentItems["URLRewritingContextHttpHandler"] = value;
            }
        }


        /// <summary>
        /// Returns current route data
        /// </summary>
        [RegisterProperty]
        public static RouteData CurrentRouteData
        {
            get
            {
                return Current.mCurrentRouteData;
            }
            set
            {
                Current.mCurrentRouteData = value;
            }
        }
    }
}
