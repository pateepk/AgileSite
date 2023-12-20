using System;
using System.Collections.Generic;
using System.Linq;

using Kentico.Builder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Personalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Handles serialization and deserialization of editable areas configuration to/from JSON format.
    /// </summary>
    internal sealed class EditableAreasConfigurationSerializer : IEditableAreasConfigurationSerializer
    {
        /// <summary>
        /// Serializes editable areas configuration to JSON string.
        /// </summary>
        /// <param name="configuration">Editable areas configuration.</param>
        public string Serialize(EditableAreasConfiguration configuration)
        {
            return JsonConvert.SerializeObject(configuration, Formatting.None, GetSettings());
        }


        /// <summary>
        /// Deserializes JSON string to editable areas configuration.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <param name="widgetDefinitionProvider">Provider to retrieve widget definitions.</param>
        /// <param name="sectionDefinitionProvider">Provider to retrieve section definitions.</param>
        /// <param name="conditionTypeDefinitionProvider">>Provider to retrieve personalization condition type definitions.</param>
        /// <exception cref="InvalidOperationException"><paramref name="json"/> is in incorrect format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="widgetDefinitionProvider"/> or <paramref name="sectionDefinitionProvider"/> or <paramref name="conditionTypeDefinitionProvider"/> is <c>null</c>.</exception>
        public EditableAreasConfiguration Deserialize(string json, IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider, IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider, IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypeDefinitionProvider)
        {
            if (widgetDefinitionProvider == null)
            {
                throw new ArgumentNullException(nameof(widgetDefinitionProvider));
            }

            if (sectionDefinitionProvider == null)
            {
                throw new ArgumentNullException(nameof(sectionDefinitionProvider));
            }

            if (conditionTypeDefinitionProvider == null)
            {
                throw new ArgumentNullException(nameof(conditionTypeDefinitionProvider));
            }

            try
            {
                var widgetDefinitions = widgetDefinitionProvider.GetAll().ToList();
                var sectionDefinitions = sectionDefinitionProvider.GetAll().ToList();
                var conditionTypesDefinitions = conditionTypeDefinitionProvider.GetAll().ToList();
                var jConfiguration = JObject.Parse(json);

                var configuration = new EditableAreasConfiguration();
                dynamic editableAreas = ((dynamic)jConfiguration).editableAreas;

                foreach (dynamic jArea in editableAreas)
                {
                    var area = GetArea(jArea, widgetDefinitions, sectionDefinitions, conditionTypesDefinitions);

                    configuration.EditableAreas.Add(area);
                }

                return configuration;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Incorrect format of deserialized string.", e);
            }
        }


        private EditableAreaConfiguration GetArea(dynamic jArea, List<WidgetDefinition> widgetDefinitions, List<SectionDefinition> sectionDefinitions, List<ConditionTypeDefinition> conditionTypeDefinitions)
        {
            var area = new EditableAreaConfiguration { Identifier = jArea.identifier };
            foreach (var jSection in jArea.sections)
            {
                var section = GetSection(jSection, widgetDefinitions, sectionDefinitions, conditionTypeDefinitions);

                area.Sections.Add(section);
            }

            return area;
        }


        private SectionConfiguration GetSection(dynamic jSection, List<WidgetDefinition> widgetDefinitions, List<SectionDefinition> sectionDefinitions, List<ConditionTypeDefinition> conditionTypeDefinitions)
        {
            var section = new SectionConfiguration
            {
                Identifier = jSection.identifier,
                TypeIdentifier = jSection.type
            };

            var sectionDefinition = GetSectionDefinition(sectionDefinitions, section.TypeIdentifier);
            section.Properties = GetProperties<ISectionProperties>(jSection as JObject, sectionDefinition);

            foreach (var jZone in jSection.zones)
            {
                var zone = GetZone(jZone, widgetDefinitions, conditionTypeDefinitions);

                section.Zones.Add(zone);
            }

            return section;
        }


        private ZoneConfiguration GetZone(dynamic jZone, List<WidgetDefinition> widgetDefinitions, List<ConditionTypeDefinition> conditionTypeDefinitions)
        {
            var zone = new ZoneConfiguration
            {
                Identifier = jZone.identifier
            };

            foreach (dynamic jWidget in jZone.widgets)
            {
                var widget = GetWidget(jWidget, widgetDefinitions, conditionTypeDefinitions);
                zone.Widgets.Add(widget);
            }

            return zone;
        }


        private WidgetConfiguration GetWidget(dynamic jWidget, List<WidgetDefinition> widgetDefinitions, List<ConditionTypeDefinition> conditionTypeDefinitions)
        {
            var widget = new WidgetConfiguration
            {
                Identifier = jWidget.identifier,
                TypeIdentifier = jWidget.type,
                PersonalizationConditionTypeIdentifier = jWidget.conditionType,
            };

            var widgetDefinition = GetWidgetDefinition(widgetDefinitions, widget.TypeIdentifier);
            var conditionTypeDefinition = GetConditionTypeDefinition(conditionTypeDefinitions, widget.PersonalizationConditionTypeIdentifier);
            var variantIndex = 0;
            foreach (dynamic jVariant in jWidget.variants)
            {
                var isOriginalVariant = variantIndex == 0;
                var variant = GetVariant(jVariant, isOriginalVariant, widgetDefinition, conditionTypeDefinition);
                widget.Variants.Add(variant);
                variantIndex++;
            }

            return widget;
        }


        private WidgetVariantConfiguration GetVariant(dynamic jVariant, bool isOriginalVariant, WidgetDefinition widgetDefinition, ConditionTypeDefinition conditionTypeDefinition)
        {
            var variant = new WidgetVariantConfiguration
            {
                Identifier = jVariant.identifier,
                Name = jVariant.name
            };

            variant.Properties = GetProperties<IWidgetProperties>(jVariant as JObject, widgetDefinition);
            if (!isOriginalVariant)
            {
                variant.PersonalizationConditionType = GetConditionType(jVariant as JObject, conditionTypeDefinition);
            }

            return variant;
        }


        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = SerializerHelper.GetDefaultContractResolver()
            };
        }


        private static WidgetDefinition GetWidgetDefinition(List<WidgetDefinition> widgetDefinitions, string typeIdentifier)
        {
            return widgetDefinitions.FirstOrDefault(def => def.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }


        private static SectionDefinition GetSectionDefinition(List<SectionDefinition> sectionDefinitions, string typeIdentifier)
        {
            return sectionDefinitions.FirstOrDefault(def => def.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }


        private static ConditionTypeDefinition GetConditionTypeDefinition(List<ConditionTypeDefinition> conditionTypeDefinitions, string typeIdentifier)
        {
            return conditionTypeDefinitions.FirstOrDefault(def => def.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }


        private TPropertiesInterface GetProperties<TPropertiesInterface>(JObject jObject, IPropertiesComponentDefinition componentDefinition)
            where TPropertiesInterface : class, IComponentProperties
        {
            var propertiesType = componentDefinition?.PropertiesType;
            if (propertiesType == null)
            {
                return null;
            }

            if (!jObject.TryGetValue("properties", StringComparison.InvariantCultureIgnoreCase, out var jProperties) || jProperties.Type == JTokenType.Null)
            {
                return new ComponentDefaultPropertiesProvider().Get<TPropertiesInterface>(propertiesType);
            }

            return new ComponentPropertiesSerializer().DeserializeInternal<TPropertiesInterface>(jProperties, propertiesType);
        }


        private IConditionType GetConditionType(dynamic jVariant, ConditionTypeDefinition conditionTypeDefinition)
        {
            if (conditionTypeDefinition == null)
            {
                return null;
            }

            if (jVariant.conditionTypeParameters == null)
            {
                throw new InvalidOperationException("Missing personalization condition type parameters.");
            }

            return jVariant.conditionTypeParameters.ToObject(conditionTypeDefinition.Type) as IConditionType;
        }
    }
}