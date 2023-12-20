using System;
using System.Linq;

using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder;
using CMS.DocumentEngine.PageBuilder.Internal;
using CMS.SiteProvider;

using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.PageBuilder.Web.Mvc.Personalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Manages the Page builder configuration.
    /// </summary>
    internal sealed class PageBuilderConfigurationManager : IPageBuilderConfigurationManager
    {
        private const string TEMP_GUID_FIELD_NAME = "PageBuilderWidgetsGUID";

        private readonly IEditableAreasConfigurationSerializer editableAreasSerializer;
        private readonly IPageTemplateConfigurationSerializer pageTemplateSerializer;
        private readonly IPageBuilderConfigurationSourceLoader loader;
        private readonly IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider;
        private readonly IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider;
        private readonly IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider;
        private readonly IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypeDefinitionProvider;
        private readonly IPreviewLinksConverter previewLinksConverter;


        /// <summary>
        /// Initializes a new instance of the <see cref="PageBuilderConfigurationManager"/> class.
        /// </summary>
        /// <param name="editableAreasSerializer">Serializer to be used to load the editable areas configuration data.</param>
        /// <param name="loader">Loader to be used to retrieve page configuration.</param>
        /// <param name="pageTemplateSerializer">Serializer to be used to load the page template configuration data.</param>
        /// <param name="widgetDefinitionProvider">Provider to retrieve widget definitions.</param>
        /// <param name="sectionDefinitionProvider">Provider to retrieve section definitions.</param>
        /// <param name="pageTemplateDefinitionProvider">Provider to retrieve page template definitions.</param>
        /// <param name="conditionTypeDefinitionProvider">Provider to retrieve personalization condition type definitions.</param>
        /// <param name="previewLinksConverter">The preview links converter to be used for unresolving links in rich text properties.</param>
        /// <exception cref="ArgumentNullException"><paramref name="editableAreasSerializer"/> or <paramref name="pageTemplateSerializer"/> or <paramref name="loader"/> or
        /// <paramref name="widgetDefinitionProvider"/> or <paramref name="sectionDefinitionProvider"/> or <paramref name="sectionDefinitionProvider"/> or
        /// <paramref name="pageTemplateDefinitionProvider"/> or <paramref name="conditionTypeDefinitionProvider"/> or <paramref name="previewLinksConverter"/> is <c>null</c></exception>
        public PageBuilderConfigurationManager(
            IEditableAreasConfigurationSerializer editableAreasSerializer,
            IPageTemplateConfigurationSerializer pageTemplateSerializer,
            IPageBuilderConfigurationSourceLoader loader,
            IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider,
            IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider,
            IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider,
            IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypeDefinitionProvider,
            IPreviewLinksConverter previewLinksConverter)
        {
            this.editableAreasSerializer = editableAreasSerializer ?? throw new ArgumentNullException(nameof(editableAreasSerializer));
            this.pageTemplateSerializer = pageTemplateSerializer ?? throw new ArgumentNullException(nameof(pageTemplateSerializer));
            this.loader = loader ?? throw new ArgumentNullException(nameof(loader));
            this.widgetDefinitionProvider = widgetDefinitionProvider ?? throw new ArgumentNullException(nameof(widgetDefinitionProvider));
            this.sectionDefinitionProvider = sectionDefinitionProvider ?? throw new ArgumentNullException(nameof(sectionDefinitionProvider));
            this.pageTemplateDefinitionProvider = pageTemplateDefinitionProvider ?? throw new ArgumentNullException(nameof(pageTemplateDefinitionProvider));
            this.conditionTypeDefinitionProvider = conditionTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(conditionTypeDefinitionProvider));
            this.previewLinksConverter = previewLinksConverter ?? throw new ArgumentNullException(nameof(previewLinksConverter));
        }


        /// <summary>
        /// Loads configuration from the storage.
        /// </summary>
        /// <param name="page">Page with the configuration.</param>
        public PageBuilderConfiguration Load(TreeNode page)
        {
            if (page == null)
            {
                return new PageBuilderConfiguration();
            }

            var configurationSource = loader.Load(page);

            return new PageBuilderConfiguration
            {
                Page = GetPageConfiguration(configurationSource.WidgetsConfiguration),
                PageTemplate = GetTemplateConfiguration(configurationSource.PageTemplateConfiguration)
            };
        }


        /// <summary>
        /// Stores configuration to the storage.
        /// </summary>
        /// <param name="instanceGuid">Identifier of the editing instance.</param>
        /// <param name="configuration">Configuration to store.</param>
        public void Store(Guid instanceGuid, string configuration)
        {
            if (instanceGuid == Guid.Empty)
            {
                throw new ArgumentException("Editing instance GUID not provided.", nameof(instanceGuid));
            }

            string pageConfiguration;
            string templateConfiguration;

            try
            {
                var jConfiguration = JObject.Parse(configuration);

                var pageConfigurationValue = jConfiguration.GetValue("page");
                pageConfiguration = pageConfigurationValue.Type != JTokenType.Null ? pageConfigurationValue.ToString(Formatting.None) : null;

                var templateConfigurationValue = jConfiguration.GetValue("pageTemplate");
                templateConfiguration = templateConfigurationValue.Type != JTokenType.Null ? templateConfigurationValue.ToString(Formatting.None) : null;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Incorrect format of deserialized string.", e);
            }

            var pagebuilderConfiguration = new PageBuilderConfiguration
            {
                Page = GetPageConfiguration(pageConfiguration),
                PageTemplate = GetTemplateConfiguration(templateConfiguration)
            };

            // Unresolve URLs in text properties
            previewLinksConverter.Unresolve(pagebuilderConfiguration);

            StoreConfiguration(instanceGuid, pagebuilderConfiguration);
        }


        /// <summary>
        /// Changes configuration based on new page template and stores it to the storage.
        /// </summary>
        /// <param name="instanceGuid">Identifier of the editing instance.</param>
        /// <param name="templateIdentifier">Identifier of page template to set.</param>
        public void ChangeTemplate(Guid instanceGuid, string templateIdentifier)
        {
            if (instanceGuid == Guid.Empty)
            {
                throw new ArgumentException("Editing instance GUID not provided.", nameof(instanceGuid));
            }

            if (string.IsNullOrEmpty(templateIdentifier))
            {
                throw new ArgumentException("Page template identifier not provided.", nameof(templateIdentifier));
            }

            PageBuilderConfiguration configuration;
            if (IsDefaultPageTemplate(templateIdentifier))
            {
                configuration = CreateDefaultPageTemplateConfiguration(templateIdentifier);
            }
            else if (Guid.TryParse(templateIdentifier, out var customTemplateGuid) && customTemplateGuid != Guid.Empty)
            {
                configuration = CreateCustomPageTemplateConfiguration(customTemplateGuid);
            }
            else
            {
                throw new ArgumentException($"Page template with '{templateIdentifier}' identifier doesn't exist.", nameof(templateIdentifier));
            }

            StoreConfiguration(instanceGuid, configuration);
        }


        private void StoreConfiguration(Guid instanceGuid, PageBuilderConfiguration configuration)
        {
            var tempData = TempPageBuilderWidgetsInfoProvider.GetPageBuilderWidgets()
                                                            .WhereEquals(TEMP_GUID_FIELD_NAME, instanceGuid)
                                                            .TopN(1)
                                                            .FirstOrDefault() ?? new TempPageBuilderWidgetsInfo { PageBuilderWidgetsGuid = instanceGuid };
            if (configuration.Page != null)
            {
                tempData.PageBuilderWidgetsConfiguration = editableAreasSerializer.Serialize(configuration.Page);
            }

            if (configuration.PageTemplate != null)
            {
                tempData.PageBuilderTemplateConfiguration = pageTemplateSerializer.Serialize(configuration.PageTemplate);
            }

            TempPageBuilderWidgetsInfoProvider.SetPageBuilderWidgetsInfo(tempData);
        }


        private PageTemplateConfiguration GetTemplateConfiguration(string templateConfiguration)
        {
            return string.IsNullOrEmpty(templateConfiguration) ? null : pageTemplateSerializer.Deserialize(templateConfiguration, pageTemplateDefinitionProvider);
        }


        private EditableAreasConfiguration GetPageConfiguration(string pageConfiguration)
        {
            return string.IsNullOrEmpty(pageConfiguration) ? new EditableAreasConfiguration() : editableAreasSerializer.Deserialize(pageConfiguration, widgetDefinitionProvider, sectionDefinitionProvider, conditionTypeDefinitionProvider);
        }


        private PageBuilderConfiguration CreateCustomPageTemplateConfiguration(Guid customTemplateGuid)
        {
            var customTemplate = PageTemplateConfigurationInfoProvider.GetPageTemplateConfigurationInfoByGUID(customTemplateGuid, SiteContext.CurrentSiteID);
            var pageConfiguration = GetPageConfiguration(customTemplate.PageTemplateConfigurationWidgets);
            var templateConfiguration = GetTemplateConfiguration(customTemplate.PageTemplateConfigurationTemplate);
            templateConfiguration.ConfigurationIdentifier = customTemplateGuid;

            return new PageBuilderConfiguration
            {
                Page = pageConfiguration,
                PageTemplate = templateConfiguration
            };
        }


        private PageBuilderConfiguration CreateDefaultPageTemplateConfiguration(string defaultTemplateIdentifier)
        {
            return new PageBuilderConfiguration
            {
                Page = new EditableAreasConfiguration(),
                PageTemplate = new PageTemplateConfiguration
                {
                    Identifier = defaultTemplateIdentifier
                }
            };
        }


        private bool IsDefaultPageTemplate(string templateIdentifier)
        {
            return pageTemplateDefinitionProvider
                .GetAll()
                .Any(def => def.Identifier.Equals(templateIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
