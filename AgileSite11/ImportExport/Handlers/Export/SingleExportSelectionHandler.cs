using System;
using System.Collections.Generic;
using System.Data;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Single export object selection handler
    /// </summary>
    public class SingleExportSelectionHandler : SimpleHandler<SingleExportSelectionHandler, SingleExportSelectionEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="infoObj">Exported object</param>
        /// <param name="selectedObjects">List of selected objects</param>
        public SingleExportSelectionEventArgs StartEvent(SiteExportSettings settings, GeneralizedInfo infoObj, List<string> selectedObjects)
        {
            var e = new SingleExportSelectionEventArgs
                {
                    Settings = settings,
                    InfoObject = infoObj,
                    SelectedObjects = selectedObjects
                };

            return StartEvent(e);
        }
    }
}