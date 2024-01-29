using System;
using System.Collections.Generic;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.EventManager
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
            if (e.ObjectType == EventAttendeeInfo.OBJECT_TYPE)
            {
                e.Folder = ImportExportHelper.DOCUMENTS_FOLDER;
            }
        }

        #endregion
    }
}