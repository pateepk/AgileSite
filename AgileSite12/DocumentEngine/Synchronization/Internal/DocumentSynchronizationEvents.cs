namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Document synchronization events.
    /// </summary>
    public static class DocumentSynchronizationEvents
    {
        /// <summary>
        /// Fired when document is clonned during synchronization task creation process.
        /// </summary>
        public static readonly LogDocumentChangeCloneHandler LogDocumentChangeClone = new LogDocumentChangeCloneHandler { Name = "DocumentSynchronizationEvents.LogDocumentChangeClone" };
    }
}
