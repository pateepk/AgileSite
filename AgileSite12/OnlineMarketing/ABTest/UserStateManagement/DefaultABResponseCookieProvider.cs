using System;
using System.Collections.Generic;
using CMS.Helpers;

namespace CMS.OnlineMarketing.Internal
{
    internal class DefaultABResponseCookieProvider : IABResponseCookieProvider
    {
        public void SetValue(string name, string value, string path, DateTime expires, bool? httpOnly, string domain)
        {
            CookieHelper.SetValue(name, value, path, expires, httpOnly, domain);
        }


        public string GetValue(string cookieName)
        {
            return CookieHelper.GetValue(cookieName);
        }


        public IEnumerable<string> GetDistinctCookieNames()
        {
            return CookieHelper.GetDistinctCookieNames();
        }
    }
}
