using System;
using System.Collections.Generic;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IValidationRuleConfigurationsXmlSerializer), typeof(ValidationRuleConfigurationsXmlSerializer), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// XML serializer for collection of <see cref="ValidationRuleConfiguration"/>s.
    /// </summary>
    public interface IValidationRuleConfigurationsXmlSerializer
    {
        /// <summary>
        /// Serializes a collection of validation rule configurations to an XML string.
        /// </summary>
        /// <param name="validationRuleConfigurations">Validation rule configurations to be serialized.</param>
        /// <returns>Returns an XML representation of the rules.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationRuleConfigurations"/> is null.</exception>
        string Serialize(IEnumerable<ValidationRuleConfiguration> validationRuleConfigurations);


        /// <summary>
        /// Deserializes a collection of validation rule configurations from an XML string.
        /// </summary>
        /// <param name="validationRuleConfigurationsXml">XML representation of the rule configurations to be deserialized.</param>
        /// <returns>Returns a collection of validation rule configurations.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="validationRuleConfigurationsXml"/> is null or an empty string.</exception>
        List<ValidationRuleConfiguration> Deserialize(string validationRuleConfigurationsXml);
    }
}
