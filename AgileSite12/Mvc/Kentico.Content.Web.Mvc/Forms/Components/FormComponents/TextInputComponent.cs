using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(TextInputComponent.IDENTIFIER, typeof(TextInputComponent), "{$kentico.formbuilder.component.textinput.name$}", Description = "{$kentico.formbuilder.component.textinput.description$}", IconClass = "icon-l-text", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a single line input form component.
    /// </summary>
    public class TextInputComponent : FormComponent<TextInputProperties, string>
    {
        /// <summary>
        /// Represents the <see cref="TextInputComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.TextInput";


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public string Value { get; set; }


        /// <summary>
        /// Gets name of the <see cref="Value"/> property.
        /// </summary>
        public override string LabelForPropertyName => nameof(Value);


        /// <summary>
        /// Gets the <see cref="Value"/>.
        /// </summary>
        public override string GetValue()
        {
            return Value;
        }


        /// <summary>
        /// Sets the <see cref="Value"/>.
        /// </summary>
        public override void SetValue(string value)
        {
            Value = value;
        }
    }
}