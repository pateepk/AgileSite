using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.Membership;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Handles special actions during the BizForm import process.
    /// </summary>
    public static class BizFormImport
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
            if (objectType == BizFormInfo.OBJECT_TYPE)
            {
                if (!e.ParentImported)
                {
                    return;
                }

                var settings = e.Settings;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_Form"]))
                {
                    return;
                }

                // Import data
                DataTable table = data.Tables["CMS_Form"];
                ImportData(settings, table, e.TranslationHelper);
            }
        }


        /// <summary>
        /// Import BizForms data.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="table">Parent data</param>
        /// <param name="th">Translation helper</param>
        private static void ImportData(SiteImportSettings settings, DataTable table, TranslationHelper th)
        {
            ProcessObjectEnum processType = settings.GetObjectsProcessType(BizFormInfo.OBJECT_TYPE, true);
            if (processType == ProcessObjectEnum.None)
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

                // Get form name
                string bizFormName = dr["FormName"].ToString();
                string bizFormDisplayName = dr["FormDisplayName"].ToString();
                var bizFormDevelopmentModel = (FormDevelopmentModelEnum)dr["FormDevelopmentModel"].ToInteger(0);
                int bizFormClassId = ValidationHelper.GetInteger(dr["FormClassID"], 0);

                try
                {
                    // Check import settings
                    if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BIZFORM_DATA), true))
                    {
                        // Check if the form is selected
                        if ((processType != ProcessObjectEnum.All) && !settings.IsSelected(BizFormInfo.OBJECT_TYPE, bizFormName, true))
                        {
                            continue;
                        }

                        // Log progress
                        ImportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.ImportingBizFormData", "Importing BizForm '{0}' data"), HTMLHelper.HTMLEncode(bizFormDisplayName)));

                        // Get existing form
                        BizFormInfo bizForm = BizFormInfoProvider.GetBizFormInfo(bizFormName, settings.SiteId);
                        if (bizForm == null)
                        {
                            throw new InvalidOperationException("[ImportProvider.ImportBizFormsData]: Existing BizForm '" + bizFormName + "' not found.");
                        }

                        // Get form class
                        int classId = bizForm.FormClassID;
                        var dataClass = DataClassInfoProvider.GetDataClassInfo(classId);
                        if (dataClass == null)
                        {
                            throw new InvalidOperationException("[ImportProvider.ImportBizFormsData]: Class name for BizForm '" + bizFormName + "' not found.");
                        }

                        string originalClassName = null;
                        var classRow = th.GetRecord(DataClassInfo.OBJECT_TYPE, bizFormClassId);
                        if (classRow != null)
                        {
                            originalClassName = ValidationHelper.GetString(classRow["CodeName"], null);
                        }

                        string bizFormImportObjectType = ImportExportHelper.BIZFORM_PREFIX + originalClassName.ToLowerCSafe();

                        // Get data
                        DataSet ds = ImportProvider.LoadObjects(settings, bizFormImportObjectType, true);
                        if (DataHelper.DataSourceIsEmpty(ds))
                        {
                            continue;
                        }

                        // Delete all data
                        BizFormInfoProvider.DeleteData(bizFormName, settings.SiteId);

                        // Import data
                        DataTable dataDT = ds.Tables[0];
                        foreach (DataRow dataDR in dataDT.Rows)
                        {
                            // Process canceled
                            if (settings.ProcessCanceled)
                            {
                                ImportProvider.ImportCanceled();
                            }

                            // Raise event to translate custom columns
                            if (ColumnsTranslationEvents.TranslateColumns.IsBound)
                            {
                                th.SetDefaultValue(UserInfo.OBJECT_TYPE, settings.AdministratorId);
                                ColumnsTranslationEvents.TranslateColumns.StartEvent(th, BizFormItemProvider.GetObjectType(dataClass.ClassName), new DataRowContainer(dataDR));
                                th.RemoveDefaultValue(UserInfo.OBJECT_TYPE);
                            }

                            // Add the data
                            BizFormItem item = BizFormItem.New(dataClass.ClassName, dataDR);
                            item.Insert();
                        }

                        // Ensure to copy physical files
                        if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BIZFORM_FILES_PHYSICAL), true))
                        {
                            AddImportFiles(settings, ds, bizFormName, bizFormDevelopmentModel, dataClass);
                        }
                    }

                    // Refresh data count
                    BizFormInfoProvider.RefreshDataCount(bizFormName, settings.SiteId);
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ImportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorImportingBizFormData", "Error importing BizForm '{0}' data."), HTMLHelper.HTMLEncode(bizFormDisplayName)), ex);
                    throw;
                }
            }
        }


        private static void AddImportFiles(SiteImportSettings settings, DataSet data, string formName, FormDevelopmentModelEnum formDevelopmentModel, DataClassInfo dataClass)
        {
            // Get file columns
            string[] fileColumns = BizFormInfoProvider.GetBizFormFileColumns(dataClass.ClassFormDefinition, formDevelopmentModel);
            if (fileColumns == null)
            {
                return;
            }

            string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\bizform_files\\";
            string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);

            if (data == null)
            {
                return;
            }

            DataTable dataTable = data.Tables[0];
            if (DataHelper.DataSourceIsEmpty(dataTable))
            {
                return;
            }

            foreach (DataRow dr in dataTable.Rows)
            {
                // Import process canceled
                if (settings.ProcessCanceled)
                {
                    ImportProvider.ImportCanceled();
                }

                // Process each file column
                foreach (string columnName in fileColumns)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    string fileNameString = ValidationHelper.GetString(dr[columnName], null);
                    string fileName = FormHelper.GetGuidFileName(fileNameString);

                    string sourceObjectPath = sourcePath + DirectoryHelper.CombinePath(ImportExportHelper.SITE_MACRO, "bizform", formName, fileName);
                    string targetObjectPath = FormHelper.GetFilePhysicalPath(settings.SiteName, fileName, targetPath);

                    // Source file
                    settings.FileOperations.Add(BizFormInfo.OBJECT_TYPE, sourceObjectPath, targetObjectPath, FileOperationEnum.CopyFile);
                }
            }
        }

        #endregion
    }
}