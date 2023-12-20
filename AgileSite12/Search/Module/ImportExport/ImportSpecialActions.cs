using System.Data;
using System.Linq;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Base;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Search
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.ProcessAdditionalActions.Before += ProcessAdditionalActions_Before;
            ImportExportEvents.ImportObjectType.After += ImportObjectTypeAfter;
            ImportExportEvents.Import.After += ImportObjectsAfter;
            ImportExportEvents.Import.Failure += ImportObjectsAfter;
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
            SpecialActionsEvents.ProcessMainObject.After += RemoveObsoleteSearchSettings;
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var objectType = infoObj.TypeInfo.ObjectType;
            var parameters = e.Parameters;

            using (new ImportSpecialCaseContext(settings))
            {
                if (objectType == SearchIndexInfo.OBJECT_TYPE)
                {
                    if (!parameters.SkipObjectUpdate && (parameters.ObjectProcessType == ProcessObjectEnum.All))
                    {
                        // Clear last rebuild time of the search index
                        infoObj.SetValue("IndexLastRebuildTime", null);
                    }
                }
            }
        }


        private static void ImportObjectsAfter(object sender, ImportBaseEventArgs e)
        {
            SearchTaskInfoProvider.ProcessTasks(true);
        }


        /// <summary>
        /// Makes sure that search settings are consistent with object definition.
        /// </summary>
        private static void RemoveObsoleteSearchSettings(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObject = e.Object;
            var objectType = infoObject.TypeInfo.ObjectType;

            if (settings.IsOlderVersion && (objectType == DataClassInfo.OBJECT_TYPE || objectType == DataClassInfo.OBJECT_TYPE_SYSTEMTABLE ||
                objectType == PredefinedObjectType.CUSTOMTABLECLASS || objectType == PredefinedObjectType.DOCUMENTTYPE))
            {
                if (infoObject is DataClassInfo dataClassInfo)
                {
                    dataClassInfo.RemoveObsoleteSearchSettings();
                    infoObject.Generalized.SetObject();
                }
            }
        }


        private static void ImportObjectTypeAfter(object sender, ImportDataEventArgs e)
        {
            var settings = e.Settings;
            var data = e.Data;
            var objectType = e.ObjectType;
            var siteObject = e.SiteObjects;

            if (objectType == SearchIndexInfo.OBJECT_TYPE)
            {
                GetAssemblyFiles(settings, data, objectType, siteObject);
            }
        }


        private static void ProcessAdditionalActions_Before(object sender, ImportBaseEventArgs e)
        {
            var settings = e.Settings;

            // Ensure rebuild of indexes
            if (settings.RebuildSearchIndex && settings.SiteIsIncluded)
            {
                // Log site settings progress
                ImportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ImportProvider.RebuildSiteIndexes", "Rebuilding site indexes"));

                // Save the settings
                settings.SavePersistentLog();

                // Get all site indexes and create rebuild tasks
                var indexIdQuery = SearchIndexSiteInfoProvider.GetSearchIndexSites().WhereEquals("IndexSiteID", settings.SiteId).Column("IndexID");
                var indexes = SearchIndexInfoProvider.GetSearchIndexes().WhereIn("IndexID", indexIdQuery).Columns("IndexName, IndexID").TypedResult;
                if (indexes.Any())
                {
                    using (var ctx = new CMSActionContext())
                    {
                        // Allow smart search task creation
                        ctx.CreateSearchTask = true;

                        // Loop through all site indexes and create rebuild tasks
                        foreach (var index in indexes)
                        {
                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Rebuild, null, null, index.IndexName, index.IndexID);
                        }
                    }
                }
            }
        }


        private static void GetAssemblyFiles(SiteImportSettings settings, DataSet data, string objectType, bool siteObject)
        {
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_ASSEMBLIES), false))
            {
                return;
            }

            string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

            // Reset path
            string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";

            // Add target path for site (testing and conversion)
            string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);

            // Get object info
            BaseInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);

            // Get table
            string codeNameColumn = infoObj.Generalized.CodeNameColumn;

            var dt = ObjectHelper.GetTable(data, infoObj);
            if (DataHelper.DataSourceIsEmpty(dt))
            {
                return;
            }

            sourcePath += safeObjectType + "\\";
            sourcePath += @"bin\";

            foreach (DataRow dr in dt.Rows)
            {
                // Import process canceled
                if (settings.ProcessCanceled)
                {
                    ImportProvider.ImportCanceled();
                }

                string objectName = dr[codeNameColumn].ToString();
                string indexSettings = ValidationHelper.GetString(dr["IndexSettings"], null);
                if (!string.IsNullOrEmpty(indexSettings))
                {
                    var sis = new SearchIndexSettings();
                    sis.LoadData(indexSettings);
                    foreach (var item in sis.Items)
                    {
                        var sisi = item.Value;
                        if (sisi != null)
                        {
                            string customSearchIndexAssemblyName = ValidationHelper.GetString(sisi.GetValue("AssemblyName"), string.Empty);
                            if (!string.IsNullOrEmpty(customSearchIndexAssemblyName))
                            {
                                ImportProvider.ImportAssembly(settings, objectType, siteObject, sourcePath, targetPath, objectName, customSearchIndexAssemblyName);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}