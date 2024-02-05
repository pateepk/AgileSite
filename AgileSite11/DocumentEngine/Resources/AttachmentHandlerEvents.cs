using CMS.Base;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Events for document attachment handler.
    /// </summary>
    public static class AttachmentHandlerEvents
    {
        /// <summary>
        /// Fires when an attachment is being served either from cache or for the first time.
        /// </summary>
        public static readonly SimpleHandler<AttachmentSendFileEventArgs> SendFile = new SimpleHandler<AttachmentSendFileEventArgs> { Name = "AttachmentHandlerEvents.SendFile" };


        /// <summary>
        /// Fires when the attachment handler request is being processed before the attachment is being processed and served.
        /// </summary>
        internal static readonly SimpleHandler ProcessRequest = new SimpleHandler { Name = "AttachmentHandlerEvents.ProcessRequest" };
    }
}
