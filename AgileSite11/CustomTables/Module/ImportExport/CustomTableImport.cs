using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;

namespace CMS.CustomTables
{
    /// <summary>
    /// Handles special actions during the Custom table import process.
    /// </summary>
    internal static class CustomTableImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;
        }


        private static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE)
            {
                if (!e.ParentImported)
                {
                    return;
                }

                var settings = e.Settings;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_Class"]))
                {
                    return;
                }

                // Import data
                DataTable table = data.Tables["CMS_Class"];
                ImportData(settings, table, e.TranslationHelper);
            }
        }


        /// <summary>
        /// Import custom tables data.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="table">Parent data</param>
        /// <param name="th">Translation helper</param>
        private static void ImportData(SiteImportSettings settings, DataTable table, TranslationHelper th)
        {
            ProcessObjectEnum processType = settings.GetObjectsProcessType(CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE, true);
            if (processType == ProcessObjectEnum.None)
            {
                return;
            }

            // Check import settings
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_CUSTOMTABLE_DATA), true))
            {
                return;
            }

            foreach (DataRow dr in table.Rows)
            {
                // Process canceled
                if (settings.ProcessCanceled)
                {
                    ImportProvider.ImportCanceled();
                }

                // Get custom table name
                string customTableName = dr["ClassName"].ToString();
                string customTableDisplayName = dr["ClassDisplayName"].ToString();

                try
                {
                    // Check if any table selected
                    if ((processType != ProcessObjectEnum.All) && !settings.IsSelected(CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE, customTableName, false))
                    {
                        continue;
                    }

                    // Log progress
                    ImportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.ImportingCustomTableData", "Importing custom table '{0}' data"), HTMLHelper.HTMLEncode(customTableDisplayName)));

                    string customTableObjectType = ImportExportHelper.CUSTOMTABLE_PREFIX + customTableName.ToLowerCSafe();

                    // Get data
                    DataSet ds = ImportProvider.LoadObjects(settings, customTableObjectType, true);
                    if (DataHelper.DataSourceIsEmpty(ds))
                    {
                        continue;
                    }

                    // Delete all data
                    using (CMSActionContext ctx = new CMSActionContext())
                    {
                        ctx.UpdateSystemFields = false;
                        ctx.TouchCacheDependencies = false;

                        CustomTableItemProvider.DeleteItems(customTableName);
                    }


                    // Import data
                    DataTable dataDT = ds.Tables[0];
                    foreach (DataRow dataDR in dataDT.Rows)
                    {
                        // Process canceled
                        if (settings.ProcessCanceled)
                        {
                            ImportProvider.ImportCanceled();
                        }

                        // Translate build-in columns
                        th.SetDefaultValue(UserInfo.OBJECT_TYPE, settings.AdministratorId);
                        th.TranslateColumn(dataDR, "ItemCreatedBy", UserInfo.OBJECT_TYPE);
                        th.TranslateColumn(dataDR, "ItemModifiedBy", UserInfo.OBJECT_TYPE);

                        // Raise event to translate custom columns
                        if (ColumnsTranslationEvents.TranslateColumns.IsBound)
                        {
                            ColumnsTranslationEvents.TranslateColumns.StartEvent(th, CustomTableItemProvider.GetObjectType(customTableName), new DataRowContainer(dataDR));
                        }

                        th.RemoveDefaultValue(UserInfo.OBJECT_TYPE);

                        // Add the data
                        CustomTableItem item = CustomTableItem.New(customTableName, dataDR);

                        // Ensure Item GUID
                        if (item.ItemGUID == Guid.Empty)
                        {
                            item.ItemGUID = Guid.NewGuid();
                        }

                        item.Insert();
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ImportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorImportingCustomTableData", "Error importing custom table '{0}' data."), HTMLHelper.HTMLEncode(customTableDisplayName)), ex);
                    throw;
                }
            }
        }

        #endregion
    }
}