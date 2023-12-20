using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpCachePolicyBase"/> object implementing <see cref="ICache"/> interface.
    /// </summary>
    internal class CachePolicyImpl : ICache
    {
        private readonly HttpCachePolicyBase mCache;


        public CachePolicyImpl(HttpCachePolicyBase cache)
        {
            mCache = cache;
        }


        public void SetCacheability(Base.HttpCacheability cacheability)
        {
            mCache?.SetCacheability((System.Web.HttpCacheability)cacheability);
        }
    }
}
