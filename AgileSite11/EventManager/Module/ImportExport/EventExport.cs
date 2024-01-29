using System;

using CMS.CMSImportExport;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.EventManager
{
    /// <summary>
    /// Handles special actions during the Event export process.
    /// </summary>
    internal static class EventExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            DocumentImportExportEvents.ExportDocuments.After += ExportDocuments_After;
        }


        private static void ExportDocuments_After(object sender, DocumentsExportEventArgs e)
        {
            var settings = e.Settings;
            var data = e.Data;
            var th = e.TranslationHelper;

            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            // No data to be exported since the module is not loaded
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.EVENTMANAGER))
            {
                return;
            }

            // Event attendees
            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_EVENT_ATTENDEES), true))
            {
                var where = ImportExportHelper.GetDocumentsDataWhereCondition(EventAttendeeInfo.OBJECT_TYPE, data, "AttendeeEventNodeID", "NodeID");
                DocumentExport.ExportRelatedDocumentsData(settings, EventAttendeeInfo.OBJECT_TYPE, th, false, where);
            }
        }

        #endregion
    }
}