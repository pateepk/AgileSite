using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Checks the route constraint to validate the component identifier.
    /// </summary>
    /// <remarks>Ensures controller to route data based on matching definition and includes the definition to route data tokens for further use.</remarks>
    internal class ComponentIdentifierConstraint<TDefinition> : IRouteConstraint
        where TDefinition : ComponentDefinition
    {
        private readonly IEnumerable<TDefinition> definitions;


        /// <summary>
        /// Creates an instance of <see cref="ComponentIdentifierConstraint{TDefinition}"/> class.
        /// </summary>
        public ComponentIdentifierConstraint()
        {
            definitions = ComponentDefinitionStore<TDefinition>.Instance.GetAll();
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
                var definition = definitions.FirstOrDefault(def => string.Equals(def.Identifier, value.ToString(), StringComparison.InvariantCultureIgnoreCase));
                if (definition != null)
                {
                    values["controller"] = definition.ControllerName;
                    values[PageBuilderConstants.COMPONENT_DEFINITION_ROUTE_DATA_KEY] = definition;

                    return true;
                }
            }

            return false;
        }
    }
}