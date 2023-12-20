using System;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Model binder used for binding contextual data required when updating form's markup e.g. applying visibility conditions.
    /// </summary>
    /// <seealso cref="UpdatableMvcForm"/>
    internal class UpdatableFormModelBinder : IModelBinder
    {        
        // This constant is also used in updatableFormHelper.js script file.
        internal const string UPDATING_FORM = "kentico_update_form";


        /// <summary>
        /// Method called automatically when parameter is annotated with <seealso cref="ModelBinderAttribute"/> with type
        /// set to this binder.
        /// Performs validation of the model and in case of any errors populates the <see cref="ModelState"/>
        /// of the <paramref name="controllerContext"/>.
        /// </summary>
        /// <param name="controllerContext">Controller context of the current request, used to obtain form ID.</param>
        /// <param name="bindingContext">Binding context, currently not used.</param>
        /// <returns>Model instance containing values bound from request data.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var formCollection = new FormCollection(controllerContext.HttpContext.Request.Unvalidated.Form);
            return IsFormUpdating(formCollection);
        }


        /// <summary>
        /// Returns true, when <paramref name="formCollection"/> contains an identifier with value equal to 'true' defining that the form was submitted to update form's markup.
        /// </summary>
        internal bool IsFormUpdating(FormCollection formCollection)
        {
            return formCollection?[UPDATING_FORM]?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}