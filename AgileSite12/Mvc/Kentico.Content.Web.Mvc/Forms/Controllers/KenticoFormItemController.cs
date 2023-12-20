using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.OnlineForms;

using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Attributes.Mvc;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoint for rendering form builder forms with submitted values.
    /// </summary>
    [FormBuilderMvcExceptionFilter]
    public class KenticoFormItemController : Controller
    {
        private readonly IPathDecorator formBuilderPathDecorator;
        private readonly IFormProvider formProvider;
        private readonly IFormComponentModelBinder formComponentModelBinder;
        private readonly IFormComponentVisibilityEvaluator formComponentVisibilityEvaluator;
        private readonly IBizFormMailSenderFactory bizFormMailSenderFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormItemController"/> class.
        /// </summary>
        public KenticoFormItemController()
        : this(new FormBuilderPathDecorator(), Service.Resolve<IRecordedDataFormProvider>(), Service.Resolve<IFormComponentModelBinder>(), Service.Resolve<IFormComponentVisibilityEvaluator>(), Service.Resolve<IBizFormMailSenderFactory>())
        { }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormItemController"/> class.
        /// </summary>
        /// <param name="formBuilderPathDecorator">Form builder path decorator.</param>
        /// <param name="formProvider">Provider for form components retrieval.</param>
        /// <param name="formComponentModelBinder">Form component model binder.</param>
        /// <param name="formComponentVisibilityEvaluator">Evaluator for form component visibility.</param>
        /// <param name="bizFormMailSenderFactory">Factory used to create notification and autoresponder emails sender.</param>
        /// <exception cref="ArgumentNullException">Is thrown when at least of the parameters is null.</exception>
        internal KenticoFormItemController(IPathDecorator formBuilderPathDecorator, IRecordedDataFormProvider formProvider, IFormComponentModelBinder formComponentModelBinder, IFormComponentVisibilityEvaluator formComponentVisibilityEvaluator, IBizFormMailSenderFactory bizFormMailSenderFactory)
        {
            this.formBuilderPathDecorator = formBuilderPathDecorator ?? throw new ArgumentNullException(nameof(formBuilderPathDecorator));
            this.formProvider = formProvider ?? throw new ArgumentNullException(nameof(formProvider));
            this.formComponentModelBinder = formComponentModelBinder ?? throw new ArgumentNullException(nameof(formComponentModelBinder));
            this.formComponentVisibilityEvaluator = formComponentVisibilityEvaluator ?? throw new ArgumentNullException(nameof(formComponentVisibilityEvaluator));
            this.bizFormMailSenderFactory = bizFormMailSenderFactory ?? throw new ArgumentNullException(nameof(bizFormMailSenderFactory));
        }


        /// <summary>
        /// Action renders a page containing components of form specified by <paramref name="formId"/> and their values.
        /// </summary>
        /// <param name="formId">ID of the form to be rendered.</param>
        /// <param name="itemId">ID of the submitted form record containing data.</param>
        [AuthorizeFormVirtualContext(ModuleName.ACTIVITIES, "ReadActivities")]
        public ActionResult PreviewItem(int formId, int itemId)
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

            var formItem = GetFormItem(itemId, form.FormClassID);

            if (formItem == null)
            {
                var errorPageModel = new ErrorPage
                {
                    ErrorMessage = ResHelper.GetStringFormat("kentico.formbuilder.formrecordnotfound", itemId, formId)
                };

                return View("~/Views/Shared/Kentico/FormBuilder/ErrorPage.cshtml", errorPageModel);
            }

            var model = formProvider.GetFormComponents(form, new BizFormComponentContext() { FormIsSubmittable = false });

            BindValues(formItem, model);

            return View("~/Views/Shared/Kentico/FormBuilder/FormItemPreview.cshtml", model);
        }


        /// <summary>
        /// Action renders page for editing or creating form record.
        /// </summary>
        /// <param name="formId">Identifier of the form whose record is being edited/created.</param>
        /// <param name="itemId">
        /// Identifier of the form record to be edited. If less or equal to 0, new form record is created.
        /// </param>
        [AuthorizeFormVirtualContext(ModuleName.BIZFORM, "ReadData"), AuthorizeFormVirtualContext(ModuleName.BIZFORM, "EditData")]
        public ActionResult EditItem(int formId, int? itemId)
        {
            var visibleComponents = new List<FormComponent>();

            var formInfo = BizFormInfoProvider.GetBizFormInfo(formId);
            var formComponents = formProvider.GetFormComponents(formInfo);

            var formItemId = itemId.GetValueOrDefault();
            var formItem = formItemId > 0 ? GetFormItem(formItemId, formInfo.FormClassID) : null;

            if (formItem != null)
            {
                BindValues(formItem, formComponents);
            }

            foreach (var component in formComponents)
            {
                if (formComponentVisibilityEvaluator.IsComponentVisible(component, visibleComponents))
                {
                    visibleComponents.Add(component);
                }
            }

            var model = new FormItemEdit
            {
                FormId = formId,
                FormItemId = formItemId,
                FormComponents = visibleComponents,
                SubmitUrl = formBuilderPathDecorator.Decorate(Url.RouteUrl(FormBuilderRoutes.FORM_ITEM_EDIT_SUBMIT_ROUTE_NAME)),
                SendNotificationEmail = formItem == null,
                SendAutoResponderEmail = formItem == null
            };

            return View("~/Views/Shared/Kentico/FormBuilder/FormItemEdit.cshtml", model);
        }


        /// <summary>
        /// Action processes submitted forms from <see cref="EditItem(int, int?)"/> action.
        /// </summary>
        /// <param name="formId">Identifier of the submitted form.</param>
        /// <param name="formItemId">Identifier of the form record to be edited.</param>
        /// <param name="sendNotificationEmail">If true notification email is sent.</param>
        /// <param name="sendAutoResponderEmail">If true auto responder email is sent.</param>
        /// <param name="isUpdating">Decides whether this request updates form's markup e.g. due to visibility conditions.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFormVirtualContext(ModuleName.BIZFORM, "ReadData"), AuthorizeFormVirtualContext(ModuleName.BIZFORM, "EditData")]
        public ActionResult Submit(int formId, int formItemId, bool sendNotificationEmail, bool sendAutoResponderEmail, [UpdatableFormModelBinder]bool isUpdating)
        {
            var formInfo = BizFormInfoProvider.GetBizFormInfo(formId);
            var binder = GetModelBinder(formInfo);

            // Bind model without binding context to ignore the smart fields feature
            var components = binder.BindModel(ControllerContext, null) as List<FormComponent>;

            var model = new FormItemEdit
            {
                FormId = formId,
                FormItemId = formItemId,
                FormComponents = components,
                SendNotificationEmail = sendNotificationEmail,
                SendAutoResponderEmail = sendAutoResponderEmail,
                SubmitUrl = formBuilderPathDecorator.Decorate(Url.RouteUrl(FormBuilderRoutes.FORM_ITEM_EDIT_SUBMIT_ROUTE_NAME))
            };

            if (isUpdating)
            {
                // If form is updating then clear all validation errors
                ModelState.Clear();

                return View("~/Views/Shared/Kentico/FormBuilder/FormItemEditForm.cshtml", model);
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Shared/Kentico/FormBuilder/FormItemEditForm.cshtml", model);
            }

            BizFormItem formItem;

            if (formItemId > 0)
            {
                formItem = formProvider.UpdateFormData(formInfo, formItemId, components, null);
            }
            else
            {
                formItem = formProvider.SetFormData(formInfo, components, null);
            }

            var uploadFields = components.Where(c => c.BaseProperties.DataType == BizFormUploadFile.DATATYPE_FORMFILE).Select(c => c.Name);
            var sender = bizFormMailSenderFactory.GetFormMailSender(formInfo, formItem, uploads: uploadFields);

            if (model.SendNotificationEmail)
            {
                try
                {
                    sender.SendNotificationEmail();
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("BizForm", "Sending notification failed", ex);
                }
            }
            if (model.SendAutoResponderEmail)
            {
                try
                {
                    sender.SendConfirmationEmail();
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("BizForm", "Sending autoresponder failed", ex);
                }
            }

            ModelState.Remove(nameof(formItemId));
            model.FormItemId = formItem.ItemID;
            model.ChangesSavedMessage = true;

            return PartialView("~/Views/Shared/Kentico/FormBuilder/FormItemEditForm.cshtml", model);
        }


        /// <summary>
        /// Binds values from the database onto form components.
        /// </summary>
        /// <param name="item">Item containing data.</param>
        /// <param name="components">Collection of form components to bind data onto.</param>
        internal void BindValues(BizFormItem item, List<FormComponent> components)
        {
            foreach (var component in components)
            {
                var value = item.GetValue(component.Name);
                var componentValueType = component.GetType().FindTypeByGenericDefinition(typeof(FormComponent<,>)).GetGenericArguments()[1];
                var isNullable = componentValueType.IsClass || componentValueType.FindTypeByGenericDefinition(typeof(Nullable<>)) != null;

                value = DataTypeManager.ConvertToSystemType(TypeEnum.Field, component.BaseProperties.DataType, value, nullIfDefault: isNullable);

                component.SetObjectValue(value);
            }
        }


        internal virtual BizFormItem GetFormItem(int itemId, int formClassId)
        {
            var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(formClassId);
            return BizFormItemProvider.GetItem(itemId, dataClassInfo.ClassName);
        }


        internal virtual IModelBinder GetModelBinder(BizFormInfo formInfo)
        {
            return new FormBuilderModelBinder(formInfo, formProvider, formComponentModelBinder, formComponentVisibilityEvaluator);
        }
    }
}
