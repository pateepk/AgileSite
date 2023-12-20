using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Binds all properties of the specified component without executing the <see cref="FormComponent"/>'s <see cref="IValidatableObject"/> validation.
    /// </summary>
    internal class FormComponentPrebinder : DefaultModelBinder, IFormComponentModelBinder
    {       
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

            shouldValidationContinue = false;

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
        }


        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            // Do nothing
        }
    }
}
