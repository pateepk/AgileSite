using System.Web;

using CMS.Core;

using Kentico.Web.Mvc;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Class used to generate cache item key for different A/B test variants.
    /// </summary>
    internal class ABTestOutputCacheKey : IOutputCacheKey
    {
        public string Name => "KenticoABTestVariant";

        public string GetVaryByCustomString(HttpContextBase context, string custom)
        {
#pragma warning disable BH1006 // 'Request.Url' should not be used. Use 'RequestContext.URL' instead.
            var variant = Service.Resolve<IOutputCacheUrlToPageMapper>().GetABTestVariantForUrl(context.Request.Url);
#pragma warning restore BH1006 // 'Request.Url' should not be used. Use 'RequestContext.URL' instead.
            if (variant != null)
            {
                return $"{Name}={variant.Guid}";
            }

            return null;
        }
    }
}
