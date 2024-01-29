using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document rating handler
    /// </summary>
    public class DocumentRatingHandler : SimpleHandler<DocumentRatingHandler, DocumentRatingEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="node">Document</param>
        public DocumentRatingEventArgs StartEvent(TreeNode node)
        {
            var e = new DocumentRatingEventArgs
            {
                Node = node
            };

            StartEvent(e);

            return e;
        }
    }
}