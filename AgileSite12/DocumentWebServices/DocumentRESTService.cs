using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Activation;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.TranslationServices;
using CMS.WebServices;

using StreamReader = CMS.IO.StreamReader;

namespace CMS.DocumentWebServices
{
    /// <summary>
    /// REST service to access and manage documents.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DocumentRESTService : BaseRESTService, IDocumentRESTService
    {
        #region "Properties

        /// <summary>
        /// Gets the list of allowed document types separated by semicolon. Empty string means all document types are allowed.
        /// </summary>
        public string AllowedDocumentTypes
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(CurrentSiteName + ".CMSRESTAllowedDocTypes").Replace(" ", "");
            }
        }

        #endregion


        #region "Document methods"

        /// <summary>
        /// TreeProvider object.
        /// </summary>
        private TreeProvider mTreeProvider;

        private TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = RESTSecurityInvoker.IsHashAuthenticated() ? new TreeProvider() : new TreeProvider(CurrentUser));
            }
        }


        /// <summary>
        /// Returns document according to given settings.
        /// </summary>
        /// <param name="settings">Settings object</param>
        private TreeNode GetDocumentFromSettings(TraverseObjectSettings settings)
        {
            var parameters = GetSelectionParameters(settings);

            if (settings.DocVersion == "last")
            {
                return DocumentHelper.GetDocument(parameters, TreeProvider);
            }

            return TreeProvider.SelectSingleNode(parameters);
        }


        /// <summary>
        /// Returns TreeNode instance (existing for update, new instance for creations).
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="data">TreeNode data</param>
        /// <param name="isUpdate">If true, document is retrieved using settings data, otherwise new document is created</param>
        private TreeNode GetTreeNode(TraverseObjectSettings settings, string data, bool isUpdate)
        {
            TreeNode node;
            int nodeClassId = 0;
            if (isUpdate)
            {
                node = GetDocumentFromSettings(settings);
            }
            else
            {
                string className = DocumentHierarchyHelper.GetNodeClassName(data, settings.Format);
                if (string.IsNullOrEmpty(className))
                {
                    // New culture version case where class name is not specified - get the classname from existing culture
                    node = TreeProvider.SelectSingleNode(settings.DocSiteName, settings.DocAliasPath, TreeProvider.ALL_CULTURES, true, TreeProvider.ALL_CLASSNAMES, settings.WhereCondition.ToString(true), settings.OrderBy, settings.MaxRelativeLevel, settings.DocSelectOnlyPublished, settings.Columns);
                    if (node != null)
                    {
                        node = TreeNode.New(node.NodeClassName);
                    }
                }
                else
                {
                    node = TreeNode.New(className);
                }
                if (node != null)
                {
                    nodeClassId = ValidationHelper.GetInteger(node.GetValue("NodeClassID"), 0);
                }
            }

            if (node == null)
            {
                return null;
            }

            List<string> excludedColumns = null;
            if ((!AllowedSensitiveColumns || !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)))
            {
                excludedColumns = node.TypeInfo.SensitiveColumns;
            }

            switch (settings.Format)
            {
                case ExportFormatEnum.JSON:
                    DocumentHierarchyHelper.LoadObjectFromJSON(node, data, true, true, excludedColumns);
                    break;

                default:
                    string cultureName = SettingsHelper.AppSettings["CMSRESTCulture"] ?? "en-US";
                    DocumentHierarchyHelper.LoadObjectFromXML(node, data, true, true, false, cultureName, excludedColumns);
                    break;
            }

            if (isUpdate)
            {
                return node;
            }

            if ((nodeClassId > 0) && (ValidationHelper.GetInteger(node.GetValue("NodeClassID"), 0) == 0))
            {
                node.SetValue("NodeClassID", nodeClassId);
            }
            node.DocumentCulture = settings.DocCultureCode;
            node.SetValue("NodeSiteID", SiteInfoProvider.GetSiteID(settings.DocSiteName));

            return node;
        }


        /// <summary>
        /// Returns DataSet of documents matching the settings given in ExportObjectSettings object
        /// </summary>
        /// <param name="operation">Document operation</param>
        /// <param name="settings">ExportObjectSettings object</param>
        /// <param name="docTypes">Allowed document types</param>
        /// <param name="checkAllowedDocTypes">If true, allowed document types (from settings) are checked and if the document has not allowed type, null is returned</param>
        private DataSet GetDocumentsFromSettings(string operation, TraverseObjectSettings settings, string docTypes, bool checkAllowedDocTypes = true)
        {
            DataSet ds = null;

            // Get the classNames first (for single doc. retrieval, because of performace, we can't call GetDocuments with TreeProvider.ALL_CLASSNAMES parameter).
            string allowedDocTypes = docTypes;

            if (IsSingleDocumentOperation(operation) && (string.IsNullOrEmpty(settings.DocClassNames) || (settings.DocClassNames == TreeProvider.ALL_CLASSNAMES)))
            {
                var node = TreeProvider.SelectSingleNode(settings.DocSiteName, settings.DocAliasPath, TreeProvider.ALL_CULTURES, true, null, false);
                if ((node != null) && checkAllowedDocTypes)
                {
                    allowedDocTypes = CombineDocTypes(node.NodeClassName);

                    if (string.IsNullOrEmpty(allowedDocTypes))
                    {
                        return null;
                    }
                }
            }

            // Get the alias path to select, if null no selection is done
            var aliasPath = GetAliasPath(operation, settings);
            if (aliasPath != null)
            {
                var parameters = GetSelectionParameters(settings);

                parameters.AliasPath = aliasPath;
                parameters.ClassNames = allowedDocTypes;

                // Prepare parameters
                var lastVersion = (settings.DocVersion == "last");

                // Get document data
                ds = lastVersion ? DocumentHelper.GetDocuments(parameters, TreeProvider) : TreeProvider.SelectNodes(parameters);
            }

            return ds;
        }


        private static NodeSelectionParameters GetSelectionParameters(TraverseObjectSettings settings)
        {
            var parameters = new NodeSelectionParameters
            {
                SiteName = settings.DocSiteName,
                AliasPath = settings.DocAliasPath,
                CultureCode = settings.DocCultureCode,
                CombineWithDefaultCulture = settings.DocCombineWithDefaultCulture,
                ClassNames = settings.DocClassNames,
                Where = settings.WhereCondition.ToString(true),
                OrderBy = settings.OrderBy,
                MaxRelativeLevel = settings.MaxRelativeLevel,
                SelectOnlyPublished = settings.DocSelectOnlyPublished,
                TopN = settings.TopN,
                Columns = settings.Columns,
                SelectAllData = settings.DocCoupledData,
                SelectSingleNode = settings.DocSingleDocument
            };

            return parameters;
        }


        private static string GetAliasPath(string operation, TraverseObjectSettings settings)
        {
            string aliasPath = settings.DocAliasPath;

            switch (operation.ToLowerCSafe())
            {
                case "document":
                    aliasPath = settings.DocAliasPath;
                    break;

                case "all":
                    aliasPath = aliasPath + "%";
                    break;

                case "childrenof":
                    aliasPath = aliasPath.TrimEnd('/') + "/%";
                    break;

                default:
                    aliasPath = null;
                    break;
            }

            return aliasPath;
        }


        /// <summary>
        /// Selects tree node(s) according to provided parameters and returns them as dataset. 
        /// Three oprations are supported: document (= select single document), all (= select documents), childrenof (= all children of given node).
        /// If classNames not specified, the result does not contain coupled data.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        public Stream GetDocument(string operation)
        {
            var singleDocument = IsSingleDocumentOperation(operation);

            var settings = GetDocumentExportSettings("Document", singleDocument);

            if (!CheckQueryStringParameters())
            {
                return ReturnForbiddenStatus();
            }

            // Check security for documents if the version in not last or setting is set to always
            bool performSecurityCheck = SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + ".CMSRESTDocumentsSecurityCheck") || (settings.DocVersion == "last");

            // Merge columns with minimal columns for security check
            string originalColumns = settings.Columns;
            if (performSecurityCheck)
            {
                if (!string.IsNullOrEmpty(settings.Columns))
                {
                    settings.Columns = SqlHelper.MergeColumns(settings.Columns, DocumentColumnLists.SECURITYCHECK_REQUIRED_COLUMNS);
                }
            }

            // Add minimal set of required columns which is needed 
            if (!string.IsNullOrEmpty(settings.Columns))
            {
                var cols = (settings.DocVersion == "last") ? DocumentColumnLists.GETDOCUMENTS_REQUIRED_COLUMNS : DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS;
                settings.Columns = SqlHelper.MergeColumns(settings.Columns, cols);
            }

            string allowedDocTypes = CombineDocTypes(settings.DocClassNames);
            if ((settings.DocClassNames == null) || !string.IsNullOrEmpty(allowedDocTypes))
            {
                DataSet ds = GetDocumentsFromSettings(operation, settings, allowedDocTypes);

                // Put back original columns to filter the result correctly
                settings.Columns = originalColumns;

                if (performSecurityCheck && !RESTSecurityInvoker.IsHashAuthenticated())
                {
                    ds = TreeSecurityProvider.FilterDataSetByPermissions(ds, NodePermissionsEnum.Read, CurrentUser);
                }

                // When DataSet is null and operation type is not "document" than we don't want to display 404
                if (ds == null)
                {
                    ds = new DataSet();
                }

                // Set correct name to the data set and the tables
                EnsureCorrectFormat(ds, "cms.document");
                foreach (DataTable table in ds.Tables)
                {
                    table.TableName = TranslationHelper.GetSafeClassName(table.TableName);
                }

                if (!singleDocument || !DataHelper.DataSourceIsEmpty(ds))
                {
                    return GetStream(ds, "cms.document", settings);
                }
            }

            return ReturnNotFoundStatus();
        }


        /// <summary>
        /// Creates new document.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        /// <param name="stream">Stream with document data</param>
        public Stream CreateDocument(string operation, Stream stream)
        {
            if (!CheckQueryStringParameters())
            {
                return ReturnForbiddenStatus();
            }

            var data = StreamReader.New(stream).ReadToEnd();

            if (IsSingleDocumentOperation(operation))
            {
                var settings = GetDocumentExportSettings(null, true);
                var node = GetTreeNode(settings, data, false);

                if (node == null)
                {
                    return ReturnNotFoundStatus();
                }

                // Check security
                if (!string.IsNullOrEmpty(node.NodeClassName) && !IsAuthorizedForDocumentType(node.NodeClassName))
                {
                    return ReturnForbiddenStatus();
                }

                if ((node.NodeID == 0) && (node.DocumentID == 0))
                {
                    TreeNode parent;

                    // Get the parent ID
                    int parentId = node.NodeParentID;
                    if (parentId <= 0)
                    {
                        // If parent is not specified within the request body, find parent according to alias path specified in the URL
                        parent = DocumentHelper.GetDocument(settings.DocSiteName, settings.DocAliasPath, TreeProvider.ALL_CULTURES, true, TreeProvider.ALL_CLASSNAMES, null, null, TreeProvider.ALL_LEVELS, false, null, TreeProvider);
                    }
                    else
                    {
                        parent = DocumentHelper.GetDocument(parentId, TreeProvider.ALL_CULTURES, TreeProvider);
                    }

                    if (parent == null)
                    {
                        return ReturnNotFoundStatus("Parent node does not exist.");
                    }

                    // Check security
                    if (!CurrentUser.IsAuthorizedToCreateNewDocument(parent, node.NodeClassName))
                    {
                        return ReturnForbiddenStatus();
                    }

                    int linkNodeId = ValidationHelper.GetInteger(node.GetValue("NodeLinkedNodeID"), 0);
                    if ((node.NodeID == 0) && (linkNodeId > 0))
                    {
                        // Insert as a link
                        DocumentHelper.InsertDocumentAsLink(node, parent, TreeProvider);
                    }
                    else
                    {
                        // Normal insert
                        DocumentHelper.InsertDocument(node, parent, TreeProvider);
                    }
                }
                else if ((node.NodeID != 0) && (node.DocumentID == 0))
                {
                    // Get the node data set the culture and create as new culture version
                    node = TreeProvider.SelectSingleNode(node.NodeID);
                    if (node != null)
                    {
                        // Check security
                        if (!CurrentUser.IsAuthorizedToCreateNewDocument(node.NodeParentID, node.NodeClassName))
                        {
                            return ReturnForbiddenStatus();
                        }

                        // Load data from XML again (in update mode)
                        node = GetTreeNode(settings, data, true);
                        if (node == null)
                        {
                            return ReturnNotFoundStatus();
                        }

                        DocumentHelper.InsertNewCultureVersion(node, TreeProvider, settings.DocCultureCode);
                    }
                }

                return GetStream(node, "cms.document", settings);
            }

            return ReturnNotFoundStatus();
        }


        /// <summary>
        /// Deletes given document.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        public Stream DeleteDocument(string operation)
        {
            if (!CheckQueryStringParameters())
            {
                return ReturnForbiddenStatus();
            }

            if (IsSingleDocumentOperation(operation))
            {
                TraverseObjectSettings settings = GetDocumentExportSettings(null, true);
                settings.DocCombineWithDefaultCulture = false;
                settings.DocSelectOnlyPublished = false;
                settings.DocCoupledData = false;
                DataSet ds = GetDocumentsFromSettings("document", settings, settings.DocClassNames, false);
                if (DataHelper.DataSourceIsEmpty(ds))
                {
                    return ReturnNotFoundStatus("Node does not exist.");
                }

                List<TreeNode> nodes = new List<TreeNode>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int classId = ValidationHelper.GetInteger(dr["NodeClassID"], 0);
                    if (classId <= 0)
                    {
                        continue;
                    }

                    DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(classId);
                    if (dci == null)
                    {
                        continue;
                    }

                    TreeNode node = TreeNode.New(dci.ClassName, dr);

                    // Check security
                    if (!IsAuthorizedForDocumentType(node.NodeClassName))
                    {
                        return ReturnForbiddenStatus();
                    }

                    if (CurrentUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Delete) != AuthorizationResultEnum.Allowed)
                    {
                        return ReturnForbiddenStatus();
                    }

                    nodes.Add(node);
                    DocumentHelper.DeleteDocument(node, TreeProvider, settings.DocDeleteAllCultures, settings.DocDestroyHistory);
                }

                return GetResponse(nodes, settings);
            }

            return ReturnNotFoundStatus();
        }


        /// <summary>
        /// Processes given document.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        /// <param name="stream">Stream with document data</param>
        public Stream UpdateDocument(string operation, Stream stream)
        {
            if (!CheckQueryStringParameters())
            {
                return ReturnForbiddenStatus();
            }

            TraverseObjectSettings settings = GetDocumentExportSettings(null, true);
            TreeNode node;

            string data = StreamReader.New(stream).ReadToEnd();

            if (IsSingleDocumentOperation(operation))
            {
                node = GetTreeNode(settings, data, true);
                if (node == null)
                {
                    return ReturnNotFoundStatus("Node does not exist.");
                }

                // Check security
                if (!IsAuthorizedForDocumentType(node.NodeClassName))
                {
                    return ReturnForbiddenStatus();
                }
                if (CurrentUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Modify) != AuthorizationResultEnum.Allowed)
                {
                    return ReturnForbiddenStatus();
                }

                DocumentHelper.UpdateDocument(node, TreeProvider);
            }
            else
            {
                // Force the version to last
                settings.DocVersion = "last";
                settings.DocSelectOnlyPublished = false;

                node = GetDocumentFromSettings(settings);
                if (node == null)
                {
                    return ReturnNotFoundStatus("Node does not exist.");
                }

                // Check security
                if (!IsAuthorizedForDocumentType(node.NodeClassName))
                {
                    return ReturnForbiddenStatus();
                }
                if (CurrentUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Modify) != AuthorizationResultEnum.Allowed)
                {
                    return ReturnForbiddenStatus();
                }

                switch (operation.ToLowerCSafe())
                {
                    case "publish":
                        node.MoveToPublishedStep(GetQueryParam("comment"));
                        break;

                    case "checkin":
                        node.CheckIn(null, GetQueryParam("comment"));
                        break;

                    case "checkout":
                        node.CheckOut();
                        break;

                    case "archive":
                        node.Archive(GetQueryParam("comment"));
                        break;

                    case "movetonextstep":
                        node.MoveToNextStep(GetQueryParam("comment"));
                        break;

                    case "movetopreviousstep":
                        node.MoveToPreviousStep(GetQueryParam("comment"));
                        break;
                }
            }

            return GetStream(node, "cms.document", settings);
        }


        /// <summary>
        /// Returns true if the given document is 
        /// </summary>
        /// <param name="operation">Operation name</param>
        private static bool IsSingleDocumentOperation(string operation)
        {
            return operation.EqualsCSafe("document", true);
        }

        #endregion


        #region "Translation methods"

        /// <summary>
        /// Gateway for submitting an XLIFF document.
        /// </summary>
        public Stream Translate(Stream stream)
        {
            string data = StreamReader.New(stream).ReadToEnd();

            try
            {
                var result = TranslationServiceHelper.ProcessTranslation(data);

                var node = result as TreeNode;
                if (node != null)
                {
                    return GetResponse(new List<TreeNode> { node }, GetDocumentExportSettings(null));
                }

                return GetResponse((BaseInfo)result, GetDocumentExportSettings(null));
            }
            catch (TargetDocumentNotExistsException)
            {
                return ReturnNotFoundStatus("Node does not exist.");
            }
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns proper stream from given object.
        /// </summary>
        /// <param name="obj">Info object or TreeNode to get into the stream</param>
        /// <param name="objectType">Original object type</param>
        /// <param name="settings">Export settings</param>
        protected override Stream GetStream(object obj, string objectType, TraverseObjectSettings settings)
        {
            return settings.Translate ? ObjectTranslationRESTService.GetTranslation(obj, CurrentSiteName) : base.GetStream(obj, objectType, settings);
        }


        /// <summary>
        /// Returns true if given document type is allowed in settings.
        /// </summary>
        /// <param name="docType">Document type of an document</param>
        protected bool IsAuthorizedForDocumentType(string docType)
        {
            // Check the allowed object types
            string allowedTypes = ";" + AllowedDocumentTypes.ToLowerCSafe() + ";";
            if ((allowedTypes == ";;") || allowedTypes.Contains(";" + docType.ToLowerCSafe() + ";"))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Creates ExportObjectSettings object from query string parameters.
        /// </summary>
        /// <param name="rootName">Name of the root</param>
        /// <param name="isSingleDocument">If true, the request is a request to a single document</param>
        protected TraverseObjectSettings GetDocumentExportSettings(string rootName, bool isSingleDocument = false)
        {
            var settings = GetExportSettings(rootName);

            if (WebOperationContext.Current != null)
            {
                var query = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

                // Load document specific properties

                // Handle sitename
                string siteName = ValidationHelper.GetString(query["sitename"], TreeProvider.ALL_SITES);
                if (siteName.ToLowerCSafe() == "currentsite")
                {
                    siteName = CurrentSiteName;
                }
                settings.DocSiteName = siteName;

                settings.Translate = ValidationHelper.GetBoolean(query["translate"], false);

                settings.DocVersion = ValidationHelper.GetString(query["version"], "published").ToLowerCSafe();
                settings.DocAliasPath = ValidationHelper.GetString(query["aliaspath"], TreeProvider.ALL_DOCUMENTS);
                settings.DocCombineWithDefaultCulture = ValidationHelper.GetBoolean(query["combinewithdefaultculture"], true);
                settings.DocSelectOnlyPublished = ValidationHelper.GetBoolean(query["selectonlypublished"], true);
                settings.DocClassNames = ValidationHelper.GetString(query["classnames"], TreeProvider.ALL_CLASSNAMES);

                var getsAllClasses = (settings.DocClassNames == TreeProvider.ALL_CLASSNAMES);

                settings.DocCoupledData = ValidationHelper.GetBoolean(query["coupleddata"], isSingleDocument || !getsAllClasses);
                settings.DocSingleDocument = isSingleDocument;

                // Set the culture
                string culture = ValidationHelper.GetString(query["cultureCode"], CultureHelper.EnglishCulture.Name);
                if (culture.ToLowerCSafe() == "defaultculture")
                {
                    culture = CultureHelper.EnglishCulture.Name;
                }
                else if (culture.ToLowerCSafe() == "allcultures")
                {
                    culture = TreeProvider.ALL_CULTURES;
                    settings.DocCombineWithDefaultCulture = false;
                }
                settings.DocCultureCode = culture;

                settings.DocDeleteAllCultures = ValidationHelper.GetBoolean(query["DeleteAllCultures"], false);
                settings.DocDestroyHistory = ValidationHelper.GetBoolean(query["DestroyHistory"], false);
            }

            return settings;
        }


        /// <summary>
        /// Combines (intersection) requested and allowed document types to use in selecting documents methods.
        /// </summary>
        /// <param name="requestedTypes">Document types requested by users</param>
        protected string CombineDocTypes(string requestedTypes)
        {
            if (string.IsNullOrEmpty(requestedTypes) || string.IsNullOrEmpty(AllowedDocumentTypes))
            {
                // If allowed types are not defined, return all requested types
                return requestedTypes;
            }

            // If you request all classnames, the result is allowed classnames
            if (requestedTypes == TreeProvider.ALL_CLASSNAMES)
            {
                return AllowedDocumentTypes;
            }

            string[] requested = requestedTypes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] allowed = AllowedDocumentTypes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string result = "";

            // Do the intersection
            foreach (string r in requested)
            {
                foreach (string a in allowed)
                {
                    if (r.EqualsCSafe(a, true))
                    {
                        result += ";" + r;
                        break;
                    }
                }
            }
            if (result != "")
            {
                // Remove first semicolon
                result = result.Substring(1);
            }
            return result;
        }


        /// <summary>
        /// Returns SyndicationFeed of object dataset.
        /// </summary>
        /// <param name="ds">DataSet with objects data</param>
        /// <param name="objectType">Object type of the data inside</param>
        /// <param name="settings">Export settings</param>
        protected override SyndicationFeed GetObjectDataSetFeed(DataSet ds, string objectType, TraverseObjectSettings settings)
        {
            string pattern = "rest/content/site/" + settings.DocSiteName + "/{#documentculture#}/document{#nodealiaspath#}";
            return ToODATA(ds, settings, "Document", "Documents", "List of documents.", new Uri(CurrentBaseUri, "rest/content"), pattern, "DocumentModifiedWhen", "DocumentLastPublished", "DocumentName", "DocumentID");
        }


        /// <summary>
        /// Returns response for modifying methods - ID and GUID of the modified object.
        /// </summary>
        /// <param name="nodes">TreeNode objects which were created/updated/deleted</param>
        /// <param name="settings">Export settings</param>
        protected Stream GetResponse(IEnumerable<TreeNode> nodes, TraverseObjectSettings settings)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (TreeNode node in nodes)
            {
                if (node != null)
                {
                    string rootName = node.NodeClassName.Replace(".", "_");
                    string result;
                    switch (settings.Format)
                    {
                        case ExportFormatEnum.JSON:
                            result = (first ? "" : ",") + "{ \"" + rootName + "\": {\"DocumentID\": " + node.NodeID + ", \"DocumentGUID\": \"" + node.DocumentGUID + "\" }}";
                            break;

                        default:
                            result = "<" + rootName + "><DocumentID>" + node.NodeID + "</DocumentID><DocumentGUID>" + node.DocumentGUID + "</DocumentGUID></" + rootName + ">";
                            break;
                    }
                    sb.Append(result);
                    first = false;
                }
            }

            var context = WebOperationContext.Current;
            if (context != null)
            {
                string retval = sb.ToString();

                switch (settings.Format)
                {
                    case ExportFormatEnum.JSON:
                        context.OutgoingResponse.ContentType = "application/json";
                        retval = "{\"Documents\": [ " + retval + " ] }";
                        break;

                    default:
                        context.OutgoingResponse.ContentType = "text/xml";
                        retval = "<Documents>" + retval + "</Documents>";
                        break;
                }

                // Set encoding
                context.OutgoingResponse.ContentType += "; charset=" + settings.Encoding.WebName;

                MemoryStream ms = new MemoryStream(settings.Encoding.GetBytes(retval));
                ms.Position = 0;

                return ms;
            }

            return null;
        }

        #endregion
    }
}