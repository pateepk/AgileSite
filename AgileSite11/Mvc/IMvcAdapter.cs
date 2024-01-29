using System;
using System.Web;
using System.Web.Routing;

namespace CMS.Mvc
{
    /// <summary>
    /// MvcAdapter contract interface. Provides an access to the core MVC functionality and objects.
    /// </summary>
    public interface IMvcAdapter
    {
        #region "Methods"

        /// <summary>
        /// Gets the MVC handler.
        /// </summary>
        /// <param name="context">The request context</param>
        IHttpHandler GetMvcHandler(RequestContext context);


        /// <summary>
        /// Gets the MVC route handler.
        /// </summary>
        IRouteHandler GetMvcRouteHandler();


        /// <summary>
        /// Proceeds the default route values.
        /// </summary>
        /// <param name="values">The values</param>
        /// <param name="routeDefaults">The route defaults</param>
        void ProceedDefaultRouteValues(RouteValueDictionary values, RouteValueDictionary routeDefaults);


        /// <summary>
        /// Gets the "Optional URL parameter" type.
        /// </summary>
        object OptionalUrlParameter
        {
            get;
        }


        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="controllerName">Name of the controller</param>
        Type GetControllerType(RequestContext context, string controllerName);

        #endregion
    }
}
