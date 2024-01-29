using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Removes attachment from a document version.
    /// </summary>
    internal class AttachmentHistoryRemover
    {
        private AttachmentHistoryInfo AttachmentVersion
        {
            get;
            set;
        }


        private int VersionHistoryID
        {
            get;
            set;
        }


        /// <summary>
        /// Creates instance of <see cref="AttachmentHistoryRemover"/>.
        /// </summary>
        /// <param name="attachmentVersion">Attachment history version.</param>
        /// <param name="versionHistoryId">Version history ID.</param>
        public AttachmentHistoryRemover(AttachmentHistoryInfo attachmentVersion, int versionHistoryId)
        {
            if (attachmentVersion == null)
            {
                throw new ArgumentNullException("attachmentVersion");
            }

            if (attachmentVersion.IsVariant())
            {
                throw new InvalidOperationException("Removing attachment variant from a version is not supported. Use AttachmentHistoryInfoProvider to handle attachment variants data.");
            }

            AttachmentVersion = attachmentVersion;
            VersionHistoryID = versionHistoryId;
        }


        /// <summary>
        /// Creates instance of <see cref="AttachmentHistoryRemover"/>.
        /// </summary>
        /// <param name="manager">Version manager.</param>
        /// <param name="versionHistoryId">Version history ID.</param>
        /// <param name="attachmentGuid">GUID of the attachment version which should be removed.</param>
        public AttachmentHistoryRemover(VersionManager manager, int versionHistoryId, Guid attachmentGuid)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            AttachmentVersion = manager.GetAttachmentVersion(versionHistoryId, attachmentGuid, false);
            if (AttachmentVersion == null)
            {
                throw new NullReferenceException("Attachment history version not found.");
            }

            if (AttachmentVersion.IsVariant())
            {
                throw new InvalidOperationException("Removing attachment variant from a version is not supported. Use AttachmentHistoryInfoProvider to handle attachment variants data.");
            }

            VersionHistoryID = versionHistoryId;
        }


        /// <summary>
        /// Removes the attachment history from version.
        /// </summary>
        public void Remove()
        {
            VersionAttachmentInfoProvider.DeleteVersionAttachmentInfo(VersionHistoryID, AttachmentVersion.AttachmentHistoryID);

            // Check if there are some more version bindings for this attachment
            var count = GetNumberOfVersionsUsingAttachment();
            if (count == 0)
            {
                // Delete the attachment if not used by other versions
                AttachmentVersion.Delete();
            }
        }
        

        private int GetNumberOfVersionsUsingAttachment()
        {
            return VersionAttachmentInfoProvider.GetVersionAttachments()
                                                .WhereEquals("AttachmentHistoryID", AttachmentVersion.AttachmentHistoryID)
                                                .Count;
        }
    }
}