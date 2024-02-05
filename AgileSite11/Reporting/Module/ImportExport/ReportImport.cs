using System;

using CMS.CMSImportExport;
using CMS.Helpers;

namespace CMS.Reporting
{
    /// <summary>
    /// Handles special actions during the Report import process.
    /// </summary>
    public static class ReportImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetBinaryDataSourcePath.Execute += GetBinaryDataSourcePath_Execute;
        }


        private static void GetBinaryDataSourcePath_Execute(object sender, GetBinaryDataSourcePathEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            if (infoObj.TypeInfo.ObjectType == SavedGraphInfo.OBJECT_TYPE)
            {
                var graph = (SavedGraphInfo)infoObj;
                string extension = MimeTypeHelper.GetExtension(graph.SavedGraphMimeType);

                // Get path
                e.Path = ImportProvider.GetBinaryDataSourcePath(settings, infoObj, "reporting_savedgraph", graph.SavedGraphGUID.ToString(), extension);
            }
        }

        #endregion
    }
}