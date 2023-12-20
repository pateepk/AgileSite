using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="System.Web.Routing.RequestContext"/> object implementing <see cref="IRequestContext"/> interface.
    /// </summary>
    internal class RequestContextImpl : IRequestContext
    {
        private readonly HttpContextBase mContext;


        public RequestContextImpl(HttpContextBase context)
        {
            mContext = context;
        }


        public IRouteData RouteData
        {
            get
            {
                if (mContext.Request?.RequestContext.RouteData != null)
                {
                    return new RouteDataImpl(mContext.Request.RequestContext.RouteData);
                }

                return null;
            }
        }
    }
}
