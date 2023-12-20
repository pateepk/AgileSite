using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="TextAreaComponent"/>.
    /// </summary>
    public class TextAreaProperties : FormComponentProperties<string>
    {
        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        [DefaultValueEditingComponent(TextAreaComponent.IDENTIFIER)]
        public override string DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value displayed as placeholder in the input.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.placeholder.label$}")]
        public override string Placeholder
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextAreaProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.LongText"/>.
        /// </remarks>
        public TextAreaProperties()
            : base(FieldDataType.LongText)
        {
        }
    }
}
