using System;

using CMS.CMSImportExport;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Import export handler
    /// </summary>
    public class DocumentImportHandler : AdvancedHandler<DocumentImportHandler, DocumentImportEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        public DocumentImportHandler StartEvent(SiteImportSettings settings, TranslationHelper th)
        {
            DocumentImportEventArgs e = new DocumentImportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th
                };

            return StartEvent(e, true);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        /// <param name="document">Imported document</param>
        public DocumentImportHandler StartEvent(SiteImportSettings settings, TranslationHelper th, TreeNode document)
        {
            DocumentImportEventArgs e = new DocumentImportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th,
                    Document = document
                };

            return StartEvent(e, true);
        }
    }
}