using System.Collections;
using System.Web;

using CMS.Base;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/> and <see cref="HttpContextBase"/> classes.
    /// </summary>
    public static class HttpContextExtensions
    {
        private const string FEATURE_BUNDLE_KEY = "Kentico.Features";

        /// <summary>
        /// Returns a feature set for the specified request.
        /// </summary>
        /// <param name="context">The object that encapsulates information about an HTTP request.</param>
        /// <returns>A feature set for the specified request.</returns>
        public static IFeatureSet Kentico(this HttpContext context)
        {
            return GetOrCreateFeaturesFromItems(context.Items);
        }


        /// <summary>
        /// Returns a feature set for the specified request.
        /// </summary>
        /// <param name="context">The object that encapsulates information about an HTTP request.</param>
        /// <returns>A feature set for the specified request.</returns>
        public static IFeatureSet Kentico(this HttpContextBase context)
        {
            return GetOrCreateFeaturesFromItems(context.Items);
        }



        /// <summary>
        /// Returns a feature set for the specified request.
        /// </summary>
        /// <param name="context">The object that encapsulates information about an HTTP request.</param>
        /// <returns>A feature set for the specified request.</returns>
        internal static IFeatureSet Kentico(this IHttpContext context)
        {
            return GetOrCreateFeaturesFromItems(context.Items);
        }


        private static IFeatureSet GetOrCreateFeaturesFromItems(IDictionary items)
        {
            IFeatureSet features = items[FEATURE_BUNDLE_KEY] as IFeatureSet;
            if (features == null)
            {
                features = new FeatureSet();
                items[FEATURE_BUNDLE_KEY] = features;
            }

            return features;
        }
    }
}
