using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Definition of a registered form component visibility condition.
    /// </summary>
    public class VisibilityConditionDefinition : ComponentDefinitionBase, IFormBuilderDefinition
    {
        /// <summary>
        /// Gets the type of the form component visibility condition. The type inherits <see cref="VisibilityCondition"/>.
        /// </summary>
        public Type VisibilityConditionType { get; }


        Type IFormBuilderDefinition.DefinitionType => VisibilityConditionType;


        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConditionDefinition"/> class using given identifier, form component visibility condition type and name.
        /// </summary>
        /// <param name="identifier">Unique identifier of the form component visibility condition.</param>
        /// <param name="visibilityConditionType">Type of the form component visibility condition.</param>
        /// <param name="name">Name of the form component visibility condition.</param>
        /// <exception cref="ArgumentException">
        /// <para>Specified <paramref name="identifier"/> is null, an empty string or identifier does not specify a valid code name.</para>
        /// <para>-or-</para>
        /// <para>Specified <paramref name="visibilityConditionType"/> does not inherit <see cref="VisibilityCondition"/>, is an abstract type or is a generic type which is not constructed.</para>
        /// <para>-or-</para>
        /// <para>Specified <paramref name="name"/> is null or an empty string.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="visibilityConditionType"/> is null.</exception>
        public VisibilityConditionDefinition(string identifier, Type visibilityConditionType, string name)
            : base(identifier, name)
        {
            ValidateVisibilityConditionType(visibilityConditionType);

            VisibilityConditionType = visibilityConditionType;
        }


        private void ValidateVisibilityConditionType(Type visibilityConditionType)
        {
            if (visibilityConditionType == null)
            {
                throw new ArgumentNullException(nameof(visibilityConditionType), "The form component visibility condition type must be specified.");
            }

            if (!typeof(VisibilityCondition).IsAssignableFrom(visibilityConditionType))
            {
                throw new ArgumentException($"Implementation of the '{visibilityConditionType.FullName}' form component visibility condition must inherit the '{typeof(VisibilityCondition).FullName}' class.", nameof(visibilityConditionType));
            }

            if (visibilityConditionType.IsAbstract)
            {
                throw new ArgumentException($"Implementation of the '{visibilityConditionType.FullName}' form component visibility condition type cannot be abstract.", nameof(visibilityConditionType));
            }

            if (visibilityConditionType.IsGenericType && !visibilityConditionType.IsConstructedGenericType)
            {
                throw new ArgumentException($"Implementation of the '{visibilityConditionType.FullName}' form component visibility condition must be a constructed generic type.", nameof(visibilityConditionType));
            }
        }
    }
}
