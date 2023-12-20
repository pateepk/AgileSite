using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import canceled handler
    /// </summary>
    public class ImportCanceledHandler : SimpleHandler<ImportCanceledHandler, ImportBaseEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        public ImportBaseEventArgs StartEvent(SiteImportSettings settings, TranslationHelper th)
        {
            var e = new ImportBaseEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th
                };

            return StartEvent(e);
        }
    }
}