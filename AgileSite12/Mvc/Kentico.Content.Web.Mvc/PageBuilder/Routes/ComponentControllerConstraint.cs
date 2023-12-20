using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Checks the route constraint to validate the component controller.
    /// </summary>
    internal class ComponentControllerConstraint<TDefinition> : IRouteConstraint
    where TDefinition : ComponentDefinition
    {
        private readonly IList<string> controllers;


        /// <summary>
        /// Creates an instance of <see cref="ComponentControllerConstraint{TDefinition}"/> class.
        /// </summary>
        public ComponentControllerConstraint()
        {
            controllers = ComponentDefinitionStore<TDefinition>.Instance
                              .GetAll()
                              .Select(w => w.ControllerName)
                              .ToList();
        }


        /// <summary>
        /// Determines whether the URL parameter contains a valid value for this constraint.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <param name="route">The object that this constraint belongs to.</param>
        /// <param name="parameterName">The name of the parameter that is being checked.</param>
        /// <param name="values">An object that contains the parameters for the URL.</param>
        /// <param name="routeDirection">An object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated.</param>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(parameterName, out var value) && value != null)
            {
                return controllers.Contains(value.ToString(), StringComparer.InvariantCultureIgnoreCase);
            }

            // If the controller is not provided do not block the route, the conroller is optional
            return true;
        }
    }
}