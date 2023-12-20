using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

using CMS.Helpers;

using DataTypeValidation = CMS.FormEngine.DataTypeIntegrityValidationResultType;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Model binder used for binding form data to <see cref="FormComponent"/>.
    /// </summary>
    internal class FormComponentModelBinder : DefaultModelBinder, IFormComponentModelBinder
    {
        /// <summary>
        /// Validates that all attributes annotated with <see cref="BindablePropertyAttribute"/> can be bound properly.
        /// This includes validation attributes and <see cref="IValidatableObject.Validate(ValidationContext)"/> method.
        /// In case all properties are bound properly validate the resulting component value using its <see cref="ValidationRule"/>s
        /// and <see cref="FormComponentProperties.Required"/> setting.
        /// </summary>
        /// <param name="controllerContext">Controller context of the current request.</param>
        /// <param name="formComponent">Component to be bound.</param>
        /// <param name="formCollection">Source of data for the binding.</param>
        /// <param name="nameHtmlFieldPrefix">Prefix used in the name attribute of the inputs belonging to <paramref name="formComponent"/> to identify values in form collection.</param>
        /// <param name="shouldValidationContinue">
        /// Indicates whether validation should continue after component is bound.
        /// Validation won't continue in case binding of any one of the partial values fails.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formComponent"/> or <paramref name="formCollection"/> is null.</exception>
        public void BindComponent(ControllerContext controllerContext, FormComponent formComponent, FormCollection formCollection, string nameHtmlFieldPrefix, out bool shouldValidationContinue)
        {
            if (formComponent == null)
            {
                throw new ArgumentNullException(nameof(formComponent));
            }

            if (formCollection == null)
            {
                throw new ArgumentNullException(nameof(formCollection));
            }

            var bindablePropertyNames = formComponent.GetBindablePropertyNames();

            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => formComponent, formComponent.GetType()),
                ModelName = nameHtmlFieldPrefix,
                ModelState = controllerContext.Controller.ViewData.ModelState,
                ValueProvider = formCollection,
                PropertyFilter = name => bindablePropertyNames.Contains(name, StringComparer.OrdinalIgnoreCase)
            };

            BindModel(controllerContext, bindingContext);

            var prefix = String.IsNullOrEmpty(nameHtmlFieldPrefix) ? "" : $"{nameHtmlFieldPrefix}.";

            if (bindablePropertyNames.Any(prop => bindingContext.ModelState[$"{prefix}{prop}"]?.Errors.Any() ?? false))
            {
                shouldValidationContinue = false;
                return;
            }

            ValidateComponentValue(bindingContext, formComponent, out shouldValidationContinue);
        }


        /// <summary>
        /// Validates bound component value.
        /// </summary>
        /// <param name="bindingContext">Binding context, containing possible error messages.</param>
        /// <param name="component">Component whose value is validated.</param>
        /// <param name="shouldValidationContinue">
        /// Indicates whether validation should continue after component is bound.
        /// Validation won't continue if component requires value and the resulting value is null, or if component value is out of range of it's data type, or if some validation rule assigned to component is not valid.
        /// </param>
        internal virtual void ValidateComponentValue(ModelBindingContext bindingContext, FormComponent component, out bool shouldValidationContinue)
        {
            var componentValue = component.GetObjectValue();

            var modelStateKey = component.GetModelStateKeyForValueError(bindingContext.ModelName);

            if (componentValue == null)
            {
                shouldValidationContinue = false;
                if (component.BaseProperties.Required)
                {
                    var localizedError = ResHelper.GetString("general.requiresvalue");
                    bindingContext.ModelState.AddModelError(modelStateKey, localizedError);
                }

                return;
            }

            var dataTypeValidationResult = FormComponentDataTypeHelper.GetValueDataTypeValidationResult(component);
            if (!dataTypeValidationResult.Success &&
                dataTypeValidationResult.ResultType != DataTypeValidation.MaxLengthError)
            {
                // MaxLengthError will be handled after custom rules check.

                shouldValidationContinue = false;
                bindingContext.ModelState.AddModelError(modelStateKey, dataTypeValidationResult.ErrorMessage);

                return;
            }

            var rules = component.BaseProperties.ValidationRuleConfigurations.Select(v => v.ValidationRule)
                                 .Where(rule => rule.GetType().FindTypeByGenericDefinition(typeof(CompareToFieldValidationRule<>)) == null);

            var maxLengthCustomValidationError = false;
            foreach (var rule in rules)
            {
                if (!rule.IsValueValid(componentValue))
                {
                    var localizedError = ResHelper.LocalizeString(rule.ErrorMessage);
                    bindingContext.ModelState.AddModelError(modelStateKey, localizedError);

                    if (rule is MaximumLengthValidationRule)
                    {
                        maxLengthCustomValidationError = true;
                    }
                }
            }

            // Check again DataTypeIntegrity
            if (dataTypeValidationResult.ResultType == DataTypeValidation.MaxLengthError)
            {
                shouldValidationContinue = false;

                // If custom MaxLength validation error was displayed, do not show basic one
                if (!maxLengthCustomValidationError)
                {
                    bindingContext.ModelState.AddModelError(modelStateKey, dataTypeValidationResult.ErrorMessage);
                }

                return;
            }

            shouldValidationContinue = true;
        }
    }
}
