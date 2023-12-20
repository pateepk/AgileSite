using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.CustomTables
{
    /// <summary>
    /// Handles special actions during the Custom table export process.
    /// </summary>
    internal static class CustomTableExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += Export_After;
        }


        private static void Export_After(object sender, ExportEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE)
            {
                // Export data
                ExportData(e.Settings, e.Data, e.TranslationHelper);
            }
        }


        /// <summary>
        /// Export custom tables data.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="data">Parent dataset</param>
        /// <param name="th">Translation helper</param>
        private static void ExportData(SiteExportSettings settings, DataSet data, TranslationHelper th)
        {
            // Check export setting
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_CUSTOMTABLE_DATA), true))
            {
                return;
            }

            // There is no data for this object type
            if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_Class"]))
            {
                return;
            }
            
            // Export data for all custom tables
            DataTable table = data.Tables["CMS_Class"];
            foreach (DataRow dr in table.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportProvider.ExportCanceled();
                }

                // Get custom table name
                string className = dr["ClassName"].ToString();
                string tableName = dr["ClassTableName"].ToString();
                string customTableDisplayName = dr["ClassDisplayName"].ToString();

                // Log progress
                ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.ExportingCustomTableData", "Exporting custom table '{0}' data"), HTMLHelper.HTMLEncode(customTableDisplayName)));

                // Save settings
                settings.SavePersistentLog();

                try
                {
                    // Initialize data
                    string customTableObjectType = ImportExportHelper.CUSTOMTABLE_PREFIX + className.ToLowerCSafe();

                    // Get custom table data
                    DataSet ds = GetData(className, tableName);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Raise event to register custom translations
                        if (ColumnsTranslationEvents.RegisterRecords.IsBound)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                ColumnsTranslationEvents.RegisterRecords.StartEvent(th, CustomTableItemProvider.GetObjectType(className), new DataRowContainer(row));
                            }
                        }

                        ExportProvider.SaveObjects(settings, ds, customTableObjectType, true);
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ExportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorExportingCustomTableData", "Error exporting custom table '{0}' data."), HTMLHelper.HTMLEncode(customTableDisplayName)), ex);
                    throw;
                }
            }
        }


        /// <summary>
        /// Returns DataSet of custom table records.
        /// </summary>
        /// <param name="className">Custom table class name</param>
        /// <param name="tableName">Table name</param>
        private static DataSet GetData(string className, string tableName)
        {
            var data = CustomTableItemProvider.GetItems(className);
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                data.Tables[0].TableName = tableName;
            }

            return data;
        }

        #endregion
    }
}