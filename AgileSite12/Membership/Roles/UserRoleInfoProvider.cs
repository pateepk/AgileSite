using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Search;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing UserRoleInfo management.
    /// </summary>
    public class UserRoleInfoProvider : AbstractInfoProvider<UserRoleInfo, UserRoleInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the UserRoleInfo structure for the specified userRole.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="roleId">RoleID</param>
        public static UserRoleInfo GetUserRoleInfo(int userId, int roleId)
        {
            return ProviderObject.GetUserRoleInfoInternal(userId, roleId);
        }


        /// <summary>
        /// Returns the query for all relationships between users and roles.
        /// </summary>   
        public static ObjectQuery<UserRoleInfo> GetUserRoles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified userRole.
        /// </summary>
        /// <param name="userRole">UserRole to set</param>
        public static void SetUserRoleInfo(UserRoleInfo userRole)
        {
            ProviderObject.SetInfo(userRole);
        }


        /// <summary>
        /// Deletes specified userRole.
        /// </summary>
        /// <param name="infoObj">UserRole object</param>
        public static void DeleteUserRoleInfo(UserRoleInfo infoObj)
        {
            ProviderObject.DeleteUserRoleInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified userRole.
        /// </summary>
        /// <param name="userInfo">User which will be removed from role</param>
        /// <param name="roleInfo">Role from which will be the user removed</param>
        public static void DeleteUserRoleInfo(UserInfo userInfo, RoleInfo roleInfo)
        {
            if ((userInfo != null) && (roleInfo != null))
            {
                var userRoleInfo = GetUserRoleInfo(userInfo.UserID, roleInfo.RoleID);

                ProviderObject.DeleteUserRoleInfoInternal(userRoleInfo, userInfo, roleInfo);
            }
        }


        /// <summary>
        /// Adds specified user to the role.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="roleId">RoleID</param>
        /// <param name="dt">Date till user role connection is valid</param>
        public static void AddUserToRole(int userId, int roleId, DateTime dt)
        {
            // Get infos
            UserInfo ui = UserInfoProvider.GetUserInfo(userId);
            RoleInfo ri = RoleInfoProvider.GetRoleInfo(roleId);

            // Set relation
            AddUserToRole(ui, ri, dt);
        }


        /// <summary>
        /// Adds specified user to the site.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="roleId">RoleID</param>
        public static void AddUserToRole(int userId, int roleId)
        {
            AddUserToRole(userId, roleId, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Adds specified user to the site.
        /// </summary>
        /// <param name="ui">UserInfo</param>
        /// <param name="ri">RoleInfo</param>        
        public static void AddUserToRole(UserInfo ui, RoleInfo ri)
        {
            AddUserToRole(ui, ri, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Adds specified user to the site.
        /// </summary>
        /// <param name="ui">UserInfo</param>
        /// <param name="ri">RoleInfo</param>
        /// <param name="dt">Date till user role connection is valid</param>
        public static void AddUserToRole(UserInfo ui, RoleInfo ri, DateTime dt)
        {
            if ((ui != null) && (ri != null))
            {
                // Create new binding
                UserRoleInfo infoObj = ProviderObject.CreateInfo();

                infoObj.UserID = ui.UserID;
                infoObj.RoleID = ri.RoleID;
                infoObj.ValidTo = dt;

                var genInfo = infoObj.Generalized;

                // Do not log synchronization for group roles
                if (ri.RoleGroupID != 0)
                {
                    genInfo.StoreSettings();
                    genInfo.LogSynchronization = SynchronizationTypeEnum.None;
                }

                // Save to the database
                SetUserRoleInfo(infoObj);

                genInfo.RestoreSettings();

                ui.Generalized.Invalidate(false);
                
                // Insert information about event to eventlog.
                EventLogProvider.LogEvent(EventType.INFORMATION, "Add user to role", "USERTOROLE", "User " + ui.FullName + " has been added to role " + ri.RoleDisplayName + ".", RequestContext.RawURL, 0, null, 0, null, RequestContext.UserHostAddress);

                // Log search task
                if (SearchIndexInfoProvider.SearchTypeEnabled(UserInfo.OBJECT_TYPE) && IsSearchRequired())
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, UserInfo.OBJECT_TYPE, ui.TypeInfo.IDColumn, ui.UserID.ToString(), ui.UserID);
                }
            }
        }


        internal static void RemoveUserFromSiteRoles(int userId, int siteId)
        {
            GetUserRoles()
                .WhereID("UserID", userId)
                .WhereIn("RoleID", RoleInfoProvider.GetRoles()
                                                   .Columns("RoleID")
                                                   .WhereID("SiteID", siteId))
                .ForEachObject(userRoleInfo =>
                    userRoleInfo.Delete());
        }


        /// <summary>
        /// Returns true if the user is member of specified role.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        public static bool IsUserInRole(int userId, int roleId)
        {
            UserRoleInfo uri = GetUserRoleInfo(userId, roleId);
            return (uri != null);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns true if exists at least one user index with role condition
        /// </summary>
        private static bool IsSearchRequired()
        {
            // Get user index ids
            List<int> ids = SearchIndexInfoProvider.GetIndexIDs(new List<string>() { UserInfo.OBJECT_TYPE });
            if (ids != null)
            {
                // Loop thru all ids
                foreach (int id in ids)
                {
                    // Get index info object
                    SearchIndexInfo sii = SearchIndexInfoProvider.GetSearchIndexInfo(id);
                    // Check whether object is defined and has settings
                    if ((sii != null) && (sii.IndexSettings.Items != null))
                    {
                        foreach (SearchIndexSettingsInfo sisi in sii.IndexSettings.Items.Values)
                        {
                            // Try get roles conditions
                            string inRoles = ValidationHelper.GetString(sisi.GetValue("UserInRoles"), String.Empty);
                            string notInRoles = ValidationHelper.GetString(sisi.GetValue("UserNotInRoles"), String.Empty);

                            // If role condition is defined return true => search task is required
                            if (!String.IsNullOrEmpty(inRoles) || !String.IsNullOrEmpty(notInRoles))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            // Search task is not required
            return false;
        }


        /// <summary>
        /// Returns the UserRoleInfo structure for the specified userRole.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="roleId">RoleID</param>
        protected virtual UserRoleInfo GetUserRoleInfoInternal(int userId, int roleId)
        {
            var condition = new WhereCondition()
                .WhereEquals("UserID", userId)
                .WhereEquals("RoleID", roleId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Deletes specified userRole.
        /// </summary>
        /// <param name="infoObj">UserRole object</param>
        protected virtual void DeleteUserRoleInfoInternal(UserRoleInfo infoObj)
        {
            if (infoObj != null)
            {
                var userInfo = UserInfoProvider.GetUserInfo(infoObj.UserID);
                var roleInfo = RoleInfoProvider.GetRoleInfo(infoObj.RoleID);

                DeleteUserRoleInfoInternal(infoObj, userInfo, roleInfo);
            }
        }


        /// <summary>
        /// Deletes specified userRole.
        /// </summary>
        /// <param name="infoObj">UserRole object</param>
        /// <param name="userInfo">User which will be removed from role</param>
        /// <param name="roleInfo">Role from which will be the user removed</param>
        protected virtual void DeleteUserRoleInfoInternal(UserRoleInfo infoObj, UserInfo userInfo, RoleInfo roleInfo)
        {
            if ((infoObj != null) && (userInfo != null) && (roleInfo != null))
            {
                var info = infoObj.Generalized;

                // Do not log synchronization for group roles
                if (roleInfo.RoleGroupID != 0)
                {
                    info.StoreSettings();
                    info.LogSynchronization = SynchronizationTypeEnum.None;
                }

                DeleteInfo(infoObj);

                info.RestoreSettings();

                userInfo.Generalized.Invalidate(false);

                // Insert information about event to eventlog.
                EventLogProvider.LogEvent(EventType.INFORMATION, "Remove user from role", "USERFROMROLE", "User " + userInfo.FullName + " has been removed from role " + roleInfo.RoleDisplayName + ".", RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);

                if (SearchIndexInfoProvider.SearchTypeEnabled(UserInfo.OBJECT_TYPE) && IsSearchRequired())
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, UserInfo.OBJECT_TYPE, userInfo.TypeInfo.IDColumn, userInfo.UserID.ToString(), userInfo.UserID);
                }
            }
        }

        #endregion
    }
}