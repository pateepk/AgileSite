using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;

using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.OnlineForms;
using CMS.SiteProvider;

using Kentico.Forms.Web.Mvc.Widgets.Internal;
using Kentico.Forms.Web.Mvc.Widgets;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;

[assembly: RegisterWidget(KenticoFormWidgetController.WIDGET_IDENTIFIER, typeof(KenticoFormWidgetController), "{$kentico.formbuilder.widget.name$}", Description = "{$kentico.formbuilder.widget.description$}", IconClass = "icon-w-on-line-form")]

namespace Kentico.Forms.Web.Mvc.Widgets
{
    /// <summary>
    /// Form widget controller
    /// </summary>
    public class KenticoFormWidgetController : WidgetController<FormWidgetProperties>
    {
        private readonly IFormProvider formProvider;
        private readonly IFormComponentModelBinder formComponentModelBinder;
        private readonly IFormComponentVisibilityEvaluator formComponentVisibilityEvaluator;
        private readonly HashSet<string> mDependencyCacheKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Form widget identifier.
        /// </summary>
        public const string WIDGET_IDENTIFIER = "Kentico.FormWidget";


        /// <summary>
        /// Creates an instance of <see cref="KenticoFormWidgetController"/> class.
        /// </summary>
        public KenticoFormWidgetController()
            : this(Service.Resolve<IFormProvider>(), Service.Resolve<IFormComponentModelBinder>(), Service.Resolve<IFormComponentVisibilityEvaluator>())
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="KenticoFormWidgetController"/> class.
        /// </summary>
        /// <param name="formProvider">Provider for retrieving forms and its fields.</param>
        /// <param name="formComponentModelBinder">Binder for binding form data to form component.</param>
        /// <param name="formComponentVisibilityEvaluator">Evaluator for form component visibility.</param>
        public KenticoFormWidgetController(IFormProvider formProvider, IFormComponentModelBinder formComponentModelBinder, IFormComponentVisibilityEvaluator formComponentVisibilityEvaluator)
        {
            this.formProvider = formProvider;
            this.formComponentModelBinder = formComponentModelBinder;
            this.formComponentVisibilityEvaluator = formComponentVisibilityEvaluator;
        }


        /// <summary>
        /// Creates an instance of <see cref="KenticoFormWidgetController"/> class.
        /// </summary>
        /// <param name="formProvider">Provider for retrieving forms and its fields.</param>
        /// <param name="formComponentModelBinder">Binder for binding form data to form component.</param>
        /// <param name="formComponentVisibilityEvaluator">Evaluator for form component visibility.</param>
        /// <param name="propertiesRetriever">Retriever for widget properties.</param>
        /// <param name="currentPageRetriever">Retriever for current page where is the widget used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected KenticoFormWidgetController(IFormProvider formProvider,
                                    IFormComponentModelBinder formComponentModelBinder,
                                    IFormComponentVisibilityEvaluator formComponentVisibilityEvaluator,
                                    IComponentPropertiesRetriever<FormWidgetProperties> propertiesRetriever,
                                    ICurrentPageRetriever currentPageRetriever)
            : base(propertiesRetriever, currentPageRetriever)

        {
            this.formProvider = formProvider;
            this.formComponentModelBinder = formComponentModelBinder;
            this.formComponentVisibilityEvaluator = formComponentVisibilityEvaluator;
        }


        /// <summary>
        /// Default controller action
        /// </summary>
        [FormWidgetExceptionFilter]
        public ActionResult Index()
        {
            FormWidgetProperties properties = null;
            try
            {
                properties = GetProperties();
                var formInfo = BizFormInfoProvider.GetBizFormInfo(properties.SelectedForm, SiteContext.CurrentSiteID);
                List<SelectListItem> siteFormItems = null;
                var isEditMode = HttpContext.Kentico().PageBuilder().EditMode;

                if (isEditMode)
                {
                    var siteForms = GetSiteForms();
                    siteFormItems = CreateFormItems(siteForms);

                    if (formInfo == null)
                    {
                        return FormWidgetPartialView(formInfo, null, properties, new FormWidgetViewModel()
                        {
                            FormName = properties.SelectedForm,
                            SiteForms = siteFormItems
                        });
                    }
                }
                else if (formInfo == null)
                {
                    if (!String.IsNullOrEmpty(properties.SelectedForm))
                    {
                        LogMissingForm();
                    }

                    return new EmptyResult();
                }

                
                if (!DisableOutputCacheForFormWithSmartField(HttpContext.Response.Cache, formInfo.Form?.ContainsSmartField() ?? false))
                {
                    AddOutputCacheDependencies(HttpContext, formInfo);
                }


                var existingItem = GetExistingBizFormItem(formInfo);
                var components = formProvider.GetFormComponents(formInfo, new BizFormComponentContext() { FormIsSubmittable = !isEditMode} )
                                             .GetDisplayedComponents(ContactManagementContext.CurrentContact, formInfo, existingItem, formComponentVisibilityEvaluator)
                                             .ToList();
                var prefix = GenerateFormPrefix(properties.SelectedForm);

                ViewData.TemplateInfo.HtmlFieldPrefix = prefix;

                return FormWidgetPartialView(formInfo, components, properties, new FormWidgetViewModel
                {
                    FormName = properties.SelectedForm,
                    FormConfiguration = GetFormConfiguration(formInfo),
                    SiteForms = siteFormItems,
                    FormComponents = components,
                    FormPrefix = prefix,
                    SubmitButtonText = GetSubmitButtonText(formInfo.FormSubmitButtonText),
                    SubmitButtonImage = formInfo.FormSubmitButtonImage,
                    IsFormSubmittable = !isEditMode
                });
            }
            catch (Exception ex)
            {
                LogFormException(properties?.SelectedForm, ex);

                return GetErrorMessageIfAny(properties?.SelectedForm);
            }
        }


        /// <summary>
        /// Action processing submission of a form.
        /// </summary>
        /// <param name="formName">Name of the form which is submitted.</param>
        /// <param name="prefix">Prefix of the keys in the form collection. Value without trailing dot is expected. If null, no prefix is assumed.</param>
        /// <param name="isUpdating">Decides whether this request updates form's markup e.g. due to visibility conditions.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FormSubmit(string formName, string prefix, [UpdatableFormModelBinder]bool isUpdating)
        {
            try
            {
                return FormSubmitInternal(formName, prefix, isUpdating);
            }
            catch (Exception ex)
            {
                LogFormException(formName, ex);

                return GetErrorMessage(ResHelper.GetString("kentico.formbuilder.submit.error"));
            }
        }


        internal ActionResult FormSubmitInternal(string formName, string prefix, bool isUpdating)
        {
            var formInfo = BizFormInfoProvider.GetBizFormInfo(formName, SiteContext.CurrentSiteID);
            var binder = GetModelBinder(formInfo, prefix);

            var modelBindingContext = GetModelBindingContext(formInfo);
            var components = binder.BindModel(ControllerContext, modelBindingContext) as List<FormComponent>;

            ViewData.SetFormSubmit();

            if (!ModelState.IsValid || isUpdating)
            {
                // If form is updating then clear all validation errors
                if (isUpdating)
                {
                    ModelState.Values.ToList().ForEach(v => v.Errors.Clear());
                }

                ViewData.TemplateInfo.HtmlFieldPrefix = prefix;

                return FormWidgetPartialView(formInfo, components, null, new FormWidgetViewModel
                {
                    FormName = formName,
                    FormConfiguration = GetFormConfiguration(formInfo),
                    FormComponents = components,
                    FormPrefix = prefix,
                    SubmitButtonText = GetSubmitButtonText(formInfo.FormSubmitButtonText),
                    SubmitButtonImage = formInfo.FormSubmitButtonImage
                });
            }

            var items = BizFormItemProvider.GetItems(DataClassInfoProvider.GetClassName(formInfo.FormClassID));
            if (items.HasExistingItemForContact(formInfo, ContactManagementContext.CurrentContact?.ContactGUID, out var formData))
            {
                formData = formProvider.UpdateFormData(formInfo, formData.ItemID, components, ContactManagementContext.CurrentContact?.ContactGUID);
            }
            else
            {
                formData = formProvider.SetFormData(formInfo, components, ContactManagementContext.CurrentContact?.ContactGUID);
            }

            var uploadFields = components.Where(c => c.BaseProperties.DataType == BizFormUploadFile.DATATYPE_FORMFILE).Select(c => c.Name);
            formProvider.SendEmails(formInfo, formData, uploadFields);

            if (!String.IsNullOrEmpty(formInfo.FormRedirectToUrl))
            {
                var url = GetMacroResolver(formInfo, formData, encodeResolvedValues: false).ResolveMacros(formInfo.FormRedirectToUrl);
                return Json(new
                {
                    redirectTo = Url.Content(url)
                });
            }

            if (!String.IsNullOrEmpty(formInfo.FormDisplayText))
            {
                var text = GetMacroResolver(formInfo, formData, encodeResolvedValues: true).ResolveMacros(formInfo.FormDisplayText);
                var textTag = new TagBuilder("div") { InnerHtml = text };

                textTag.AddCssClass("formwidget-submit-text");

                return Content(textTag.ToString());
            }

            var displayedComponents = formProvider.GetFormComponents(formInfo).GetDisplayedComponents(ContactManagementContext.CurrentContact, formInfo, formData, formComponentVisibilityEvaluator);

            var newPrefix = GenerateFormPrefix(formName);
            ViewData.TemplateInfo.HtmlFieldPrefix = newPrefix;

            return FormWidgetPartialView(formInfo, displayedComponents, null, new FormWidgetViewModel
            {
                FormName = formName,
                FormConfiguration = GetFormConfiguration(formInfo),
                FormComponents = displayedComponents.ToList(),
                FormPrefix = newPrefix,
                SubmitButtonText = GetSubmitButtonText(formInfo.FormSubmitButtonText),
                SubmitButtonImage = formInfo.FormSubmitButtonImage
            });
        }


        private void AddOutputCacheDependencies(HttpContextBase httpContext, BizFormInfo form)
        {
            if (httpContext.Kentico().PageBuilder().EditMode)
            {
                return;
            }

            var dependencyCacheKey = $"{form.TypeInfo.ObjectType}|byid|{form.FormID}";
            AddCacheItemDependency(dependencyCacheKey, httpContext.Response);
        }


        private void AddCacheItemDependency(string dependencyCacheKey, HttpResponseBase httpResponse)
        {
            if (!mDependencyCacheKeys.Contains(dependencyCacheKey))
            {
                mDependencyCacheKeys.Add(dependencyCacheKey);
                CacheHelper.EnsureDummyKey(dependencyCacheKey);
                httpResponse.AddCacheItemDependency(dependencyCacheKey);
            }
        }


        /// <summary>
        /// Disables output cache for pages with smart-fields, we can't ensure vary by smart field collection or contacts
        /// </summary>
        /// <returns><c>true</c> when output cache disabled; otherwise <c>false</c></returns>
        private bool DisableOutputCacheForFormWithSmartField(HttpCachePolicyBase httpCachePolicy, bool formContainsSmartField)
        {
            if (formContainsSmartField)
            {
                httpCachePolicy.SetNoServerCaching();
                return true;
            }

            return false;
        }


        private PartialViewResult FormWidgetPartialView(BizFormInfo formInfo, IEnumerable<FormComponent> components, FormWidgetProperties properties, FormWidgetViewModel model)
        {
            if (components?.Any() == true)
            {
                var formWidgetRenderingConfiguration = FormWidgetRenderingConfiguration.GetConfigurationInternal(formInfo, components, properties, FormWidgetRenderingConfiguration.Default);
                ViewData.AddFormWidgetRenderingConfiguration(formWidgetRenderingConfiguration);
            }

            return PartialView("~/Views/Shared/Kentico/Widgets/_FormWidget.cshtml", model);
        }


        private FormBuilderConfiguration GetFormConfiguration(BizFormInfo formInfo)
        {
            return Service.Resolve<IFormBuilderConfigurationRetriever>().Retrieve(formInfo);
        }


        /// <summary>
        /// Returns <see cref="MacroResolver"/> instance containing form fields data.
        /// </summary>
        protected virtual MacroResolver GetMacroResolver(BizFormInfo formInfo, BizFormItem formItem, bool encodeResolvedValues)
        {
            var resolver = MacroResolver.GetInstance().CreateChild();
            resolver.Settings.EncodeResolvedValues = encodeResolvedValues;

            var fields = formInfo.Form.GetFields(true, true);
            foreach (var field in fields)
            {
                var name = field.Name;
                resolver.SetNamedSourceData(name, formItem.GetValue(name));
            }

            return resolver;
        }


        internal virtual IModelBinder GetModelBinder(BizFormInfo formInfo, string prefix)
        {
            return new FormBuilderModelBinder(formInfo, formProvider, formComponentModelBinder, formComponentVisibilityEvaluator, prefix);
        }


        private static BizFormItem GetExistingBizFormItem(BizFormInfo formInfo)
        {
            var formClassName = DataClassInfoProvider.GetClassName(formInfo.FormClassID);

            if (formClassName == null)
            {
                return null;
            }

            return BizFormItemProvider.GetItems(formClassName)?.GetExistingItemForContact(formInfo, ContactManagementContext.CurrentContact?.ContactGUID);
        }


        private ModelBindingContext GetModelBindingContext(BizFormInfo formInfo)
        {
            return new FormBuilderModelBindingContext
            {
                Contact = ContactManagementContext.CurrentContact,
                ExistingItem = GetExistingBizFormItem(formInfo)
            };
        }


        private List<BizFormInfo> GetSiteForms()
        {
            return BizFormInfoProvider.GetBizForms()
                .Columns("FormSiteID", "FormName", "FormDisplayName", "FormDevelopmentModel")
                .WhereEquals("FormDevelopmentModel", (int)FormDevelopmentModelEnum.Mvc)
                .WhereEquals("FormSiteID", SiteContext.CurrentSiteID)
                .ToList();
        }


        private List<SelectListItem> CreateFormItems(List<BizFormInfo> forms)
        {
            return forms
                .Select(f => new SelectListItem() { Text = f.FormDisplayName, Value = f.FormName })
                .ToList();
        }


        internal virtual string GenerateFormPrefix(string formName)
        {
            var guid = Guid.NewGuid().ToString().Substring(0, 4);
            var prefix = $"form-{formName}-{guid}";

            return prefix;
        }


        private string GetSubmitButtonText(string text)
        {
            return String.IsNullOrEmpty(text) ? ResHelper.GetString("general.submit") : ResHelper.LocalizeString(text);
        }


        private void LogFormException(string formName, Exception ex)
        {
            EventLogProvider.LogException("FormWidget", "EXCEPTION", ex, SiteContext.CurrentSiteID,
                additionalMessage: $"Selected form '{formName}' is not in valid state. This event is logged only once.",
                loggingPolicy: LoggingPolicy.ONLY_ONCE);
        }


        private void LogMissingForm()
        {
            EventLogProvider.LogEvent(EventType.WARNING, "FormWidget", "GETBIZFORMINFO",
                eventDescription: "Selected form was not found. Verify that it exists on the given web site. This event is logged only once.",
                loggingPolicy: LoggingPolicy.ONLY_ONCE);
        }


        internal ActionResult GetErrorMessageIfAny(string formName)
        {
            // Show additional information if in edit mode.
            if (HttpContext.Kentico().PageBuilder().EditMode)
            {
                return GetErrorMessage(ResHelper.GetStringFormat("kentico.formbuilder.error", formName));
            }

            return new EmptyResult();
        }


        private ActionResult GetErrorMessage(string message)
        {
            var spanTag = new TagBuilder("span")
            {
                InnerHtml = message
            };
            spanTag.AddCssClass("formwidget-error");

            return Content(spanTag.ToString());
        }
    }
}