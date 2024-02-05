using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Base;
using CMS.Helpers;

namespace CMS.Membership
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
            SpecialActionsEvents.GetExistingObject.Before += GetExistingObject_Before;
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
            SpecialActionsEvents.ProcessAdditionalActions.Before += ProcessAdditionalActions_Before;
            SpecialActionsEvents.ProcessDeleteTask.Before += ProcessDeleteTask_Before;
            ImportExportEvents.ImportChild.Before += ImportChild_Before;
            ImportExportEvents.ImportObjectType.After += ImportObjectTypeAfter;
        }


        private static void ImportObjectTypeAfter(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            var settings = e.Settings;
            var th = e.TranslationHelper;

            // ### Special case for CMSDefaultUserID settings
            if (objectType == SettingsKeyInfo.OBJECT_TYPE)
            {
                using (new ImportSpecialCaseContext(settings))
                {
                    // Translate global settings
                    SettingsKeyInfo globalSki = SettingsKeyInfoProvider.GetSettingsKeyInfo("CMSDefaultUserID");
                    if ((globalSki != null) && !string.IsNullOrEmpty(globalSki.KeyValue))
                    {
                        int newUserId = th.GetNewID(UserInfo.OBJECT_TYPE, ValidationHelper.GetInteger(globalSki.KeyValue, 0), "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                        if (newUserId > 0)
                        {
                            globalSki.KeyValue = newUserId.ToString();
                            SettingsKeyInfoProvider.SetSettingsKeyInfo(globalSki);
                        }
                    }

                    if (settings.SiteIsIncluded)
                    {
                        // Translate local settings
                        SettingsKeyInfo localSki = SettingsKeyInfoProvider.GetSettingsKeyInfo(settings.SiteName + ".CMSDefaultUserID");
                        if ((localSki != null) && !string.IsNullOrEmpty(localSki.KeyValue))
                        {
                            int newUserId = th.GetNewID(UserInfo.OBJECT_TYPE, ValidationHelper.GetInteger(localSki.KeyValue, 0), "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                            if (newUserId > 0)
                            {
                                localSki.KeyValue = newUserId.ToString();
                                SettingsKeyInfoProvider.SetSettingsKeyInfo(localSki);
                            }
                        }
                    }
                }
            }
        }


        private static void ImportChild_Before(object sender, ImportEventArgs e)
        {
            var infoObj = e.ParentObject;
            var child = e.Object;

            // ### Special case - Ensure correct user settings GUID
            if (infoObj.TypeInfo.ObjectType == UserInfo.OBJECT_TYPE)
            {
                if (child.TypeInfo.ObjectType == UserSettingsInfo.OBJECT_TYPE)
                {
                    child.SetValue("UserSettingsUserGUID", infoObj.GetValue("UserGUID"));
                }
            }
        }


        private static void ProcessDeleteTask_Before(object sender, ProcessDeleteTaskArgs e)
        {
            var settings = e.Settings;
            var existingObj = e.Object;
            var taskObj = e.Task;

            // Do not delete current user
            if (existingObj.TypeInfo.ObjectType == UserInfo.OBJECT_TYPE)
            {
                if (existingObj.GetIntegerValue("UserID", 0) == settings.CurrentUser.UserID)
                {
                    // Log warning about task not processed
                    ImportProvider.LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportProvider.TaskUserNotProcessed", "Delete task '{0}' was not processed, because it would delete current user. Sign in as different user and delete this user manually."), taskObj.TaskTitle));
                    e.Cancel();
                }
            }
        }


        private static void ProcessAdditionalActions_Before(object sender, ImportBaseEventArgs e)
        {
            // Update user counts (Count of the objects like forum posts, blog posts, blog comments etc.)
            using (CMSActionContext ctx = new CMSActionContext())
            {
                ctx.UpdateUserCounts = true;
                UserInfoProvider.UpdateUserCounts(ActivityPointsEnum.All, 0, 0);
            }
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var infoObj = e.Object;
            var parameters = e.Parameters;
            var existing = parameters.ExistingObject;

            // Ensure consistency between user and settings
            if (infoObj.TypeInfo.ObjectType != UserInfo.OBJECT_TYPE)
            {
                return;
            }

            UserInfo importUser = (UserInfo)infoObj;
            if (existing != null)
            {
                if (!parameters.SkipObjectUpdate)
                {
                    UserInfo existingUser = (UserInfo)existing;

                    importUser.UserSettings.UserSettingsID = existingUser.UserSettings.UserSettingsID;
                    importUser.UserSettings.UserSettingsUserGUID = existingUser.UserGUID;
                    importUser.UserSettings.UserSettingsUserID = existingUser.UserID;
                }
            }
        }


        private static void GetExistingObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            using (new ImportSpecialCaseContext(settings))
            {
                if (infoObj.TypeInfo.ObjectType == UserInfo.OBJECT_TYPE)
                {
                    if (settings.SiteInfo != null)
                    {
                        // If site prefixed user - clone current user with new GUID prefix 
                        UserInfo ui = (UserInfo)infoObj;
                        if (UserInfoProvider.IsSitePrefixedUser(ui.UserName))
                        {
                            ui.UserID = 0;
                            ui.UserGUID = Guid.NewGuid();
                            string plainUserName = UserInfoProvider.TrimSitePrefix(ui.UserName);
                            ui.UserName = UserInfoProvider.EnsureSitePrefixUserName(plainUserName, settings.SiteInfo);
                        }
                    }
                }
            }
        }

        #endregion
    }
}