namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Definition of a component for Page builder.
    /// </summary>
    internal interface IComponentDefinition
    {
        /// <summary>
        /// Unique identifier of the component definition.
        /// </summary> 
        string Identifier { get; }
    }
}
