namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Definition of a component which can use a custom view from either predefined system location or defined by the registration.
    /// </summary>
    internal interface ICustomViewNameComponentDefinition
    {
        /// <summary>
        /// Gets component view name.
        /// </summary> 
        string ViewName { get; }
    }
}
