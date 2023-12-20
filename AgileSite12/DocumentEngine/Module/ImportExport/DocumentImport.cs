using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Scheduler;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Synchronization;
using CMS.Taxonomy;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the Documents import process.
    /// </summary>
    internal static class DocumentImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportCanceled.Execute += ImportCanceled_Execute;
            ImportExportEvents.ImportError.Execute += ImportError_Execute;
            ImportExportEvents.ImportObjects.Before += ImportObjectsBefore;
            ImportExportEvents.ImportObjectType.Before += ImportObjectTypeBefore;
        }


        private static void ImportObjectTypeBefore(object sender, ImportDataEventArgs e)
        {
            if (e.ObjectType != TreeNode.OBJECT_TYPE)
            {
                return;
            }

            var settings = e.Settings;
            if (settings == null)
            {
                return;
            }

            // Documents should be imported
            if (settings.IsObjectTypeProcessed(TreeNode.OBJECT_TYPE, true, ProcessObjectEnum.Default))
            {
                ImportDocumentsData(settings, e.TranslationHelper);
            }

            // Cancel further processing
            e.Cancel();
        }


        private static void ImportObjectsBefore(object sender, ImportBaseEventArgs e)
        {
            EnsureSite(e.Settings);
        }


        private static void ImportError_Execute(object sender, ImportErrorEventArgs e)
        {
            DeleteIncompleteSite(e.Settings);
        }


        private static void ImportCanceled_Execute(object sender, ImportBaseEventArgs e)
        {
            DeleteIncompleteSite(e.Settings);
        }


        /// <summary>
        /// Import documents data with all dependent objects.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        internal static void ImportDocumentsData(SiteImportSettings settings, TranslationHelper th)
        {
            CancelImportIfCanceled(settings);

            try
            {
                List<int> importedDocumentIds = new List<int>();
                TreeProvider tree = InitTreeProvider(settings);

                // Handle the event
                var e = new DocumentsImportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th
                };

                using (var h = DocumentImportExportEvents.ImportDocuments.StartEvent(e))
                {
                    if (h.CanContinue())
                    {
                        // Import documents data
                        Dictionary<int, int> importedDocuments = ImportDocuments(settings, th, ref importedDocumentIds, tree);
                        var documentsIds = importedDocuments.Keys.ToList();
                        var nodesIds = importedDocuments.Values.ToList();

                        e.ImportedDocumentIDs = documentsIds;
                        e.ImportedNodeIDs = nodesIds;

                        // Import document relationships
                        if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DOC_RELATIONSHIPS), true))
                        {
                            ImportProvider.ImportObjectType(settings, RelationshipInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, null);
                            ImportProvider.ImportObjectType(settings, RelationshipInfo.OBJECT_TYPE_ADHOC, false, th, ProcessObjectEnum.All, null);
                        }

                        ImportProvider.ImportObjectType(settings, DocumentAliasInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, nodesIds);
                        ImportProvider.ImportObjectType(settings, DocumentCategoryInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, documentsIds);

                        ImportProvider.ImportObjectType(settings, AlternativeUrlInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, documentsIds);

                        h.FinishEvent();
                    }
                }

                // Log staging tasks
                if ((!settings.LogSynchronization && !settings.LogIntegration) || (importedDocumentIds.Count <= 0))
                {
                    return;
                }

                using (CMSActionContext context = new CMSActionContext())
                {
                    // Enable staging tasks logging
                    context.LogSynchronization = settings.LogSynchronization;
                    context.LogIntegration = settings.LogIntegration;
                    context.AllowAsyncActions = true;

                    QueryDataParameters parameters = new QueryDataParameters();
                    using (SelectCondition cond = new SelectCondition(parameters))
                    {
                        cond.PrepareCondition("DocumentID", importedDocumentIds);

                        string documentsWhere = cond.WhereCondition;

                        // Some data should be logged
                        if (documentsWhere != SqlHelper.NO_DATA_WHERE)
                        {
                            DocumentSynchronizationHelper.LogDocumentChange(settings.SiteName, TreeProvider.ALL_DOCUMENTS, TaskTypeEnum.UpdateDocument, true, true, tree, SynchronizationInfoProvider.ENABLED_SERVERS, true, null, tree.AllowAsyncActions, documentsWhere);
                        }
                    }
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log exception
                ImportProvider.LogProgressError(settings, settings.GetAPIString("SiteImport.ErrorImportDocuments", "Error importing pages"), ex);
                throw;
            }
        }


        private static void CancelImportIfCanceled(SiteImportSettings settings)
        {
            if (settings.ProcessCanceled)
            {
                ImportProvider.ImportCanceled();
            }
        }


        /// <summary>
        /// Initialize tree provider.
        /// </summary>
        /// <param name="settings">Import settings</param>
        private static TreeProvider InitTreeProvider(SiteImportSettings settings)
        {
            // Create the TreeProvider
            var tree = new TreeProvider(settings.CurrentUser)
            {
                GenerateNewGuid = false,
                UseAutomaticOrdering = false,
                UpdateUser = false,
                UpdateTimeStamps = false,
                UpdateDocumentContent = false,
                AllowAsyncActions = true,
                EnsureSafeNodeAlias = false,
                AutomaticallyUpdateDocumentAlias = false,

                CheckUniqueNames = settings.ExistingSite,
                UpdatePaths = settings.ExistingSite,
                UpdateSKUColumns = false,
                LogEvents = false,
                LogSynchronization = false,
                LogIntegration = false,
                EnableNotifications = false,
                TouchCacheDependencies = false
            };

            return tree;
        }


        /// <summary>
        /// Sets documents.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        /// <param name="importedDocumentIDs">List of imported document IDs</param>
        /// <param name="tree">Tree provider</param>
        /// <returns>Hash table (key: Old DocumentID value, value: Old NodeID value)</returns>
        private static Dictionary<int, int> ImportDocuments(SiteImportSettings settings, TranslationHelper th, ref List<int> importedDocumentIDs, TreeProvider tree)
        {
            // Hash table with imported document IDs [oldDocumentId -> oldNodeId]
            Dictionary<int, int> importedDocuments = new Dictionary<int, int>();

            // Get documents data
            DataSet dsDocuments = ImportProvider.LoadObjects(settings, TreeNode.OBJECT_TYPE, true);
            if (DataHelper.DataSourceIsEmpty(dsDocuments))
            {
                return importedDocuments;
            }

            var documentCultures = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var existingDocumentAliases = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var linkedDocs = new Dictionary<int, int[]>();
            var importACLs = new Dictionary<int, int>();

            // Imported document IDs
            if (importedDocumentIDs == null)
            {
                importedDocumentIDs = new List<int>();
            }

            // Initialize tree provider
            if (tree == null)
            {
                tree = InitTreeProvider(settings);
            }
            Hashtable guidTable = new Hashtable();

            int nodeLevel = 0;
            bool isNodeToProceed = true;
            int prevNodeId = 0;

            var root = GetRootDocument(settings, dsDocuments, tree);
            int currentRootACLID = root.GetValue("NodeACLID", 0);
            int originalRootACLID = 0;

            // While there are nodes to proceed
            while (isNodeToProceed)
            {
                bool nodeAffected = false;

                // Go through all data
                foreach (DataTable dt in dsDocuments.Tables)
                {
                    CancelImportIfCanceled(settings);

                    // Ensure that the data will be processed from root to the child documents
                    DataRow[] rows = dt.Select("NodeLevel = '" + nodeLevel + "'");
                    foreach (DataRow dr in rows)
                    {
                        CancelImportIfCanceled(settings);

                        string aliasPath = "";

                        try
                        {
                            nodeAffected = true;

                            // Create the node
                            string className = ValidationHelper.GetString(dr["ClassName"], "");

                            TreeNode node = TreeNode.New(className, dr, tree);
                            node.MakeComplete(false);

                            // Save old values
                            int oldNodeId = node.NodeID;
                            int oldDocumentId = node.DocumentID;
                            int oldACLID = node.GetValue("NodeACLID", 0);

                            if (node.IsRoot())
                            {
                                originalRootACLID = oldACLID;
                            }

                            // Handle the event
                            using (var h = DocumentImportExportEvents.ImportDocument.StartEvent(settings, th, node))
                            {
                                if (h.CanContinue())
                                {
                                    aliasPath = node.NodeAliasPath;

                                    bool importNode = true;

                                    // Do not import document if already exists
                                    if (settings.ExistingSite)
                                    {
                                        DataSet existingDS = tree.SelectNodes(settings.SiteName, TreeProvider.ALL_DOCUMENTS, TreeProvider.ALL_CULTURES, false, null, "NodeAliasPath = '" + SqlHelper.EscapeQuotes(aliasPath) + "' OR NodeGUID = '" + node.NodeGUID + "'", null, TreeProvider.ALL_LEVELS, false, 1);
                                        // Document exist or another culture version wasn't imported
                                        if (!DataHelper.DataSourceIsEmpty(existingDS) && !existingDocumentAliases.Contains(aliasPath))
                                        {
                                            // Add translation for existing node
                                            DataRow existingDR = existingDS.Tables[0].Rows[0];
                                            int existingNodeId = ValidationHelper.GetInteger(existingDR["NodeID"], 0);
                                            int existingDocumentId = ValidationHelper.GetInteger(existingDR["DocumentID"], 0);
                                            int existingNodeACLID = ValidationHelper.GetInteger(existingDR["NodeACLID"], 0);

                                            // Add translation record
                                            if (oldNodeId != prevNodeId)
                                            {
                                                th.AddIDTranslation(DocumentNodeDataInfo.OBJECT_TYPE, oldNodeId, existingNodeId, 0);
                                            }
                                            th.AddIDTranslation(TreeNode.OBJECT_TYPE, oldDocumentId, existingDocumentId, 0);
                                            th.AddIDTranslation(DocumentCultureDataInfo.OBJECT_TYPE, oldDocumentId, existingDocumentId, 0);
                                            th.AddIDTranslation(AclInfo.OBJECT_TYPE, oldACLID, existingNodeACLID, 0);

                                            importNode = false;
                                        }
                                        else
                                        {
                                            // Keep imported node alias path
                                            if (!existingDocumentAliases.Contains(aliasPath))
                                            {
                                                existingDocumentAliases.Add(aliasPath);
                                            }
                                        }
                                    }

                                    if (importNode)
                                    {
                                        TreeNode importedNode = node;

                                        // Log progress
                                        ImportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("SiteImport.ImportDocument", "Importing document:") + " " + node.NodeAliasPath + " (" + node.DocumentCulture + ")");

                                        EnsureUniqueNodeGUID(guidTable, node);

                                        if (node.IsRoot())
                                        {
                                            // Preserve root values
                                            node.NodeParentID = 0;
                                            node.NodeID = root.NodeID;
                                            node.OriginalNodeID = root.NodeID;
                                            node.DocumentID = root.DocumentID;
                                        }
                                        else
                                        {
                                            // Get node parent
                                            node.NodeParentID = th.GetNewID(DocumentNodeDataInfo.OBJECT_TYPE, node.NodeParentID, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                                            if (node.NodeParentID == 0)
                                            {
                                                throw new Exception("[ImportProvider.ImportDocuments]: Cannot find parent node for page '" + node.NodeAliasPath + "(" + node.DocumentCulture + ")'.");
                                            }
                                        }

                                        // Save the ACL ID
                                        if (oldACLID > 0)
                                        {
                                            importACLs[oldACLID] = oldNodeId;
                                        }

                                        node.NodeACLID = currentRootACLID;
                                        // Add translation record
                                        th.AddIDTranslation(AclInfo.OBJECT_TYPE, oldACLID, currentRootACLID, 0);

                                        // Temporarily set NULLs
                                        node.DocumentCheckedOutVersionHistoryID = 0;
                                        node.DocumentPublishedVersionHistoryID = 0;
                                        node.DocumentIsWaitingForTranslation = false;

                                        node.NodeSiteID = root.NodeSiteID;

                                        // Set additional IDs columns
                                        TranslateDocumentColumns(settings, node, th);

                                        // Refresh macro security for imported node
                                        RefresMacroSecurity(settings, node);

                                        // Handle linked documents
                                        int oldNodeLinkedNodeId = DataHelper.GetIntValue(dr, "NodeLinkedNodeID");

                                        // If current document is link
                                        bool isLink = (oldNodeLinkedNodeId > 0);
                                        if (isLink)
                                        {
                                            // Get new node ID
                                            int newNodeLinkedNodeId = th.GetNewID(DocumentNodeDataInfo.OBJECT_TYPE, oldNodeLinkedNodeId, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                                            // If document which is a target of the link has been already imported, create link
                                            if (newNodeLinkedNodeId != 0)
                                            {
                                                // Do not insert duplicate links for other culture versions of the same document
                                                if (oldNodeId != prevNodeId)
                                                {
                                                    node.NodeLinkedNodeID = newNodeLinkedNodeId;
                                                    node.NodeLinkedNodeSiteID = root.NodeSiteID;

                                                    TreeNode parent = tree.SelectSingleNode(node.NodeParentID, TreeProvider.ALL_CULTURES);
                                                    node.InsertAsLink(parent, useDocumentHelper: false);
                                                }
                                            }
                                            // Else create temporary standard document
                                            else
                                            {
                                                // Create link as standard document
                                                node.NodeLinkedNodeID = 0;
                                                node.NodeLinkedNodeSiteID = 0;

                                                // If previous node ID is the same, insert new node culture version
                                                if (oldNodeId == prevNodeId)
                                                {
                                                    int nodeId = th.GetNewID(DocumentNodeDataInfo.OBJECT_TYPE, oldNodeId, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                                                    node.NodeID = nodeId;

                                                    // Culture version uses complete data, there is no need to detect changes
                                                    node.ResetChanges();
                                                    node.InsertAsNewCultureVersion(null, false);
                                                }
                                                else
                                                {
                                                    TreeNode parent = tree.SelectSingleNode(node.NodeParentID, TreeProvider.ALL_CULTURES);
                                                    node.Insert(parent, false);

                                                    // Keep information for the correction
                                                    linkedDocs[node.NodeID] = new[] { oldNodeLinkedNodeId, oldDocumentId, node.DocumentID };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // If previous node ID is the same, insert new node culture version
                                            if (oldNodeId == prevNodeId)
                                            {
                                                int nodeId = th.GetNewID(DocumentNodeDataInfo.OBJECT_TYPE, oldNodeId, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                                                node.NodeID = nodeId;

                                                // Culture version uses complete data, there is no need to detect changes
                                                node.ResetChanges();
                                                node.InsertAsNewCultureVersion(null, false);
                                            }
                                            // Else insert new node
                                            else
                                            {
                                                // ### Special cases - Update root
                                                if (node.IsRoot())
                                                {
                                                    DocumentHelper.CopyNodeData(node, root, new CopyNodeDataSettings(true, null) { CopySKUData = false });

                                                    root.Update();

                                                    importedNode = root;
                                                }
                                                else
                                                {
                                                    TreeNode parent = tree.SelectSingleNode(node.NodeParentID, TreeProvider.ALL_CULTURES);
                                                    node.Insert(parent, false);
                                                }
                                            }

                                            // Add translation record for document
                                            th.AddIDTranslation(TreeNode.OBJECT_TYPE, oldDocumentId, importedNode.DocumentID, 0);
                                            th.AddIDTranslation(DocumentCultureDataInfo.OBJECT_TYPE, oldDocumentId, importedNode.DocumentID, 0);

                                            importedDocuments[oldDocumentId] = oldNodeId;

                                            // Store document cultures
                                            if (!documentCultures.Contains(importedNode.DocumentCulture))
                                            {
                                                documentCultures.Add(importedNode.DocumentCulture);
                                            }
                                        }

                                        // Store imported document ID for staging tasks logging
                                        if (settings.LogSynchronization || settings.LogIntegration)
                                        {
                                            importedDocumentIDs.Add(importedNode.DocumentID);
                                        }

                                        // Update search index for new node
                                        if (DocumentHelper.IsSearchTaskCreationAllowed(importedNode))
                                        {
                                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, importedNode.GetSearchID(), importedNode.DocumentID);
                                        }

                                        // Add translation records except the culture versions of the same document
                                        if (oldNodeId != prevNodeId)
                                        {
                                            th.AddIDTranslation(DocumentNodeDataInfo.OBJECT_TYPE, oldNodeId, importedNode.NodeID, 0);
                                        }

                                        h.EventArguments.Document = importedNode;
                                    }
                                }

                                h.FinishEvent();
                            }

                            prevNodeId = oldNodeId;
                        }
                        catch (ProcessCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            // Add information to the log
                            ImportProvider.LogProgressError(settings, settings.GetAPIString("SiteImport.ErrorImportDoc", "Error importing page") + " '" + aliasPath + "'", ex);

                            throw;
                        }
                    }
                }
                if (!nodeAffected)
                {
                    isNodeToProceed = false;
                }

                nodeLevel++;
            }

            CorrectLinkedDocuments(settings, th, tree, linkedDocs);
            AddDocumentCulturesToSite(settings, documentCultures);

            var backwardCompatibilityACLOwners = ImportDocumentsACLs(settings, th, importACLs, currentRootACLID, originalRootACLID);

            // Free the memory
            GC.Collect();

            var documentsIds = importedDocuments.Keys.ToList();
            ImportDocumentsAttachments(settings, th, documentsIds);

            // Free the memory
            GC.Collect();

            ImportDocumentsHistory(settings, th, documentsIds);
            ImportDocumentsScheduledTasks(settings, th, documentsIds);
            ImportDocumentsAdditionalProperties(settings, th, tree, dsDocuments, importedDocuments, currentRootACLID, backwardCompatibilityACLOwners);

            return importedDocuments;
        }


        private static TreeNode GetRootDocument(SiteImportSettings settings, DataSet dsDocuments, TreeProvider tree)
        {
            return settings.ExistingSite ?
                tree.SelectSingleNode(settings.SiteName, "/", TreeProvider.ALL_CULTURES, false, SystemDocumentTypes.Root, false) :
                CreateSiteRoot(settings, dsDocuments, tree);
        }


        private static void AddDocumentCulturesToSite(SiteImportSettings settings, HashSet<string> documentCultures)
        {
            foreach (string culture in documentCultures)
            {
                // Insert only if not yet on site (prevent license limitations exception)
                if (!CultureSiteInfoProvider.IsCultureOnSite(culture, settings.SiteName))
                {
                    CultureSiteInfoProvider.AddCultureToSite(culture, settings.SiteName);
                }
            }
        }


        private static HashSet<int> ImportDocumentsACLs(SiteImportSettings settings, TranslationHelper th, Dictionary<int, int> importACLs, int currentRootACLID, int originalRootACLID)
        {
            var aclOwners = new HashSet<int>();

            if (importACLs.Count == 0)
            {
                return aclOwners;
            }

            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DOC_ACLS), true))
            {
                return aclOwners;
            }

            DataSet aclDS = ImportProvider.LoadObjects(settings, AclInfo.OBJECT_TYPE, false);
            if (DataHelper.DataSourceIsEmpty(aclDS))
            {
                return aclOwners;
            }

            aclOwners = ImportACLs(settings, th, aclDS, importACLs, currentRootACLID, originalRootACLID);
            ImportACLItems(settings, th, aclDS, importACLs);

            return aclOwners;
        }


        private static HashSet<int> ImportACLs(SiteImportSettings settings, TranslationHelper th, DataSet aclDS, Dictionary<int, int> importACLs, int currentRootACLID, int originalRootACLID)
        {
            var aclOwners = new HashSet<int>();

            DataTable aclsDT = aclDS.Tables["CMS_ACL"];
            if (DataHelper.DataSourceIsEmpty(aclsDT))
            {
                return aclOwners;
            }

            ImportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("SiteImport.ImportACLs", "Importing ACLs"));

            // First cycle to initialize ACL hash table and create incomplete ACLs
            foreach (DataRow dr in aclsDT.Rows)
            {
                CancelImportIfCanceled(settings);

                // Only imported ACLs
                int oldACLID = Convert.ToInt32(dr["ACLID"]);
                if (!importACLs.ContainsKey(oldACLID))
                {
                    continue;
                }

                try
                {
                    int newACLID = currentRootACLID;

                    // If current ACL is not for the root, create ACL
                    if (originalRootACLID != oldACLID)
                    {
                        // Create new incomplete ACL
                        AclInfo newACL = new AclInfo(dr);
                        newACL.ACLID = 0;
                        newACL.ACLInheritedACLs = currentRootACLID.ToString();
                        newACL.ACLSiteID = settings.SiteId;
                        newACL.Generalized.InsertData();
                        newACLID = newACL.ACLID;
                    }
                    // If current ACL is for the root, the ACL is already created (in the CreateSiteRoot method)
                    else
                    {
                        Guid aclGUID = ValidationHelper.GetGuid(dr["ACLGUID"], Guid.Empty);

                        // Update ACL GUID
                        var rootACL = AclInfoProvider.GetAclInfo(currentRootACLID);
                        rootACL.ACLGUID = aclGUID;
                        rootACL.Update();
                    }

                    // Add translation record
                    th.AddIDTranslation(AclInfo.OBJECT_TYPE, oldACLID, newACLID, 0);
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Add information to the log
                    ImportProvider.LogProgressError(settings, settings.GetAPIString("SiteImport.ErrorImportACLs", "Error importing ACLs") + " (old ACLID: " + oldACLID + ")", ex);
                    throw;
                }
            }

            // Second cycle to ensure inherited ACLs list translation
            foreach (DataRow dr in aclsDT.Rows)
            {
                CancelImportIfCanceled(settings);

                // Only items for imported ACLs
                int oldAclId = Convert.ToInt32(dr["ACLID"]);
                if (!importACLs.ContainsKey(oldAclId))
                {
                    continue;
                }

                int newAclId = th.GetNewID(AclInfo.OBJECT_TYPE, oldAclId, "ACLGUID", settings.SiteId, "ACLSiteID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                try
                {
                    if (newAclId != 0)
                    {
                        // Get old inherited ACLs
                        string oldInheritedACLs = ValidationHelper.GetString(dr["ACLInheritedACLs"].ToString(), null);
                        string inheritedACLs = "";
                        if (!string.IsNullOrEmpty(oldInheritedACLs))
                        {
                            inheritedACLs = GetInheritedACLs(settings, oldInheritedACLs, th);
                        }

                        // Update ACL inherited ACLs
                        AclInfo acl = AclInfoProvider.GetAclInfo(newAclId);
                        acl.ACLInheritedACLs = inheritedACLs;
                        acl.Update();
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Add information to the log
                    ImportProvider.LogProgressError(settings, settings.GetAPIString("SiteImport.ErrorImportACLs", "Error importing ACLs") + " (old ACLID: " + oldAclId + ")", ex);
                    throw;
                }
            }

            return aclOwners;
        }


        private static void ImportDocumentsAttachments(SiteImportSettings settings, TranslationHelper th, List<int> documentsIds)
        {
            // Import attachments
            ImportProvider.ImportObjectType(settings, AttachmentInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, documentsIds);
        }


        private static void ImportACLItems(SiteImportSettings settings, TranslationHelper th, DataSet aclDS, Dictionary<int,int> importACLs)
        {
            ImportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("SiteImport.ImportACLItems", "Importing ACL items"));

            DataTable aclItemsDT = aclDS.Tables["CMS_ACLItem"];
            if (DataHelper.DataSourceIsEmpty(aclItemsDT))
            {
                return;
            }

            foreach (DataRow dr in aclItemsDT.Rows)
            {
                CancelImportIfCanceled(settings);

                int oldACLID = Convert.ToInt32(dr["ACLID"]);
                if (!importACLs.ContainsKey(oldACLID))
                {
                    continue;
                }
                int newACLID = th.GetNewID(AclInfo.OBJECT_TYPE, oldACLID, "ACLGUID", settings.SiteId, "ACLSiteID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                try
                {
                    // Get old ID values
                    int oldUserId = DataHelper.GetIntValue(dr, "UserID");
                    int oldRoleId = DataHelper.GetIntValue(dr, "RoleID");
                    int oldLastModUserId = DataHelper.GetIntValue(dr, "LastModifiedByUserID");

                    // Get new ID values
                    int userId = th.GetNewID(UserInfo.OBJECT_TYPE, oldUserId, "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                    int roleId = th.GetNewID(RoleInfo.OBJECT_TYPE, oldRoleId, "RoleName", settings.SiteId, "SiteID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, "RoleGroupID");
                    int lastModUserId = th.GetNewID(UserInfo.OBJECT_TYPE, oldLastModUserId, "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                    if (lastModUserId == 0)
                    {
                        lastModUserId = settings.AdministratorId;
                    }

                    // Import only for valid data
                    if ((newACLID != 0) && ((userId != 0) || (roleId != 0)))
                    {
                        // Create new ACLItem
                        AclItemInfo aclItem = new AclItemInfo(dr);
                        // Set new ID
                        aclItem.ACLID = newACLID;

                        // Set the ACL item due to type
                        if (userId != 0)
                        {
                            aclItem.UserID = userId;
                        }
                        else if (roleId != 0)
                        {
                            aclItem.RoleID = roleId;
                        }

                        aclItem.LastModifiedByUserID = lastModUserId;
                        aclItem.Generalized.InsertData();
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ImportProvider.LogProgressError(settings, settings.GetAPIString("ImportSite.ErrorImportACLItems", "Error importing ACL items") + " (old ACLID: " + oldACLID + ")", ex);
                    throw;
                }
            }
        }


        private static void ImportDocumentsAdditionalProperties(SiteImportSettings settings, TranslationHelper th, TreeProvider tree, DataSet dsDocuments, Dictionary<int, int> importedDocuments, int currentRootACLID, HashSet<int> backwardCompatibilityACLOwners)
        {
            // Log setting additional properties
            ImportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("SiteImport.ImportDocsAdditionalPoperties", "Importing additional page properties"));

            // For all documents set additional properties
            foreach (DataTable dt in dsDocuments.Tables)
            {
                CancelImportIfCanceled(settings);

                foreach (DataRow dr in dt.Rows)
                {
                    CancelImportIfCanceled(settings);

                    // Only update imported documents
                    int oldDocumentId = Convert.ToInt32(dr["DocumentID"]);
                    if (importedDocuments.ContainsKey(oldDocumentId))
                    {
                        try
                        {
                            string documentCulture = dr["DocumentCulture"].ToString();
                            bool change = false;

                            // Checked out version history
                            int oldCheckedOutVersionHistoryId = DataHelper.GetIntValue(dr, "DocumentCheckedOutVersionHistoryID");
                            int newCheckedOutVersionHistoryId = 0;
                            if (oldCheckedOutVersionHistoryId > 0)
                            {
                                newCheckedOutVersionHistoryId = th.GetNewID(VersionHistoryInfo.OBJECT_TYPE, oldCheckedOutVersionHistoryId, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                                change = true;
                            }

                            // Published version history
                            int oldPublishedVersionHistoryId = DataHelper.GetIntValue(dr, "DocumentPublishedVersionHistoryID");
                            int newPublishedVersionHistoryID = 0;
                            if (oldPublishedVersionHistoryId > 0)
                            {
                                newPublishedVersionHistoryID = th.GetNewID(VersionHistoryInfo.OBJECT_TYPE, oldPublishedVersionHistoryId, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                                change = true;
                            }

                            // ACL ID
                            int oldACLId = DataHelper.GetIntValue(dr, "NodeACLID");
                            int newACLId = 0;
                            if (oldACLId > 0)
                            {
                                newACLId = th.GetNewID(AclInfo.OBJECT_TYPE, oldACLId, "ACLGUID", settings.SiteId, "ACLSiteID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                                if (newACLId != currentRootACLID)
                                {
                                    change = true;
                                }
                            }

                            // Get tree node if change occurs
                            if (change)
                            {
                                // Get initial data
                                int oldNodeId = Convert.ToInt32(dr["NodeID"]);

                                int newNodeId = th.GetNewID(DocumentNodeDataInfo.OBJECT_TYPE, oldNodeId, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                                TreeNode node = tree.SelectSingleNode(newNodeId, documentCulture);
                                if (node != null)
                                {
                                    change = false;

                                    // Checked out version history
                                    if (node.SetIntegerValue("DocumentCheckedOutVersionHistoryID", newCheckedOutVersionHistoryId, false))
                                    {
                                        change = true;
                                    }

                                    // Published version history
                                    if (node.SetIntegerValue("DocumentPublishedVersionHistoryID", newPublishedVersionHistoryID, false))
                                    {
                                        change = true;
                                    }

                                    // Set new node ACLID
                                    if (node.SetIntegerValue("NodeACLID", newACLId, false))
                                    {
                                        change = true;

                                        // If ACL ID is different, ensure correct ownership flag from imported document
                                        node.NodeIsACLOwner = DataHelper.GetBoolValue(dr, "NodeIsACLOwner");
                                        
                                    }

                                    // Update tree node if some value changed
                                    if (change)
                                    {
                                        node.Update();

                                        // Update search index for node
                                        if (DocumentHelper.IsSearchTaskCreationAllowed(node))
                                        {
                                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                                        }
                                    }
                                }
                            }
                        }
                        catch (ProcessCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            // Add information to the log
                            ImportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("SiteImport.ErrorImportDocsAdditionalPoperties", "Error importing additional page properties"), ValidationHelper.GetString(dr["NodeAliasPath"], "")), ex);
                            throw;
                        }
                    }
                }
            }
        }


        private static void ImportDocumentsScheduledTasks(SiteImportSettings settings, TranslationHelper th, List<int> documentsIds)
        {
            ImportProvider.ImportObjectType(settings, TaskInfo.OBJECT_TYPE_OBJECTTASK, false, th, ProcessObjectEnum.All, documentsIds);
        }


        private static void ImportDocumentsHistory(SiteImportSettings settings, TranslationHelper th, List<int> documentsIds)
        {
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DOC_HISTORY), true))
            {
                return;
            }

            Dictionary<int, int> importedVersions = ImportProvider.ImportObjectType(settings, VersionHistoryInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, documentsIds);
            ImportProvider.ImportObjectType(settings, AttachmentHistoryInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, documentsIds);
            var versions = importedVersions.Keys.ToList();
            ImportProvider.ImportObjectType(settings, WorkflowHistoryInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, versions);
        }


        private static void CorrectLinkedDocuments(SiteImportSettings settings, TranslationHelper th, TreeProvider tree, Dictionary<int, int[]> linkedDocs)
        {
            string lastDocument = null;
            try
            {
                foreach (int nodeId in linkedDocs.Keys)
                {
                    CancelImportIfCanceled(settings);

                    // Get temporary document
                    int[] value = linkedDocs[nodeId];

                    // Get new ID of the original document
                    int oldNodeLinkedNodeId = value[0];
                    int newNodeLinkedNodeId = th.GetNewID(DocumentNodeDataInfo.OBJECT_TYPE, oldNodeLinkedNodeId, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                    if (newNodeLinkedNodeId > 0)
                    {
                        // Convert the document to the link
                        TreeNode node = tree.SelectSingleNode(nodeId);
                        if (node != null)
                        {
                            lastDocument = node.NodeAliasPath;

                            // Convert document to link if original is present
                            DocumentHelper.ChangeDocumentToLink(node, newNodeLinkedNodeId, tree);
                        }
                    }
                    else
                    {
                        // Keep the document physical, log document id conversion
                        int oldDocumentId = value[1];
                        int newDocumentId = value[2];
                        th.AddIDTranslation(TreeNode.OBJECT_TYPE, oldDocumentId, newDocumentId, 0);
                        th.AddIDTranslation(DocumentCultureDataInfo.OBJECT_TYPE, oldDocumentId, newDocumentId, 0);
                    }
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Add information to the log
                ImportProvider.LogProgressError(settings, settings.GetAPIString("SiteImport.ErrorLinkedDoc", "Error importing linked page") + " '" + lastDocument + "'", ex);
                throw;
            }
        }


        /// <summary>
        /// Sets additional document ID columns.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="node">Tree node</param>
        /// <param name="th">Translation helper</param>
        private static void TranslateDocumentColumns(SiteImportSettings settings, TreeNode node, TranslationHelper th)
        {
            th.SetDefaultValue(UserInfo.OBJECT_TYPE, settings.AdministratorId);

            // Workflow step
            bool importHistories = ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DOC_HISTORY), true);
            bool workflowEnabled = LicenseHelper.CheckFeature(settings.CurrentUrl, FeatureEnum.WorkflowVersioning);
            if (importHistories && workflowEnabled)
            {
                int oldObjectId = node.GetValue("DocumentWorkflowStepID", 0);
                if (oldObjectId > 0)
                {
                    node.SetIntegerValue("DocumentWorkflowStepID", th.GetNewID(WorkflowStepInfo.OBJECT_TYPE, oldObjectId, "StepName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, "StepWorkflowID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN), false);
                }
            }
            else
            {
                node.SetIntegerValue("DocumentWorkflowStepID", 0, false);
            }

            // Page template
            node.DocumentPageTemplateID = th.GetNewID(PageTemplateInfo.OBJECT_TYPE, node.DocumentPageTemplateID, "PageTemplateCodeName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
            node.NodeTemplateID = th.GetNewID(PageTemplateInfo.OBJECT_TYPE, node.NodeTemplateID, "PageTemplateCodeName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

            // CSS stylesheet
            node.DocumentStylesheetID = th.GetNewID(CssStylesheetInfo.OBJECT_TYPE, node.DocumentStylesheetID, "StylesheetName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);


            // User IDs
            int userId = node.DocumentCheckedOutByUserID;
            node.SetIntegerValue("DocumentCheckedOutByUserID", th.GetNewID(UserInfo.OBJECT_TYPE, userId, "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN), false);

            userId = node.GetValue("DocumentCreatedByUserID", 0);
            node.SetIntegerValue("DocumentCreatedByUserID", th.GetNewID(UserInfo.OBJECT_TYPE, userId, "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN), false);

            userId = node.GetValue("DocumentModifiedByUserID", 0);
            node.SetIntegerValue("DocumentModifiedByUserID", th.GetNewID(UserInfo.OBJECT_TYPE, userId, "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN), false);

            node.SetIntegerValue("NodeOwner", th.GetNewID(UserInfo.OBJECT_TYPE, node.NodeOwner, "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN), false);

            // Node group ID
            int groupId = node.GetValue("NodeGroupID", 0);
            node.SetIntegerValue("NodeGroupID", th.GetNewID(PredefinedObjectType.GROUP, groupId, "GroupName", settings.SiteId, "GroupSiteID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN), false);

            // Tag group ID
            node.DocumentTagGroupID = th.GetNewID(TagGroupInfo.OBJECT_TYPE, node.DocumentTagGroupID, "TagGroupName", settings.SiteId, "TagGroupSiteID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

            // Raise event to translate custom columns
            if (ColumnsTranslationEvents.TranslateColumns.IsBound)
            {
                ColumnsTranslationEvents.TranslateColumns.StartEvent(th, TreeNode.OBJECT_TYPE, node);
            }

            th.RemoveDefaultValue(UserInfo.OBJECT_TYPE);
        }


        /// <summary>
        /// Translate inherited ACL list.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="oldAclList">Old ACL list</param>
        /// <param name="th">Translation helper</param>
        private static string GetInheritedACLs(SiteImportSettings settings, string oldAclList, TranslationHelper th)
        {
            string aclList = "";

            // Get ACLs
            string[] acls = oldAclList.Split(',');

            // Translate ACLs
            foreach (string strOldAcl in acls)
            {
                int oldAcl = ValidationHelper.GetInteger(strOldAcl, 0);
                int newAcl = th.GetNewID(AclInfo.OBJECT_TYPE, oldAcl, "ACLGUID", settings.SiteId, "ACLSiteID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                if (newAcl != 0)
                {
                    aclList += ((aclList == "") ? "" : ",") + newAcl;
                }
            }
            return aclList;
        }


        private static void EnsureUniqueNodeGUID(Hashtable guidTable, TreeNode node)
        {
            int actualNodeId = node.NodeID;
            string actualGuid = ValidationHelper.GetString(node.GetValue("NodeGUID"), "");
            string newGuid = actualGuid;

            if (guidTable.ContainsKey(actualGuid))
            {
                string[] parts = ((string)guidTable[actualGuid]).Split(';');
                int processedNodeId = Convert.ToInt32(parts[0]);
                string processedGuid = parts[1];

                // Not just only different culture versions
                Guid guid = actualNodeId != processedNodeId ? Guid.NewGuid() : new Guid(processedGuid);

                newGuid = guid.ToString();

                // If there is a new GUID, update the node
                if (actualGuid != newGuid)
                {
                    node.NodeGUID = guid;
                }
            }

            // Add the GUID to the hash table
            guidTable[actualGuid] = actualNodeId + ";" + newGuid;
        }


        private static void EnsureSite(SiteImportSettings settings)
        {
            // Import site if selected and not import to existing site
            bool importSite = settings.ImportSite;
            if (!importSite)
            {
                return;
            }

            if (!settings.ExistingSite)
            {
                CreateSite(settings);
            }
            else
            {
                // Get values from existing site
                SiteInfo site = SiteInfoProvider.GetSiteInfo(settings.SiteId);
                if (site == null)
                {
                    string message = "[ImportProvider.ImportObjectsData]: Existing site ID " + settings.SiteId + " not found.";
                    ImportProvider.LogProgress(LogStatusEnum.Error, settings, message);
                    throw new Exception(message);
                }

                settings.SiteName = site.SiteName;
            }
        }


        private static void CreateSite(SiteImportSettings settings)
        {
            CancelImportIfCanceled(settings);

            bool deleteIncomplete = false;

            try
            {
                using (new ImportSpecialCaseContext(settings))
                {
                    // Create site info
                    SiteInfo site = new SiteInfo
                    {
                        SiteID = 0,
                        DisplayName = ValidationHelper.GetString(settings.SiteDisplayName, "New site"),
                        SiteName = ValidationHelper.GetString(settings.SiteName, "NewSite"),
                        DomainName = ValidationHelper.GetString(settings.SiteDomain, "localhost"),
                        Description = ValidationHelper.GetString(settings.SiteDescription, ""),
                        SiteIsContentOnly = settings.SiteIsContentOnly,
                        Status = SiteStatusEnum.Stopped
                    };

                    // Set site info
                    SiteInfoProvider.SetSiteInfo(site);
                    settings.SiteId = site.SiteID;

                    deleteIncomplete = true;

                    // Create site root when documents are not imported
                    if (!settings.IsObjectTypeProcessed(TreeNode.OBJECT_TYPE, true, ProcessObjectEnum.Default))
                    {
                        var tree = InitTreeProvider(settings);
                        CreateSiteRoot(settings, null, tree);
                    }
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Cancel delete incomplete settings if necessary
                bool deleteSite = ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DELETE_SITE), false);
                settings.SetSettings(ImportExportHelper.SETTINGS_DELETE_SITE, deleteSite && deleteIncomplete);

                // Log exception
                ImportProvider.LogProgressError(settings, settings.GetAPIString("SiteImport.CreateSiteError", "Error creating site skeleton."), ex);
                throw;
            }
        }


        private static TreeNode CreateSiteRoot(SiteImportSettings settings, DataSet dsDocuments, TreeProvider tree)
        {
            CancelImportIfCanceled(settings);

            try
            {
                var siteSettings = GetSiteRootCreationSettings(settings.SiteName, dsDocuments);
                return tree.CreateSiteRoot(siteSettings);
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Add information to the log
                ImportProvider.LogProgressError(settings, settings.GetAPIString("SiteImport.CreateSiteRootError", "Error creating root of the site."), ex);
                throw;
            }
        }


        private static SiteRootCreationSettings GetSiteRootCreationSettings(string siteName, DataSet dsDocuments)
        {
            if (DataHelper.DataSourceIsEmpty(dsDocuments))
            {
                return new SiteRootCreationSettings(siteName);
            }

            var table = dsDocuments.Tables[SystemDocumentTypes.Root.ToLowerInvariant()];
            if (table == null)
            {
                return new SiteRootCreationSettings(siteName);
            }

            // Try to get root in default, English or any culture
            var row = GetRootRecord(table, siteName);

            if (row == null)
            {
                return null;
            }

            var culture = DataHelper.GetStringValue(row, "DocumentCulture");

            return new SiteRootCreationSettings(siteName, ValidationHelper.GetGuid(row["NodeGUID"], Guid.Empty), ValidationHelper.GetGuid(row["DocumentGUID"], Guid.Empty), culture);
        }


        private static DataRow GetRootRecord(DataTable table, string siteName)
        {
            var tableEnum = table.AsEnumerable().Where(r => ValidationHelper.GetInteger(r["NodeLevel"], 0) == 0);

            // Try to get root in default culture
            return tableEnum.FirstOrDefault(r => ValidationHelper.GetString(r["DocumentCulture"], "").Equals(CultureHelper.GetDefaultCultureCode(siteName), StringComparison.InvariantCultureIgnoreCase))
                // Try to get root in English culture
                ?? tableEnum.FirstOrDefault(r => ValidationHelper.GetString(r["DocumentCulture"], "").Equals(CultureHelper.EnglishCulture.Name, StringComparison.InvariantCultureIgnoreCase))
                // Try to get root in any culture
                ?? tableEnum.FirstOrDefault();
        }


        private static void DeleteIncompleteSite(SiteImportSettings settings)
        {
            // Ensure site deletion if set
            bool deleteSite = ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_DELETE_SITE), false);
            if ((settings.SiteId == 0) || !deleteSite)
            {
                return;
            }

            try
            {
                // Log deletion
                ImportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ImportSite.DeleteSite", "Deleting incomplete site"));

                SiteInfoProvider.DeleteSiteInfo(settings.SiteInfo);
            }
            catch (Exception deletionEx)
            {
                // Log exception
                ImportProvider.LogProgressError(settings, settings.GetAPIString("ImportSite.DeleteSiteError", "Error during site deletion."), deletionEx);
            }
        }


        private static void RefresMacroSecurity(SiteImportSettings settings, TreeNode treeNode)
        {
            // Sign again all macros in object.
            if (!settings.RefreshMacroSecurity)
            {
                return;
            }

            try
            {
                var identityOption = MacroIdentityOption.FromUserInfo(settings.CurrentUser);
                MacroSecurityProcessor.RefreshSecurityParameters(treeNode, identityOption, false);
            }
            catch (Exception ex)
            {
                string message = "Signing " + TypeHelper.GetNiceObjectTypeName(treeNode.TypeInfo.ObjectType) + " " + treeNode.DocumentName + " failed: " + ex.Message;
                EventLogProvider.LogEvent(EventType.ERROR, "Import", "MACROSECURITY", message);
            }
        }

        #endregion
    }
}