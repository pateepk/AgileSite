using System.Xml;

using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// Base class for the UniGrid actions.
    /// </summary>
    public abstract class AbstractAction
    {
        /// <summary>
        /// Gets the value for particular attribute of the XML node.
        /// </summary>
        /// <param name="node">Xml node</param>
        /// <param name="name">Attribute name</param>
        protected static string GetAttribute(XmlNode node, string name)
        {
            return XmlHelper.GetAttributeValue(node, name, null);
        }


        /// <summary>
        /// Gets the value for particular attribute of the XML node.
        /// </summary>
        /// <param name="node">Xml node</param>
        /// <param name="name">Attribute name</param>
        /// <param name="defaultValue">Default value</param>
        protected static string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            return XmlHelper.GetAttributeValue(node, name, defaultValue);
        }
    }
}