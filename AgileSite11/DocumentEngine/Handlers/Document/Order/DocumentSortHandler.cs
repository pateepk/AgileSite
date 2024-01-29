using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document change order handler
    /// </summary>
    public class DocumentSortHandler : AdvancedHandler<DocumentSortHandler, DocumentSortEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentSortHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public DocumentSortHandler(DocumentSortHandler parentHandler)
        {
            Parent = parentHandler;
        }
    }
}