using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Base import handler
    /// </summary>
    public class ImportBaseHandler : AdvancedHandler<ImportBaseHandler, ImportBaseEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        public ImportBaseHandler StartEvent(SiteImportSettings settings, TranslationHelper th)
        {
            ImportBaseEventArgs e = new ImportBaseEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th
                };

            return StartEvent(e, true);
        }
    }
}