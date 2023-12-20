using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(NumericComparisonTypeSelectorComponent.IDENTIFIER, typeof(NumericComparisonTypeSelectorComponent), "Comparison type selector", IsAvailableInFormBuilderEditor = false, ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Selector component for numeric comparison.
    /// </summary>
    public class NumericComparisonTypeSelectorComponent : ComparisonTypeSelectorComponent<NumericFieldComparisonTypes>
    {
        /// <summary>
        /// Represents the <see cref="NumericComparisonTypeSelectorComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.NumericFieldComparisonTypeSelector";
    }
}
