using System;
using System.Linq;
using System.Xml.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides serialization of mapping of CMS objects to SalesForce entities.
    /// </summary>
    public sealed class MappingSerializer
    {

        /// <summary>
        /// Serializes the specified mapping into a string representation, and returns it.
        /// </summary>
        /// <param name="mapping">The mapping to serialize.</param>
        /// <returns>A string representation of the specified mapping.</returns>
        public string SerializeMapping(Mapping mapping)
        {
            XElement element = new XElement("mapping", new XAttribute("externalName", mapping.ExternalIdentifierAttributeName), new XAttribute("externalLabel", mapping.ExternalIdentifierAttributeLabel), mapping.Items.Select(x => new XElement("item", new XAttribute("attributeName", x.AttributeName), new XAttribute("attributeLabel", x.AttributeLabel), new XAttribute("sourceName", x.SourceName), new XAttribute("sourceLabel", x.SourceLabel), new XAttribute("sourceType", Enum.GetName(typeof(MappingItemSourceTypeEnum), x.SourceType)))));

            return element.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Deserializes a mapping from the specified string representation, and returns it.
        /// </summary>
        /// <param name="content">A string representation of the mapping.</param>
        /// <returns>A mapping corresponding to the specified string representation.</returns>
        public Mapping DeserializeMapping(string content)
        {
            XElement element = XElement.Parse(content, LoadOptions.None);

            return new Mapping(element.Attribute("externalName").Value, element.Attribute("externalLabel").Value, element.Elements().Select(x => new MappingItem(x.Attribute("attributeName").Value, x.Attribute("attributeLabel").Value, x.Attribute("sourceName").Value, x.Attribute("sourceLabel").Value, (MappingItemSourceTypeEnum)Enum.Parse(typeof(MappingItemSourceTypeEnum), x.Attribute("sourceType").Value))));
        }

    }

}