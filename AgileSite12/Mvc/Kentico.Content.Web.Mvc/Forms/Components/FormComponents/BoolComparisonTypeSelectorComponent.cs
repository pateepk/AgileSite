using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(BoolComparisonTypeSelectorComponent.IDENTIFIER, typeof(BoolComparisonTypeSelectorComponent), "Comparison type selector", IsAvailableInFormBuilderEditor = false, ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Selector component for boolean comparison.
    /// </summary>
    public class BoolComparisonTypeSelectorComponent : ComparisonTypeSelectorComponent<BoolFieldValueTypes>
    {
        /// <summary>
        /// Represents the <see cref="BoolComparisonTypeSelectorComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.BoolFieldValueTypeSelector";
    }
}
