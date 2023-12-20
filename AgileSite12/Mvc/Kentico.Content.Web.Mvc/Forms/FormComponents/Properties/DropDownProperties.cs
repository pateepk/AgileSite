namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="DropDownComponent"/>.
    /// </summary>
    public class DropDownProperties : SelectorProperties
    {
        /// <summary>
        /// Gets or sets the first option label value.
        /// </summary>
        /// <remarks>
        /// Option is not displayed when null or <see cref="string.Empty"/> is set.
        /// </remarks>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.component.dropdown.properties.optionlabel$}", Tooltip = "{$kentico.formbuilder.component.dropdown.properties.optionlabel.tooltip$}")]
        public string OptionLabel
        {
            get;
            set;
        }
    }
}
