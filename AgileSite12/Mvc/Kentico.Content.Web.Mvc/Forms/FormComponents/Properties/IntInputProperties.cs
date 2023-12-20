using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="IntInputComponent"/>.
    /// </summary>
    public class IntInputProperties : FormComponentProperties<int?>
    {
        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        [DefaultValueEditingComponent(IntInputComponent.IDENTIFIER)]
        public override int? DefaultValue
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
        /// Initializes a new instance of the <see cref="IntInputProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Integer"/>.
        /// </remarks>
        public IntInputProperties()
            : base(FieldDataType.Integer)
        {
        }
    }
}