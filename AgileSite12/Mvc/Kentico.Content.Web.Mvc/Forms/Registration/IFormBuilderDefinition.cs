using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Definition of a component with markup for Form builder.
    /// </summary>
    public interface IFormBuilderDefinition
    {
        /// <summary>
        /// Unique identifier of the component definition.
        /// </summary> 
        string Identifier { get; }


        /// <summary>
        /// Gets the type of the component.
        /// </summary>
        Type DefinitionType { get; }


        /// <summary>
        /// Name of the registered component.
        /// </summary> 
        string Name { get; }
    }
}
