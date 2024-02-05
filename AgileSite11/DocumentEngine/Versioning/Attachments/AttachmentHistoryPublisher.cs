using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Publishes attachments of a document version.
    /// </summary>
    internal class AttachmentHistoryPublisher
    {
        private VersionHistoryInfo Version
        {
            get;
            set;
        }


        private readonly Dictionary<int, int> processedAttachments = new Dictionary<int, int>();


        /// <summary>
        /// Creates instance of <see cref="AttachmentHistoryPublisher"/>.
        /// </summary>
        /// <param name="version">Version for which the attachments should be published.</param>
        public AttachmentHistoryPublisher(VersionHistoryInfo version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            Version = version;
        }


        /// <summary>
        /// Publishes the attachments of the given version.
        /// </summary>
        public void Publish()
        {
            PublishMainAttachments();
            PublishAttachmentVariants();
        }


        private void PublishMainAttachments()
        {
            var attachments = AttachmentHistoryInfoProvider.GetAttachmentHistories().InVersionExceptVariants(Version.VersionHistoryID);
            foreach (var versionAttachment in attachments)
            {
                var attachment = PublishAttachmentData(versionAttachment);
                processedAttachments.Add(versionAttachment.AttachmentHistoryID, attachment.AttachmentID);
            }
        }


        private AttachmentInfo PublishAttachmentData(AttachmentHistoryInfo versionAttachment)
        {
            var attachment = versionAttachment.ConvertToAttachment();
            BindAttachmentToExistingAttachment(attachment);
            UpdateAttachmentFieldsFromVersion(attachment);
            ConvertParentVariantId(attachment);

            AttachmentInfoProvider.SetAttachmentInfo(attachment);

            return attachment;
        }


        private void PublishAttachmentVariants()
        {
            var attachmentVariants = AttachmentHistoryInfoProvider.GetAttachmentHistories().VariantsForAttachments(processedAttachments.Keys.ToArray());
            foreach (var attachmentVariant in attachmentVariants)
            {
                PublishAttachmentData(attachmentVariant);
            }
        }


        private void UpdateAttachmentFieldsFromVersion(AttachmentInfo attachment)
        {
            attachment.AttachmentDocumentID = Version.DocumentID;
            attachment.AttachmentSiteID = Version.NodeSiteID;
        }


        private void BindAttachmentToExistingAttachment(AttachmentInfo attachment)
        {
            var existingAttachmentId = GetExistingAttachmentId(attachment.AttachmentGUID);
            if (existingAttachmentId <= 0)
            {
                return;
            }

            attachment.AttachmentID = existingAttachmentId;
            attachment.MakeComplete(false);
        }


        private int GetExistingAttachmentId(Guid attachmentGuid)
        {
            var existingAttachmentId = AttachmentInfoProvider.GetAttachments()
                                                             .TopN(1)
                                                             .Column("AttachmentID")
                                                             .BinaryData(false)
                                                             .WhereEquals("AttachmentDocumentID", Version.DocumentID)
                                                             .WhereEquals("AttachmentGUID", attachmentGuid)
                                                             .GetScalarResult<int>();
            return existingAttachmentId;
        }


        private void ConvertParentVariantId(AttachmentInfo attachment)
        {
            if (attachment.AttachmentVariantParentID <= 0)
            {
                return;
            }

            attachment.AttachmentVariantParentID = processedAttachments[attachment.AttachmentVariantParentID];
        }
    }
}