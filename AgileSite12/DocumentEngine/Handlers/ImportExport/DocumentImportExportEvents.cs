using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document import export events
    /// </summary>
    public static class DocumentImportExportEvents
    {
        #region "Import"

        /// <summary>
        /// Fires when the import process runs for a single document
        /// </summary>
        public static DocumentImportHandler ImportDocument = new DocumentImportHandler { Name = "DocumentImportExportEvents.ImportDocument" };


        /// <summary>
        /// Fires when the documents are being imported
        /// </summary>
        public static DocumentsImportHandler ImportDocuments = new DocumentsImportHandler { Name = "DocumentImportExportEvents.ImportDocuments" };

        #endregion


        #region "Export"

        /// <summary>
        /// Fires when the export process runs for exporting the document DataSet
        /// </summary>
        public static DocumentsExportHandler ExportDocuments = new DocumentsExportHandler { Name = "DocumentImportExportEvents.ExportDocuments" };

        #endregion
    }
}