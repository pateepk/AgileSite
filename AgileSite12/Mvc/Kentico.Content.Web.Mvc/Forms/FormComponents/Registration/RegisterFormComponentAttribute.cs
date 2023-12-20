using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Registers a form component to Form builder.
    /// </summary>
    public sealed class RegisterFormComponentAttribute : RegisterComponentAttribute
    {
        /// <summary>
        /// Description of the registered component.
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Font-icon CSS class of the registered component.
        /// </summary>
        public string IconClass { get; set; }


        /// <summary>
        /// Name of the view to render.
        /// </summary>
        /// <remarks>
        /// If not set default location is used ("FormComponents/_{Identifier}").
        /// </remarks>
        public string ViewName { get; set; }


        /// <summary>
        /// Determines if the form component can be added via Form builder UI to a form.
        /// Set to false to allow the component to be used programmatically only.
        /// True by default.
        /// </summary>
        public bool IsAvailableInFormBuilderEditor { get; set; } = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterFormComponentAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the form component.</param>
        /// <param name="formComponentType">Type of the form component. The form component must inherit the <see cref="FormComponent{TProperties, TValue}"/> class.</param>
        /// <param name="name">Name of the form component.</param>
        /// <remarks>
        /// Make sure to provide a unique identifier for the form component from the start.
        /// This identifier is used within the form configuration of components and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.ComponentName', e.g. 'Kentico.Content.TextInputComponent'.
        /// </remarks>
        public RegisterFormComponentAttribute(string identifier, Type formComponentType, string name)
            : base(identifier, formComponentType, name)
        {
        }


        /// <summary>
        /// Registers the form component during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            var definition = CreateFormComponentDefinition();

            ComponentDefinitionStore<FormComponentDefinition>.Instance.Add(definition);
        }


        /// <summary>
        /// Creates an instance of the <see cref="FormComponentDefinition"/> class which represents the registration properties specified via this attribute.
        /// </summary>
        internal FormComponentDefinition CreateFormComponentDefinition()
        {
            return new FormComponentDefinition(Identifier, MarkedType, Name)
            {
                Description = Description,
                IconClass = IconClass,
                IsAvailableInFormBuilderEditor = IsAvailableInFormBuilderEditor,
                ViewName = ViewName?.Equals(FormComponentConstants.AutomaticSystemViewName, StringComparison.Ordinal) == true ? $"~/Views/Shared/Kentico/FormComponents/_{IdentifierUtils.GetIdentifier(Identifier)}.cshtml" : ViewName
            };
        }
    }
}
