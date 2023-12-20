using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(StringComparisonTypeSelectorComponent.IDENTIFIER, typeof(StringComparisonTypeSelectorComponent), "Comparison type selector", IsAvailableInFormBuilderEditor = false, ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Selector component for string comparison.
    /// </summary>
    public class StringComparisonTypeSelectorComponent : ComparisonTypeSelectorComponent<StringFieldComparisonTypes>
    {
        /// <summary>
        /// Represents the <see cref="StringComparisonTypeSelectorComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.StringFieldComparisonTypeSelector";
    }
}
