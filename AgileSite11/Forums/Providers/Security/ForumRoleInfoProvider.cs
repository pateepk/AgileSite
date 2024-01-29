using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing ForumRoleInfo management.
    /// </summary>
    public class ForumRoleInfoProvider : AbstractInfoProvider<ForumRoleInfo, ForumRoleInfoProvider>
    {
        #region "Public Methods"

        /// <summary>
        /// Returns the ForumRoleInfo structure for the specified forumRole.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="forumId">ForumID</param>
        /// <param name="permissionId">Permission ID</param>
        public static ForumRoleInfo GetForumRoleInfo(int roleId, int forumId, int permissionId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoleID", roleId);
            parameters.Add("@ForumID", forumId);
            parameters.Add("@PermissionID", permissionId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.ForumRole.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ForumRoleInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumRole.
        /// </summary>
        /// <param name="forumRole">ForumRole to set</param>
        public static void SetForumRoleInfo(ForumRoleInfo forumRole)
        {
            if (forumRole != null)
            {
                // Check IDs
                if ((forumRole.RoleID <= 0) || (forumRole.ForumID <= 0) || (forumRole.PermissionID <= 0))
                {
                    throw new Exception("[ForumRoleInfoProvider.SetForumRoleInfo]: Object IDs not set.");
                }

                // Get existing
                ForumRoleInfo existing = GetForumRoleInfo(forumRole.RoleID, forumRole.ForumID, forumRole.PermissionID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                    //forumRole.Generalized.UpdateData();
                }
                else
                {
                    forumRole.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[ForumRoleInfoProvider.SetForumRoleInfo]: No ForumRoleInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified forumRole.
        /// </summary>
        /// <param name="infoObj">ForumRole object</param>
        public static void DeleteForumRoleInfo(ForumRoleInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified forumRole.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="forumId">ForumID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void RemoveRoleFromForum(int roleId, int forumId, int permissionId)
        {
            ForumRoleInfo infoObj = GetForumRoleInfo(roleId, forumId, permissionId);
            if (infoObj != null)
            {
                DeleteForumRoleInfo(infoObj);

                RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
                ForumInfo forum = ForumInfoProvider.GetForumInfo(forumId);

                if ((role != null) && (forum != null))
                {
                    // Insert information about event to eventlog.
                    EventLogProvider.LogEvent(EventType.INFORMATION, "Remove role from forum", "ROLEFROMFORUM", "Role " + role.RoleName + " has been removed from forum " + HTMLHelper.HTMLEncode(forum.ForumName) + ".", RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);
                }
            }
        }


        /// <summary>
        /// Adds specified role to the forum.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="forumId">ForumID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void AddRoleToForum(int roleId, int forumId, int permissionId)
        {
            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
            ForumInfo forum = ForumInfoProvider.GetForumInfo(forumId);
            PermissionNameInfo pni = PermissionNameInfoProvider.GetPermissionNameInfo(permissionId);

            if ((role != null) && (forum != null) && (pni != null))
            {
                // Create new binding
                ForumRoleInfo infoObj = new ForumRoleInfo();
                infoObj.RoleID = roleId;
                infoObj.ForumID = forumId;
                infoObj.PermissionID = permissionId;

                // Save to the database
                SetForumRoleInfo(infoObj);

                // Insert information about event to eventlog.
                EventLogProvider.LogEvent(EventType.INFORMATION, "Add role to forum", "ROLETOFORUM", "Role " + role.RoleName + " has been added to forum " + HTMLHelper.HTMLEncode(forum.ForumName) + ".", RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);
            }
        }


        /// <summary>
        /// Returns the permission matrix for the specified forum.
        /// </summary>
        /// <param name="resourceId">ID of the resource matrix is returned for</param>        
        /// <param name="siteId">Site ID</param>
        /// <param name="roleGroupId">ID of the group roles should belongs to</param>
        /// <param name="forumId">ID of the forum permission matrix is retrieved for</param>
        public static DataSet GetPermissionMatrix(int resourceId, int siteId, int roleGroupId, int forumId)
        {
            // Retrive permission matrix data
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", resourceId);
            parameters.Add("@ForumID", forumId);
            parameters.Add("@SiteID", siteId);

            string where = "";

            if (roleGroupId > 0)
            {
                where = "RoleGroupID=" + roleGroupId.ToString();
            }
            else
            {
                where = "RoleGroupID IS NULL";
            }

            // Get the data
            return ConnectionHelper.ExecuteQuery("Forums.ForumRole.getpermissionMatrix", parameters, where);
        }


        /// <summary>
        /// Delete all forum roles.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteAllRoles(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }


        /// <summary>
        /// Sets permissions for list of roles.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="roleIds">List of role IDs</param>
        /// <param name="permissionsIds">List of permission IDs</param>
        public static void SetPermissions(int forumId, IEnumerable<int> roleIds, IEnumerable<int> permissionsIds)
        {
            ProviderObject.SetPermissionsInternal(forumId, roleIds, permissionsIds);
        }




        /// <summary>
        /// Returns the relationships specified by the parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        public static DataSet GetRelationships(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("Forums.ForumRole.selectall", null, where, orderBy, topN, columns);
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Sets permissions for list of roles.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="roleIds">List of role IDs</param>
        /// <param name="permissionsIds">List of permission IDs</param>
        private void SetPermissionsInternal(int forumId, IEnumerable<int> roleIds, IEnumerable<int> permissionsIds)
        {
            if (forumId <= 0 || roleIds == null || !roleIds.Any() || permissionsIds == null || !permissionsIds.Any())
            {
                return;
            }

            var forumRoles = new List<ForumRoleInfo>();

            foreach (var roleId in roleIds)
            {
                var rolePermissions = CreateForumRole(forumId, roleId, permissionsIds);
                forumRoles.AddRange(rolePermissions);
            }

            BulkInsertInfos(forumRoles);
        }


        /// <summary>
        /// Creates collection of forum role objects with specific permissions.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="permissionsIds">List of permission IDs</param>
        private static IEnumerable<ForumRoleInfo> CreateForumRole(int forumId, int roleId, IEnumerable<int> permissionsIds)
        {
            return permissionsIds.Select(permissionsId => new ForumRoleInfo
            {
                RoleID = roleId,
                ForumID = forumId,
                PermissionID = permissionsId
            });
        }

        #endregion
    }
}