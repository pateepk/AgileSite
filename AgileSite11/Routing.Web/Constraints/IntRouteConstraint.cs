using System;
using System.Globalization;
using System.Web;
using System.Web.Routing;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Constrains a route parameter to represent only <see cref="Int32"/> values.
    /// </summary>
    internal sealed class IntRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Determines whether this instance matches a specified route.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="route">The route to match.</param>
        /// <param name="parameterName">The name of the route parameter.</param>
        /// <param name="values">A list of route parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns><c>true</c> if this instance matches a specified route; otherwise, <c>false</c>.</returns>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (parameterName == null)
            {
                throw new ArgumentException("parameterName");
            }

            if (values == null)
            {
                throw new ArgumentException("values");
            }

            object value;
            if (values.TryGetValue(parameterName, out value) && value != null)
            {
                if (value is Int32)
                {
                    return true;
                }

                int result;
                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);

                return Int32.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
            }

            return false;
        }
    }
}
