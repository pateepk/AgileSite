using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using CMS.IO;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Base class for <see cref="ValidationRuleConfigurationsXmlSerializer"/> and <see cref="VisibilityConditionConfigurationXmlSerializer"/>.
    /// </summary>
    public abstract class ConfigurationsXmlSerializer<TConfiguration>
    {
        /// <summary>
        /// Gets the name of the element containing the serialized <typeparamref name="TConfiguration"/>.
        /// </summary>
        protected abstract string ConfigurationElementName { get; }


        /// <summary>
        /// Gets the name of the element containing type identifier of the serialized configured object wrapped by <typeparamref name="TConfiguration"/>.
        /// </summary>
        protected abstract string ConfiguredObjectIdentifierElementName { get; }


        /// <summary>
        /// Gets the name of the element containing the serialized configured object wrapped by <typeparamref name="TConfiguration"/>.
        /// </summary>
        protected abstract string ConfiguredObjectElementName { get; }


        /// <summary>
        /// Serializes <paramref name="configuration"/> and appends it to <paramref name="result"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when serialization of <paramref name="configuration"/> fails.</exception>
        protected virtual void Serialize(TConfiguration configuration, Type configuredType, StringBuilder result)
        {
            try
            {
                var xmlSerializer = CreateXmlSerializer(configuredType);

                if (result.Length != 0)
                {
                    result.AppendLine();
                }

                using (var xmlWriter = XmlWriter.Create(result, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
                {
                    xmlSerializer.Serialize(xmlWriter, configuration, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Serialization of configuration of type '{configuredType}' failed. See the inner exception for details.", ex);
            }
        }


        /// <summary>
        /// Creates an <see cref="XmlSerializer"/> suitable for serializing an instance of <typeparamref name="TConfiguration"/> encapsulating object of <paramref name="encapsulatedType"/>.
        /// </summary>
        private XmlSerializer CreateXmlSerializer(Type encapsulatedType)
        {
            var xmlAttributes = new XmlAttributes();
            xmlAttributes.XmlElements.Add(new XmlElementAttribute(encapsulatedType));

            var serializedType = typeof(TConfiguration);

            var attributeOverrides = new XmlAttributeOverrides();
            attributeOverrides.Add(serializedType, ConfiguredObjectElementName, xmlAttributes);

            return new XmlSerializer(serializedType, attributeOverrides, new[] { encapsulatedType }, new XmlRootAttribute(ConfigurationElementName), null);
        }


        /// <summary>
        /// Deserializes a configuration from an XML element.
        /// </summary>
        /// <param name="configurationXmlNode">XML representation of <typeparamref name="TConfiguration"/>.</param>
        /// <returns>Returns a <typeparamref name="TConfiguration"/> configuration object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization of <paramref name="configurationXmlNode"/> fails.</exception>
        protected virtual TConfiguration Deserialize(XmlElement configurationXmlNode)
        {
            var typeIdentifier = GetConfiguredObjectTypeIdentifier(configurationXmlNode);
            var visibilityConditionType = GetConfiguredObjectType(typeIdentifier);
            var xmlSerializer = CreateXmlDeserializer(visibilityConditionType);

            var visibilityConditionNode = configurationXmlNode.GetElementsByTagName(ConfiguredObjectElementName).Item(0);
            using (StringReader stringReader = new StringReader(visibilityConditionNode.OuterXml))
            {
                return CreateDeserializedConfiguration(typeIdentifier, xmlSerializer.Deserialize(stringReader));
            }
        }


        /// <summary>
        /// Gets type identifier from an XML element representing the <typeparamref name="TConfiguration"/>.
        /// </summary>
        private string GetConfiguredObjectTypeIdentifier(XmlElement configurationXmlNode)
        {
            var identifierNode = configurationXmlNode.GetElementsByTagName(ConfiguredObjectIdentifierElementName).Item(0);

            return identifierNode?.InnerText ?? throw new InvalidOperationException($"Type identifier is missing. Configuration cannot be deserialized. Configuration XML:{Environment.NewLine}{Environment.NewLine}{configurationXmlNode.OuterXml}");
        }


        /// <summary>
        /// Gets configured object type from a <paramref name="typeIdentifier"/>.
        /// </summary>
        protected abstract Type GetConfiguredObjectType(string typeIdentifier);


        /// <summary>
        /// Creates an <see cref="XmlSerializer"/> suitable for deserializing XML containing a configured object.
        /// </summary>
        private XmlSerializer CreateXmlDeserializer(Type configuredObjectType)
        {
            return new XmlSerializer(configuredObjectType, new XmlRootAttribute(ConfiguredObjectElementName));
        }


        /// <summary>
        /// Creates a new <typeparamref name="TConfiguration"/> based on deserialized <paramref name="typeIdentifier"/> and <paramref name="configuredObject"/>.
        /// </summary>
        /// <param name="typeIdentifier">Type identifier of the <paramref name="configuredObject"/>.</param>
        /// <param name="configuredObject">Deserialized configured object.</param>
        /// <returns>Returns deserialized configuration.</returns>
        protected abstract TConfiguration CreateDeserializedConfiguration(string typeIdentifier, object configuredObject);
    }
}
