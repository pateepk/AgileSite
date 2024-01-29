using System;

using CMS.CMSImportExport;

namespace CMS.Blogs
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetObjectTypeFolder.Execute += GetObjectTypeFolder_Execute;
        }


        private static void GetObjectTypeFolder_Execute(object sender, GetObjectTypeFolderArgs e)
        {
            switch (e.ObjectType)
            {
                case BlogCommentInfo.OBJECT_TYPE:
                case BlogPostSubscriptionInfo.OBJECT_TYPE:
                    e.Folder = ImportExportHelper.DOCUMENTS_FOLDER;
                    break;
            }
        }

        #endregion
    }
}