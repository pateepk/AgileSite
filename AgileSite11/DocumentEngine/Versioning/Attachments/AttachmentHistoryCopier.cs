using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Copies attachment histories between versions.
    /// </summary>
    internal class AttachmentHistoryCopier
    {
        private int SourceVersionHistoryID
        {
            get;
            set;
        }

        private VersionHistoryInfo TargetVersion
        {
            get;
            set;
        }


        private IDictionary<Guid, Guid> AttachmentGUIDs
        {
            get;
            set;
        }


        private readonly IDictionary<int, int> processedAttachments = new Dictionary<int, int>();


        /// <summary>
        /// Creates instance of <see cref="AttachmentHistoryCopier"/>.
        /// </summary>
        /// <param name="sourceVersionHistoryId">Source version history ID.</param>
        /// <param name="targetVersionHistoryId">Target version history ID.</param>
        /// <param name="guidTable">Attachment GUID conversion dictionary, pairs [OldGUID -> NewGUID].</param>
        public AttachmentHistoryCopier(int sourceVersionHistoryId, int targetVersionHistoryId, IDictionary<Guid, Guid> guidTable)
        {
            if (sourceVersionHistoryId <= 0)
            {
                throw new ArgumentOutOfRangeException("sourceVersionHistoryId", "Source version history ID not provided.");
            }

            TargetVersion = VersionHistoryInfoProvider.GetVersionHistoryInfo(targetVersionHistoryId);
            if (TargetVersion == null)
            {
                throw new NullReferenceException("Source version history not found.");
            }

            if (guidTable == null)
            {
                throw new ArgumentNullException("guidTable");
            }

            AttachmentGUIDs = guidTable;
            SourceVersionHistoryID = sourceVersionHistoryId;
        }


        /// <summary>
        /// Copies the version attachments to the given version, physically copies the attachment history records.
        /// </summary>
        public void Copy()
        {
            CopyVersionMainAttachments();
            CopyVersionAttachmentVariants();
        }


        private void CopyVersionMainAttachments()
        {
            var attachments = AttachmentHistoryInfoProvider.GetAttachmentHistories().InVersionExceptVariants(SourceVersionHistoryID);
            foreach (var attachment in attachments)
            {
                CopyVersionAttachment(attachment);
            }
        }


        private void CopyVersionAttachment(AttachmentHistoryInfo attachment)
        {
            var sourceAttachmentId = attachment.AttachmentHistoryID;
            CopyAttachmentData(attachment);
            processedAttachments.Add(sourceAttachmentId, attachment.AttachmentHistoryID);
            CreateVersionBinding(attachment);
        }


        private void CreateVersionBinding(AttachmentHistoryInfo attachment)
        {
            VersionAttachmentInfoProvider.SetVersionAttachmentInfo(TargetVersion.VersionHistoryID, attachment.AttachmentHistoryID);
        }


        private void CopyAttachmentData(AttachmentHistoryInfo attachment)
        {
            // Ensure loading of binary data, it may be stored on file system.
            // This has to be done before changing attachment document so that the data is picked up from the right location.
            attachment.AttachmentBinary = attachment.AttachmentBinary;

            // Update the attachment to the new document
            attachment.AttachmentDocumentID = TargetVersion.DocumentID;
            attachment.AttachmentSiteID = TargetVersion.NodeSiteID;

            Guid originalGuid = attachment.AttachmentGUID;
            if (originalGuid != Guid.Empty)
            {
                // Create new GUID if not present
                if (!AttachmentGUIDs.ContainsKey(originalGuid))
                {
                    AttachmentGUIDs[originalGuid] = Guid.NewGuid();
                }
                attachment.AttachmentGUID = AttachmentGUIDs[originalGuid];
            }

            attachment.Insert();
        }


        private void CopyVersionAttachmentVariants()
        {
            var attachmentVariants = AttachmentHistoryInfoProvider.GetAttachmentHistories().VariantsForAttachments(processedAttachments.Keys.ToArray());
            foreach (var attachmentVariant in attachmentVariants)
            {
                attachmentVariant.AttachmentVariantParentID = processedAttachments[attachmentVariant.AttachmentVariantParentID];
                CopyAttachmentData(attachmentVariant);
            }
        }
    }
}