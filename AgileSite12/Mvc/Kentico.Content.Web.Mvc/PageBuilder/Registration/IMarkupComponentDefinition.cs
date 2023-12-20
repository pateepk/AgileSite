namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Definition of a component with markup for Page builder.
    /// </summary>
    internal interface IMarkupComponentDefinition : IComponentDefinition
    {
        /// <summary>
        /// Name of the route under which is the component available.
        /// </summary>
        string RouteName { get; }


        /// <summary>
        /// Name of the controller derived from controller class name without the <c>Controller</c> suffix.
        /// </summary>
        string ControllerName { get; }
    }
}
