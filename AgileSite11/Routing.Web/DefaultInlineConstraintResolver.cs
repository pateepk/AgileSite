using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Resolves inline constraints as instances of <see cref="IRouteConstraint"/>.
    /// </summary>
    internal sealed class DefaultInlineConstraintResolver : IInlineConstraintResolver
    {
        private readonly Dictionary<string, Type> mConstraintTypes = GetDefaultConstraintTypes();


        /// <summary>
        /// Resolves the inline constraint.
        /// </summary>
        /// <param name="inlineConstraint">The inline constraint to resolve.</param>
        /// <returns>The <see cref="IRouteConstraint"/> that the inline constraint was resolved to, if applicable; otherwise, <c>null</c>.</returns>
        public IRouteConstraint ResolveConstraint(string inlineConstraint)
        {
            if (inlineConstraint == null)
            {
                throw new ArgumentNullException(nameof(inlineConstraint));
            }

            Type constraintType;
            
            if (!mConstraintTypes.TryGetValue(inlineConstraint, out constraintType))
            {
                return null;
            }

            if (!typeof(IRouteConstraint).IsAssignableFrom(constraintType))
            {
                throw new InvalidOperationException($"The constraint type '{constraintType.Name}' which is mapped to constraint key '{inlineConstraint}' must implement the IRouteConstraint interface.");
            }

            return (IRouteConstraint)Activator.CreateInstance(constraintType);
        }


        private static Dictionary<string, Type> GetDefaultConstraintTypes()
        {
            return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                {"guid", typeof(GuidRouteConstraint)},
                {"int", typeof(IntRouteConstraint)}
            };
        }
    }
}
