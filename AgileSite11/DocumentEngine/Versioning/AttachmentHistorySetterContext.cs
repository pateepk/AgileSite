namespace CMS.DocumentEngine
{
    /// <summary>
    /// Encapsulates context required for saving of particular attachment version of a document.
    /// </summary>
    internal class AttachmentHistorySetterContext
    {
        public DocumentAttachment SourceAttachment
        {
            get;
        }


        public int VersionHistoryId
        {
            get;
        }


        public AttachmentHistoryInfo CurrentAttachmentVersion
        {
            get;
        }


        public bool NewAttachmentVersionToBeCreated
        {
            get;
        }


        /// <summary>
        /// Creates an instance of the <see cref="AttachmentHistorySetterContext"/> class.
        /// </summary>
        /// <param name="sourceAttachment">Attachment to be saved into version.</param>
        /// <param name="versionHistoryId">Version history ID.</param>
        public AttachmentHistorySetterContext(DocumentAttachment sourceAttachment, int versionHistoryId)
        {
            SourceAttachment = sourceAttachment;
            VersionHistoryId = versionHistoryId;

            var vm = VersionManager.GetInstance(null);
            CurrentAttachmentVersion = vm.GetAttachmentVersion(versionHistoryId, sourceAttachment.AttachmentGUID, false);
            NewAttachmentVersionToBeCreated = CurrentAttachmentVersion == null || IsAttachmentVersionOptimized();
        }


        private bool IsAttachmentVersionOptimized()
        {
            return GetAttachmentVersionsBoundToCurrentAttachmentVersionCount() != 1;
        }


        private int GetAttachmentVersionsBoundToCurrentAttachmentVersionCount()
        {
            if (CurrentAttachmentVersion.AttachmentHistoryID <= 0)
            {
                return 0;
            }

            return VersionAttachmentInfoProvider.GetVersionAttachments()
                                                .WhereEquals("AttachmentHistoryID", CurrentAttachmentVersion.AttachmentHistoryID)
                                                .Count;
        }
    }
}