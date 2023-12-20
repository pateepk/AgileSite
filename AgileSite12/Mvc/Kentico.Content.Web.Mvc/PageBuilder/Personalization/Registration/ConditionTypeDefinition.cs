using System;

using Kentico.PageBuilder.Web.Mvc.Personalization.Internal;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Definition of a personalization condition type for Page builder.
    /// </summary>
    public class ConditionTypeDefinition : ComponentDefinition
    {
        /// <summary>
        /// Name of the route under which is the condition type available.
        /// </summary>
        public override string RouteName => string.Equals(ControllerName, PageBuilderRoutes.CONDITION_TYPE_PARAMETERS_FORM_CONTROLLER_NAME, StringComparison.Ordinal) ? PageBuilderRoutes.FORM_ROUTE_NAME : PageBuilderRoutes.PERSONALIZATION_CONDITION_TYPE_ROUTE_NAME;


        /// <summary>
        /// Type of the registered condition type.
        /// </summary>
        public Type Type { get; internal set; }


        /// <summary>
        /// Hint displayed above configuration form.
        /// </summary>
        public string Hint { get; internal set; }


        /// <summary>
        /// Creates an instance of the <see cref="ConditionTypeDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the personalization condition type definition.</param>
        /// <param name="type">Type of the registered condition type.</param>
        /// <param name="controllerType">Type of the registered personalization condition type controller.</param>
        /// <param name="name">Name of the registered personalization condition type.</param>
        /// <param name="description">Description of the registered personalization condition type.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered personalization condition type.</param>
        /// <param name="hint">Hint displayed above configuration form.</param>
        public ConditionTypeDefinition(string identifier, Type type, Type controllerType, string name, string description, string iconClass, string hint) 
            : base(identifier, controllerType ?? typeof(KenticoConditionTypeParametersFormController), name, description, iconClass)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type), "The condition type must be specified.");
            if (!typeof(IConditionType).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Implementation of the {type.FullName} condition type must implement {nameof(IConditionType)} interface.", nameof(type));
            }

            // Check type for custom controller
            if (controllerType != null && !IsSubclassOfGenericType(typeof(ConditionTypeController<>), controllerType))
            {
                throw new ArgumentException($"Implementation of the {controllerType.FullName} condition type controller must inherit from ConditionTypeController class.", nameof(controllerType));
            }

            Hint = hint;
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ConditionTypeDefinition"/> class. 
        /// </summary>
        internal ConditionTypeDefinition()
        {
        }


        private bool IsSubclassOfGenericType(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == currentType)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
