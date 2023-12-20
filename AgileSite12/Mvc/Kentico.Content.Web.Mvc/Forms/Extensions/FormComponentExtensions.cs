using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Extension methods for <see cref="FormComponent"/> class.
    /// </summary>
    public static class FormComponentExtensions
    {
        /// <summary>
        /// Gets <see cref="BizFormComponentContext"/> of a form component, if such a context was bound to the component.
        /// </summary>
        /// <param name="component">Component to get <see cref="BizFormComponentContext"/> instance for.</param>
        /// <returns>Returns an instance of <see cref="BizFormComponentContext"/>, or null if such a context was not bound to the component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> is null.</exception>
        public static BizFormComponentContext GetBizFormComponentContext(this FormComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            return component.FormComponentContext as BizFormComponentContext;
        }


        /// <summary>
        /// Gets names of component properties which are enabled for binding.
        /// To enable property for binding annotate it with <see cref="BindablePropertyAttribute"/>.
        /// </summary>
        /// <param name="component">Component to get properties for.</param>
        /// <returns>An enumeration of names of binding enabled properties.</returns>
        internal static IEnumerable<string> GetBindablePropertyNames(this FormComponent component)
        {
            return component
                   .GetType()
                   .GetProperties()
                   .Where(p => p.GetCustomAttributes<BindablePropertyAttribute>(false).Any())
                   .Select(x => x.Name);
        }


        /// <summary>
        /// Gets the model state key that should be used for storing component value validation errors.
        /// </summary>
        /// <remarks>
        /// If component contains one bindable property, the key corresponds with property full name.
        /// If component contains multiple bindable properties, model state errors should be stored under the <paramref name="nameHtmlFieldPrefix"/>.
        /// </remarks>
        /// <param name="component">Component to get model state key for.</param>
        /// <param name="nameHtmlFieldPrefix">Prefix used in the name attribute of the inputs belonging to <paramref name="component"/> to identify values in form collection.</param>
        internal static string GetModelStateKeyForValueError(this FormComponent component, string nameHtmlFieldPrefix)
        {
            var propertyFullNameFormat = String.IsNullOrEmpty(nameHtmlFieldPrefix) ? $"{{0}}" : $"{nameHtmlFieldPrefix}.{{0}}";

            var bindablePropertyNames = component.GetBindablePropertyNames().ToList();

            return bindablePropertyNames.Count == 1
                ? String.Format(propertyFullNameFormat, bindablePropertyNames.First())
                : (nameHtmlFieldPrefix ?? String.Empty);
        }
    }
}
