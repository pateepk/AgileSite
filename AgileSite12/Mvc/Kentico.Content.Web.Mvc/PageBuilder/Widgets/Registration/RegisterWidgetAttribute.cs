using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Registers a widget definition to be used within Page builder feature.
    /// </summary>
    public sealed class RegisterWidgetAttribute : RegisterComponentAttribute
    {
        private readonly Type propertiesType;
        private readonly string customViewName;


        /// <summary>
        /// Creates an instance of the <see cref="RegisterWidgetAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the widget definition.</param>
        /// <param name="controllerType">Type of the widget controller to register.</param>
        /// <param name="name">Name of the registered widget.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the widget definition from the start. 
        /// This identifier is used within the page configuration of widgets and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.WidgetName', e.g. 'Kentico.Content.ArticlesListWidget'.
        /// </remarks>
        public RegisterWidgetAttribute(string identifier, Type controllerType, string name)
            : base(identifier, controllerType, name)
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="RegisterWidgetAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the widget definition.</param>
        /// <param name="name">Name of the registered widget.</param>
        /// <param name="propertiesType">Type of the registered widget properties model. The type needs to implement the <see cref="IWidgetProperties"/> interface.</param>
        /// <param name="customViewName">Custom view name for the registered widget. If not specified, the default system view name in format '~/Shared/Widgets/_{identifier}' is used.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the widget definition from the start. 
        /// This identifier is used within the page configuration and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.WidgetName', e.g. 'Kentico.Content.ArticlesListWidget'.
        /// 
        /// Use this attribute when simple widget is being developed and no custom controller is needed. 
        /// </remarks>
        public RegisterWidgetAttribute(string identifier, string name, Type propertiesType = null, string customViewName = null)
            : base(identifier, null, name)
        {
            this.propertiesType = propertiesType;
            this.customViewName = customViewName;
        }


        /// <summary>
        /// Registers the widget definition during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            ComponentDefinitionStore<WidgetDefinition>.Instance.Add(new WidgetDefinition(Identifier, MarkedType, Name, Description, IconClass, propertiesType, customViewName));
        }
    }
}
