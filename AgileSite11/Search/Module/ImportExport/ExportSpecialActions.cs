using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Search
{
    /// <summary>
    /// Handles special actions during the export process.
    /// </summary>
    internal static class ExportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ExportObjects.After += CopyAssemblyFiles;
            ImportExportEvents.GetExportData.After += RemoveNotRelevantColumns;
        }


        private static void RemoveNotRelevantColumns(object sender, ExportGetDataEventArgs e)
        {
            var data = e.Data;
            var objectType = e.ObjectType;

            if (objectType.Equals(SearchIndexInfo.OBJECT_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                var table = data.Tables["cms_searchindex"];
                if (table != null)
                {
                    // Do not include IndexStatus column because its value is not valid in target instance
                    table.Columns.Remove("IndexStatus");
                }
            }
        }


        private static void CopyAssemblyFiles(object sender, ExportEventArgs e)
        {
            var settings = e.Settings;
            var data = e.Data;
            var objectType = e.ObjectType;

            if (objectType == SearchIndexInfo.OBJECT_TYPE)
            {
                CopyAssemblyFiles(settings, data, objectType);
            }
        }


        private static void CopyAssemblyFiles(SiteExportSettings settings, DataSet data, string objectType)
        {
            // Copy files only if required
            if (!settings.CopyFiles)
            {
                return;
            }

            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_ASSEMBLIES), false))
            {
                return;
            }

            // Get object info
            BaseInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
            string assemblyColumn = infoObj.TypeInfo.AssemblyNameColumn;

            if (string.IsNullOrEmpty(assemblyColumn))
            {
                return;
            }

            if (data == null)
            {
                return;
            }

            // Get table
            var dt = ObjectHelper.GetTable(data, infoObj);
            if (DataHelper.DataSourceIsEmpty(dt))
            {
                return;
            }

            string sourcePath = settings.WebsitePath + @"bin\";
            string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);
            string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + safeObjectType + @"\bin\";

            // Process all assembly files
            foreach (DataRow dr in dt.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportProvider.ExportCanceled();
                }

                string indexSettings = ValidationHelper.GetString(dr["IndexSettings"], null);
                if (string.IsNullOrEmpty(indexSettings))
                {
                    continue;
                }

                SearchIndexSettings sis = new SearchIndexSettings();
                sis.LoadData(indexSettings);
                foreach (var item in sis.Items)
                {
                    SearchIndexSettingsInfo sisi = item.Value;
                    if (sisi == null)
                    {
                        continue;
                    }

                    string customSearchIndexAssemblyName = ValidationHelper.GetString(sisi.GetValue("AssemblyName"), string.Empty);
                    if (!string.IsNullOrEmpty(customSearchIndexAssemblyName))
                    {
                        ExportProvider.ExportAssembly(settings.WebsitePath, targetPath, sourcePath, customSearchIndexAssemblyName);
                    }
                }
            }
        }

        #endregion
    }
}