using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains methods for forms and their fields retrieval.
    /// </summary>
    public class FormProvider : IFormProvider
    {
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;
        private readonly IFormComponentActivator formComponentActivator;
        private readonly IFormComponentPropertiesMapper formComponentPropertiesMapper;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormProvider"/> class.
        /// </summary>
        /// <param name="formComponentDefinitionProvider">Provider for registered form components retrieval.</param>
        /// <param name="formComponentActivator">Activator for form components.</param>
        /// <param name="formComponentPropertiesMapper">Mapper for form component properties.</param>
        public FormProvider(IFormComponentDefinitionProvider formComponentDefinitionProvider, IFormComponentActivator formComponentActivator, IFormComponentPropertiesMapper formComponentPropertiesMapper)
        {
            this.formComponentDefinitionProvider = formComponentDefinitionProvider ?? throw new ArgumentNullException(nameof(formComponentDefinitionProvider));
            this.formComponentActivator = formComponentActivator ?? throw new ArgumentNullException(nameof(formComponentActivator));
            this.formComponentPropertiesMapper = formComponentPropertiesMapper ?? throw new ArgumentNullException(nameof(formComponentPropertiesMapper));
        }


        /// <summary>
        /// Gets <see cref="FormInfo"/> for the specified <paramref name="bizFormInfo"/>.
        /// </summary>
        /// <param name="bizFormInfo">Form to return <see cref="FormInfo"/> for.</param>
        /// <returns>Returns an instance of <see cref="FormInfo"/> which corresponds to form given.</returns>
        /// <remarks>Returned instance of <see cref="FormInfo"/> should not be modified as it is cached within <see cref="FormHelper"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> is null.</exception>
        protected internal virtual FormInfo GetFormInfo(BizFormInfo bizFormInfo)
        {
            if (bizFormInfo == null)
            {
                throw new ArgumentNullException(nameof(bizFormInfo));
            }

            DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(bizFormInfo.FormClassID);
            FormInfo formInfo = FormHelper.GetFormInfo(dataClassInfo.ClassName, false);

            return formInfo;
        }


        /// <summary>
        /// Gets a list of form components which represent the model to be rendered in a view.
        /// </summary>
        /// <param name="bizFormInfo">Biz form for which to return a list of corresponding form components.</param> 
        /// <param name="bizFormComponentContext">
        /// Biz form component context, which is set in created components. 
        /// If <see cref="BizFormComponentContext.FormInfo"/> is null, it is set to <paramref name="bizFormInfo"/>.
        /// </param>
        /// <returns>Returns a list of form components.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> is null.</exception>
        public virtual List<FormComponent> GetFormComponents(BizFormInfo bizFormInfo, BizFormComponentContext bizFormComponentContext = null)
        {
            if (bizFormInfo == null)
            {
                throw new ArgumentNullException(nameof(bizFormInfo));
            }

            return GetFormComponentsInternal(bizFormInfo, field => field.Visible, true, bizFormComponentContext ?? new BizFormComponentContext());
        }


        /// <summary>
        /// Gets a list of form components which is based on given <paramref name="bizFormInfo"/> and <paramref name="predicate"/>.
        /// </summary>
        /// <param name="bizFormInfo">Biz form for which to return a list of corresponding form components</param>
        /// <param name="predicate">Function for filtering form fields components of which will be returned</param>
        /// <param name="analyzeDependency">Indicates if components should be marked when having any dependency</param>
        /// <param name="bizFormComponentContext">
        /// Biz form component context, which is set in created components. 
        /// If <see cref="BizFormComponentContext.FormInfo"/> is null, it is set to <paramref name="bizFormInfo"/>.
        /// </param>
        private List<FormComponent> GetFormComponentsInternal(BizFormInfo bizFormInfo, Func<FormFieldInfo, bool> predicate,
            bool analyzeDependency, BizFormComponentContext bizFormComponentContext)
        {
            var formInfo = GetFormInfo(bizFormInfo);
            var formComponents = new List<FormComponent>();
            var fieldsHavingDependingField = new HashSet<Guid>();

            bizFormComponentContext.FormInfo = bizFormComponentContext.FormInfo ?? bizFormInfo;

            foreach (var field in formInfo.GetFields<FormFieldInfo>().Where(predicate))
            {
                var fieldContext = new BizFormComponentContext();
                bizFormComponentContext.CopyTo(fieldContext);
                var component = CreateFormComponent(field, fieldContext);

                if (analyzeDependency)
                {
                    AnalyzeDependingField(component, fieldsHavingDependingField);
                }

                formComponents.Add(component);
            }

            if (analyzeDependency)
            {
                MarkFieldsHavingDependingField(formComponents, fieldsHavingDependingField);
            }

            return formComponents;
        }


        /// <summary>
        /// Creates a new form component instance based on its description in the form field.
        /// </summary>
        /// <param name="formFieldInfo">Form field describing the form component to create.</param>
        /// <param name="context">Contextual information specifying where the form component is being used.</param>
        /// <returns>Returns an instance of the form component as described in the form field.</returns>
        /// <remarks>
        /// A component instance is created using the <see cref="FormComponentActivator"/> and <see cref="IFormComponentPropertiesMapper"/>
        /// passed when initializing this provider. A proper <see cref="FormComponent.Name"/> is filled in the created component
        /// and <see cref="FormComponent.SetObjectValue(object)"/> method is called to set default value.
        /// </remarks>
        protected virtual FormComponent CreateFormComponent(FormFieldInfo formFieldInfo, BizFormComponentContext context)
        {
            var definition = formComponentDefinitionProvider.Get(formComponentPropertiesMapper.GetComponentIdentifier(formFieldInfo));
            var properties = formComponentPropertiesMapper.FromFieldInfo(formFieldInfo);

            var component = formComponentActivator.CreateFormComponent(definition, properties, context);
            component.Name = formFieldInfo.Name;

            var defaultValue = properties.GetDefaultValue();
            if (defaultValue != null)
            {
                component.SetObjectValue(defaultValue);
            }

            return component;
        }


        /// <summary>
        /// Tests whether <paramref name="formComponent"/> has a visibility condition of type <see cref="IAnotherFieldVisibilityCondition"/>
        /// and if so, adds the target component's GUID to <paramref name="fieldsHavingDependingField"/>.
        /// </summary>
        private void AnalyzeDependingField(FormComponent formComponent, HashSet<Guid> fieldsHavingDependingField)
        {
            if (formComponent.BaseProperties.VisibilityConditionConfiguration?.VisibilityCondition is IAnotherFieldVisibilityCondition anotherFieldVisibilityCondition)
            {
                fieldsHavingDependingField.Add(anotherFieldVisibilityCondition.DependeeFieldGuid);
            }
        }


        /// <summary>
        /// Sets <see cref="FormComponent.HasDependingFields"/> flag of all <paramref name="formComponents"/> based on whether their GUID is contained in <paramref name="fieldsHavingDependingField"/>.
        /// </summary>
        private void MarkFieldsHavingDependingField(List<FormComponent> formComponents, HashSet<Guid> fieldsHavingDependingField)
        {
            formComponents.ForEach(c => c.HasDependingFields = fieldsHavingDependingField.Contains(c.BaseProperties.Guid));
        }


        /// <summary>
        /// Sets data of a form represented by a list of its components.
        /// </summary>
        /// <remarks>
        /// The <see cref="BizFormComponentContext.SaveBizFormItem"/> event is invoked on all form components which have been initialized using an instance of <see cref="BizFormComponentContext"/>.
        /// </remarks>
        /// <param name="bizFormInfo">Biz form whose data are to be set.</param>
        /// <param name="formComponents">Form components containing values to be set.</param>
        /// <param name="contactGuid">Guid of current contact, can be null.</param>
        /// <returns>Returns the biz form item set.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> or <paramref name="formComponents"/> is null.</exception>
        public virtual BizFormItem SetFormData(BizFormInfo bizFormInfo, List<FormComponent> formComponents, Guid? contactGuid)
        {
            if (bizFormInfo == null)
            {
                throw new ArgumentNullException(nameof(bizFormInfo));
            }
            if (formComponents == null)
            {
                throw new ArgumentNullException(nameof(formComponents));
            }

            DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(bizFormInfo.FormClassID);
            BizFormItem bizFormItem = BizFormItem.New(dataClassInfo.ClassName);

            return PersistFormData(bizFormInfo, bizFormItem, formComponents, item => item.Insert(), contactGuid);
        }


        /// <summary>
        /// Updates form data of already existing <see cref="BizFormItem" />.
        /// </summary>
        /// <remarks>
        /// The <see cref="BizFormComponentContext.SaveBizFormItem"/> event is invoked on all form components which have been initialized using an instance of <see cref="BizFormComponentContext"/>.
        /// </remarks>
        /// <param name="bizFormInfo">Form to be updated.</param>
        /// <param name="bizFormItemId">Identifier of form record to be updated.</param>
        /// <param name="formComponents">Form components containing values to be set.</param>
        /// <param name="contactGuid">Guid of current contact, can be null.</param>
        /// <returns>Returns the updated biz form item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> or <paramref name="formComponents"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bizFormItemId"/> does not specify an existing <see cref="BizFormItem"/>.</exception>
        public virtual BizFormItem UpdateFormData(BizFormInfo bizFormInfo, int bizFormItemId, List<FormComponent> formComponents, Guid? contactGuid)
        {
            if (bizFormInfo == null)
            {
                throw new ArgumentNullException(nameof(bizFormInfo));
            }
            if (formComponents == null)
            {
                throw new ArgumentNullException(nameof(formComponents));
            }

            DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(bizFormInfo.FormClassID);
            BizFormItem bizFormItem = BizFormItemProvider.GetItem(bizFormItemId, dataClassInfo.ClassName);

            if (bizFormItem == null)
            {
                throw new ArgumentOutOfRangeException($"Identifier {bizFormItem} does not specify an existing form.");
            }

            return PersistFormData(bizFormInfo, bizFormItem, formComponents, item => item.Update(), contactGuid);
        }


        /// <summary>
        /// Merge values of <paramref name="formComponents"/> into <paramref name="bizFormItem"/> and persist data using <paramref name="persistData"/> action.
        /// </summary>
        /// <remarks>
        /// The <see cref="BizFormComponentContext.SaveBizFormItem"/> event is invoked on all form components which have been initialized using an instance of <see cref="BizFormComponentContext"/>.
        /// </remarks>
        /// <param name="bizFormInfo">Biz form whose data are to be persisted.</param>
        /// <param name="bizFormItem">Data to be combined with values of <paramref name="formComponents"/> and persisted.</param>
        /// <param name="formComponents">Form components containing values to be combined with data in <paramref name="bizFormItem"/> and persisted.</param>
        /// <param name="persistData">Action to be used for persisting.</param>
        /// <param name="contactGuid">Guid of current contact, can be null.</param>
        /// <returns>Returns the persisted biz form item.</returns>
        private BizFormItem PersistFormData(BizFormInfo bizFormInfo, BizFormItem bizFormItem, List<FormComponent> formComponents, Action<BizFormItem> persistData, Guid? contactGuid)
        {
            SetBizFormItemData(bizFormItem, formComponents);

            if (contactGuid.HasValue)
            {
                SetContact(bizFormInfo.Form, bizFormItem, contactGuid.Value);
            }

            var eventArgs = new SaveBizFormItemEventArgs
            {
                FormItem = bizFormItem
            };

            // Extract distinct contexts from all components to invoke the saving event on them
            List<BizFormComponentContext> bizFormComponentContexts = new HashSet<BizFormComponentContext>(formComponents.Select(fc => fc.FormComponentContext as BizFormComponentContext).Where(ctx => ctx != null)).ToList();
            List<SaveBizFormItemHandler> saveBizFormItemHandlers = new List<SaveBizFormItemHandler>(bizFormComponentContexts.Count);
            try
            {
                foreach (var context in bizFormComponentContexts)
                {
                    var eventHandler = context.SaveBizFormItem.StartEvent(eventArgs);
                    saveBizFormItemHandlers.Add(eventHandler);
                }

                persistData(eventArgs.FormItem);

                saveBizFormItemHandlers.ForEach(h => h.FinishEvent());
            }
            finally
            {
                saveBizFormItemHandlers.ForEach(h => h.Dispose());
            }

            return bizFormItem;
        }


        /// <summary>
        /// Sends notification and autoresponder emails if the form is configured to do so.
        /// </summary>
        /// <param name="bizFormInfo">Online form for which an emails are to be send</param>
        /// <param name="bizFormItem">Form's submitted data which can be included within sent emails</param>
        /// <param name="fileUploadFieldNames">Names of fields containing file uploads to be included as attachments.</param>
        public virtual void SendEmails(BizFormInfo bizFormInfo, BizFormItem bizFormItem, IEnumerable<string> fileUploadFieldNames = null)
        {
            var senderFactory = Service.Resolve<IBizFormMailSenderFactory>();
            var sender = senderFactory.GetFormMailSender(bizFormInfo, bizFormItem, uploads: fileUploadFieldNames);

            sender.SendNotificationEmail();
            sender.SendConfirmationEmail();
        }


        /// <summary>
        /// Sets the current contact identifier to given <paramref name="item"/>.
        /// </summary>
        protected internal void SetContact(FormInfo formInfo, BizFormItem item, Guid contactGuid)
        {
            if (!SmartFieldLicenseHelper.HasLicense() || !formInfo.ContainsSmartField())
            {
                return;
            }

            item.SetValue(SmartFieldConstants.CONTACT_COLUMN_NAME, contactGuid);
        }


        /// <summary>
        /// Sets data from <paramref name="visibleFormComponents"/> to given <paramref name="formItem"/>.
        /// </summary>
        /// <param name="formItem">Form item to which data is set.</param>
        /// <param name="visibleFormComponents">Collection of visible form components from which to set data inside <paramref name="formItem"/>.</param>
        protected virtual void SetBizFormItemData(BizFormItem formItem, List<FormComponent> visibleFormComponents)
        {
            foreach (var formComponent in visibleFormComponents)
            {
                formItem.SetValue(formComponent.Name, formComponent.GetObjectValue());
            }

            // Ensure values in required hidden fields
            EnsureValueForHiddenRequiredFormComponents(formItem, visibleFormComponents);
        }


        /// <summary>
        /// Ensures correct values for hidden required form components.
        /// </summary>
        /// <param name="formItem">Form item to which data is set.</param>
        /// <param name="visibleFormComponents">Collection of visible form components from which to set data inside <paramref name="formItem"/>.</param>
        private void EnsureValueForHiddenRequiredFormComponents(BizFormItem formItem, List<FormComponent> visibleFormComponents)
        {
            var formInfo = GetFormInfo(formItem.BizFormInfo);

            // Get components for required hidden fields
            var visibleComponentNames = new HashSet<string>(visibleFormComponents.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
            var predicate = new Func<FormFieldInfo, bool>(f => f.Visible && !f.AllowEmpty && !visibleComponentNames.Contains(f.Name));

            var hiddenRequiredComponents = GetFormComponentsInternal(formItem.BizFormInfo, predicate, false, new BizFormComponentContext());
            foreach (var formComponent in hiddenRequiredComponents)
            {
                if (formItem.GetValue(formComponent.Name) == null)
                {
                    // Get component default value which may be defined within component API or properties
                    var componentValue = formComponent.GetObjectValue();

                    // If it's still null, use the default value of the component's value type
                    if (componentValue == null)
                    {
                        var fieldInfo = formInfo.GetFormField(formComponent.Name);

                        componentValue = GetComponentTypeDefaultValue(fieldInfo);
                    }

                    formItem.SetValue(formComponent.Name, componentValue);
                }
            }
        }


        private static object GetComponentTypeDefaultValue(FormFieldInfo fieldInfo)
        {
            var value = DataTypeManager.GetDataType(TypeEnum.Field, fieldInfo.DataType).ObjectDefaultValue;

            return DataTypeManager.ConvertToSystemType(TypeEnum.Field, fieldInfo.DataType, value);
        }
    }
}
