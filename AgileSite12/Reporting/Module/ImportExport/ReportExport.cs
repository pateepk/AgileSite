using System;
using System.Collections.Generic;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.Reporting
{
    /// <summary>
    /// Handles special actions during the Report export process.
    /// </summary>
    internal static class ReportExport
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
            if (e.ObjectType == SavedReportInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                if (!settings.CopyFiles)
                {
                    return;
                }

                // Get info object
                const string objectType = SavedGraphInfo.OBJECT_TYPE;
                
                var infoObj = ModuleManager.GetReadOnlyObject(objectType);

                var data = e.Data;

                var dt = ObjectHelper.GetTable(data, infoObj);

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(dt))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
                targetPath = DirectoryHelper.CombinePath(targetPath, safeObjectType) + "\\";

                // Log process
                ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingFiles", "Copying '{0}' files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                // Process all graphs
                foreach (DataRow dr in dt.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    string guid = ValidationHelper.GetString(dr["SavedGraphGUID"], "");
                    string mimetype = ValidationHelper.GetString(dr["SavedGraphMimeType"], "");
                    string extension = MimeTypeHelper.GetExtension(mimetype);
                    string fileName = guid + extension;

                    if (!ExportProvider.IsFileExcluded(fileName))
                    {
                        // Get the binary
                        object binary = DataHelper.GetDataRowValue(dr, "SavedGraphBinary");
                        byte[] fileBinary = null;
                        if (binary != DBNull.Value)
                        {
                            fileBinary = (byte[])binary;
                        }
                        if (fileBinary == null)
                        {
                            // Get the graph object
                            int graphId = ValidationHelper.GetInteger(dr["SavedGraphID"], 0);

                            GeneralizedInfo graph = ProviderHelper.GetInfoById(SavedGraphInfo.OBJECT_TYPE, graphId);
                            if (graph != null)
                            {
                                fileBinary = (byte[])graph.GetValue("SavedGraphBinary");
                            }
                        }

                        // Save the file
                        if ((fileBinary != null) && (guid != ""))
                        {
                            try
                            {
                                string filePath = targetPath + DirectoryHelper.CombinePath(guid.Substring(0, 2), fileName);
                                filePath = ImportExportHelper.GetExportFilePath(filePath);

                                // Copy file
                                DirectoryHelper.EnsureDiskPath(filePath, settings.WebsitePath);
                                File.WriteAllBytes(filePath, fileBinary);

                                // Clear the binary
                                DataHelper.SetDataRowValue(dr, "SavedGraphBinary", null);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }
        
        
        private static void SingleExportSelection_Execute(object sender, SingleExportSelectionEventArgs e)
        {
            var infoObj = e.InfoObject;
            var settings = e.Settings;

            if (infoObj.TypeInfo.ObjectType.ToLowerCSafe() == ReportInfo.OBJECT_TYPE)
            {
                var selectedReports = GetSavedReports(infoObj);
                // Add saved reports
                if (selectedReports.Count != 0)
                {
                    settings.SetObjectsProcessType(ProcessObjectEnum.Selected, SavedReportInfo.OBJECT_TYPE, false);
                    settings.SetSelectedObjects(selectedReports, SavedReportInfo.OBJECT_TYPE, false);
                }
            }
        }


        /// <summary>
        /// Get list of saved reports for report object
        /// </summary>
        /// <param name="infoObj">Report object</param>
        private static List<string> GetSavedReports(GeneralizedInfo infoObj)
        {
            var reports = new List<string>();

            var data = SavedReportInfoProvider.GetSavedReports().WhereEquals("SavedReportReportID", infoObj.ObjectID).Column("SavedReportGUID");
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    reports.Add(dr["SavedReportGUID"].ToString());
                }
            }


            return reports;
        }

        #endregion
    }
}