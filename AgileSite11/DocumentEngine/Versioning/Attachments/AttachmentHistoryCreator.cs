using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Creates attachment histories for a new version from all document published attachments.
    /// </summary>
    internal class AttachmentHistoryCreator
    {
        private VersionHistoryInfo Version
        {
            get;
            set;
        }


        private readonly IDictionary<int, int> processedAttachments = new Dictionary<int, int>();


        /// <summary>
        /// Creates instance of <see cref="AttachmentHistoryCreator"/>.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID.</param>
        public AttachmentHistoryCreator(int versionHistoryId)
        {
            if (versionHistoryId <= 0)
            {
                throw new ArgumentOutOfRangeException("versionHistoryId", "Version history ID not provided.");
            }

            var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
            if (version == null)
            {
                throw new NullReferenceException("Missing document version.");
            }

            Version = version;
        }


        /// <summary>
        /// Creates attachments for given document version from all document published attachments.
        /// </summary>
        public void Create()
        {
            CreateMainAttachments();
            CreateAttachmentVariants();
        }


        private void CreateMainAttachments()
        {
            var attachments = AttachmentInfoProvider.GetAttachments(Version.DocumentID, true)
                                                    .ExceptVariants();

            foreach (var attachment in attachments)
            {
                CreateAttachment(attachment);
            }
        }


        private void CreateAttachmentVariants()
        {
            var variants = AttachmentInfoProvider.GetAttachments(Version.DocumentID, true)
                                                 .VariantsForAttachments(processedAttachments.Keys.ToArray());

            foreach (var variant in variants)
            {
                CreateVariant(variant);
            }
        }


        private void CreateAttachment(AttachmentInfo attachment)
        {
            var attachmentHistory = new AttachmentHistoryInfo();
            attachmentHistory.ApplyData((DocumentAttachment)attachment);

            AttachmentHistoryInfoProvider.SetAttachmentHistory(attachmentHistory);
            VersionAttachmentInfoProvider.SetVersionAttachmentInfo(Version.VersionHistoryID, attachmentHistory.AttachmentHistoryID);

            processedAttachments.Add(attachment.AttachmentID, attachmentHistory.AttachmentHistoryID);
        }


        private void CreateVariant(AttachmentInfo attachment)
        {
            var attachmentHistory = new AttachmentHistoryInfo();
            attachmentHistory.ApplyData((DocumentAttachment)attachment);

            ConvertVariantParentId(attachment, attachmentHistory);
            AttachmentHistoryInfoProvider.SetAttachmentHistory(attachmentHistory);
        }


        private void ConvertVariantParentId(AttachmentInfo attachment, AttachmentHistoryInfo attachmentHistory)
        {
            if (attachment.AttachmentVariantParentID > 0)
            {
                attachmentHistory.AttachmentVariantParentID = processedAttachments[attachment.AttachmentVariantParentID];
            }
        }
    }
}