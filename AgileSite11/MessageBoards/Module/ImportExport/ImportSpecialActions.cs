using System;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.MessageBoards
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
            SpecialActionsEvents.GetEmptyObject.Execute += GetImportEmptyObject_Execute;
            SpecialActionsEvents.GetObjectWhereCondition.Execute += GetObjectWhereCondition_Execute;
        }


        private static void GetObjectWhereCondition_Execute(object sender, GetObjectWhereConditionArgs e)
        {
            var objectType = e.ObjectType;
            var settings = e.Settings;
            if (objectType == BoardInfo.OBJECT_TYPE)
            {
                e.Where.Where("BoardDocumentID IN (SELECT DocumentID FROM " + SystemViewNames.View_CMS_Tree_Joined + " WHERE NodeSiteID=" + settings.SiteId + ")");
                e.CombineWhereCondition = false;
            }
        }


        private static void GetImportEmptyObject_Execute(object sender, ImportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            GeneralizedInfo infoObj = null;

            // Get info object
            if (objectType.StartsWithCSafe(ImportExportHelper.BOARDMESSAGE_PREFIX))
            {
                infoObj = ModuleManager.GetReadOnlyObject(BoardMessageInfo.OBJECT_TYPE);
            }

            if (infoObj != null)
            {
                e.Object = infoObj;
            }
        }

        #endregion
    }
}