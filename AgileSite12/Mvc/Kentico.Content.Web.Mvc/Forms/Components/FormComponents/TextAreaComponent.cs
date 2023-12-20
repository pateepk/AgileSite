using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(TextAreaComponent.IDENTIFIER, typeof(TextAreaComponent), "{$kentico.formbuilder.component.textarea.name$}", Description = "{$kentico.formbuilder.component.textarea.description$}", IconClass = "icon-l-text", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a text area form component.
    /// </summary>
    public class TextAreaComponent : FormComponent<TextAreaProperties, string>
    {
        /// <summary>
        /// Represents the <see cref="TextAreaComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.TextArea";


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
