using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Definition of a widget for Page builder.
    /// </summary>
    public sealed class WidgetDefinition : ComponentDefinition, IPropertiesComponentDefinition, ICustomViewNameComponentDefinition
    {
        private readonly string viewName;


        /// <summary>
        /// Name of the route under which is the widget available.
        /// </summary>
        public override string RouteName { get; } = PageBuilderRoutes.WIDGETS_ROUTE_NAME;


        /// <summary>
        /// Type of the registered widget properties model.
        /// </summary>
        /// <remarks>The type needs to implement the <see cref="IWidgetProperties"/> interface.</remarks>
        public Type PropertiesType { get; internal set; }


        /// <summary>
        /// Name of the default properties action.
        /// </summary>
        public string DefaultPropertiesActionName { get; } = PageBuilderRoutes.DEFAULT_WIDGET_PROPERTIES_ACTION_NAME;


        /// <summary>
        /// Name of the properties form markup controller.
        /// </summary>
        public string PropertiesFormMarkupControllerName { get; } = PageBuilderRoutes.WIDGET_PROPERTIES_FORM_CONTROLLER_NAME;


        /// <summary>
        /// View name of the widget component.
        /// </summary> 
        string ICustomViewNameComponentDefinition.ViewName => viewName;


        /// <summary>
        /// Creates an instance of the <see cref="WidgetDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the widget definition.</param>
        /// <param name="controllerType">Type of the registered widget controller.</param>
        /// <param name="name">Name of the registered widget.</param>
        /// <param name="description">Description of the registered widget.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered widget.</param>
        public WidgetDefinition(string identifier, Type controllerType, string name, string description, string iconClass) 
            : this(identifier, controllerType, name, description, iconClass, null, null)
        {

        }


        /// <summary>
        /// Creates an instance of the <see cref="WidgetDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the widget definition.</param>
        /// <param name="controllerType">Type of the registered widget controller.</param>
        /// <param name="name">Name of the registered widget.</param>
        /// <param name="description">Description of the registered widget.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered widget.</param>
        /// <param name="propertiesType">Type of the registered widget properties model.</param>
        /// <param name="customViewName">Custom view name for the registered widget.</param>
        internal WidgetDefinition(string identifier, Type controllerType, string name, string description, string iconClass, Type propertiesType, string customViewName)
            : base(identifier, controllerType, name, description, iconClass)
        {
            if (controllerType != null)
            {
                if (!typeof(WidgetController).IsAssignableFrom(controllerType) && !TypeUtils.IsAssignableToGenericType(controllerType, typeof(WidgetController<>)))
                {
                    throw new ArgumentException($"Implementation of the {controllerType.FullName} widget controller must inherit from {nameof(WidgetController)} class.", nameof(controllerType));
                }

                PropertiesType = TypeUtils.GetMatchingGenericType(controllerType, typeof(WidgetController<>));
            }
            else
            {
                if (propertiesType != null && !typeof(IWidgetProperties).IsAssignableFrom(propertiesType))
                {
                    throw new ArgumentException($"Implementation of the widget properties must implement {nameof(IWidgetProperties)} interface.");
                }

                PropertiesType = propertiesType;
                viewName = ComponentViewNameUtils.GetViewName(identifier, PageBuilderConstants.WIDGET_VIEW_FOLDER, customViewName);
            }
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="WidgetDefinition"/> class. 
        /// </summary>
        internal WidgetDefinition()
        {
        }
    }
}
