using System.Collections.Specialized;
using System.Text;
using System.Web;

using CMS.AspNet.Platform.HttpContext.Extensions;
using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpResponseBase"/> object implementing <see cref="IResponse"/> interface.
    /// </summary>
    internal class ResponseImpl : IResponse
    {
#pragma warning disable BH1004 // 'Response.Cookies' should not be used. Use 'CookieHelper.ResponseCookies' instead.
#pragma warning disable BH1008 // 'Response.Redirect()' should not be used.

        private readonly HttpContextBase mContext;
        private IHttpCookieCollection mCookies;
        private ICache mCache;

        public ResponseImpl(HttpContextBase context)
        {
            mContext = context;
        }


        public IHttpCookieCollection Cookies => mCookies ?? (mCookies = new HttpCookieCollectionImpl(mContext.Response.Cookies));


        public ICache Cache => mCache ?? (mCache = new CachePolicyImpl(mContext.Response.Cache));


        public int StatusCode
        {
            get => mContext.Response.StatusCode;
            set => mContext.Response.StatusCode = value;
        }


        public string RedirectLocation
        {
            get => mContext.Response.RedirectLocation;
            set => mContext.Response.RedirectLocation =value;
        }


        public Encoding HeaderEncoding => mContext.Response.HeaderEncoding;


        public NameValueCollection Headers => mContext.Response.Headers;


        public void AddHeader(string name, string value)
        {
            mContext.Response.AddHeader(name, value);
        }


        public void Clear()
        {
            mContext.Response.Clear();
        }


        public void End()
        {
            mContext.Response.End();
        }


        public void Redirect(string url, bool endResponse)
        {
            mContext.Response.Redirect(url, endResponse);
        }


        public void RedirectPermanent(string url, bool endResponse)
        {
            mContext.Response.RedirectPermanent(url, endResponse);
        }


        public void SetCookie(IHttpCookie cookie)
        {
            mContext.Response.SetCookie(cookie.ToHttpCookie());
        }


        public void Write(string content)
        {
            mContext.Response.Write(content);
        }

#pragma warning restore BH1004 // 'Response.Cookies' should not be used. Use 'CookieHelper.ResponseCookies' instead.
#pragma warning restore BH1008 // 'Response.Redirect()' should not be used.
    }
}
