using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine.PageBuilder;
using CMS.DocumentEngine.PageBuilder.Internal;
using CMS.EventLog;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc.HttpAttributes;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.PageBuilder.Web.Mvc.Personalization;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving and storing the Page builder configuration.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [UsePageBuilderJsonSerializerSettings]
    [CheckPagePermissions(PermissionsEnum.Modify)]
    public sealed class KenticoEditableAreasConfigurationController : ApiController
    {
        private IPageBuilderConfigurationManager mManager;
        private readonly IPageRetriever pageRetriever;
        private readonly IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider;
        private readonly IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider;
        private readonly IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider;
        private readonly IComponentDefaultPropertiesProvider propertiesProvider;


        /// <summary>
        /// Manager for the Page builder configuration.
        /// </summary>
        internal IPageBuilderConfigurationManager Manager
        {
            get
            {
                if (mManager == null)
                {
                    mManager = GetManager();
                }

                return mManager;
            }
            set
            {
                mManager = value;
            }
        }


        /// <summary>
        /// Creates an instance of <see cref="KenticoEditableAreasConfigurationController"/>.
        /// </summary>
        public KenticoEditableAreasConfigurationController()
        {
            widgetDefinitionProvider = new ComponentDefinitionProvider<WidgetDefinition>();
            sectionDefinitionProvider = new ComponentDefinitionProvider<SectionDefinition>();
            pageTemplateDefinitionProvider = new ComponentDefinitionProvider<PageTemplateDefinition>();
            propertiesProvider = new ComponentDefaultPropertiesProvider();
            pageRetriever = new PageRetriever();
        }


        /// <summary>
        /// Creates an instance of <see cref="KenticoEditableAreasConfigurationController"/>.
        /// </summary>
        /// <param name="pageRetriever">Retriever for page with configuration.</param>
        /// <param name="widgetDefinitionProvider">Provider to retrieve widget definitions.</param>
        /// <param name="sectionDefinitionProvider">Provider to retrieve section definitions.</param>
        /// <param name="pageTemplateDefinitionProvider">Provider to retrieve page template definitions.</param>
        /// <param name="propertiesProvider">Default widget properties provider.</param>
        internal KenticoEditableAreasConfigurationController(IPageRetriever pageRetriever,
                                         IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider,
                                         IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider,
                                         IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider,
                                         IComponentDefaultPropertiesProvider propertiesProvider)
        {
            this.pageRetriever = pageRetriever;
            this.widgetDefinitionProvider = widgetDefinitionProvider;
            this.sectionDefinitionProvider = sectionDefinitionProvider;
            this.pageTemplateDefinitionProvider = pageTemplateDefinitionProvider;
            this.propertiesProvider = propertiesProvider;
        }


        /// <summary>
        /// Gets configuration for specified page.
        /// </summary>
        /// <param name="pageId">Page ID.</param>
        [HttpGet]
        [ResponseType(typeof(PageBuilderConfiguration))]
        public HttpResponseMessage Get(int pageId)
        {
            try
            {
                var dataContext = new PageBuilderDataContext(pageId, Manager, pageRetriever);
                return Request.CreateResponse(HttpStatusCode.OK, dataContext.Configuration);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "GetConfiguration", ex);
                throw;
            }
        }


        private Guid GetEditingIdentifier()
        {
            var query = Request.GetQueryNameValuePairs();
            var pair = query.FirstOrDefault(p => p.Key.Equals(PageBuilderHelper.EDITING_INSTANCE_QUERY_NAME, StringComparison.InvariantCultureIgnoreCase));
            Guid.TryParse(pair.Value, out var editingInstanceIdentifier);

            return editingInstanceIdentifier;
        }


        private IPageBuilderConfigurationManager GetManager()
        {
            var editableAreasSerializer = new EditableAreasConfigurationSerializer();
            var pageTemplateSerializer = new PageTemplateConfigurationSerializer();
            var editingInstanceIdentifier = GetEditingIdentifier();
            var loader = new PageBuilderConfigurationSourceLoader(editingInstanceIdentifier, Service.Resolve<IPageBuilderConfigurationSourceLoader>());
            var conditionTypeDefinitionProvider = new ComponentDefinitionProvider<ConditionTypeDefinition>();
            var previewLinksConverter = new PreviewLinksConverter(SystemContext.ApplicationPath);

            return new PageBuilderConfigurationManager(editableAreasSerializer, pageTemplateSerializer, loader, widgetDefinitionProvider, sectionDefinitionProvider, pageTemplateDefinitionProvider, conditionTypeDefinitionProvider, previewLinksConverter);
        }


        /// <summary>
        /// Gets default properties for a widget type.
        /// </summary>
        /// <param name="typeIdentifier">Widget type identifier.</param>
        [HttpGet]
        [ResponseType(typeof(IWidgetProperties))]
        public HttpResponseMessage GetWidgetDefaultProperties(string typeIdentifier)
        {
            try
            {
                var definition = widgetDefinitionProvider.GetAll().FirstOrDefault(def => def.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
                var properties = propertiesProvider.Get<IWidgetProperties>(definition?.PropertiesType);
                return Request.CreateResponse(HttpStatusCode.OK, properties);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "GetWidgetDefaultProperties", ex);
                throw;
            }
        }


        /// <summary>
        /// Gets default properties for a section type.
        /// </summary>
        /// <param name="typeIdentifier">Section type identifier.</param>
        [HttpGet]
        [ResponseType(typeof(ISectionProperties))]
        public HttpResponseMessage GetSectionDefaultProperties(string typeIdentifier)
        {
            try
            {
                var definition = sectionDefinitionProvider.GetAll().FirstOrDefault(def => def.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
                var properties = propertiesProvider.Get<ISectionProperties>(definition?.PropertiesType);
                return Request.CreateResponse(HttpStatusCode.OK, properties);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "GetSectionDefaultProperties", ex);
                throw;
            }
        }


        /// <summary>
        /// Gets default properties for a page template type.
        /// </summary>
        /// <param name="typeIdentifier">Page template type identifier.</param>
        [HttpGet]
        [ResponseType(typeof(IPageTemplateProperties))]
        public HttpResponseMessage GetPageTemplateDefaultProperties(string typeIdentifier)
        {
            try
            {
                var definition = pageTemplateDefinitionProvider.GetAll().FirstOrDefault(def => def.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
                var properties = propertiesProvider.Get<IPageTemplateProperties>(definition?.PropertiesType);
                return Request.CreateResponse(HttpStatusCode.OK, properties);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "GetPageTemplateDefaultProperties", ex);
                throw;
            }
        }


        /// <summary>
        /// Sets configuration for specified page.
        /// </summary>
        [HttpPost]
        public HttpResponseMessage Set()
        {
            try
            {
                var instanceGuid = GetInstanceGuid();
                var configuration = Request.Content.ReadAsStringAsync().Result;
                Manager.Store(instanceGuid, configuration);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "SetConfiguration", ex);
                throw;
            }
        }


        /// <summary>
        /// Change template for specified page.
        /// </summary>
        [HttpPost]
        public HttpResponseMessage ChangeTemplate()
        {
            try
            {
                var instanceGuid = GetInstanceGuid();
                var templateIdentifier = Request.Content.ReadAsStringAsync().Result;
                Manager.ChangeTemplate(instanceGuid, templateIdentifier);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "ChangeTemplate", ex);
                throw;
            }
        }


        private Guid GetInstanceGuid()
        {
            if (!Guid.TryParse(Request.Headers.GetValues(BuilderConstants.INSTANCE_HEADER_NAME).FirstOrDefault(), out var instanceGuid))
            {
                throw new InvalidOperationException("Missing editing instance identifier in headers.");
            }

            return instanceGuid;
        }
    }
}
