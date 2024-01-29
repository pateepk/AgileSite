namespace CMS.UIControls
{
    /// <summary>
    /// Type of documents to show in my desk.
    /// </summary>
    public enum ListingTypeEnum
    {
        /// <summary>
        /// My documents.
        /// </summary>
        MyDocuments,

        /// <summary>
        /// Recent documents.
        /// </summary>
        RecentDocuments,

        /// <summary>
        /// Pending changes (waiting for approval).
        /// </summary>
        PendingDocuments,

        /// <summary>
        /// Checked out documents.
        /// </summary>
        CheckedOut,

        /// <summary>
        /// Outdated documents.
        /// </summary>
        OutdatedDocuments,

        /// <summary>
        /// Recycle bin.
        /// </summary>
        RecycleBin,

        /// <summary>
        /// Workflow documents.
        /// </summary>
        WorkflowDocuments,

        /// <summary>
        /// Page template documents.
        /// </summary>
        PageTemplateDocuments,

        /// <summary>
        /// Category documents.
        /// </summary>
        CategoryDocuments,

        /// <summary>
        /// Document type documents.
        /// </summary>
        DocTypeDocuments,

        /// <summary>
        /// Tag documents.
        /// </summary>
        TagDocuments,

        /// <summary>
        /// Product documents.
        /// </summary>
        ProductDocuments,

        /// <summary>
        /// All at once.
        /// </summary>
        All
    }
}