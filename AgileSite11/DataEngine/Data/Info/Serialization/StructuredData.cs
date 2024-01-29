using System;
using System.Linq;
using System.Text;
using System.Xml;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Basic structured object implementation for valid XMLs.
    /// </summary>
    public class StructuredData : IStructuredData
    {
        private XmlElement xmlData;


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public void LoadFromXmlElement(XmlElement element)
        {
            xmlData = element;
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        public XmlElement GetXmlElement(XmlDocument doc)
        {
            return (XmlElement)doc.ImportNode(xmlData, true);
        }
    }
}
