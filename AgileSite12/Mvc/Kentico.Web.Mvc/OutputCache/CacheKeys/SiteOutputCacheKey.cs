using System.Web;

using CMS.SiteProvider;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Class used to generate cache item key for different sites.
    /// </summary>
    internal class SiteOutputCacheKey : IOutputCacheKey
    {
        public string Name => "KenticoSite";


        public string GetVaryByCustomString(HttpContextBase context, string custom)
        {
            return $"{Name}={SiteContext.CurrentSiteName}";
        }
    }
}
