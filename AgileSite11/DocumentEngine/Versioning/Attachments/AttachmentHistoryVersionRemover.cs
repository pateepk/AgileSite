namespace CMS.DocumentEngine
{
    /// <summary>
    /// Removes all attachments from a document version.
    /// </summary>
    internal class AttachmentHistoryVersionRemover
    {
        private int VersionHistoryID
        {
            get;
            set;
        }


        /// <summary>
        /// Creates instance of <see cref="AttachmentHistoryVersionRemover"/>.
        /// </summary>
        /// <param name="versionHistoryId">Version history ID.</param>
        public AttachmentHistoryVersionRemover(int versionHistoryId)
        {
            VersionHistoryID = versionHistoryId;
        }


        /// <summary>
        /// Removes all the attachment histories from version.
        /// </summary>
        public void Remove()
        {
            AttachmentHistoryInfoProvider.GetAttachmentHistories()
                                         .InVersionExceptVariants(VersionHistoryID)
                                         .ForEachObject(history => new AttachmentHistoryRemover(history, VersionHistoryID).Remove());
        }
    }
}