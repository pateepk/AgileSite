namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Document events for internal purposes.
    /// </summary>
    public static class DocumentEventsInternal
    {
        #region "Preview link generation"

        /// <summary>
        /// Fires during preview link generation.
        /// </summary>
        public static GeneratePreviewLinkHandler GeneratePreviewLink = new GeneratePreviewLinkHandler { Name = "DocumentEventsInternal.GeneratePreviewLink" };

        #endregion
    }
}
