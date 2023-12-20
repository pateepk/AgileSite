using System;

using Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Definition of a page template for Page builder feature.
    /// </summary>
    public sealed class PageTemplateDefinition : ComponentDefinition, IPropertiesComponentDefinition, ICustomViewNameComponentDefinition
    {
        private readonly string viewName;


        /// <summary>
        /// There is no route for page templates. The template markup is rendered directly.
        /// </summary>
        public override string RouteName { get; } = null;


        /// <summary>
        /// Type of the registered page template properties model.
        /// </summary>
        /// <remarks>The type needs to implement the <see cref="IPageTemplateProperties"/> interface.</remarks>
        public Type PropertiesType { get; internal set; }


        /// <summary>
        /// Name of the default properties action.
        /// </summary>
        public string DefaultPropertiesActionName { get; } = PageTemplateRoutes.DEFAULT_TEMPLATE_PROPERTIES_ACTION_NAME;


        /// <summary>
        /// Name of the properties form markup controller.
        /// </summary>
        public string PropertiesFormMarkupControllerName { get; } = PageTemplateRoutes.TEMPLATE_PROPERTIES_FORM_CONTROLLER_NAME;


        /// <summary>
        /// View name of the page template component.
        /// </summary> 
        string ICustomViewNameComponentDefinition.ViewName => viewName;


        /// <summary>
        /// Creates an instance of the <see cref="PageTemplateDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the page template definition.</param>
        /// <param name="controllerType">Type of the registered page template controller.</param>
        /// <param name="name">Name of the registered page template.</param>
        /// <param name="description">Description of the registered page template.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered page template.</param>
        public PageTemplateDefinition(string identifier, Type controllerType, string name, string description, string iconClass)
            : this(identifier, controllerType, name, description, iconClass , null, null)
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="PageTemplateDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the page template definition.</param>
        /// <param name="controllerType">Type of the registered page template controller.</param>
        /// <param name="name">Name of the registered page template.</param>
        /// <param name="description">Description of the registered page template.</param>
        /// <param name="iconClass">Font-icon CSS class of the registered page template.</param>
        /// <param name="propertiesType">Type of the registered page template properties model.</param>
        /// <param name="customViewName">Custom view name for the registered page template.</param>
        internal PageTemplateDefinition(string identifier, Type controllerType, string name, string description, string iconClass, Type propertiesType, string customViewName)
            : base(identifier, controllerType, name, description, iconClass)
        {
            if (controllerType != null)
            {
                if (!typeof(PageTemplateController).IsAssignableFrom(controllerType) && !TypeUtils.IsAssignableToGenericType(controllerType, typeof(PageTemplateController<>)))
                {
                    throw new ArgumentException($"Implementation of the {controllerType.FullName} page template controller must inherit from {nameof(PageTemplateController)} class.", nameof(controllerType));
                }

                PropertiesType = TypeUtils.GetMatchingGenericType(controllerType, typeof(PageTemplateController<>));
            }
            else
            {
                ControllerName = PageBuilderConstants.PAGE_TEMPLATE_DEFAULT_CONTROLLER_NAME;
                ControllerFullName = typeof(KenticoPageTemplateDefaultController).FullName;

                if (propertiesType != null && !typeof(IPageTemplateProperties).IsAssignableFrom(propertiesType))
                {
                    throw new ArgumentException($"Implementation of the page template properties must implement {nameof(IPageTemplateProperties)} interface.");
                }

                PropertiesType = propertiesType;
                viewName = ComponentViewNameUtils.GetViewName(identifier, PageBuilderConstants.PAGE_TEMPLATE_VIEW_FOLDER, customViewName);
            }
        }


        /// <summary>
        /// Creates an empty instance o the <see cref="PageTemplateDefinition"/> class. 
        /// </summary>
        internal PageTemplateDefinition()
        {
        }
    }
}
