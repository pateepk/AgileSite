using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpApplication"/> object implementing <see cref="IHttpApplication"/> interface.
    /// </summary>
    internal class HttpApplicationImpl : IHttpApplication
    {
        private readonly HttpContextBase mContext;


        public HttpApplicationImpl(HttpContextBase context)
        {
            mContext = context;
        }


        public void CompleteRequest()
        {
            mContext.ApplicationInstance.CompleteRequest();
        }
    }
}
