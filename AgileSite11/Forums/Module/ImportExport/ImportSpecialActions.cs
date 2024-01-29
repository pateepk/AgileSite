using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.Forums
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
            if (objectType == ForumInfo.OBJECT_TYPE)
            {
                string groupWhere = settings.GetObjectWhereCondition(ForumGroupInfo.OBJECT_TYPE, true).ToString(true);

                e.Where.Where("ForumGroupID IN (SELECT GroupID FROM Forums_ForumGroup" + ((groupWhere != null) ? " WHERE " + groupWhere : "") + ")");
                e.CombineWhereCondition = false;
            }
        }


        private static void GetImportEmptyObject_Execute(object sender, ImportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            GeneralizedInfo infoObj = null;

            // Get info object
            if (objectType.StartsWithCSafe(ImportExportHelper.FORUMPOST_PREFIX))
            {
                infoObj = ModuleManager.GetReadOnlyObject(ForumPostInfo.OBJECT_TYPE);
            }

            if (infoObj != null)
            {
                e.Object = infoObj;
            }
        }

        #endregion
    }
}