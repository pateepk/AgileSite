using System;

using Kentico.PageBuilder.Web.Mvc.Personalization;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for serialization and deserialization of editable areas configuration to/from JSON format.
    /// </summary>
    internal interface IEditableAreasConfigurationSerializer
    {
        /// <summary>
        /// Serializes editable areas configuration to JSON string.
        /// </summary>
        /// <param name="configuration">Editable areas configuration.</param>
        string Serialize(EditableAreasConfiguration configuration);


        /// <summary>
        /// Deserializes JSON string to editable areas configuration.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <param name="widgetDefinitionProvider">Provider to retrieve widget definitions.</param>
        /// <param name="sectionDefinitionProvider">Provider to retrieve section definitions.</param>
        /// <param name="conditionTypeDefinitionProvider">>Provider to retrieve personalization condition type definitions.</param>
        /// <exception cref="InvalidOperationException"><paramref name="json"/> is in incorrect format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="widgetDefinitionProvider"/> or <paramref name="sectionDefinitionProvider"/> or <paramref name="conditionTypeDefinitionProvider"/> is <c>null</c>.</exception>
        EditableAreasConfiguration Deserialize(string json, IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider, IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider, IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypeDefinitionProvider);
    }
}