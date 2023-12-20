using System;

using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for the <see cref="IFeatureSet"/> interface.
    /// </summary>
    public static class IFeatureSetExtensions
    {
        /// <summary>
        /// Returns a feature that provides information about preview.
        /// </summary>
        /// <param name="features">The set of features.</param>
        /// <returns>A feature that provides information about preview.</returns>
        /// <exception cref="InvalidOperationException">Thrown when preview feature is not initialized through <see cref="ApplicationBuilderExtensions.UsePreview(IApplicationBuilder)"/>.</exception>
        public static IPreviewFeature Preview(this IFeatureSet features)
        {
            var feature = features.GetFeature<IPreviewFeature>();
            
            return feature ?? throw new InvalidOperationException("The preview feature is not enabled. " +
                                                                  "You need to call the 'UsePreview()' method of the Kentico.Web.Mvc.ApplicationBuilder " +
                                                                  "instance at the start of your application's life cycle.");
        }
    }
}
