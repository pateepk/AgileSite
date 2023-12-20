namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface to build components metadata.
    /// </summary>
    internal interface IComponentsMetadataBuilder
    {
        /// <summary>
        /// Gets metadata for all registered components.
        /// </summary>
        ComponentsMetadata GetAll();
    }
}
