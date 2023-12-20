using System;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates <see cref="Mvc.VisibilityCondition"/> with its <see cref="VisibilityConditionDefinition"/> identifier.
    /// </summary>
    /// <seealso cref="Mvc.VisibilityCondition"/>
    /// <seealso cref="VisibilityConditionDefinition"/>
    [JsonConverter(typeof(VisibilityConditionConfigurationJsonConverter))]
    public sealed class VisibilityConditionConfiguration
    {
        /// <summary>
        /// Gets or sets identifier of the <see cref="VisibilityCondition"/>.
        /// </summary>
        public string Identifier { get; set; }


        /// <summary>
        /// Gets or sets visibility condition.
        /// </summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public VisibilityCondition VisibilityCondition { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConditionConfiguration"/> class.
        /// This constructor serves for the purpose of deserialization.
        /// </summary>
        public VisibilityConditionConfiguration() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConditionConfiguration"/> class.
        /// </summary>
        /// <param name="identifier">Identifies type of the <paramref name="visibilityCondition"/>.</param>
        /// <param name="visibilityCondition">Visibility condition.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="identifier"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="visibilityCondition"/> is null.</exception>
        public VisibilityConditionConfiguration(string identifier, VisibilityCondition visibilityCondition)
        {
            Identifier = String.IsNullOrEmpty(identifier) ? throw new ArgumentException(nameof(identifier)) : identifier;
            VisibilityCondition = visibilityCondition ?? throw new ArgumentNullException(nameof(visibilityCondition));
        }


        /// <summary>
        /// Returns GUID of the field to which <see cref="VisibilityCondition"/> depends on.
        /// Null is returned if <see cref="VisibilityCondition"/> does not implement <see cref="AnotherFieldVisibilityCondition{TValue}"/>
        /// or <see cref="VisibilityCondition"/> is null.
        /// </summary>
        /// <seealso cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldGuid"/>
        internal Guid? GetDependeeFieldGuid()
        {
            return (VisibilityCondition as IAnotherFieldVisibilityCondition)?.DependeeFieldGuid;
        }
    }
}
