using System.Linq;
using System.Xml.Linq;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Provides serialization and deserialization between an entity mapping and its string representation.
    /// </summary>
    public sealed class EntityMappingSerializer
    {
        #region "Public methods"

        /// <summary>
        /// Serializes the specified entity mapping into a string.
        /// </summary>
        /// <param name="mapping">The entity mapping to serialize.</param>
        /// <returns>The string that contains the specified entity mapping.</returns>
        public string SerializeEntityMapping(EntityMapping mapping)
        {
            XElement element = new XElement("mapping", mapping.Items.Select(x => new XElement("item", new XAttribute("attribute", x.AttributeName), new XAttribute("field", x.FieldName))));

            return element.ToString(SaveOptions.DisableFormatting);
        }


        /// <summary>
        /// Parses the specified string and returns the deserialized entity mapping.
        /// </summary>
        /// <param name="content">The string that contains an entity mapping.</param>
        /// <returns>The deserialized entity mapping.</returns>
        public EntityMapping UnserializeEntityMapping(string content)
        {
            XElement element = XElement.Parse(content, LoadOptions.None);

            return new EntityMapping(element.Elements().Select(x => new EntityMappingItem(x.Attribute("attribute").Value, x.Attribute("field").Value)));
        }

        #endregion
    }

}