using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER, typeof(InvalidComponent), "{$kentico.formbuilder.component.invalidcomponent.name$}", IsAvailableInFormBuilderEditor = false, ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents an invalid form component.
    /// </summary>
    public class InvalidComponent : FormComponent<InvalidComponentProperties, string>
    {
        /// <summary>
        /// Returns empty string as the component does not contain any input.
        /// </summary>
        public override string LabelForPropertyName => "";


        /// <summary>
        /// Initializes a new instance of <see cref="InvalidComponent"/>.
        /// </summary>
        public InvalidComponent()
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="InvalidComponent"/> with given <paramref name="properties"/>.
        /// </summary>
        public InvalidComponent(InvalidComponentProperties properties)
        {
            Properties = properties;
        }


        /// <summary>
        /// Gets the message describing the error of the original component.
        /// </summary>
        public override string GetValue()
        {
            return Properties.ErrorMessage;
        }


        /// <summary>
        /// Sets the message describing the error of the original component.
        /// </summary>
        public override void SetValue(string value)
        {
            Properties.ErrorMessage = value;
        }
    }
}
