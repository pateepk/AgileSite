using System;
using System.Collections.Generic;

using CMS.CMSImportExport;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the widget mport process.
    /// </summary>
    internal static class WidgetImport
    {
        /// <summary>
        /// List of removed widget GUIDs.
        /// Widget in this list won't be imported.
        /// </summary>
        private static readonly HashSet<Guid> RemovedWidgetGUIDsInV11 = new HashSet<Guid>
        {
            new Guid("4ed56d71-0849-4eeb-bd1a-1d9e89957611")  // Flash
        };


        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObject.Before += SkipRemovedWidgets;
        }


        /// <summary>
        /// Filters-out old widgets which were removed from the product in version 11.
        /// That way these widgets won't be imported back to the database even if an older import package is being imported.
        /// </summary>
        private static void SkipRemovedWidgets(object sender, ImportEventArgs e)
        {
            var infoObj = e.Object;

            if (infoObj.TypeInfo.ObjectType == WidgetInfo.OBJECT_TYPE)
            {
                bool removed = e.Settings.IsLowerVersion("11.0") && RemovedWidgetGUIDsInV11.Contains(infoObj.Generalized.ObjectGUID);
                if (removed)
                {
                    e.Cancel();
                }
            }
        }
    }
}