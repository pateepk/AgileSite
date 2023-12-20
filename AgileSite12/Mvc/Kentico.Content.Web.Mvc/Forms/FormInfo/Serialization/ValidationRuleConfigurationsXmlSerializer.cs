using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// XML serializer for collection of <see cref="ValidationRuleConfiguration"/>s.
    /// </summary>
    public class ValidationRuleConfigurationsXmlSerializer : ConfigurationsXmlSerializer<ValidationRuleConfiguration>, IValidationRuleConfigurationsXmlSerializer
    {
        private readonly IValidationRuleDefinitionProvider ruleDefinitionProvider;


        /// <summary>
        /// Gets the name of the element containing the serialized <see cref="ValidationRuleConfiguration"/>.
        /// </summary>
        protected override string ConfigurationElementName => "ValidationRuleConfiguration";


        /// <summary>
        /// Gets the name of the element containing type identifier of the serialized <see cref="ValidationRule"/> wrapped by <see cref="ValidationRuleConfiguration"/>.
        /// </summary>
        protected override string ConfiguredObjectIdentifierElementName => "Identifier";


        /// <summary>
        /// Gets the name of the element containing the serialized <see cref="ValidationRule"/> wrapped by <see cref="ValidationRuleConfiguration"/>.
        /// </summary>
        protected override string ConfiguredObjectElementName => "ValidationRule";


        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRuleConfigurationsXmlSerializer"/> class.
        /// </summary>
        /// <param name="ruleDefinitionProvider">Provider of validation rule definitions.</param>
        public ValidationRuleConfigurationsXmlSerializer(IValidationRuleDefinitionProvider ruleDefinitionProvider)
        {
            this.ruleDefinitionProvider = ruleDefinitionProvider ?? throw new ArgumentNullException(nameof(ruleDefinitionProvider));
        }


        /// <summary>
        /// Serializes a collection of validation rule configurations to an XML string.
        /// </summary>
        /// <param name="validationRuleConfigurations">Validation rule configurations to be serialized.</param>
        /// <returns>Returns an XML representation of the rules.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationRuleConfigurations"/> is null.</exception>
        public virtual string Serialize(IEnumerable<ValidationRuleConfiguration> validationRuleConfigurations)
        {
            if (validationRuleConfigurations == null)
            {
                throw new ArgumentNullException(nameof(validationRuleConfigurations));
            }

            var result = new StringBuilder();

            foreach (var validationRuleConfiguration in validationRuleConfigurations)
            {
                Serialize(validationRuleConfiguration, validationRuleConfiguration.ValidationRule.GetType(), result);
            }

            return result.ToString();
        }


        /// <summary>
        /// Deserializes a collection of validation rule configurations from an XML string.
        /// </summary>
        /// <param name="validationRuleConfigurationsXml">XML representation of the rules to be deserialized.</param>
        /// <returns>Returns a collection of validation rule configurations.</returns>
        public virtual List<ValidationRuleConfiguration> Deserialize(string validationRuleConfigurationsXml)
        {
            List<ValidationRuleConfiguration> result = new List<ValidationRuleConfiguration>();

            if (String.IsNullOrEmpty(validationRuleConfigurationsXml))
            {
                return result;
            }

            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml($"<root>{validationRuleConfigurationsXml}</root>");

                foreach (XmlElement validationRuleConfigurationXmlNode in document.DocumentElement.ChildNodes)
                {
                    var validationRule = Deserialize(validationRuleConfigurationXmlNode);

                    result.Add(validationRule);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Deserialization of validation rules failed. See the inner exception for details. Validation rules XML:{Environment.NewLine}{Environment.NewLine}{validationRuleConfigurationsXml}", ex);
            }
        }


        /// <summary>
        /// Gets validation rule type from a <paramref name="typeIdentifier"/>.
        /// </summary>
        protected override Type GetConfiguredObjectType(string typeIdentifier)
        {
            var ruleDefinition = ruleDefinitionProvider.Get(typeIdentifier);
            if (ruleDefinition == null)
            {
                throw new InvalidOperationException($"Validation rule identifier '{typeIdentifier}' is not a known validation rule identifier.");
            }

            return ruleDefinition.ValidationRuleType;
        }


        /// <summary>
        /// Creates a new <see cref="ValidationRuleConfiguration"/> based on deserialzed <paramref name="typeIdentifier"/> and <paramref name="configuredObject"/>.
        /// </summary>
        /// <param name="typeIdentifier">Type identifier of the <paramref name="configuredObject"/>.</param>
        /// <param name="configuredObject">Deserialized configured object.</param>
        /// <returns>Returns deserialized configuration.</returns>
        protected override ValidationRuleConfiguration CreateDeserializedConfiguration(string typeIdentifier, object configuredObject)
        {
            return new ValidationRuleConfiguration(typeIdentifier, (ValidationRule)configuredObject);
        }
    }
}
