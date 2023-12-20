using System;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IVisibilityConditionActivator), typeof(VisibilityConditionActivator), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines methods for creating <see cref="VisibilityCondition"/>s.
    /// </summary>
    public interface IVisibilityConditionActivator
    {
        /// <summary>
        /// Creates a new instance of the <see cref="VisibilityCondition"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="visibilityConditionIdentifier">Identifies <see cref="VisibilityCondition"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="VisibilityCondition"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="visibilityConditionIdentifier"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="VisibilityCondition"/> with given <paramref name="visibilityConditionIdentifier"/> is not registered in the system.</exception>
        VisibilityCondition CreateVisibilityCondition(string visibilityConditionIdentifier);


        /// <summary>
        /// Creates a new instance of the <see cref="VisibilityCondition"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="definition">Defines <see cref="VisibilityCondition"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="VisibilityCondition"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        VisibilityCondition CreateVisibilityCondition(VisibilityConditionDefinition definition);
    }
}
