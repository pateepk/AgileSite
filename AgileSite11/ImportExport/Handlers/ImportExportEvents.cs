using System;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import export events
    /// </summary>
    public static class ImportExportEvents
    {
        #region "Import"

        /// <summary>
        /// Fires when the import process runs for the whole import process. This is the only event where the CMSActionContext is not modified.
        /// </summary>
        public static ImportBaseHandler Import = new ImportBaseHandler { Name = "ImportExportEvents.Import" };


        /// <summary>
        /// Fires when the import process runs for the import data process. This handler is inside action context with all logging disabled (CMSActionContext.DisableAll method is called).
        /// </summary>
        public static ImportBaseHandler ImportObjects = new ImportBaseHandler { Name = "ImportExportEvents.ImportObjects" };


        /// <summary>
        /// Fires when the import process runs for a single object
        /// </summary>
        public static ImportHandler ImportObject = new ImportHandler { Name = "ImportExportEvents.ImportObject" };


        /// <summary>
        /// Fires before objects for specific object type are being processed.
        /// </summary>
        public static ImportDataHandler ImportObjectType = new ImportDataHandler { Name = "ImportExportEvents.ImportObjectType" };


        /// <summary>
        /// Fires when the binding objects are imported
        /// </summary>
        public static ImportHandler ImportBindings = new ImportHandler { Name = "ImportExportEvents.ImportBindings" };
                
        
        /// <summary>
        /// Fires when the binding object type is imported
        /// </summary>
        public static ImportObjectTypeHandler ImportBindingType = new ImportObjectTypeHandler { Name = "ImportExportEvents.ImportBindingType" };


        /// <summary>
        /// Fires when the single binding object is imported
        /// </summary>
        public static ImportHandler ImportBinding = new ImportHandler { Name = "ImportExportEvents.ImportBinding" };

        
        /// <summary>
        /// Fires when the child objects are imported
        /// </summary>
        public static ImportHandler ImportChildren = new ImportHandler { Name = "ImportExportEvents.ImportChildren" };
        
        
        /// <summary>
        /// Fires when the child object type is imported
        /// </summary>
        public static ImportObjectTypeHandler ImportChildType = new ImportObjectTypeHandler { Name = "ImportExportEvents.ImportChildType" };
      

        /// <summary>
        /// Fires when the single child object is imported
        /// </summary>
        public static ImportHandler ImportChild = new ImportHandler { Name = "ImportExportEvents.ImportChild" };


        /// <summary>
        /// Fires when the import process throws an error
        /// </summary>
        public static ImportErrorHandler ImportError = new ImportErrorHandler { Name = "ImportExportEvents.ImportError" };


        /// <summary>
        /// Fires when the import process is canceled
        /// </summary>
        public static ImportCanceledHandler ImportCanceled = new ImportCanceledHandler { Name = "ImportExportEvents.ImportCanceled" };
        

        /// <summary>
        /// Fires when the import process gets data for object type or documents from package
        /// </summary>
        public static ImportGetDataHandler GetImportData = new ImportGetDataHandler { Name = "ImportExportEvents.GetImportData" };


        /// <summary>
        /// Fires when the import process processes file/folder operations from package
        /// </summary>
        public static ImportProcessFileOperationHandler ProcessFileOperation = new ImportProcessFileOperationHandler { Name = "ImportExportEvents.ProcessFileOperation" };

        #endregion


        #region "Export"

        /// <summary>
        /// Fires when the export process runs for the whole export process
        /// </summary>
        public static ExportHandler Export = new ExportHandler { Name = "ImportExportEvents.Export" };

        /// <summary>
        /// Fires when the export process is being finalized
        /// </summary>
        public static ExportHandler FinalizeExport = new ExportHandler { Name = "ImportExportEvents.FinalizeExport" };


        /// <summary>
        /// Fires when the export process runs for the objects DataSet
        /// </summary>
        public static ExportHandler ExportObjects = new ExportHandler { Name = "ImportExportEvents.ExportObjects" };


        /// <summary>
        /// Fires when the export process throws an error
        /// </summary>
        public static ExportErrorHandler ExportError = new ExportErrorHandler { Name = "ImportExportEvents.ExportError" };


        /// <summary>
        /// Fires when the export process is canceled
        /// </summary>
        public static ExportCanceledHandler ExportCanceled = new ExportCanceledHandler { Name = "ImportExportEvents.ExportCanceled" };

        
        /// <summary>
        /// Fires when the export process gets data for object type or documents
        /// </summary>
        public static ExportGetDataHandler GetExportData = new ExportGetDataHandler { Name = "ImportExportEvents.GetExportData" };


        /// <summary>
        /// Fires when related objects are being selected for single object export
        /// </summary>
        public static SingleExportSelectionHandler SingleExportSelection = new SingleExportSelectionHandler { Name = "ImportExportEvents.SingleExportSelection" };

        #endregion
    }
}