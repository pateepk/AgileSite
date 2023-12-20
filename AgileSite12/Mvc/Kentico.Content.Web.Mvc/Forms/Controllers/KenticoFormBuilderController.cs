using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using CMS.SiteProvider;

using Kentico.Forms.Web.Attributes.Mvc;
using Kentico.Forms.Web.Mvc.FormComponents;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides an endpoint for serving a page with form builder.
    /// </summary>
    /// <seealso cref="RouteCollectionExtensions.MapFormBuilderRoutes"/>
    /// <exclude />
    [AuthorizeFormVirtualContext(PredefinedObjectType.BIZFORM, "EditForm")]
    [FormBuilderMvcExceptionFilter]
    public sealed class KenticoFormBuilderController : Controller
    {
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormBuilderController"/> class.
        /// </summary>
        public KenticoFormBuilderController()
        {
            formComponentDefinitionProvider = Service.Resolve<IFormComponentDefinitionProvider>();
        }


        /// <summary>
        /// Returns a page with initialized Form builder for a form with given <paramref name="formId"/>.
        /// </summary>
        public ActionResult Index(int formId)
        {
            var form = BizFormInfoProvider.GetBizFormInfo(formId);
            if (form == null)
            {
                var errorPageModel = new ErrorPage
                {
                    ErrorMessage = ResHelper.GetStringFormat("kentico.formbuilder.formnotfound", formId)
                };

                return View("~/Views/Shared/Kentico/FormBuilder/ErrorPage.cshtml", errorPageModel);
            }

            var model = new FormBuilderPage
            {
                Id = formId,
                AvailableFormBuilderComponents = JsonConvert.SerializeObject(GetAvailableFormBuilderComponents(form)),
                FormsLimitExceeded = !BizFormInfoProvider.LicenseVersionCheck(SiteContext.CurrentSite.DomainName, FeatureEnum.BizForms, ObjectActionEnum.Edit)
            };

            return View("~/Views/Shared/Kentico/FormBuilder/Index.cshtml", model);
        }


        private IEnumerable<string> GetAvailableFormBuilderComponents(BizFormInfo form)
        {
            return formComponentDefinitionProvider
                .GetAll()
                .Filter(FormBuilderFilters.FormComponents, new FormComponentFilterContext(form))
                .Where(component => component.IsAvailableInFormBuilderEditor && component.ComponentType.GetCustomAttributes<CheckLicenseAttribute>(true).All(att => att.HasValidLicense()))
                .Select(component => component.Identifier);
        }
    }
}