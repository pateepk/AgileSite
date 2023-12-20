using System.Web;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Class used to generate cache item key for different browsers.
    /// </summary>
    internal class BrowserOutputCacheKey : IOutputCacheKey
    {
        public string Name => "KenticoBrowser";


        public string GetVaryByCustomString(HttpContextBase context, string custom)
        {
            return $"browser={context.Request.Browser.Type}";
        }
    }
}
