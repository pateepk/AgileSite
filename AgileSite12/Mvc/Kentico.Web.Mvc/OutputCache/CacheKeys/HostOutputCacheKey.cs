using System.Web;

using CMS.AspNet.Platform;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Class used to generate cache item key for different hosts.
    /// </summary>
    internal class HostOutputCacheKey : IOutputCacheKey
    {
        public string Name => "KenticoHost";

        public string GetVaryByCustomString(HttpContextBase context, string custom)
        {
            return $"{Name}={context.Request.GetEffectiveUrl().Host}";
        }
    }
}
