using System.Collections;
using System.Security.Principal;
using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpContextBase"/> object implementing <see cref="IHttpContext"/> interface.
    /// </summary>
    internal class HttpContextImpl : IHttpContext
    {
        private readonly HttpContextBase mContext;
        private IRequest mRequest;
        private IResponse mResponse;
        private IServer mServer;
        private ISession mSession;
        private IHttpApplication mHttpApplication;


        public HttpContextImpl(HttpContextBase context)
        {
            mContext = context;
        }


        public IDictionary Items => mContext.Items;


        public IRequest Request
        {
            get
            {

                if (mRequest == null)
                {
                    try
                    {
                        if (mContext.Request != null)
                        {
                            mRequest = new RequestImpl(mContext);
                        }
                    }
                    catch (HttpException)
                    {
                        // Ignore exception thrown when accessing to HttpContext.Request property in unsupported life-cycle stage (Application_Start)
                    }
                }

                return mRequest;
            }
        }


        public IResponse Response
        {
            get
            {
                if (mResponse == null)
                {
                    try
                    {
                        if (mContext.Response != null)
                        {
                            mResponse = new ResponseImpl(mContext);
                        }
                    }
                    catch (HttpException)
                    {
                        // Ignore exception thrown when accessing to HttpContext.Response property in unsupported life-cycle stage (Application_Start)
                    }
                }

                return mResponse;
            }
        }


        public IServer Server
        {
            get
            {
                if (mServer == null && mContext.Server != null)
                {
                    mServer = new ServerImpl(mContext);
                }

                return mServer;
            }
        }


        public ISession Session
        {
            get
            {
                if (mSession == null && mContext.Session != null)
                {
                    mSession = new SessionImpl(mContext);
                }

                return mSession;
            }
        }


        public IHttpApplication ApplicationInstance
        {
            get
            {
                if (mHttpApplication == null && mContext.ApplicationInstance != null)
                {
                    mHttpApplication = new HttpApplicationImpl(mContext);
                }

                return mHttpApplication;
            }
        }


        public IPrincipal User => mContext.User;
    }
}
