using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document export handler
    /// </summary>
    public class DocumentsExportHandler : AdvancedHandler<DocumentsExportHandler, DocumentsExportEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="th">Translation helper</param>
        /// <param name="objectType">Exported object type</param>
        /// <param name="siteObjects">Flag whether site or global objects are exported</param>
        public DocumentsExportHandler StartEvent(SiteExportSettings settings, TranslationHelper th, string objectType, bool siteObjects)
        {
            DocumentsExportEventArgs e = new DocumentsExportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th,
                    ObjectType = objectType,
                    SiteObjects = siteObjects
                };

            return StartEvent(e, true);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="th">Translation helper</param>
        public DocumentsExportHandler StartEvent(SiteExportSettings settings, TranslationHelper th)
        {
            DocumentsExportEventArgs e = new DocumentsExportEventArgs
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
        public DocumentsExportHandler StartEvent(SiteExportSettings settings, TranslationHelper th, DataSet data)
        {
            DocumentsExportEventArgs e = new DocumentsExportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = th,
                    Data = data
                };

            return StartEvent(e, true);
        }
    }
}