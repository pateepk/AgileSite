using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Registers a form component visibility condition to Form builder.
    /// </summary>
    public sealed class RegisterFormVisibilityConditionAttribute : RegisterComponentAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterFormVisibilityConditionAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the form component visibility condition.</param>
        /// <param name="visibilityConditionType">Type of the form component visibility condition. The visibility condition must inherit the <see cref="VisibilityCondition"/> class.</param>
        /// <param name="name">Name of the form component visibility condition.</param>
        /// <remarks>
        /// Make sure to provide a unique identifier for the form component visibility condition from the start.
        /// This identifier is used within the form configuration of components and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.VisibilityCondition', e.g. 'Kentico.Content.IsInPersonaVisibilityCondition'.
        /// </remarks>
        public RegisterFormVisibilityConditionAttribute(string identifier, Type visibilityConditionType, string name)
            : base(identifier, visibilityConditionType, name)
        {
        }


        /// <summary>
        /// Registers the visibility condition during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            ComponentDefinitionStore<VisibilityConditionDefinition>.Instance.Add(
                new VisibilityConditionDefinition(Identifier, MarkedType, Name)
            );
        }
    }
}
