using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Combined collection of attachments.
    /// </summary>
    public class CombinedDocumentAttachmentCollection : CombinedInfoObjectCollection<DocumentAttachmentCollection, DocumentAttachment>
    {
        #region "Methods"

        /// <summary>
        /// Submits the changes in the collection to the database.
        /// </summary>
        public override void SubmitChanges()
        {
            // Submit all collections individually
            foreach (DocumentAttachmentCollection collection in Collections)
            {
                collection.SubmitChanges();
            }
        }

        #endregion
    }
}