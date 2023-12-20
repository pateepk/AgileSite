using System;
using System.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Definition of a section for Page builder.
    /// </summary>
    public sealed class SectionDefinition : ComponentDefinition, IPropertiesComponentDefinition, ICustomViewNameComponentDefinition
    {
        private readonly string viewName;


        /// <summary>
        /// Name of the route under which is the section available.
        /// </summary>
        public override string RouteName { get; } = PageBuilderRoutes.SECTIONS_ROUTE_NAME;


        /// <summary>
        /// Type of the registered section properties model.
        /// </summary>
        /// <remarks>The type needs to implement the <see cref="ISectionProperties"/> interface.</remarks>
        public Type PropertiesType { get; internal set; }


        /// <summary>
        /// Name of the default properties action.
        /// </summary>
        public string DefaultPropertiesActionName { get; } = PageBuilderRoutes.DEFAULT_SECTION_PROPERTIES_ACTION_NAME;


        /// <summary>
        /// Name of the properties form markup controller.
        /// </summary>
        public string PropertiesFormMarkupControllerName { get; } = PageBuilderRoutes.SECTION_PROPERTIES_FORM_CONTROLLER_NAME;


        /// <summary>
        /// View name of the section component.
        /// </summary> 
        string ICustomViewNameComponentDefinition.ViewName => viewName;


        /// <summary>
        /// Creates an instance of the <see cref="SectionDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the section definition.</param>
        /// <param name="controllerType">Type of the registered section controller.</param>
        /// <param name="name">Name of the registered section.</param>
        /// <param name="description">Description of the registered section.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered section.</param>
        public SectionDefinition(string identifier, Type controllerType, string name, string description, string iconClass)
            : this(identifier, controllerType, name, description, iconClass, null, null)
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="SectionDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the section definition.</param>
        /// <param name="controllerType">Type of the registered section controller.</param>
        /// <param name="name">Name of the registered section.</param>
        /// <param name="description">Description of the registered section.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered section.</param>
        /// <param name="propertiesType">Type of the registered section properties model.</param>
        /// <param name="customViewName">Custom view name for the registered section.</param>
        internal SectionDefinition(string identifier, Type controllerType, string name, string description, string iconClass, Type propertiesType, string customViewName)
            : base(identifier, controllerType, name, description, iconClass)
        {
            if (controllerType != null)
            {
                if (!TypeUtils.IsAssignableToGenericType(controllerType, typeof(SectionController<>)) && !typeof(Controller).IsAssignableFrom(controllerType))
                {
                    throw new ArgumentException($"Implementation of the {controllerType.FullName} section controller must inherit from SectionController or {nameof(Controller)} class.", nameof(controllerType));
                }

                PropertiesType = TypeUtils.GetMatchingGenericType(controllerType, typeof(SectionController<>));
            }
            else
            {
                if (propertiesType != null && !typeof(ISectionProperties).IsAssignableFrom(propertiesType))
                {
                    throw new ArgumentException($"Implementation of the section properties must implement {nameof(ISectionProperties)} interface.");
                }

                PropertiesType = propertiesType;
                viewName = ComponentViewNameUtils.GetViewName(identifier, PageBuilderConstants.SECTION_VIEW_FOLDER, customViewName);
            }
        }


        /// <summary>
        /// Creates an empty instance o the <see cref="SectionDefinition"/> class. 
        /// </summary>
        internal SectionDefinition()
        {
        }
    }
}
