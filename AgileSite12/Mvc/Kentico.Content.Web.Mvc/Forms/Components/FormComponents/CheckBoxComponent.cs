using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(CheckBoxComponent.IDENTIFIER, typeof(CheckBoxComponent), "{$kentico.formbuilder.component.checkbox.name$}", Description = "{$kentico.formbuilder.component.checkbox.description$}", IconClass = "icon-cb-check-preview", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a checkbox form component.
    /// </summary>
    public class CheckBoxComponent : FormComponent<CheckBoxProperties, bool>
    {
        /// <summary>
        /// Represents the <see cref="CheckBoxComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.CheckBox";


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public bool Value
        {
            get;
            set;
        }


        /// <summary>
        /// Gets name of the <see cref="Value"/> property.
        /// </summary>
        public override string LabelForPropertyName => nameof(Value);


        /// <summary>
        /// Gets the <see cref="Value"/>.
        /// </summary>
        public override bool GetValue()
        {
            return Value;
        }


        /// <summary>
        /// Sets the <see cref="Value"/>.
        /// </summary>
        public override void SetValue(bool value)
        {
            Value = value;
        }
    }
}
