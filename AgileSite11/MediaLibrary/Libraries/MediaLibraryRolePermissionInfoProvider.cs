using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Class providing media library role permission management.
    /// </summary>
    public class MediaLibraryRolePermissionInfoProvider : AbstractInfoProvider<MediaLibraryRolePermissionInfo, MediaLibraryRolePermissionInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the media library role permission structure matching specified criteria.
        /// </summary>
        /// <param name="libraryId">ID of the library</param>
        /// <param name="roleId">ID of the role</param>
        /// <param name="permissionId">ID of the permission</param>
        public static MediaLibraryRolePermissionInfo GetMediaLibraryRolePermissionInfo(int libraryId, int roleId, int permissionId)
        {
            return ProviderObject.GetMediaLibraryRolePermissionInfoInternal(libraryId, roleId, permissionId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified library role permission.
        /// </summary>
        /// <param name="libraryRolePermission">Library role permission to set</param>
        public static void SetMediaLibraryRolePermissionInfo(MediaLibraryRolePermissionInfo libraryRolePermission)
        {
            ProviderObject.SetInfo(libraryRolePermission);
        }


        /// <summary>
        /// Deletes specified media library role permission.
        /// </summary>
        /// <param name="infoObj">Media library role permission object</param>
        public static void DeleteMediaLibraryRolePermissionInfo(MediaLibraryRolePermissionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns the query for all media library role permissions.
        /// </summary>        
        public static ObjectQuery<MediaLibraryRolePermissionInfo> GetMediaLibraryRolePermissions()
        {
            return ProviderObject.GetObjectQuery();
        }

       
        /// <summary>
        /// Returns the media library role permission data matching specified criteria.
        /// </summary>
        /// <param name="where">Where condition used to filter the data</param>
        /// <param name="orderBy">Order by statement to use</param>
        public static ObjectQuery<MediaLibraryRolePermissionInfo> GetLibraryRolePermissions(string where, string orderBy = null)
        {
            return GetMediaLibraryRolePermissions().Where(where).OrderBy(orderBy);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Adds specified role to the library.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void AddRoleToLibrary(int roleId, int libraryId, int permissionId)
        {
            ProviderObject.AddRoleToLibraryInternal(roleId, libraryId, permissionId);
        }


        /// <summary>
        /// Deletes specified library role.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void RemoveRoleFromLibrary(int roleId, int libraryId, int permissionId)
        {
            ProviderObject.RemoveRoleFromLibraryInternal(roleId, libraryId, permissionId);
        }


        /// <summary>
        /// Delete all media library roles.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteAllRoles(string where = null)
        {
            ProviderObject.DeleteAllRolesInternal(where);
        }


        /// <summary>
        /// Sets permissions in <paramref name="permissionsIds"/> for each role in <paramref name="roleIds"/>.
        /// </summary>
        /// <param name="libraryId">Media library ID.</param>
        /// <param name="roleIds">List of role IDs.</param>
        /// <param name="permissionsIds">List of permission IDs.</param>
        public static void SetPermissions(int libraryId, IList<int> roleIds, IList<int> permissionsIds)
        {
            ProviderObject.SetPermissionsInternal(libraryId, roleIds, permissionsIds);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns the media library role permission structure matching specified criteria.
        /// </summary>
        /// <param name="libraryId">ID of the library</param>
        /// <param name="roleId">ID of the role</param>
        /// <param name="permissionId">ID of the permission</param>
        protected virtual MediaLibraryRolePermissionInfo GetMediaLibraryRolePermissionInfoInternal(int libraryId, int roleId, int permissionId)
        {
            var condition = new WhereCondition()
                .WhereEquals("LibraryID", libraryId)
                .WhereEquals("RoleID", roleId)
                .WhereEquals("PermissionID", permissionId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Adds specified role to the library.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="permissionId">Permission ID</param>
        protected virtual void AddRoleToLibraryInternal(int roleId, int libraryId, int permissionId)
        {
            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
            MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);
            PermissionNameInfo pni = PermissionNameInfoProvider.GetPermissionNameInfo(permissionId);

            if ((role == null) || (library == null) || (pni == null))
            {
                return;
            }

            var infoObj = CreateMediaLibraryRolePermissionInfo(roleId, libraryId, permissionId);

            // Save to the database
            SetMediaLibraryRolePermissionInfo(infoObj);

            LogAddRoleToLibraryToEventLog(role, library);
        }


        private static void LogAddRoleToLibraryToEventLog(RoleInfo role, MediaLibraryInfo library)
        {
            EventLogProvider.LogEvent(EventType.INFORMATION,
                "Add role to library",
                "ROLETOLIBRARY",
                String.Format("Role {0} has been added to library {1}.", role.RoleName, library.LibraryDisplayName), RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);
        }


        /// <summary>
        /// Deletes specified library role.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="permissionId">Permission ID</param>
        protected virtual void RemoveRoleFromLibraryInternal(int roleId, int libraryId, int permissionId)
        {
            MediaLibraryRolePermissionInfo infoObj = GetMediaLibraryRolePermissionInfo(libraryId, roleId, permissionId);
            if (infoObj != null)
            {
                DeleteMediaLibraryRolePermissionInfo(infoObj);

                RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);

                if ((role != null) && (library != null))
                {
                    // Insert information about event to event log.
                    EventLogProvider.LogEvent(EventType.INFORMATION,
                                "Remove role from library",
                                "ROLEFROMLIBRARY",
                                String.Format("Role {0} has been removed from forum {1}.", role.RoleName, library.LibraryDisplayName), RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);
                }
            }
        }


        /// <summary>
        /// Delete all media library roles.
        /// </summary>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteAllRolesInternal(string where)
        {
            BulkDelete(new WhereCondition(where));
        }


        /// <summary>
        /// Sets permissions in <paramref name="permissionsIds"/> for each role in <paramref name="roleIds"/>.
        /// </summary>
        /// <param name="libraryId">Media library ID.</param>
        /// <param name="roleIds">List of role IDs.</param>
        /// <param name="permissionsIds">List of permission IDs.</param>
        protected virtual void SetPermissionsInternal(int libraryId, IList<int> roleIds, IList<int> permissionsIds)
        {
            if (libraryId <= 0 || roleIds == null || !roleIds.Any() || permissionsIds == null || !permissionsIds.Any())
            {
                return;
            }

            var rolePermissionForAllRoles = new List<MediaLibraryRolePermissionInfo>();

            foreach (var roleId in roleIds)
            {
                var rolePermissions = CreateRolePermissionsForRole(libraryId, roleId, permissionsIds);
                rolePermissionForAllRoles.AddRange(rolePermissions);
            }

            BulkInsertInfos(rolePermissionForAllRoles);
        }


        private static IEnumerable<MediaLibraryRolePermissionInfo> CreateRolePermissionsForRole(int libraryId, int roleId, IEnumerable<int> permissionsIds)
        {
            return permissionsIds.Select(permissionId => 
                CreateMediaLibraryRolePermissionInfo(roleId, libraryId, permissionId));
        }


        private static MediaLibraryRolePermissionInfo CreateMediaLibraryRolePermissionInfo(int roleId, int libraryId, int permissionId)
        {
            return new MediaLibraryRolePermissionInfo
            {
                RoleID = roleId,
                LibraryID = libraryId,
                PermissionID = permissionId
            };
        }

        #endregion
    }
}