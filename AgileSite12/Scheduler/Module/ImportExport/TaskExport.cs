using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;

namespace CMS.Scheduler
{
    /// <summary>
    /// Handles special actions during the Task export process.
    /// </summary>
    internal static class TaskExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += ExportObjects_After;
            ImportExportEvents.GetExportData.After += GetExportData_After;
        }


        private static void ExportObjects_After(object sender, ExportEventArgs e)
        {
            // Register translation record
            RegisterTranslationRecords(e.Data, e.ObjectType, e.TranslationHelper, e.Settings.ExcludedNames);
        }


        /// <summary>
        /// Ensure translation records registration.
        /// </summary>
        /// <param name="data">Source dataset</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="th">Translation helper</param>
        /// <param name="excludedNames">Excluded object names</param>
        private static void RegisterTranslationRecords(DataSet data, string objectType, TranslationHelper th, string[] excludedNames)
        {
            if (objectType != TaskInfo.OBJECT_TYPE_OBJECTTASK || DataHelper.DataSourceIsEmpty(data))
            {
                // Object is either not OBJECTTASK or data source is empty
                return;
            }
                
            // Records for scheduled tasks
            th.RegisterRecords(data.Tables[0], ResourceInfo.OBJECT_TYPE, "TaskResourceID", null, excludedNames);
        }


        /// <summary>
        /// Removes columns that only have meaning in current local context.
        /// </summary>
        /// <remarks>
        /// Event is called twice:
        ///     First during Objects selection of Site Export – where there are only certain ID columns present in the dataTable.
        ///     Second time during export itself – where there are all columns present in the dataTable.
        /// </remarks>
        private static void GetExportData_After(object sender, ExportGetDataEventArgs e)
        {
            const string EXECUTING_SERVER_NAME_COLUMN = "TaskExecutingServerName";
            var objectType = e.ObjectType;

            if ((objectType != TaskInfo.OBJECT_TYPE) && (objectType != TaskInfo.OBJECT_TYPE_OBJECTTASK))
            {
                // object is not TaskInfo (neither user nor system task)
                return;
            }

            // Get TaskInfos' table
            GeneralizedInfo transformation = ModuleManager.GetReadOnlyObject(TaskInfo.OBJECT_TYPE);
            var dataTable = ObjectHelper.GetTable(e.Data, transformation);

            // Remove TaskExecutingServerName column (if present) as it only has meaning in current local context
            if (dataTable.Columns.Contains(EXECUTING_SERVER_NAME_COLUMN))
            {
                dataTable.Columns.Remove(EXECUTING_SERVER_NAME_COLUMN);
            }
        }

        #endregion
    }
}