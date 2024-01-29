using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Process file operation handler
    /// </summary>
    public class ImportProcessFileOperationHandler : AdvancedHandler<ImportProcessFileOperationHandler, ImportProcessFileOperationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        /// <param name="operation">File operation</param>
        public ImportProcessFileOperationHandler StartEvent(SiteImportSettings settings, TranslationHelper th, FileOperation operation)
        {
            ImportProcessFileOperationEventArgs e = new ImportProcessFileOperationEventArgs
                {
                    Settings = settings,
                    Operation = operation,
                    TranslationHelper = th
                };

            return StartEvent(e, true);
        }
    }
}