using System;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Community
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
            SpecialActionsEvents.ImportEnsureAutomaticSelection.Execute += EnsureAutomaticSelection_Execute;
            SpecialActionsEvents.GetObjectWhereCondition.Execute += GetObjectWhereCondition_Execute;
        }


        private static void GetObjectWhereCondition_Execute(object sender, GetObjectWhereConditionArgs e)
        {
            var objectType = e.ObjectType;
            var settings = e.Settings;
            if (objectType == GroupMemberInfo.OBJECT_TYPE)
            {
                string communityGroupWhere = settings.GetObjectWhereCondition(GroupInfo.OBJECT_TYPE, true).ToString(true);

                e.Where.Where("MemberGroupID IN (SELECT GroupID FROM Community_Group" + ((communityGroupWhere != null) ? " WHERE " + communityGroupWhere : "") + ")");
                e.CombineWhereCondition = false;
            }
        }


        private static void EnsureAutomaticSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == GroupMemberInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                e.ProcessDependency = !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_GROUP_MEMBERSHIP), false);
            }
        }

        #endregion
    }
}