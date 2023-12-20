using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="EmailInputComponent"/>.
    /// </summary>
    public class EmailInputProperties : FormComponentProperties<string>
    {
        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        [DefaultValueEditingComponent(EmailInputComponent.IDENTIFIER)]
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
        /// Initializes a new instance of the <see cref="EmailInputProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Text"/> and size 500.
        /// </remarks>
        public EmailInputProperties()
            : base(FieldDataType.Text, 500)
        {
        }
    }
}
