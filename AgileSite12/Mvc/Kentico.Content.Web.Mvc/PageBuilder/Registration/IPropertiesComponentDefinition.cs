using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Definition of a component with properties for Page builder.
    /// </summary>
    internal interface IPropertiesComponentDefinition : IComponentDefinition
    {
        /// <summary>
        /// Name of the default properties action.
        /// </summary>
        string DefaultPropertiesActionName { get; }


        /// <summary>
        /// Name of the properties form markup controller.
        /// </summary>
        string PropertiesFormMarkupControllerName { get; }


        /// <summary>
        /// Type of the registered component properties model.
        /// </summary>
        /// <remarks>The type needs to implement the <see cref="IComponentProperties"/> interface.</remarks>
        Type PropertiesType { get; }
    }
}
