using System;
using System.Web;
using System.Web.Routing;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Route constraint matching form component identifier against components provided by specified <see cref="IFormComponentDefinitionProvider"/>.
    /// </summary>
    internal class FormComponentIdentifierConstraint : IRouteConstraint
    {
        private readonly IFormComponentDefinitionProvider componentDefinitionProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentIdentifierConstraint"/> class using the specified form component definition provider.
        /// </summary>
        /// <param name="componentProvider">Component provider to be used for validating component identifier.</param>
        public FormComponentIdentifierConstraint(IFormComponentDefinitionProvider componentProvider)
        {
            componentDefinitionProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
        }


        /// <summary>
        /// Determines whether route values associated with <paramref name="parameterName"/> is a valid form component identifier.
        /// </summary>
        /// <returns>Returns true if route parameter of name <paramref name="parameterName"/> is a valid form component identifier, otherwise returns false.</returns>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(parameterName, out var value) && (value is string identifier) && (identifier != null))
            {
                return componentDefinitionProvider.Get(identifier) != null;
            }

            return false;
        }
    }
}
