using System;

using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(ICookieService), typeof(DefaultCookieService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides methods for getting or setting the cookie from/to the request/response.
    /// </summary>
    internal interface ICookieService
    {
        /// <summary>
        /// Returns cookie value for given <paramref name="cookieName"/> from the request/response.
        /// </summary>
        /// <remarks>
        /// Cookie can be loaded from the response if it was set earlier during the request handling.
        /// </remarks>
        /// <param name="cookieName">Name of the cookie to be obtained</param>
        /// <example>
        /// <para>Following example shows how to use GetValue method</para>
        /// <code>
        /// ...
        /// ICookieService cookieService = someImplementation;
        /// // Returns value of the cookie with name 'cookie'
        /// cookieService.GetValue("cookie");
        /// 
        /// // Returns null
        /// cookieServive.GetValue("nonExistingCookie");
        /// ...
        /// </code>
        /// </example>
        /// <returns>Value of the cookie. If no cookie with <paramref name="cookieName"/> exists, returns <c>null</c></returns>
        string GetValue(string cookieName);


        /// <summary>
        /// Sets given <paramref name="value"/> to the cookie named <paramref name="cookieName"/> to the response.
        /// </summary>
        /// <param name="cookieName">Name of the cookie to be set</param>
        /// <param name="value">Value of the cookie to be set</param>
        /// <param name="duration">Duration specifying how long will be the cookie valid till it expires</param>
        /// <remarks>
        /// <para>If cookie with <paramref name="cookieName"/> already exists in the response, it is overwritten with new value.</para>
        /// <para>
        /// As a base date for the cookie expiration is used current date time, therefore when <c>TimeSpan.FromDays(20)</c> is used for <paramref name="duration"/> 
        /// at midnight on 2016/01/01, the cookie will expire at midnight on 2016/01/21.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>Following example shows how to use SetValue method</para>
        /// <code>
        /// ...
        /// ICookieService cookieService = someImplementation;
        /// // Sets cookie with name 'cookieName' with value 'cookieValue' and sets its duration for 20 days
        /// cookieService.SetValue("cookieName", "cookieValue", TimeSpan.FromDays(20));
        /// ...
        /// </code>
        /// </example>
        void SetValue(string cookieName, string value, TimeSpan duration);
    }
}