using CMS.Base;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document search handler
    /// </summary>
    public class DocumentSearchHandler : SimpleHandler<DocumentSearchHandler, DocumentSearchEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentSearchHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public DocumentSearchHandler(DocumentSearchHandler parentHandler)
        {
            Parent = parentHandler;
        }
    }
}