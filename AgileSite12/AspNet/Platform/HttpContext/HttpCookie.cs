using System;
using System.Web;

using CMS.Base;
using CMS.Core;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpCookie"/> object implementing <see cref="IHttpCookie"/> interface.
    /// </summary>
    internal class HttpCookieImpl : IHttpCookie
    {
        internal HttpCookie Cookie { get; }


        /// <summary>
        /// Constructor is required for <see cref="ObjectFactory"/>
        /// </summary>
        public HttpCookieImpl()
            : this("")
        {
        }


        public HttpCookieImpl(string name)
        {
            Cookie = new HttpCookie(name);
        }


        internal HttpCookieImpl(HttpCookie cookie)
        {
            Cookie = cookie;
        }


        public string Name
        {
            get => Cookie.Name;
            set => Cookie.Name = value;
        }


        public string Value
        {
            get => Cookie.Value;
            set => Cookie.Value = value;
        }


        public DateTime Expires
        {
            get => Cookie.Expires;
            set => Cookie.Expires = value;
        }


        public bool HttpOnly
        {
            get => Cookie.HttpOnly;
            set => Cookie.HttpOnly = value;
        }


        public string Domain
        {
            get => Cookie.Domain;
            set => Cookie.Domain = value;
        }


        public string Path
        {
            get => Cookie.Path;
            set => Cookie.Path = value;
        }
    }
}
