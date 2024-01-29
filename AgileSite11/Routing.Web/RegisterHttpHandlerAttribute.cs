using System;
using System.Web;
using System.Web.Routing;

using CMS.Core;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Represents a route to a Kentico HTTP handler using a route template that supports inline constraints.
    /// </summary>
    /// <remarks>
    /// Route templates are compatible with ASP.NET MVC 5 route templates with the following exceptions:
    /// <list type="bullet">
    /// <item><description>Only <c>guid</c> and <c>int</c> inline constraints are supported, other constraints are ignored.</description></item>
    /// <item><description>There must be no unpaired braces except the escaped ones, otherwise an exception is thrown.</description></item>
    /// </list>
    /// Routes to Kentico HTTP handlers are excluded from URL generation, i.e. in MVC applications it is not possible
    /// to accidentally generate an URL to a Kentico HTTP handler instead of a controller action.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterHttpHandlerAttribute : Attribute, IPreInitAttribute, IRouteHandler
    {
        /// <summary>
        /// Gets the type of a Kentico HTTP handler.
        /// </summary>
        public Type MarkedType
        {
            get;
        }


        /// <summary>
        /// Gets the route template.
        /// </summary>
        public string RouteTemplate
        {
            get;
        }


        /// <summary>
        /// Gets or sets the order weight of the route.
        /// </summary>
        /// <remarks>
        /// Routes are sorted for each HTTP handler in increasing order based on the order value.
        /// Routes without the order weight specified have an order value of 0.
        /// Negative values are valid and can be used to position a route before all non-negative routes.
        /// </remarks>
        public int Order
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterHttpHandlerAttribute"/> class.
        /// </summary>
        /// <param name="routeTemplate">The route template.</param>
        /// <param name="handlerType">The type of a Kentico HTTP handler.</param>
        public RegisterHttpHandlerAttribute(string routeTemplate, Type handlerType)
        {
            RouteTemplate = routeTemplate;
            MarkedType = handlerType;
            Order = 0;
        }


        /// <summary>
        /// Adds this instance to the table with routes to Kentico HTTP handlers.
        /// </summary>
        public void PreInit()
        {
            HttpHandlerRouteTable.Default.Register(this);
        }


        /// <summary>
        /// Provides the Kentico HTTP handler that processes the request.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the request.</param>
        /// <returns>A Kentico HTTP handler that processes the request.</returns>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return (IHttpHandler)ObjectFactory.New(MarkedType);
        }
    }
}
