using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.Synchronization;
using CMS.Taxonomy;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class containing document synchronization logic.
    /// </summary>
    public static class DocumentSynchronizationHelper
    {
        private const string ATTACHMENT_BINARY_COLUMN_NAME = "AttachmentBinary";

        #region "Variables"

        /// <summary>
        /// Indicates whether shared template should be synchronized with document.
        /// </summary>
        private static bool? mSynchronizeSharedTemplatesWithDocuments;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether shared templates should be synchronized with documents.
        /// </summary>
        public static bool SynchronizeSharedTemplatesWithDocuments
        {
            get
            {
                if (mSynchronizeSharedTemplatesWithDocuments == null)
                {
                    mSynchronizeSharedTemplatesWithDocuments = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSynchronizeSharedTemplatesWithDocuments"], true);
                }
                return mSynchronizeSharedTemplatesWithDocuments.Value;
            }
            set
            {
                mSynchronizeSharedTemplatesWithDocuments = value;
            }
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Indicates if logging staging tasks for content is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool LogContentChanges(string siteName)
        {
            return StagingTaskInfoProvider.LogContentChanges(siteName);
        }


        /// <summary>
        /// Indicates if the integration task should be logged.
        /// </summary>
        private static bool CheckIntegrationLogging()
        {
            return IntegrationHelper.IntegrationLogInternal;
        }


        /// <summary>
        /// Indicates if the staging task should be logged.
        /// </summary>
        /// <param name="siteName">Name of site to check</param>
        /// <param name="siteId">Identifier of site to check</param>
        /// <param name="serverId">Identifier of server to check</param>
        private static bool CheckStagingLogging(string siteName, int siteId, int serverId)
        {
            return (serverId != SynchronizationInfoProvider.ENABLED_SERVERS) || (LogContentChanges(siteName) && ServerInfoProvider.IsEnabledServer(siteId));
        }


        /// <summary>
        /// Gets document staging task title.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="node">Tree node</param>
        public static string GetTaskTitle(TaskTypeEnum taskType, TreeNode node)
        {
            // Ensure / for the root document
            string documentName = node.GetDocumentName();

            string cultureCode = CultureHelper.PreferredUICultureCode;
            string title;

            switch (taskType)
            {
                case TaskTypeEnum.CreateDocument:
                    if (node.IsLink)
                    {
                        title = ResHelper.GetAPIString("TaskTitle.CreateLinkedDocument", cultureCode, "Create linked page {0}");
                    }
                    else
                    {
                        title = ResHelper.GetAPIString("TaskTitle.CreateDocument", cultureCode, "Create page {0}");
                    }
                    break;

                case TaskTypeEnum.UpdateDocument:
                    if (node.IsLink)
                    {
                        title = ResHelper.GetAPIString("TaskTitle.UpdateLinkedDocument", cultureCode, "Update linked page {0}");
                    }
                    else
                    {
                        title = ResHelper.GetAPIString("TaskTitle.UpdateDocument", cultureCode, "Update page {0}");
                    }
                    break;

                case TaskTypeEnum.PublishDocument:
                    title = ResHelper.GetAPIString("TaskTitle.PublishDocument", cultureCode, "Publish page {0}");
                    break;

                case TaskTypeEnum.ArchiveDocument:
                    title = ResHelper.GetAPIString("TaskTitle.ArchiveDocument", cultureCode, "Archive page {0}");
                    break;

                case TaskTypeEnum.RejectDocument:
                    title = ResHelper.GetAPIString("TaskTitle.RejectDocument", cultureCode, "Reject page {0}");
                    break;

                case TaskTypeEnum.DeleteDocument:
                    if (node.IsLink)
                    {
                        title = ResHelper.GetAPIString("TaskTitle.DeleteLinkedDocument", cultureCode, "Delete linked page {0}");
                    }
                    else
                    {
                        title = ResHelper.GetAPIString("TaskTitle.DeleteDocument", cultureCode, "Delete page {0}");
                    }
                    break;

                case TaskTypeEnum.DeleteAllCultures:
                    title = ResHelper.GetAPIString("TaskTitle.DeleteAllCultures", cultureCode, "Delete all culture versions of page {0}");
                    break;

                case TaskTypeEnum.MoveDocument:
                    title = ResHelper.GetAPIString("TaskTitle.MoveDocument", cultureCode, "Move page {0}");
                    break;

                case TaskTypeEnum.BreakACLInheritance:
                    title = ResHelper.GetAPIString("TaskTitle.BreakACLInheritance", cultureCode, "Break permissions inheritance of page {0}");
                    break;

                case TaskTypeEnum.RestoreACLInheritance:
                    title = ResHelper.GetAPIString("TaskTitle.RestoreACLInheritance", cultureCode, "Restore permissions inheritance of page {0}");
                    break;

                default:
                    title = ResHelper.GetAPIString("TaskTitle.Unknown", cultureCode, "[Unknown] {0}");
                    break;
            }
            return TextHelper.LimitLength(String.Format(title, documentName), 450);
        }


        /// <summary>
        /// Gets XML of a given node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="th">Translations (if null TranslationHelper will be created and filled)</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="taskType">Task type</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <param name="siteName">Site name corresponding to document</param>
        /// <returns>XML representing document</returns>
        public static string GetDocumentXML(TreeNode node, TranslationHelper th, TreeProvider tree, TaskTypeEnum taskType, TaskDataTypeEnum dataType, TaskParameters taskParams, string siteName)
        {
            // Get caching keys
            string storageKey = "documentdata|" + node.NodeClassName + "|" + node.DocumentID;
            string key = "|xml|" + taskType + "|" + dataType + "|" + ((taskParams != null) ? taskParams.RequestStockKey : null) + "|" + siteName;

            // Try to load XML from cache
            if (CMSActionContext.CurrentUseCacheForSynchronizationXMLs && (node.DocumentID > 0))
            {
                string cachedXml = (string)RequestStockHelper.GetItem(storageKey, key, false);
                if (cachedXml != null)
                {
                    return cachedXml;
                }
            }
            // Generate XML from dataset
            DataSet ds = GetDocumentDataSet(node, th, taskType, dataType, taskParams, siteName);
            string documentXml = ds.GetXml();

            // Cache generated XML
            if (CMSActionContext.CurrentUseCacheForSynchronizationXMLs)
            {
                RequestStockHelper.AddToStorage(storageKey, key, documentXml, false);
            }

            return documentXml;
        }


        /// <summary>
        /// Gets a dataset with data of a node and corresponding translation helper.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="th">Translations (if null TranslationHelper will be created and filled)</param>
        /// <param name="taskType">Task type</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <param name="siteName">Documents site name</param>
        /// <returns>DataSet representing document</returns>
        private static DataSet GetDocumentDataSet(TreeNode node, TranslationHelper th, TaskTypeEnum taskType, TaskDataTypeEnum dataType, TaskParameters taskParams, string siteName)
        {
            // Get site name of the original document
            var originalSite = SiteInfoProvider.GetSiteInfo(node.OriginalNodeSiteID);

            bool translationsNotPresent = (th == null);
            // Translation table
            if (translationsNotPresent)
            {
                th = new TranslationHelper();
            }
            // Get document data
            var data = node.GetDataSet();
            data.Tables[0].TableName = TranslationHelper.GetSafeClassName(node.NodeClassName);

            if (translationsNotPresent)
            {
                // Add site translation by default
                th.RegisterRecord(node.Site);
                if (node.IsLink)
                {
                    th.RegisterRecord(originalSite);
                }
            }

            // Add additional data based on the task type
            bool includeBindings = ((taskType == TaskTypeEnum.CreateDocument) || (taskType == TaskTypeEnum.UpdateDocument) || (taskType == TaskTypeEnum.PublishDocument));
            bool includeBindingData = (((taskType == TaskTypeEnum.CreateDocument) || (taskType == TaskTypeEnum.UpdateDocument)) && (dataType == TaskDataTypeEnum.Snapshot));
            bool includeBindingTranslations = ((dataType == TaskDataTypeEnum.Snapshot) || (dataType == TaskDataTypeEnum.SimpleSnapshot));
            bool includeChildren = (dataType == TaskDataTypeEnum.Snapshot);

            switch (taskType)
            {
                // Document create and update
                case TaskTypeEnum.CreateDocument:
                case TaskTypeEnum.UpdateDocument:
                    {
                        #region "Child objects"

                        if (includeChildren)
                        {
                            // Get document attachments
                            DataSet attachmentsDS = AttachmentInfoProvider.GetAttachments(node.DocumentID, true);

                            if (!DataHelper.DataSourceIsEmpty(attachmentsDS))
                            {
                                DataTable attData = attachmentsDS.Tables[0];
                                attachmentsDS.Tables.Clear();
                                attData.TableName = "CMS_Attachment";

                                EnsureAttachmentsData(attData, originalSite.SiteName);

                                if (translationsNotPresent)
                                {
                                    EnsureRegisterRecordsEvents(attData, AttachmentInfo.OBJECT_TYPE, th);
                                }

                                data.Tables.Add(attData);
                            }

                            // Get relationships
                            DataSet relationshipsDS = RelationshipInfoProvider.GetRelationships(node.NodeID, true, true, null, 0, "LeftNodeID, LeftNodeGUID, RelationshipName, RelationshipNameID, RightNodeID, RightNodeGUID, RelationshipCustomData, RelationshipOrder");
                            if (!DataHelper.DataSourceIsEmpty(relationshipsDS))
                            {
                                DataTable relationShipsTable = relationshipsDS.Tables[0];

                                // Ensure translation records
                                foreach (DataRow dr in relationShipsTable.Rows)
                                {
                                    int leftNodeId = ValidationHelper.GetInteger(dr["LeftNodeID"], 0);
                                    int rightNodeId = ValidationHelper.GetInteger(dr["RightNodeID"], 0);
                                    Guid leftNodeGuid = ValidationHelper.GetGuid(dr["LeftNodeGUID"], Guid.Empty);
                                    Guid rightNodeGuid = ValidationHelper.GetGuid(dr["RightNodeGUID"], Guid.Empty);
                                    int nodeToRegister = leftNodeId;
                                    Guid guidToRegister = leftNodeGuid;
                                    if (nodeToRegister == node.NodeID)
                                    {
                                        nodeToRegister = rightNodeId;
                                        guidToRegister = rightNodeGuid;
                                    }
                                    if ((nodeToRegister != node.NodeID) && translationsNotPresent)
                                    {
                                        th.RegisterRecord(nodeToRegister, new TranslationParameters(DocumentNodeDataInfo.OBJECT_TYPE) { Guid = guidToRegister, SiteName = siteName });
                                    }
                                }

                                if (translationsNotPresent)
                                {
                                    EnsureRegisterRecordsEvents(relationShipsTable, RelationshipInfo.OBJECT_TYPE, th);
                                }

                                relationshipsDS.Tables.Clear();
                                relationShipsTable.TableName = "CMS_RelationShip";
                                data.Tables.Add(relationShipsTable);
                            }

                            // Get aliases
                            DocumentAliasInfo dai = new DocumentAliasInfo();
                            DataSet aliasesDS = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, dai, "AliasNodeID = " + node.NodeID, null, true, false, th);
                            if (!DataHelper.DataSourceIsEmpty(aliasesDS))
                            {
                                DataHelper.TransferTables(data, aliasesDS);
                            }

                            // Alternative urls
                            var alternativeUrls = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, new AlternativeUrlInfo(), $"AlternativeUrlDocumentID = {node.DocumentID}", null, true, false, th);
                            if (!DataHelper.DataSourceIsEmpty(alternativeUrls))
                            {
                                DataHelper.TransferTables(data, alternativeUrls);
                            }

                            // Get categories
                            DocumentCategoryInfo dci = new DocumentCategoryInfo();
                            DataSet categoriesDS = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, dci, "DocumentID = " + node.DocumentID, null, true, false, th);
                            if (!DataHelper.DataSourceIsEmpty(categoriesDS))
                            {
                                DataHelper.TransferTables(data, categoriesDS);
                            }

                            // Get message board
                            GeneralizedInfo board = ModuleManager.GetObject(PredefinedObjectType.BOARD);
                            DataSet boardsDS = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, board, "BoardDocumentID = " + node.DocumentID, null, true, false, th);
                            if (!DataHelper.DataSourceIsEmpty(boardsDS))
                            {
                                DataHelper.TransferTables(data, boardsDS);
                            }

                            // Get ACL
                            AclInfo aclInfo = new AclInfo();
                            DataSet aclDS = null;
                            DataTable aclItemsTable = null;

                            if (node.NodeIsACLOwner)
                            {
                                aclDS = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, aclInfo, "ACLID = " + node.NodeACLID, null, true, false, th);
                                aclItemsTable = aclDS.Tables["CMS_ACLItem"];
                            }

                            // Get MVT variants and combinations
                            GeneralizedInfo mvtVariant = ModuleManager.GetObject(PredefinedObjectType.DOCUMENTMVTVARIANT);
                            DataSet mvtVariantsDS = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, mvtVariant, "MVTVariantDocumentID = " + node.DocumentID, null, true, false, th);
                            if (!DataHelper.DataSourceIsEmpty(mvtVariantsDS))
                            {
                                DataHelper.TransferTables(data, mvtVariantsDS);
                            }
                            GeneralizedInfo mvtCombination = ModuleManager.GetObject(PredefinedObjectType.DOCUMENTMVTCOMBINATION);
                            DataSet mvtCombinationsDS = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, mvtCombination, "MVTCombinationDocumentID = " + node.DocumentID, null, true, false, th);
                            if (!DataHelper.DataSourceIsEmpty(mvtCombinationsDS))
                            {
                                DataHelper.TransferTables(data, mvtCombinationsDS);
                            }

                            // Get Content personalization variants
                            GeneralizedInfo cpVariant = ModuleManager.GetObject(PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT);
                            DataSet cpVariantsDS = SynchronizationHelper.GetObjectsData(OperationTypeEnum.Synchronization, cpVariant, "VariantDocumentID = " + node.DocumentID, null, true, false, th);
                            if (!DataHelper.DataSourceIsEmpty(cpVariantsDS))
                            {
                                DataHelper.TransferTables(data, cpVariantsDS);
                            }

                            // Ensure translation records
                            if (aclItemsTable != null)
                            {
                                foreach (DataRow dr in aclItemsTable.Rows)
                                {
                                    int userId = ValidationHelper.GetInteger(dr["UserID"], 0);
                                    int roleId = ValidationHelper.GetInteger(dr["RoleID"], 0);
                                    int lastModifiedByUserId = ValidationHelper.GetInteger(dr["LastModifiedByUserID"], 0);

                                    // Register userId record
                                    if ((userId > 0) && translationsNotPresent && !th.RecordExists(UserInfo.OBJECT_TYPE, userId))
                                    {
                                        var user = UserInfoProvider.GetUserInfo(userId);
                                        if (user != null)
                                        {
                                            th.RegisterRecord(user);
                                        }
                                    }

                                    // Register roleId record
                                    if ((roleId > 0) && translationsNotPresent && !th.RecordExists(RoleInfo.OBJECT_TYPE, roleId))
                                    {
                                        var role = RoleInfoProvider.GetRoleInfo(roleId);
                                        if (role != null)
                                        {
                                            th.RegisterRecord(role);
                                        }
                                    }

                                    // Register lastModifiedByUserId record
                                    if ((lastModifiedByUserId > 0) && translationsNotPresent && !th.RecordExists(UserInfo.OBJECT_TYPE, lastModifiedByUserId))
                                    {
                                        var user = UserInfoProvider.GetUserInfo(lastModifiedByUserId);
                                        if (user != null)
                                        {
                                            th.RegisterRecord(user);
                                        }
                                    }
                                }
                            }

                            if (!DataHelper.DataSourceIsEmpty(aclDS))
                            {
                                DataHelper.TransferTables(data, aclDS);
                            }
                        }

                        #endregion
                    }
                    break;

                // Document publishing
                case TaskTypeEnum.PublishDocument:
                    {
                        #region "Child objects"

                        if (includeChildren)
                        {
                            // Get version data
                            int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
                            if (versionHistoryId > 0)
                            {
                                // Get the version data
                                var versionHistory = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                                if (versionHistory != null)
                                {
                                    var version = new InfoDataSet<VersionHistoryInfo>(versionHistory);
                                    var versionTable = version.Tables[0];
                                    version.Tables.Clear();
                                    versionTable.TableName = "CMS_VersionHistory";

                                    // Clear the version data - present within document data
                                    versionTable.Rows[0]["NodeXML"] = DBNull.Value;

                                    if (translationsNotPresent)
                                    {
                                        EnsureRegisterRecordsEvents(versionTable, VersionHistoryInfo.OBJECT_TYPE, th);
                                    }

                                    data.Tables.Add(versionTable);
                                }

                                // Get the attachments
                                var versionAttachments = AttachmentHistoryInfoProvider.GetAttachmentHistories().AllInVersion(versionHistoryId).Result;
                                if (!DataHelper.DataSourceIsEmpty(versionAttachments))
                                {
                                    var attachmentsTable = versionAttachments.Tables[0];
                                    versionAttachments.Tables.Clear();
                                    attachmentsTable.TableName = "CMS_AttachmentHistory";

                                    EnsureAttachmentsHistoryData(attachmentsTable);

                                    // Add VersionHistoryID column
                                    attachmentsTable.Columns.Add(new DataColumn("VersionHistoryID", typeof(int), versionHistoryId.ToString()));

                                    if (translationsNotPresent)
                                    {
                                        EnsureRegisterRecordsEvents(attachmentsTable, AttachmentHistoryInfo.OBJECT_TYPE, th);
                                    }

                                    data.Tables.Add(attachmentsTable);
                                }
                            }
                            else
                            {
                                // Add published attachments
                                var attachmentsDS = AttachmentInfoProvider.GetAttachments(node.DocumentID, true);

                                if (!DataHelper.DataSourceIsEmpty(attachmentsDS))
                                {
                                    var attData = attachmentsDS.Tables[0];
                                    attachmentsDS.Tables.Clear();
                                    attData.TableName = "CMS_Attachment";

                                    EnsureAttachmentsData(attData, originalSite.SiteName);

                                    if (translationsNotPresent)
                                    {
                                        EnsureRegisterRecordsEvents(attData, AttachmentInfo.OBJECT_TYPE, th);
                                    }

                                    data.Tables.Add(attData);
                                }
                            }
                        }

                        #endregion
                    }
                    break;
            }

            // Register bindings
            if (includeBindings)
            {
                // Add information about workflow step
                var table = data.Tables[0];
                DataHelper.EnsureColumn(table, "StepType", typeof(int));
                table.Rows[0]["StepType"] = (int)node.WorkflowStepType;
            }

            if (includeBindings && (includeBindingData || includeBindingTranslations))
            {
                if (includeBindingTranslations && translationsNotPresent)
                {
                    // Register class translation
                    th.RegisterRecord(node.DataClassInfo);
                }

                // Register page template translation and data
                RegisterDocumentPageTemplate(th, data, translationsNotPresent, includeBindingData, includeBindingTranslations, node.NodeTemplateID);
                if (node.DocumentPageTemplateID != node.NodeTemplateID)
                {
                    RegisterDocumentPageTemplate(th, data, translationsNotPresent, includeBindingData, includeBindingTranslations, node.DocumentPageTemplateID);
                }                

                if (includeBindingTranslations && translationsNotPresent)
                {
                    // Register page stylesheet translation
                    if (node.DocumentStylesheetID > 0)
                    {
                        CssStylesheetInfo csi = CssStylesheetInfoProvider.GetCssStylesheetInfo(node.DocumentStylesheetID);
                        if (csi != null)
                        {
                            th.RegisterRecord(csi);
                        }
                    }

                    // Register user records
                    RegisterUserTranslation(node, th, "DocumentCreatedByUserID");
                    RegisterUserTranslation(node, th, "DocumentModifiedByUserID");
                    RegisterUserTranslation(node, th, "NodeOwner");

                    // Add parent node translation
                    var parentId = node.NodeParentID;
                    if (parentId > 0)
                    {
                        Guid parentNodeGuid = TreePathUtils.GetNodeGUIDByNodeId(parentId);
                        if (parentNodeGuid != Guid.Empty)
                        {
                            th.RegisterRecord(parentId, new TranslationParameters(DocumentNodeDataInfo.OBJECT_TYPE) { Guid = parentNodeGuid, SiteName = node.NodeSiteName });
                        }
                    }

                    // Register linked node translation
                    int linkedNodeId = ValidationHelper.GetInteger(node.GetValue("NodeLinkedNodeID"), 0);
                    if (linkedNodeId > 0)
                    {
                        Guid linkedNodeGuid = TreePathUtils.GetNodeGUIDByNodeId(linkedNodeId);
                        if (linkedNodeGuid != Guid.Empty)
                        {
                            // Node record
                            SiteInfo linkedSi = TreePathUtils.GetNodeSite(linkedNodeId);
                            th.RegisterRecord(linkedNodeId, new TranslationParameters(DocumentNodeDataInfo.OBJECT_TYPE) { Guid = linkedNodeGuid, SiteName = linkedSi.SiteName });

                            // Site record
                            th.RegisterRecord(linkedSi);
                        }
                    }

                    if (ColumnsTranslationEvents.RegisterRecords.IsBound)
                    {
                        ColumnsTranslationEvents.RegisterRecords.StartEvent(th, TreeNode.OBJECT_TYPE, node);
                    }
                }

                // SKU
                int skuId = ValidationHelper.GetInteger(node.GetValue("NodeSKUID"), 0);
                if (skuId > 0)
                {
                    // Get the object
                    var skuInfo = ProviderHelper.GetInfoById(PredefinedObjectType.SKU, skuId);
                    if (skuInfo != null)
                    {
                        // Add SKU data to the task
                        if (includeBindingData)
                        {
                            DataSet skuDS = SynchronizationHelper.GetObjectData(OperationTypeEnum.Synchronization, skuInfo, true, true, th);
                            DataHelper.TransferTables(data, skuDS);
                        }
                    }
                }

                // Group
                int groupId = ValidationHelper.GetInteger(node.GetValue("NodeGroupID"), 0);
                if (groupId > 0)
                {
                    // Get the object
                    var groupInfo = ModuleCommands.CommunityGetGroupInfo(groupId);
                    if (groupInfo != null)
                    {
                        if (includeBindingTranslations && translationsNotPresent)
                        {
                            th.RegisterRecord(groupInfo);
                        }
                        // Add group data to the task
                        if (includeBindingData)
                        {
                            DataSet groupDS = SynchronizationHelper.GetObjectData(OperationTypeEnum.Synchronization, groupInfo, true, true, th);
                            DataHelper.TransferTables(data, groupDS);
                        }
                    }
                }

                // Tag group
                int tagGroupId = ValidationHelper.GetInteger(node.GetValue("DocumentTagGroupID"), 0);
                if (tagGroupId > 0)
                {
                    // Get the object
                    var tagGroupInfo = TagGroupInfoProvider.GetTagGroupInfo(tagGroupId);
                    if (tagGroupInfo != null)
                    {
                        if (includeBindingTranslations && translationsNotPresent)
                        {
                            th.RegisterRecord(tagGroupInfo);
                        }
                        // Add tag group data to the task
                        if (includeBindingData)
                        {
                            DataSet tagGroupDS = SynchronizationHelper.GetObjectData(OperationTypeEnum.Synchronization, tagGroupInfo, true, true, th);
                            DataHelper.TransferTables(data, tagGroupDS);
                        }
                    }
                }
            }

            // Workflow step
            int stepId = ValidationHelper.GetInteger(node.GetValue("DocumentWorkflowStepID"), 0);
            if (stepId > 0)
            {
                // Get the object
                var wfStepInfo = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
                if (wfStepInfo != null)
                {
                    if (translationsNotPresent)
                    {
                        th.RegisterRecord(wfStepInfo);
                    }
                }
            }

            if (includeBindingTranslations)
            {
                // Add translation table
                data.Tables.Add(th.TranslationTable);
            }

            // Add extra Task parameters
            if (taskParams != null)
            {
                data.Tables.Add(taskParams.ParametersTable);
            }

            return data;
        }


        private static void EnsureRegisterRecordsEvents(DataTable dataTable, string objectType, TranslationHelper th)
        {
            if (ColumnsTranslationEvents.RegisterRecords.IsBound)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    ColumnsTranslationEvents.RegisterRecords.StartEvent(th, objectType, new DataRowContainer(row));
                }
            }
        }


        /// <summary>
        /// Ensures binary data in case that binary data are stored only on the filesystem. 
        /// Due to the fact that ObjectQuery doesn't support loading binary data, we have to ensure that manually.
        /// </summary>
        private static void EnsureAttachmentsHistoryData(DataTable attachmentsTable)
        {
            foreach (DataRow row in attachmentsTable.Rows)
            {
                if (DataHelper.IsEmpty(row[ATTACHMENT_BINARY_COLUMN_NAME]))
                {
                    row[ATTACHMENT_BINARY_COLUMN_NAME] = new AttachmentHistoryInfo(row).AttachmentBinary;
                }
            }
        }


        private static void EnsureAttachmentsData(DataTable attachmentsTable, string siteName)
        {
            foreach (DataRow row in attachmentsTable.Rows)
            {
                Guid attachmentGuid = ValidationHelper.GetGuid(row["AttachmentGUID"], Guid.Empty);

                if (DataHelper.IsEmpty(row[ATTACHMENT_BINARY_COLUMN_NAME]))
                {
                    row[ATTACHMENT_BINARY_COLUMN_NAME] = AttachmentBinaryHelper.GetAttachmentBinary(attachmentGuid, siteName);
                }
            }
        }


        /// <summary>
        /// Includes document page template into the logged data
        /// </summary>
        /// <param name="th">Translation helper</param>
        /// <param name="ds">DataSet with the result</param>
        /// <param name="translationsNotPresent">Flag whether translations are already present or not</param>
        /// <param name="includeBindingData">Flag indicating whether binding data should be included</param>
        /// <param name="includeBindingTranslations">Flag indicating whether binding translations should be included</param>
        /// <param name="templateId">Template ID to include to export data</param>
        private static void RegisterDocumentPageTemplate(TranslationHelper th, DataSet ds, bool translationsNotPresent, bool includeBindingData, bool includeBindingTranslations, int templateId)
        {
            if (templateId > 0)
            {
                PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
                if (pti != null)
                {
                    if (includeBindingTranslations && translationsNotPresent)
                    {
                        // Register ID translation info 
                        th.RegisterRecord(pti);
                    }
                    if ((SynchronizeSharedTemplatesWithDocuments || !pti.IsReusable) && includeBindingData)
                    {
                        // Add page template data to the task
                        DataSet templateDS = SynchronizationHelper.GetObjectData(OperationTypeEnum.Synchronization, pti, true, true, th);
                        DataHelper.TransferTables(ds, templateDS);
                    }
                }
            }
        }


        /// <summary>
        /// Registers the user translation based on the given column
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="th">Translation helper</param>
        /// <param name="columnName">Column to register</param>
        private static void RegisterUserTranslation(TreeNode node, TranslationHelper th, string columnName)
        {
            int userId = ValidationHelper.GetInteger(node.GetValue(columnName), 0);
            if (userId > 0)
            {
                var user = UserInfoProvider.GetUserInfo(userId);
                if (user != null)
                {
                    th.RegisterRecord(user);
                }
            }
        }

        #endregion


        #region "Synchronization methods"


        #region "Full methods"

        /// <summary>
        /// Creates the synchronization task for the specified document node and specified server with extra task parameters.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="taskType">Task type</param>
        /// <param name="logStaging">Indicates if the staging task should be logged</param>
        /// <param name="logIntegration">Indicates if the integration task should be logged</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="serverId">Server ID to use for synchronization</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <returns>List of synchronization tasks</returns>
        public static List<ISynchronizationTask> LogDocumentChange(TreeNode node, TaskTypeEnum taskType, bool logStaging, bool logIntegration, TreeProvider tree, int serverId, TaskParameters taskParams, bool runAsync)
        {
            var settings = new LogDocumentChangeSettings
            {
                Node = node,
                TaskType = taskType,
                LogStaging = logStaging,
                LogIntegration = logIntegration,
                Tree = tree,
                ServerID = serverId,
                TaskParameters = taskParams,
                RunAsynchronously = runAsync
            };

            return LogDocumentChange(settings);
        }


        /// <summary>
        /// Creates the synchronization task for the specified document node and specified server with extra task parameters.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="taskType">Task type</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="serverId">Server ID to use for synchronization</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <returns>Returns new synchronization task</returns>
        public static List<ISynchronizationTask> LogDocumentChange(TreeNode node, TaskTypeEnum taskType, TreeProvider tree, int serverId, TaskParameters taskParams, bool runAsync)
        {
            var settings = new LogDocumentChangeSettings
            {
                Node = node,
                TaskType = taskType,
                LogStaging = true,
                LogIntegration = true,
                Tree = tree,
                ServerID = serverId,
                TaskParameters = taskParams,
                RunAsynchronously = runAsync
            };

            return LogDocumentChange(settings);
        }


        /// <summary>
        /// Logs the synchronization after the node order change (logs all document nodes on the save level as the document).
        /// </summary>
        /// <param name="siteName">Documents site name</param>
        /// <param name="aliasPath">Starting alias path</param>
        /// <param name="logStaging">Indicates if the staging task should be logged</param>
        /// <param name="logIntegration">Indicates if the integration task should be logged</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="serverId">Server ID to use for synchronization</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        public static void LogDocumentChangeOrder(string siteName, string aliasPath, bool logStaging, bool logIntegration, TreeProvider tree, int serverId, bool runAsync)
        {
            var settings = new LogMultipleDocumentChangeSettings
            {
                SiteName = siteName,
                NodeAliasPath = aliasPath,
                LogStaging = logStaging,
                LogIntegration = logIntegration,
                Tree = tree,
                ServerID = serverId,
                RunAsynchronously = runAsync,
            };

            LogDocumentChangeOrder(settings);
        }


        /// <summary>
        /// Creates the synchronization tasks for the specified document tree.
        /// </summary>
        /// <param name="siteName">Documents site name</param>
        /// <param name="aliasPath">Starting alias path</param>
        /// <param name="taskType">Task type</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="keepTaskData">Indicates if task data should be kept in the objects</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <returns>Returns list of the ISynchronizationTasks created</returns>
        public static IEnumerable<ISynchronizationTask> LogDocumentChange(string siteName, string aliasPath, TaskTypeEnum taskType, TreeProvider tree, int serverId, bool keepTaskData, bool runAsync)
        {
            return LogDocumentChange(siteName, aliasPath, taskType, true, true, tree, serverId, keepTaskData, null, runAsync, null);
        }


        /// <summary>
        /// Creates the synchronization tasks for the specified document tree.
        /// </summary>
        /// <param name="siteName">Documents site name</param>
        /// <param name="aliasPath">Starting alias path</param>
        /// <param name="taskType">Task type</param>
        /// <param name="logStaging">Indicates if the staging task should be logged</param>
        /// <param name="logIntegration">Indicates if the integration task should be logged</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="serverId">Server ID to use for synchronization</param>
        /// <param name="keepTaskData">Indicates if task data should be kept in the objects</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <param name="where">Where condition</param>
        /// <returns>Returns collection of the tasks created</returns>
        public static IEnumerable<ISynchronizationTask> LogDocumentChange(string siteName, string aliasPath, TaskTypeEnum taskType, bool logStaging, bool logIntegration, TreeProvider tree, int serverId, bool keepTaskData, TaskParameters taskParams, bool runAsync, string where)
        {
            var settings = new LogMultipleDocumentChangeSettings
            {
                SiteName = siteName,
                NodeAliasPath = aliasPath,
                TaskType = taskType,
                LogStaging = logStaging,
                LogIntegration = logIntegration,
                Tree = tree,
                ServerID = serverId,
                KeepTaskData = keepTaskData,
                TaskParameters = taskParams,
                RunAsynchronously = runAsync,
                WhereCondition = where,
            };

            return LogDocumentChange(settings);
        }

        #endregion


        #region "Overloads"

        /// <summary>
        /// Logs the synchronization after the node order change (logs all document nodes on the same level as the document).
        /// </summary>
        /// <param name="siteName">Documents site name</param>
        /// <param name="aliasPath">Starting alias path</param>
        /// <param name="tree">Tree provider</param>
        public static void LogDocumentChangeOrder(string siteName, string aliasPath, TreeProvider tree)
        {
            LogDocumentChangeOrder(siteName, aliasPath, true, true, tree, SynchronizationInfoProvider.ENABLED_SERVERS, true);
        }


        /// <summary>
        /// Creates the synchronization task for the specified document node, creates the tasks for all the enabled servers when task logging is on.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="taskType">Task type</param>
        /// <param name="tree">Tree provider</param>
        public static void LogDocumentChange(TreeNode node, TaskTypeEnum taskType, TreeProvider tree)
        {
            LogDocumentChange(node, taskType, tree, SynchronizationInfoProvider.ENABLED_SERVERS, null, true);
        }


        /// <summary>
        /// Creates the synchronization tasks for the specified document tree.
        /// </summary>
        /// <param name="siteName">Documents site name</param>
        /// <param name="aliasPath">Starting alias path</param>
        /// <param name="taskType">Task type</param>
        /// <param name="tree">Tree provider</param>
        /// <returns>Returns list of the ISynchronizationTasks created</returns>
        public static IEnumerable<ISynchronizationTask> LogDocumentChange(string siteName, string aliasPath, TaskTypeEnum taskType, TreeProvider tree)
        {
            // Allow asynchronous operations
            bool runAsync = true;
            if (tree != null)
            {
                runAsync = tree.AllowAsyncActions;
            }
            return LogDocumentChange(siteName, aliasPath, taskType, tree, SynchronizationInfoProvider.ENABLED_SERVERS, false, runAsync);
        }

        #endregion


        /// <summary>
        /// Logs the synchronization after the node order change (logs all document nodes on the save level as the document).
        /// </summary>
        /// <param name="settings">Log document change settings</param>
        public static IList<ISynchronizationTask> LogDocumentChangeOrder(LogMultipleDocumentChangeSettings settings)
        {
            IList<ISynchronizationTask> result = new List<ISynchronizationTask>();
            string siteName = settings.SiteName;

            // Check site name
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[DocumentSynchronizationHelper.LogChangeOrderSynchronization]: Site not found.");
            }

            TreeProvider tree = settings.Tree;

            // Method is not called from worker
            if (!settings.WorkerCall)
            {
                // Check if asynchronous run
                settings.RunAsynchronously &= (CMSActionContext.CurrentAllowAsyncActions && tree.AllowAsyncActions);

                settings.LogStaging &= CheckStagingLogging(siteName, si.SiteID, settings.ServerID);
                settings.LogIntegration &= CheckIntegrationLogging();
            }


            // Log only if synchronization enabled
            if (settings.LogStaging || settings.LogIntegration)
            {
                settings.InitUserAndTaskGroups();

                if (settings.RunAsynchronously)
                {
                    // Run synchronization for order change
                    SynchronizationQueueWorker.Current.Enqueue(() =>
                    {
                        settings.RunAsynchronously = false;
                        settings.WorkerCall = true;
                        LogDocumentChangeOrder(settings);
                    });
                }
                else
                {
                    string parentPath = TreePathUtils.GetParentPath(settings.NodeAliasPath);

                    // Get all documents on the same level
                    DataSet ds = tree.SelectNodes(siteName, parentPath.TrimEnd('/') + "/%", TreeProvider.ALL_CULTURES, true, null, "NodeAliasPath <> '" + SqlHelper.EscapeQuotes(parentPath) + "'", "NodeLevel ASC", 1, false, 0, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS);

                    // Check if data source is not empty
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Go through all nodes
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            // Update child nodes
                            int logNodeId = ValidationHelper.GetInteger(dr["NodeID"], 0);
                            string culture = ValidationHelper.GetString(dr["DocumentCulture"], "");
                            string className = ValidationHelper.GetString(dr["ClassName"], "");

                            // Get the node
                            TreeNode node = tree.SelectSingleNode(logNodeId, culture, className);

                            // Create synchronization task
                            var singleSettings = new LogDocumentChangeSettings
                            {
                                LogStaging = settings.LogStaging,
                                LogIntegration = settings.LogIntegration,
                                Tree = tree,
                                ServerID = settings.ServerID,
                                Node = node,
                                TaskType = TaskTypeEnum.UpdateDocument,
                                RunAsynchronously = false,
                                EnsurePublishTask = settings.EnsurePublishTask
                            };

                            using (CMSTransactionScope cts = new CMSTransactionScope())
                            {
                                var loggedTasks = LogDocumentChange(singleSettings);
                                SynchronizationHelper.LogTasksWithUserAndTaskGroups(loggedTasks, settings.TaskGroups, settings.User);

                                cts.Commit();
                            }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Creates the synchronization tasks for the specified document tree.
        /// </summary>
        /// <param name="settings">Log multiple document change settings</param>
        /// <returns>Returns collection of the tasks created</returns>
        public static ICollection<ISynchronizationTask> LogDocumentChange(LogMultipleDocumentChangeSettings settings)
        {
            string siteName = settings.SiteName;

            // Check site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[DocumentSynchronizationHelper.LogSynchronization]: Site not found.");
            }

            TreeProvider tree = settings.Tree;
            TaskTypeEnum taskType = settings.TaskType;

            // Method is not called from worker
            if (!settings.WorkerCall)
            {
                // Check if asynchronous run
                settings.RunAsynchronously &= (CMSActionContext.CurrentAllowAsyncActions && !TaskHelper.IsExcludedAsyncTask(taskType) && tree.AllowAsyncActions);

                settings.LogStaging &= CheckStagingLogging(siteName, si.SiteID, settings.ServerID);
                settings.LogIntegration &= CheckIntegrationLogging();
            }


            // Log only if synchronization enabled
            if (settings.LogStaging || settings.LogIntegration)
            {
                settings.InitUserAndTaskGroups();

                if (settings.RunAsynchronously)
                {
                    // Run synchronization of multiple documents
                    SynchronizationQueueWorker.Current.Enqueue(() =>
                    {
                        settings.RunAsynchronously = false;
                        settings.WorkerCall = true;
                        LogDocumentChange(settings);
                    });
                }
                else
                {
                    List<ISynchronizationTask> tasks = new List<ISynchronizationTask>();

                    // Get the nodes
                    DataSet ds = tree.SelectNodes(siteName, settings.NodeAliasPath, settings.CultureCode, false, null, settings.WhereCondition, "NodeLevel ASC", TreeProvider.ALL_LEVELS, false);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Log the nodes tasks
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            string nodeAliasPath = (string)dr["NodeAliasPath"];
                            string culture = (string)dr["DocumentCulture"];
                            string className = (string)dr["ClassName"];

                            // Get the node
                            TreeNode node = tree.SelectSingleNode(siteName, nodeAliasPath, culture, false, className, false);

                            // Check if node exists
                            if (node != null)
                            {
                                // Create synchronization task
                                var singleSettings = new LogDocumentChangeSettings
                                {
                                    TaskType = taskType,
                                    LogStaging = settings.LogStaging,
                                    LogIntegration = settings.LogIntegration,
                                    Tree = tree,
                                    ServerID = settings.ServerID,
                                    TaskParameters = settings.TaskParameters,
                                    Node = node,
                                    RunAsynchronously = false,
                                    EnsurePublishTask = settings.EnsurePublishTask
                                };

                                List<ISynchronizationTask> newTasks;
                                using (CMSTransactionScope cts = new CMSTransactionScope())
                                {
                                    newTasks = LogDocumentChange(singleSettings);
                                    SynchronizationHelper.LogTasksWithUserAndTaskGroups(newTasks, settings.TaskGroups, settings.User);

                                    cts.Commit();
                                }

                                if (newTasks != null)
                                {
                                    // Clear the task data to free memory
                                    if (!settings.KeepTaskData)
                                    {
                                        foreach (ISynchronizationTask task in newTasks)
                                        {
                                            task.TaskData = null;
                                        }
                                    }

                                    tasks = tasks.Union(newTasks).ToList();
                                }
                            }
                        }
                    }

                    // Return the tasks
                    return tasks;
                }
            }

            return null;
        }


        /// <summary>
        /// Creates the synchronization task for the specified document node and specified server with extra task parameters.
        /// </summary>
        /// <param name="settings">Log document change settings</param>
        /// <returns>List of synchronization tasks</returns>
        public static List<ISynchronizationTask> LogDocumentChange(LogDocumentChangeSettings settings)
        {
            // Check node instance
            if (settings.Node == null)
            {
                throw new Exception("[SynchronizationHelper.LogDocumentChange]: Missing node instance.");
            }

            List<ISynchronizationTask> tasks = null;

            // Handle the event
            using (var h = DocumentEvents.LogChange.StartEvent(settings))
            {
                if (h.CanContinue())
                {
                    TreeProvider tree = settings.Tree;
                    TaskTypeEnum taskType = settings.TaskType;
                    TaskParameters taskParams = settings.TaskParameters;
                    int serverId = settings.ServerID;

                    // Do not log any actions if not necessary
                    if (CMSActionContext.LogDocumentChange())
                    {
                        // Initialize properties - reflect context for synchronous call
                        int siteId = settings.Node.NodeSiteID;
                        string siteName = settings.Node.NodeSiteName;

                        // Method is not called from worker
                        if (!settings.WorkerCall)
                        {
                            // Check if asynchronous run
                            settings.RunAsynchronously &= (CMSActionContext.CurrentAllowAsyncActions && !TaskHelper.IsExcludedAsyncTask(taskType) && tree.AllowAsyncActions);

                            settings.LogStaging &= CheckStagingLogging(siteName, siteId, serverId);
                            settings.LogIntegration &= CheckIntegrationLogging();

                            if (settings.LogIntegration)
                            {
                                // Get matching connectors
                                Dictionary<string, TaskProcessTypeEnum> connectors = IntegrationHelper.GetMatchingConnectors(settings.Node, taskType);
                                // Touch asynchronous connectors
                                settings.LogIntegration = IntegrationHelper.TouchAsyncConnectors(connectors);

                                // Process sync connectors
                                ProcessSyncTasks(settings.Node, taskType, siteId, connectors);
                            }
                        }

                        // Check if the document change should be logged
                        if (settings.LogStaging || settings.LogIntegration)
                        {
                            // Logging will be processed, create clone to ensure fresh data
                            if (!settings.WorkerCall)
                            {
                                var clone = settings.Node.Clone();

                                DocumentSynchronizationEvents.LogDocumentChangeClone.StartEvent(settings.Node, clone);

                                settings.Node = clone;
                            }

                            settings.InitUserAndTaskGroups();

                            if (settings.RunAsynchronously)
                            {
                                // Run synchronization of single document
                                SynchronizationQueueWorker.Current.Enqueue(() =>
                                {
                                    settings.RunAsynchronously = false;
                                    settings.WorkerCall = true;
                                    LogDocumentChange(settings);
                                });
                            }
                            else
                            {
                                // Delete request cache
                                string storageKey = "documentdata|" + settings.Node.NodeClassName + "|" + settings.Node.DocumentID;
                                RequestStockHelper.DropStorage(storageKey, false);

                                var stagTasks = new List<ISynchronizationTask>();
                                var intBusTasks = new List<ISynchronizationTask>();

                                // Log synchronization
                                if (settings.LogStaging)
                                {
                                    StagingTaskInfo task = LogSynchronization(settings.Node, taskType, tree, serverId, taskParams);
                                    stagTasks.Add(task);
                                }

                                // Log integration
                                if (settings.LogIntegration)
                                {
                                    List<IntegrationTaskInfo> intTasks = LogIntegration(settings.Node, taskType, tree, taskParams, siteId);
                                    intBusTasks.AddRange(intTasks);
                                }

                                if (settings.EnsurePublishTask && (taskType == TaskTypeEnum.UpdateDocument))
                                {
                                    // If EnsurePublishTask, then for documents in Publish step when UpdateDocument is called, generate also the PublishDocument task
                                    // This is because of manual synchronization via Staging -> Synchronize current document (subtree)
                                    if ((settings.Node != null) && (settings.Node.DocumentWorkflowStepID > 0))
                                    {
                                        var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(settings.Node.DocumentWorkflowStepID);
                                        if ((step != null) && (step.StepIsPublished))
                                        {
                                            if (settings.LogIntegration)
                                            {
                                                var publishTasks = LogIntegration(settings.Node, TaskTypeEnum.PublishDocument, tree, taskParams, siteId);
                                                intBusTasks.AddRange(publishTasks);
                                            }

                                            if (settings.LogStaging)
                                            {
                                                var publishTask = LogSynchronization(settings.Node, TaskTypeEnum.PublishDocument, tree, serverId, taskParams);
                                                stagTasks.Add(publishTask);
                                            }
                                        }
                                    }
                                }

                                using (CMSTransactionScope cts = new CMSTransactionScope())
                                {
                                    SynchronizationHelper.LogTasksWithUserAndTaskGroups(stagTasks, settings.TaskGroups, settings.User);
                                    tasks = stagTasks.Concat(intBusTasks).ToList();

                                    cts.Commit();
                                }
                            }
                        }
                    }
                }

                // Finish the event
                h.FinishEvent();
            }

            return tasks;
        }

        #endregion


        #region "Internal staging methods"

        /// <summary>
        /// Creates the synchronization task for the specified document node and specified server with extra task parameters.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="taskType">Task type</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="serverId">Server ID to use for synchronization</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <returns>Returns new synchronization task</returns>
        internal static StagingTaskInfo LogSynchronization(TreeNode node, TaskTypeEnum taskType, TreeProvider tree, int serverId, TaskParameters taskParams)
        {
            // Get node site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
            if (si == null)
            {
                throw new Exception("[DocumentSynchronizationHelper.LogSynchronization]: Node site not found.");
            }

            // Log only if synchronization enabled
            if (CheckStagingLogging(si.SiteName, si.SiteID, serverId))
            {
                try
                {
                    // Lock on the document instance to ensure only single running logging for the document
                    lock (node.Generalized.GetLockObject())
                    {
                        // Prepare task title
                        string taskTitle = GetTaskTitle(taskType, node);

                        // Create synchronization task
                        StagingTaskInfo ti = new StagingTaskInfo
                        {
                            TaskData = GetDocumentXML(node, null, tree, taskType, TaskDataTypeEnum.Snapshot, taskParams, si.SiteName),
                            TaskDocumentID = node.DocumentID,
                            TaskNodeID = node.NodeID,
                            TaskNodeAliasPath = node.NodeAliasPath,
                            TaskTime = DateTime.Now,
                            TaskTitle = taskTitle,
                            TaskSiteID = si.SiteID,
                            TaskType = taskType,
                        };

                        StagingTaskInfoProvider.UpdateTaskServers(ti);

                        using (var h = StagingEvents.LogTask.StartEvent(ti, node))
                        {
                            if (h.CanContinue())
                            {
                                var serverIds = StagingTaskInfoProvider.GetServerIdsToLogTaskTo(ti, ti.TaskSiteID, serverId);
                                if (serverIds.Count > 0)
                                {
                                    // Log task preparation
                                    var message = String.Format(ResHelper.GetAPIString("synchronization.preparing", "Preparing '{0}' task"), HTMLHelper.HTMLEncode(taskTitle));
                                    LogContext.AppendLine(message, StagingTaskInfoProvider.LOGCONTEXT_SYNCHRONIZATION);

                                    // Save within transaction to keep multithreaded consistency in DB
                                    using (var tr = new CMSTransactionScope())
                                    {
                                        StagingTaskInfoProvider.SetTaskInfo(ti);
                                        SynchronizationInfoProvider.CreateSynchronizationRecords(ti.TaskID, serverIds);
                                        StagingTaskInfoProvider.DeleteOlderTasks(ti);

                                        // Commit the transaction
                                        tr.Commit();
                                    }
                                }
                            }

                            h.FinishEvent();
                        }

                        return ti;
                    }
                }
                catch (ThreadAbortException abortEx)
                {
                    if (!CMSThread.Stopped(abortEx))
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    EventLogProvider.LogException("Staging", "LogDocument", ex);
                }
            }
            return null;
        }

        #endregion


        #region "Integration methods"

        /// <summary>
        /// Processes synchronous task subscriptions.
        /// </summary>
        /// <param name="node">Document to match</param>
        /// <param name="siteId">Site identifier</param>
        /// <param name="taskType">Type of task to match</param>
        /// <param name="connectors">Connectors to process</param>
        public static void ProcessSyncTasks(TreeNode node, TaskTypeEnum taskType, int siteId, Dictionary<string, TaskProcessTypeEnum> connectors)
        {
            // Get connectors for synchronous processing
            IEnumerable<string> syncSnapshotConnectors = IntegrationHelper.FilterConnectors(connectors, TaskProcessTypeEnum.SyncSnapshot);

            // Synchronously process the task
            foreach (string connectorName in syncSnapshotConnectors)
            {
                AbstractIntegrationConnector connector = IntegrationHelper.GetConnector(connectorName);
                string errorMessage = null;
                IntegrationProcessResultEnum result;
                try
                {
                    string siteName = SiteInfoProvider.GetSiteName(siteId);

                    // Process the task
                    result = connector.ProcessInternalTaskSync(node, taskType, siteName, out errorMessage);
                }
                catch
                {
                    result = IntegrationProcessResultEnum.Error;
                }
                if ((result == IntegrationProcessResultEnum.Error) || (result == IntegrationProcessResultEnum.ErrorAndSkip))
                {
                    // Prepare error message
                    errorMessage = "[DocumentSynchronizationHelper.ProcessSyncTasks]: Connector '" + connector.ConnectorName +
                                   "' failed to synchronously process task '" + TaskHelper.GetTaskTypeString(taskType) + "' for page identified as '" + node.NodeAliasPath + "'. Original message: " + errorMessage;
                }

                if ((result == IntegrationProcessResultEnum.Error) || (result == IntegrationProcessResultEnum.ErrorAndSkip))
                {
                    // Log error
                    EventLogProvider.LogEvent(EventType.ERROR, "Integration", "PROCESSINTERNALTASKSYNC", errorMessage);
                    switch (result)
                    {
                        // End processing
                        case IntegrationProcessResultEnum.Error:
                            return;

                        // Continue processing task for other connectors
                        case IntegrationProcessResultEnum.ErrorAndSkip:
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Logs integration task.
        /// </summary>
        /// <param name="node">Document to log</param>
        /// <param name="taskType">Type of task to log</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <param name="siteId">Site identifier</param>
        public static List<IntegrationTaskInfo> LogIntegration(TreeNode node, TaskTypeEnum taskType, TreeProvider tree, TaskParameters taskParams, int siteId)
        {
            var tasks = new List<IntegrationTaskInfo>();
            if (IntegrationHelper.IntegrationLogInternal)
            {
                // Get matching connectors
                var connectors = IntegrationHelper.GetMatchingConnectors(node, taskType);
                IntegrationHelper.TouchAsyncConnectors(connectors);

                // Asynchronously log integration bus synchronization for simple objects
                var asyncSimpleConnectors = IntegrationHelper.FilterConnectors(connectors, TaskProcessTypeEnum.AsyncSimple);
                var simpleTask = LogInternalIntegration(node, tree, taskType, taskParams, TaskProcessTypeEnum.AsyncSimple, asyncSimpleConnectors.ToList());
                if (simpleTask != null)
                {
                    tasks.Add(simpleTask);
                }

                // Log simple snapshots
                var asyncSimpleSnapshotConnectors = IntegrationHelper.FilterConnectors(connectors, TaskProcessTypeEnum.AsyncSimpleSnapshot);
                var simpleSnapshotTask = LogInternalIntegration(node, tree, taskType, taskParams, TaskProcessTypeEnum.AsyncSimpleSnapshot, asyncSimpleSnapshotConnectors.ToList());
                if (simpleSnapshotTask != null)
                {
                    tasks.Add(simpleSnapshotTask);
                }

                // Log snapshot tasks
                var asyncSnapshotConnectors = IntegrationHelper.FilterConnectors(connectors, TaskProcessTypeEnum.AsyncSnapshot);
                var snapshotTask = LogInternalIntegration(node, tree, taskType, taskParams, TaskProcessTypeEnum.AsyncSnapshot, asyncSnapshotConnectors.ToList());
                if (snapshotTask != null)
                {
                    tasks.Add(snapshotTask);
                }
            }

            return tasks;
        }


        /// <summary>
        /// Logs the integration task for the given object.
        /// </summary>
        /// <param name="node">Document to log</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="taskType">Task type</param>
        /// <param name="taskParams">Extra task parameters to use</param>
        /// <param name="taskProcessType">Processing type</param>
        /// <param name="connectorNames">Integration connector names for which to log the synchronization (nothing is logged when no connector is specified)</param>
        /// <returns>Returns new integration task</returns>
        public static IntegrationTaskInfo LogInternalIntegration(TreeNode node, TreeProvider tree, TaskTypeEnum taskType, TaskParameters taskParams, TaskProcessTypeEnum taskProcessType, List<string> connectorNames)
        {
            if ((connectorNames == null) || (connectorNames.Count == 0) || !IntegrationHelper.IntegrationLogInternal)
            {
                return null;
            }
            try
            {
                // Lock on the document instance to ensure only single running logging for the document
                lock (node.Generalized.GetLockObject())
                {
                    var dataType = IntegrationHelper.GetTaskDataTypeEnum(taskProcessType);
                    var si = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);

                    // Create integration task
                    var ti = new IntegrationTaskInfo
                    {
                        TaskData = GetDocumentXML(node, null, tree, taskType, dataType, taskParams, si.SiteName),
                        TaskDocumentID = node.DocumentID,
                        TaskNodeID = node.NodeID,
                        TaskNodeAliasPath = node.NodeAliasPath,
                        TaskTime = DateTime.Now,
                        TaskTitle = GetTaskTitle(taskType, node),
                        TaskSiteID = si.SiteID,
                        TaskIsInbound = false,
                        TaskType = taskType,
                        TaskDataType = dataType,
                    };

                    using (var h = IntegrationEvents.LogInternalTask.StartEvent(ti, node))
                    {
                        if (h.CanContinue())
                        {
                            // Save within transaction to keep multithreaded consistency in DB
                            using (var tr = new CMSTransactionScope())
                            {
                                // Save the task
                                IntegrationTaskInfoProvider.SetIntegrationTaskInfo(ti);

                                // Create the synchronization record for each connector
                                foreach (string connectorName in connectorNames)
                                {
                                    IntegrationTaskInfoProvider.CreateSynchronization(connectorName, ti);
                                }

                                // Commit the transaction
                                tr.Commit();
                            }
                        }

                        h.FinishEvent();
                    }

                    return ti;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("Integration", "LogDocument", ex);
            }

            return null;
        }


        /// <summary>
        /// Logs the integration task for the given object.
        /// </summary>
        /// <param name="node">Document to log</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="taskType">Task type</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="result">What to do when processing fails</param>
        /// <param name="connectorName">Integration connector names for which to log the synchronization (nothing is logged when no connector is specified)</param>
        /// <param name="th">Translation helper</param>
        /// <param name="siteName">Site name of the target site (for site objects)</param>
        /// <returns>Returns new integration task</returns>
        public static IntegrationTaskInfo LogExternalIntegration(TreeNode node, TreeProvider tree, TaskTypeEnum taskType, TaskDataTypeEnum dataType, IntegrationProcessTypeEnum result, string connectorName, TranslationHelper th, string siteName)
        {
            // Check object instance
            if (node == null)
            {
                throw new Exception("[DocumentSynchronizationHelper.LogExternalIntegration]: Missing object instance.");
            }
            else if ((IntegrationHelper.GetConnector(connectorName) == null) || !IntegrationHelper.IntegrationLogExternal)
            {
                return null;
            }
            try
            {
                if (tree == null)
                {
                    tree = new TreeProvider();
                }

                // Lock on the document instance to ensure only single running logging for the document
                lock (node.Generalized.GetLockObject())
                {
                    var si = SiteInfoProvider.GetSiteInfo(siteName);

                    // Create integration task
                    var ti = new IntegrationTaskInfo
                    {
                        TaskData = GetDocumentXML(node, th, tree, taskType, dataType, null, siteName),
                        TaskDocumentID = node.DocumentID,
                        TaskNodeID = node.NodeID,
                        TaskNodeAliasPath = node.NodeAliasPath,
                        TaskTime = DateTime.Now,
                        TaskTitle = GetTaskTitle(taskType,
                                                node),
                        TaskSiteID = si.SiteID,
                        TaskIsInbound = true,
                        TaskProcessType = result,
                        TaskType = taskType,
                        TaskDataType = TaskDataTypeEnum.Simple
                    };

                    using (var h = IntegrationEvents.LogExternalTask.StartEvent(ti, node))
                    {
                        if (h.CanContinue())
                        {
                            // Save within transaction to keep multithreaded consistency in DB
                            using (var tr = new CMSTransactionScope())
                            {
                                // Save the task
                                IntegrationTaskInfoProvider.SetIntegrationTaskInfo(ti);

                                // Create the synchronization record
                                IntegrationTaskInfoProvider.CreateSynchronization(connectorName, ti);

                                // Commit the transaction
                                tr.Commit();
                            }
                        }

                        h.FinishEvent();
                    }

                    return ti;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("Integration", "LogDocument", ex);
            }
            return null;
        }

        #endregion
    }
}