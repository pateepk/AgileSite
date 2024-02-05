using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Search;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class to provide node versioning management.
    /// </summary>
    public class VersionManager : AbstractManager
    {
        #region "Variables"

        // TreeProvider to use for DB access.
        private TreeProvider mTreeProvider;

        // WorkflowManager for workflow managing.
        private WorkflowManager mWorkflowManager;

        #endregion


        #region "Properties"

        /// <summary>
        /// TreeProvider instance.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider());
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Gets workflow manager instance.
        /// </summary>
        public virtual WorkflowManager WorkflowManager
        {
            get
            {
                return mWorkflowManager ?? (mWorkflowManager = WorkflowManager.GetInstance(TreeProvider));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates version manager.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. For inheritance, use constructor VersionManager(tree)")]
        public VersionManager()
        {
        }


        /// <summary>
        /// Constructor - Creates version manager.
        /// </summary>
        /// <param name="tree">TreeProvider instance</param>
        protected VersionManager(TreeProvider tree)
        {
            mTreeProvider = tree;
        }


        /// <summary>
        /// Changes the manager type to the given type
        /// </summary>
        /// <param name="newType">New manager type</param>
        public override void ChangeManagerTypeTo(Type newType)
        {
            ChangeManagerType<VersionManager>(newType);
        }


        /// <summary>
        /// Gets the instance of the manager.
        /// </summary>
        /// <param name="tree">TreeProvider instance to use</param>
        public static VersionManager GetInstance(TreeProvider tree)
        {
            var vm = LoadManager<VersionManager>();
            vm.TreeProvider = tree;

            return vm;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns specified history version.
        /// </summary>
        /// <param name="versionHistoryId">VersionHistory ID</param>
        /// <param name="sourceNode">Document node</param>
        public TreeNode GetVersion(int versionHistoryId, TreeNode sourceNode = null)
        {
            // Check if version provided
            if (versionHistoryId == 0)
            {
                return null;
            }

            var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
            return GetVersion(version, sourceNode);
        }


        /// <summary>
        /// Returns node with applied version data.
        /// </summary>
        /// <param name="version">Document version history</param>
        /// <param name="sourceNode">Document node</param>
        public TreeNode GetVersion(VersionHistoryInfo version, TreeNode sourceNode)
        {
            return GetVersionInternal(version, sourceNode);
        }


        /// <summary>
        /// Applies version data to the node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="version">Document version</param>
        /// <param name="excludedColumns">Set of columns which should not be applied</param>
        public void ApplyVersion(TreeNode node, VersionHistoryInfo version, IEnumerable<string> excludedColumns = null)
        {
            // Get the excluded columns set
            var excluded = GetColumnsSet(excludedColumns);

            ApplyVersionDataInternal(node, version, true, excluded);

            // Ensure the DB consistency
            node.EnsureConsistency();

            // Make sure document acts like last edited version to prevent published data modification
            node.IsLastVersion = true;
        }


        /// <summary>
        /// Gets the set of columns from the given list of columns
        /// </summary>
        /// <param name="columns">Columns</param>
        private static HashSet<string> GetColumnsSet(IEnumerable<string> columns)
        {
            var columnsSet =
                (columns != null) ?
                    new HashSet<string>(columns, StringComparer.InvariantCultureIgnoreCase) :
                    null;

            return columnsSet;
        }


        /// <summary>
        /// Applies latest version data to the node.
        /// </summary>
        /// <param name="node">Document node</param>
        public void ApplyLatestVersion(TreeNode node)
        {
            ApplyLatestVersionInternal(node);
        }


        /// <summary>
        /// Saves history version record to the database without checking in.
        /// </summary>
        /// <param name="node">Node object that should be stored in the version history</param>
        /// <param name="versionNumber">Version number</param>
        /// <param name="versionComment">Version comment</param>
        /// <param name="updateColumns">List of columns which should be updated explicitly (separated by ';')</param>
        /// <param name="forceWorkflow">Workflow instance to force to apply</param>
        public void SaveVersion(TreeNode node, string versionNumber = null, string versionComment = null, string updateColumns = null, WorkflowInfo forceWorkflow = null)
        {
            SaveVersionInternal(node, versionNumber, versionComment, updateColumns, forceWorkflow);
        }


        /// <summary>
        /// Checks out specified node and creates a new record in the CMS_VersionHistory table.
        /// </summary>
        /// <param name="node">Document to check out</param>
        /// <returns>Returns current workflow step of the document</returns>
        public WorkflowStepInfo CheckOut(TreeNode node)
        {
            return CheckOut(node, node.IsPublished);
        }


        /// <summary>
        /// Checks out specified node and creates a new record in the CMS_VersionHistory table.
        /// </summary>
        /// <param name="node">Document to check out</param>
        /// <param name="nodeIsPublished">Document is published</param>
        /// <param name="isAutomatic">Indicates whether check-in should be performed automatically later on</param>
        /// <returns>Returns current workflow step of the document</returns>
        public WorkflowStepInfo CheckOut(TreeNode node, bool nodeIsPublished, bool isAutomatic = false)
        {
            return CheckOutInternal(node, nodeIsPublished, isAutomatic, null);
        }


        /// <summary>
        /// Checks out specified node and creates a new record in the CMS_VersionHistory table.
        /// </summary>
        /// <param name="node">Document to check out</param>
        /// <param name="nodeIsPublished">Document is published</param>
        /// <param name="isAutomatic">Indicates whether check-in should be performed automatically later on</param>
        /// <param name="handleSpecialSteps">Indicates if special steps should be handled</param>
        /// <returns>Returns current workflow step of the document</returns>
        public WorkflowStepInfo CheckOut(TreeNode node, bool nodeIsPublished, bool isAutomatic, bool handleSpecialSteps)
        {
            return CheckOutInternal(node, nodeIsPublished, isAutomatic, handleSpecialSteps);
        }


        /// <summary>
        /// Checks provided node in and stores it in the versioning history.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="versionNumber">Version number</param>
        /// <param name="versionComment">Version comment</param>
        public void CheckIn(TreeNode node, string versionNumber, string versionComment = null)
        {
            CheckInInternal(node, versionNumber, versionComment);
        }


        /// <summary>
        /// Creates new document version. (Moves document to edit step.)
        /// </summary>
        /// <param name="node">Document node</param>
        public WorkflowStepInfo CreateNewVersion(TreeNode node)
        {
            return CreateNewVersionInternal(node);
        }


        /// <summary>
        /// Returns the version history table for the specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public ObjectQuery<VersionHistoryInfo> GetDocumentHistory(int documentId)
        {
            return GetDocumentHistoryInternal(documentId);
        }


        /// <summary>
        /// Gets latest document version
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public VersionHistoryInfo GetLatestDocumentVersion(int documentId)
        {
            return GetLatestDocumentVersionInternal(documentId);
        }


        /// <summary>
        /// Deletes complete history of specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public void DestroyDocumentHistory(int documentId)
        {
            DestroyDocumentHistoryInternal(documentId);
        }


        /// <summary>
        /// Clears complete history of specified document (keeps latest version).
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public void ClearDocumentHistory(int documentId)
        {
            ClearDocumentHistoryInternal(documentId);
        }


        /// <summary>
        /// Deletes specified record in the node version history.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID to delete</param>
        public void DestroyDocumentVersion(int versionHistoryId)
        {
            DestroyDocumentVersionInternal(versionHistoryId);
        }


        /// <summary>
        /// Deletes older document versions to keep specified version history length.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="siteName">Site name</param>
        /// <returns>Returns the version number of the last deleted version</returns>
        public void DeleteOlderVersions(int documentId, string siteName)
        {
            DeleteOlderVersionsInternal(documentId, siteName);
        }


        /// <summary>
        /// Puts the specified node history version to the front DB tables.
        /// </summary>
        /// <param name="versionHistoryId">ID of the version history record</param>
        /// <param name="checkPublish">If true, the ToBePublished item is checked to find out whether to publish or not</param>
        public void PublishVersion(int versionHistoryId, bool checkPublish = true)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.WorkflowVersioning);
            }

            PublishVersionInternal(versionHistoryId, checkPublish);
        }


        /// <summary>
        /// Undo all operations made during last checkout.
        /// </summary>
        /// <param name="node">Document node</param>
        public void UndoCheckOut(TreeNode node)
        {
            UndoCheckOutInternal(node);
        }


        /// <summary>
        /// Adds specified earlier version to the top of the version history.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID</param>
        /// <returns>Returns new version history ID</returns>
        public int RollbackVersion(int versionHistoryId)
        {
            return RollbackVersionInternal(versionHistoryId);
        }


        /// <summary>
        /// Publishes all documents scheduled for the current time.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public void PublishAllScheduled(string siteName)
        {
            PublishAllScheduledInternal(siteName);
        }


        /// <summary>
        /// Restores deleted node and returns the restored node.
        /// </summary>
        /// <param name="versionHistoryId">ID of the version to be restored</param>
        public TreeNode RestoreDocument(int versionHistoryId)
        {
            return RestoreDocumentInternal(versionHistoryId);
        }


        /// <summary>
        /// Returns new version number representation.
        /// </summary>
        /// <param name="oldVersionNumber">Old version number</param>
        /// <param name="isMajorVersion">If true, the version number is considered major</param>
        /// <param name="siteName">Site name</param>
        /// <param name="useCheckInCheckOut">Indicates if use check-in/check-out</param>
        public string GetNewVersion(string oldVersionNumber, bool isMajorVersion, string siteName, bool useCheckInCheckOut)
        {
            return GetNewVersionInternal(oldVersionNumber, isMajorVersion, siteName, useCheckInCheckOut);
        }


        /// <summary>
        /// Ensures that the document version history is present for the given document, returns the document version.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="nodeIsPublished">Document is published</param>
        /// <param name="nodeDeletion">Indicates if the version should be ensured for the recycle bin</param>
        /// <param name="forceWorkflow">Workflow instance to force to apply</param>
        /// <remarks>The document is updated to the database, there is no need to update it after</remarks>
        public int EnsureVersion(TreeNode node, bool nodeIsPublished, bool nodeDeletion = false, WorkflowInfo forceWorkflow = null)
        {
            return EnsureVersionInternal(node, nodeIsPublished, nodeDeletion, forceWorkflow);
        }


        /// <summary>
        /// Specifies whether the environment is configured to use check-in and check-out actions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UseCheckInCheckOut(string siteName)
        {
            VersionManager vm = GetInstance(null);
            return vm.UseCheckInCheckOutInternal(siteName);
        }


        /// <summary>
        /// Specifies whether the environment is configured to use automatic version numbering.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UseAutomaticVersionNumbering(string siteName)
        {
            VersionManager vm = GetInstance(null);
            return vm.UseAutomaticVersionNumberingInternal(siteName);
        }


        /// <summary>
        /// Gets version history length
        /// </summary>
        /// <param name="siteName">Site name</param>
        public int GetHistoryLength(string siteName)
        {
            VersionManager vm = GetInstance(null);
            return vm.GetHistoryLengthInternal(siteName);
        }


        /// <summary>
        /// Returns true if the given column is present within the given set of excluded columns
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="excludedColumns">Excluded columns</param>
        private static bool IsExcludedColumn(string columnName, ISet<string> excludedColumns)
        {
            if (excludedColumns == null)
            {
                return false;
            }

            return excludedColumns.Contains(columnName);
        }


        /// <summary>
        /// Returns true if the given column name is a versioned data column name.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="columnName">Column name</param>
        public static bool IsVersionedCoupledColumn(string className, string columnName)
        {
            VersionManager vm = GetInstance(null);
            return vm.IsVersionedCoupledColumnInternal(className, columnName);
        }


        /// <summary>
        /// Returns true if the given column name is a versioned data column name.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public static bool IsVersionedDocumentColumn(string columnName)
        {
            VersionManager vm = GetInstance(null);
            return vm.IsVersionedDocumentColumnInternal(columnName);
        }


        /// <summary>
        /// Returns true if the given column name is a system data column name of the node.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public static bool IsSystemNodeColumn(string columnName)
        {
            VersionManager vm = GetInstance(null);
            return vm.IsSystemNodeColumnInternal(columnName);
        }


        /// <summary>
        /// Returns true if the given column name is a system data column name.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public static bool IsSystemDocumentColumn(string columnName)
        {
            VersionManager vm = GetInstance(null);
            return vm.IsSystemDocumentColumnInternal(columnName);
        }

        #endregion


        #region "Methods for attachment versioning"

        /// <summary>
        /// Removes the AttachmentHistory binding to the document version history and deletes the AttachmentHistory object if there is no more bindings to that version.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        public void RemoveAttachmentVersion(int versionHistoryId, Guid attachmentGuid)
        {
            RemoveAttachmentVersionInternal(versionHistoryId, attachmentGuid);
        }


        /// <summary>
        /// Saves the attachment version to the database.
        /// </summary>
        /// <param name="attachment">Attachment info object</param>
        /// <param name="versionHistoryId">Version history ID</param>
        public AttachmentHistoryInfo SaveAttachmentVersion(DocumentAttachment attachment, int versionHistoryId)
        {
            var context = new AttachmentHistorySetterContext(attachment, versionHistoryId);
            return SaveAttachmentVersion(context);
        }


        /// <summary>
        /// Saves the attachment to the database.
        /// </summary>
        /// <param name="context">Context required for the attachment history to be saved.</param>
        /// <returns>Saved instance of attachment version.</returns>
        internal AttachmentHistoryInfo SaveAttachmentVersion(AttachmentHistorySetterContext context)
        {
            var attachmentHistorySetter = new AttachmentHistorySetter(context);
            AttachmentHistoryInfo history = null;

            if (!WorkflowEvents.SaveAttachmentVersion.IsBound)
            {
                return attachmentHistorySetter.Set();
            }

            var node = DocumentHelper.GetDocument(context.SourceAttachment.AttachmentDocumentID, null);

            using (var handler = WorkflowEvents.SaveAttachmentVersion.StartEvent(node, context.SourceAttachment, node.TreeProvider))
            {
                handler.DontSupportCancel();

                if (handler.CanContinue())
                {
                    history = attachmentHistorySetter.Set();
                }

                handler.FinishEvent();
            }

            return history;
        }


        /// <summary>
        /// Publishes the attachments for the given document.
        /// </summary>
        /// <param name="versionHistoryId">Version history for which the attachments should be published.</param>
        public void PublishAttachments(int versionHistoryId)
        {
            PublishAttachmentsInternal(versionHistoryId);
        }


        /// <summary>
        /// Returns the attachment version.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="getBinary">Indicates if binary data should be included</param>
        public AttachmentHistoryInfo GetAttachmentVersion(int versionHistoryId, Guid attachmentGuid, bool getBinary = true)
        {
            return GetAttachmentVersionInternal(versionHistoryId, attachmentGuid, getBinary);
        }


        /// <summary>
        /// Returns the attachment version.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="fileName">File name</param>
        /// <param name="getBinary">Indicates if binary data should be included</param>
        public AttachmentHistoryInfo GetAttachmentVersion(int versionHistoryId, string fileName, bool getBinary)
        {
            return GetAttachmentVersionInternal(versionHistoryId, fileName, getBinary);
        }


        /// <summary>
        /// Returns latest attachment version found in the AttachmentHistoryTable.
        /// Returns only an attachment which is not a variant. For an attachment variant returns <c>null</c>.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Attachment site name</param>
        /// <param name="getBinary">Indicates if binary data should be included</param>
        public AttachmentHistoryInfo GetLatestAttachmentVersion(Guid attachmentGuid, string siteName, bool getBinary = true)
        {
            return GetLatestAttachmentVersionInternal(attachmentGuid, siteName, getBinary);
        }


        /// <summary>
        /// Returns attachment histories for the given version.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID.</param>
        /// <remarks>The IncludeBinaryData property and the BinaryData method don't load binary data 
        /// for attachments stored on the filesystem. To load binary data for attachments stored on the 
        /// filesystem, use the AttachmentBinary property of every record.</remarks>
        public ObjectQuery<AttachmentHistoryInfo> GetVersionAttachments(int versionHistoryId)
        {
            return GetVersionAttachmentsInternal(versionHistoryId);
        }

        #endregion


        #region "Additional methods"

        /// <summary>
        /// Removes all the workflow information from the document and initializes the document as non-workflow.
        /// </summary>
        /// <param name="node">Document node (current version)</param>
        /// <remarks>Intended for use after finishing current workflow cycle to remove workflow scope from the document</remarks>
        public void RemoveWorkflow(TreeNode node)
        {
            RemoveWorkflowInternal(node);
        }


        /// <summary>
        /// Applies the version data to the document DataSet.
        /// </summary>
        /// <param name="data">Dataset with the data</param>
        /// <param name="coupled">Indicates if versioned data should be applied to the coupled data</param>
        /// <param name="excludedColumns">Set of columns which should not be applied</param>
        public void ApplyVersionData(DataSet data, bool coupled, IEnumerable<string> excludedColumns = null)
        {
            // Get the excluded columns set
            var excluded = GetColumnsSet(excludedColumns);

            ApplyVersionDataInternal(data, coupled, excluded);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Deletes all versions from recycle bin for given site. 
        /// Together with deleted recycle bin version are removed all related older versions.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        internal void DeleteRecycleBinAndAllRelatedOlderVersions(int siteId)
        {
            VersionHistoryInfoProvider.GetRecycleBin(siteId)
                                      .Columns("DocumentID")
                                      .GetListResult<int>()
                                      .ToList()
                                      .ForEach(DestroyDocumentHistory);
        }


        /// <summary>
        /// Returns node with applied version data.
        /// </summary>
        /// <param name="version">Document version history</param>
        /// <param name="sourceNode">Document node</param>
        protected virtual TreeNode GetVersionInternal(VersionHistoryInfo version, TreeNode sourceNode)
        {
            // Check if version provided
            if (version == null)
            {
                return sourceNode;
            }

            TreeNode node;
            if (sourceNode == null)
            {
                // Get current node by version document ID
                node = TreeProvider.SelectSingleDocument(version.DocumentID);
            }
            else if (!sourceNode.IsComplete)
            {
                // Get current node by source node information
                node = TreeProvider.SelectSingleNode(sourceNode.NodeID, sourceNode.DocumentCulture, sourceNode.NodeClassName);
            }
            else
            {
                // Make sure source node instance is not modified
                node = sourceNode.Clone();
            }

            // If current node exists, use its base data
            if (node != null)
            {
                ApplyVersion(node, version);
            }
            // Else use only version data
            else
            {
                var data = version.Data;
                if (data == null)
                {
                    throw new InvalidOperationException("Missing page data.");
                }

                node = GetDocumentFromVersionData(data);

                node.NodeSiteID = version.NodeSiteID;
                node.DocumentID = version.DocumentID;
                node.NodeAliasPath = version.VersionNodeAliasPath;

                // Update step information
                int stepId = version.VersionWorkflowStepID;
                if (stepId > 0)
                {
                    node.DocumentWorkflowStepID = stepId;

                    // Update archived flag
                    var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
                    if (step != null)
                    {
                        node.DocumentIsArchived = step.StepIsArchived;
                    }
                }

                node.DocumentCheckedOutVersionHistoryID = version.VersionHistoryID;

                // Ensure the DB consistency
                node.EnsureConsistency();

                // Make sure document acts like last edited version to prevent published data modification
                node.IsLastVersion = true;
            }

            return node;
        }


        private TreeNode GetDocumentFromVersionData(IDataContainer dataContainer)
        {
            var className = new DocumentClassNameRetriever(dataContainer, true).Retrieve();
            return TreeNode.New<TreeNode>(className, dataContainer, TreeProvider);
        }


        /// <summary>
        /// Applies latest version data to the node.
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual void ApplyLatestVersionInternal(TreeNode node)
        {
            // If there is no checkout version, return original node
            int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
            if (versionHistoryId == 0)
            {
                return;
            }

            // Get the version history record
            var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
            if (version == null)
            {
                throw new InvalidOperationException("History version data not found.");
            }

            ApplyVersion(node, version);
        }


        /// <summary>
        /// Saves history version record to the database without checking in.
        /// </summary>
        /// <param name="node">Node object that should be stored in the version history</param>
        /// <param name="versionNumber">Version number</param>
        /// <param name="versionComment">Version comment</param>
        /// <param name="updateColumns">List of columns which should be updated explicitly (separated by ';')</param>
        /// <param name="forceWorkflow">Workflow instance to force to apply</param>
        protected virtual void SaveVersionInternal(TreeNode node, string versionNumber, string versionComment, string updateColumns, WorkflowInfo forceWorkflow)
        {
            // Check if node given
            if (node == null)
            {
                throw new ArgumentNullException("node", "No node object specified.");
            }

            // Handle the event
            using (var h = WorkflowEvents.SaveVersion.StartEvent(node, ref versionNumber, ref versionComment, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    if (versionNumber != null)
                    {
                        node.DocumentLastVersionNumber = versionNumber;
                    }

                    var publishedNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);
                    if (publishedNode == null)
                    {
                        throw new InvalidOperationException("Published version of the document not found.");
                    }

                    UpdatePublishedDocumentFromVersion(node, publishedNode, updateColumns);

                    var settings = GetNonVersionedCopySettings();
                    DocumentHelper.CopyNodeData(publishedNode, node, settings);

                    CorrectAutomaticallyGeneratedProperties(node, node);

                    var versionId = EnsureVersion(publishedNode, publishedNode.IsPublished, false, forceWorkflow);
                    var versionHistory = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionId);
                    versionHistory.VersionDocumentName = node.DocumentName;
                    versionHistory.DocumentNamePath = node.DocumentNamePath;
                    versionHistory.VersionDocumentType = node.DocumentType;
                    versionHistory.VersionNodeAliasPath = node.NodeAliasPath;
                    versionHistory.VersionMenuRedirectUrl = node.DocumentMenuRedirectUrl;
                    versionHistory.VersionClassID = node.GetValue("NodeClassID", 0);
                    versionHistory.SetData(node);
                    versionHistory.ModifiedByUserID = TreeProvider.UserInfo.UserID;
                    versionHistory.ModifiedWhen = DateTime.Now;
                    if (versionComment != null)
                    {
                        versionHistory.VersionComment = versionComment;
                    }
                    if (versionNumber != null)
                    {
                        versionHistory.VersionNumber = versionNumber;
                    }
                    versionHistory.PublishFrom = node.DocumentPublishFrom;
                    versionHistory.PublishTo = node.DocumentPublishTo;

                    versionHistory.NodeSiteID = node.OriginalNodeSiteID;

                    VersionHistoryInfoProvider.SetVersionHistoryInfo(versionHistory);
                    h.EventArguments.VersionHistory = versionHistory;
                }

                // Finalize the event
                h.FinishEvent();
            }
        }


        private static CopyNodeDataSettings GetNonVersionedCopySettings()
        {
            // Update the node data that are not versioned within the original node record
            return new CopyNodeDataSettings(true, new[] { "DocumentNamePath" })
            {
                CopyVersionedData = false,
                // SKU data is versioned too
                CopySKUData = false
            };
        }


        private void UpdatePublishedDocumentFromVersion(TreeNode versionNode, TreeNode publishedNode, string extraColumnsToUpdate)
        {
            var versionHasExistingSKU = versionNode.NodeSKUID != 0;

            var settings = GetNonVersionedCopySettings();

            // Copy SKU data if new SKU is being bound
            settings.CopySKUData = !versionHasExistingSKU;

            DocumentHelper.CopyNodeData(versionNode, publishedNode, settings);

            // Set the node SKUID if the existing SKU is being bound
            if (versionHasExistingSKU)
            {
                var latestVersion = GetLatestDocumentVersion(versionNode.DocumentID);
                if (latestVersion != null)
                {
                    var lastVersionNode = GetVersion(latestVersion.VersionHistoryID);
                    if ((lastVersionNode != null) && (versionNode.NodeSKUID != lastVersionNode.NodeSKUID))
                    {
                        publishedNode.NodeSKUID = versionNode.NodeSKUID;
                    }
                }
            }

            // Update extra columns
            if (extraColumnsToUpdate != null)
            {
                var cols = extraColumnsToUpdate.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string col in cols)
                {
                    publishedNode.SetValue(col, versionNode.GetValue(col));
                }
            }

            using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
            {
                bool originalUpdateSKU = TreeProvider.UpdateSKUColumns;
                TreeProvider.UpdateSKUColumns = false;

                publishedNode.Update();

                TreeProvider.UpdateSKUColumns = originalUpdateSKU;
            }

            DocumentHelper.UpdateSearchIndexIfAllowed(publishedNode);
        }


        /// <summary>
        /// Checks out specified node and creates a new record in the CMS_VersionHistory table.
        /// </summary>
        /// <param name="node">Document to check out</param>
        /// <param name="nodeIsPublished">Document is published</param>
        /// <param name="isAutomatic">Indicates whether check-in should be performed automatically later on</param>
        /// <param name="handleSpecialSteps">Indicates if special steps should be handled</param>
        /// <returns>Returns current workflow step of the document</returns>
        protected virtual WorkflowStepInfo CheckOutInternal(TreeNode node, bool nodeIsPublished, bool isAutomatic, bool? handleSpecialSteps)
        {
            // Check if node given
            if (node == null)
            {
                throw new ArgumentNullException("node", "No node object specified.");
            }

            // Assign the TreeProvider to the node
            node.TreeProvider = TreeProvider;
            string siteName = node.NodeSiteName;

            // Insert the node within transaction
            using (var tr = new CMSTransactionScope())
            {
                // Get workflow info
                WorkflowInfo wi = WorkflowManager.GetNodeWorkflow(node);
                // Check if document is under workflow
                if (wi == null)
                {
                    throw new InvalidOperationException("The node '" + node.GetDocumentName() + "' does not support workflow.");
                }

                // Handle special steps for basic workflow by default
                if (handleSpecialSteps == null)
                {
                    handleSpecialSteps = wi.IsBasic;
                }

                bool useCheckInCheckOut = wi.UseCheckInCheckOut(siteName);

                // Check if node is not checked out yet
                if (node.IsCheckedOut && useCheckInCheckOut)
                {
                    throw new WorkflowException("The node has been already checked out.");
                }

                WorkflowStepInfo currentStep = null;

                // Handle the event
                using (var h = WorkflowEvents.CheckOut.StartEvent(node, TreeProvider))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        // Create a new document version
                        VersionHistoryInfo previousVersionHistory = null;

                        string previousVersionNumber = "0.0";
                        int previousVersionHistoryID = EnsureVersion(node, nodeIsPublished);

                        // Get previous version to read version number
                        if (previousVersionHistoryID > 0)
                        {
                            previousVersionHistory = VersionHistoryInfoProvider.GetVersionHistoryInfo(node.DocumentCheckedOutVersionHistoryID);
                            if (previousVersionHistory != null)
                            {
                                previousVersionNumber = previousVersionHistory.VersionNumber;
                            }
                        }

                        CorrectAutomaticallyGeneratedProperties(node, node);

                        var versionHistory = new VersionHistoryInfo();
                        versionHistory.DocumentID = node.DocumentID;
                        versionHistory.DocumentNamePath = node.DocumentNamePath;
                        versionHistory.VersionClassID = node.GetValue("NodeClassID", 0);
                        versionHistory.VersionDocumentName = node.DocumentName;
                        versionHistory.VersionDocumentType = node.DocumentType;
                        versionHistory.VersionMenuRedirectUrl = node.DocumentMenuRedirectUrl;
                        if (previousVersionHistory == null)
                        {
                            versionHistory.SetData(node);
                        }
                        else
                        {
                            versionHistory.NodeXML = previousVersionHistory.NodeXML;
                        }
                        versionHistory.ModifiedByUserID = TreeProvider.UserInfo.UserID;
                        versionHistory.ModifiedWhen = DateTime.Now;

                        // Set new version number
                        string newVersionNumber = GetNewVersion(previousVersionNumber, false, siteName, useCheckInCheckOut);
                        versionHistory.VersionNumber = newVersionNumber;
                        node.DocumentLastVersionNumber = newVersionNumber;

                        versionHistory.VersionComment = "";
                        versionHistory.ToBePublished = false;
                        versionHistory.NodeSiteID = node.OriginalNodeSiteID;

                        // Get workflow step info
                        if (node.DocumentWorkflowStepID > 0)
                        {
                            currentStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(node.DocumentWorkflowStepID);
                            // Update version workflow info
                            versionHistory.VersionWorkflowStepID = currentStep.StepID;
                            versionHistory.VersionWorkflowID = currentStep.StepWorkflowID;
                        }

                        // Update version
                        VersionHistoryInfoProvider.SetVersionHistoryInfo(versionHistory);

                        // Get the new version history object ID
                        int versionHistoryID = versionHistory.VersionHistoryID;

                        // Copy version attachments binding
                        if (previousVersionHistoryID > 0)
                        {
                            VersionAttachmentInfoProvider.CopyVersionAttachments(previousVersionHistoryID, versionHistoryID);
                        }

                        // Update node properties            
                        node.DocumentCheckedOutByUserID = TreeProvider.UserInfo.UserID;
                        node.DocumentCheckedOutWhen = DateTime.Now;
                        node.DocumentCheckedOutVersionHistoryID = versionHistoryID;

                        // Handle automatic checkout
                        if (isAutomatic)
                        {
                            node.DocumentCheckedOutAutomatically = true;
                        }

                        WorkflowStepInfo targetStep = null;

                        // If no workflow step is specified or step is "published" or "archived", set step to "edit" (start new workflow cycle)
                        if ((currentStep == null) || currentStep.StepIsPublished || currentStep.StepIsArchived)
                        {
                            // Check the workflow scope (clear workflow if no scope defined)
                            var scope = WorkflowManager.GetNodeWorkflowScope(node);
                            if (scope == null)
                            {
                                RemoveWorkflow(node);
                            }
                            // Move document to the first step for basic workflow
                            else if (handleSpecialSteps.Value)
                            {
                                // Reset workflow cycle GUID if node was published or archived
                                if (!TreePathUtils.AllowPermanentPreviewLink(siteName))
                                {
                                    node.DocumentWorkflowCycleGUID = Guid.NewGuid();
                                }

                                // If previous step was archived, set the node data not to be published
                                if ((currentStep != null) && currentStep.StepIsArchived)
                                {
                                    var currentNode = TreeProvider.SelectSingleDocument(node.DocumentID);
                                    currentNode.DocumentIsArchived = false;
                                    currentNode.DocumentWorkflowCycleGUID = node.DocumentWorkflowCycleGUID;

                                    using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                                    {
                                        currentNode.Update();
                                    }
                                }

                                node.DocumentIsArchived = false;

                                // Set first step for basic workflow
                                // Get the first step
                                targetStep = WorkflowStepInfoProvider.GetFirstStep(scope.ScopeWorkflowID);
                                node.DocumentWorkflowStepID = targetStep.StepID;
                            }

                            // Update version history
                            if (targetStep == null)
                            {
                                versionHistory.VersionWorkflowStepID = 0;
                                versionHistory.VersionWorkflowID = 0;
                            }
                            else
                            {
                                versionHistory.VersionWorkflowStepID = targetStep.StepID;
                                versionHistory.VersionWorkflowID = targetStep.StepWorkflowID;
                            }

                            VersionHistoryInfoProvider.SetVersionHistoryInfo(versionHistory);
                            currentStep = targetStep;
                        }

                        using (new DocumentActionContext { SendNotifications = false, LogEvents = false })
                        {
                            DocumentHelper.UpdateDocument(node, TreeProvider);
                        }

                        // Ensure version attachments if first version
                        if (previousVersionHistoryID <= 0)
                        {
                            EnsureVersionAttachments(node.DocumentCheckedOutVersionHistoryID);
                        }

                        // Delete older document versions
                        DeleteOlderVersions(node.DocumentID, siteName);

                        // Reset timeout
                        WorkflowManager.HandleStepTimeout(node, TreeProvider.UserInfo, wi, currentStep, null);

                        new DocumentEventLogger(node).Log("CHECKOUT", ResHelper.GetString("contentedit.documentwascheckedout"), false);
                    }

                    // Finalize the event
                    h.FinishEvent();
                }

                // Commit transaction if necessary
                tr.Commit();

                return currentStep;
            }
        }


        /// <summary>
        /// Checks provided node in and stores it in the versioning history.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="versionNumber">Version number</param>
        /// <param name="versionComment">Version comment</param>
        protected virtual void CheckInInternal(TreeNode node, string versionNumber, string versionComment)
        {
            // Check if node given
            if (node == null)
            {
                throw new ArgumentNullException("node", "No node object specified.");
            }

            // Get workflow info
            WorkflowInfo wi = WorkflowManager.GetNodeWorkflow(node);
            // Check if document is under workflow
            if (wi == null)
            {
                throw new InvalidOperationException("The node '" + node.GetDocumentName() + "' does not support workflow.");
            }

            string siteName = node.NodeSiteName;
            bool useCheckInCheckOut = wi.UseCheckInCheckOut(siteName);

            // Check if node is checked out
            if (!node.IsCheckedOut && useCheckInCheckOut)
            {
                throw new WorkflowException("The node has not been checked out, it cannot be checked in.");
            }

            // Handle the event
            using (var h = WorkflowEvents.CheckIn.StartEvent(node, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    EnsureVersion(node, node.IsPublished);

                    // Get current version history ID
                    int versionHistoryID = node.DocumentCheckedOutVersionHistoryID;

                    // Update record in the CMS_VersionHistory table
                    TreeNode oldNode = GetVersion(versionHistoryID, node);

                    if (oldNode != null)
                    {
                        // Reset checkout information
                        oldNode.NodeID = node.NodeID;
                        DocumentHelper.ClearCheckoutInformation(oldNode);
                        oldNode.DocumentCheckedOutVersionHistoryID = node.DocumentCheckedOutVersionHistoryID;
                        oldNode.NodeACLID = node.NodeACLID;

                        // Do not create search task, consequent update creates it
                        using (new DocumentActionContext { CreateSearchTask = false })
                        {
                            SaveVersion(oldNode, versionNumber, versionComment);
                        }
                    }

                    // Update node properties            
                    DocumentHelper.ClearCheckoutInformation(node);
                    node.DocumentCheckedOutVersionHistoryID = versionHistoryID;

                    // Defer update of search index to prevent possibly multiple search tasks being logged when MoveToPublishedStep publishes the document immediately
                    using (new DocumentActionContext { CreateSearchTask = false })
                    {
                        using (new CMSActionContext { LogEvents = false })
                        {
                            DocumentHelper.UpdateDocument(node, TreeProvider);
                        }

                        // If documents should be automatically published
                        if (wi.WorkflowAutoPublishChanges)
                        {
                            // Move document directly to published step
                            node.MoveToPublishedStep(versionComment);
                        }
                    }

                    DocumentHelper.UpdateSearchIndexIfAllowed(node);

                    // Reset timeout
                    WorkflowManager.HandleStepTimeout(node, TreeProvider.UserInfo, wi, null, node.WorkflowStep);

                    new DocumentEventLogger(node).Log("CHECKIN", ResHelper.GetString("contentedit.documentwascheckedin"), false);
                }

                // Finalize the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Creates new document version. (Moves document to edit step.)
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual WorkflowStepInfo CreateNewVersionInternal(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node", "No node object specified.");
            }

            var scope = WorkflowManager.GetNodeWorkflowScope(node);
            if (scope == null)
            {
                // No scope applied, remove workflow
                RemoveWorkflow(node);
                return null;
            }

            var workflow = WorkflowInfoProvider.GetWorkflowInfo(scope.ScopeWorkflowID);
            if (workflow == null)
            {
                return null;
            }

            string siteName = node.NodeSiteName;
            bool useCheckInCheckOut = workflow.UseCheckInCheckOut(siteName);

            WorkflowStepInfo step;
            // Do not log separate event to event log
            using (new CMSActionContext { LogEvents = false })
            {
                step = CheckOutInternal(node, node.IsPublished, true, true);
            }

            // If using check-in/check-out, there is no need to check in automatically
            if (useCheckInCheckOut)
            {
                return step;
            }

            // Do not log separate event to event log
            using (new CMSActionContext { LogEvents = false })
            {
                CheckInInternal(node, null, null);
            }

            return step;
        }


        /// <summary>
        /// Returns the version history table for the specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        protected virtual ObjectQuery<VersionHistoryInfo> GetDocumentHistoryInternal(int documentId)
        {
            return VersionHistoryInfoProvider.GetVersionHistories()
                                             .WhereEquals("DocumentID", documentId);
        }


        /// <summary>
        /// Gets latest document version
        /// </summary>
        /// <param name="documentId">Document ID</param>
        protected virtual VersionHistoryInfo GetLatestDocumentVersionInternal(int documentId)
        {
            // Find latest version
            var query = VersionHistoryInfoProvider.GetVersionHistories()
                                                    .TopN(1)
                                                    .WhereEquals("DocumentID", documentId)
                                                    .OrderByDescending("VersionHistoryID");

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Deletes complete history of specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        protected virtual void DestroyDocumentHistoryInternal(int documentId)
        {
            // Delete versions
            GetDocumentHistory(documentId)
                .Column("VersionHistoryID")
                .GetListResult<int>()
                .ToList()
                .ForEach(DestroyDocumentVersion);
        }


        /// <summary>
        /// Clears complete history of specified document (keeps the latest version).
        /// </summary>
        /// <param name="documentId">Document ID</param>
        protected virtual void ClearDocumentHistoryInternal(int documentId)
        {
            // Get the history
            var ids = GetDocumentHistory(documentId)
                .Column("VersionHistoryID")
                .GetListResult<int>()
                .ToList();
            var last = ids.LastOrDefault();

            // Delete the versions except the last one
            ids.ForEach(id => { if (id != last) { DestroyDocumentVersion(id); } });
        }


        /// <summary>
        /// Deletes specified record in the node version history.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID to delete</param>
        protected virtual void DestroyDocumentVersionInternal(int versionHistoryId)
        {
            var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
            if (version == null)
            {
                return;
            }

            DestroyDocumentVersionAttachments(versionHistoryId);

            version.Delete();
        }


        private void DestroyDocumentVersionAttachments(int versionHistoryId)
        {
            new AttachmentHistoryVersionRemover(versionHistoryId).Remove();
        }


        /// <summary>
        /// Deletes older document versions to keep specified version history length.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="siteName">Site name</param>
        protected virtual void DeleteOlderVersionsInternal(int documentId, string siteName)
        {
            // Get the versions to delete
            int maxNumber = GetHistoryLength(siteName);
            if (maxNumber <= 0)
            {
                return;
            }

            var query = VersionHistoryInfoProvider.GetVersionHistories()
                                      .Columns("VersionHistoryID")
                                      .WhereEquals("DocumentID", documentId)
                                      .WhereNotIn("VersionHistoryID", VersionHistoryInfoProvider.GetVersionHistories()
                                                                                                .TopN(maxNumber)
                                                                                                .WhereEquals("DocumentID", documentId)
                                                                                                .OrderByDescending("VersionHistoryID")
                                                                                                .AsIDQuery())
                                      .WhereNotIn("VersionHistoryID", DocumentCultureDataInfoProvider.GetDocumentCultures()
                                                                                                     .Column("DocumentPublishedVersionHistoryID")
                                                                                                     .WhereEquals("DocumentID", documentId)
                                                                                                     .WhereNotNull("DocumentPublishedVersionHistoryID"));
            query.GetListResult<int>()
                 .ToList()
                 .ForEach(DestroyDocumentVersion);
        }


        /// <summary>
        /// Puts the specified node history version to the front DB tables.
        /// </summary>
        /// <param name="versionHistoryId">ID of the version history record</param>
        /// <param name="checkPublish">If true, the ToBePublished item is checked to find out whether to publish or not</param>
        protected virtual void PublishVersionInternal(int versionHistoryId, bool checkPublish)
        {
            // Process within transaction
            using (var tr = new CMSTransactionScope())
            {
                // Get the version to publish
                var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                if (version == null)
                {
                    throw new InvalidOperationException("Version history object ID " + versionHistoryId + " not found.");
                }

                // Check if the version should really be published
                if (!checkPublish || version.ToBePublished)
                {
                    // Update the publish information
                    version.ToBePublished = false;
                    VersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                    // Get the current data, if not exists, do not continue with publishing
                    TreeNode currentNode = TreeProvider.SelectSingleDocument(version.DocumentID);
                    if (currentNode != null)
                    {
                        // Get the document data to publish
                        TreeNode nodeToBePublished = GetVersion(versionHistoryId, currentNode);

                        // Handle the event
                        using (var h = WorkflowEvents.Publish.StartEvent(nodeToBePublished, TreeProvider))
                        {
                            h.DontSupportCancel();

                            if (h.CanContinue())
                            {
                                // Get published version
                                int publishedVersionHistoryId = currentNode.DocumentPublishedVersionHistoryID;
                                // Determines whether the publish is first or not
                                bool firstPublish = (publishedVersionHistoryId == 0);

                                // Set document history version
                                currentNode.DocumentPublishedVersionHistoryID = versionHistoryId;
                                nodeToBePublished.DocumentPublishedVersionHistoryID = versionHistoryId;

                                // Copy data to be published to current node
                                var settings = new CopyNodeDataSettings(true, true, true, true, false, true, true, true, new[] { "documentnamepath" });
                                DocumentHelper.CopyNodeData(nodeToBePublished, currentNode, settings);

                                // Do not log events and do not process additional actions
                                using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                                {
                                    // Update document
                                    currentNode.Update();

                                    // Delete the previous version attachments
                                    AttachmentInfoProvider.DeleteAttachments(currentNode.DocumentID);

                                    // Publish the document attachments
                                    PublishAttachments(versionHistoryId);

                                    // Modify the version history record
                                    version.WasPublishedFrom = DateTime.Now;
                                    version.PublishTo = nodeToBePublished.DocumentPublishTo;
                                    version.ToBePublished = false;
                                    VersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                                    RemoveOlderVersionsFromScheduleToBePublished(versionHistoryId, currentNode.DocumentID);
                                    WithdrawPreviouslyPublishedVersion(versionHistoryId, publishedVersionHistoryId);
                                }

                                // Log synchronization
                                DocumentHelper.LogDocumentChange(currentNode, TaskTypeEnum.PublishDocument, TreeProvider);

                                currentNode.SendNotifications(firstPublish ? "CREATEDOC" : "UPDATEDOC");

                                if (SearchIndexInfoProvider.SearchEnabled && SearchHelper.SearchEnabledForClass(currentNode.NodeClassName))
                                {
                                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, currentNode.GetSearchID(), currentNode.DocumentID);
                                }

                                new DocumentEventLogger(currentNode).Log("PUBLISHDOC", ResHelper.GetString("contentedit.documentwaspublished"));

                                currentNode.ResetChanges();
                            }

                            h.EventArguments.PublishedDocument = currentNode;
                            h.FinishEvent();
                        }
                    }
                }

                tr.Commit();
            }
        }


        private static void WithdrawPreviouslyPublishedVersion(int versionHistoryId, int publishedVersionHistoryId)
        {
            if ((publishedVersionHistoryId <= 0) || (publishedVersionHistoryId == versionHistoryId))
            {
                return;
            }

            var publishedVersion = VersionHistoryInfoProvider.GetVersionHistoryInfo(publishedVersionHistoryId);
            if (publishedVersion == null)
            {
                return;
            }

            publishedVersion.WasPublishedTo = DateTime.Now;
            publishedVersion.PublishTo = DateTime.Now;
            VersionHistoryInfoProvider.SetVersionHistoryInfo(publishedVersion);
        }


        private static void RemoveOlderVersionsFromScheduleToBePublished(int versionHistoryId, int documentId)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@DocumentID", documentId);
            parameters.Add("@VersionHistoryID", versionHistoryId);
            VersionHistoryInfoProvider.UpdateData("[ToBePublished] = 0", "[DocumentID] = @DocumentID AND [VersionHistoryID] <= @VersionHistoryID", parameters);
        }


        /// <summary>
        /// Undo all operations made during last checkout.
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual void UndoCheckOutInternal(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node", "Node not specified.");
            }

            // Process within transaction
            using (var tr = new CMSTransactionScope())
            {
                // Handle the event
                using (var h = WorkflowEvents.UndoCheckOut.StartEvent(node, TreeProvider))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        // Throw the last checked out version
                        int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;

                        // Delete last record in CMS_VersionHistory
                        DestroyDocumentVersion(versionHistoryId);

                        // Get previous version
                        VersionHistoryInfo previousVersion = GetLatestDocumentVersion(node.DocumentID);
                        if (previousVersion == null)
                        {
                            throw new WorkflowException("There's no previous version to roll back.");
                        }

                        // Get the published node record from the database
                        TreeNode currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);

                        // Update node
                        DocumentHelper.ClearCheckoutInformation(node);
                        DocumentHelper.ClearCheckoutInformation(currentNode);

                        // Update document information
                        node.DocumentCheckedOutVersionHistoryID = previousVersion.VersionHistoryID;
                        currentNode.DocumentCheckedOutVersionHistoryID = previousVersion.VersionHistoryID;
                        node.DocumentLastVersionNumber = previousVersion.VersionNumber;
                        currentNode.DocumentLastVersionNumber = previousVersion.VersionNumber;

                        // Update step information
                        int stepId = previousVersion.VersionWorkflowStepID;
                        if (stepId > 0)
                        {
                            node.DocumentWorkflowStepID = stepId;
                            currentNode.DocumentWorkflowStepID = stepId;

                            // Update archived flag
                            var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
                            if (step != null)
                            {
                                node.DocumentIsArchived = step.StepIsArchived;
                                currentNode.DocumentIsArchived = step.StepIsArchived;
                            }
                        }

                        using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                        {
                            currentNode.Update();
                        }

                        // Apply version data to the document instance
                        ApplyVersion(node, previousVersion);

                        // Get workflow info
                        var workflow = WorkflowManager.GetNodeWorkflow(node);

                        // Reset timeout
                        WorkflowManager.HandleStepTimeout(node, TreeProvider.UserInfo, workflow, null, node.WorkflowStep);

                        new DocumentEventLogger(node).Log("UNDOCHECKOUT", ResHelper.GetString("contentedit.documentundocheckout"), false);
                    }

                    // Finalize the event
                    h.FinishEvent();
                }

                // Commit transaction if necessary
                tr.Commit();
            }
        }


        /// <summary>
        /// Adds specified earlier version to the top of the version history.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID</param>
        /// <returns>Returns new version history ID</returns>
        protected virtual int RollbackVersionInternal(int versionHistoryId)
        {
            int newVersionHistoryId;

            // Process within transaction
            using (var tr = new CMSTransactionScope())
            {
                // Get the requested version history object
                var rollbackVersion = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                if (rollbackVersion == null)
                {
                    throw new InvalidOperationException("The version cannot be rolled back since it the doesn't exist.");
                }

                var currentNode = TreeProvider.SelectSingleDocument(rollbackVersion.DocumentID);

                // Check if the document is not checked out
                if (currentNode.IsCheckedOut)
                {
                    throw new InvalidOperationException("The version cannot be rolled back since the page is currently checked out.");
                }

                // Get document workflow
                var workflow = WorkflowManager.GetNodeWorkflow(currentNode);

                // Check if document is under workflow
                if (workflow == null)
                {
                    throw new InvalidOperationException("The node '" + currentNode.GetDocumentName() + "' does not support workflow.");
                }

                // Get the first step
                var firstStep = WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID);

                // Insert the new version history object (for rollback)
                rollbackVersion = rollbackVersion.Clone(true);
                rollbackVersion.ModifiedWhen = DateTime.Now;
                rollbackVersion.ModifiedByUserID = TreeProvider.UserInfo.UserID;
                rollbackVersion.DeletedWhen = DateTimeHelper.ZERO_TIME;
                rollbackVersion.DeletedByUserID = 0;
                rollbackVersion.ToBePublished = false;
                rollbackVersion.WasPublishedFrom = DateTimeHelper.ZERO_TIME;
                rollbackVersion.WasPublishedTo = DateTimeHelper.ZERO_TIME;
                rollbackVersion.VersionWorkflowStepID = firstStep.StepID;
                rollbackVersion.VersionWorkflowID = firstStep.StepWorkflowID;
                VersionHistoryInfoProvider.SetVersionHistoryInfo(rollbackVersion);

                newVersionHistoryId = rollbackVersion.VersionHistoryID;

                // Set workflow step to edit
                currentNode.DocumentWorkflowStepID = firstStep.StepID;
                currentNode.DocumentIsArchived = false;
                currentNode.DocumentLastVersionNumber = rollbackVersion.VersionNumber;
                currentNode.DocumentCheckedOutVersionHistoryID = newVersionHistoryId;

                // Update the node
                using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                {
                    currentNode.Update();
                }

                // Copy version attachments
                VersionAttachmentInfoProvider.CopyVersionAttachments(versionHistoryId, newVersionHistoryId);

                // Delete older document versions
                DeleteOlderVersions(currentNode.DocumentID, currentNode.NodeSiteName);

                // Check if allowed 'Automatically publish changes'
                if (workflow.WorkflowAutoPublishChanges)
                {
                    // Automatically publish changes
                    WorkflowManager.MoveToPublishedStep(currentNode, null, false);
                }

                new DocumentEventLogger(currentNode).Log("ROLLBACKDOC", ResHelper.GetString("contentedit.documentwasrolledback"), false);

                // Commit transaction if necessary
                tr.Commit();
            }

            return newVersionHistoryId;
        }


        /// <summary>
        /// Publishes all documents scheduled for the current time.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected virtual void PublishAllScheduledInternal(string siteName)
        {
            // Get the site info
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                throw new ArgumentNullException("siteName", "Site name '" + siteName + "' does not exists.");
            }

            // Prepare where conditions
            var now = DateTime.Now;
            var fromWhere = new WhereCondition()
                .WhereNull("PublishFrom")
                .Or()
                .Where(new WhereCondition()
                    .WhereNotNull("PublishFrom")
                    .WhereLessOrEquals("PublishFrom", now));

            var toWhere = new WhereCondition()
                .WhereNull("PublishTo")
                .Or()
                .Where(new WhereCondition()
                    .WhereNotNull("PublishTo")
                    .WhereGreaterOrEquals("PublishTo", now));

            // Publish documents
            var versionIdsToBePublished = VersionHistoryInfoProvider.GetVersionHistories()
                                                                  .Column("VersionHistoryID")
                                                                  .WhereNull("WasPublishedFrom")
                                                                  .WhereTrue("ToBePublished")
                                                                  .OnSite(site.SiteID)
                                                                  .Where(new WhereCondition().Where(fromWhere).Where(toWhere))
                                                                  .OrderBy("PublishFrom")
                                                                  .GetListResult<int>();

            // Staging task is already logged when the document was scheduled to be published
            using (new DocumentActionContext { LogSynchronization = false })
            {
                foreach (var versionId in versionIdsToBePublished)
                {
                    PublishVersion(versionId);
                }
            }
        }


        /// <summary>
        /// Restores deleted node and returns the restored node.
        /// </summary>
        /// <param name="versionHistoryId">ID of the version to be restored</param>
        protected virtual TreeNode RestoreDocumentInternal(int versionHistoryId)
        {
            // Process within transaction
            using (var tr = new CMSTransactionScope())
            {
                var restoreVersion = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                if (restoreVersion == null)
                {
                    throw new InvalidOperationException("History version data not found.");
                }

                // Get the node to restore
                var restoreNode = GetVersion(versionHistoryId);
                var originalDocumentId = restoreVersion.DocumentID;

                // Check if parent site exists
                var site = SiteInfoProvider.GetSiteInfo(restoreNode.NodeSiteID);
                if (site == null)
                {
                    throw new InvalidOperationException("The node cannot be restored because its parent site no longer exists.");
                }

                // Check if culture is allowed
                string culture = CultureSiteInfoProvider.CheckCultureCode(restoreNode.DocumentCulture, site.SiteName);
                if (!string.Equals(culture, restoreNode.DocumentCulture, StringComparison.InvariantCultureIgnoreCase))
                {
                    culture = restoreNode.DocumentCulture;

                    var ci = CultureInfoProvider.GetCultureInfo(culture);
                    if (ci != null)
                    {
                        culture = ci.CultureName;
                    }

                    throw new InvalidOperationException("Culture '" + culture + "' is not enabled for current site, cannot restore the document.");
                }

                // Make sure document in published step is restored to edit step
                if (restoreNode.WorkflowStepType == WorkflowStepTypeEnum.DocumentPublished)
                {
                    var editStep = WorkflowStepInfoProvider.GetEditStep(restoreNode.WorkflowStep.StepWorkflowID);
                    restoreNode.DocumentWorkflowStepID = editStep.StepID;
                }

                // Ensure foreign keys consistency
                restoreNode.EnsureConsistency();

                // Ensure missing values
                bool defaultValueEnsured = FormHelper.EnsureDefaultValues(restoreNode.NodeClassName, restoreNode, FormResolveTypeEnum.AllFields);

                // Check for another node culture and current culture node
                bool insertNode = true;
                bool nodeExists = false;

                // Get by node GUID
                var cultureNode = TreeProvider.SelectSingleNode(restoreNode.NodeGUID, TreeProvider.ALL_CULTURES, site.SiteName);
                if (cultureNode == null)
                {
                    // If not found, get by node alias
                    cultureNode = TreeProvider.SelectSingleNode(site.SiteName, restoreNode.NodeAliasPath, TreeProvider.ALL_CULTURES, false, restoreNode.NodeClassName, false);
                }
                else
                {
                    nodeExists = true;
                }

                if (cultureNode != null)
                {
                    // Get localized version by GUID
                    var existingNode =
                        TreeProvider.SelectSingleNode(restoreNode.NodeGUID, restoreNode.DocumentCulture, site.SiteName, false) ??
                        TreeProvider.SelectSingleNode(site.SiteName, restoreNode.NodeAliasPath, restoreNode.DocumentCulture, false, restoreNode.NodeClassName, false);

                    // If culture node does not exists, insert new culture version
                    if ((existingNode == null) && string.Equals(cultureNode.NodeClassName, restoreNode.NodeClassName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        insertNode = false;

                        // Apply Tree data
                        var settings = new CopyNodeDataSettings(true, null)
                        {
                            CopyDocumentData = false,
                            CopyCoupledData = false,
                            CopyVersionedData = false,
                            CopySKUData = false,
                            ResetChanges = true
                        };
                        DocumentHelper.CopyNodeData(cultureNode, restoreNode, settings);

                        // Make complete node
                        restoreNode.MakeComplete(false);

                        // Insert new culture version, do not create new version
                        restoreNode.DocumentPublishedVersionHistoryID = 0;

                        // Clear last published time stamp
                        restoreNode.DocumentLastPublished = DateTime.MinValue;

                        var newCultureVersionSettings = new NewCultureDocumentSettings(restoreNode, null, TreeProvider)
                        {
                            CreateVersion = false,
                            AllowCheckOut = false,
                            ClearAttachmentFields = false,
                        };

                        DocumentHelper.InsertNewCultureVersion(newCultureVersionSettings);
                    }
                }

                // Insert the node regularly if previous action did not succeed
                if (insertNode)
                {
                    // Keep the original order of the document
                    bool originalAutomaticOrdering = TreeProvider.UseAutomaticOrdering;
                    TreeProvider.UseAutomaticOrdering = false;

                    // Check if parent node exists
                    var parentNode = TreeProvider.SelectSingleNode(site.SiteName, TreePathUtils.GetParentPath(restoreNode.NodeAliasPath), TreeProvider.ALL_CULTURES, false, null, false);
                    if (parentNode == null)
                    {
                        throw new InvalidOperationException("The node cannot be restored because its parent node no longer exists.");
                    }

                    // Insert the node with new GUID
                    if (nodeExists)
                    {
                        restoreNode.NodeGUID = Guid.NewGuid();
                    }

                    // Make complete node and insert the version
                    restoreNode.MakeComplete(false);
                    restoreNode.DocumentPublishedVersionHistoryID = 0;

                    // Clear last published time stamp
                    restoreNode.DocumentLastPublished = DateTime.MinValue;

                    restoreNode.ResetTranslationFlag();

                    restoreNode.Insert(parentNode);

                    TreeProvider.UseAutomaticOrdering = originalAutomaticOrdering;
                }

                int newDocumentId = Convert.ToInt32(restoreNode.GetValue("DocumentID"));

                // Change the document
                AttachmentHistoryInfoProvider.ChangeDocument(originalDocumentId, newDocumentId);
                VersionHistoryInfoProvider.ChangeDocument(originalDocumentId, newDocumentId);
                WorkflowHistoryInfoProvider.ChangeDocument(originalDocumentId, newDocumentId);

                // If document uses workflow, update the documentId within the versionHistory table
                var workflow = WorkflowManager.GetNodeWorkflow(restoreNode);
                if (workflow == null)
                {
                    PublishAttachments(versionHistoryId);

                    // Not using workflow, remove the version history data
                    DestroyDocumentVersion(versionHistoryId);

                    // Clear version history ID property to current instance of node
                    restoreNode.DocumentCheckedOutVersionHistoryID = 0;
                }
                else
                {
                    // Remove recycle bin flags
                    restoreVersion.DeletedByUserID = 0;
                    restoreVersion.DocumentID = restoreNode.DocumentID;
                    restoreVersion.SetValue("VersionDeletedWhen", null);

                    if (defaultValueEnsured)
                    {
                        restoreVersion.SetData(restoreNode);
                    }

                    VersionHistoryInfoProvider.SetVersionHistoryInfo(restoreVersion);

                    // Reset timeout
                    WorkflowManager.HandleStepTimeout(restoreNode, TreeProvider.UserInfo, workflow, null, restoreNode.WorkflowStep);
                }

                // Commit transaction if necessary
                tr.Commit();

                // Update search index for published document
                DocumentHelper.UpdateSearchIndexIfAllowed(restoreNode);

                if ((CMSString.Compare(restoreNode.NodeClassName, "cms.blogpost", true) == 0) && restoreNode.PublishedVersionExists)
                {
                    // Add activity point when blog post is published
                    BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.BlogPosts, restoreNode.NodeOwner, site.SiteName, true);
                }

                // Log synchronization task
                using (new CMSActionContext { EnableLogContext = false })
                {
                    DocumentSynchronizationHelper.LogDocumentChange(restoreNode, TaskTypeEnum.UpdateDocument, TreeProvider);
                }

                // Return the node
                return restoreNode;
            }
        }


        /// <summary>
        /// Returns new version number representation.
        /// </summary>
        /// <param name="oldVersionNumber">Old version number</param>
        /// <param name="isMajorVersion">If true, the version number is considered major</param>
        /// <param name="siteName">Site name</param>
        /// <param name="useCheckInCheckOut">Indicates if use check-in/check-out</param>
        protected virtual string GetNewVersionInternal(string oldVersionNumber, bool isMajorVersion, string siteName, bool useCheckInCheckOut)
        {
            if (useCheckInCheckOut && !UseAutomaticVersionNumbering(siteName))
            {
                return oldVersionNumber;
            }

            var currentVersionNumber = oldVersionNumber;
            if (string.IsNullOrEmpty(currentVersionNumber))
            {
                return isMajorVersion ? "1.0" : "0.1";
            }

            // Number already exists, get new version number
            try
            {
                // Get version number format
                var reg = RegexHelper.GetRegex(@"^\d+\.\d+$");
                if (!reg.IsMatch(currentVersionNumber))
                {
                    return oldVersionNumber;
                }

                // Get current version numbers
                var numbers = currentVersionNumber.Split('.');
                int majorVersion = ValidationHelper.GetInteger(numbers[0], 0);
                int minorVersion = ValidationHelper.GetInteger(numbers[1], 0);

                // Increase version number
                if (isMajorVersion)
                {
                    majorVersion += 1;
                    minorVersion = 0;
                }
                else
                {
                    minorVersion += 1;
                }

                return string.Format("{0}.{1}", majorVersion, minorVersion);
            }
            catch
            {
                // Version number cannot be parsed, return old version number
                return oldVersionNumber;
            }
        }


        /// <summary>
        /// Ensures that the document version history is present for the given document, returns the document version.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="nodeIsPublished">Document is published</param>
        /// <param name="nodeDeletion">Indicates if the version should be ensured for the recycle bin</param>
        /// <param name="forceWorkflow">Workflow instance to force to apply</param>
        /// <remarks>The document is updated to the database, there is no need to update it after</remarks>
        protected virtual int EnsureVersionInternal(TreeNode node, bool nodeIsPublished, bool nodeDeletion, WorkflowInfo forceWorkflow)
        {
            if (node == null)
            {
                return 0;
            }

            int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
            if (versionHistoryId > 0)
            {
                return versionHistoryId;
            }

            // Get the published node record from the database
            var currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);
            if (currentNode != null)
            {
                versionHistoryId = currentNode.DocumentCheckedOutVersionHistoryID;
            }

            // Get version from node record from database
            if (versionHistoryId > 0)
            {
                node.DocumentCheckedOutVersionHistoryID = versionHistoryId;
            }
            // Create new version
            else
            {
                // Prepare new version
                var version = new VersionHistoryInfo();

                version.DocumentID = node.DocumentID;
                version.VersionDocumentName = node.DocumentName;
                version.VersionDocumentType = node.DocumentType;
                version.VersionMenuRedirectUrl = node.DocumentMenuRedirectUrl;
                version.VersionClassID = node.GetValue("NodeClassID", 0);
                version.DocumentNamePath = node.DocumentNamePath;
                version.NodeSiteID = node.OriginalNodeSiteID;
                version.VersionNodeAliasPath = node.NodeAliasPath;
                version.ModifiedByUserID = TreeProvider.UserInfo.UserID;
                version.ModifiedWhen = DateTime.Now;
                version.SetData(null);
                version.ToBePublished = false;
                version.VersionComment = String.Empty;

                // Store additional information for recycle bin version
                if (nodeDeletion)
                {
                    version.DeletedByUserID = TreeProvider.UserInfo.UserID;
                    version.DeletedWhen = DateTime.Now;
                }

                // Handle version number and publish dates
                if (nodeIsPublished)
                {
                    // Set published version
                    version.VersionNumber = "1.0";
                    version.WasPublishedFrom = DateTime.Now;
                }
                else
                {
                    version.VersionNumber = "0.1";

                    // Publish from
                    DateTime publishFrom = node.DocumentPublishFrom;
                    if (publishFrom != DateTime.MinValue)
                    {
                        version.PublishFrom = publishFrom;
                        if (publishFrom < DateTime.Now)
                        {
                            version.WasPublishedFrom = publishFrom;
                        }
                    }

                    // Publish to
                    DateTime publishTo = node.DocumentPublishTo;
                    if (publishTo != DateTime.MaxValue)
                    {
                        version.PublishTo = publishTo;
                        if (publishTo < DateTime.Now)
                        {
                            version.WasPublishedTo = publishTo;
                        }
                    }
                }

                WorkflowStepInfo step = null;

                // Step already set
                if (node.DocumentWorkflowStepID > 0)
                {
                    step = WorkflowStepInfoProvider.GetWorkflowStepInfo(node.DocumentWorkflowStepID);
                }
                else
                {
                    // Get node workflow
                    if (forceWorkflow == null)
                    {
                        forceWorkflow = WorkflowManager.GetNodeWorkflow(node);
                    }

                    if (forceWorkflow != null)
                    {
                        int stepWorkflowId = forceWorkflow.WorkflowID;
                        step = nodeIsPublished ? WorkflowStepInfoProvider.GetPublishedStep(stepWorkflowId) : WorkflowStepInfoProvider.GetFirstStep(stepWorkflowId);
                    }
                }

                int stepId = 0;
                int workflowId = 0;
                if (step != null)
                {
                    stepId = step.StepID;
                    workflowId = step.StepWorkflowID;
                }

                // Set workflow
                version.VersionWorkflowID = workflowId;

                // Set workflow step
                version.VersionWorkflowStepID = stepId;

                // Save version
                VersionHistoryInfoProvider.SetVersionHistoryInfo(version);
                versionHistoryId = version.VersionHistoryID;

                // Update the node
                node.DocumentLastVersionNumber = version.VersionNumber;
                node.DocumentCheckedOutVersionHistoryID = versionHistoryId;
                node.DocumentWorkflowStepID = stepId;
                node.DocumentIsArchived = false;

                if (nodeIsPublished)
                {
                    node.DocumentPublishedVersionHistoryID = versionHistoryId;
                }

                // Get current document data
                version.SetData(node);

                VersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                // Do not update the document if it is being deleted, this data gets deleted anyway
                if (!nodeDeletion)
                {
                    using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                    {
                        // Update current node
                        if (currentNode != null)
                        {
                            currentNode.DocumentLastVersionNumber = version.VersionNumber;
                            currentNode.DocumentCheckedOutVersionHistoryID = versionHistoryId;
                            currentNode.DocumentWorkflowStepID = stepId;
                            currentNode.DocumentIsArchived = false;

                            if (nodeIsPublished)
                            {
                                currentNode.DocumentPublishedVersionHistoryID = versionHistoryId;
                            }

                            currentNode.Update();
                        }
                        else
                        {
                            node.Update();
                        }
                    }
                }

                EnsureVersionAttachments(versionHistoryId);

                // Reset timeout if not using check-in/out.
                // Do not perform for deleted document as the timeout task will be deleted anyway together with the document data
                if (!nodeDeletion && (forceWorkflow != null) && !forceWorkflow.UseCheckInCheckOut(node.NodeSiteName))
                {
                    WorkflowManager.HandleStepTimeout(node, TreeProvider.UserInfo, forceWorkflow, null, step);
                }
            }

            return versionHistoryId;
        }


        /// <summary>
        /// Ensures attachments for given version.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID.</param>
        private void EnsureVersionAttachments(int versionHistoryId)
        {
            if (versionHistoryId <= 0)
            {
                return;
            }

            new AttachmentHistoryCreator(versionHistoryId).Create();
        }


        /// <summary>
        /// Specifies whether the environment is configured to use check-in and check-out actions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected virtual bool UseCheckInCheckOutInternal(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseCheckinCheckout");
        }


        /// <summary>
        /// Specifies whether the environment is configured to use automatic version numbering.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected virtual bool UseAutomaticVersionNumberingInternal(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseAutomaticVersionNumbering");
        }


        /// <summary>
        /// Gets version history length
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected virtual int GetHistoryLengthInternal(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSVersionHistoryLength");
        }


        /// <summary>
        /// Returns true if the given column name is a versioned data column name.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="columnName">Column name</param>
        protected virtual bool IsVersionedCoupledColumnInternal(string className, string columnName)
        {
            string fullname = string.Format("{0}.{1}", className, columnName);

            // Check if the column is present
            return !DocumentColumnLists.NonVersionedCoupledColumns.Contains(fullname);
        }


        /// <summary>
        /// Returns true if the given column name is a versioned data column name.
        /// </summary>
        /// <param name="columnName">Column name</param>
        protected virtual bool IsVersionedDocumentColumnInternal(string columnName)
        {
            // System columns are never versioned
            if (IsSystemDocumentColumn(columnName))
            {
                return false;
            }

            return !DocumentColumnLists.NonVersionedDocumentColumns.Contains(columnName);
        }


        /// <summary>
        /// Returns true if the given column name is a system data column name of the node.
        /// </summary>
        /// <param name="columnName">Column name</param>
        protected virtual bool IsSystemNodeColumnInternal(string columnName)
        {
            return DocumentColumnLists.SystemNodeColumns.Contains(columnName);
        }


        /// <summary>
        /// Returns true if the given column name is a system data column name.
        /// </summary>
        /// <param name="columnName">Column name</param>
        protected virtual bool IsSystemDocumentColumnInternal(string columnName)
        {
            return DocumentColumnLists.SystemDocumentColumns.Contains(columnName);
        }


        /// <summary>
        /// Ensures the document consistency with current database content.
        /// </summary>
        /// <param name="node">Node to check</param>
        internal virtual void EnsureConsistencyInternal(TreeNode node)
        {
            // Check the page template
            EnsureTemplateConsistency(node, "DocumentPageTemplateID");
            EnsureTemplateConsistency(node, "NodeTemplateID");

            // Check workflow step
            int workflowStepId = node.DocumentWorkflowStepID;
            if (workflowStepId > 0)
            {
                var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(workflowStepId);
                if (step == null)
                {
                    node.DocumentWorkflowStepID = 0;
                    node.DocumentIsArchived = false;
                }
            }

            // Check users
            EnsureUserConsistency(node, "DocumentModifiedByUserID");
            EnsureUserConsistency(node, "DocumentCreatedByUserID");
            EnsureUserConsistency(node, "DocumentCheckedOutByUserID");
            EnsureUserConsistency(node, "NodeOwnerID");

            // Check stylesheet
            int stylesheetId = node.DocumentStylesheetID;
            if (stylesheetId > 0)
            {
                var style = CssStylesheetInfoProvider.GetCssStylesheetInfo(stylesheetId);
                if (style == null)
                {
                    node.DocumentStylesheetID = 0;
                }
            }
        }


        /// <summary>
        /// Ensures the consistency of the user ID column
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="colName">Column name with the user ID</param>
        private static void EnsureUserConsistency(TreeNode node, string colName)
        {
            int userId = node.GetValue(colName, 0);
            if (userId <= 0)
            {
                return;
            }

            var user = UserInfoProvider.GetUserInfo(userId);
            if (user == null)
            {
                node.SetValue(colName, null);
            }
        }


        /// <summary>
        /// Ensures the consistency of the template ID column
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="colName">Column name with the template ID</param>
        private static void EnsureTemplateConsistency(TreeNode node, string colName)
        {
            int templateId = node.GetValue(colName, 0);
            if (templateId <= 0)
            {
                return;
            }

            // Check if the page template exists
            var template = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
            if (template == null)
            {
                node.SetValue(colName, null);
            }
        }

        #endregion


        #region "Internal methods for attachment versioning"

        /// <summary>
        /// Removes the AttachmentHistory binding to the document version history and deletes the AttachmentHistory object if there is no more bindings to that version.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        protected virtual void RemoveAttachmentVersionInternal(int versionHistoryId, Guid attachmentGuid)
        {
            new AttachmentHistoryRemover(this, versionHistoryId, attachmentGuid).Remove();
        }


        /// <summary>
        /// Publishes the attachments for the given version.
        /// </summary>
        /// <param name="versionHistoryId">Version history for which the attachments should be published</param>
        protected virtual void PublishAttachmentsInternal(int versionHistoryId)
        {
            if (versionHistoryId == 0)
            {
                return;
            }

            var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
            if (version == null)
            {
                return;
            }

            new AttachmentHistoryPublisher(version).Publish();
        }


        /// <summary>
        /// Returns attachment version for latest version of a document.
        /// Searches only in main attachments. Attachment variants are omitted and if found, returns <c>null</c>.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Attachment site name</param>
        /// <param name="getBinary">Indicates if binary data should be included</param>
        protected virtual AttachmentHistoryInfo GetLatestAttachmentVersionInternal(Guid attachmentGuid, string siteName, bool getBinary)
        {
            var attachmentHistory = GetAttachmentVersionForLatestDocumentVersion(attachmentGuid, siteName)
                .BinaryData(getBinary)
                .FirstObject;

            return attachmentHistory;
        }


        /// <summary>
        /// Returns latest document version history ID for given attachment.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Attachment site name</param>
        internal int GetLatestVersionHistoryId(Guid attachmentGuid, string siteName)
        {
            return GetAttachmentVersionForLatestDocumentVersion(attachmentGuid, siteName)
                .Column("VersionHistoryID")
                .GetScalarResult<int>();
        }


        private ObjectQuery<AttachmentHistoryInfo> GetAttachmentVersionForLatestDocumentVersion(Guid attachmentGuid, string siteName)
        {
            return AttachmentHistoryInfoProvider.GetAttachmentHistories()
                                                .Source(sourceItem => sourceItem.Join<VersionAttachmentInfo>("AttachmentHistoryID", "AttachmentHistoryID"))
                                                .TopN(1)
                                                // Include all columns from attachment history table explicitly since there is a collision in joined table
                                                .Columns(string.Format("[{0}].*", AttachmentHistoryInfo.TYPEINFO.ClassStructureInfo.TableName), "VersionHistoryID")
                                                .WhereEquals("AttachmentGUID", attachmentGuid)
                                                .OnSite(siteName)
                                                .OrderByDescending("VersionHistoryID");
        }


        /// <summary>
        /// Returns attachment histories for the given version.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID.</param>
        /// <remarks>The IncludeBinaryData property and the BinaryData method don't load binary data 
        /// for attachments stored on the filesystem. To load binary data for attachments stored on the 
        /// filesystem, use the AttachmentBinary property of every record.</remarks>
        protected virtual ObjectQuery<AttachmentHistoryInfo> GetVersionAttachmentsInternal(int versionHistoryId)
        {
            return AttachmentHistoryInfoProvider.GetAttachmentHistories().InVersionExceptVariants(versionHistoryId);
        }


        /// <summary>
        /// Returns the attachment version.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="getBinary">Indicates if binary data should be included</param>
        protected virtual AttachmentHistoryInfo GetAttachmentVersionInternal(int versionHistoryId, Guid attachmentGuid, bool getBinary)
        {
            return AttachmentHistoryInfoProvider.GetAttachmentHistories()
                                                .AllInVersion(versionHistoryId)
                                                .BinaryData(getBinary)
                                                .WhereEquals("AttachmentGUID", attachmentGuid)
                                                .FirstObject;
        }


        /// <summary>
        /// Returns the attachment version.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="fileName">File name</param>
        /// <param name="getBinary">Indicates if binary data should be included</param>
        protected virtual AttachmentHistoryInfo GetAttachmentVersionInternal(int versionHistoryId, string fileName, bool getBinary)
        {
            return AttachmentHistoryInfoProvider.GetAttachmentHistories()
                                                .InVersionExceptVariants(versionHistoryId)
                                                .BinaryData(getBinary)
                                                .WhereEquals("AttachmentName", fileName)
                                                .FirstObject;
        }
        
        #endregion


        #region "Internal additional methods"

        /// <summary>
        /// Removes all the workflow information from the document and initializes the document as non-workflow.
        /// </summary>
        /// <param name="node">Document node (current version)</param>
        /// <remarks>Intended for use after finishing current workflow cycle to remove workflow scope from the document</remarks>
        protected virtual void RemoveWorkflowInternal(TreeNode node)
        {
            // Get node site name
            var site = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
            if (site == null)
            {
                throw new InvalidOperationException("Node site not found.");
            }

            if (node.DocumentCheckedOutVersionHistoryID != node.DocumentPublishedVersionHistoryID)
            {
                // Delete the previous version attachments
                AttachmentInfoProvider.DeleteAttachments(node.DocumentID);

                // Make sure current attachments are published
                PublishAttachments(node.DocumentCheckedOutVersionHistoryID);
            }

            // Get the node record from the database
            var currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);

            using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
            {
                // Update the document within main database to apply current data and remove the workflow information
                DocumentHelper.ClearWorkflowInformation(node);
                DocumentHelper.ClearWorkflowInformation(currentNode);
                currentNode.Update();
            }

            // Destroy history - non-versioned documents do not have any version history
            DestroyDocumentHistory(node.DocumentID);

            // Log synchronization
            DocumentHelper.LogDocumentChange(currentNode, TaskTypeEnum.UpdateDocument, TreeProvider);

            // Update search index for node
            if (SearchIndexInfoProvider.SearchEnabled && SearchHelper.SearchEnabledForClass(currentNode.NodeClassName))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, currentNode.GetSearchID(), node.DocumentID);
            }
        }


        /// <summary>
        /// Applies the document version data to the document DataSet.
        /// </summary>
        /// <param name="data">Dataset with the data</param>
        /// <param name="coupled">Indicates if versioned data should be applied to the coupled data</param>
        /// <param name="excludedColumns">Set of columns which should not be applied</param>
        protected virtual void ApplyVersionDataInternal(DataSet data, bool coupled, ISet<string> excludedColumns)
        {
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            // No version IDs found
            var ids = GetVersionHistoryIDs(data);
            if (ids.Count == 0)
            {
                return;
            }

            // No versions found
            var versions = VersionHistoryInfoProvider.GetVersionHistories(ids);
            if ((versions == null) || (versions.Count <= 0))
            {
                return;
            }

            foreach (DataTable table in data.Tables)
            {
                if (!table.Columns.Contains("DocumentCheckedOutVersionHistoryID"))
                {
                    continue;
                }

                // Get related versions
                foreach (DataRow row in table.Rows)
                {
                    var id = row["DocumentCheckedOutVersionHistoryID"].ToInteger(0);
                    if (id <= 0)
                    {
                        continue;
                    }

                    var version = versions[id] as VersionHistoryInfo;
                    var dc = new DataRowContainer(row);

                    ApplyVersionDataInternal(dc, version, coupled, excludedColumns);
                }
            }
        }


        /// <summary>
        /// Gets the version history IDs from the given data set
        /// </summary>
        /// <param name="data">Input data set</param>
        private static List<int> GetVersionHistoryIDs(DataSet data)
        {
            var ids = new List<int>();

            // Get version IDs
            foreach (DataTable table in data.Tables)
            {
                if (table.Columns.Contains("DocumentCheckedOutVersionHistoryID"))
                {
                    // Get related versions
                    ids.AddRange(DataHelper.GetIntegerValues(table, "DocumentCheckedOutVersionHistoryID"));
                }
            }

            return ids;
        }


        /// <summary>
        /// Corrects the values of automatically properties in data based on latest data from version data.
        /// </summary>
        /// <param name="container">Data container to which the version data is applied</param>
        /// <param name="versionContainer">Version data container</param>
        private static void CorrectAutomaticallyGeneratedProperties(IDataContainer container, IDataContainer versionContainer)
        {
            // Do not update fields which are intended as a source of document URL (NodeAlias, NodeAliasPath, automatic DocumentUrlPath),
            // because we need to make sure the generated URLs are valid in preview mode etc.

            var updateNodeName = container.ContainsColumn("NodeName");
            var updateDocumentName = container.ContainsColumn("DocumentName");
            var updateDocumentNamePath = container.ContainsColumn("DocumentNamePath");
            var updateDocumentUrlPath = container.ContainsColumn("DocumentUrlPath");
            if (!updateDocumentNamePath && !updateNodeName && !updateDocumentName && !updateDocumentUrlPath)
            {
                // There is no data to update
                return;
            }

            var siteName = GetSiteName(container, versionContainer);
            var versionDocumentName = GetVersionDocumentName(versionContainer);

            if (updateNodeName)
            {
                var cultureCode = ValidationHelper.GetString(container.GetValue("DocumentCulture"), string.Empty);
                CorrectContainerNodeName(container, cultureCode, versionDocumentName, siteName);
            }

            if (updateDocumentName)
            {
                CorrectContainerDocumentName(container, versionDocumentName);
            }

            if (updateDocumentNamePath)
            {
                CorrectContainerNamePath(container, versionDocumentName, siteName);
            }

            if (updateDocumentUrlPath)
            {
                CorrectContainerCustomUrlPath(container, siteName);
            }
        }


        private static string GetSiteName(IDataContainer container, IDataContainer versionContainer)
        {
            return SiteInfoProvider.GetSiteName(ValidationHelper.GetInteger(versionContainer.GetValue("NodeSiteID"), ValidationHelper.GetInteger(container.GetValue("NodeSiteID"), 0)));
        }


        private static string GetVersionDocumentName(IDataContainer versionContainer)
        {
            var documentName = ValidationHelper.GetString(versionContainer.GetValue("DocumentName"), string.Empty);
            var versionClassName = new DocumentClassNameRetriever(versionContainer, true).Retrieve();

            return TreePathUtils.EnsureMaxNodeNameLength(documentName, versionClassName);
        }


        private static void CorrectContainerDocumentName(IDataContainer container, string documentName)
        {
            container.SetValue("DocumentName", documentName);
        }


        private static void CorrectContainerNodeName(IDataContainer container, string cultureCode, string documentName, string siteName)
        {
            // Node name is automatically updated for documents in default site culture
            var defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);
            if (!string.Equals(cultureCode, defaultCulture, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            container.SetValue("NodeName", documentName);
        }


        private static void CorrectContainerCustomUrlPath(IDataContainer container, string siteName)
        {
            // There is no need to validate the user input when custom URL path is not used
            var useNamePathForUrlPath = ValidationHelper.GetBoolean(container.GetValue("DocumentUseNamePathForUrlPath"), false);
            if (useNamePathForUrlPath)
            {
                return;
            }

            container.SetValue("DocumentUrlPath", TreePathUtils.GetSafeUrlPath(ValidationHelper.GetString(container.GetValue("DocumentUrlPath"), string.Empty), siteName));
        }


        private static void CorrectContainerNamePath(IDataContainer container, string documentName, string siteName)
        {
            var documentNamePath = ValidationHelper.GetString(container.GetValue("DocumentNamePath"), string.Empty);
            var parentDocumentNamePath = TreePathUtils.GetParentPath(documentNamePath).TrimEnd('/');

            container.SetValue("DocumentNamePath", string.Join("/", parentDocumentNamePath, TreePathUtils.GetSafeDocumentName(documentName, siteName)));
        }


        /// <summary>
        /// Applies the document version data to the document data row.
        /// </summary>
        /// <param name="container">Data container</param>
        /// <param name="version">Document version</param>
        /// <param name="coupled">Indicates if versioned data should be applied to the coupled data</param>
        /// <param name="excludedColumns">Set of columns which should not be applied</param>
        protected virtual void ApplyVersionDataInternal(IDataContainer container, VersionHistoryInfo version, bool coupled, ISet<string> excludedColumns)
        {
            if (container == null)
            {
                return;
            }

            if (version == null)
            {
                return;
            }

            var versionData = version.Data;
            if (versionData == null)
            {
                throw new InvalidOperationException("Missing page version data.");
            }

            ApplyDocumentData(container, versionData, excludedColumns);

            if (coupled)
            {
                ApplyCoupledData(container, versionData, excludedColumns);
            }
            else
            {
                ApplySKUData(container, versionData, excludedColumns);
            }

            CorrectAutomaticallyGeneratedProperties(container, versionData);
        }


        /// <summary>
        /// Applies coupled versioned data including SKU data
        /// </summary>
        /// <param name="container">Data container</param>
        /// <param name="versionContainer">Version container</param>
        /// <param name="excludedColumns">Set of columns which should not be applied</param>
        private static void ApplyCoupledData(IDataContainer container, IDataContainer versionContainer, ISet<string> excludedColumns)
        {
            // Get class name from data container
            var className = new DocumentClassNameRetriever(container, true).Retrieve();

            var type = DataClassInfoProvider.GetDataClassInfo(className);

            // Copy the coupled data if present
            if (type.ClassIsCoupledClass)
            {
                var coupledStructure = ClassStructureInfo.GetClassInfo(className);

                ApplyVersionColumns(
                    container,
                    versionContainer,
                    coupledStructure,
                    col => IsVersionedCoupledColumn(className, col) && !IsExcludedColumn(col, excludedColumns)
                );
            }

            // Copy SKU data if present
            if (type.ClassIsProduct)
            {
                ApplySKUData(container, versionContainer, excludedColumns);
            }
        }


        /// <summary>
        /// Applies SKU versioned data
        /// </summary>
        /// <param name="container">Data container</param>
        /// <param name="versionContainer">Version container</param>
        /// <param name="excludedColumns">Set of columns which should not be applied</param>
        private static void ApplySKUData(IDataContainer container, IDataContainer versionContainer, ISet<string> excludedColumns)
        {
            // Apply data only if part of the container
            if (!container.ContainsColumn("SKUID") || container.GetValue("NodeSKUID").ToInteger(0) <= 0)
            {
                return;
            }

            var sku = ClassStructureInfo.GetClassInfo("ecommerce.sku");

            ApplyVersionColumns(
                container,
                versionContainer,
                sku,
                col => IsVersionedCoupledColumn("ecommerce.sku", col) && !IsExcludedColumn(col, excludedColumns)
            );

            // Refresh document name after applying SKU data (especially for TreeNode instance)
            if (container.ContainsColumn("DocumentName"))
            {
                container.SetValue("DocumentName", versionContainer["DocumentName"]);
            }
        }


        /// <summary>
        /// Applies document versioned data
        /// </summary>
        /// <param name="container">Data container</param>
        /// <param name="versionContainer">Version data container</param>
        /// <param name="excludedColumns">Set of columns which should not be applied</param>
        private static void ApplyDocumentData(IDataContainer container, IDataContainer versionContainer, ISet<string> excludedColumns)
        {
            var doc = ClassStructureInfo.GetClassInfo("cms.document");

            ApplyVersionColumns(
                container,
                versionContainer,
                doc,
                col => IsVersionedDocumentColumn(col) && !IsExcludedColumn(col, excludedColumns)
            );
        }


        /// <summary>
        /// Applies the given list of columns from the source data row to the node data row
        /// </summary>
        /// <param name="targetContainer">Taret data container</param>
        /// <param name="sourceContainer">Source data container</param>
        /// <param name="structure">Class structure</param>
        /// <param name="columnCondition">Column condition</param>
        private static void ApplyVersionColumns(IDataContainer targetContainer, IDataContainer sourceContainer, ClassStructureInfo structure, Func<string, bool> columnCondition)
        {
            if (structure == null)
            {
                // No structure info, do not apply versioned data
                return;
            }

            var className = structure.ClassName;
            var form = FormHelper.GetFormInfo(className, false);

            var targetColumns = new HashSet<string>(targetContainer.ColumnNames, StringComparer.InvariantCultureIgnoreCase);
            var sourceColumns = sourceContainer.ColumnNames;

            var idColumnName = structure.IDColumn;

            // Process all given columns
            foreach (var column in structure.ColumnDefinitions)
            {
                var columnName = column.ColumnName;

                // Check if the column is excluded
                if ((columnCondition != null) && !columnCondition(columnName))
                {
                    continue;
                }

                // Skip ID column (never versioned)
                if (columnName.Equals(idColumnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                // If both tables contain given column, apply the data
                if (!targetColumns.Contains(columnName))
                {
                    continue;
                }

                if (sourceColumns.Contains(columnName))
                {
                    // Handle the data type
                    var sourceValue = sourceContainer[columnName];

                    object targetValue = sourceValue != DBNull.Value ? DataHelper.ConvertValue(sourceValue, column.ColumnType) : sourceValue;

                    // Update column value
                    targetContainer.SetValue(columnName, targetValue);
                }
                else
                {
                    // Get field info
                    var field = form.GetFormField(columnName);
                    if ((field == null) || field.AllowEmpty)
                    {
                        // Set empty value
                        targetContainer.SetValue(columnName, DBNull.Value);
                    }
                }
            }
        }


        #endregion
    }
}