using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.OnlineMarketing.Internal;


namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Implementation of<see cref="IABResponseCookieProvider"/> with delayed set to headers.
    /// </summary>
    /// <remarks>
    /// Set is delayed because of default <see cref="OutputCache"/> behavior. The output is not cached if cookie is set within request.
    /// Output and cookie is set if we postpone set operation to the <see cref="RequestEvents.PreSendRequestHeaders"/> request event.
    /// </remarks>
    internal class DelayedABResponseCookieProvider : IABResponseCookieProvider
    {
        private const string COOKIE_STORE_KEY = "ABCookieDelayedSetterCookies";

        /// <summary>
        /// Initialize <see cref="DelayedABResponseCookieProvider"/>.
        /// </summary>
        public static void Init()
        {
            Service.Use<IABResponseCookieProvider, DelayedABResponseCookieProvider>();
            RequestEvents.PreSendRequestHeaders.Execute += PreSendRequestHeaders_Execute;
        }


        /// <summary>
        /// Sets the cookie to the delayed response table.
        /// </summary>
        public void SetValue(string name, string value, string path, DateTime expires, bool? httpOnly, string domain)
        {
            var cookie = new Cookie()
            {
                Name = name,
                Value = value,
                Path = path,
                Expires = expires,
                Domain = domain,
                HttpOnly = httpOnly
            };

            var cookiesTable = GetResponseCookiesTable(create: true);
            cookiesTable[cookie.Name] = cookie;
        }


        /// <summary>
        /// Gets the cookie from delayed response cookie table or from <see cref="CookieHelper.GetValue(string)"/>
        /// </summary>
        public string GetValue(string cookieName)
        {
            var cookiesTable = GetResponseCookiesTable(create: false);
            if (cookiesTable != null && cookiesTable.TryGetValue(cookieName, out var cookie))
            {
                return cookie.Value;
            }

            return CookieHelper.GetValue(cookieName);
        }


        /// <summary>
        /// Sets required cookies to the header
        /// </summary>
        private static void PreSendRequestHeaders_Execute(object sender, EventArgs e)
        {
            var cookiesTable = GetResponseCookiesTable(create: false);
            if (cookiesTable != null)
            {
                foreach (var cookie in cookiesTable.Values)
                {
                    CookieHelper.SetValue(cookie.Name, cookie.Value, cookie.Path, cookie.Expires, cookie.HttpOnly, cookie.Domain);
                }
            }
        }


        /// <summary>
        /// Returns unique cookies names from reques, response and current delayed in-memory response table.
        /// </summary>
        public IEnumerable<string> GetDistinctCookieNames()
        {
            var cookiesTableKeys = GetResponseCookiesTable(create: false)?.Keys ?? Enumerable.Empty<string>();
            return CookieHelper.GetDistinctCookieNames().Union(cookiesTableKeys, StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Returns collection of cookies required for current response.
        /// </summary>
        /// <param name="create">Indicates whether empty collection should be created if not exist.</param>
        internal static IDictionary<string, Cookie> GetResponseCookiesTable(bool create)
        {
            IDictionary<string, Cookie> cookiesTable = null;
            var items = CMSHttpContext.Current?.Items;

            if (items != null)
            {
                cookiesTable = items[COOKIE_STORE_KEY] as IDictionary<string, Cookie>;

                if (cookiesTable == null && create)
                {
                    cookiesTable = new Dictionary<string, Cookie>(StringComparer.InvariantCultureIgnoreCase);
                    items[COOKIE_STORE_KEY] = cookiesTable;
                }
            }

            return cookiesTable;
        }


        /// <summary>
        /// Cookie container class
        /// </summary>
        internal class Cookie
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Path { get; set; }
            public string Domain { get; set; }
            public DateTime Expires { get; set; }
            public bool? HttpOnly { get; set; }
        }
    }
}
