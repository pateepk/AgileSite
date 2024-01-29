using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class represents excluded code names for an object type.
    /// </summary>
    [Serializable]
    public class ExcludedObjectTypeCodeNames : IXmlSerializable
    {
        /// <summary>
        /// Object type for which the excluded code names are specified.
        /// </summary>
        public string ObjectType;


        /// <summary>
        /// Collection of code names that will be excluded.
        /// </summary>
        public List<string> CodeNames;


        /// <summary>
        /// Implementation of IXmlSerializable.GetSchema().
        /// </summary>
        public XmlSchema GetSchema()
        {
            return null;
        }


        /// <summary>
        /// Deserialize object from XML.
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            ObjectType = reader.GetAttribute(RepositoryConfigurationFile.OBJECT_TYPE_ELEMENT_NAME);
            CodeNames = reader.ReadElementString(RepositoryConfigurationFile.OBJECT_EXCLUDED_CODE_NAMES_ELEMENT_NAME)
                                    .Split(';')
                                    .Select(x => x.Trim())
                                    .Where(x => !String.IsNullOrEmpty(x))
                                    .ToList();
        }


        /// <summary>
        /// Serialize object to XML.
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(RepositoryConfigurationFile.OBJECT_TYPE_ELEMENT_NAME, ObjectType);

            if (CodeNames != null)
            {
                writer.WriteString(String.Join(";", CodeNames));
            }
        }
    }
}
