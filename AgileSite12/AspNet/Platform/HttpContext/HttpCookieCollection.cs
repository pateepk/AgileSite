using System;
using System.Collections;
using System.Web;

using CMS.AspNet.Platform.HttpContext.Extensions;
using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpCookieCollection"/> object implementing <see cref="IHttpCookieCollection"/> interface.
    /// </summary>
    internal class HttpCookieCollectionImpl : IHttpCookieCollection
    {
        private HttpCookieCollection CookieCollection { get; }


        public HttpCookieCollectionImpl(HttpCookieCollection sourceCollection)
        {
            CookieCollection = sourceCollection;
        }


        public void Add(IHttpCookie cookie)
        {
            CookieCollection.Add(cookie.ToHttpCookie());
        }


        public IHttpCookie this[string name]
        {
            get
            {
                var cookie = CookieCollection[name];
                return cookie != null ? new HttpCookieImpl(cookie) : null;
            }
        }


        public string[] AllKeys => CookieCollection.AllKeys;


        public int Count => CookieCollection.Count;


        public object SyncRoot => ((ICollection)CookieCollection).SyncRoot;


        public bool IsSynchronized => ((ICollection)CookieCollection).IsSynchronized;


        public void Clear()
        {
            CookieCollection.Clear();
        }


        public void Remove(string name)
        {
            CookieCollection.Remove(name);
        }


        public void CopyTo(Array array, int index)
        {
            CookieCollection.CopyTo(array, index);
        }


        public IEnumerator GetEnumerator()
        {
            return CookieCollection.GetEnumerator();
        }
    }
}
