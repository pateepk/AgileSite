using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.HttpAttributes;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.PageBuilder.Web.Mvc.Personalization;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving the Page builder metadata.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [UsePageBuilderJsonSerializerSettings]
    [CheckPagePermissions(PermissionsEnum.Modify)]
    public sealed class KenticoComponentsMetadataController : ApiController
    {
        private readonly IComponentsMetadataBuilder builder;


        /// <summary>
        /// Creates an instance of <see cref="KenticoComponentsMetadataController"/> class.
        /// </summary>
        public KenticoComponentsMetadataController()
        {
            var administrationCultureCode = MembershipContext.AuthenticatedUser.PreferredUICultureCode;
            var markupUrlRetriever = new ComponentMarkupUrlRetriever(CMSHttpContext.Current, new EditModePathDecorator());
            var defaultPropertiesRetriever = new ComponentDefaultPropertiesUrlRetriever(CMSHttpContext.Current, new PreviewPathDecorator(false));
            var propertiesMarkupUrlRetriever = new ComponentPropertiesFormMarkupUrlRetriever(CMSHttpContext.Current, new PreviewPathDecorator(false, administrationCultureCode));
            var localizationService = Service.Resolve<IComponentLocalizationService>();
            var editablePropertiesCollector = Service.Resolve<IEditablePropertiesCollector>();
            var administrationMarkupUrlRetriever = new ComponentMarkupUrlRetriever(CMSHttpContext.Current, new EditModePathDecorator(administrationCultureCode));
            builder = new ComponentsMetadataBuilder(new ComponentDefinitionProvider<WidgetDefinition>(), 
                                                    new ComponentDefinitionProvider<SectionDefinition>(),
                                                    new ComponentDefinitionProvider<PageTemplateDefinition>(),
                                                    new ComponentDefinitionProvider<ConditionTypeDefinition>(), 
                                                    markupUrlRetriever,
                                                    administrationMarkupUrlRetriever,
                                                    defaultPropertiesRetriever,
                                                    propertiesMarkupUrlRetriever,
                                                    localizationService,
                                                    editablePropertiesCollector);
        }


        /// <summary>
        /// Creates an instance of <see cref="KenticoComponentsMetadataController"/> class.
        /// </summary>
        /// <param name="builder">Custom builder to get components configuration.</param>
        internal KenticoComponentsMetadataController(IComponentsMetadataBuilder builder)
        {
            this.builder = builder;
        }


        /// <summary>
        /// Gets metadata for all registered components.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(ComponentsMetadata))]
        public HttpResponseMessage GetAll()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, builder.GetAll());
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "GetMetadata", ex);
                throw;
            }
        }
    }
}
