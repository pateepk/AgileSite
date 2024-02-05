using System;
using System.Linq;
using System.Text;
using System.Xml;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for the structured objects
    /// </summary>
    public interface IStructuredData
    {
        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        void LoadFromXmlElement(XmlElement element);


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        XmlElement GetXmlElement(XmlDocument doc);
    }
}
