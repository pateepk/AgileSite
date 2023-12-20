using System;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides methods for obtaining and manipulation with the cookie level for the current request.
    /// </summary>
    internal class CurrentCookieLevelProvider : ICurrentCookieLevelProvider
    {
        private ISiteService siteService;

        public CurrentCookieLevelProvider()
        {
            siteService = Service.Resolve<ISiteService>();
        }


        /// <summary>
        /// Cookie level from site settings.
        /// </summary>
        /// <returns></returns>
        public int GetDefaultCookieLevel()
        {
            var settingName = "CMSDefaultCookieLevel";
            if (siteService.CurrentSite != null)
            {
                settingName = siteService.CurrentSite.SiteName + ".CMSDefaultCookieLevel";
            }
            return CookieHelper.ConvertCookieLevelToIntegerValue(CoreServices.Settings[settingName], CookieLevel.All);
        }


        /// <summary>
        /// Gets the cookie level of the current request.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the cookie level is not set yet, the application setting "CMSDefaultCookieLevel" specified in the settings is used.
        /// </para>
        /// <para>
        /// When calling this method, <see cref="CookieName.CookieLevel" /> response cookie is set.
        /// </para>
        /// <para>
        /// Inbuilt cookie level values are represented by the constants in the <see cref="CookieLevel"/> class. 
        /// </para>
        /// </remarks>
        /// <returns>Returns the cookie level of the current request.</returns>
        public int GetCurrentCookieLevel()
        {
            // Get the cached value from the current request
            var cachedCookieLevel = RequestStockHelper.GetItem(CookieName.CookieLevel, true);
            if (cachedCookieLevel != null)
            {
                return Convert.ToInt32(cachedCookieLevel);
            }

            // Get the cookie level value from the current request cookies

            // Do not use the defaultValue, the defaultValue is handled in this method manually,
            // otherwise the call from CookieHelper.GetValue(CookieName.CookieLevel) will end up here again and will lead to a newer-ending loop.
            string cookieLevel = CookieHelper.GetValue(CookieName.CookieLevel, useDefaultValue: false, allowSensitiveData: false);
            int cookieLevelValue;

            if (cookieLevel != null)
            {
                cookieLevelValue = ValidationHelper.GetInteger(cookieLevel, GetDefaultCookieLevel());
            }
            else
            {
                // Cookie not set yet, use the given default cookie level
                cookieLevelValue = GetDefaultCookieLevel();
            }

            return cookieLevelValue;
        }


        /// <summary>
        /// Sets the cookie level to the specified level.
        /// </summary>
        /// <remarks>
        /// This action clears all the cookies that have a cookie level set lower that the new cookie level.
        /// New cookie level value is stored into the response cookie <see cref="CookieName.CookieLevel" />.
        /// </remarks>
        /// <param name="cookieLevel">Cookie level to be set. Predefined constants in the <see cref="CookieLevel" /> class can be used.</param>
        public void SetCurrentCookieLevel(int cookieLevel)
        {
            // Remove all the user cookies above the new cookie level
            CookieHelper.RemoveAllCookies(cookieLevel);

            // Cache the cookie level value in the request
            RequestStockHelper.Add(CookieName.CookieLevel, cookieLevel, true);

            CookieHelper.SetValue(CookieName.CookieLevel, cookieLevel.ToString(), null, DateTime.Now.AddYears(1), httpOnly: true);
        }
    }
}
