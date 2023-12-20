using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Registers a personalization condition type definition to be used within Page builder feature.
    /// </summary>
    public class RegisterPersonalizationConditionTypeAttribute : RegisterComponentAttribute
    {
        /// <summary>
        /// Type of custom controller responsible for condition parameters form.
        /// </summary>
        public Type ControllerType { get; set; }


        /// <summary>
        /// Hint displayed above the configuration form in the UI.
        /// </summary>
        public string Hint { get; set; }


        /// <summary>
        /// Creates an instance of the <see cref="RegisterPersonalizationConditionTypeAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the condition type definition.</param>
        /// <param name="type">Type of the registered condition.</param>
        /// <param name="name">Name of the registered condition type.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the condition type definition from the start. 
        /// This identifier is used within the page configuration and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.ConditionTypeName', e.g. 'Kentico.Personalization.IsInPersona'.
        /// </remarks>
        public RegisterPersonalizationConditionTypeAttribute(string identifier, Type type, string name)
            : base(identifier, null, name)
        {
            MarkedType = type;
        }


        /// <summary>
        /// Registers the condition type definition during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            ComponentDefinitionStore<ConditionTypeDefinition>.Instance.Add(new ConditionTypeDefinition(Identifier, MarkedType, ControllerType, Name, Description, IconClass, Hint));
        }
    }
}
