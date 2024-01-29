using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Handles special actions during the BizForm export process.
    /// </summary>
    internal static class BizFormExport
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
            if (objectType == BizFormInfo.OBJECT_TYPE)
            {
                ExportData(e.Settings, e.Data, e.TranslationHelper);
            }
        }


        /// <summary>
        /// Export BizForms data.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="data">Parent dataset</param>
        /// <param name="th">Translation helper</param>
        private static void ExportData(SiteExportSettings settings, DataSet data, TranslationHelper th)
        {
            // Check export setting
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BIZFORM_DATA), true))
            {
                return;
            }

            // There is no data for this object type
            if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_Form"]))
            {
                return;
            }

            // Export data for all forms
            DataTable table = data.Tables["CMS_Form"];
            foreach (DataRow dr in table.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportProvider.ExportCanceled();
                }

                // Log progress
                string formDisplayName = dr["FormDisplayName"].ToString();
                ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.ExportingBizFormData", "Exporting BizForm '{0}' data"), HTMLHelper.HTMLEncode(formDisplayName)));

                // Save settings
                settings.SavePersistentLog();

                try
                {
                    // Initialize data
                    string formName = dr["FormName"].ToString();
                    int classId = ValidationHelper.GetInteger(dr["FormClassID"], 0);
                    var dci = DataClassInfoProvider.GetDataClassInfo(classId);

                    if (dci == null)
                    {
                        throw new Exception("[ImportBizFormsData]: Class name for BizForm '" + formName + "' not found.");
                    }

                    // Get form object type
                    string formImportObjectType = ImportExportHelper.BIZFORM_PREFIX + dci.ClassName.ToLowerCSafe();

                    // Get BizForm data
                    DataSet ds = GetData(dci);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Raise event to register custom translations
                        if (ColumnsTranslationEvents.RegisterRecords.IsBound)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                ColumnsTranslationEvents.RegisterRecords.StartEvent(th, BizFormItemProvider.GetObjectType(dci.ClassName), new DataRowContainer(row));
                            }
                        }

                        // Save data
                        ExportProvider.SaveObjects(settings, ds, formImportObjectType, true);

                        // Copy files
                        if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BIZFORM_FILES_PHYSICAL), true))
                        {
                            CopyFiles(settings, ds, formImportObjectType, formName, dci);
                        }
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ExportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorExportingBizFormData", "Error exporting BizForm '{0}' data."), HTMLHelper.HTMLEncode(formDisplayName)), ex);
                    throw;
                }
            }
        }


        private static void CopyFiles(SiteExportSettings settings, DataSet data, string objectType, string formName, DataClassInfo dataClass)
        {
            if (!settings.CopyFiles)
            {
                return;
            }

            // Get file columns
            var fileColumns = BizFormInfoProvider.GetBizFormFileColumns(dataClass.ClassFormDefinition);
            if (fileColumns == null)
            {
                return;
            }

            // Log process
            ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingFiles", "Copying '{0}' files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

            var table = data.Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportProvider.ExportCanceled();
                }

                // Process each file column
                foreach (string columnName in fileColumns)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    string fileNameString = ValidationHelper.GetString(dr[columnName], null);
                    string fileName = FormHelper.GetGuidFileName(fileNameString);

                    if (!ExportProvider.IsFileExcluded(fileName))
                    {
                        string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\bizform_files\\";
                        string targetObjectPath = targetPath + DirectoryHelper.CombinePath(ImportExportHelper.SITE_MACRO, "bizform", formName) + "\\";
                        string targetFilePath = targetObjectPath + fileName;
                        string sourceObjectPath = FormHelper.GetFilePhysicalPath(settings.SiteName, fileName);

                        try
                        {
                            // Source files
                            ExportProvider.CopyFile(sourceObjectPath, targetFilePath, settings.WebsitePath);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns DataSet of form records.
        /// </summary>
        /// <param name="dataClass">Form class</param>
        private static DataSet GetData(DataClassInfo dataClass)
        {
            var data = BizFormItemProvider.GetItems(dataClass.ClassName);
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                data.Tables[0].TableName = dataClass.ClassTableName;
            }

            return data;
        }

        #endregion
    }
}