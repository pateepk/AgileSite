using System;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Provides interface for serialization and deserialization of page template configuration to/from JSON format.
    /// </summary>
    internal interface IPageTemplateConfigurationSerializer
    {
        /// <summary>
        /// Serializes page template configuration to JSON string.
        /// </summary>
        /// <param name="configuration">Page template configuration.</param>
        string Serialize(PageTemplateConfiguration configuration);


        /// <summary>
        /// Deserializes JSON string to page template configuration.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <param name="pageTemplateDefinitionProvider">Provider to retrieve page template definitions.</param>
        /// <exception cref="InvalidOperationException"><paramref name="json"/> is in incorrect format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pageTemplateDefinitionProvider"/> is <c>null</c>.</exception>
        PageTemplateConfiguration Deserialize(string json, IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider);
    }
}