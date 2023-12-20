using System;
using System.Text;
using System.Xml;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// XML serializer for <see cref="VisibilityConditionConfiguration"/>.
    /// </summary>
    public class VisibilityConditionConfigurationXmlSerializer : ConfigurationsXmlSerializer<VisibilityConditionConfiguration>, IVisibilityConditionConfigurationXmlSerializer
    {
        private readonly IVisibilityConditionDefinitionProvider conditionDefinitionProvider;


        /// <summary>
        /// Gets the name of the element containing the serialized <see cref="VisibilityConditionConfiguration"/>.
        /// </summary>
        protected override string ConfigurationElementName => "VisibilityConditionConfiguration";


        /// <summary>
        /// Gets the name of the element containing type identifier of the serialized <see cref="VisibilityCondition"/> wrapped by <see cref="VisibilityConditionConfiguration"/>.
        /// </summary>
        protected override string ConfiguredObjectIdentifierElementName => "Identifier";


        /// <summary>
        /// Gets the name of the element containing the serialized <see cref="VisibilityCondition"/> wrapped by <see cref="VisibilityConditionConfiguration"/>.
        /// </summary>
        protected override string ConfiguredObjectElementName => "VisibilityCondition";




        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConditionConfigurationXmlSerializer"/> class.
        /// </summary>
        /// <param name="conditionDefinitionProvider">Provider of visibility condition definitions.</param>
        public VisibilityConditionConfigurationXmlSerializer(IVisibilityConditionDefinitionProvider conditionDefinitionProvider)
        {
            this.conditionDefinitionProvider = conditionDefinitionProvider ?? throw new ArgumentNullException(nameof(conditionDefinitionProvider));
        }


        /// <summary>
        /// Serializes a visibility condition configuration to an XML string.
        /// </summary>
        /// <param name="visibilityConditionConfiguration">Visibility condition configuration to be serialized, or null.</param>
        /// <returns>Returns an XML representation of the condition, or an empty string when no condition is specified.</returns>
        public string Serialize(VisibilityConditionConfiguration visibilityConditionConfiguration)
        {
            if (visibilityConditionConfiguration == null)
            {
                return "";
            }

            var result = new StringBuilder();

            Serialize(visibilityConditionConfiguration, visibilityConditionConfiguration.VisibilityCondition.GetType(), result);

            return result.ToString();
        }


        /// <summary>
        /// Deserializes a visibility condition configuration from an XML string.
        /// </summary>
        /// <param name="visibilityConditionConfigurationXml">XML representation of the condition configuration to be deserialized, or null.</param>
        /// <returns>Returns a visibility condition configuration, or null when null or empty XML is specified.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="visibilityConditionConfigurationXml"/> is null or an empty string.</exception>
        public VisibilityConditionConfiguration Deserialize(string visibilityConditionConfigurationXml)
        {
            if (String.IsNullOrEmpty(visibilityConditionConfigurationXml))
            {
                return null;
            }

            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(visibilityConditionConfigurationXml);

                return Deserialize(document.DocumentElement);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Deserialization of visibility condition failed. See the inner exception for details. Visibility condition XML:{Environment.NewLine}{Environment.NewLine}{visibilityConditionConfigurationXml}", ex);
            }
        }


        /// <summary>
        /// Gets visibility condition type from a <paramref name="typeIdentifier"/>.
        /// </summary>
        protected override Type GetConfiguredObjectType(string typeIdentifier)
        {
            var conditionDefinition = conditionDefinitionProvider.Get(typeIdentifier);
            if (conditionDefinition == null)
            {
                throw new InvalidOperationException($"Visibility condition identifier '{typeIdentifier}' is not a known visiblity condition identifier.");
            }

            return conditionDefinition.VisibilityConditionType;
        }


        /// <summary>
        /// Creates a new <see cref="VisibilityConditionConfiguration"/> based on deserialzed <paramref name="typeIdentifier"/> and <paramref name="configuredObject"/>.
        /// </summary>
        /// <param name="typeIdentifier">Type identifier of the <paramref name="configuredObject"/>.</param>
        /// <param name="configuredObject">Deserialized configured object.</param>
        /// <returns>Returns deserialized configuration.</returns>
        protected override VisibilityConditionConfiguration CreateDeserializedConfiguration(string typeIdentifier, object configuredObject)
        {
            return new VisibilityConditionConfiguration(typeIdentifier, (VisibilityCondition)configuredObject);
        }
    }
}
