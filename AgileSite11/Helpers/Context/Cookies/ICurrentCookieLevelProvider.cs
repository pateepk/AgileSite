using CMS.Helpers;

[assembly: CMS.RegisterImplementation(typeof(ICurrentCookieLevelProvider), typeof(CurrentCookieLevelProvider), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Helpers
{
    /// <summary>
    /// Provides methods for obtaining and manipulation with the cookie level for the current request.
    /// </summary>
    public interface ICurrentCookieLevelProvider
    {
        /// <summary>
        /// Gets the default cookie level based on settings.
        /// </summary>
        /// <returns>Returns the default cookie level.</returns>
        int GetDefaultCookieLevel();


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
        int GetCurrentCookieLevel();


        /// <summary>
        /// Sets the cookie level to the specified level.
        /// </summary>
        /// <remarks>
        /// This action clears all the cookies that have a cookie level set lower that the new cookie level.
        /// New cookie level value is stored into the response cookie <see cref="CookieName.CookieLevel" />.
        /// </remarks>
        /// <param name="cookieLevel">Cookie level to be set. Predefined constants in the <see cref="CookieLevel" /> class can be used.</param>
        void SetCurrentCookieLevel(int cookieLevel);
    }
}