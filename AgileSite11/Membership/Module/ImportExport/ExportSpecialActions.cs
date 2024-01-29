using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Base;
using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Handles special actions during the export process.
    /// </summary>
    internal static class ExportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetSelectionWhereCondition.Execute += ExportSelection_Execute;
            ImportExportEvents.GetExportData.After += GetExportData_After;
        }


        private static void GetExportData_After(object sender, ExportGetDataEventArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;
            var ds = e.Data;

            if (objectType == UserInfo.OBJECT_TYPE)
            {
                // ### Special cases - additionally modify the data for web template export
                if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_WEBTEMPLATE_EXPORT), false))
                {
                    return;
                }

                if (DataHelper.DataSourceIsEmpty(ds))
                {
                    return;
                }

                // Ensure users splash screen
                GeneralizedInfo userSettings = ModuleManager.GetReadOnlyObject(UserSettingsInfo.OBJECT_TYPE);

                var dt = ObjectHelper.GetTable(ds, userSettings);
                if (!DataHelper.DataSourceIsEmpty(dt))
                {
                    DataHelper.ChangeBooleanValues(dt, "UserShowIntroductionTile", false, true, null);
                    DataHelper.ChangeStringValues(dt, "UserDialogsConfiguration", null, null);

                    DataHelper.ChangeValuesToNull(dt, "UserUsedWebParts", null);
                    DataHelper.ChangeValuesToNull(dt, "UserUsedWidgets", null);

                    DataHelper.ChangeValuesToNull(dt, "UserPasswordLastChanged", null);
                }

                GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE);

                // Remove user dialog configuration and other settings
                dt = ObjectHelper.GetTable(ds, infoObj);
                if (!DataHelper.DataSourceIsEmpty(dt))
                {
                    DataHelper.ChangeValuesToNull(dt, "LastLogon", null);
                    DataHelper.ChangeValuesToNull(dt, "UserLastLogonInfo", null);
                    DataHelper.ChangeValuesToNull(dt, "UserLastModified", null);
                }
            }
        }


        private static void ExportSelection_Execute(object sender, ExportSelectionArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;

            if (e.Select)
            {
                #region "Special cases"

                // Add additional where condition for special cases
                switch (objectType.ToLowerCSafe())
                {
                    // Include default avatars
                    case AvatarInfo.OBJECT_TYPE:
                        {
                            var avatarWhere = "AvatarID IN (SELECT AvatarID FROM CMS_Avatar WHERE (DefaultMaleUserAvatar = 1 OR DefaultFemaleUserAvatar = 1 OR DefaultGroupAvatar = 1 OR DefaultUserAvatar = 1 OR AvatarIsCustom = 0))";
                            e.Where.Or().Where(avatarWhere);
                        }
                        break;

                    // Include users assigned to the exported site
                    case UserInfo.OBJECT_TYPE:
                        {
                            string userWhere = String.Format("UserID IN (SELECT UserID FROM CMS_UserSite WHERE SiteID = {0})", settings.SiteId);
                            e.Where.Or().Where(userWhere);
                        }
                        break;
                }

                #endregion

                // Add additional where condition due to depending objects
                switch (objectType.ToLowerCSafe())
                {
                    // Special cases
                    case BadgeInfo.OBJECT_TYPE:
                        e.IncludeDependingObjects = false;
                        break;
                }
            }
        }

        #endregion
    }
}