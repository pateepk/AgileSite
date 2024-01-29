using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace CMS.Helpers
{
    /// <summary>
    /// Extension methods for <see cref="System.Xml" /> classes.
    /// </summary>
    public static class XmlExtensions
    {
        #region "Variables"

        private static Regex mCDataEndTagRegExp;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression to match CDATA end tag.
        /// </summary>
        private static Regex CDataEndTagRegExp
        {
            get
            {
                return mCDataEndTagRegExp ?? (mCDataEndTagRegExp = RegexHelper.GetRegex(Regex.Escape("]]>"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
            }
        }

        #endregion


        #region "XmlDocument Extensions"

        /// <summary>
        /// Returns formatted XML content.
        /// </summary>
        /// <remarks>
        /// Ensures indentation and line breaking after each element.
        /// </remarks>
        /// <param name="document">XML document</param>
        /// <param name="omitXmlDeclaration">If true, the XML declaration is not included into the output</param>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is null.</exception>
        public static string ToFormattedXmlString(this XmlDocument document, bool omitXmlDeclaration = false)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var str = new StringBuilder();

            // Prepare settings
            var settings = FormattedXmlWriter.GetDefaultSettings();

            settings.OmitXmlDeclaration = omitXmlDeclaration;

            // Write the content
            using (var writer = new FormattedXmlWriter(str, settings))
            {
                document.WriteContentTo(writer);
            }

            return str.ToString();
        }


        /// <summary>
        /// Writes formatted XML content to given stream.
        /// </summary>
        /// <remarks>
        /// Ensures indentation and line breaking after each element.
        /// </remarks>
        /// <param name="document">XML document</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="omitXmlDeclaration">If true, the XML declaration is not included into the output.</param>
        /// <param name="encoding">Encoding to be used when writing the content to stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is null.</exception>
        public static void WriteFormattedXmlToStream(this XmlDocument document, System.IO.Stream stream, bool omitXmlDeclaration = false, Encoding encoding = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            // Prepare settings
            var settings = FormattedXmlWriter.GetDefaultSettings();

            if (encoding != null)
            {
                settings.Encoding = encoding;
            }

            settings.OmitXmlDeclaration = omitXmlDeclaration;

            // Write the content
            using (var writer = new FormattedXmlWriter(stream, settings))
            {
                document.WriteContentTo(writer);
            }
        }

        #endregion


        #region "XmlElement Extensions"

        /// <summary>
        /// Adds the given list of attributes to the XML node. Adds the attributes in alphabetic order to maintain stability of order.
        /// </summary>
        /// <param name="node">XML node to add attributes to.</param>
        /// <param name="attributes">Attributes to be added.</param>
        /// <param name="removeEmptyEntries">If true, only attributes with non-empty value are added to the element.</param>
        /// <exception cref="ArgumentException">Some key of <paramref name="attributes"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Keys of <paramref name="attributes"/> cannot be ordered.</exception>
        /// <exception cref="XmlException">Some key of <paramref name="attributes"/> contains characters not suitable for an XML attribute name.</exception>
        public static void AddAttributes(this XmlElement node, IDictionary attributes, bool removeEmptyEntries = true)
        {
            if (attributes != null)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                // Order attributes alphabetically to maintain stable order
                var sortedAttributes = new SortedList(attributes, StringComparer.InvariantCulture);

                foreach (DictionaryEntry attribute in sortedAttributes)
                {
                    string key = Convert.ToString(attribute.Key, CultureHelper.EnglishCulture);
                    string value = Convert.ToString(attribute.Value, CultureHelper.EnglishCulture);

                    if (!removeEmptyEntries || !String.IsNullOrEmpty(value))
                    {
                        node.SetAttribute(key, value);
                    }
                }
            }
        }


        /// <summary>
        /// Adds the collection of values into the XML element, each value as a nested element. Ensures the alphabetic order of the values in the element.
        /// </summary>
        /// <param name="node">Parent XML element.</param>
        /// <param name="values">Collection of element values.</param>
        /// <param name="elementName">If set, the elements are added with the given name and the key is stored in attribute name. Key is used for the element name otherwise.</param>
        /// <param name="transform">Function allowing to modify the added element that has three parameters - XML element to modify, string key and value from the collection <paramref name="values"/>.</param>
        /// <param name="removeEmptyEntries">If true, only elements with non-empty value are added to the parent element <paramref name="node"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="elementName"/> was not supplied and some keys of <paramref name="values"/> are an empty string.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Keys of <paramref name="values"/> cannot be ordered or <paramref name="node"/> is missing owner document.</exception>
        /// <exception cref="XmlException"><paramref name="elementName"/> was not supplied and some keys of <paramref name="values"/> contain characters not suitable for an XML element name.</exception>
        public static void AddChildElements(this XmlElement node, IDictionary values, string elementName = null, Action<XmlElement, string, object> transform = null, bool removeEmptyEntries = true)
        {
            if (values != null)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                var doc = node.OwnerDocument;
                if (doc == null)
                {
                    throw new InvalidOperationException("XmlElement is missing OwnerDocument.");
                }

                var sortedValues = new SortedList(values, StringComparer.InvariantCulture);

                // Order values alphabetically to maintain stable order
                foreach (DictionaryEntry property in sortedValues)
                {
                    string key = Convert.ToString(property.Key, CultureHelper.EnglishCulture);
                    var propNode = doc.CreateElement(String.IsNullOrEmpty(elementName) ? key : elementName);

                    // Name attribute if necessary
                    if (!String.IsNullOrEmpty(elementName))
                    {
                        propNode.SetAttribute("name", key);
                    }

                    propNode.InnerText = Convert.ToString(property.Value, CultureHelper.EnglishCulture);

                    transform?.Invoke(propNode, key, property.Value);

                    if (!removeEmptyEntries || !String.IsNullOrEmpty(propNode.InnerText))
                    {
                        node.AppendChild(propNode);
                    }
                }
            }
        }


        /// <summary>
        /// Appends CDATA sections to the XML element based on the input text. If the text already contains CDATA tags, the text is split into corresponding number of CDATA sections in place of the CDATA end tags.
        /// </summary>
        /// <param name="node">Parent XML node</param>
        /// <param name="text">Input text</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public static void AppendCData(this XmlElement node, string text)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var doc = node.OwnerDocument;
            if (doc == null)
            {
                throw new InvalidOperationException("XmlElement is missing OwnerDocument.");
            }

            // Split input text in place of CDATA end tags and create separate CDATA sections
            string[] parts = CDataEndTagRegExp.Split(text);
            
            // Clean all child nodes
            node.InnerXml = "";

            foreach (var part in parts)
            {
                node.AppendChild(doc.CreateCDataSection(part));
            }
        }

        #endregion
    }
}
