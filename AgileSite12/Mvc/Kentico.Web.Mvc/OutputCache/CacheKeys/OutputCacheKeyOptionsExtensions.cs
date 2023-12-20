namespace Kentico.Web.Mvc
{
    /// <summary>
    /// <see cref="IOutputCacheKeyOptions"/> extension methods.
    /// </summary>
    public static class OutputCacheKeyOptionsExtensions
    {
        /// <summary>
        /// Ensures that output cache vary by user.
        /// </summary>
        /// <param name="options"><see cref="IOutputCacheKeyOptions"/> object.</param>
        public static IOutputCacheKeyOptions VaryByUser(this IOutputCacheKeyOptions options)
        {
            options.AddCacheKey(new UserOutputCacheKey());

            return options;
        }


        /// <summary>
        /// Ensures that output cache vary by host name.
        /// </summary>
        /// <param name="options"><see cref="IOutputCacheKeyOptions"/> object.</param>
        public static IOutputCacheKeyOptions VaryByHost(this IOutputCacheKeyOptions options)
        {
            options.AddCacheKey(new HostOutputCacheKey());

            return options;
        }


        /// <summary>
        /// Ensures that output cache vary by site name.
        /// </summary>
        /// <param name="options"><see cref="IOutputCacheKeyOptions"/> object.</param>
        public static IOutputCacheKeyOptions VaryBySite(this IOutputCacheKeyOptions options)
        {
            options.AddCacheKey(new SiteOutputCacheKey());

            return options;
        }


        /// <summary>
        /// Ensures that output cache vary by browser type.
        /// </summary>
        /// <param name="options"><see cref="IOutputCacheKeyOptions"/> object.</param>
        public static IOutputCacheKeyOptions VaryByBrowser(this IOutputCacheKeyOptions options)
        {
            options.AddCacheKey(new BrowserOutputCacheKey());

            return options;
        }


        /// <summary>
        /// Ensures that output cache vary by cookie level.
        /// </summary>
        /// <param name="options"><see cref="IOutputCacheKeyOptions"/> object.</param>
        public static IOutputCacheKeyOptions VaryByCookieLevel(this IOutputCacheKeyOptions options)
        {
            options.AddCacheKey(new CookieLevelCacheKey());

            return options;
        }
    }
}
