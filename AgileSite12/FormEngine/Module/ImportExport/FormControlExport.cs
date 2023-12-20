using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.FormEngine
{
    /// <summary>
    /// Handles special actions during the Form control export process.
    /// </summary>
    internal static class FormControlExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += Export_After;
            ImportExportEvents.SingleExportSelection.Execute += SingleExportSelection_Execute;
        }


        private static void Export_After(object sender, ExportEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == FormUserControlInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                if (!settings.CopyFiles)
                {
                    return;
                }

                var infoObj = ModuleManager.GetReadOnlyObject(objectType);
                var data = e.Data;
                var dt = ObjectHelper.GetTable(data, infoObj);

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(dt))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);
                string webSitePath = settings.WebsitePath;
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
                string targetFilesPath = targetPath;
                string sourcePath = webSitePath;

                targetPath = DirectoryHelper.CombinePath(targetFilesPath, safeObjectType) + "\\";

                ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingFiles", "Copying '{0}' files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                foreach (DataRow dr in dt.Rows)
                {
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    string objectFileName = dr["UserControlFileName"].ToString();

                    if (!String.IsNullOrEmpty(objectFileName) && !"inherited".Equals(objectFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        string targetObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, FormUserControlInfoProvider.FormUserControlsDirectory, targetPath);
                        string sourceObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, FormUserControlInfoProvider.FormUserControlsDirectory, sourcePath);

                        try
                        {
                            ExportProvider.CopySourceFiles(sourceObjectPath, targetObjectPath, webSitePath);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }


        private static void SingleExportSelection_Execute(object sender, SingleExportSelectionEventArgs e)
        {
            var infoObj = e.InfoObject;

            if (infoObj.TypeInfo.ObjectType.ToLowerCSafe() == FormUserControlInfo.OBJECT_TYPE)
            {
                e.SelectedObjects.AddRange(GetInheritedUserControls(infoObj));
            }
        }


        /// <summary>
        /// Get list of inherited user controls for user control object
        /// </summary>
        /// <param name="infoObj">User control object</param>
        private static List<string> GetInheritedUserControls(GeneralizedInfo infoObj)
        {
            var userControls = new List<string>();

            int parentUserControlId = ValidationHelper.GetInteger(infoObj.GetValue("UserControlParentID"), 0);
            if (parentUserControlId != 0)
            {
                var fuci = FormUserControlInfoProvider.GetFormUserControlInfo(parentUserControlId);
                if (fuci != null)
                {
                    userControls.Add(fuci.UserControlCodeName);
                    userControls.AddRange(GetInheritedUserControls(fuci));
                }
            }

            return userControls;
        }

        #endregion
    }
}