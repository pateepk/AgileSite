using System;
using System.ComponentModel;
using System.Web.Http;
using System.Web.Http.Description;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.HttpAttributes;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving registered page templates.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [UseCamelCasePropertyNamesContractResolver]
    public sealed class KenticoPageTemplatesViewModelController : ApiController
    {
        private readonly IPageTemplatesViewModelBuilder viewModelBuilder;
        private readonly IVirtualContextPageRetriever pageRetriever;


        /// <summary>
        /// Creates an instance of <see cref="KenticoPageTemplatesViewModelController"/>.
        /// </summary>
        public KenticoPageTemplatesViewModelController()
            : this(
                new PageTemplatesViewModelBuilder(Service.Resolve<IComponentLocalizationService>(), new PageTemplateDefinitionProvider(), new CustomPageTemplateProvider(Service.Resolve<ISiteService>())), 
                new VirtualContextPageRetriever())
        {

        }


        /// <summary>
        /// Creates an instance of <see cref="KenticoPageTemplatesViewModelController"/>.
        /// </summary>
        /// <param name="viewModelBuilder">Builder for view model.</param>
        /// <param name="pageRetriever">Provides current page in virtual context.</param>
        internal KenticoPageTemplatesViewModelController(IPageTemplatesViewModelBuilder viewModelBuilder, IVirtualContextPageRetriever pageRetriever)
        {
            this.viewModelBuilder = viewModelBuilder ?? throw new ArgumentNullException(nameof(viewModelBuilder));
            this.pageRetriever = pageRetriever ?? throw new ArgumentNullException(nameof(pageRetriever));
        }


        /// <summary>
        /// Gets definitions of registered page templates filtered based on restrictions of filters.
        /// </summary>
        [HttpGet]
        [CheckPagePermissions(PermissionsEnum.Create)]
        public IHttpActionResult GetFiltered(string pagetype, string culture)
        {
            try
            {
                var filterContext = new PageTemplateFilterContext(pageRetriever.Retrieve(), pagetype, culture);
                return Ok(viewModelBuilder.Build(filterContext));
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("PageTemplates", "GetFiltered", ex);
                throw;
            }
        }
    }
}
