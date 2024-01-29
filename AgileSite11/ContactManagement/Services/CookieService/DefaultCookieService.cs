using System;

using CMS.Core.Internal;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides methods for getting and setting the cookie from/to the request/response.
    /// </summary>
    /// <remarks>
    /// Only wraps <see cref="CookieHelper"/> for providing the ability to mock the service.
    /// </remarks>
    internal class DefaultCookieService : ICookieService
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        /// <summary>
        /// Instantiates new instance of <see cref="DefaultCookieService"/>
        /// </summary>
        /// <param name="dateTimeNowService">Service for obtaining current date time</param>
        /// <exception cref="ArgumentNullException"><paramref name="dateTimeNowService"/> is <c>null</c></exception>
        public DefaultCookieService(IDateTimeNowService dateTimeNowService)
        {
            if (dateTimeNowService == null)
            {
                throw new ArgumentNullException(nameof(dateTimeNowService));
            }
            
            mDateTimeNowService = dateTimeNowService;
        }


        /// <summary>
        /// Returns cookie value for given <paramref name="cookieName"/> from the request/response.
        /// </summary>
        /// <remarks>
        /// Cookie can be loaded from the response if it was set earlier during the request handling.
        /// </remarks>
        /// <param name="cookieName">Name of the cookie to be obtained</param>
        /// <exception cref="ArgumentNullException"><paramref name="cookieName"/> is <c>null</c></exception>
        /// <returns>Value of the cookie. If no cookie with <paramref name="cookieName"/> exists, returns <c>null</c></returns>
        public string GetValue(string cookieName)
        {
            if (cookieName == null)
            {
                throw new ArgumentNullException(nameof(cookieName));
            }

            return CookieHelper.GetValue(cookieName);
        }


        /// <summary>
        /// Sets given <paramref name="value"/> to the cookie named <paramref name="cookieName"/> to the response.
        /// </summary>
        /// <remarks>
        /// If cookie with <paramref name="cookieName"/> already exists in the response, it is overwritten with new value.
        /// </remarks>
        /// <param name="cookieName">Name of the cookie to be set</param>
        /// <param name="value">Value of the cookie to be set</param>
        /// <param name="duration">Duration specifying how long will be the cookie valid till it expires</param>
        public void SetValue(string cookieName, string value, TimeSpan duration)
        {
            CookieHelper.SetValue(cookieName, value, mDateTimeNowService.GetDateTimeNow().Add(duration));
        }
    }
}