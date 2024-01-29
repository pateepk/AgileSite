using CMS;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.DocumentEngine;

[assembly: RegisterExtension(typeof(CacheMethods), typeof(SystemNamespace))]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Wrapper class to provide basic caching methods in the MacroEngine.
    /// </summary>
    internal class CacheMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns the cached data context
        /// </summary>
        /// <param name="parameters">Macro functions parameters ([0] - cache minutes [1] - cache key</param>
        [MacroMethod(typeof(object), "Returns cached data context object.", 2)]
        [MacroMethodParam(0, "cacheMinutes", typeof(int), "Cache minutes")]
        [MacroMethodParam(1, "cacheItemName", typeof(string), "Cache item name.")]
        public static object CMSCachedContext(params object[] parameters)
        {
            // Prepare the parameters
            int cacheMinutes = -1;
            if (parameters.Length >= 1)
            {
                cacheMinutes = ValidationHelper.GetInteger(parameters[0], -1);
            }
            string siteName = SiteContext.CurrentSiteName;
            if (cacheMinutes < 0)
            {
                cacheMinutes = CacheHelper.CacheMinutes(siteName);
            }

            // Cache item name
            string cacheItemName = null;
            if (parameters.Length >= 2)
            {
                cacheItemName = ValidationHelper.GetString(parameters[1], null);
            }

            // Dependency keys
            string dependencies = null;
            if (parameters.Length >= 3)
            {
                dependencies = ValidationHelper.GetString(parameters[3], null);
            }

            // Get the context
            CMSDataContext result = null;
            using (var cs = new CachedSection<CMSDataContext>(ref result, cacheMinutes, true, cacheItemName, "CachedDataContext", siteName, cacheMinutes))
            {
                if (cs.LoadData)
                {
                    result = CMSDataContext.Current;
                    result.IsCachedObject = true;

                    // Add cache dependencies
                    if (dependencies != null)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                    }

                    cs.Data = result;
                }
            }

            return result;
        }
    }
}