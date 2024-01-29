using System;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.PortalEngine;

namespace CMS.OnlineMarketing
{
    internal class WebPartsStructuredData : IStructuredData
    {
        private WebPartZoneInstance mZoneInstance;
        private WebPartInstance mWebPartInstance;


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public void LoadFromXmlElement(XmlElement element)
        {
            switch (element.Name.ToLowerCSafe())
            {
                case "webpartzone":
                    mZoneInstance = new WebPartZoneInstance(element);
                    break;
                case "webpart":
                    mWebPartInstance = new WebPartInstance(element);
                    break;
                default:
                    throw new NotSupportedException("Web parts definitions declared by " + element.Name + " root element are not supported.");
            }      
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        public XmlElement GetXmlElement(XmlDocument doc)
        {
            if ((mZoneInstance == null) && (mWebPartInstance == null))
            {
                return null;
            }

            var resultElement = (mZoneInstance != null) ? mZoneInstance.GetXmlNode(doc) : mWebPartInstance.GetXmlNode(doc);

            doc.AppendChild(resultElement);

            return resultElement;
        }
    }
}
