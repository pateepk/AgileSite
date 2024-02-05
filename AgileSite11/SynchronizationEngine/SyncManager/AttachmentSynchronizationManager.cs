using System;
using System.Data;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.Synchronization
{
    /// <summary>
    /// Internal part of SyncManager responsible for attachment handling
    /// </summary>
    internal class AttachmentSynchronizationManager
    {
        private const string GUID_COLUMN = "AttachmentGUID";

        private readonly TreeNode CurrentTreeNode;
        private readonly TranslationHelper CurrentTranslationHelper;


        /// <summary>
        /// Creates an instance of <see cref="AttachmentSynchronizationManager"/>.
        /// </summary>
        /// <param name="node">Processed attachments will be added to this document.</param>
        /// <param name="translationHelper">Translation helper used in current synchronization task.</param>
        public AttachmentSynchronizationManager(TreeNode node, TranslationHelper translationHelper)
        {
            CurrentTreeNode = node;
            CurrentTranslationHelper = translationHelper;
        }


        /// <summary>
        /// Run synchronization of attachment from <paramref name="taskData"/>
        /// </summary>
        /// <param name="taskData">Data pro synchronization task.</param>
        /// <param name="sourceDocumentId">Identifier of document which is currently processed.</param>
        public void Synchronize(DataSet taskData, int sourceDocumentId)
        {
            DataTable attachmentsFromTask = GetAttachmentsTable(taskData);

            if (attachmentsFromTask != null)
            {
                UpdateAttachments(sourceDocumentId, attachmentsFromTask);
            }
            else
            {
                AttachmentInfoProvider.DeleteAttachments(CurrentTreeNode.DocumentID);
            }
        }


        private void UpdateAttachments(int sourceDocumentId, DataTable attachmentsFromTask)
        {
            DataTable targetAttachmentsTable = null;

            // Get attachments on target
            DataSet currentAttachmentsDS = AttachmentInfoProvider.GetAttachments(CurrentTreeNode.DocumentID, true);

            if (!DataHelper.DataSourceIsEmpty(currentAttachmentsDS))
            {
                targetAttachmentsTable = currentAttachmentsDS.Tables[0];
            }

            var mainWhere = new WhereCondition()
                .WhereEquals("AttachmentDocumentID", sourceDocumentId)
                .WhereNull("AttachmentVariantParentID");

            var variantsWhere = new WhereCondition()
                .WhereEquals("AttachmentDocumentID", sourceDocumentId)
                .WhereNotNull("AttachmentVariantParentID");

            DataRow[] mainAttachmentsFromTask = attachmentsFromTask.Select(mainWhere.ToString(true));
            DataRow[] attachmentVariantsFromTask = attachmentsFromTask.Select(variantsWhere.ToString(true));

            ProcessMainAttachments(mainAttachmentsFromTask, targetAttachmentsTable);
            ProcessAttachmentsVariants(attachmentVariantsFromTask, targetAttachmentsTable);

            DeleteAttachmentsLeavingOnTarget(targetAttachmentsTable);
        }


        private void ProcessAttachmentsVariants(DataRow[] attachmentVariants, DataTable targetAttachmentsTable)
        {
            if (attachmentVariants.Length <= 0)
            {
                return;
            }

            foreach (DataRow variant in attachmentVariants)
            {
                ProcessAttachment(variant, targetAttachmentsTable);
            }
        }


        private void ProcessMainAttachments(DataRow[] mainAttachmentsFromTask, DataTable targetAttachmentsTable)
        {
            if (mainAttachmentsFromTask.Length <= 0)
            {
                return;
            }

            foreach (DataRow attachment in mainAttachmentsFromTask)
            {
                var newAttachment = ProcessAttachment(attachment, targetAttachmentsTable);

                if (newAttachment != null)
                {
                    var originalAttachmentId = ValidationHelper.GetInteger(attachment["AttachmentID"], 0);

                    CurrentTranslationHelper.AddIDTranslation(
                        AttachmentInfo.OBJECT_TYPE,
                        originalAttachmentId,
                        newAttachment.AttachmentID,
                        0);
                }
            }
        }


        private void DeleteAttachmentsLeavingOnTarget(DataTable targetAttachmentsTable)
        {
            if (targetAttachmentsTable == null)
            {
                return;
            }

            foreach (DataRow dr in targetAttachmentsTable.Rows)
            {
                int attachmentId = ValidationHelper.GetInteger(dr["AttachmentID"], 0);
                if (attachmentId > 0)
                {
                    AttachmentInfoProvider.DeleteAttachmentInfo(attachmentId);
                }
            }
        }


        private AttachmentInfo ProcessAttachment(DataRow attachmentFromTask, DataTable currentAttachmentsTable)
        {
            // Get current attachment
            var attachmentGuid = ValidationHelper.GetGuid(attachmentFromTask["AttachmentGUID"], Guid.Empty);
            if (attachmentGuid == Guid.Empty)
            {
                return null;
            }


            DataRow currentAttachment = null;
            if (currentAttachmentsTable != null)
            {
                DataRow[] currentAttachments = currentAttachmentsTable.Select("AttachmentGUID = '" + attachmentGuid + "'");
                if (currentAttachments.Length > 0)
                {
                    currentAttachment = currentAttachments[0];
                }
            }

            var newAttachment = PrepareAttachmentInstance(attachmentFromTask, currentAttachmentsTable, currentAttachment);

            AttachmentInfoProvider.SetAttachmentInfo(newAttachment);
            return newAttachment;

        }


        private AttachmentInfo PrepareAttachmentInstance(DataRow sourceAttachmentRow, DataTable currentAttachmentsTable, DataRow currentAttachment)
        {
            // Create new attachment
            var newAttachment = new AttachmentInfo(sourceAttachmentRow);

            // Reset object ID so that it behaves as external object and does full update
            newAttachment.AttachmentID = 0;

            if (currentAttachment != null)
            {
                newAttachment.AttachmentID = ValidationHelper.GetInteger(currentAttachment["AttachmentID"], 0);

                // Remove current attachment row to mark as processed
                currentAttachmentsTable.Rows.Remove(currentAttachment);
            }

            newAttachment.AttachmentDocumentID = CurrentTreeNode.DocumentID;
            newAttachment.AttachmentSiteID = CurrentTreeNode.OriginalNodeSiteID;

            if (newAttachment.AttachmentVariantParentID > 0)
            {
                newAttachment.AttachmentVariantParentID = CurrentTranslationHelper.GetNewID(
                    AttachmentInfo.OBJECT_TYPE,
                    newAttachment.AttachmentVariantParentID,
                    GUID_COLUMN,
                    newAttachment.AttachmentSiteID,
                    null,
                    null,
                    null
                );
            }
            return newAttachment;
        }


        private DataTable GetAttachmentsTable(DataSet documentDS)
        {
            return documentDS.Tables["CMS_Attachment"];
        }
    }
}