using System.Web;

using CMS.Core;
using CMS.Helpers;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Class used to generate cache item key for different cookie levels.
    /// </summary>
    internal class CookieLevelCacheKey : IOutputCacheKey
    {
        public string Name => "KenticoCookieLevel";


        public string GetVaryByCustomString(HttpContextBase context, string custom)
        {
            var cookieLevelProvider = Service.Resolve<ICurrentCookieLevelProvider>();

            return $"{Name}={cookieLevelProvider.GetCurrentCookieLevel()}";
        }
    }
}
