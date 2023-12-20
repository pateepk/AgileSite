using Kentico.Web.Mvc;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// <see cref="IOutputCacheKeyOptions"/> extension methods.
    /// </summary>
    public static class OutputCacheKeyOptionsExtensions
    {
        /// <summary>
        /// Ensures that output cache varies on persona.
        /// </summary>
        /// <param name="options"><see cref="IOutputCacheKeyOptions"/> object.</param>
        public static IOutputCacheKeyOptions VaryByPersona(this IOutputCacheKeyOptions options)
        {
            options.AddCacheKey(new PersonaOutputCacheKey());

            return options;
        }


        /// <summary>
        /// Ensures that output cache varies on A/B test variants.
        /// </summary>
        /// <param name="options"><see cref="IOutputCacheKeyOptions"/> object.</param>
        public static IOutputCacheKeyOptions VaryByABTestVariant(this IOutputCacheKeyOptions options)
        {
            options.AddCacheKey(new ABTestOutputCacheKey());

            return options;
        }
    }
}
