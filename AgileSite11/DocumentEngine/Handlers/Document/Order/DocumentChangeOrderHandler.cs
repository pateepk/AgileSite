using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document change order handler
    /// </summary>
    public class DocumentChangeOrderHandler : AdvancedHandler<DocumentChangeOrderHandler, DocumentChangeOrderEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentChangeOrderHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public DocumentChangeOrderHandler(DocumentChangeOrderHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="node">Handled document</param>
        public DocumentChangeOrderHandler StartEvent(TreeNode node)
        {
            DocumentChangeOrderEventArgs e = new DocumentChangeOrderEventArgs()
            {
                Node = node
            };

            return StartEvent(e);
        }
    }
}