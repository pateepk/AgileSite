using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export error handler
    /// </summary>
    public class ExportErrorHandler : SimpleHandler<ExportErrorHandler, ExportErrorEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="ex">Exception thrown within the export process</param>
        public ExportErrorEventArgs StartEvent(SiteExportSettings settings, Exception ex)
        {
            var e = new ExportErrorEventArgs
                {
                Settings = settings,
                Exception = ex
            };

            return StartEvent(e);
        }
    }
}