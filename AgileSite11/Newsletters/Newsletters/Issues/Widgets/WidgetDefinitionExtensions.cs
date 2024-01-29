using System.Linq;

using CMS.FormEngine;

namespace CMS.Newsletters.Issues.Widgets
{
    /// <summary>
    /// Extension methods for <see cref="WidgetDefinition"/>.
    /// </summary>
    internal static class WidgetDefinitionExtensions
    {
        /// <summary>
        /// Indicates that widget definition contains any required field with no value provided.
        /// </summary>
        /// <param name="definition">Email widget definition.</param>
        public static bool HasRequiredPropertyWithoutDefaultValue(this WidgetDefinition definition)
        {
            return definition.Fields.Any(field =>
                !field.AllowEmpty
                && !HasDefaultValue(definition, field)
                && !IsExcludedRequiredField(field)
            );
        }


        private static bool IsExcludedRequiredField(FormFieldInfo field)
        {
            return FormHelper.IsFieldOfType(field, FormFieldControlTypeEnum.CheckBoxControl);
        }


        private static bool HasDefaultValue(WidgetDefinition definition, FormFieldInfo field)
        {
            var resolver = WidgetResolvers.GetWidgetDefaultPropertiesResolver(definition);
            return !string.IsNullOrEmpty(field.GetDefaultValue(FormResolveTypeEnum.AllFields, resolver));
        }
    }
}
