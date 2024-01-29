using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing UserSettingsInfo management.
    /// </summary>
    public class UserSettingsInfoProvider : AbstractInfoProvider<UserSettingsInfo, UserSettingsInfoProvider>
    {
        #region "Variables"

        private static bool mDeleteCustomAvatars = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Defines whether to delete custom avatar along with user settings object (if exists).
        /// </summary>
        public static bool DeleteCustomAvatars
        {
            get
            {
                return mDeleteCustomAvatars;
            }
            set
            {
                mDeleteCustomAvatars = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns UserSettings info for specified user.
        /// </summary>
        /// <param name="userId">User whose Settings info is to be returned</param>
        public static UserSettingsInfo GetUserSettingsInfoByUser(int userId)
        {
            return ProviderObject.GetUserSettingsInfoByUserInternal(userId);
        }


        /// <summary>
        /// Returns the UserSettingsInfo structure for the specified userSettings.
        /// </summary>
        /// <param name="userSettingsId">UserSettings id</param>
        public static UserSettingsInfo GetUserSettingsInfo(int userSettingsId)
        {
            return ProviderObject.GetInfoById(userSettingsId);
        }


        /// <summary>
        /// Returns the query for all user settings.
        /// </summary>   
        public static ObjectQuery<UserSettingsInfo> GetUserSettings()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified userSettings.
        /// </summary>
        /// <param name="userSettings">UserSettings to set</param>
        public static void SetUserSettingsInfo(UserSettingsInfo userSettings)
        {
            ProviderObject.SetInfo(userSettings);
        }


        /// <summary>
        /// Deletes specified userSettings.
        /// </summary>
        /// <param name="infoObj">UserSettings object</param>
        public static void DeleteUserSettingsInfo(UserSettingsInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified userSettings.
        /// </summary>
        /// <param name="userSettingsId">UserSettings id</param>
        public static void DeleteUserSettingsInfo(int userSettingsId)
        {
            UserSettingsInfo infoObj = GetUserSettingsInfo(userSettingsId);
            DeleteUserSettingsInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns UserSettings info for specified user.
        /// </summary>
        /// <param name="userId">User whose Settings info is to be returned</param>
        protected virtual UserSettingsInfo GetUserSettingsInfoByUserInternal(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            return GetObjectQuery().WhereEquals("UserSettingsUserID", userId).FirstObject;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(UserSettingsInfo info)
        {
            if (info != null)
            {
                // Set user dialog settings
                info.SetValue("UserDialogsConfiguration", info.UserDialogsConfiguration.GetData());

                // Set user xml data
                info.SetValue("UserRegistrationInfo", info.UserRegistrationInfo.GetData());

                // Set user preferences
                info.SetValue("UserPreferences", info.UserPreferences.GetData());

                // Get user to invalidate object in hashtables
                UserInfo ui = UserInfoProvider.GetUserInfo(info.UserSettingsUserID);

                // Set user activation date
                if (info.UserActivationDate == DateTimeHelper.ZERO_TIME)
                {
                    if ((ui != null) && (ui.Enabled))
                    {
                        info.UserActivationDate = DateTime.Now;
                    }
                }

                base.SetInfo(info);

                // Update user in hashtables, invalidate user info, propagate change in web farm environment
                if (ui != null)
                {
                    ui.UpdateUserSettings(info);
                }

                if (SearchIndexInfoProvider.SearchTypeEnabled(UserInfo.OBJECT_TYPE))
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, UserInfo.OBJECT_TYPE, UserInfo.TYPEINFO.IDColumn, info.UserSettingsUserID.ToString(), info.UserSettingsUserID);
                }
            }
            else
            {
                throw new Exception("[UserSettingsInfoProvider.SetUserSettingsInfo]: No UserSettingsInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(UserSettingsInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);

                // Invalidate user info, propagate change in web farm environment                 
                UserInfo ui = UserInfoProvider.GetUserInfo(info.UserSettingsUserID);
                if (ui != null)
                {
                    ui.Generalized.Invalidate(false);
                }
            }
        }

        #endregion
    }
}