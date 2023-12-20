using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(IntInputComponent.IDENTIFIER, typeof(IntInputComponent), "{$kentico.formbuilder.component.intinput.name$}", Description = "{$kentico.formbuilder.component.intinput.description$}", IconClass = "icon-l-text", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a numeric input form component.
    /// </summary>
    public class IntInputComponent : FormComponent<IntInputProperties, int?>
    {
        /// <summary>
        /// Represents the <see cref="IntInputComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.IntInput";


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public int? Number { get; set; }


        /// <summary>
        /// Gets name of the <see cref="Number"/> property.
        /// </summary>
        public override string LabelForPropertyName => nameof(Number);


        /// <summary>
        /// Gets the <see cref="Number"/>.
        /// </summary>
        public override int? GetValue()
        {
            return Number;
        }


        /// <summary>
        /// Sets the <see cref="Number"/>.
        /// </summary>
        public override void SetValue(int? value)
        {
            Number = value;
        }
    }
}