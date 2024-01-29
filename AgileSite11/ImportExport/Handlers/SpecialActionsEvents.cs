using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import export special actions events
    /// </summary>
    public static class SpecialActionsEvents
    {
        #region "Import"

        /// <summary>
        /// Fires when the existing object is being get
        /// </summary>
        public static ImportHandler GetExistingObject = new ImportHandler { Name = "SpecialActionsEvents.GetExistingObject" };


        /// <summary>
        /// Fires when the existing object is being handled
        /// </summary>
        public static ImportHandler HandleExistingObject = new ImportHandler { Name = "SpecialActionsEvents.HandleExistingObject" };


        /// <summary>
        /// Fires when the main object is being processed
        /// </summary>
        public static ImportHandler ProcessMainObject = new ImportHandler { Name = "SpecialActionsEvents.ProcessMainObject" };


        /// <summary>
        /// Fires when the object's data is being processed
        /// </summary>
        public static ImportHandler ProcessObjectData = new ImportHandler { Name = "SpecialActionsEvents.ProcessObjectData" };


        /// <summary>
        /// Fires when the object is successfully processed
        /// </summary>
        public static SimpleImportHandler ObjectProcessed = new SimpleImportHandler { Name = "SpecialActionsEvents.ObjectProcessed" };


        /// <summary>
        /// Fires when the additional actions are being processed
        /// </summary>
        public static ImportBaseHandler ProcessAdditionalActions = new ImportBaseHandler { Name = "SpecialActionsEvents.ProcessAdditionalActions" };


        /// <summary>
        /// Fires when the objects are being post processed
        /// </summary>
        public static ImportHandler ObjectsPostProcessing = new ImportHandler { Name = "SpecialActionsEvents.ObjectsPostProcessing" };


        /// <summary>
        /// Fires when the data structure is being prepared for an object type
        /// </summary>
        public static SimpleDataImportHandler PrepareDataStructure = new SimpleDataImportHandler { Name = "SpecialActionsEvents.PrepareDataStructure" };


        /// <summary>
        /// Fires when the import empty object is being get
        /// </summary>
        public static SimpleDataImportHandler GetEmptyObject = new SimpleDataImportHandler { Name = "SpecialActionsEvents.GetEmptyObject" };
        

        /// <summary>
        /// Fires when the delete task is being processed
        /// </summary>
        public static ProcessDeleteTaskHandler ProcessDeleteTask = new ProcessDeleteTaskHandler { Name = "SpecialActionsEvents.ProcessDeleteTask" };


        /// <summary>
        /// Fires when the binary data source path is being get
        /// </summary>
        public static GetBinaryDataSourcePathHandler GetBinaryDataSourcePath = new GetBinaryDataSourcePathHandler { Name = "SpecialActionsEvents.GetBinaryDataSourcePath" };


        /// <summary>
        /// Fires when the objects for an object type is being selected during the import process
        /// </summary>
        public static ImportLoadSelectionHandler ImportLoadDefaultSelection = new ImportLoadSelectionHandler { Name = "SpecialActionsEvents.ImportLoadDefaultSelection" };


        /// <summary>
        /// Fires when the objects for an object type is being automatically selected during the import process
        /// </summary>
        public static ImportLoadSelectionHandler ImportEnsureAutomaticSelection = new ImportLoadSelectionHandler { Name = "SpecialActionsEvents.ImportEnsureAutomaticSelection" };

        #endregion


        #region "General"

        /// <summary>
        /// Fires when the administrator user is required for the import process
        /// </summary>
        public static GetObjectWhereConditionHandler GetObjectWhereCondition = new GetObjectWhereConditionHandler { Name = "SpecialActionsEvents.GetObjectWhereCondition" };


        /// <summary>
        /// Fires when the name of the package folder is being get
        /// </summary>
        public static GetObjectTypeFolderHandler GetObjectTypeFolder = new GetObjectTypeFolderHandler { Name = "SpecialActionsEvents.GetObjectTypeFolder" }; 
        
        #endregion


        #region "Export"

        /// <summary>
        /// Fires when the objects for an object type is being loaded during the export process selection
        /// </summary>
        public static ExportLoadSelectionHandler ExportLoadDefaultSelection = new ExportLoadSelectionHandler { Name = "SpecialActionsEvents.ExportLoadDefaultSelection" };


        /// <summary>
        /// Fires when the objects for an object type is being automatically selected during the export process
        /// </summary>
        public static ExportLoadSelectionHandler ExportEnsureAutomaticSelection = new ExportLoadSelectionHandler { Name = "SpecialActionsEvents.ExportEnsureAutomaticSelection" };


        /// <summary>
        /// Fires when the objects for an object type is being selected during the export process
        /// </summary>
        public static ExportGetSelectionWhereConditionHandler GetSelectionWhereCondition = new ExportGetSelectionWhereConditionHandler { Name = "SpecialActionsEvents.GetSelectionWhereCondition" };

        #endregion
    }
}