using System;
using System.ComponentModel;
using System.Web.Http.Description;
using System.Web.Mvc;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Attributes;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Provides endpoint for page template selector dialog.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [CheckPagePermissions(PermissionsEnum.Modify)]
    public sealed class KenticoPageTemplatesSelectorDialogController : Controller
    {
        private readonly IPageTemplatesViewModelBuilder viewModelBuilder;
        private readonly IVirtualContextPageRetriever pageRetriever;
        private readonly IComponentLocalizationService localizationService;


        /// <summary>
        /// Creates an instance of <see cref="KenticoPageTemplatesSelectorDialogController"/>.
        /// </summary>
        public KenticoPageTemplatesSelectorDialogController()
            : this(
                new PageTemplatesViewModelBuilder(Service.Resolve<IComponentLocalizationService>(), new PageTemplateDefinitionProvider(), new CustomPageTemplateProvider(Service.Resolve<ISiteService>())),
                new VirtualContextPageRetriever(),
                Service.Resolve<IComponentLocalizationService>())
        {
        }


        internal KenticoPageTemplatesSelectorDialogController(IPageTemplatesViewModelBuilder viewModelBuilder, IVirtualContextPageRetriever pageRetriever, IComponentLocalizationService localizationService)
        {
            this.viewModelBuilder = viewModelBuilder ?? throw new ArgumentNullException(nameof(viewModelBuilder));
            this.pageRetriever = pageRetriever ?? throw new ArgumentNullException(nameof(pageRetriever));
            this.localizationService = localizationService;
        }


        /// <summary>
        /// Gets view for page templates selector.
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            var templates = GetAvailableTemplatesForPage();
            var infoMessage = GetInfoMessage();

            return View("~/Views/Shared/Kentico/PageBuilder/_PageTemplateSelectorDialog.cshtml", new PageTemplatesSelectorViewModel {
                Templates = templates,
                InfoMessage = infoMessage
            });
        }
                

        private PageTemplatesViewModel GetAvailableTemplatesForPage()
        {
            var page = pageRetriever.Retrieve();
            var filterContext = new PageTemplateFilterContext(page.Parent, page.ClassName, page.DocumentCulture);

            return viewModelBuilder.Build(filterContext);
        }


        private string GetInfoMessage()
        {            
            var link = DocumentationHelper.GetDocumentationTopicUrl("page_templates_using_mvc");
            return string.Format(localizationService.GetString("kentico.pagebuilder.template.changeInfoMessage"), link);
        }
    }
}
