using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.OnlineForms;

using Kentico.Forms.Web.Attributes.Mvc;
using Kentico.Forms.Web.Mvc.AnnotationExtensions;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the Form builder's properties panel markup as well as validating <see cref="FormComponent"/> properties.
    /// </summary>
    [AuthorizeFormVirtualContext(PredefinedObjectType.BIZFORM, "EditForm")]
    [PropertiesPanelExceptionFilter]
    public class KenticoFormBuilderPropertiesPanelController : Controller
    {
        private readonly IFormBuilderConfigurationSerializer formBuilderConfigurationSerializer;
        private readonly IEditablePropertiesCollector editablePropertiesCollector;
        private readonly IEditablePropertiesModelBinder editablePropertiesModelBinder;
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;
        private readonly IFormComponentActivator formComponentActivator;
        private readonly IValidationRuleActivator validationRuleActivator;
        private readonly IValidationRuleDefinitionProvider validationRuleDefinitionProvider;
        private readonly IFormProvider formProvider;
        private readonly IVisibilityConditionDefinitionProvider visibilityConditionDefinitionProvider;
        private readonly IVisibilityConditionActivator visibilityConditionActivator;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormBuilderPropertiesPanelController"/> class.
        /// </summary>
        public KenticoFormBuilderPropertiesPanelController()
            : this(Service.Resolve<IFormBuilderConfigurationSerializer>(), Service.Resolve<IEditablePropertiesCollector>(), Service.Resolve<IEditablePropertiesModelBinder>(),
                   Service.Resolve<IFormComponentDefinitionProvider>(), Service.Resolve<IFormComponentActivator>(), Service.Resolve<IValidationRuleActivator>(), Service.Resolve<IValidationRuleDefinitionProvider>(),
                   Service.Resolve<IFormProvider>(), Service.Resolve<IVisibilityConditionDefinitionProvider>(), Service.Resolve<IVisibilityConditionActivator>())
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormBuilderPropertiesPanelController"/> class.
        /// </summary>
        /// <param name="formBuilderConfigurationSerializer">Serializer to be used for configuration deserialization.</param>
        /// <param name="editablePropertiesCollector">Collects editable properties from a model.</param>
        /// <param name="editablePropertiesModelBinder">Binder responsible for binding form data to a model.</param>
        /// <param name="formComponentDefinitionProvider">Provider for retrieval of registered form component definitions for Form builder.</param>
        /// <param name="formComponentActivator">Creates form components and their properties.</param>
        /// <param name="validationRuleActivator">Creates validation rules.</param>
        /// <param name="validationRuleDefinitionProvider">Provider for retrieval of registered validation rule definitions for Form builder.</param>
        /// <param name="formProvider">Provides forms.</param>
        /// <param name="visibilityConditionDefinitionProvider">Provides <see cref="VisibilityConditionDefinition"/>.</param>
        /// <param name="visibilityConditionActivator">Creates visibility conditions and their properties.</param>
        internal KenticoFormBuilderPropertiesPanelController(IFormBuilderConfigurationSerializer formBuilderConfigurationSerializer, IEditablePropertiesCollector editablePropertiesCollector,
                                                      IEditablePropertiesModelBinder editablePropertiesModelBinder, IFormComponentDefinitionProvider formComponentDefinitionProvider,
                                                      IFormComponentActivator formComponentActivator, IValidationRuleActivator validationRuleActivator, IValidationRuleDefinitionProvider validationRuleDefinitionProvider,
                                                      IFormProvider formProvider, IVisibilityConditionDefinitionProvider visibilityConditionDefinitionProvider, IVisibilityConditionActivator visibilityConditionActivator)
        {
            this.formBuilderConfigurationSerializer = formBuilderConfigurationSerializer ?? throw new ArgumentNullException(nameof(formBuilderConfigurationSerializer));
            this.editablePropertiesCollector = editablePropertiesCollector ?? throw new ArgumentNullException(nameof(editablePropertiesCollector));
            this.editablePropertiesModelBinder = editablePropertiesModelBinder ?? throw new ArgumentNullException(nameof(editablePropertiesModelBinder));
            this.formComponentDefinitionProvider = formComponentDefinitionProvider ?? throw new ArgumentNullException(nameof(formComponentDefinitionProvider));
            this.formComponentActivator = formComponentActivator ?? throw new ArgumentNullException(nameof(formComponentActivator));
            this.validationRuleActivator = validationRuleActivator ?? throw new ArgumentNullException(nameof(KenticoFormBuilderPropertiesPanelController.validationRuleActivator));
            this.validationRuleDefinitionProvider = validationRuleDefinitionProvider ?? throw new ArgumentNullException(nameof(KenticoFormBuilderPropertiesPanelController.validationRuleDefinitionProvider));
            this.formProvider = formProvider ?? throw new ArgumentNullException(nameof(KenticoFormBuilderPropertiesPanelController.formProvider));
            this.visibilityConditionDefinitionProvider = visibilityConditionDefinitionProvider ?? throw new ArgumentNullException(nameof(KenticoFormBuilderPropertiesPanelController.visibilityConditionDefinitionProvider));
            this.visibilityConditionActivator = visibilityConditionActivator ?? throw new ArgumentNullException(nameof(KenticoFormBuilderPropertiesPanelController.visibilityConditionActivator));
        }


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Action for retrieving form component's properties markup. The <see cref="FormComponentConfiguration"/> for which
        /// the panel is to be rendered is in the request body.
        /// </summary>
        /// <param name="formId">Identifier of a <see cref="BizFormInfo"/> where form component is used.</param>
        [HttpPost]
        public ActionResult GetPropertiesMarkup(int formId)
        {
            string configurationJson = StreamUtils.ReadStreamToEnd(Request.InputStream, true);

            FormComponentConfiguration formComponentConfiguration = formBuilderConfigurationSerializer.DeserializeFormComponentConfiguration(configurationJson);

            if (formComponentConfiguration.IsInvalidComponent())
            {
                return new EmptyResult();
            }

            string formComponentTypeIdentifier = formComponentConfiguration.TypeIdentifier;
            Guid formComponentInstanceIdentifier = formComponentConfiguration.Identifier;

            ViewData.TemplateInfo.HtmlFieldPrefix = GetFormComponentInstancePrefix(formComponentInstanceIdentifier);
            PropertiesPanel viewModel = GetPropertiesPanelViewModel(formComponentInstanceIdentifier, formId, formComponentConfiguration.Properties.Name, formComponentTypeIdentifier, formComponentConfiguration.Properties, false);

            return PartialView("~/Views/Shared/Kentico/FormBuilder/_PropertiesTab.cshtml", viewModel);
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Validates form component properties.
        /// </summary>
        /// <param name="formComponentInstanceIdentifier">Form component instance identifier for which to validate it's properties.</param>
        /// <param name="formComponentTypeIdentifier">Type identifier of the <see cref="FormComponent"/>.</param>
        /// <param name="formId">Identifier of a <see cref="BizFormInfo"/> where form component is used.</param>
        /// <param name="formFieldName">Name of field the properties belong to.</param>
        [HttpPost]
        public ActionResult ValidateProperties(Guid formComponentInstanceIdentifier, string formComponentTypeIdentifier, int formId, string formFieldName)
        {
            if (formComponentTypeIdentifier ==  FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER)
            {
                return new EmptyResult();
            }

            FormCollection form = new FormCollection(ControllerContext.HttpContext.Request.Unvalidated.Form);

            var formComponentInstancePrefix = GetFormComponentInstancePrefix(formComponentInstanceIdentifier);
            var formComponentProperties = GetFormComponentProperties(formComponentTypeIdentifier);
            editablePropertiesModelBinder.BindModel(ControllerContext, CreatePropertiesPanelContext(formId, formFieldName), formComponentProperties, form, formComponentInstancePrefix);

            ViewData.TemplateInfo.HtmlFieldPrefix = formComponentInstancePrefix;
            var viewModel = GetPropertiesPanelViewModel(formComponentInstanceIdentifier, formId, formFieldName, formComponentTypeIdentifier, formComponentProperties, true, ModelState.IsValid);
            return PartialView("~/Views/Shared/Kentico/FormBuilder/_PropertiesTab.cshtml", viewModel);
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Gets markup for configuring <see cref="ValidationRule"/>.
        /// </summary>
        /// <param name="formComponentInstanceIdentifier">Identifier of a Form component instance for which to retrieve validation rule's configuration markup.</param>
        /// <param name="validationRuleIdentifier">Identifies type of validation rule for which to retrieve it's markup.</param>
        /// <param name="validationRule">Validation rule data to be inserted into a resulting markup.</param>
        /// <param name="formId">Identifier of form being edited.</param>
        /// <param name="formFieldName">Name of field the properties belong to.</param>
        [HttpPost]
        public ActionResult GetValidationRuleConfigurationMarkup(Guid formComponentInstanceIdentifier, string validationRuleIdentifier, string validationRule, int formId, string formFieldName)
        {
            var validationRuleType = validationRuleDefinitionProvider.Get(validationRuleIdentifier)?.ValidationRuleType;
            if (validationRuleType == null)
            {
                throw new InvalidOperationException($"Validation rule with identifier '{validationRuleIdentifier}' is not registered in the system.");
            }

            var validationRuleInstance = (ValidationRule)JsonConvert.DeserializeObject(validationRule, validationRuleType, FormBuilderConfigurationSerializer.GetSettings());

            var formComponentInstancePrefix = GetFormComponentInstancePrefix(formComponentInstanceIdentifier);
            ViewData.TemplateInfo.HtmlFieldPrefix = formComponentInstancePrefix;

            return GetValidationRuleConfigurationView(formComponentInstanceIdentifier, validationRuleIdentifier, validationRuleInstance, formId, formFieldName ?? "unknown", false);
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Validates whether <see cref="ValidationRule"/> is configured correctly.
        /// </summary>
        /// <param name="formComponentInstanceIdentifier">Form component instance identifier for which to validate it's validation rule.</param>
        /// <param name="validationRuleIdentifier">Identifies type of validation rule which configuration is being validated.</param>
        /// <param name="formId">Identifier of form being edited.</param>
        /// <param name="formFieldName">Name of field the properties belong to.</param>
        [HttpPost]
        public ActionResult ValidateValidationRuleConfiguration(Guid formComponentInstanceIdentifier, string validationRuleIdentifier, int formId, string formFieldName)
        {
            ValidationRule validationRule;
            try
            {
                validationRule = validationRuleActivator.CreateValidationRule(validationRuleIdentifier);
            }
            catch (Exception e)
            {
                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, "ValidationRuleMissing", e);
                throw;
            }

            var formComponentInstancePrefix = GetFormComponentInstancePrefix(formComponentInstanceIdentifier);
            FormCollection form = new FormCollection(ControllerContext.HttpContext.Request.Unvalidated.Form);
            editablePropertiesModelBinder.BindModel(ControllerContext, CreatePropertiesPanelContext(formId, formFieldName), validationRule, form, formComponentInstancePrefix);

            ViewData.TemplateInfo.HtmlFieldPrefix = formComponentInstancePrefix;

            return GetValidationRuleConfigurationView(formComponentInstanceIdentifier, validationRuleIdentifier, validationRule, formId, formFieldName, true);
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Gets markup for configuring visibility condition.
        /// </summary>
        /// <param name="formId">Identifier of form being edited.</param>
        /// <param name="formFieldName">Name of field the properties belong to.</param>
        /// <param name="formComponentInstanceIdentifier">Identifier of a Form component instance for which to retrieve validation rule's configuration markup.</param>
        [HttpPost]
        public ActionResult GetVisibilityConditionConfigurationMarkup(int formId, string formFieldName, Guid formComponentInstanceIdentifier)
        {
            // Get current visibility condition
            var currentVisibilityConditionConfiguration = GetCurrentVisibilityConditionConfiguration(formId, formComponentInstanceIdentifier, out List<FormComponent> components);

            // Get all visibility condition eligible for current form component
            var allAvailableConditions = GetAvailableVisibilityConditions(formComponentInstanceIdentifier, currentVisibilityConditionConfiguration, components);

            return GetVisibilityConditionFormView(formId, formFieldName, formComponentInstanceIdentifier, allAvailableConditions,
                currentVisibilityConditionConfiguration, false);
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF

#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF


        /// <summary>
        /// Validates data sent for configuring <see cref="VisibilityCondition"/>.
        /// </summary>
        /// <param name="formId">Identifier of form being edited.</param>
        /// <param name="formFieldName">Name of field the properties belong to.</param>
        /// <param name="formComponentInstanceIdentifier">Identifier of a Form component instance for which to retrieve validation rule's configuration markup.</param>
        /// <param name="isUpdating">Decides whether this request is form's submission or updates form's markup e.g. due to visibility conditions.</param>
        [HttpPost]
        public ActionResult ValidateVisibilityConditionConfiguration(int formId, string formFieldName, Guid formComponentInstanceIdentifier, [UpdatableFormModelBinder]bool isUpdating)
        {
            // Get visibility condition identifier from dropdown
            var visibilityConditionDropdownItemIdentifier = HttpContext.Request.Unvalidated.Form.Get(nameof(VisibilityConditionForm.SelectedVisibilityConditionIdentifier));
            var formComponentInstancePrefix = GetFormComponentInstancePrefix(formComponentInstanceIdentifier, visibilityConditionDropdownItemIdentifier);

            // If no visibility condition is selected only placeholder is selected
            if (String.IsNullOrEmpty(visibilityConditionDropdownItemIdentifier))
            {
                var availableConditions = GetAvailableVisibilityConditions(formComponentInstanceIdentifier, null, formProvider.GetFormComponents(BizFormInfoProvider.GetBizFormInfo(formId)));
                return GetVisibilityConditionFormView(formId, formFieldName, formComponentInstanceIdentifier, availableConditions, null, false);
            }
            string visibilityConditionConfigurationIdentifier = GetVisibilityConditionIdentifierAndFormComponentInstanceGuid(visibilityConditionDropdownItemIdentifier, out Guid? formComponentInstanceGuid);

            // Create visibility condition configuration out of identifier from dropdown, if
            var currentVisibilityConditionConfiguration = GetCurrentVisibilityConditionConfiguration(formId, formComponentInstanceIdentifier, out List<FormComponent> components);
            bool isCurrentlySelectedItemStored = visibilityConditionDropdownItemIdentifier.Equals(GetVisibilityConditionDropdownItemIdentifier(currentVisibilityConditionConfiguration), StringComparison.OrdinalIgnoreCase);
            var selectedVisibilityConditionConfiguration = isCurrentlySelectedItemStored ? currentVisibilityConditionConfiguration : new VisibilityConditionConfiguration
            {
                Identifier = visibilityConditionConfigurationIdentifier,
                VisibilityCondition = CreateVisibilityCondition(visibilityConditionConfigurationIdentifier, formComponentInstanceGuid.GetValueOrDefault())
            };

            // Create visibility condition dropdown list
            var allAvailableConditions = GetAvailableVisibilityConditions(formComponentInstanceIdentifier, selectedVisibilityConditionConfiguration, components);

            // Bind the data from the form to visibility condition created above
            FormCollection form = new FormCollection(ControllerContext.HttpContext.Request.Unvalidated.Form);
            editablePropertiesModelBinder.BindModel(ControllerContext, CreatePropertiesPanelContext(formId, formFieldName), selectedVisibilityConditionConfiguration.VisibilityCondition, form, formComponentInstancePrefix);

            // If the form is only updating then clear all the validation errors
            if (!ModelState.IsValid && isUpdating)
            {
                ModelState.Clear();
            }

            return GetVisibilityConditionFormView(formId, formFieldName, formComponentInstanceIdentifier, allAvailableConditions, selectedVisibilityConditionConfiguration, !isUpdating);
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


        #region "Visibility conditions private methods"

        /// <summary>
        /// Creates a new instance of the <see cref="VisibilityCondition"/> specified by its definition with default property values.
        /// If <see cref="VisibilityCondition"/> inherits from <see cref="AnotherFieldVisibilityCondition{TValue}"/> its <see cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldGuid"/>
        /// is set with <paramref name="dependeeFieldIdentifier"/>.
        /// </summary>
        private VisibilityCondition CreateVisibilityCondition(string visibilityConditionIdentifier, Guid dependeeFieldIdentifier)
        {
            var visibilityCondition = visibilityConditionActivator.CreateVisibilityCondition(visibilityConditionIdentifier);
            return AddDependeeField(dependeeFieldIdentifier, visibilityCondition);
        }


        private VisibilityCondition AddDependeeField(Guid dependeeFieldIdentifier, VisibilityCondition visibilityCondition)
        {
            if (visibilityCondition is IAnotherFieldVisibilityCondition anotherFieldVisibilityCondition)
            {
                anotherFieldVisibilityCondition.DependeeFieldGuid = dependeeFieldIdentifier;
            }

            return visibilityCondition;
        }


        /// <summary>
        /// Returns <see cref="VisibilityConditionConfiguration"/> for current form component as well as list of form components in currently edited form.
        /// </summary>
        private VisibilityConditionConfiguration GetCurrentVisibilityConditionConfiguration(int formId, Guid formComponentInstanceIdentifier, out List<FormComponent> components)
        {
            components = formProvider.GetFormComponents(BizFormInfoProvider.GetBizFormInfo(formId));
            var currentComponent = components.FirstOrDefault(c => c.BaseProperties.Guid == formComponentInstanceIdentifier);
            if (currentComponent == null)
            {
                EventLogProvider.LogEvent(EventType.ERROR, FormBuilderConstants.EVENT_LOG_SOURCE, "FormComponentMissing", $"Form component with instance identifier {formComponentInstanceIdentifier} is missing in form with it '{formId}'.");
                throw new InvalidOperationException($"Form component with instance identifier '{formComponentInstanceIdentifier}' is missing in form with it '{formId}'.");
            }

            return currentComponent.BaseProperties.VisibilityConditionConfiguration;
        }


        /// <summary>
        /// Gets collection of form components for <see cref="visibilityConditionDefinitionProvider"/>.
        /// </summary>
        private IEnumerable<FormComponent> GetFormComponents(VisibilityConditionConfiguration visibilityConditionConfiguration, PropertiesPanelComponentContext context)
        {
            return visibilityConditionConfiguration != null ?
                editablePropertiesCollector.GetFormComponents(visibilityConditionConfiguration.VisibilityCondition, context) :
                Enumerable.Empty<FormComponent>();
        }


        private List<VisibilityConditionDropdownItem> GetAvailableVisibilityConditions(Guid currentFormComponentInstanceIdentifier, VisibilityConditionConfiguration currentVisibilityConditionConfiguration, List<FormComponent> components)
        {
            var defaultDependentConditions = GetVisibilityConditionsDependingOnAnotherFieldItems(currentFormComponentInstanceIdentifier, currentVisibilityConditionConfiguration, components, d => d.IsDefaultVisibilityCondition());
            var customDependentConditions = GetVisibilityConditionsDependingOnAnotherFieldItems(currentFormComponentInstanceIdentifier, currentVisibilityConditionConfiguration, components, d => !d.IsDefaultVisibilityCondition());
            var customConditions = GetCustomVisibilityConditions();

            defaultDependentConditions.AddRange(customDependentConditions);
            defaultDependentConditions.AddRange(customConditions);
            return defaultDependentConditions;
        }


        /// <summary>
        /// Returns collection of visibility conditions not depending on another field that will be used in dropdown list to select visibility condition for a current form component.
        /// </summary>
        private IEnumerable<VisibilityConditionDropdownItem> GetCustomVisibilityConditions()
        {
            return visibilityConditionDefinitionProvider.GetCustomVisibilityConditionDefinitions().Select(d => new VisibilityConditionDropdownItem(d));
        }


        /// <summary>
        /// Returns collection of visibility conditions depending on another field filtered by <paramref name="filter"/> used in dropdown list to
        /// select visibility condition for a current form component.
        /// Conditions are returned only for form components preceding <paramref name="currentFormComponentInstanceIdentifier"/>.
        /// </summary>
        private List<VisibilityConditionDropdownItem> GetVisibilityConditionsDependingOnAnotherFieldItems(Guid currentFormComponentInstanceIdentifier, VisibilityConditionConfiguration currentVisibilityConditionConfiguration, List<FormComponent> components, Func<VisibilityConditionDefinition, bool> filter)
        {
            var currentVisibilityConditionDropdownItemIdentifier = GetVisibilityConditionDropdownItemIdentifier(currentVisibilityConditionConfiguration);

            var allDropdownItems = new List<VisibilityConditionDropdownItem>();
            VisibilityConditionDropdownItem dropdownItemRepresentingCurrentVisibilityCondition = null;

            var componentIsAfterCurrentOne = false;
            foreach (var component in components)
            {
                var definitions = visibilityConditionDefinitionProvider.GetAnotherFieldVisibilityConditionDefinitions(component.Definition.GetNonNullableValueType());

                if (!componentIsAfterCurrentOne)
                {
                    componentIsAfterCurrentOne = component.BaseProperties.Guid == currentFormComponentInstanceIdentifier;
                }

                foreach (var definition in definitions.Where(filter))
                {
                    var dropdownItem = new VisibilityConditionDropdownItem(definition, component);
                    if (!componentIsAfterCurrentOne)
                    {
                        allDropdownItems.Add(dropdownItem);
                    }

                    if (currentVisibilityConditionDropdownItemIdentifier.Equals(dropdownItem.ConditionIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        dropdownItemRepresentingCurrentVisibilityCondition = dropdownItem;
                    }
                }
            }

            if (dropdownItemRepresentingCurrentVisibilityCondition != null &&
                currentVisibilityConditionConfiguration?.GetDependeeFieldGuid() != null &&
                !allDropdownItems.Contains(dropdownItemRepresentingCurrentVisibilityCondition))
            {
                // If visibility condition depends on another field and is not present int result list it means that it is
                // dependent on field beneath this makes condition invalid, however, condition has to be present first in the
                // dropdown list so the user can see the problem and fix it
                dropdownItemRepresentingCurrentVisibilityCondition.IsValidVisibilityCondition = false;
                allDropdownItems.Insert(0, dropdownItemRepresentingCurrentVisibilityCondition);
            }

            return allDropdownItems;
        }


        /// <summary>
        /// Creates an identifier used in visibility condition dropdown list that will uniquely identify a condition.
        /// Condition that depends on another field has its identifier '{conditionIdentifier}|{formComponentDependingOn}'.
        /// Condition that does not depend on another field has its identifier '{conditionIdentifier}|'.
        /// </summary>
        private string GetVisibilityConditionDropdownItemIdentifier(VisibilityConditionConfiguration visibilityConditionConfiguration)
        {
            var anotherFieldVisibilityCondition = visibilityConditionConfiguration?.VisibilityCondition as IAnotherFieldVisibilityCondition;
            return GetVisibilityConditionDropdownItemIdentifier(visibilityConditionConfiguration?.Identifier, anotherFieldVisibilityCondition?.DependeeFieldGuid);
        }


        /// <summary>
        /// Creates an identifier used in visibility condition dropdown list that will uniquely identify a condition.
        /// Condition that depends on another field has its identifier '{conditionIdentifier}|{formComponentDependingOn}'.
        /// Condition that does not depend on another field has its identifier '{conditionIdentifier}|'.
        /// </summary>
        private static string GetVisibilityConditionDropdownItemIdentifier(string visibilityConditionDefinitionIdentifier, Guid? formComponentInstanceGuid)
        {
            return $"{visibilityConditionDefinitionIdentifier}|{formComponentInstanceGuid}";
        }


        /// <summary>
        /// Returns <see cref="VisibilityConditionConfiguration.Identifier"/> from the visibility condition drop down list.
        /// If the <see cref="VisibilityCondition"/> implements <see cref="IAnotherFieldVisibilityCondition"/> then instance identifier
        /// of the form component that the condition depends on is returned as well.
        /// </summary>
        private string GetVisibilityConditionIdentifierAndFormComponentInstanceGuid(string visibilityConditionDropdownItemIdentifier, out Guid? formComponentInstanceGuid)
        {
            var arr = visibilityConditionDropdownItemIdentifier.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length < 2)
            {
                formComponentInstanceGuid = null;
                return arr[0];
            }

            formComponentInstanceGuid = ValidationHelper.GetGuid(arr[1], Guid.Empty);
            return arr[0];
        }


        /// <summary>
        /// Encapsulates data required for dropdown list with visibility conditions.
        /// </summary>
        private class VisibilityConditionDropdownItem
        {
            public bool IsValidVisibilityCondition { get; set; } = true;


            public string Title => ConditionDefinition.GetVisibilityConditionTitle(FormComponent);


            public string ConditionIdentifier => GetVisibilityConditionDropdownItemIdentifier(ConditionDefinition.Identifier, FormComponent?.BaseProperties?.Guid);


            public VisibilityConditionDefinition ConditionDefinition { get; }


            public FormComponent FormComponent { get; }


            public VisibilityConditionDropdownItem(VisibilityConditionDefinition definition, FormComponent component = null)
            {
                FormComponent = component;
                ConditionDefinition = definition;
            }
        }


        private ActionResult GetVisibilityConditionFormView(int formId, string formFieldName, Guid formComponentInstanceIdentifier,
            IEnumerable<VisibilityConditionDropdownItem> allAvailableConditions, VisibilityConditionConfiguration currentVisibilityConditionConfiguration, bool notifyFormBuilder)
        {
            var visibilityConditionDropdownItemIdentifier = GetVisibilityConditionDropdownItemIdentifier(currentVisibilityConditionConfiguration);

            // Set the form prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = GetFormComponentInstancePrefix(formComponentInstanceIdentifier, visibilityConditionDropdownItemIdentifier);

            var propertiesPanelContext = CreatePropertiesPanelContext(formId, formFieldName);

            FormFieldInfo formInfo = GetFormInfoByPropertiesPanelContext(propertiesPanelContext);
            var caption = ResHelper.LocalizeString(formInfo.Caption);
            var selectedVisibilityConditionLocalizedLabel = ResHelper.GetStringFormat("kentico.formbuilder.propertiespanel.visibilityconditions.selectedcondition.label", caption);

            return PartialView("~/Views/Shared/Kentico/FormBuilder/_VisibilityConditionForm.cshtml", new VisibilityConditionForm
            {
                NotifyFormBuilder = notifyFormBuilder,

                FormId = formId,
                FormFieldName = formFieldName,
                FormComponentInstanceIdentifier = formComponentInstanceIdentifier,
                SelectedVisibilityConditionLocalizedLabel = selectedVisibilityConditionLocalizedLabel,

                VisibilityConditionConfiguration = currentVisibilityConditionConfiguration,
                FormComponents = GetFormComponents(currentVisibilityConditionConfiguration, CreatePropertiesPanelContext(formId, formFieldName)),
                AvailableVisibilityConditions = allAvailableConditions.Select(c => new SelectListItem
                {
                    Text = c.Title,
                    Value = c.ConditionIdentifier,
                    Selected = c.ConditionIdentifier.Equals(visibilityConditionDropdownItemIdentifier, StringComparison.OrdinalIgnoreCase)
                }),

                IsVisibilityConditionValid = allAvailableConditions.FirstOrDefault()?.IsValidVisibilityCondition ?? true
            });
        }

        #endregion


        /// <summary>
        /// Returns View for <see cref="ValidationRule"/> configuration.
        /// </summary>
        private ActionResult GetValidationRuleConfigurationView(Guid formComponentInstanceIdentifier, string validationRuleIdentifier, ValidationRule validationRule,
            int formId, string fieldName, bool notifyFormBuilder)
        {
            return PartialView("~/Views/Shared/Kentico/FormBuilder/_ValidationRuleForm.cshtml", new ValidationRuleForm
            {
                FormComponentInstanceIdentifier = formComponentInstanceIdentifier,
                FormId = formId,
                FieldName = fieldName,
                FormComponents = editablePropertiesCollector.GetFormComponents(validationRule, CreatePropertiesPanelContext(formId, fieldName)),
                NotifyFormBuilder = notifyFormBuilder,
                ValidationRuleConfiguration = new ValidationRuleConfiguration(validationRuleIdentifier, validationRule)
            });
        }


        private PropertiesPanelComponentContext CreatePropertiesPanelContext(int formId, string fieldName)
        {
            return new PropertiesPanelComponentContext
            {
                BizFormInfo = BizFormInfoProvider.GetBizFormInfo(formId),
                FieldName = fieldName
            };
        }


        /// <summary>
        /// Gets properties panel view model.
        /// </summary>
        private PropertiesPanel GetPropertiesPanelViewModel(Guid formComponentInstanceIdentifier, int formId, string fieldName, string formComponentTypeIdentifier, FormComponentProperties properties, bool notifyFormBuilder, bool isModelStateValid = false)
        {
            return new PropertiesPanel
            {
                InstanceIdentifier = formComponentInstanceIdentifier,
                FormId = formId,
                FormFieldName = isModelStateValid ? properties.Name : fieldName,
                TypeIdentifier = formComponentTypeIdentifier,
                PropertiesFormComponents = editablePropertiesCollector.GetFormComponents(properties, CreatePropertiesPanelContext(formId, fieldName)),
                NotifyFormBuilder = notifyFormBuilder,
                UpdatedProperties = isModelStateValid ?
                                    properties.GetAnnotatedProperties<EditingComponentAttribute>(false)
                                              .ToDictionary(p => p.Name, p => p.GetValue(properties, null), StringComparer.OrdinalIgnoreCase) : null
            };
        }


        /// <summary>
        /// Gets properties for given <paramref name="formComponentIdentifier"/>.
        /// </summary>
        private FormComponentProperties GetFormComponentProperties(string formComponentIdentifier)
        {
            var formComponentDefinition = formComponentDefinitionProvider.Get(formComponentIdentifier);
            return formComponentActivator.CreateDefaultProperties(formComponentDefinition);
        }


        /// <summary>
        /// Returns form component instance prefix that is derived from <paramref name="formComponentInstanceIdentifier"/>.
        /// </summary>
        private string GetFormComponentInstancePrefix(Guid formComponentInstanceIdentifier)
        {
            return $"form-{formComponentInstanceIdentifier}";
        }


        /// <summary>
        /// Returns form component instance prefix that is derived from <paramref name="formComponentInstanceIdentifier"/> and <paramref name="visibilityConditionDropdownItemId"/>.
        /// </summary>
        private string GetFormComponentInstancePrefix(Guid formComponentInstanceIdentifier, string visibilityConditionDropdownItemId)
        {
            return $"form-{formComponentInstanceIdentifier}-{visibilityConditionDropdownItemId}";
        }


        /// <summary>
        /// Returns <see cref="FormInfo"/> for the specified <see cref="PropertiesPanelComponentContext"/>.
        /// </summary>
        /// <param name="propertiesPanelContext">Current <see cref="PropertiesPanelComponentContext"/>.</param>
        /// <returns></returns>
        private FormFieldInfo GetFormInfoByPropertiesPanelContext(PropertiesPanelComponentContext propertiesPanelContext)
        {
            var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(propertiesPanelContext.BizFormInfo.FormClassID);
            var formInfo = new FormInfo(dataClassInfo.ClassFormDefinition);

            return formInfo.GetFields<FormFieldInfo>().FirstOrDefault(f => f.Name.Equals(propertiesPanelContext.FieldName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
