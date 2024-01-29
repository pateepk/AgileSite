using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// SearchIndexSettings handles management of index settings container.
    /// </summary>    
    public class SearchIndexSettings : IStructuredData
    {
        #region "Constants"

        /// <summary>
        /// Root element name of index settings data.
        /// </summary>
        private const string CONTAINERROOTNAME = "index";

        /// <summary>
        /// Element name which determines items.
        /// </summary>
        private const string ITEMELEMENTNAME = "item";

        /// <summary>
        /// Attribute name which identifies single item of index settings.
        /// </summary>
        private const string ITEMIDENTIFIER = "id";

        #endregion


        #region "Private variable"

        private readonly Dictionary<Guid, SearchIndexSettingsInfo> mItems = new Dictionary<Guid, SearchIndexSettingsInfo>();

        #endregion"


        #region "Properties"

        /// <summary>
        /// Gets list of all settings items.
        /// </summary>
        public Dictionary<Guid, SearchIndexSettingsInfo> Items
        {
            get
            {
                return mItems;
            }
        }

        #endregion


        #region "XML methods"

        /// <summary>
        /// Loads settings from xml data.
        /// </summary>
        /// <param name="data">String of xml data</param>
        public virtual void LoadData(string data)
        {
            // Create xml data object for loading
            XmlDocument xml = new XmlDocument();

            if (!String.IsNullOrEmpty(data))
            {
                xml.LoadXml(data);
            }

            LoadFromXmlElement(xml.DocumentElement);
        }


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public void LoadFromXmlElement(XmlElement element)
        {
            // Clear old items
            Items.Clear();

            if (element != null)
            {
                foreach (XmlNode item in element.ChildNodes)
                {
                    if ((item.NodeType == XmlNodeType.Element) && (item.Attributes != null))
                    {
                        XmlAttribute idAttr = item.Attributes[ITEMIDENTIFIER];
                        if (idAttr != null)
                        {
                            Guid itemGuid = ValidationHelper.GetGuid(idAttr.Value, Guid.Empty);

                            if (itemGuid != Guid.Empty)
                            {
                                var settings = new SearchIndexSettingsInfo();

                                // Set all attribute values from xml to info object
                                foreach (XmlAttribute attr in item.Attributes)
                                {
                                    settings.SetValue(attr.Name, attr.Value);
                                }

                                // Add to list
                                Items.Add(itemGuid, settings);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns xml code of search index settings.
        /// </summary>        
        public virtual string GetData()
        {
            // Prepare the XML document
            var xml = new XmlDocument();

            var element = GetXmlElement(xml);

            xml.AppendChild(element);

            return xml.ToFormattedXmlString(true);
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>        
        /// <param name="document">Parent XML document</param>
        public XmlElement GetXmlElement(XmlDocument document)
        {
            var root = document.CreateElement(CONTAINERROOTNAME);

            // Order by guids to maintain consistent order
            foreach (var item in Items.OrderBy(item => item.Key).Select(item => item.Value))
            {
                var structuredNode = document.CreateElement(ITEMELEMENTNAME);

                // Add column values as attributes
                var attributes = item.ColumnNames
                                     .ToDictionary(column => column, column => ValidationHelper.GetString(item.GetValue(column), null));

                structuredNode.AddAttributes(attributes, removeEmptyEntries: false);

                // Save the node
                root.AppendChild(structuredNode);
            }

            return root;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns search index setting item.
        /// </summary>
        /// <param name="guid">String id</param>
        /// <returns>Returns search setting item or null if there is no such item</returns>
        public virtual SearchIndexSettingsInfo GetSearchIndexSettingsInfo(string guid)
        {
            Guid itemGuid = ValidationHelper.GetGuid(guid, Guid.Empty);
            return GetSearchIndexSettingsInfo(itemGuid);
        }


        /// <summary>
        /// Returns search index setting item.
        /// </summary>
        /// <param name="guid">ID of search setting item</param>
        /// <returns>Returns search setting item or null if there is no such item</returns>
        public virtual SearchIndexSettingsInfo GetSearchIndexSettingsInfo(Guid guid)
        {
            if (Items.ContainsKey(guid))
            {
                return Items[guid];
            }
            return null;
        }


        /// <summary>
        /// Adds or updates item in search index settings.
        /// </summary>
        /// <param name="sisi">Search index object</param>
        public virtual void SetSearchIndexSettingsInfo(SearchIndexSettingsInfo sisi)
        {
            if (sisi != null)
            {
                Items[sisi.ID] = sisi;
            }
        }


        /// <summary>
        /// Deletes search index setting item.
        /// </summary>
        /// <param name="guid">ID of search setting item</param>        
        public virtual void DeleteSearchIndexSettingsInfo(Guid guid)
        {
            if (Items[guid] != null)
            {
                Items.Remove(guid);
            }
        }


        /// <summary>
        /// Deletes search index setting item.
        /// </summary>
        /// <param name="sisi">SearchIndexSettingsInfo</param>        
        public virtual void DeleteSearchIndexSettingsInfo(SearchIndexSettingsInfo sisi)
        {
            if (sisi != null)
            {
                DeleteSearchIndexSettingsInfo(sisi.ID);
            }
        }


        /// <summary>
        /// Returns dataset with all index settings items.
        /// </summary>
        public virtual DataSet GetAll()
        {
            if ((Items == null) || (Items.Count == 0))
            {
                return null;
            }

            DataTable dt = new DataTable();

            // Go trough all items
            foreach (SearchIndexSettingsInfo item in Items.Values)
            {
                // Get item attributes
                List<string> itemAttributes = item.ColumnNames;

                if (itemAttributes != null)
                {
                    DataRow dr = dt.NewRow();

                    // Go trough all item attributes
                    foreach (string atrib in itemAttributes)
                    {
                        // If attribute hasn't column, create it
                        if (!dt.Columns.Contains(atrib))
                        {
                            dt.Columns.Add(new DataColumn(atrib));
                        }


                        dr[atrib] = item.GetValue(atrib);
                    }

                    dt.Rows.Add(dr);
                }
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            return ds;
        }

        #endregion
    }
}