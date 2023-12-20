using System.Collections.Generic;
using Kentico.Content.Web.Mvc;


namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface to retrieve list of registered component definitions for Page builder.
    /// </summary>
    /// <typeparam name="TDefinition">Type of the component definition.</typeparam>
    public interface IComponentDefinitionProvider<out TDefinition> 
        where TDefinition : ComponentDefinitionBase
    {
        /// <summary>
        /// Gets list of registered components.
        /// </summary>
        IEnumerable<TDefinition> GetAll();
    }
}