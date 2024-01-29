using System;
using System.Data;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Handles special actions during the import/export process.
    /// </summary>
    public static class SpecialActions
    {
        private static Lazy<HashSet<string>> settingsKeyWithChangedUserControl = new Lazy<HashSet<string>>(() =>
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                     "CMSStagingServiceAuthentication",
                     "CMSRedirectInvalidCasePages",
                     "CMSControlElement",
                     "CMSDefaulPage",
                     "CMSNewDocumentOrder",
                     "CMSProcessDomainPrefix",
                     "CMSEmailFormat",
                     "CMSRestoreObjects",
                     "CMSPasswordExpirationBehaviour",
                     "CMSPasswordFormat",
                     "CMSCheckPagePermissions",
                     "CMSPOP3AuthenticationMethod",
                     "CMSUseURLsWithTrailingSlash",
                     "CMSImageWatermarkPosition",
                     "CMSRESTServiceTypeEnabled",
                     "CMSEmailEncoding" };

        });


        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetExistingObject.Before += GetExistingObject_Before;
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
            SpecialActionsEvents.ProcessObjectData.After += ProcessObjectData_After;
            SpecialActionsEvents.GetEmptyObject.Execute += GetImportEmptyObject_Execute;
            SpecialActionsEvents.GetBinaryDataSourcePath.Execute += GetBinaryDataSourcePath_Execute;
            SpecialActionsEvents.GetObjectWhereCondition.Execute += GetObjectWhereCondition_Execute;
            SpecialActionsEvents.ProcessMainObject.After += ProcessMainObject_After;
            SpecialActionsEvents.ExportLoadDefaultSelection.Execute += LoadExportDefaultSelection_Execute;
            ImportExportEvents.GetExportData.After += GetExportData_After;
            SpecialActionsEvents.HandleExistingObject.After += KeepFormControlPathAndSettings_ForOlderVersionImport;
        }


        private static void KeepFormControlPathAndSettings_ForOlderVersionImport(object sender, ImportEventArgs e)
        {
            if (e.Settings.IsLowerVersion("11.0"))
            {
                // Keeps original form control settings for specified setting keys changed in v11
                var existing = e.Parameters.ExistingObject as SettingsKeyInfo;
                if ((existing != null) && (existing.SiteID <= 0) && settingsKeyWithChangedUserControl.Value.Contains(existing.KeyName))
                {
                    existing.KeyEditingControlPath = Convert.ToString(existing.GetOriginalValue("KeyEditingControlPath"));
                    existing.KeyFormControlSettings = Convert.ToString(existing.GetOriginalValue("KeyFormControlSettings"));
                }
            }
        }


        private static void GetExportData_After(object sender, ExportGetDataEventArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;
            var ds = e.Data;

            if (objectType == SettingsKeyInfo.OBJECT_TYPE)
            {
                // ### Special cases - additionally modify the data for web template export
                if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_WEBTEMPLATE_EXPORT), false))
                {
                    return;
                }

                // Ensure default values
                if (DataHelper.DataSourceIsEmpty(ds))
                {
                    return;
                }

                GeneralizedInfo settingKeys = ModuleManager.GetReadOnlyObject(SettingsKeyInfo.OBJECT_TYPE);

                var dt = ObjectHelper.GetTable(ds, settingKeys);
                if (!DataHelper.DataSourceIsEmpty(dt))
                {
                    foreach (DataRow drKey in dt.Rows)
                    {
                        if (dt.Columns.Contains("KeyDefaultValue") && string.IsNullOrEmpty(ValidationHelper.GetString(drKey["SiteID"], null)))
                        {
                            // Only for global keys
                            DataHelper.SetDataRowValue(drKey, "KeyValue", drKey["KeyDefaultValue"]);
                        }
                    }
                }
            }
        }


        private static void LoadExportDefaultSelection_Execute(object sender, ExportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            var settings = e.Settings;
            var siteObject = e.SiteObject;

            if (objectType == SiteInfo.OBJECT_TYPE)
            {
                // Site - Always select
                DefaultSelectionParameters parameters = new DefaultSelectionParameters()
                {
                    ObjectType = objectType,
                    SiteObjects = siteObject,
                    ExportType = ExportTypeEnum.All
                };
                settings.LoadDefaultSelection(parameters);

                // Cancel default selection
                e.Select = false;
            }
        }


        private static void ProcessMainObject_After(object sender, ImportEventArgs e)
        {
            var infoObj = e.Object;
            var settings = e.Settings;

            // Load new site info to the settings
            if (infoObj.TypeInfo.ObjectType == SiteInfo.OBJECT_TYPE)
            {
                settings.SiteId = infoObj.Generalized.ObjectID;
            }
        }


        private static void GetObjectWhereCondition_Execute(object sender, GetObjectWhereConditionArgs e)
        {
            var objectType = e.ObjectType;
            var settings = e.Settings;
            if (objectType == SiteInfo.OBJECT_TYPE)
            {
                e.Where.WhereEquals("SiteID", settings.SiteId);
            }
        }


        private static void GetBinaryDataSourcePath_Execute(object sender, GetBinaryDataSourcePathEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            if (infoObj.TypeInfo.ObjectType == MetaFileInfo.OBJECT_TYPE)
            {
                var file = (MetaFileInfo)infoObj;

                // Get path
                e.Path = ImportProvider.GetBinaryDataSourcePath(settings, infoObj, "cms_metafile\\CMSFiles", file.MetaFileGUID.ToString(), file.MetaFileExtension);
            }
        }


        private static void GetImportEmptyObject_Execute(object sender, ImportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            GeneralizedInfo infoObj = null;

            // Get info object
            if (objectType.StartsWithCSafe(ImportExportHelper.CUSTOMTABLE_PREFIX))
            {
                string className = objectType.Substring(ImportExportHelper.CUSTOMTABLE_PREFIX.Length);
                infoObj = ModuleManager.GetReadOnlyObjectByClassName(className);
            }
            else if ((objectType == ImportExportHelper.OBJECT_TYPE_TRANSLATION) || (objectType == ImportExportHelper.CMS_INFO_TYPE))
            {
                // No object representation for import
                infoObj = InfoHelper.EmptyInfo;
            }

            if (infoObj != null)
            {
                e.Object = infoObj;
            }
        }


        private static void ProcessObjectData_After(object sender, ImportEventArgs e)
        {
            var parameters = e.Parameters;
            var infoObj = e.Object;
            var settings = e.Settings;

            using (new ImportSpecialCaseContext(settings))
            {
                if (parameters.ObjectProcessType == ProcessObjectEnum.All)
                {
                    // ### Special case for class - update the default queries and view
                    if (infoObj is DataClassInfo)
                    {
                        DataClassInfo ci = (DataClassInfo)infoObj;

                        // Clear default queries
                        QueryInfoProvider.ClearDefaultQueries(ci, true, true);

                        // Update class structure info, do not log web farm tasks - cache is cleared completely in the end of the import
                        ClassStructureInfo.Remove(ci.ClassName, false);
                    }
                }
            }
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var ti = infoObj.TypeInfo;
            var parameters = e.Parameters;
            var existing = parameters.ExistingObject;

            using (new ImportSpecialCaseContext(settings))
            {
                // Special case for custom and system tables - Keeps class state (whether is system table or custom table) from existing object.
                if (!parameters.SkipObjectUpdate && (parameters.ObjectProcessType == ProcessObjectEnum.All) && (existing != null))
                {
                    var classObj = infoObj as DataClassInfo;
                    if (classObj != null)
                    {
                        var existingClass = (DataClassInfo)existing;
                        classObj.ClassShowAsSystemTable = existingClass.ClassShowAsSystemTable;
                        classObj.ClassIsCustomTable = existingClass.ClassIsCustomTable;
                    }
                }
            }
        }


        private static void GetExistingObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var objectType = infoObj.TypeInfo.ObjectType;

            if (objectType == SiteInfo.OBJECT_TYPE)
            {
                if (settings.SiteInfo == null)
                {
                    throw new Exception("[ImportProvider]: Import settings site object is not initialized.");
                }
                SiteInfo siteObj = (SiteInfo)infoObj;

                // Assign values which should be preserved
                siteObj.SiteID = settings.SiteInfo.SiteID;
                siteObj.Status = SiteStatusEnum.Stopped;

                siteObj.DisplayName = ValidationHelper.GetString(settings.SiteDisplayName, siteObj.DisplayName);
                siteObj.SiteName = ValidationHelper.GetString(settings.SiteName, siteObj.SiteName);
                siteObj.DomainName = ValidationHelper.GetString(settings.SiteDomain, siteObj.DomainName);
                siteObj.Description = ValidationHelper.GetString(settings.SiteDescription, siteObj.Description);

                // Generate new GUID for new site
                siteObj.SiteGUID = !settings.ExistingSite ? Guid.NewGuid() : settings.SiteInfo.SiteGUID;

                // Cancel default processing
                e.Cancel();
            }
        }


        /// <summary>
        /// Executes given SQL script.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="settings">Site import settings</param>
        private static void ExecuteSQLScript(string fileName, SiteImportSettings settings)
        {
            // Log progress
            ImportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportProvider.ConvertingPermissions", "Processing '{0}' conversion SQL script"), fileName));

            // Save the settings
            settings.SavePersistentLog();

            // Check file path
            string filePath = SqlInstallationHelper.GetSQLInstallPath() + "\\Update\\" + fileName;
            if (!File.Exists(filePath))
            {
                throw new Exception("[ImportProvider.ExecuteSQLScript]: SQL script file '" + filePath + "' could not be found.");
            }

            // Execute SQL script
            StreamReader str = File.OpenText(filePath);
            string query = str.ReadToEnd();
            str.Close();

            if (!string.IsNullOrEmpty(query))
            {
                // Resolve macros
                MacroResolver resolver = MacroResolver.GetInstance();
                string[,] macro = new string[2, 2];
                macro[0, 0] = "##SITEID##";
                macro[0, 1] = settings.SiteId.ToString();
                macro[1, 0] = "##PARAMETER##";
                macro[1, 1] = settings.ScriptParameter;

                query = TextHelper.BulkReplace(query, macro);
                query = resolver.ResolveMacros(query);

                // Run the query
                ConnectionHelper.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery, true);
            }
        }
    }
}