using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="CheckBoxComponent"/>.
    /// </summary>
    public class CheckBoxProperties : FormComponentProperties<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Boolean"/>.
        /// </remarks>
        public CheckBoxProperties() : base(FieldDataType.Boolean)
        {
        }


        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        [DefaultValueEditingComponent(CheckBoxComponent.IDENTIFIER)]
        public override bool DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.component.checkbox.properties.text$}")]
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value indicating whether the underlying field is required. False by default.
        /// If false, the form component's implementation must accept nullable input.
        /// </summary>
        public override bool Required
        {
            get;
            set;
        }
    }
}
