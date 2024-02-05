using System;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Saves attachment in a document version.
    /// </summary>
    internal class AttachmentHistorySetter
    {
        private readonly AttachmentHistorySetterContext Context;


        /// <summary>
        /// Creates an instance of the <see cref="AttachmentHistorySetter"/> class.
        /// </summary>
        /// <param name="context">The context required for the attachment history to be saved.</param>
        public AttachmentHistorySetter(AttachmentHistorySetterContext context)
        {
            if (context.SourceAttachment.IsVariant())
            {
                throw new InvalidOperationException("Saving version of attachment history variants is not supported. Use AttachmentHistoryInfoProvider to handle attachment history variants data.");
            }

            Context = context;
        }


        /// <summary>
        /// Sets the attachment into specific version.
        /// </summary>
        public AttachmentHistoryInfo Set()
        {
            EnsureSourceAttachmentBinary();
            EnsureSourceAttachmentSafeFileName();

            var attachmentVersion = Context.NewAttachmentVersionToBeCreated ? InsertAttachmentVersion() : UpdateAttachmentVersion();

            Context.SourceAttachment.Load(attachmentVersion, Context.VersionHistoryId);

            return attachmentVersion;
        }


        private void EnsureSourceAttachmentSafeFileName()
        {
            var sourceAttachment = Context.SourceAttachment;
            var siteName = SiteInfoProvider.GetSiteName(sourceAttachment.AttachmentSiteID);

            sourceAttachment.AttachmentName = URLHelper.GetSafeFileName(sourceAttachment.AttachmentName, siteName);
        }


        private AttachmentHistoryInfo UpdateAttachmentVersion()
        {
            var attachmentHistoryInfo = GetAttachmentVersionForUpdate();
            attachmentHistoryInfo.Update();

            return attachmentHistoryInfo;
        }


        private AttachmentHistoryInfo GetAttachmentVersionForUpdate()
        {
            var sourceAttachment = Context.SourceAttachment;
            var currentAttachment = Context.CurrentAttachmentVersion;
            currentAttachment.ApplyData(sourceAttachment);

            // Propagate settings to eventually not load the binary data if not needed
            currentAttachment.AllowPartialUpdate = sourceAttachment.AllowPartialUpdate;
            if (!sourceAttachment.AllowPartialUpdate)
            {
                EnsureAttachmentBinary(currentAttachment);
            }

            return currentAttachment;
        }


        private AttachmentHistoryInfo InsertAttachmentVersion()
        {
            RemovePreviousBinding();

            var attachmentVersion = GetAttachmentVersionForInsert();
            attachmentVersion.Insert();

            CreateBinding(attachmentVersion);

            return attachmentVersion;
        }


        private AttachmentHistoryInfo GetAttachmentVersionForInsert()
        {
            var sourceAttachment = Context.SourceAttachment;
            var attachmentVersion = new AttachmentHistoryInfo();
            attachmentVersion.ApplyData(sourceAttachment);

            // Propagate settings to eventually not load the binary data if not needed
            attachmentVersion.AllowPartialUpdate = sourceAttachment.AllowPartialUpdate;
            EnsureAttachmentBinary(attachmentVersion);

            attachmentVersion.AttachmentHistoryGUID = Guid.NewGuid();

            return attachmentVersion;
        }


        private void EnsureAttachmentBinary(AttachmentHistoryInfo attachmentHistoryInfo)
        {
            if (attachmentHistoryInfo.AttachmentBinary != null)
            {
                return;
            }

            var sourceAttachment = Context.SourceAttachment;
            var siteName = SiteInfoProvider.GetSiteName(sourceAttachment.AttachmentSiteID);
            attachmentHistoryInfo.AttachmentBinary = AttachmentBinaryHelper.GetAttachmentBinary(sourceAttachment.AttachmentGUID, siteName);
        }


        private void EnsureSourceAttachmentBinary()
        {
            var currentAttachmentversion = Context.CurrentAttachmentVersion;
            if (currentAttachmentversion == null || currentAttachmentversion.AttachmentHistoryID <= 0)
            {
                return;
            }

            var sourceAttachment = Context.SourceAttachment;
            if (sourceAttachment.AttachmentBinary != null)
            {
                return;
            }

            sourceAttachment.AttachmentBinary = currentAttachmentversion.Generalized.EnsureBinaryData();
        }


        private void CreateBinding(AttachmentHistoryInfo attachmentVersion)
        {
            VersionAttachmentInfoProvider.SetVersionAttachmentInfo(Context.VersionHistoryId, attachmentVersion.AttachmentHistoryID);
        }


        private void RemovePreviousBinding()
        {
            if (Context.CurrentAttachmentVersion == null)
            {
                return;
            }

            // Prevent from touching the previous attachment history by deleting the binding to not to change the time stamp.
            // The ETag for client caching should not be changed.
            using (new DocumentActionContext { TouchParent = false })
            {
                // Remove the previous binding
                VersionAttachmentInfoProvider.DeleteVersionAttachmentInfo(Context.VersionHistoryId, Context.CurrentAttachmentVersion.AttachmentHistoryID);
            }
        }
    }
}