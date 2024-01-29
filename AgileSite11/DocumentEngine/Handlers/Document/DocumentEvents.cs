namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document events.
    /// </summary>
    public static class DocumentEvents
    {
        #region "Documents"

        /// <summary>
        /// Fires when document is updated.
        /// </summary>
        public static DocumentHandler Update = new DocumentHandler { Name = "DocumentEvents.Update" };


        /// <summary>
        /// Fires when the document is updated in the database. 
        /// This is a read-only event, it is forbidden to change the document passed as event argument. If you need to modify the document, use <see cref="Update"/> events.
        /// </summary>
        /// <remarks>
        /// <see cref="Update"/>.Before event is fired before this event and <see cref="Update"/>.After event is fired after this event.
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        internal static readonly DocumentHandler UpdateInner = new DocumentHandler { Name = "DocumentEvents.UpdateInner" };


        /// <summary>
        /// Fires when new document is created.
        /// </summary>
        public static DocumentHandler Insert = new DocumentHandler { Name = "DocumentEvents.Insert" };


        /// <summary>
        /// Fires when document data are being retrieved.
        /// </summary>
        public static DocumentDataEventHandler GetData = new DocumentDataEventHandler { Name = "DocumentEvents.GetData" };


        /// <summary>
        /// Fires when new culture version of document is created.
        /// </summary>
        public static DocumentHandler InsertNewCulture = new DocumentHandler { Name = "DocumentEvents.InsertNewCulture" };


        /// <summary>
        /// Fires when new document link is created.
        /// </summary>
        public static DocumentHandler InsertLink = new DocumentHandler { Name = "DocumentEvents.InsertLink" };


        /// <summary>
        /// Fires when document is deleted.
        /// </summary>
        public static DocumentHandler Delete = new DocumentHandler { Name = "DocumentEvents.Delete" };


        /// <summary>
        /// Fires when document is moved.
        /// </summary>
        public static DocumentHandler Move = new DocumentHandler { Name = "DocumentEvents.Move" };


        /// <summary>
        /// Fires when document is copied.
        /// </summary>
        public static DocumentHandler Copy = new DocumentHandler { Name = "DocumentEvents.Copy" };


        /// <summary>
        /// Fires when the content of the document for searching is requested.
        /// </summary>
        public static DocumentSearchHandler GetContent = new DocumentSearchHandler { Name = "DocumentEvents.GetContent" };


        /// <summary>
        /// Fires when document tasks are logged. You can disable tasks logging for staging, integration etc.
        /// </summary>
        public static LogDocumentChangeHandler LogChange = new LogDocumentChangeHandler { Name = "DocumentEvents.LogChange" };


        /// <summary>
        /// Fires when permissions are checked on the document.
        /// </summary>
        public static DocumentSecurityHandler CheckPermissions = new DocumentSecurityHandler { Name = "DocumentEvents.CheckPermissions" };


        /// <summary>
        /// Fires when change order is requested on the document.
        /// </summary>
        public static DocumentChangeOrderHandler ChangeOrder = new DocumentChangeOrderHandler { Name = "DocumentEvents.ChangeOrder" };


        /// <summary>
        /// Fires when sorting of a document sub-section is requested.
        /// </summary>
        public static DocumentSortHandler Sort = new DocumentSortHandler { Name = "DocumentEvents.Sort" };


        /// <summary>
        /// Fires when rating of a document is reset.
        /// </summary>
        public static DocumentRatingHandler ResetRating = new DocumentRatingHandler { Name = "DocumentEvents.ResetRating" };

        
        /// <summary>
        /// Fires when document is changed to a link using <see cref="TreeNode.ChangeToLink"/>.
        /// </summary>
        internal static DocumentHandler ChangeToLink = new DocumentHandler { Name = "DocumentEvents.ChangeToLink" };


        /// <summary>
        /// Fires when document type of a document is changed using <see cref="TreeNode.ChangeNodeDocumentType"/>.
        /// </summary>
        internal static DocumentHandler ChangeDocumentType = new DocumentHandler { Name = "DocumentEvents.ChangeDocumentType" };

        #endregion


        #region "Attachments"

        /// <summary>
        /// Fires when document attachment is saved.
        /// </summary>
        public static DocumentHandler SaveAttachment = new DocumentHandler { Name = "DocumentEvents.SaveAttachment" };


        /// <summary>
        /// Fires when document attachment is deleted.
        /// </summary>
        public static DocumentHandler DeleteAttachment = new DocumentHandler { Name = "DocumentEvents.DeleteAttachment" };

        #endregion


        #region "Security"

        /// <summary>
        /// Fires when the permission for particular document is evaluated.
        /// </summary>
        public static DocumentAuthorizationHandler AuthorizeDocument = new DocumentAuthorizationHandler { Name = "DocumentEvents.AuthorizeDocument" };


        /// <summary>
        /// Fires when the given DataSet should be filtered according to the user permissions.
        /// </summary>
        public static DocumentAuthorizationHandler FilterDataSetByPermissions = new DocumentAuthorizationHandler { Name = "DocumentEvents.FilterDataSetByPermissions" };

        #endregion


        #region "Document mark"

        /// <summary>
        /// Fires at the end of getting document marks for content tree.
        /// </summary>
        public static DocumentMarkHandler GetDocumentMark = new DocumentMarkHandler { Name = "DocumentEvents.GetDocumentMark" };

        #endregion
    }
}