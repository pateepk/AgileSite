using System;
using System.Text;
using System.Web.Routing;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Builds routes for Kentico HTTP handlers from route templates that support inline constraints.
    /// </summary>
    internal sealed class HttpHandlerRouteBuilder
    {
        private readonly IInlineConstraintResolver mInlineConstraintResolver;


        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandlerRouteBuilder"/> class using the default inline constraint resolver.
        /// </summary>
        public HttpHandlerRouteBuilder()
        {
            mInlineConstraintResolver = new DefaultInlineConstraintResolver();
        }

        
        /// <summary>
        /// Builds a route for a Kentico HTTP handler.
        /// </summary>
        /// <param name="routeTemplate">The route template with optional inline constraints.</param>
        /// <param name="routeHandler">The route handler.</param>
        /// <returns>A route for a Kentico HTTP handler.</returns>
        public HttpHandlerRoute Build(string routeTemplate, IRouteHandler routeHandler)
        {
            if (routeTemplate == null)
            {
                throw new ArgumentNullException(nameof(routeTemplate));
            }

            if (routeHandler == null)
            {
                throw new ArgumentNullException(nameof(routeHandler));
            }

            RouteValueDictionary constraints = null;
            StringBuilder builder = new StringBuilder(routeTemplate.Length);

            int closingBracketIndex = -1;

            int openingBracketIndex = routeTemplate.IndexOf('{');
            while (openingBracketIndex >= 0)
            {
                // Ignore escaped opening braces
                if (openingBracketIndex < routeTemplate.Length - 1 && routeTemplate[openingBracketIndex + 1] == '{')
                {
                    openingBracketIndex = routeTemplate.IndexOf('{', openingBracketIndex + 2);
                    continue;
                }

                // Append a substring between the last and current route parameters to the URL template
                builder.Append(routeTemplate, closingBracketIndex + 1, openingBracketIndex - closingBracketIndex - 1);
                
                closingBracketIndex = routeTemplate.IndexOf('}', openingBracketIndex + 1);
                
                if (closingBracketIndex < 0)
                {
                    throw new ArgumentException($"The route template '{routeTemplate}' contains a parameter without a closing bracket.", nameof(routeTemplate));
                }

                int separatorIndex = routeTemplate.IndexOf(':', openingBracketIndex + 1, closingBracketIndex - openingBracketIndex - 1);
                if (separatorIndex >= 0)
                {
                    // Inline constraint was found, append a parameter without the inline constraint to the URL template
                    builder.Append(routeTemplate, openingBracketIndex, separatorIndex - openingBracketIndex).Append('}');
                    string inlineConstraint = routeTemplate.Substring(separatorIndex + 1, closingBracketIndex - separatorIndex - 1);
                    IRouteConstraint constraint = mInlineConstraintResolver.ResolveConstraint(inlineConstraint);
                    if (constraint != null)
                    {
                        if (constraints == null)
                        {
                            constraints = new RouteValueDictionary();
                        }
                        string parameterName = routeTemplate.Substring(openingBracketIndex + 1, separatorIndex - openingBracketIndex - 1);
                        constraints.Add(parameterName, constraint);
                    }
                }
                else
                {
                    // No inline constraint was found, append a parameter to the URL template
                    builder.Append(routeTemplate, openingBracketIndex, closingBracketIndex - openingBracketIndex + 1);
                }
                openingBracketIndex = routeTemplate.IndexOf('{', closingBracketIndex + 1);
            }

            // Append the rest of the route template to the URL template
            builder.Append(routeTemplate, closingBracketIndex + 1, routeTemplate.Length - closingBracketIndex - 1);

            return new HttpHandlerRoute(builder.ToString(), routeHandler)
            {
                Constraints = constraints
            };
        }
    }
}
