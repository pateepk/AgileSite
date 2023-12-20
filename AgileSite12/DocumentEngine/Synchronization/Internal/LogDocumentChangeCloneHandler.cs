using CMS.Base;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    ///  Event handler for <see cref="DocumentSynchronizationEvents.LogDocumentChangeClone"/> event.
    /// </summary>
    public sealed class LogDocumentChangeCloneHandler : SimpleHandler<LogDocumentChangeCloneHandler, LogDocumentChangeCloneEventArgs>
    {
        /// <summary>
        /// Creates an empty instance of the <see cref="LogDocumentChangeCloneHandler"/> class.
        /// </summary>
        public LogDocumentChangeCloneHandler()
        {
        }


        /// <summary>
        /// Initiates the new handler context.
        /// </summary>
        /// <param name="original">Original node.</param>
        /// <param name="clone">Cloned node.</param>
        public LogDocumentChangeCloneEventArgs StartEvent(TreeNode original, TreeNode clone)
        {
            var args = new LogDocumentChangeCloneEventArgs(original, clone);
            return StartEvent(args);
        }
    }
}
