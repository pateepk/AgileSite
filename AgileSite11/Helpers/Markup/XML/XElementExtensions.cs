using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CMS.Helpers
{
    /// <summary>
    /// Extension methods for <see cref="XElement"/> type to be able query XML data with ignoring case.
    /// </summary>
    public static class XElementExtensions
    {
        /// <summary>
        /// Gets the child element of <paramref name="element"/> element given by <paramref name="elementName"/>. 
        /// Ignores case in element name and defined namespace. 
        /// </summary>
        /// <param name="element">Source XML element</param>
        /// <param name="elementName">Element name</param>
        public static XElement GetElement(this XElement element, string elementName)
        {
            return element.GetElements(elementName).FirstOrDefault();
        }


        /// <summary>
        /// Gets the child elements from <paramref name="element"/> source XML data given by <paramref name="elementName"/>. 
        /// Ignores case in element name and defined namespace.
        /// </summary>
        /// <param name="element">Source XML element</param>
        /// <param name="elementName">Element name</param>
        public static IEnumerable<XElement> GetElements(this XElement element, string elementName)
        {
            return element.Elements().Where(t => t.Name.LocalName.Equals(elementName, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Gets the attribute of <paramref name="element"/> element given by <paramref name="attributeName"/>. 
        /// Ignores case in attribute name and defined namespace.
        /// </summary>
        /// <param name="element">Source XML element</param>
        /// <param name="attributeName">Attribute name</param>
        public static XAttribute GetAttribute(this XElement element, string attributeName)
        {
            return element.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Gets the attribute value of <typeparamref name="T"/> type of <paramref name="element"/> element given by <paramref name="attributeName"/>.
        /// If no attribute is found or source data are not provided returns <paramref name="defaultValue"/> default value. 
        /// Ignores case in attribute name and defined namespace.
        /// </summary>
        /// <remarks>This method doesn't work with nullable types such as bool? or int?</remarks>
        /// <typeparam name="T">Type of return value</typeparam>
        /// <param name="element">Source XML element</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="defaultValue">Default value</param>
        public static T GetAttributeValue<T>(this XElement element, string attributeName, T defaultValue)
        {
            var attribute = element.GetAttribute(attributeName);
            if (attribute == null)
            {
                return defaultValue;
            }

            return ValidationHelper.GetValue(attribute.Value, defaultValue);
        }


        /// <summary>
        /// Gets the attribute value as string type from <paramref name="element"/> source data given by <paramref name="attributeName"/>.
        /// If no attribute is found or source data are not provided returns <paramref name="defaultValue"/> default value. 
        /// Ignores case in attribute name and defined namespace.
        /// </summary>
        /// <param name="element">Source XML element</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetAttributeStringValue(this XElement element, string attributeName, string defaultValue = null)
        {
            return GetAttributeValue(element, attributeName, defaultValue);
        }
    }
}
