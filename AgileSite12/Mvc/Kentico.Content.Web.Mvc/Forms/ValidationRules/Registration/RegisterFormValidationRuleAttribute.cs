using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Registers a form component validation rule to Form builder.
    /// </summary>
    public sealed class RegisterFormValidationRuleAttribute : RegisterComponentAttribute
    {
        /// <summary>
        /// Description of the registered validation rule.
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterFormValidationRuleAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the form component validation rule.</param>
        /// <param name="validationRuleType">Type of the form component validation rule. The validation rule must inherit the <see cref="ValidationRule"/> class.</param>
        /// <param name="name">Name of the form component validation rule.</param>
        /// <remarks>
        /// Make sure to provide a unique identifier for the form component validation rule from the start.
        /// This identifier is used within the form configuration of components and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.ValidationRule', e.g. 'Kentico.Content.RegularExpressionValidationRule'.
        /// </remarks>
        public RegisterFormValidationRuleAttribute(string identifier, Type validationRuleType, string name)
            : base(identifier, validationRuleType, name)
        {
        }


        /// <summary>
        /// Registers the validation rule during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            ComponentDefinitionStore<ValidationRuleDefinition>.Instance.Add(
                new ValidationRuleDefinition(Identifier, MarkedType, Name)
                {
                    Description = Description
                }
            );
        }
    }
}
