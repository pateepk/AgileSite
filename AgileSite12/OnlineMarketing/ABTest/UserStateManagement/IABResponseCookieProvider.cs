using System;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.OnlineMarketing.Internal;

[assembly: RegisterImplementation(typeof(IABResponseCookieProvider), typeof(DefaultABResponseCookieProvider), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Defines interface for cookie methods.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public interface IABResponseCookieProvider
    {
        /// <summary>
        /// Sets the cookie value.
        /// </summary>
        /// <param name="name">Cookie name</param>
        /// <param name="value">Cookie value</param>
        /// <param name="path">Cookie path</param>
        /// <param name="expires">Cookie expiration</param>
        /// <param name="httpOnly">Indicates whether a cookie is accessible by client-side script</param>
        /// <param name="domain">Cookie domain</param>
        void SetValue(string name, string value, string path, DateTime expires, bool? httpOnly, string domain);


        /// <summary>
        /// Returns cookie value
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        string GetValue(string cookieName);


        /// <summary>
        ///  Gets names of both request and response cookies without duplicates.
        /// </summary>
        IEnumerable<string> GetDistinctCookieNames();
    }
}
