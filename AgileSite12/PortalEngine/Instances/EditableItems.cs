using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.PortalEngine
{
    using EditableItemsDictionary = MultiKeyDictionary<string>;


    /// <summary>
    /// Editable items container.
    /// </summary>
    public class EditableItems : IDataContainer, IStructuredData
    {
        #region "Variables"

        private EditableItemsDictionary mEditableRegions = new EditableItemsDictionary();
        private EditableItemsDictionary mEditableWebParts = new EditableItemsDictionary();
        private const string SEPARATOR = ";";

        #endregion


        #region "Properties"

        /// <summary>
        /// Editable regions contained within the document.
        /// </summary>
        public virtual EditableItemsDictionary EditableRegions
        {
            get
            {
                return mEditableRegions;
            }
        }


        /// <summary>
        /// Editable WebParts contained within the document.
        /// </summary>
        public virtual EditableItemsDictionary EditableWebParts
        {
            get
            {
                return mEditableWebParts;
            }
        }


        /// <summary>
        /// Returns the editable value.
        /// </summary>
        /// <param name="controlId">Control ID</param>
        public string this[string controlId]
        {
            get
            {
                if (controlId == null)
                {
                    return null;
                }

                // Get editable web part content
                controlId = controlId.ToLowerCSafe();
                string value = EditableWebParts[controlId] ?? EditableRegions[controlId];

                // If not found, get editable region content
                return value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the first key from the key list defined as string with ';' as separator.
        /// </summary>
        /// <param name="key">Key</param>
        public static string GetFirstKey(string key)
        {
            // Get the part before the first separator
            int separatorIndex = key.IndexOf(SEPARATOR, StringComparison.Ordinal);
            if (separatorIndex >= 0)
            {
                key = key.Substring(0, separatorIndex);
            }

            return key;
        }


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public void LoadFromXmlElement(XmlElement element)
        {
            // Load editable regions
            LoadEditableItems(EditableRegions, element, "region");

            // Load editable web parts
            LoadEditableItems(EditableWebParts, element, "webpart");
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        public XmlElement GetXmlElement(XmlDocument doc)
        {
            var docElem = doc.CreateElement("content");
            doc.AppendChild(docElem);

            // Save editable regions
            SetItemsXml(EditableRegions, doc, "region");

            // Save web parts
            SetItemsXml(EditableWebParts, doc, "webpart");

            return doc.DocumentElement;
        }


        /// <summary>
        /// Loads the content XML to the content table.
        /// </summary>
        /// <param name="contentXml">Content XML to load</param>
        public void LoadContentXml(string contentXml)
        {
            EditableRegions.Clear();
            EditableWebParts.Clear();

            // Do not load if empty
            if (string.IsNullOrEmpty(contentXml))
            {
                return;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(contentXml);

            LoadFromXmlElement(xml.DocumentElement);
        }


        /// <summary>
        /// Fills editable items dictionary from xml document for specific element
        /// </summary>
        /// <param name="items">Items collection</param>
        /// <param name="xmlElement">Source Xml element</param>
        /// <param name="elementName">Element name</param>
        private static void LoadEditableItems(EditableItemsDictionary items, XmlElement xmlElement, string elementName)
        {
            if (xmlElement == null)
            {
                return;
            }

            // Load editable web parts
            XmlNodeList parts = xmlElement.SelectNodes(elementName);
            if (parts == null)
            {
                return;
            }
            foreach (XmlNode part in parts)
            {
                if ((part == null) || (part.Attributes == null))
                {
                    continue;
                }

                string id = part.Attributes["id"].Value;
                string value = part.InnerText;

                // Unescape CDATA
                value = value.Replace("]]#>", "]]>");

                items[id.ToLowerCSafe()] = value;
            }
        }


        /// <summary>
        /// Returns the xml code of the document contents (Editable regions, web parts).
        /// </summary>
        public string GetContentXml()
        {
            if ((EditableWebParts.Count == 0) && (EditableRegions.Count == 0))
            {
                return "";
            }

            var xml = new XmlDocument();

            GetXmlElement(xml);

            // Return the xml
            return xml.InnerXml;
        }


        /// <summary>
        /// Sets the items to the xml document under the specific node defined by element name
        /// </summary>
        /// <param name="items">Editable items list</param>
        /// <param name="xml">XML document</param>
        /// <param name="elementName">Xml element name</param>
        private static void SetItemsXml(EditableItemsDictionary items, XmlDocument xml, string elementName)
        {
            // Save editable items sorted by ID stored in Key values
            foreach (var part in items.Cast<DictionaryEntry>().ToDictionary(d => d.Key, d => d.Value).OrderBy(s => s.Key))
            {
                string value = (string)part.Value;
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                // Create new web part node
                XmlNode newNode = xml.CreateElement(elementName);
                XmlAttribute attr = xml.CreateAttribute("id");
                attr.Value = (string)part.Key;

                // Escape CDATA
                value = value.Replace("]]>", "]]#>");

                // Save the content
                XmlCDataSection data = xml.CreateCDataSection(value);
                newNode.AppendChild(data);

                // Save the node
                newNode.Attributes.Append(attr);
                xml.DocumentElement.AppendChild(newNode);
            }
        }


        /// <summary>
        /// Returns the cloned object.
        /// </summary>
        public EditableItems Clone()
        {
            EditableItems newItems = new EditableItems();

            newItems.mEditableRegions = (EditableItemsDictionary)EditableRegions.Clone();
            newItems.mEditableWebParts = (EditableItemsDictionary)EditableWebParts.Clone();

            return newItems;
        }

        #endregion


        #region ISimpleDataContainer Members

        /// <summary>
        /// Gets or sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object ISimpleDataContainer.this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Gets the value of specific field.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            return this[columnName];
        }


        /// <summary>
        /// Sets the value of specific field.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public bool SetValue(string columnName, object value)
        {
            EditableWebParts[columnName.ToLowerCSafe()] = (string)value;

            return true;
        }


        /// <summary>
        /// Returns the available column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                List<string> names = new List<string>();

                // Add editable web parts items
                foreach (string item in mEditableWebParts.Keys)
                {
                    string name = GetFirstKey(item);
                    names.Add(name);
                }

                // Add editable control items
                foreach (string item in mEditableRegions.Keys)
                {
                    string name = GetFirstKey(item);
                    names.Add(name);
                }

                names.Sort();

                return names;
            }
        }


        /// <summary>
        /// Tries to get the value of specific column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns value</param>
        public bool TryGetValue(string columnName, out object value)
        {
            value = GetValue(columnName);

            return (value != null);
        }


        /// <summary>
        /// Returns true if the structure contains specific column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return (GetValue(columnName) != null);
        }

        #endregion
    }
}