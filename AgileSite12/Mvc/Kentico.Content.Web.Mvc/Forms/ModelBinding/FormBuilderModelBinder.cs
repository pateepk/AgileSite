using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

using CMS.Helpers;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Model binder used for binding values when submitting forms built via Form builder.
    /// </summary>
    public class FormBuilderModelBinder : IModelBinder
    {
        private readonly BizFormInfo formInfo;
        private readonly IFormProvider formProvider;
        private readonly IFormComponentModelBinder formComponentModelBinder;
        private readonly IFormComponentModelBinder formComponentPrebinder = new FormComponentPrebinder();
        private readonly IFormComponentVisibilityEvaluator formComponentVisibilityEvaluator;
        private readonly string fieldPrefix;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormBuilderModelBinder"/> class.
        /// </summary>
        /// <param name="formInfo">Biz form whose values are to be bound.</param>
        /// <param name="formProvider">Provides form components for <paramref name="formInfo"/>.</param>
        /// <param name="formComponentModelBinder">Binder for binding values from form to form components.</param>
        /// <param name="formComponentVisibilityEvaluator">Evaluator for form component visibility.</param>
        /// <param name="fieldPrefix">Prefix of the keys in the form collection. Value without trailing dot is expected. If null, no prefix is assumed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formInfo"/>, <paramref name="formProvider"/>
        /// or <paramref name="formComponentModelBinder"/> is null.</exception>
        public FormBuilderModelBinder(BizFormInfo formInfo, IFormProvider formProvider, IFormComponentModelBinder formComponentModelBinder,
            IFormComponentVisibilityEvaluator formComponentVisibilityEvaluator, string fieldPrefix = null)
        {
            this.formInfo = formInfo ?? throw new ArgumentNullException(nameof(formInfo));
            this.formProvider = formProvider ?? throw new ArgumentNullException(nameof(formProvider));
            this.formComponentModelBinder = formComponentModelBinder ?? throw new ArgumentNullException(nameof(formComponentModelBinder));
            this.formComponentVisibilityEvaluator = formComponentVisibilityEvaluator ?? throw new ArgumentNullException(nameof(formComponentVisibilityEvaluator));
            this.fieldPrefix = String.IsNullOrEmpty(fieldPrefix) ? null : $"{fieldPrefix}.";
        }


        /// <summary>
        /// Method called automatically when parameter is annotated with <seealso cref="ModelBinderAttribute"/> with type
        /// set to this binder.
        /// Performs validation of the model and in case of any errors populates the <see cref="ModelState"/>
        /// of the <paramref name="controllerContext"/>.
        /// </summary>
        /// <param name="controllerContext">Controller context of the current request.</param>
        /// <param name="bindingContext">Binding context.</param>
        /// <returns>Model instance containing values bound from request data.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var formCollection = new FormCollection(controllerContext.HttpContext.Request.Unvalidated.Form);
            var components = formProvider.GetFormComponents(formInfo);
            var componentsToContinueValidationFor = new List<FormComponent>();

            PrebindComponents(controllerContext, components, formCollection);

            var displayedComponents = GetDisplayedFormComponents(bindingContext, components).ToList();

            foreach (var component in displayedComponents)
            {
                var prefixedComponentName = $"{fieldPrefix}{component.Name}";

                formComponentModelBinder.BindComponent(controllerContext, component, formCollection, prefixedComponentName, out var shouldValidationContinue);

                if (shouldValidationContinue)
                {
                    componentsToContinueValidationFor.Add(component);
                }
            }

            foreach (var component in componentsToContinueValidationFor)
            {
                var prefixedComponentName = $"{fieldPrefix}{component.Name}";
                var validationErrors = ValidateCompareToFieldRules(component, components);

                controllerContext.Controller.ViewData.ModelState.AddErrorMessages(prefixedComponentName, validationErrors);
            }

            return displayedComponents;
        }


        /// <summary>
        /// Validates rules which are of compare-to-another-field type.
        /// </summary>
        /// <param name="component">Component to validate.</param>
        /// <param name="allComponents">All form components.</param>
        /// <returns>Enumeration of validation errors, empty if validation succeeded.</returns>
        internal virtual IEnumerable<ValidationResult> ValidateCompareToFieldRules(FormComponent component, List<FormComponent> allComponents)
        {
            var dependsOnRules = component.BaseProperties.ValidationRuleConfigurations.Select(v => v.ValidationRule)
                .Where(rule => rule.GetType().FindTypeByGenericDefinition(typeof(CompareToFieldValidationRule<>)) != null);

            foreach (var rule in dependsOnRules)
            {
                var dependentRule = (ICompareToFieldValidationRule)rule;
                var comparedValue = component.GetObjectValue();

                var dependeeComponent = allComponents.Find(c => c.BaseProperties.Guid.Equals(dependentRule.DependeeFieldGuid));
                if (dependeeComponent == null)
                {
                    var exceptionMessage = $"Form '{formInfo.FormName}' contains component '{component.Name}' having validation rule '{rule.Title}' depending on missing field.";
                    throw new InvalidOperationException(exceptionMessage);
                }

                var errorMessage = ResHelper.LocalizeString(rule.ErrorMessage);

                var valueComparedAgainst = dependeeComponent.GetObjectValue();
                if (valueComparedAgainst != null)
                {
                    dependentRule.SetDependeeFieldValue(valueComparedAgainst);

                    if (!rule.IsValueValid(comparedValue))
                    {
                        yield return new ValidationResult(errorMessage);
                    }
                }
                else
                {
                    yield return new ValidationResult(errorMessage);
                }
            }
        }


        /// <summary>
        /// Returns list of form components displayed by form.
        /// </summary>
        /// <param name="bindingContext">Binding context.</param>
        /// <param name="components">List of components visible in form.</param>
        protected List<FormComponent> GetDisplayedFormComponents(ModelBindingContext bindingContext, List<FormComponent> components)
        {
            var context = bindingContext as FormBuilderModelBindingContext;
            return components.GetDisplayedComponents(context?.Contact, formInfo, context?.ExistingItem, formComponentVisibilityEvaluator).ToList();
        }


        /// <summary>
        /// Prebinds all components, skips the validation and clears model state errors. The actual binding and validation should occur later.
        /// This is a necessary preparation step to evaluate visibility conditions and validation rules that are based on another field value.
        /// </summary>
        private void PrebindComponents(ControllerContext controllerContext, List<FormComponent> components, FormCollection formCollection)
        {
            foreach (var component in components)
            {
                var prefixedComponentName = $"{fieldPrefix}{component.Name}";
                formComponentPrebinder.BindComponent(controllerContext, component, formCollection, prefixedComponentName, out _);
            }
            controllerContext.Controller.ViewData.ModelState.Clear();
        }
    }
}