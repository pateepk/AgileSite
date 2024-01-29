using System.Web.Routing;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Represents the contract for resolving inline constraints as instances of <see cref="IRouteConstraint"/>.
    /// </summary>
    internal interface IInlineConstraintResolver
    {
        /// <summary>
        /// Resolves the inline constraint.
        /// </summary>
        /// <param name="inlineConstraint">The inline constraint to resolve.</param>
        /// <returns>The <see cref="IRouteConstraint"/> that the inline constraint was resolved to, if applicable; otherwise, <c>null</c>.</returns>
        IRouteConstraint ResolveConstraint(string inlineConstraint);
    }
}
