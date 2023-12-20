namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for conversion of absolute URL paths and removal of virtual context data of all the component properties of the type <see cref="string"/>.
    /// </summary>
    internal interface IPreviewLinksConverter
    {
        /// <summary>
        /// Unresolves URLs in HTML attributes and removes virtual context data of all the component properties of the type <see cref="string"/>.
        /// </summary>
        /// <param name="configuration">The page builder configuration to be processed.</param>
        void Unresolve(PageBuilderConfiguration configuration);
    }
}