using System;

using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Definition of a component for Page builder.
    /// </summary>
    public abstract class ComponentDefinition : ComponentDefinitionBase, IMarkupComponentDefinition
    {
        /// <summary>
        /// Name of the route under which is the component available.
        /// </summary>
        public abstract string RouteName { get; }


        /// <summary>
        /// Name of the controller derived from controller class name without the <c>Controller</c> suffix.
        /// </summary>
        public string ControllerName { get; internal set; }


        /// <summary>
        /// Gets the fully qualified name of the component controller type, including its namespace without the assembly name.
        /// </summary>
        public string ControllerFullName { get; internal set; }


        /// <summary>
        /// Description of the registered component.
        /// </summary> 
        public string Description { get; internal set; }


        /// <summary>
        /// Icon CSS class of the registered component.
        /// </summary> 
        public string IconClass { get; internal set; }


        /// <summary>
        /// Creates an instance of the <see cref="ComponentDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the component definition.</param>
        /// <param name="controllerType">Type of the registered component controller.</param>
        /// <param name="name">Name of the registered component.</param>
        /// <param name="description">Description of the registered component.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered component.</param>
        protected ComponentDefinition(string identifier, Type controllerType, string name, string description, string iconClass)
            :base(identifier, name)
        {
            if (controllerType != null)
            {
                ValidateController(controllerType);

                var suffixRemover = new ControllerSuffixRemover();
                ControllerName = suffixRemover.Remove(controllerType.Name);
                ControllerFullName = controllerType.FullName;
            }
            else
            {
                ControllerName = PageBuilderConstants.COMPONENT_DEFAULT_CONTROLLER_NAME;
                ControllerFullName = typeof(KenticoComponentDefaultController).FullName;
            }

            Description = description;
            IconClass = iconClass;
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ComponentDefinition"/> class.
        /// </summary>
        internal ComponentDefinition()
        {
        }


        private void ValidateController(Type controllerType)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType), "The controller type must be specified.");
            }

            if (controllerType.IsAbstract)
            {
                throw new ArgumentException($"Implementation of the {controllerType.FullName} component controller cannot be abstract.", nameof(controllerType));
            }

            if (!controllerType.IsPublic)
            {
                throw new ArgumentException($"Implementation of the {controllerType.FullName} component controller must be public.", nameof(controllerType));
            }

            if (!controllerType.Name.EndsWith(ControllerSuffixRemover.CONTROLLER_NAME_SUFFIX, StringComparison.OrdinalIgnoreCase))
            {
               throw new ArgumentException($"Implementation of the {controllerType.FullName} component controller must be named by using the \"{ControllerSuffixRemover.CONTROLLER_NAME_SUFFIX}\" suffix.", nameof(controllerType));
            }
        }
    }
}
