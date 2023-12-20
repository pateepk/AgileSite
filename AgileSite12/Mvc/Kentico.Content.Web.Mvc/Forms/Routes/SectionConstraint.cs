using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Checks the route constraint to validate the section controller.
    /// </summary>
    internal class SectionConstraint : IRouteConstraint
    {
        private readonly IList<string> controllerNames;


        /// <summary>
        /// Initializes a new instance of the <see cref="SectionConstraint"/> class.
        /// </summary>
        /// <param name="sectionDefinitionProvider">Section provider to be used for validating section controller.</param>
        public SectionConstraint(ISectionDefinitionProvider sectionDefinitionProvider)
        {
            controllerNames = sectionDefinitionProvider
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
                return controllerNames.Contains(value.ToString(), StringComparer.InvariantCultureIgnoreCase);
            }

            return false;
        }
    }
}
