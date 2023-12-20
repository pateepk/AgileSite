using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains methods for creating <see cref="VisibilityCondition"/>s.
    /// </summary>
    public class VisibilityConditionActivator : IVisibilityConditionActivator
    {
        private readonly IVisibilityConditionDefinitionProvider visibilityConditionDefinitionProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConditionActivator"/> class.
        /// </summary>
        /// <param name="visibilityConditionDefinitionProvider">Retrieves <see cref="VisibilityConditionDefinition"/>s.</param>
        public VisibilityConditionActivator(IVisibilityConditionDefinitionProvider visibilityConditionDefinitionProvider)
        {
            this.visibilityConditionDefinitionProvider = visibilityConditionDefinitionProvider;
        }


        /// <summary>
        /// Creates a new instance of the <see cref="VisibilityCondition"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="visibilityConditionIdentifier">Identifies <see cref="VisibilityCondition"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="VisibilityCondition"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="visibilityConditionIdentifier"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="VisibilityCondition"/> with given <paramref name="visibilityConditionIdentifier"/> is not registered in the system.</exception>
        public VisibilityCondition CreateVisibilityCondition(string visibilityConditionIdentifier)
        {
            var definition = visibilityConditionDefinitionProvider.Get(visibilityConditionIdentifier);
            if (definition == null)
            {
                throw new InvalidOperationException($"Visibility condition with identifier '{visibilityConditionIdentifier}' is not registered in the system.");
            }

            return CreateVisibilityCondition(definition);
        }


        /// <summary>
        /// Creates a new instance of the <see cref="VisibilityCondition"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="definition">Defines <see cref="VisibilityCondition"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="VisibilityCondition"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        public VisibilityCondition CreateVisibilityCondition(VisibilityConditionDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return (VisibilityCondition)Activator.CreateInstance(definition.VisibilityConditionType);
        }
    }
}
