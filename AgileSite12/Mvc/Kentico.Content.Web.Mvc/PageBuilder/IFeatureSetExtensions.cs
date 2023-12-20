using System;

using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for the <see cref="IFeatureSet"/> interface.
    /// </summary>
    public static class IFeatureSetExtensions
    {
        /// <summary>
        /// Returns a feature that provides information about Page builder.
        /// </summary>
        /// <param name="features">The set of features.</param>
        /// <returns>A feature that provides information about Page builder.</returns>
        /// <exception cref="InvalidOperationException">Thrown when page builder feature is not initialized through <see cref="ApplicationBuilderExtensions.UsePageBuilder(IApplicationBuilder, PageBuilderOptions)"/>.</exception>
        public static IPageBuilderFeature PageBuilder(this IFeatureSet features)
        {
            var feature = features.GetFeature<IPageBuilderFeature>();

            return feature ?? throw new InvalidOperationException("The page builder feature is not enabled. " +
                                                                  "You need to call the 'UsePageBuilder()' method of the Kentico.Web.Mvc.ApplicationBuilder " +
                                                                  "instance at the start of your application's life cycle.");
        }
    }
}
