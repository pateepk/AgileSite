using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.Membership
{
    using TypedDataSet = InfoDataSet<RoleInfo>;

    /// <summary>
    /// Provides access to information about roles.
    /// </summary>
    public class RoleInfoProvider : AbstractInfoProvider<RoleInfo, RoleInfoProvider>
    {
        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        public static RoleInfo GetRoleInfoByGUID(Guid guid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(guid, siteId);
        }


        /// <summary>
        /// Returns the role info object with specified ID.
        /// </summary>
        /// <param name="roleName">Name of role to retrieve</param>
        /// <param name="siteId">Site ID which the role depends to</param>
        public static RoleInfo GetRoleInfo(string roleName, int siteId)
        {
            return ProviderObject.GetRoleInfoInternal(roleName, siteId);
        }


        /// <summary>
        /// Returns info object of role.
        /// </summary>
        /// <param name="roleName">Role code name</param>
        /// <param name="siteID">Role site ID - if zero all roles are picked</param>
        public static RoleInfo GetExistingRoleInfo(string roleName, int siteID)
        {
            return ProviderObject.GetExistingRoleInfoInternal(roleName, siteID);
        }


        /// <summary>
        /// Returns info object of role.
        /// </summary>
        /// <param name="roleName">Role code name</param>
        /// <param name="siteID">Role site ID - if zero all roles are picked</param>
        /// <param name="groupId">Community group ID</param>
        public static RoleInfo GetExistingRoleInfo(string roleName, int siteID, int groupId)
        {
            return ProviderObject.GetExistingRoleInfoInternal(roleName, siteID, groupId);
        }


        /// <summary>
        /// Returns the role info object with specified role name.
        /// </summary>
        /// <param name="roleName">Role name to find. Role name with dots is considered as global.</param>
        /// <param name="siteName">Role's site name.</param>
        /// <param name="globalIfNoSite">If true, global role is returned, if no site role is found </param>
        public static RoleInfo GetRoleInfo(string roleName, string siteName, bool globalIfNoSite)
        {
            return ProviderObject.GetRoleInfoInternal(roleName, siteName, globalIfNoSite);
        }


        /// <summary>
        /// Returns the role info object with specified role name.
        /// </summary>
        /// <param name="roleName">Name of role to retrieve. Role name with dots is considered as global. </param>
        /// <param name="siteName">Site which the role depends to. If empty global role is returned.</param>
        public static RoleInfo GetRoleInfo(string roleName, string siteName)
        {
            return ProviderObject.GetRoleInfoInternal(roleName, siteName, false);
        }


        /// <summary>
        /// Returns the RoleInfo structure for the specified role.
        /// </summary>
        /// <param name="roleId">RoleID to use for retrieving the role data</param>
        public static RoleInfo GetRoleInfo(int roleId)
        {
            return ProviderObject.GetInfoById(roleId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified role.
        /// </summary>
        /// <param name="roleInfoObj">Role to set</param>
        public static void SetRoleInfo(RoleInfo roleInfoObj)
        {
            ProviderObject.SetInfo(roleInfoObj);
        }


        /// <summary>
        /// Deletes specified role.
        /// </summary>
        /// <param name="roleObj">Role object</param>
        public static void DeleteRoleInfo(RoleInfo roleObj)
        {
            ProviderObject.DeleteInfo(roleObj);
        }


        /// <summary>
        /// Deletes specified role.
        /// </summary>
        /// <param name="roleId">Role id</param>
        public static void DeleteRoleInfo(int roleId)
        {
            RoleInfo roleObj = GetRoleInfo(roleId);
            DeleteRoleInfo(roleObj);
        }


        /// <summary>
        /// Returns the query for all roles.
        /// </summary>
        public static ObjectQuery<RoleInfo> GetRoles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns DataSet with roles of given site (if siteId > 0) or with all roles.
        /// </summary>
        /// <param name="siteId">SiteID of site which roles we want to return</param>
        public static TypedDataSet GetAllRoles(int siteId)
        {
            return GetAllRoles(siteId, false, true);
        }


        /// <summary>
        /// Returns DataSet with roles of given site (if siteId > 0) with option to display GroupRoles.
        /// </summary>
        /// <param name="siteId">SiteID of site which roles we want to return</param>
        /// <param name="includingGroups">If set to true then GroupRoles are also returned in DataSet</param>
        /// <param name="includingGeneric">If set to true then Generic roles are also returned in DataSet</param>
        public static TypedDataSet GetAllRoles(int siteId, bool includingGroups, bool includingGeneric)
        {
            // Create where condition
            string where = (siteId > 0) ? "SiteID = " + siteId : " 1 = 1 ";

            // Check if roles should be restricted only to basic roles
            if (!includingGroups)
            {
                where = "(" + where + ") AND (RoleGroupID IS NULL OR RoleGroupID = '0')";
            }

            // Generate where condition for generic roles
            if (!includingGeneric)
            {
                string genericWhere = null;
                ArrayList genericRoles = GetGenericRoles();
                if (genericRoles.Count != 0)
                {
                    foreach (string role in genericRoles)
                    {
                        genericWhere += "'" + SqlHelper.GetSafeQueryString(role, false) + "',";
                    }

                    genericWhere = genericWhere.TrimEnd(',');
                    where += " AND ( RoleName NOT IN (" + genericWhere + ") )";
                }
            }

            return GetRoles().Where(where).TypedResult;
        }


        /// <summary>
        /// Returns true if role of given name exists.
        /// </summary>
        /// <param name="roleName">Name of role</param>
        /// <param name="siteName">Site that the role belongs to</param>
        public static bool RoleExists(string roleName, string siteName)
        {
            return ProviderObject.RoleExistsInternal(roleName, siteName);
        }


        /// <summary>
        /// Returns the table of the role users or null.
        /// </summary>
        /// <param name="roleId">Role ID for retrieving the table of users</param>
        public static DataTable GetRoleUsers(int roleId)
        {
            return ProviderObject.GetRoleUsersInternal(roleId);
        }


        /// <summary>
        /// Deletes the specified Role.
        /// </summary>
        /// <param name="roleName">Role to delete</param>
        /// <param name="siteName">Site that role to delete depends to</param>
        public static void DeleteRole(string roleName, string siteName)
        {
            RoleInfo roleObj = GetRoleInfo(roleName, siteName);
            DeleteRoleInfo(roleObj);
        }


        /// <summary>
        /// Returns list of generic roles.
        /// </summary>
        public static ArrayList GetGenericRoles()
        {
            ArrayList roles = new ArrayList();

            roles.Add(RoleName.AUTHENTICATED);
            roles.Add(RoleName.NOTAUTHENTICATED);
            roles.Add(RoleName.EVERYONE);

            return roles;
        }


        /// <summary>
        /// Gets the DataSet of the required roles for the specified class permission.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        public static TypedDataSet GetRequiredClassRoles(string className, string permissionName, string siteName)
        {
            return ProviderObject.GetRequiredClassRolesInternal(className, permissionName, siteName);
        }


        /// <summary>
        /// Gets the DataSet of the required roles for the specified resource permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        public static DataSet GetRequiredResourceRoles(string resourceName, string permissionName, string siteName)
        {
            return ProviderObject.GetRequiredResourceRolesInternal(resourceName, permissionName, siteName);
        }


        /// <summary>
        /// Returns the role info object with specified ID.
        /// </summary>
        /// <param name="roleName">Name of role to retrieve</param>
        /// <param name="siteId">Site ID which the role depends to</param>
        protected virtual RoleInfo GetRoleInfoInternal(string roleName, int siteId)
        {
            roleName = roleName ?? String.Empty;

            // Rolename with dot is global, zero site ID
            if (roleName.StartsWith(".", StringComparison.Ordinal))
            {
                siteId = 0;
                roleName = roleName.TrimStart('.');
            }

            var where = new WhereCondition().WhereEquals("RoleName", roleName);
            if (siteId == 0)
            {
                where = where.WhereNull("SiteID");
            }
            else
            {
                where = where.WhereEquals("SiteID", siteId);
            }

            return GetRoles().Where(where).BinaryData(true).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Returns role object depending on site.
        /// </summary>
        /// <param name="roleName">Role code name</param>
        /// <param name="siteID">Site ID - if zero all roles are picked</param>
        protected virtual RoleInfo GetExistingRoleInfoInternal(string roleName, int siteID)
        {
            return base.GetInfoByCodeName(roleName, siteID);
        }


        /// <summary>
        /// Returns role object depending on site and group.
        /// </summary>
        /// <param name="roleName">Role code name</param>
        /// <param name="siteID">Site ID - if zero all roles are picked</param>
        /// <param name="groupId">Community group ID</param>
        protected virtual RoleInfo GetExistingRoleInfoInternal(string roleName, int siteID, int groupId)
        {
            return base.GetInfoByCodeName(roleName, siteID, groupId);
        }


        /// <summary>
        /// Returns the role info object with specified role name.
        /// </summary>
        /// <param name="roleName">Role name to find</param>
        /// <param name="siteName">Role's site name.</param>
        /// <param name="globalIfNoSite">If true, global role with the same codename is returned, when no site role is found</param>
        protected virtual RoleInfo GetRoleInfoInternal(string roleName, string siteName, bool globalIfNoSite)
        {
            roleName = roleName ?? String.Empty;
            String where = String.Empty;

            // If globalIfNoSite is true, global role with same code name is returned when no site role is found
            // If false, only site role is returned for non empty sitename and only global role is return for emtpy sitename.
            if (!globalIfNoSite)
            {
                // Role name with dot is always global
                if (roleName.StartsWithCSafe("."))
                {
                    roleName = roleName.TrimStart('.');
                    siteName = "";
                }

                where = String.IsNullOrEmpty(siteName) ? "CMS_Role.SiteID IS NULL" : "CMS_Role.SiteID IN (SELECT SiteID FROM CMS_Site WHERE SiteName = @SiteName)";
                where = SqlHelper.AddWhereCondition(where, "CMS_Role.RoleName = @RoleName");
            }
            else
            {
                where = @"(CMS_Role.SiteID IS NULL OR CMS_Role.SiteID IN (SELECT SiteID FROM CMS_Site WHERE @SiteName = SiteName))
                        AND (CMS_Role.RoleName = @RoleName OR '.' + CMS_Role.RoleName = @RoleName)";
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoleName", roleName);
            parameters.Add("@SiteName", String.IsNullOrEmpty(siteName) ? "" : siteName);

            String orderBy = String.IsNullOrEmpty(siteName) ? "SiteID ASC" : "SiteID DESC";

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.role.selectbyname", parameters, where, orderBy);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new RoleInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns true if role of given name exists.
        /// </summary>
        /// <param name="roleName">Name of role</param>
        /// <param name="siteName">Site that the role belongs to</param>
        protected virtual bool RoleExistsInternal(string roleName, string siteName)
        {
            RoleInfo role = GetRoleInfoInternal(roleName, siteName, false);
            if (role != null)
            {
                return GetObjectQuery().WhereEquals("RoleID", role.RoleID).HasResults();
            }
            return false;
        }


        /// <summary>
        /// Returns the table of the role users or null.
        /// </summary>
        /// <param name="roleId">Role ID for retrieving the table of users</param>
        protected virtual DataTable GetRoleUsersInternal(int roleId)
        {
            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("CMS.User.SelectByRole", null, "RoleID = " + roleId);
            if ((ds == null) || (ds.Tables.Count == 0))
            {
                return null;
            }
            else
            {
                return ds.Tables[0];
            }
        }


        /// <summary>
        /// Gets the DataSet of the required roles for the specified class permission.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        protected virtual TypedDataSet GetRequiredClassRolesInternal(string className, string permissionName, string siteName)
        {
            // Get Class info
            DataClassInfo ci = DataClassInfoProvider.GetDataClassInfo(className);
            if (ci == null)
            {
                throw new Exception("[UserInfoProvider.GetRequiredClassRolesInternal]: Class '" + className + "' not found.");
            }

            // Get site info
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[UserInfoProvider.GetRequiredClassRolesInternal]: Site name '" + siteName + "' not found.");
            }

            // Get resource permissions result
            string storageKey = "membership|requiredroles|class";
            string rolesKey = "|" + className + "|" + permissionName + "|" + si.SiteID;

            TypedDataSet ds = (TypedDataSet)RequestStockHelper.GetItem(storageKey, rolesKey, false);
            if (ds == null)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ClassID", ci.ClassID);
                parameters.Add("@PermissionName", permissionName);
                parameters.Add("@SiteID", si.SiteID);
                parameters.EnsureDataSet<RoleInfo>();

                // Get the list of roles required for the given permission
                ds = ConnectionHelper.ExecuteQuery("CMS.Class.SelectRequiredRoles", parameters).As<RoleInfo>();
                RequestStockHelper.AddToStorage(storageKey, rolesKey, ds, false);
            }

            return ds;
        }


        /// <summary>
        /// Gets the DataSet of the required roles for the specified resource permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        protected virtual DataSet GetRequiredResourceRolesInternal(string resourceName, string permissionName, string siteName)
        {
            DataSet ds = new DataSet();
            int siteId = 0;

            // Get site info if site name set, otherwise global resources
            if (!String.IsNullOrEmpty(siteName))
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);

                // Check if specified site exists
                if (si == null)
                {
                    throw new Exception("[UserInfoProvider.GetRequiredResourceRolesInternal]: Site name '" + siteName + "' not found.");
                }
                siteId = si.SiteID;
            }

            // Try to get data from cache
            using (var cs = new CachedSection<DataSet>(ref ds, 1440, true, null, "SiteResourceRolesPermissions", siteId, resourceName))
            {
                if (cs.LoadData)
                {
                    ds = GetRoles()
                            .Source(s => s
                                .LeftJoin("View_CMS_RoleResourcePermission_Joined", "RoleID", "RoleID", new WhereCondition("ResourceName", QueryOperator.Equals, resourceName)))
                            .WhereIn("CMS_Role.RoleID", GetRoles()
                                .From("View_CMS_RoleResourcePermission_Joined")
                                .Column("RoleID")
                                .WhereEquals("ResourceName", resourceName))
                            .WhereEqualsOrNull("SiteID", siteId)
                            .Columns("SiteID, RoleName, ResourceName, PermissionName,CMS_Role.RoleID,PermissionID")
                            .Result;

                    // Save the result to the cache
                    if (cs.Cached)
                    {
                        // Get cache dependencies
                        List<string> keys = new List<string>
                        {
                            PermissionNameInfo.OBJECT_TYPE_RESOURCE + "|all",
                            RoleInfo.OBJECT_TYPE + "|all"
                        };

                        cs.CacheDependency = CacheHelper.GetCacheDependency(keys);
                    }

                    cs.Data = ds;
                }
            }

            // Filter result only for required permission
            var filteredTable = ds.Tables[0].AsEnumerable()
                                     .Where(row => String.Equals(row.Field<string>("PermissionName"), permissionName, StringComparison.InvariantCultureIgnoreCase))
                                     .AsDataView()
                                     .ToTable();

            ds = new DataSet();
            ds.Tables.Add(filteredTable);

            return ds;
        }


        /// <summary>
        /// Validates the role name. Returns <c>true</c> if the role name follows rules defined by <see cref="ValidationHelper.IsCodeName"/>
        /// </summary>
        /// <remarks>
        /// Since domain role name can be retrieved from different services with different rules for role name,
        /// validation for domain role names can be turned off using <c>CMSEnsureSafeRoleNames</c> key in web.config.
        /// </remarks>
        /// <param name="info">Role info object to check</param>
        public override bool ValidateCodeName(RoleInfo info)
        {
            if (info.RoleIsDomain && !ValidationHelper.UseSafeRoleName)
            {
                return !String.IsNullOrEmpty(info.RoleName);
            }

            return base.ValidateCodeName(info);
        }
    }
}