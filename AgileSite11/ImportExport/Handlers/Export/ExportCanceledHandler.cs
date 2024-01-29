using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export canceled handler
    /// </summary>
    public class ExportCanceledHandler : SimpleHandler<ExportCanceledHandler, ExportBaseEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Export settings</param>
        public ExportBaseEventArgs StartEvent(SiteExportSettings settings)
        {
            var e = new ExportBaseEventArgs
                {
                    Settings = settings
                };

            return StartEvent(e);
        }
    }
}