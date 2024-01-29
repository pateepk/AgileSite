using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import error handler
    /// </summary>
    public class ImportErrorHandler : SimpleHandler<ImportErrorHandler, ImportErrorEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        /// <param name="ex">Exception thrown within the import process</param>
        public ImportErrorEventArgs StartEvent(SiteImportSettings settings, TranslationHelper th, Exception ex)
        {
            var e = new ImportErrorEventArgs
                {
                    Settings = settings,
                    Exception = ex,
                    TranslationHelper = th
                };

            return StartEvent(e);
        }
    }
}