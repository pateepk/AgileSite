using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// SearchSettings class provides methods for manipulation with SearchSettingsInfo objects.
    /// </summary>
    [Serializable]
    public class SearchSettings : IEnumerable<SearchSettingsInfo>, IStructuredData
    {
        #region "Constants"

        /// <summary>
        /// Root element name of search settings data.
        /// </summary>
        private const string CONTAINERROOTNAME = "search";


        /// <summary>
        /// Element name which determines items.
        /// </summary>
        private const string ITEMELEMENTNAME = "item";


        /// <summary>
        /// Attribute name which identifies single item of index settings.
        /// </summary>
        private const string ITEMIDENTIFIER = "id";


        /// <summary>
        /// Name of flag indicating whether field is searchable in context of local indexes.
        /// </summary>
        /// <remarks>
        /// In local indexes a searchable field has a dedicated index field with its original value stored so that the value can be retrieved upon search.
        /// </remarks>
        public const string SEARCHABLE = "Searchable";


        /// <summary>
        /// Name of flag indicating whether field is tokenized in context of local indexes.
        /// </summary>
        public const string TOKENIZED = "Tokenized";


        /// <summary>
        /// Field field.
        /// </summary>
        public const string IFIELDNAME = "iFieldname";


        /// <summary>
        /// Name of flag indicating whether field is considered a content field in context of local indexes.
        /// </summary>
        public const string CONTENT = "Content";

        #endregion


        #region "Private variable"

        private readonly SafeDictionary<Guid, SearchSettingsInfo> mItems = new SafeDictionary<Guid, SearchSettingsInfo>();

        private readonly StringSafeDictionary<SearchSettingsInfo> mItemsByName = new StringSafeDictionary<SearchSettingsInfo>();

        #endregion"


        #region "Properties"

        /// <summary>
        /// Gets list of all settings items indexed by GUID.
        /// </summary>
        public SafeDictionary<Guid, SearchSettingsInfo> Items
        {
            get
            {
                return mItems;
            }
        }


        /// <summary>
        /// Gets list of all settings items indexed by name.
        /// </summary>
        internal StringSafeDictionary<SearchSettingsInfo> ItemsByName
        {
            get
            {
                return mItemsByName;
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
        /// Returns xml code of search index settings.
        /// </summary>        
        public virtual string GetData()
        {
            // Prepare the XML document
            var xml = new XmlDocument();

            var element = GetXmlElement(xml);

            xml.AppendChild(element);

            return xml.InnerXml;
        }


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public void LoadFromXmlElement(XmlElement element)
        {
            // Clear old items
            Items.Clear();

            if (element == null)
            {
                return;
            }

            foreach (XmlNode item in element.ChildNodes)
            {
                if ((item.NodeType != XmlNodeType.Element) || (item.Attributes == null))
                {
                    continue;
                }

                XmlAttribute idAttr = item.Attributes[ITEMIDENTIFIER];
                if (idAttr == null)
                {
                    continue;
                }

                Guid itemGuid = ValidationHelper.GetGuid(idAttr.Value, Guid.Empty);
                if (itemGuid == Guid.Empty)
                {
                    continue;
                }

                var settings = new SearchSettingsInfo();

                // Set all attribute values from xml to info object
                foreach (XmlAttribute attr in item.Attributes)
                {
                    settings.SetValue(attr.Name, attr.Value);
                }

                // Add to list
                Items[itemGuid] = settings;
                ItemsByName[settings.Name] = settings;
            }
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>        
        /// <param name="document">Parent XML document</param>
        public XmlElement GetXmlElement(XmlDocument document)
        {
            var docElem = document.CreateElement(CONTAINERROOTNAME);

            // Add the ordered items to the xml
            foreach (SearchSettingsInfo item in this.OrderBy(item => item.ID))
            {
                var structuredNode = document.CreateElement(ITEMELEMENTNAME);

                var attributes = item.ColumnNames
                                     .ToDictionary(column => column, column => ValidationHelper.GetString(item.GetValue(column), null));

                structuredNode.AddAttributes(attributes);

                docElem.AppendChild(structuredNode);
            }

            return docElem;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Adds or overwrites search setting item in hashtable.
        /// </summary>
        /// <param name="ssi">Search setting item</param>
        public virtual void SetSettingsInfo(SearchSettingsInfo ssi)
        {
            if (ssi != null)
            {
                Items[ssi.ID] = ssi;
            }
        }


        /// <summary>
        /// Returns search setting item by specified II (GUID).
        /// </summary>
        /// <param name="guid">String idenfificator</param>
        /// <returns>Returns search setting item or null if there is no such item</returns>
        public virtual SearchSettingsInfo GetSettingsInfo(string guid)
        {
            Guid itemGuid = ValidationHelper.GetGuid(guid, Guid.Empty);
            return GetSettingsInfo(itemGuid);
        }


        /// <summary>
        /// Returns search setting item by specified II (GUID).
        /// </summary>
        /// <param name="guid">ID of search setting item</param>
        /// <returns>Returns search setting item or null if no item found</returns>
        public virtual SearchSettingsInfo GetSettingsInfo(Guid guid)
        {
            if (Items.ContainsKey(guid))
            {
                return Items[guid];
            }
            return null;
        }


        /// <summary>
        /// Returns all search settings as DataSet.
        /// </summary>
        public virtual DataSet GetAllSettingsInfos()
        {
            if ((Items == null) || (Items.Count == 0))
            {
                return null;
            }

            DataTable dt = new DataTable();

            // Go trough all items
            foreach (SearchSettingsInfo item in Items.Values)
            {
                // Get item attributes
                var itemAttributes = item.ColumnNames;

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


        /// <summary>
        /// Deletes search setting item.
        /// </summary>
        /// <param name="guid">ID of search setting item</param>        
        public virtual void DeleteSearchSettingsInfo(Guid guid)
        {
            if (Items[guid] != null)
            {
                Items.Remove(guid);
            }
        }


        /// <summary>
        /// Deletes search index setting item.
        /// </summary>
        /// <param name="ssi">SearchIndexSettingsInfo</param>        
        public virtual void DeleteSearchSettingsInfo(SearchSettingsInfo ssi)
        {
            if (ssi != null)
            {
                DeleteSearchSettingsInfo(ssi.ID);
            }
        }


        /// <summary>
        /// Returns true if the search settings indexes any of the given columns
        /// </summary>
        /// <param name="columns">Columns to check</param>
        public virtual bool SearchesAnyOf(IEnumerable<string> columns)
        {
            if (columns == null)
            {
                return false;
            }

            foreach (var column in columns)
            {
                // Try specific columns
                var item = ItemsByName[column];
                if ((item != null) && (SearchFieldsHelper.Instance.IsIndexField(null, item) || SearchFieldsHelper.Instance.IsContentField(null, item)))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Copies selected fields to another search settings object
        /// </summary>
        /// <param name="target">Target settings</param>
        /// <param name="func">Function to say which items to copy. If null, all items are copied</param>
        public void CopyTo(SearchSettings target, Func<SearchSettingsInfo, bool> func = null)
        {
            foreach (DictionaryEntry item in Items)
            {
                var info = item.Value as SearchSettingsInfo;
                if ((info != null) && ((func == null) || func(info)))
                {
                    target.Items[(Guid)item.Key] = info;
                    target.ItemsByName[info.Name] = info;
                }
            }
        }


        /// <summary>
        /// Renames existing column in settings to the new name
        /// </summary>
        /// <param name="oldColumnName">Column to rename</param>
        /// <param name="newColumnName">New column name</param>
        public void RenameColumn(string oldColumnName, string newColumnName)
        {
            var settingsInfo = Items.TypedValues
                                .FirstOrDefault(info => (info != null) && info.Name.Equals(oldColumnName, StringComparison.InvariantCultureIgnoreCase));

            if (settingsInfo != null)
            {
                settingsInfo.Name = newColumnName;
                ItemsByName.Remove(oldColumnName);
                ItemsByName[newColumnName] = settingsInfo;
            }
        }

        #endregion


        #region "IEnumerable members"

        /// <summary>
        /// Returns the enumerator
        /// </summary>
        public IEnumerator<SearchSettingsInfo> GetEnumerator()
        {
            if (Items == null)
            {
                yield break;
            }

            foreach (var item in Items.TypedValues)
            {
                yield return item;
            }
        }


        /// <summary>
        /// Returns the enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}