using System;

using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.EventManager
{
    /// <summary>
    /// Handles special actions during the Event import process.
    /// </summary>
    internal static class EventImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            DocumentImportExportEvents.ImportDocuments.After += ImportDocuments_After;
        }


        private static void ImportDocuments_After(object sender, DocumentsImportEventArgs e)
        {
            var settings = e.Settings;
            var th = e.TranslationHelper;
            var nodesIds = e.ImportedNodeIDs;

            // Import event attendees
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.EVENTMANAGER) || !LicenseHelper.CheckFeature(settings.CurrentUrl, FeatureEnum.EventManager) || !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_EVENT_ATTENDEES), true))
            {
                return;
            }

            ImportProvider.ImportObjectType(settings, EventAttendeeInfo.OBJECT_TYPE, false, th, ProcessObjectEnum.All, nodesIds);
        }

        #endregion
    }
}