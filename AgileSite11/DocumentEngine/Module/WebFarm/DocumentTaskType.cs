namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm tasks for document operations
    /// </summary>
    public class DocumentTaskType
    {
        /// <summary>
        /// Attachment update.
        /// </summary>
        public const string UpdateAttachment = "UPDATEATTACHMENT";


        /// <summary>
        /// Attachment delete.
        /// </summary>
        public const string DeleteAttachment = "DELETEATTACHMENT";


        /// <summary>
        /// Clear document fields type infos
        /// </summary>
        public const string ClearDocumentFieldsTypeInfos = "CLEARDOCUMENTFIELDSTYPEINFOS";


        /// <summary>
        /// Invalidate document fields type info
        /// </summary>
        public const string InvalidateDocumentFieldsTypeInfo = "INVALIDATEDOCUMENTFIELDSTYPEINFO";


        /// <summary>
        /// Clear document type infos
        /// </summary>
        public const string ClearDocumentTypeInfos = "CLEARDOCUMENTTYPEINFOS";


        /// <summary>
        /// Update document type info
        /// </summary>
        public const string InvalidateDocumentTypeInfo = "INVALIDATEDOCUMENTTYPEINFO";
    }
}
