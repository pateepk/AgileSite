using System;
using System.Data;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;

namespace CMS.Community
{
    /// <summary>
    /// Class providing GroupRolePermissionInfo management.
    /// </summary>
    public class GroupRolePermissionInfoProvider : AbstractInfoProvider<GroupRolePermissionInfo, GroupRolePermissionInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns GroupRolePermissionInfo for specified group/role/permission.
        /// </summary>
        /// <param name="groupId">GroupID</param>
        /// <param name="roleId">RoleID</param>
        /// <param name="permissionId">PermissionID</param>
        public static GroupRolePermissionInfo GetGroupRolePermissionInfo(int groupId, int roleId, int permissionId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoleID", roleId);
            parameters.Add("@GroupID", groupId);
            parameters.Add("@PermissionID", permissionId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Community.GroupRolePermission.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new GroupRolePermissionInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Gets GroupRolePermissionInfo object specified by where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">OrderBy expression</param>
        public static DataSet GetGroupRolePermissionInfos(string where, string orderBy)
        {
            // Ensure connection

            return ConnectionHelper.ExecuteQuery("community.grouprolepermission.selectall", null, where, orderBy);
        }


        /// <summary>
        /// Sets (updates or inserts) specified GroupRolePermissionInfo.
        /// </summary>
        /// <param name="groupRolePermission">GroupRolePermissionInfo object to set</param>
        public static void SetGroupRolePermissionInfo(GroupRolePermissionInfo groupRolePermission)
        {
            if (groupRolePermission != null)
            {
                // Check IDs
                if ((groupRolePermission.GroupID <= 0) || (groupRolePermission.RoleID <= 0) || (groupRolePermission.PermissionID <= 0))
                {
                    throw new Exception("[GroupRolePermissionInfoProvider.SetGroupRolePermissionInfo]: Object IDs not set.");
                }

                // Get existing
                GroupRolePermissionInfo existing = GetGroupRolePermissionInfo(groupRolePermission.GroupID, groupRolePermission.RoleID, groupRolePermission.PermissionID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                }
                else
                {
                    groupRolePermission.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[GroupRolePermissionInfoProvider.SetGroupRolePermissionInfo]: No GroupRolePermissionInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified GroupRolePermissionInfo.
        /// </summary>
        /// <param name="infoObj">GroupRolePermissionInfo object</param>
        public static void DeleteGroupRolePermissionInfo(GroupRolePermissionInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Removes role from specified group.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="groupId">GroupID</param>
        /// <param name="permissionId">PermissionID</param>
        public static void RemoveRoleFromGroup(int roleId, int groupId, int permissionId)
        {
            GroupRolePermissionInfo infoObj = GetGroupRolePermissionInfo(groupId, roleId, permissionId);
            if (infoObj != null)
            {
                DeleteGroupRolePermissionInfo(infoObj);

                RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
                GroupInfo group = GroupInfoProvider.GetGroupInfo(groupId);

                if ((role != null) && (group != null))
                {
                    // Insert information about event to eventlog.
                    EventLogProvider.LogEvent(EventType.INFORMATION, "Remove role from group", "ROLEFROMGROUP", "Role " + role.RoleName + " has been removed from group " + @group.GroupName + ".", RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);
                }
            }
        }


        /// <summary>
        /// Adds role to specified group.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="groupId">GroupID</param>
        /// <param name="permissionId">PermissionID</param>
        public static void AddRoleToGroup(int roleId, int groupId, int permissionId)
        {
            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
            GroupInfo group = GroupInfoProvider.GetGroupInfo(groupId);
            PermissionNameInfo pni = PermissionNameInfoProvider.GetPermissionNameInfo(permissionId);

            if ((role != null) && (group != null) && (pni != null))
            {
                // Create new binding
                GroupRolePermissionInfo infoObj = new GroupRolePermissionInfo();
                infoObj.RoleID = roleId;
                infoObj.GroupID = groupId;
                infoObj.PermissionID = permissionId;

                // Save to the database
                SetGroupRolePermissionInfo(infoObj);

                // Insert information about event to eventlog.
                EventLogProvider.LogEvent(EventType.INFORMATION, "Add role to group", "ROLETOGROUP", "Role " + role.RoleName + " has been added to group " + @group.GroupName + ".", RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress, 0);
            }
        }


        /// <summary>
        /// Returns the permission matrix for the specified group.
        /// </summary>
        /// <param name="resourceId">ID of the resource matrix</param>
        /// <param name="siteId">SiteID</param>
        /// <param name="groupId">GroupID</param>
        public static DataSet GetPermissionMatrix(int resourceId, int siteId, int groupId)
        {
            // Retrive permission matrix data
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", resourceId);
            parameters.Add("@GroupID", groupId);
            parameters.Add("@SiteID", siteId);

            // Setup WHERE condition
            string where = "RoleGroupID=" + groupId.ToString();

            // Get the data
            return ConnectionHelper.ExecuteQuery("Community.GroupRolePermission.getpermissionMatrix", parameters, where);
        }

        #endregion
    }
}