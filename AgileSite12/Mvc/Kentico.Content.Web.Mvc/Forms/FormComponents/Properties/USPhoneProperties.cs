using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="USPhoneComponent"/>.
    /// </summary>
    public class USPhoneProperties : FormComponentProperties<string>
    {
        /// <summary>
        /// Value used for input masking.
        /// </summary>
        public const string INPUT_MASK = "(999) 999-9999";


        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        public override string DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="USPhoneProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Text"/> and size equal to length of <see cref="INPUT_MASK"/>.
        /// </remarks>
        public USPhoneProperties()
            : base(FieldDataType.Text, INPUT_MASK.Length)
        { }
    }
}
