using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System;

using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// Abstract class for the UniGrid configuration.
    /// </summary>
    public abstract class AbstractConfiguration : DataBoundControl
    {
        /// <summary>
        /// Returns attribute value of the specific option (specific key node).
        /// </summary>
        /// <param name="optionNode">Option node containing key nodes</param>
        /// <param name="keyName">Attribute name of the specific key node</param>
        /// <returns>Attribute value of the specific key node</returns>
        protected static string GetKeyValue(XmlNode optionNode, string keyName)
        {
            // Get the specific key
            XmlNode keyNode = optionNode.SelectSingleNode("key[@name='" + keyName + "']");
            if (keyNode != null)
            {
                if (keyNode.Attributes != null)
                {
                    return keyNode.Attributes["value"].Value;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns attribute value of the specific option (specific key node) converted to boolean.
        /// </summary>
        /// <param name="optionNode">Option node containing key nodes</param>
        /// <param name="keyName">Attribute name of the specific key node</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Attribute value of the specific key node</returns>
        protected static bool? GetBoolKeyValue(XmlNode optionNode, string keyName, bool? defaultValue)
        {
            string value = GetKeyValue(optionNode, keyName);
            if (value == null)
            {
                return defaultValue;
            }

            return ValidationHelper.GetBoolean(value, false);
        }


        /// <summary>
        /// Gets the option key value of <typeparamref name="T"/> type from <paramref name="element"/> source data given by <paramref name="keyName"/>.
        /// If no option key is found or source data are not provided returns <paramref name="defaultValue"/> default value. Ignores case in key name and defined namespace.
        /// </summary>
        /// <remarks>This method doesn't work with nullable types such as bool? or int?</remarks>
        /// <typeparam name="T">Type of return value</typeparam>
        /// <param name="element">Source XML data element</param>
        /// <param name="keyName">Option key name</param>
        /// <param name="defaultValue">Default value</param>
        internal T GetOptionKeyValue<T>(XElement element, string keyName, T defaultValue)
        {
            var optionElement = element.Elements()
                   .FirstOrDefault(e => e.Attributes()
                                        .Any(att => att.Name.LocalName.Equals("name", StringComparison.InvariantCultureIgnoreCase) && att.Value.Equals(keyName, StringComparison.InvariantCultureIgnoreCase)));


            if (optionElement == null)
            {
                return defaultValue;
            }

            return optionElement.GetAttributeValue("value", defaultValue);
        }


        /// <summary>
        /// Gets the option key boolean value from <paramref name="element"/> source data given by <paramref name="keyName"/>.
        /// If no option key is found or source data are not provided returns <paramref name="defaultValue"/> default value. Ignores case in key name.
        /// </summary>
        /// <param name="element">Source XML data element</param>
        /// <param name="keyName">Option key name</param>
        /// <param name="defaultValue">Default value</param>
        internal bool? GetOptionKeyBoolValue(XElement element, string keyName, bool? defaultValue)
        {
            var value = GetOptionKeyValue<string>(element, keyName, null);

            return ValidationHelper.GetNullableBoolean(value, defaultValue);
        }


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
        /// Gets the value for particular attribute of the XML node converted to integer.
        /// </summary>
        /// <param name="node">Xml node</param>
        /// <param name="name">Attribute name</param>
        /// <param name="defaultValue">Default value</param>
        protected static int GetIntAttribute(XmlNode node, string name, int defaultValue)
        {
            return ValidationHelper.GetValue(GetAttribute(node, name), defaultValue);
        }


        /// <summary>
        /// Gets the value for particular attribute of the XML node converted to boolean.
        /// </summary>
        /// <param name="node">Xml node</param>
        /// <param name="name">Attribute name</param>
        /// <param name="defaultValue">Default value</param>
        protected static bool GetBoolAttribute(XmlNode node, string name, bool defaultValue)
        {
            return ValidationHelper.GetValue(GetAttribute(node, name), defaultValue);
        }
    }
}