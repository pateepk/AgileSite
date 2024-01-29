using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.Taxonomy;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Object hierarchy manipulation methods.
    /// </summary>
    public class DocumentHierarchyHelper : HierarchyHelper
    {
        #region "Variables"

        private TreeNode mNode = null;

        private static Regex mNodeClassXMLRegex = null;
        private static Regex mNodeClassJSONRegex = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regex to parse node class id from xml data.
        /// </summary>
        public static Regex NodeClassXMLRegex
        {
            get
            {
                return mNodeClassXMLRegex ?? (mNodeClassXMLRegex = RegexHelper.GetRegex("<NodeClassID>(?<classid>[0-9]+?)</NodeClassID>", true));
            }
        }


        /// <summary>
        /// Regex to parse node class id from JSON data.
        /// </summary>
        public static Regex NodeClassJSONRegex
        {
            get
            {
                return mNodeClassJSONRegex ?? (mNodeClassJSONRegex = RegexHelper.GetRegex("\"NodeClassID\":\\s*(?<classid>[0-9]+)", true));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance which can be used to serialize given object.
        /// </summary>
        /// <param name="settings">Settings of the serialization</param>
        /// <param name="node">TreeNode to serialize</param>
        public DocumentHierarchyHelper(TraverseObjectSettings settings, TreeNode node)
            : base(settings)
        {
            mNode = node;
        }

        #endregion


        #region "General document traversal methods"

        /// <summary>
        /// Traverses hierarchical structure of tree node and processes ID callbacks.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="node">TreeNode to serialize</param>
        public static void TraverseNodeStructure(TraverseObjectSettings settings, TreeNode node)
        {
            new DocumentHierarchyHelper(settings, node).TraverseNodeStructure(settings, node, 0);
        }

        /// <summary>
        /// Traverses hierarchical structure of tree node and processes ID callbacks.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="node">TreeNode to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        protected void TraverseNodeStructure(TraverseObjectSettings settings, TreeNode node, int currentLevel)
        {
            if (node != null)
            {
                // Check if not excluded
                if (settings.ExcludedNames != null)
                {
                    foreach (string name in settings.ExcludedNames)
                    {
                        if (node.NodeName.StartsWithCSafe(name, true) ||
                            node.DocumentName.StartsWithCSafe(name, true))
                        {
                            return;
                        }
                    }
                }

                CallObjectCallback(settings, node, currentLevel);

                // Site translation
                if (node.NodeSiteID > 0)
                {
                    ProcessIDMethod(settings, node, "NodeSiteID", SiteInfo.OBJECT_TYPE, true);
                }

                // Register class translation (direct registration, since classname is known)
                ProcessIDMethod(settings, node, "NodeClassID", DataClassInfo.OBJECT_TYPE, true);

                // Register page templates translation
                if (node.DocumentPageTemplateID > 0)
                {
                    ProcessIDMethod(settings, node, "DocumentPageTemplateID", PageTemplateInfo.OBJECT_TYPE, true);
                }
                if (node.NodeTemplateID > 0)
                {
                    ProcessIDMethod(settings, node, "NodeTemplateID", PageTemplateInfo.OBJECT_TYPE, true);
                }

                // Register page stylesheet translation
                if (node.DocumentStylesheetID > 0)
                {
                    ProcessIDMethod(settings, node, "DocumentStylesheetID", CssStylesheetInfo.OBJECT_TYPE, true);
                }

                // Created by
                int createdByUserId = ValidationHelper.GetInteger(node.GetValue("DocumentCreatedByUserID"), 0);
                if (createdByUserId > 0)
                {
                    ProcessIDMethod(settings, node, "DocumentCreatedByUserID", UserInfo.OBJECT_TYPE, true);
                }

                // Modified by
                int modifiedByUserId = ValidationHelper.GetInteger(node.GetValue("DocumentModifiedByUserID"), 0);
                if (modifiedByUserId > 0)
                {
                    ProcessIDMethod(settings, node, "DocumentModifiedByUserID", UserInfo.OBJECT_TYPE, true);
                }

                // Node owner
                int nodeOwnerId = ValidationHelper.GetInteger(node.GetValue("NodeOwner"), 0);
                if (nodeOwnerId > 0)
                {
                    ProcessIDMethod(settings, node, "NodeOwner", UserInfo.OBJECT_TYPE, true);
                }

                // Register linked node translation
                int linkedNodeId = ValidationHelper.GetInteger(node.GetValue("NodeLinkedNodeID"), 0);
                if (linkedNodeId > 0)
                {
                    ProcessIDMethod(settings, node, "NodeLinkedNodeID", DocumentNodeDataInfo.OBJECT_TYPE, true);
                }

                // SKU
                if (node.NodeSKUID > 0)
                {
                    ProcessIDMethod(settings, node, "NodeSKUID", PredefinedObjectType.SKU, true);
                }

                // Group
                int groupId = ValidationHelper.GetInteger(node.GetValue("NodeGroupID"), 0);
                if (groupId > 0)
                {
                    ProcessIDMethod(settings, node, "NodeGroupID", PredefinedObjectType.GROUP, true);
                }

                // Tag group
                if (node.DocumentTagGroupID > 0)
                {
                    ProcessIDMethod(settings, node, "DocumentTagGroupID", TagGroupInfo.OBJECT_TYPE, true);
                }

                // Append child documents
                if (settings.IncludeChildren && (node.Children != null) && (node.Children.Count > 0))
                {
                    TraverseTreeCollection(settings, node.Children, "children", currentLevel);
                }

                // Append linked documents
                if (settings.DocLinkedDocuments && (node.Links != null) && (node.Links.Count > 0))
                {
                    TraverseTreeCollection(settings, node.Links, "linkeddocuments", currentLevel);
                }

                // Append connected objects
                if ((settings.DocConnectedObjects != null) && (settings.DocConnectedObjects.Count > 0) && (node.ConnectedObjects != null))
                {
                    // Create special settings for connected object export
                    TraverseObjectSettings childObjectSettings = new TraverseObjectSettings();
                    childObjectSettings.RootName = null;
                    childObjectSettings.MaxRelativeLevel = -1;
                    childObjectSettings.IncludeMetafiles = true;
                    childObjectSettings.IncludeCategories = true;
                    childObjectSettings.IncludeBindings = true;
                    childObjectSettings.ExcludedNames = settings.ExcludedNames;
                    childObjectSettings.CreateHierarchy = settings.CreateHierarchy;
                    childObjectSettings.IncludeChildren = true;
                    childObjectSettings.IncludeTranslations = settings.IncludeTranslations;

                    CallStartCollection(settings, "ConnectedObjects");

                    bool appendSeparator = false;
                    foreach (string colName in node.ConnectedObjects.CollectionNames)
                    {
                        if (settings.DocConnectedObjects.Contains(colName))
                        {
                            // Process all items
                            var item = node.ConnectedObjects[colName];
                            if ((item != null) && (item.Count > 0))
                            {
                                if (appendSeparator)
                                {
                                    ProcessArraySeparatorMethod(settings);
                                }

                                // Traverse the collection
                                TraverseObjectCollection(childObjectSettings, item, 0, false, null);
                                appendSeparator = true;
                            }
                        }
                    }

                    // Append page template
                    if (settings.DocConnectedObjects.Contains("pagetemplate"))
                    {
                        string colName = node.GetUsedPageTemplateIdColumn();

                        TraverseConnectedObject(settings, node, childObjectSettings, colName);
                    }

                    // Append SKU
                    if (settings.DocConnectedObjects.Contains("sku"))
                    {
                        TraverseConnectedObject(settings, node, childObjectSettings, "NodeSKUID");
                    }

                    // Append Group
                    if (settings.DocConnectedObjects.Contains("group"))
                    {
                        TraverseConnectedObject(settings, node, childObjectSettings, "NodeGroupID");
                    }

                    // Append TagGroup
                    if (settings.DocConnectedObjects.Contains("taggroup"))
                    {
                        TraverseConnectedObject(settings, node, childObjectSettings, "DocumentTagGroupID");
                    }

                    CallEndCollection(settings, "ConnectedObjects");
                }
            }
        }


        /// <summary>
        /// Traverses the internal connected object
        /// </summary>
        /// <param name="settings">Traverse settings</param>
        /// <param name="node">Document node</param>
        /// <param name="childObjectSettings">Child object settings</param>
        /// <param name="colName">Column name referring to the object</param>
        private string TraverseConnectedObject(TraverseObjectSettings settings, TreeNode node, TraverseObjectSettings childObjectSettings, string colName)
        {
            int objectId = node.GetValue(colName, 0);
            if ((objectId > 0) && colName.EndsWithCSafe("ID", true))
            {
                // Trim ID from the column name
                colName = colName.Substring(0, colName.Length - 2);

                BaseInfo info = (BaseInfo)node.GetProperty(colName);
                if (info != null)
                {
                    ProcessArraySeparatorMethod(settings);
                    TraverseObjectStructure(childObjectSettings, info, 0);
                }
            }

            return colName;
        }


        /// <summary>
        /// Exports given TreeNode collection.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="collection">Collection to export</param>
        /// <param name="name">Name of the collection</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void TraverseTreeCollection(TraverseObjectSettings settings, IEnumerable<TreeNode> collection, string name, int currentLevel)
        {
            ProcessStartCollectionMethod(settings, name, true);

            bool appendSeparator = false;

            foreach (TreeNode child in collection)
            {
                if (appendSeparator)
                {
                    ProcessArraySeparatorMethod(settings);
                }

                CallItemCallback(settings, false);
                base.TraverseObjectStructure(settings, child, currentLevel + 1);

                CallItemCallback(settings, true);
                appendSeparator = true;
            }

            ProcessEndCollectionMethod(settings, name, true);
        }

        #endregion


        #region "Load document data methods"

        /// <summary>
        /// Loads objects data including collection from a given XML representation.
        /// </summary>
        /// <param name="node">TreeNode to load</param>
        /// <param name="xml">XML data</param>
        /// <param name="disconnectObject">If true, object collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="updateOnlySpecifiedColumns">If true, only column contained in the <paramref name="xml"/> data are loaded (columns which are not contained are not set to null)</param>
        /// <param name="prepareStructure">If true, in create mode the structure of data set is prepared in advanced (causes the whole process to be case sensitive)</param>
        /// <param name="cultureName">Name of the culture to use for parsing double and datetime values</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        /// <returns>Translation helper</returns>
        public static TranslationHelper LoadObjectFromXML(TreeNode node, string xml, bool disconnectObject = true, bool updateOnlySpecifiedColumns = false, bool prepareStructure = true, string cultureName = null, List<string> excludedColumns = null)
        {
            // Create DataSet from given XML
            string className = string.IsNullOrEmpty(node.NodeClassName) ? GetNodeClassName(xml, ExportFormatEnum.XML) : node.NodeClassName;
            DataSet ds = DocumentHelper.GetTreeNodeDataSet(className, true, true);

            if (!updateOnlySpecifiedColumns)
            {
                // Add empty translation table to DataSet
                if (!ds.Tables.Contains(TranslationHelper.TRANSLATION_TABLE))
                {
                    DataTable translationTable = TranslationHelper.GetEmptyTable();
                    ds.Tables.Add(translationTable);
                }
            }

            // Load XML to DataSet
            XmlParserContext xmlContext = new XmlParserContext(null, null, null, XmlSpace.None);
            XmlReaderSettings rs = new XmlReaderSettings();
            rs.ConformanceLevel = ConformanceLevel.Auto;
            rs.CheckCharacters = false;

            XmlReader xmlReader = XmlReader.Create(new XmlTextReader(xml, XmlNodeType.Element, xmlContext), rs);

            List<string> updatedColumns = null;
            DataHelper.ReadDataSetFromXml(ds, xmlReader, null, null, null, out updatedColumns, cultureName);

            // Create translation helper
            TranslationHelper th = new TranslationHelper(ds.Tables[TranslationHelper.TRANSLATION_TABLE]);

            // Load data from DataSet
            LoadObjectFromDataSet(node, ds, disconnectObject, (updateOnlySpecifiedColumns ? updatedColumns : null), excludedColumns);
            return th;
        }


        /// <summary>
        /// Loads objects data including collection from a given XML representation.
        /// </summary>
        /// <param name="node">TreeNode to load</param>
        /// <param name="json">JSON data</param>
        /// <param name="disconnectObject">If true, object collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="updateOnlySpecifiedColumns">If true, only column contained in the <paramref name="json"/> data are loaded (columns which are not contained are not set to null)</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        /// <returns>Translation helper</returns>
        public static TranslationHelper LoadObjectFromJSON(TreeNode node, string json, bool disconnectObject = false, bool updateOnlySpecifiedColumns = false, List<string> excludedColumns = null)
        {
            string className = string.IsNullOrEmpty(node.NodeClassName) ? GetNodeClassName(json, ExportFormatEnum.JSON) : node.NodeClassName;
            DataSet ds = DocumentHelper.GetTreeNodeDataSet(className, true, true);
            if (ds != null)
            {
                // Add empty translation table to DataSet
                DataTable translationTable = TranslationHelper.GetEmptyTable();
                ds.Tables.Add(translationTable);

                JavaScriptSerializer serializer = CreateJavaScriptSerializer();
                IDictionary values = (IDictionary)serializer.DeserializeObject(json);

                // Create translation helper
                TranslationHelper th = new TranslationHelper(translationTable);

                List<string> columnsToUpdate = LoadDataSetFromJSON(ds, ds.Tables[0].TableName, new object[] { values });
                if (!updateOnlySpecifiedColumns)
                {
                    columnsToUpdate = null;
                }

                // Load data from DataSet
                LoadObjectFromDataSet(node, ds, disconnectObject, columnsToUpdate, excludedColumns);
                return th;
            }
            return new TranslationHelper();
        }


        /// <summary>
        /// Creates a new instance of the <see cref="JavaScriptSerializer"/> class.
        /// </summary>
        private static JavaScriptSerializer CreateJavaScriptSerializer()
        {
            var serializer = new JavaScriptSerializer();
            var length = GetMaxJsonLength();
            if (length > 0)
            {
                serializer.MaxJsonLength = length;
            }

            return serializer;
        }


        /// <summary>
        /// Gets maximum JSON length from the application configuration file.
        /// </summary>
        private static int GetMaxJsonLength()
        {
            return CoreServices.AppSettings["CMSRestMaxJsonLength"].ToInteger(0);
        }


        /// <summary>
        /// Loads TreeNode data including collection from a given data set.
        /// </summary>
        /// <param name="node">TreeNode to load</param>
        /// <param name="ds">Dataset with data</param>
        /// <param name="disconnectNode">If true, node collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="id">ID of the object to identify it within the DataSet</param>
        /// <param name="columnsToUpdate">List of columns which will be updated</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        protected static void LoadObjectFromDataSet(TreeNode node, DataSet ds, bool disconnectNode, int id, List<string> columnsToUpdate, List<string> excludedColumns)
        {
            if (node != null)
            {
                if (disconnectNode)
                {
                    node.Generalized.Disconnect();
                }
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Process root object
                    DataTable rootTable = ds.Tables[0];
                    if (rootTable != null)
                    {
                        if ((id == 0) && (rootTable.Rows.Count > 0))
                        {
                            // If there is no ID, get the first item in the given table
                            LoadObjectFromDataRow(node, rootTable.Rows[0], columnsToUpdate, excludedColumns);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Parses NodeClassName from node data.
        /// </summary>
        /// <param name="nodeData">Node data</param>
        /// <param name="format">Format of the data</param>
        public static string GetNodeClassName(string nodeData, ExportFormatEnum format)
        {
            return GetNodeClassName(nodeData, format, true);
        }


        /// <summary>
        /// Parses NodeClassName from node data.
        /// </summary>
        /// <param name="nodeData">Node data</param>
        /// <param name="format">Format of the data</param>
        /// <param name="tryToGetByClassId">Tries to get the name by extracting class identifier.</param>
        public static string GetNodeClassName(string nodeData, ExportFormatEnum format, bool tryToGetByClassId)
        {
            string toReturn = null;
            if (tryToGetByClassId)
            {
                Match match;
                switch (format)
                {
                    case ExportFormatEnum.JSON:
                        match = NodeClassJSONRegex.Match(nodeData);
                        break;

                    default:
                        match = NodeClassXMLRegex.Match(nodeData);
                        break;
                }

                if (match.Success)
                {
                    // Try to get class name by class id
                    int id = ValidationHelper.GetInteger(match.Groups["classid"].Value, 0);
                    DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(id);
                    if (dci != null)
                    {
                        toReturn = dci.ClassName;
                    }
                }
            }

            if (!string.IsNullOrEmpty(toReturn))
            {
                return toReturn;
            }

            // If ClassID is not specified, try to look at the root element
            string trimmedData = nodeData.Trim();
            int index;
            switch (format)
            {
                case ExportFormatEnum.JSON:
                    index = trimmedData.IndexOfCSafe('"');
                    break;

                default:
                    const string newDataSetString = "<NewDataSet>";
                    if (trimmedData.StartsWithCSafe(newDataSetString))
                    {
                        trimmedData = trimmedData.Remove(0, newDataSetString.Length);
                        trimmedData = trimmedData.Trim();
                    }
                    index = trimmedData.IndexOfCSafe('>');
                    break;
            }

            if (index > 1)
            {
                toReturn = trimmedData.Substring(1, index - 1).Replace("_", ".").Trim();
            }

            return toReturn;
        }


        /// <summary>
        /// Loads object data including collection from a given data set.
        /// </summary>
        /// <param name="node">TreeNode to load</param>
        /// <param name="ds">Dataset with data</param>
        /// <param name="disconnectObject">If true, object collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="columnsToUpdate">List of columns to update</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        public static void LoadObjectFromDataSet(TreeNode node, DataSet ds, bool disconnectObject = true, List<string> columnsToUpdate = null, List<string> excludedColumns = null)
        {
            LoadObjectFromDataSet(node, ds, disconnectObject, 0, columnsToUpdate, excludedColumns);
        }

        #endregion


        #region "Document export methods"

        /// <summary>
        /// Gets the syndication item created from current object
        /// </summary>
        /// <param name="content">Content</param>
        protected override SyndicationItem GetSyndicationItem(SyndicationContent content)
        {
            SyndicationItem item = null;

            if (mNode != null)
            {
                // Get from document
                DateTimeOffset modifiedTime = DateTimeOffset.MaxValue;

                if (mNode.GetValue("DocumentModifiedWhen") != null)
                {
                    modifiedTime = ValidationHelper.GetDateTime(mNode.GetValue("DocumentModifiedWhen"), DateTimeHelper.ZERO_TIME);
                }

                DateTimeOffset publishedTime = DateTimeOffset.MaxValue;

                if (mNode.GetValue("DocumentLastPublished") != null)
                {
                    publishedTime = ValidationHelper.GetDateTime(mNode.GetValue("DocumentLastPublished"), DateTimeHelper.ZERO_TIME);
                }

                item = new SyndicationItem(mNode.GetDocumentName(), content, Settings.ExportItemURI, mNode.NodeID + "", modifiedTime);
                item.PublishDate = publishedTime;
            }
            else
            {
                // Get from base
                item = base.GetSyndicationItem(content);
            }

            return item;
        }


        /// <summary>
        /// Handles export to required format.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="obj">Object (TreeNode / Info Object) to process</param>
        /// <param name="currentLevel">Current level within the object tree structure</param>
        protected override void ProcessObjectMethod(TraverseObjectSettings settings, ICMSObject obj, int currentLevel)
        {
            if (obj is TreeNode)
            {
                // TreeNode
                TreeNode node = (TreeNode)obj;

                // Append main object
                switch (settings.Format)
                {
                    case ExportFormatEnum.JSON:
                        {
                            string objResult = node.ToJSON(null, settings.Binary);
                            if (currentLevel == 0)
                            {
                                objResult = objResult.Substring(1, objResult.Length - 2);
                            }
                            ResultBuilder.Append(objResult);
                        }
                        break;

                    default:
                        ResultBuilder.Append(node.ToXML(node.NodeClassName, settings.Binary));
                        break;
                }
            }
            else
            {
                base.ProcessObjectMethod(settings, obj, currentLevel);
            }
        }


        /// <summary>
        /// Gets the metadata source object
        /// </summary>
        protected override object GetMetadataSource()
        {
            object obj = null;

            if (mNode != null)
            {
                obj = mNode;
            }
            else
            {
                obj = base.GetMetadataSource();
            }

            return obj;
        }


        /// <summary>
        /// Exports the object data
        /// </summary>
        protected override void ExportObjectData()
        {
            if (mNode != null)
            {
                // Go through the node structure
                base.TraverseObjectStructure(Settings, mNode, 0);
            }
            else
            {
                base.ExportObjectData();
            }
        }


        /// <summary>
        /// Returns true if the data can be exported
        /// </summary>
        protected override bool DataAvailable()
        {
            return (mNode != null) || base.DataAvailable();
        }

        #endregion
    }
}
