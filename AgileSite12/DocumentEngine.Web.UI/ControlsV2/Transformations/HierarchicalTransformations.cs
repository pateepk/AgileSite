using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

using CMS.Base;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// HierarchicalTransformations.
    /// </summary>
    public class HierarchicalTransformations
    {
        #region "Variables"

        private readonly string mConditionColumnName;
        private char mItemListSeparator = ';';

        private readonly Dictionary<Guid, HierarchicalTransformationInfo> mItemCollection = new Dictionary<Guid, HierarchicalTransformationInfo>();
        private readonly Dictionary<string, Guid> mLookupTable = new Dictionary<string, Guid>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether edit/delete buttons should be used for transformations
        /// </summary>
        public EditModeButtonEnum EditButtonsMode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the column name which is used for condition value.
        /// </summary>
        public string ConditionColumnName
        {
            get
            {
                return mConditionColumnName;
            }
        }


        /// <summary>
        /// Gets or sets the item list separator. This separator is used if In or NotIn TransformationConditionTypeEnum is used.
        /// </summary>
        public char ItemListSeparator
        {
            get
            {
                return mItemListSeparator;
            }
            set
            {
                mItemListSeparator = value;
            }
        }


        /// <summary>
        /// Items count in items collection.
        /// </summary>
        public int ItemsCount
        {
            get
            {
                if (mItemCollection != null)
                {
                    return mItemCollection.Count;
                }
                return 0;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// TransformationCollection constructor.
        /// </summary>
        ///// <param name="conditionColumnName">Condition column name</param>
        public HierarchicalTransformations(string conditionColumnName)
        {
            mConditionColumnName = conditionColumnName;
        }

        #endregion


        #region "XML Methods"

        /// <summary>
        /// Loads transformations from XML.
        /// </summary>
        /// <param name="xml">XML string</param>
        public void LoadFromXML(string xml)
        {
            // XML document
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            LoadFromXML(xmlDoc);
        }


        /// <summary>
        /// Loads transformations from XML document.
        /// </summary>
        /// <param name="xmlDoc">XML Document</param>
        public void LoadFromXML(XmlDocument xmlDoc)
        {
            mLookupTable.Clear();
            mItemCollection.Clear();

            if (xmlDoc == null)
            {
                return;
            }

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                HierarchicalTransformationInfo obj = new HierarchicalTransformationInfo();

                obj.ItemID = new Guid(node.Attributes["ID"].Value);
                obj.ItemLevel = Convert.ToInt32(node.Attributes["Level"].Value);
                obj.ItemType = StringToUniViewItemType(node.Attributes["Type"].Value);
                obj.TransformationName = node.Attributes["TransformationName"].Value;
                obj.Value = node.Attributes["Value"].Value;

                // Allow inheritance
                XmlNode nl = node.Attributes["ApplyToSublevels"];
                bool applyToSublevels = true;
                if (nl != null)
                {
                    applyToSublevels = ValidationHelper.GetBoolean(nl.Value, true);
                }

                obj.ApplyToSublevels = applyToSublevels;

                SetTransformation(obj);
            }
        }


        /// <summary>
        /// Generates XML for current collection.
        /// </summary>
        /// <returns>Returns XML definition</returns>
        public string GetXML()
        {
            // XML document
            XmlDocument xmlDoc = new XmlDocument();

            // Root node
            XmlElement rootNode = xmlDoc.CreateElement("Items");

            // Adding root node to the xml document
            xmlDoc.AppendChild(rootNode);

            var items = mItemCollection;

            // Order by guids to maintain consistent order on output 
            var guids = items.Keys.OrderBy(key => key);

            foreach (var guid in guids)
            {
                var item = items[guid];

                // Item node
                var xmlNode = xmlDoc.CreateElement("Item");

                var attributes = new Dictionary<string, string>
                {
                    {"ID", item.ItemID.ToString()},
                    {"Level", item.ItemLevel.ToString()},
                    {"Type", UniViewItemTypeToString(item.ItemType)},
                    {"Value", item.Value},
                    {"TransformationName", item.TransformationName},
                    {"ApplyToSublevels", item.ApplyToSublevels.ToString()}
                };

                xmlNode.AddAttributes(attributes, false);

                rootNode.AppendChild(xmlNode);
            }

            return xmlDoc.ToFormattedXmlString(true);
        }

        #endregion


        #region "Editing methods"

        /// <summary>
        /// Inserts or updates hierarchical transformation object.
        /// </summary>
        /// <param name="obj">Hierarchical transformation object</param>
        /// <returns>Returns guid id for current object</returns>
        public Guid SetTransformation(HierarchicalTransformationInfo obj)
        {
            if (obj == null)
            {
                throw new Exception("[HierarchicalTransformations.SetTransformation] transformation object is not defined.");
            }

            // Generate new guid if is not defined
            if (obj.ItemID == Guid.Empty)
            {
                obj.ItemID = Guid.NewGuid();
            }

            // Add item to the collection
            mItemCollection[obj.ItemID] = obj;

            // Insert/Update records in look up table
            foreach (string key in GetLookUpTableKeys(obj))
            {
                mLookupTable[key] = obj.ItemID;
            }

            // Return current item ID
            return obj.ItemID;
        }


        /// <summary>
        /// Removes specified object from collection.
        /// </summary>
        /// <param name="objectId">Hierarchical transformation object id</param>
        public void DeleteTransformation(Guid objectId)
        {
            HierarchicalTransformationInfo obj = null;


            // Check whether object exists
            if (mItemCollection.ContainsKey(objectId))
            {
                obj = mItemCollection[objectId];
                mItemCollection.Remove(objectId);
            }

            if (obj != null)
            {
                // Remove records from look up table
                foreach (string key in GetLookUpTableKeys(obj))
                {
                    mLookupTable.Remove(key);
                }
            }
        }


        /// <summary>
        /// Gets the hierarchical transformation object by specified id from current collection.
        /// </summary>
        /// <param name="objectId">Hierarchical transformation object id</param>
        /// <returns>Returns HierarchicalTransformationInfo object</returns>
        public HierarchicalTransformationInfo GetTransformation(Guid objectId)
        {
            // Check whether object exists
            if (mItemCollection.ContainsKey(objectId))
            {
                return mItemCollection[objectId];
            }

            return null;
        }


        /// <summary>
        /// Generates DataSet object from current collection filtered by specified parameters.
        /// </summary>
        /// <param name="level">Required level (-1 returns all items from all levels)</param>
        /// <param name="itemType">Required item type</param>
        /// <param name="documentType">Required document type</param>
        /// <returns>Returns DataSet with columns ID{Guid}, Level{int}, {ConditionColumnName} {string}, TransformationName{string}</returns>
        public DataSet GetDataSet(int level, UniViewItemType itemType, string documentType)
        {
            // Check whether at least one item is defined
            if (mItemCollection.Count > 0)
            {
                // Create data table
                DataTable dt = new DataTable();
                dt.Columns.Add("ID", typeof(Guid));
                dt.Columns.Add("Level", typeof(int));
                dt.Columns.Add(ConditionColumnName, typeof(string));
                dt.Columns.Add("TransformationName", typeof(string));
                dt.Columns.Add("Type", typeof(string));

                // Loop thru all items and add them to the dataset
                foreach (KeyValuePair<Guid, HierarchicalTransformationInfo> entry in mItemCollection)
                {
                    if ((level != -1) && (entry.Value.ItemLevel != level))
                    {
                        continue;
                    }

                    if ((itemType != UniViewItemType.All) && (itemType != entry.Value.ItemType))
                    {
                        continue;
                    }
                    if (!String.IsNullOrEmpty(documentType) && (!entry.Value.Value.ToLowerCSafe().Contains(documentType.ToLowerCSafe())))
                    {
                        continue;
                    }

                    dt.Rows.Add(new object[] { entry.Value.ItemID, entry.Value.ItemLevel, entry.Value.Value, entry.Value.TransformationName, ResHelper.GetString("hiertransf." + entry.Value.ItemType.ToString()) });
                }

                // Create data set object
                DataSet ds = new DataSet();
                ds.Tables.Add(dt);

                return ds;
            }

            // Return null by default
            return null;
        }

        #endregion


        #region "Processing methods"

        /// <summary>
        /// Gets the transformation name for specific item type, level and condition value
        /// Uses the TransformationConditionTypeEnum for comparison
        /// </summary>
        /// <param name="type">Required item type</param>
        /// <param name="level">Required level</param>
        /// <param name="conditionValue">Condition value</param>
        /// <returns>Returns transformation name</returns>
        public string GetTransformationName(UniViewItemType type, int level, string conditionValue)
        {
            // LookUp table key template
            string keyTemplate = type.ToString() + "_#LVL#_" + conditionValue;

            // Declare transformation info object
            HierarchicalTransformationInfo obj = null;

            int foundOnLevel = -1;

            // Loop thru level in descent order
            for (int i = level; i >= -1; i--)
            {
                // Replace level macro in key template
                string key = keyTemplate.Replace("#LVL#", i.ToString());

                // Check whether exists item for current key
                if (mLookupTable.ContainsKey(key))
                {
                    //Get transformation info
                    obj = mItemCollection[mLookupTable[key]];

                    // If not same level and transformation is used only for current level continue searching
                    if ((obj != null) && (level != obj.ItemLevel) && !obj.ApplyToSublevels && (obj.ItemLevel != -1))
                    {
                        obj = null;
                        continue;
                    }

                    foundOnLevel = i;
                    break;
                }
            }

            if ((foundOnLevel != level) && (obj != null))
            {
                mLookupTable.Add(keyTemplate.Replace("#LVL#", level.ToString()), obj.ItemID);
            }

            // If object exists return transformation name
            if (obj != null)
            {
                return obj.TransformationName;
            }

            // Return empty string by default
            return String.Empty;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets the collection of lookup table keys for specified object.
        /// </summary>
        /// <param name="obj">Hierarchical transformation object</param>
        private IEnumerable<string> GetLookUpTableKeys(HierarchicalTransformationInfo obj)
        {
            string rootKey = obj.ItemType + "_" + obj.ItemLevel + "_";
            string[] values = obj.Value.Split(ItemListSeparator);

            var keys = new List<string>();

            foreach (string value in values)
            {
                keys.Add(rootKey + value);
            }

            return keys;
        }

        #endregion


        #region "Enum methods"

        /// <summary>
        /// Returns UniViewItemType object from specified string.
        /// </summary>
        /// <param name="value">String value</param>
        public static UniViewItemType StringToUniViewItemType(string value)
        {
            switch (value.ToLowerCSafe())
            {
                // Item
                case "item":
                    return UniViewItemType.Item;

                // Single Item
                case "singleitem":
                    return UniViewItemType.SingleItem;

                // Alternating Item
                case "alternatingitem":
                    return UniViewItemType.AlternatingItem;

                // First Item
                case "firstitem":
                    return UniViewItemType.FirstItem;

                // Last Item
                case "lastitem":
                    return UniViewItemType.LastItem;

                // Header
                case "header":
                    return UniViewItemType.Header;

                // Footer
                case "footer":
                    return UniViewItemType.Footer;

                // Separator
                case "separator":
                    return UniViewItemType.Separator;

                // Current item
                case "currentitem":
                    return UniViewItemType.CurrentItem;

                default:
                    return UniViewItemType.All;
            }
        }


        /// <summary>
        /// Returns string representation of enum value.
        /// </summary>
        /// <param name="value">UniViewItemType value</param>
        public static string UniViewItemTypeToString(UniViewItemType value)
        {
            // Switch by enum type
            switch (value)
            {
                //All
                case UniViewItemType.All:
                    return "all";

                // Item
                case UniViewItemType.Item:
                    return "item";

                // Single Item
                case UniViewItemType.SingleItem:
                    return "singleitem";

                // Alternating Item
                case UniViewItemType.AlternatingItem:
                    return "alternatingitem";

                // First Item
                case UniViewItemType.FirstItem:
                    return "firstitem";

                // Last Item
                case UniViewItemType.LastItem:
                    return "lastitem";

                // Header
                case UniViewItemType.Header:
                    return "header";

                // Footer
                case UniViewItemType.Footer:
                    return "footer";

                // Separator
                case UniViewItemType.Separator:
                    return "separator";

                // Current item
                case UniViewItemType.CurrentItem:
                    return "currentitem";
            }

            return null;
        }

        #endregion
    }
}