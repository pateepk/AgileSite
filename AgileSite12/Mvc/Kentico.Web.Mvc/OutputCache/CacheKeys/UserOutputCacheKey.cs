using System.Web;

namespace Kentico.Web.Mvc
{
    
    /// <summary>
    /// Class used to generate cache item key for different users.
    /// </summary>
    internal class UserOutputCacheKey : IOutputCacheKey
    {
        public string Name => "KenticoUser";

        public string GetVaryByCustomString(HttpContextBase context, string custom)
        {
            return $"{Name}={context.User.Identity.Name}";
        }
    }
}
