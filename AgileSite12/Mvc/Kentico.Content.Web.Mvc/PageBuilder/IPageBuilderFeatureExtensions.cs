namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for the <see cref="IPageBuilderFeature"/> interface.
    /// </summary>
    internal static class IPageBuilderFeatureExtensions
    {
        /// <summary>
        /// Returns a data context for the Page builder feature.
        /// </summary>
        /// <param name="feature">The Page builder feature.</param>
        public static IPageBuilderDataContext GetDataContext(this IPageBuilderFeature feature)
        {
            return feature.GetProperties().DataContext;
        }


        /// <summary>
        /// Returns internal properties of the Page builder feature.
        /// </summary>
        /// <param name="feature">The Page builder feature.</param>
        internal static IPageBuilderInternalProperties GetProperties(this IPageBuilderFeature feature)
        {
            return feature as IPageBuilderInternalProperties;
        }
    }
}
