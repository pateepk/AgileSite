using System;
using System.Collections.Specialized;
using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpRequestBase"/> object implementing <see cref="IRequest"/> interface.
    /// </summary>
    internal class RequestImpl : IRequest
    {
#pragma warning disable BH1003 // 'Request.Cookies' should not be used. Use 'CookieHelper.RequestCookies' instead.
#pragma warning disable BH1005 // 'Request.UserHostAddress' should not be used. Use 'RequestContext.UserHostAddress' instead.
#pragma warning disable BH1006 // 'Request.Url' should not be used. Use 'RequestContext.URL' instead.

        private readonly HttpContextBase mContext;
        private IRequestContext mRequestContext;
        private IHttpCookieCollection mCookies;
        private IBrowser mBrowser;


        public RequestImpl(HttpContextBase context)
        {
            mContext = context;
        }


        public IRequestContext RequestContext
        {
            get
            {
                if (mRequestContext == null && mContext.Request.RequestContext != null)
                {
                    mRequestContext = new RequestContextImpl(mContext);
                }

                return mRequestContext;
            }
        }


        public IHttpCookieCollection Cookies
        {
            get
            {
                if (mCookies == null && mContext.Request.Cookies != null)
                {
                    mCookies = new HttpCookieCollectionImpl(mContext.Request.Cookies);
                }

                return mCookies;
            }
        }

        public IBrowser Browser => mBrowser ?? (mBrowser = new BrowserImpl(mContext.Request.Browser));


        public string RawUrl => mContext.Request.RawUrl;


        public Uri Url => mContext.Request.Url;


        public NameValueCollection Headers => mContext.Request.Headers;


        public NameValueCollection ServerVariables => mContext.Request.ServerVariables;


        public NameValueCollection QueryString => mContext.Request.QueryString;


        public NameValueCollection Form => mContext.Request.Form;


        public string HttpMethod => mContext.Request.HttpMethod;


        public string UserAgent => mContext.Request.UserAgent;


        public Uri UrlReferrer => mContext.Request.UrlReferrer;


        public string[] UserLanguages => mContext.Request.UserLanguages;


        public string AppRelativeCurrentExecutionFilePath => mContext.Request.AppRelativeCurrentExecutionFilePath;


        public bool IsSecureConnection => mContext.Request.IsSecureConnection;


        public string UserHostAddress => mContext.Request.UserHostAddress;


        public string PhysicalApplicationPath => mContext.Request.PhysicalApplicationPath;

#pragma warning restore BH1003 // 'Request.Cookies' should not be used. Use 'CookieHelper.RequestCookies' instead.
#pragma warning restore BH1005 // 'Request.UserHostAddress' should not be used. Use 'RequestContext.UserHostAddress' instead.
#pragma warning restore BH1006 // 'Request.Url' should not be used. Use 'RequestContext.URL' instead.
    }
}
