using System;
using System.Data;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export handler
    /// </summary>
    public class ExportHandler : AdvancedHandler<ExportHandler, ExportEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="th">Translation helper</param>
        public ExportHandler StartEvent(SiteExportSettings settings, TranslationHelper th)
        {
            ExportEventArgs e = new ExportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th
                };

            return StartEvent(e, true);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="th">Translation helper</param>
        /// <param name="data">Exported data</param>
        public ExportHandler StartEvent(SiteExportSettings settings, TranslationHelper th, DataSet data)
        {
            ExportEventArgs e = new ExportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th,
                    Data = data
                };

            return StartEvent(e, true);
        }
    }
}