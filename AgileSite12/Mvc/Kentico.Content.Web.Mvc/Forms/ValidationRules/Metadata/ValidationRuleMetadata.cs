using System;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Metadata describing a validation rule for the client.
    /// </summary>
    /// <seealso cref="ValidationRule{TValue}"/>
    /// <seealso cref="ValidationRuleDefinition"/>
    public sealed class ValidationRuleMetadata
    {
        /// <summary>
        /// Validation rule identifier as specified in <see cref="ValidationRuleDefinition"/>.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; internal set; }


        /// <summary>
        /// Validation rule name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }


        /// <summary>
        /// Validation rule description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }


        /// <summary>
        /// Type of value this validation rule is supposed to validate.
        /// </summary>
        /// <remarks>
        /// Full name of type normalized to lower case is expected.
        /// </remarks>
        [JsonIgnore]
        public string ValidatedDataType { get; internal set; }
    }
}
