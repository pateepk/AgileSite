using System;
using System.Linq;
using System.Reflection;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Utility methods for <see cref="EditingComponentAttribute"/>.
    /// </summary>
    internal static class EditingComponentUtils
    {
        /// <summary>
        /// Sets values of <paramref name="model"/> properties annotated with <see cref="EditingComponentAttribute"/>
        /// to <see cref="EditingComponentAttribute.DefaultValue"/>, if defined.
        /// </summary>
        /// <param name="model">Object to set values on.</param>
        public static void SetDefaultPropertiesValues(object model)
        {
            var properties = model.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(EditingComponentAttribute)));

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttributes<EditingComponentAttribute>(false).First();
                var defaultValue = attribute.DefaultValue;

                if (defaultValue != null)
                {
                    property.SetValue(model, defaultValue);
                }
            }
        }
    }
}
