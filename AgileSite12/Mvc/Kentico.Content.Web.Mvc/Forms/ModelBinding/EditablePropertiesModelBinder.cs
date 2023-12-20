using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates logic for binding data to instance.
    /// </summary>
    internal class EditablePropertiesModelBinder : IEditablePropertiesModelBinder
    {
        private readonly IEditablePropertiesCollector editablePropertiesCollector;
        private readonly IFormComponentModelBinder formComponentModelBinder;


        /// <summary>
        /// Initializes a new instance of the <see cref="EditablePropertiesModelBinder"/> class.
        /// </summary>
        public EditablePropertiesModelBinder(IEditablePropertiesCollector editablePropertiesCollector, IFormComponentModelBinder formComponentModelBinder)
        {
            this.editablePropertiesCollector = editablePropertiesCollector;
            this.formComponentModelBinder = formComponentModelBinder;
        }


        /// <summary>
        /// Binds and validates data from <paramref name="formCollection"/> to given <paramref name="model"/>, where <paramref name="formComponentInstancePrefix"/> is common prefix of <paramref name="formCollection"/>'s data keys that should be bound.
        /// Also fills the model state of the <paramref name="controllerContext"/> with any errors that occured during validation.
        /// </summary>
        /// <param name="controllerContext">Controller context to whose model state validation errors are added.</param>
        /// <param name="formComponentContext">Contextual information specifying where the form components representing the <paramref name="model"/> are being used.</param>
        /// <param name="model">Instance to which bind data from <paramref name="formCollection"/>.</param>
        /// <param name="formCollection">Form collection from which to bind data to given <paramref name="model"/>.</param>
        /// <param name="formComponentInstancePrefix">Form identifier used as common prefix of <paramref name="formCollection"/>'s data keys that should be bound.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="controllerContext"/>, <paramref name="model"/> or <paramref name="formCollection"/> is null.</exception>
        public void BindModel(ControllerContext controllerContext, FormComponentContext formComponentContext, object model, FormCollection formCollection, string formComponentInstancePrefix)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (formCollection == null)
            {
                throw new ArgumentNullException(nameof(formCollection));
            }

            var propertyFullNameFormat = String.IsNullOrEmpty(formComponentInstancePrefix) ? $"{{0}}" : $"{formComponentInstancePrefix}.{{0}}";

            // Properties with their form components that are used for their editing
            var editableProperties = editablePropertiesCollector.GetEditablePropertiesWithEditors(model, formComponentContext).ToDictionary(kvp => kvp.Key.Name, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<PropertyInfo, FormComponent> propertyEditorKeyValuePair in editableProperties.Values)
            {
                PropertyInfo propertyInfo = propertyEditorKeyValuePair.Key;
                FormComponent formComponent = propertyEditorKeyValuePair.Value;

                var propertyFullName = String.Format(propertyFullNameFormat, propertyInfo.Name);
                var value = GetComponentValue(controllerContext, propertyFullName, formComponent, formCollection);

                try
                {
                    propertyInfo.SetValue(model, value, null);
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException($"Property '{propertyInfo.Name}' of class '{model.GetType()}'" +
                                                        $" cannot be bound with value '{value}' provided by a form component '{formComponent.Definition.Identifier}'.", e);
                }
            }

            // Validate the model after all properties are set
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType());
            var validationErrors = ModelValidator.GetModelValidator(metadata, controllerContext).Validate(null);

            foreach (var error in validationErrors)
            {
                if (editableProperties.TryGetValue(error.MemberName, out var errorKVP))
                {
                    var component = errorKVP.Value;
                    var propertyFullName = String.Format(propertyFullNameFormat, error.MemberName);
                    var modelStateKey = component.GetModelStateKeyForValueError(propertyFullName);

                    controllerContext.Controller.ViewData.ModelState.AddModelError(modelStateKey, error.Message);
                }
            }
        }


        /// <summary>
        /// Binds all properties of specified component.
        /// </summary>
        /// <param name="controllerContext">Controller context to whose model state validation errors are added.</param>
        /// <param name="propertyName">Name of the property belonging to the <see cref="FormComponent"/> to which the value of <paramref name="component"/> will be bound.</param>
        /// <param name="component">Component to be bound.</param>
        /// <param name="formCollection">Source of data for the binding.</param>
        private object GetComponentValue(ControllerContext controllerContext, string propertyName, FormComponent component, FormCollection formCollection)
        {
            formComponentModelBinder.BindComponent(controllerContext, component, formCollection, propertyName, out _);

            return component.GetObjectValue();
        }
    }
}
