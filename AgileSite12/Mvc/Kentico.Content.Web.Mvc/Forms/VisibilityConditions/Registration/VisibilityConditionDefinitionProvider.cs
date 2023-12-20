using System;
using System.Collections.Generic;
using System.Linq;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Retrieves list of registered component visibility condition definitions for Form builder.
    /// </summary>
    public sealed class VisibilityConditionDefinitionProvider : IVisibilityConditionDefinitionProvider
    {
        private readonly ComponentDefinitionStore<VisibilityConditionDefinition> store;


        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConditionDefinitionProvider"/> class.
        /// </summary>
        public VisibilityConditionDefinitionProvider()
            : this (ComponentDefinitionStore<VisibilityConditionDefinition>.Instance)
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConditionDefinitionProvider"/> class using given <paramref name="store"/>.
        /// </summary>
        internal VisibilityConditionDefinitionProvider(ComponentDefinitionStore<VisibilityConditionDefinition> store)
        {
            this.store = store;
        }


        /// <summary>
        /// Gets a form component visibility condition definition by its identifier.
        /// </summary>
        /// <param name="identifier">Identifier of the form component visibility condition definition to retrieve.</param>
        /// <returns>Returns form component visibility condition definition with given identifier, or null when not found.</returns>
        public VisibilityConditionDefinition Get(string identifier)
        {
            return store.Get(identifier);
        }


        /// <summary>
        /// Gets an enumeration of all registered form control visibility condition definitions.
        /// </summary>
        public IEnumerable<VisibilityConditionDefinition> GetAll()
        {
            return store.GetAll();
        }


        /// <summary>
        /// Returns <see cref="VisibilityConditionDefinition"/>s that define <see cref="VisibilityCondition"/>s that inherits from <see cref="AnotherFieldVisibilityCondition{TValue}"/> and targets given <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">Value type for which to return <see cref="VisibilityConditionDefinition"/>s that define <see cref="VisibilityCondition"/>s that inherits from <see cref="AnotherFieldVisibilityCondition{TValue}"/>.</param>
        /// <see cref="AnotherFieldVisibilityCondition{TValue}"/>
        /// <see cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldGuid"/>
        public IEnumerable<VisibilityConditionDefinition> GetAnotherFieldVisibilityConditionDefinitions(Type targetType)
        {
            foreach (var definition in store.GetAll().OrderBy(d => d.Name))
            {
                var type = definition.VisibilityConditionType.FindTypeByGenericDefinition(typeof(AnotherFieldVisibilityCondition<>));
                if (type == null)
                {
                    continue;
                }

                var visibilityConditionApplicableType = type.GetGenericArguments()[0];
                if (visibilityConditionApplicableType == targetType || (visibilityConditionApplicableType.FindTypeByGenericDefinition(typeof(Nullable<>))?.GetGenericArguments()[0] == targetType))
                {
                    yield return definition;
                }
            }
        }


        /// <summary>
        /// Returns all <see cref="VisibilityConditionDefinition"/>s that evaluates custom conditions not depending on the another field.
        /// </summary>
        public IEnumerable<VisibilityConditionDefinition> GetCustomVisibilityConditionDefinitions()
        {
            return store.GetAll().Where(d => !d.IsDefaultVisibilityCondition() && !d.IsDependingOnAnotherField()).OrderBy(d => d.Name);
        }
    }
}