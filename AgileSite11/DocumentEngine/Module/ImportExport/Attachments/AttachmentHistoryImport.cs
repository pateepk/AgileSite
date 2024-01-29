using System;

using CMS.CMSImportExport;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the Attachment history import process.
    /// </summary>
    internal static class AttachmentHistoryImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetBinaryDataSourcePath.Execute += GetBinaryDataSourcePath_Execute;
        }


        private static void GetBinaryDataSourcePath_Execute(object sender, GetBinaryDataSourcePathEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            var attachment = infoObj as AttachmentHistoryInfo;
            if (attachment != null)
            {
                var extension = attachment.AttachmentExtension;
                var fileName = attachment.AttachmentHistoryGUID.ToString();
                
                // Get path
                e.Path = ImportProvider.GetBinaryDataSourcePath(settings, infoObj, "cms_attachmenthistory", fileName, extension);
            }
        }

        #endregion
    }
}