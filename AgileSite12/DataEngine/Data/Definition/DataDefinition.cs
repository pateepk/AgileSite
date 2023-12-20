using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data definition base class
    /// </summary>
    public class DataDefinition : IStructuredData
    {
        #region "Variables"

        /// <summary>
        /// XML document representing FormInfo.
        /// </summary>
        private XmlDocument xmlDoc;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns list of all FormItems.
        /// </summary>
        public List<IDataDefinitionItem> ItemsList
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor, creates empty not initialized data definition
        /// </summary>
        public DataDefinition()
        {
            ItemsList = new List<IDataDefinitionItem>();
        }


        /// <summary>
        /// Constructor, creates the form info structure and loads specified form definition.
        /// </summary>
        /// <param name="dataDefinition">XML definition of the data</param>
        public DataDefinition(string dataDefinition)
            : this()
        {
            LoadFromDefinition(dataDefinition);
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        public virtual XmlElement GetXmlElement(XmlDocument doc)
        {
            throw new NotSupportedException("This operation is not supported, it must be implemented by the inheriting class.");
        }


        /// <summary>
        /// Loads the data definition from XML file
        /// </summary>
        /// <param name="dataDefinition">Data definition</param>
        public void LoadFromDefinition(string dataDefinition)
        {
            // Initialize xml if empty
            if (!String.IsNullOrEmpty(dataDefinition))
            {
                xmlDoc = new XmlDocument();

                try
                {
                    xmlDoc.LoadXml(dataDefinition);
                }
                catch (XmlException ex)
                {
                    CoreServices.EventLog.LogException("DataDefinition", "LOADXML", ex);

                    throw;
                }

                LoadFromXmlElement(xmlDoc.DocumentElement);
            }
        }


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public virtual void LoadFromXmlElement(XmlElement element)
        {
            if (element != null)
            {
                foreach (XmlNode node in element.ChildNodes)
                {
                    LoadItem(node);
                }
            }
        }


        /// <summary>
        /// Loads the item from the XML node
        /// </summary>
        /// <param name="node">XML node</param>
        protected virtual IDataDefinitionItem LoadItem(XmlNode node)
        {
            // Form Field Info
            if (node.Name.Equals("field", StringComparison.InvariantCultureIgnoreCase))
            {
                return LoadField(node);
            }

            return null;
        }


        /// <summary>
        /// Loads the data definition from structure of a database table
        /// </summary>
        /// <param name="tableName">Database table name</param>
        /// <param name="manager">Table manager instance to use</param>
        /// <param name="isSystem">If it is true, all field are assigned like system</param>
        public void LoadFromDataStructure(string tableName, TableManager manager, bool isSystem = false)
        {
            // Use manager with default connection string if not provided
            if (manager == null)
            {
                manager = new TableManager(null);
            }

            var dataStructure = manager.GetColumnInformation(tableName);
            if (DataHelper.DataSourceIsEmpty(dataStructure))
            {
                return;
            }

            // Get primary keys
            var primaryKeys = manager.GetPrimaryKeyColumns(tableName);

            foreach (DataRow row in dataStructure.Tables[0].Rows)
            {
                var item = NewFieldDefinition() as IField;
                if (item == null)
                {
                    continue;
                }

                var columnName = DataHelper.GetDataRowValue(row, "ColumnName").ToString(null);
                var isPrimary = primaryKeys.Contains(columnName, StringComparer.InvariantCultureIgnoreCase);
                item.LoadFromTableData(row, isPrimary, isSystem);

                ItemsList.Add(item);
            }
        }


        /// <summary>
        /// Loads the field from the XML node
        /// </summary>
        /// <param name="node">XML node</param>
        protected virtual IDataDefinitionItem LoadField(XmlNode node)
        {
            var ffi = NewFieldDefinition();
            if (ffi != null)
            {
                ffi.LoadFromXmlNode(node);

                ItemsList.Add(ffi);
            }

            return ffi;
        }


        /// <summary>
        /// Gets the field of the given type
        /// </summary>
        public IEnumerable<ItemType> GetFields<ItemType>()
            where ItemType : IDataDefinitionItem
        {
            return ItemsList.OfType<ItemType>();
        }


        /// <summary>
        /// Creates a new field definition object
        /// </summary>
        protected virtual IDataDefinitionItem NewFieldDefinition()
        {
            return new FieldInfo();
        }

        #endregion
    }
}
