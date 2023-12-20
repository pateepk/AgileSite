using Kentico.Forms.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Base class for personalization condition type.
    /// </summary>
    public abstract class ConditionType : IConditionType
    {
        /// <summary>
        /// Evaluate condition type.
        /// </summary>
        /// <returns>Returns <c>true</c> if implemented condition is met.</returns>
        public abstract bool Evaluate();


        /// <summary>
        /// Variant name.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 100, Label = "{$kentico.pagebuilder.variant.name$}")]
        [EditingComponentProperty(nameof(TextInputProperties.Required), true)]
        public virtual string VariantName
        {
            get;
            set;
        }
    }
}
