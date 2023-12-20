using System;
using System.Collections.Generic;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IVisibilityConditionDefinitionProvider), typeof(VisibilityConditionDefinitionProvider), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provider for retrieval of registered form component visibility condition definitions for Form builder.
    /// </summary>
    public interface IVisibilityConditionDefinitionProvider : IFormBuilderDefinitionProvider<VisibilityConditionDefinition>
    {
        /// <summary>
        /// Returns all <see cref="VisibilityConditionDefinition"/>s that evaluates custom conditions not depending on the another field. 
        /// </summary>
        IEnumerable<VisibilityConditionDefinition> GetCustomVisibilityConditionDefinitions();

        
        /// <summary>
        /// Returns <see cref="VisibilityConditionDefinition"/>s that define <see cref="VisibilityCondition"/>s that inherits from <see cref="AnotherFieldVisibilityCondition{TValue}"/> and targets given <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">Value type for which to return <see cref="VisibilityConditionDefinition"/>s that define <see cref="VisibilityCondition"/>s that inherits from <see cref="AnotherFieldVisibilityCondition{TValue}"/>.</param>
        /// <see cref="AnotherFieldVisibilityCondition{TValue}"/>
        /// <see cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldGuid"/>
        IEnumerable<VisibilityConditionDefinition> GetAnotherFieldVisibilityConditionDefinitions(Type targetType);
    }
}
