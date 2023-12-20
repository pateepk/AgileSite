using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CMS.Base;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Helps with creating a cache key to be used in output cache.
    /// </summary>
    public class OutputCacheKeyHelper : AbstractHelper<OutputCacheKeyHelper>
    {
        /// <summary>
        /// Creates <see cref="IOutputCacheKeyOptions"/> instance, used to set up cache dependencies.
        /// </summary>
        public static IOutputCacheKeyOptions CreateOptions()
        {
            return HelperObject.CreateOptionsInternal();
        }


        /// <summary>
        /// Returns a cache key built from <paramref name="options"/>, <paramref name="context"/> and <paramref name="custom"/>.
        /// </summary>
        /// <param name="context">An <see cref="HttpContext"/> object that contains information about the current Web request.</param>
        /// <param name="custom">The custom string that specifies which cached response is used to respond to the current request.</param>
        /// <param name="options">Options object with various vary by values.</param>
        public static string GetVaryByCustomString(HttpContext context, string custom, IOutputCacheKeyOptions options)
        {
            var contextWrapper = new HttpContextWrapper(context);
            return HelperObject.GetVaryByCustomStringInternal(contextWrapper, custom, options);
        }


        /// <summary>
        /// Returns a cache key built from <paramref name="options"/>, <paramref name="context"/> and <paramref name="custom"/>.
        /// </summary>
        /// <param name="context">An <see cref="HttpContext"/> object that contains information about the current Web request.</param>
        /// <param name="custom">The custom string that specifies which cached response is used to respond to the current request.</param>
        /// <param name="options">Options object with various vary by values.</param>
        protected virtual string GetVaryByCustomStringInternal(HttpContextBase context, string custom, IOutputCacheKeyOptions options)
        {
            var itemCustoms = options.GetOutputCacheKeys().Select(x => x.GetVaryByCustomString(context, custom));

            return String.Join("|", itemCustoms ?? new string[] { });
        }


        /// <summary>
        /// Creates <see cref="IOutputCacheKeyOptions"/> instance, used to set up cache dependencies.
        /// </summary>
        protected virtual IOutputCacheKeyOptions CreateOptionsInternal()
        {
            return new OutputCacheOptions();
        }


        private class OutputCacheOptions : IOutputCacheKeyOptions
        {
            private readonly IList<IOutputCacheKey> cacheKeys = new List<IOutputCacheKey>();

            public void AddCacheKey(IOutputCacheKey outputCacheKey)
            {
                cacheKeys.Add(outputCacheKey);
            }

            public IEnumerable<IOutputCacheKey> GetOutputCacheKeys()
            {
                return cacheKeys;
            }
        }
    }
}
