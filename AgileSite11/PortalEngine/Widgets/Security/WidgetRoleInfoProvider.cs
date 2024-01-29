using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;

namespace CMS.PortalEngine
{
    using TypedDataSet = InfoDataSet<WidgetRoleInfo>;

    /// <summary>
    /// Class providing WidgetRoleInfo management.
    /// </summary>
    public class WidgetRoleInfoProvider : AbstractInfoProvider<WidgetRoleInfo, WidgetRoleInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns all widget -- role bindings.
        /// </summary>
        public static ObjectQuery<WidgetRoleInfo> GetWidgetRoleInfos()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the WidgetRoleInfo structure for the specified widget and role.
        /// </summary>
        /// <param name="widgetId">Widget id</param>     
        /// <param name="roleId">Role id</param>
        /// <param name="permissionId">Permission id</param>
        public static WidgetRoleInfo GetWidgetRoleInfo(int widgetId, int roleId, int permissionId)
        {
            return ProviderObject.GetWidgetRoleInfoInternal(widgetId, roleId, permissionId);
        }


        /// <summary>
        /// Returns all widget-role bindings according to parameters.
        /// </summary>
        /// <param name="columns">Column names</param>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Number of items that should be returned (all if 0)</param>
        [Obsolete("Use method GetWidgetRoleInfos() instead")]
        public static TypedDataSet GetWidgetRoles(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetWidgetRolesInternal(columns, where, orderBy, topN);
        }


        /// <summary>
        /// Sets (updates or inserts) specified widget role info.
        /// </summary>
        /// <param name="wri">WidgetRole to set</param>
        public static void SetWidgetRoleInfo(WidgetRoleInfo wri)
        {
            ProviderObject.SetInfo(wri);
        }


        /// <summary>
        /// Deletes specified widget-role.
        /// </summary>
        /// <param name="wri">WidgetRole object</param>
        public static void DeleteWidgetRoleInfo(WidgetRoleInfo wri)
        {
            ProviderObject.DeleteInfo(wri);
        }


        /// <summary>
        /// Adds specified role and permission to the widget.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="widgetId">Widget ID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void AddRoleToWidget(int roleId, int widgetId, int permissionId)
        {
            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
            WidgetInfo widget = WidgetInfoProvider.GetWidgetInfo(widgetId);
            PermissionNameInfo pni = PermissionNameInfoProvider.GetPermissionNameInfo(permissionId);

            if ((role != null) && (widget != null) && (pni != null))
            {
                // Create new binding
                WidgetRoleInfo infoObj = new WidgetRoleInfo();
                infoObj.RoleID = roleId;
                infoObj.WidgetID = widgetId;
                infoObj.PermissionID = permissionId;

                // Save to the database
                SetWidgetRoleInfo(infoObj);

                // Insert information about event to eventlog.
                EventLogProvider.LogEvent(EventType.INFORMATION, "Add role to widget", "ROLETOWIDGET", "Role " + role.RoleName + " has been added to widget " + HTMLHelper.HTMLEncode(widget.WidgetName) + ".", RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);
            }
        }


        /// <summary>
        /// Deletes specified widgetRole.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="widgetId">WidgetID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void RemoveRoleFromWidget(int roleId, int widgetId, int permissionId)
        {
            WidgetRoleInfo infoObj = GetWidgetRoleInfo(widgetId, roleId, permissionId);
            if (infoObj != null)
            {
                DeleteWidgetRoleInfo(infoObj);

                RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
                WidgetInfo widget = WidgetInfoProvider.GetWidgetInfo(widgetId);

                if ((role != null) && (widget != null))
                {
                    // Insert information about event to eventlog.
                    EventLogProvider.LogEvent(EventType.INFORMATION, "Remove role from widget", "ROLEFROMWIDGET", "Role " + role.RoleName + " has been removed from widget " + HTMLHelper.HTMLEncode(widget.WidgetName) + ".", RequestContext.RawURL, 0, UserInfoProvider.GetUserName(), 0, null, RequestContext.UserHostAddress);
                }
            }
        }


        /// <summary>
        /// Returns true if widget is allowed for specified user.
        /// </summary>
        /// <param name="widgetId">ID of the widget</param>
        /// <param name="userId">ID of the accessing user</param>
        /// <param name="isAuthenticated">Value indicating if accessing user is authenticated</param>        
        public static bool IsWidgetAllowed(int widgetId, int userId, bool isAuthenticated)
        {
            WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(widgetId);
            return IsWidgetAllowed(wi, userId, isAuthenticated);
        }


        /// <summary>
        /// Return true if widget is allowed for specified user. 
        /// Only security settings is considered. Widget still can be allowed for group admins.
        /// </summary>
        /// <param name="wi">Widget info</param>
        /// <param name="userId">ID of the accessing user</param>
        /// <param name="isAuthenticated">Value indicating if accessing user is authenticated</param>        
        public static bool IsWidgetAllowed(WidgetInfo wi, int userId, bool isAuthenticated)
        {
            UserInfo ui = UserInfoProvider.GetUserInfo(userId);
            if (wi == null || ui == null)
            {
                return false;
            }

            // Allow all for global admin
            if (ui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }

            SecurityAccessEnum access = wi.AllowedFor;

            return ((access == SecurityAccessEnum.AllUsers) || ((access == SecurityAccessEnum.AuthenticatedUsers) && isAuthenticated) || ((access == SecurityAccessEnum.GlobalAdmin) && ui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                    || ((access == SecurityAccessEnum.AuthorizedRoles) && isAuthenticated && 
                    !DataHelper.DataSourceIsEmpty(GetWidgetRoleInfos()
                        .WhereEquals("WidgetID", wi.WidgetID)
                        .WhereIn("RoleID", new DataQuery().From("View_CMS_UserRole_MembershipRole_ValidOnly_Joined")
                                                          .WhereEquals("UserID", userId).Column("RoleID"))
                        .TopN(1).Column("RoleID"))));

        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WidgetRoleInfo structure for the specified widget and role.
        /// </summary>
        /// <param name="widgetId">Widget id</param>
        /// <param name="roleId">Role id</param>
        /// <param name="permissionId">Permission id</param>
        protected virtual WidgetRoleInfo GetWidgetRoleInfoInternal(int widgetId, int roleId, int permissionId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("WidgetID", widgetId)
                .WhereEquals("RoleID", roleId)
                .WhereEquals("PermissionID", permissionId).FirstOrDefault();
        }


        /// <summary>
        /// Returns all widget-role bindings according to parameters.
        /// </summary>
        /// <param name="columns">Column names</param>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Number of items that should be returned (all if 0)</param>
        [Obsolete("Use method GetWidgetRoleInfos() instead")]
        protected virtual TypedDataSet GetWidgetRolesInternal(string columns, string where, string orderBy, int topN)
        {
            return GetWidgetRoleInfos().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WidgetRoleInfo info)
        {
            if (info != null)
            {
                // Check IDs
                if ((info.WidgetID <= 0) || (info.RoleID <= 0) || (info.PermissionID <= 0))
                {
                    throw new ArgumentException("Object IDs not set.");
                }

                base.SetInfo(info);
            }
            else
            {
                throw new ArgumentNullException(nameof(info));
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WidgetRoleInfo info)
        {
            if (info != null)
            {
                // Delete object from database
                base.DeleteInfo(info);
            }
        }

        #endregion
    }
}