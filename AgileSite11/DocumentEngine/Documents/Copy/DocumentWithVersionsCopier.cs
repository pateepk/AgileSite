using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Copies document data including version data.
    /// </summary>
    internal class DocumentWithVersionsCopier : DocumentCopier
    {
        /// <summary>
        /// Creates instance of <see cref="DocumentWithVersionsCopier"/>.
        /// </summary>
        /// <param name="settings">Copy document settings.</param>
        public DocumentWithVersionsCopier(CopyDocumentSettings settings)
            : base(settings)
        {

        }


        protected override void CopyNode(CopyDocumentSettings settings)
        {
            new DocumentWithVersionsCopier(settings).Copy();
        }


        protected override TreeNode CopyLink()
        {
            var link = base.CopyLink();
            LogStagingTask(link);

            return link;
        }


        private void LogStagingTask(TreeNode document)
        {
            DocumentHelper.LogDocumentChange(document, TaskTypeEnum.CreateDocument, Settings.Tree);
        }


        protected override TreeNode CopyCultureVersion(TreeNode sourceCultureNode, TreeNode firstCopiedCultureNode)
        {
            var targetCultureNode = base.CopyCultureVersion(sourceCultureNode, firstCopiedCultureNode);
            targetCultureNode = CopyDocumentVersion(sourceCultureNode, targetCultureNode);
            LogStagingTask(targetCultureNode);

            return targetCultureNode;
        }


        private TreeNode CopyDocumentVersion(TreeNode sourceCultureNode, TreeNode targetCultureNode)
        {
            if (sourceCultureNode.DocumentCheckedOutVersionHistoryID <= 0)
            {
                return targetCultureNode;
            }

            var targetVersion = InitializeTargetVersion(sourceCultureNode, targetCultureNode);
            var targetVersionNode = InitializeTargetVersionNode(targetCultureNode, targetVersion);

            SaveTargetVersion(targetVersion, targetVersionNode);

            UpdateVersionIDsInPublishedData(targetCultureNode, targetVersion, sourceCultureNode);
            UpdateVersionIDsInVersionData(targetVersionNode, targetVersion, sourceCultureNode);

            if (Settings.CopyAttachments)
            {
                CopyAttachmentsVersions(sourceCultureNode, targetVersionNode);
            }

            if (targetVersionNode.IsCoupled)
            {
                EnsureVersionAttachmentFieldValues(targetVersionNode);
            }

            SaveTargetVersionNode(targetVersionNode);

            return targetVersionNode;
        }


        private static void SaveTargetVersion(VersionHistoryInfo targetVersion, TreeNode targetVersionNode)
        {
            UpdateTargetVersionData(targetVersion, targetVersionNode);
            VersionHistoryInfoProvider.SetVersionHistoryInfo(targetVersion);
        }


        private void SaveTargetVersionNode(TreeNode targetVersionNode)
        {
            var manager = VersionManager.GetInstance(Settings.Tree);
            manager.SaveVersion(targetVersionNode);
        }


        private static void UpdateTargetVersionData(VersionHistoryInfo targetVersion, TreeNode targetVersionNode)
        {
            targetVersion.SetData(targetVersionNode);
        }


        private TreeNode InitializeTargetVersionNode(TreeNode targetCultureNode, VersionHistoryInfo targetVersion)
        {
            var targetVersionNode = targetCultureNode.Clone();
            var manager =  VersionManager.GetInstance(Settings.Tree);
            manager.ApplyVersion(targetVersionNode, targetVersion);

            return targetVersionNode;
        }


        private VersionHistoryInfo InitializeTargetVersion(TreeNode sourceCultureNode, TreeNode targetCultureNode)
        {
            var sourceVersion = VersionHistoryInfoProvider.GetVersionHistoryInfo(sourceCultureNode.DocumentCheckedOutVersionHistoryID);
            var targetVersion = sourceVersion.Clone(true);

            targetVersion.DocumentID = targetCultureNode.DocumentID;
            targetVersion.DocumentNamePath = targetCultureNode.DocumentNamePath;
            targetVersion.VersionNodeAliasPath = targetCultureNode.NodeAliasPath;
            targetVersion.VersionDocumentName = targetCultureNode.DocumentName;
            targetVersion.VersionDocumentType = targetCultureNode.DocumentType;
            targetVersion.VersionMenuRedirectUrl = targetCultureNode.DocumentMenuRedirectUrl;
            targetVersion.VersionClassID = targetCultureNode.GetValue("NodeClassID", 0);
            targetVersion.ModifiedByUserID = Settings.Tree.UserInfo.UserID;
            targetVersion.ModifiedWhen = DateTime.Now;
            targetVersion.NodeSiteID = targetCultureNode.OriginalNodeSiteID;

            return targetVersion;
        }


        private void EnsureVersionAttachmentFieldValues(ITreeNode targetVersionNode)
        {
            GetAttachmentFieldNames(targetVersionNode).ForEach(fieldName =>
            {
                var sourceValue = ValidationHelper.GetGuid(targetVersionNode.GetValue(fieldName), Guid.Empty);
                if (sourceValue == Guid.Empty)
                {
                    return;
                }

                var newValue = Settings.CopyAttachments ? AttachmentGUIDs[sourceValue] : Guid.Empty;
                targetVersionNode.SetValue(fieldName, newValue);
            });
        }


        private List<string> GetAttachmentFieldNames(ITreeNode targetCultureNode)
        {
            var form = FormHelper.GetFormInfo(targetCultureNode.ClassName, false);
            return form.GetFields(FieldDataType.File).Select(field => field.Name).ToList();
        }


        private static void UpdateVersionIDsInVersionData(TreeNode versionNode, VersionHistoryInfo targetVersion, TreeNode sourceCultureNode)
        {
            versionNode.DocumentCheckedOutVersionHistoryID = targetVersion.VersionHistoryID;
            versionNode.DocumentPublishedVersionHistoryID = GetNewPublishedVersionHistoryId(sourceCultureNode, targetVersion);
        }


        private static int GetNewPublishedVersionHistoryId(TreeNode sourceCultureNode, VersionHistoryInfo targetVersion)
        {
            return sourceCultureNode.DocumentCheckedOutVersionHistoryID == sourceCultureNode.DocumentPublishedVersionHistoryID ? targetVersion.VersionHistoryID : 0;
        }


        private static void UpdateVersionIDsInPublishedData(TreeNode targetCultureNode, VersionHistoryInfo targetVersion, TreeNode sourceCultureNode)
        {
            var targetCultureData = DocumentCultureDataInfoProvider.GetDocumentCultureInfo(targetCultureNode.DocumentID);

            targetCultureData.DocumentCheckedOutVersionHistoryID = targetVersion.VersionHistoryID;
            targetCultureData.DocumentPublishedVersionHistoryID = GetNewPublishedVersionHistoryId(sourceCultureNode, targetVersion);

            targetCultureData.Update();
        }


        private void CopyAttachmentsVersions(TreeNode sourceCultureNode, TreeNode targetCultureNode)
        {
            new AttachmentHistoryCopier(sourceCultureNode.DocumentCheckedOutVersionHistoryID, targetCultureNode.DocumentCheckedOutVersionHistoryID, AttachmentGUIDs).Copy();
        }
    }
}