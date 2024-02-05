using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document security handler
    /// </summary>
    public class DocumentSecurityHandler : AdvancedHandler<DocumentSecurityHandler, DocumentSecurityEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentSecurityHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public DocumentSecurityHandler(DocumentSecurityHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="node">Handled document</param>
        public DocumentSecurityHandler StartEvent(TreeNode node)
        {
            DocumentSecurityEventArgs e = new DocumentSecurityEventArgs()
            {
                Node = node
            };

            return StartEvent(e);
        }
    }
}