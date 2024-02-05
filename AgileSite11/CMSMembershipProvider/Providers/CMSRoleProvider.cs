using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Web.Security;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.MembershipProvider
{
    /// <summary>
    /// Class providing role management.
    /// </summary>
    public class CMSRoleProvider : RoleProvider
    {
        #region "Variables"

        private string mApplicationName = "";
        private string mDescription = "";
        private string mName = "";

        #endregion


        #region "Properties"

        /// <summary>
        /// Application name.
        /// </summary>
        public override string ApplicationName
        {
            get
            {
                return mApplicationName;
            }
            set
            {
                mApplicationName = value;
            }
        }


        /// <summary>
        /// Description.
        /// </summary>
        public override string Description
        {
            get
            {
                return mDescription;
            }
        }


        /// <summary>
        /// Name.
        /// </summary>
        public override string Name
        {
            get
            {
                return mName;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="config">Config</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            mName = name;
            mApplicationName = "/";

            base.Initialize(name, config);
        }


        /// <summary>
        /// Adds user to role.
        /// </summary>
        /// <param name="usernames">User names</param>
        /// <param name="roleNames">Role names</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            // Sets site name of current site
            string siteName = SiteContext.CurrentSiteName;
            // Loop thru all users
            foreach (string user in usernames)
            {
                // Loop thru all roles
                foreach (string role in roleNames)
                {
                    UserInfoProvider.AddUserToRole(user, role, siteName);
                }
            }
        }


        /// <summary>
        /// Creates current site role.
        /// </summary>
        /// <param name="roleName">Role name to create</param>
        public override void CreateRole(string roleName)
        {
            RoleInfo newRole = new RoleInfo();
            newRole.RoleName = roleName;
            newRole.RoleDisplayName = roleName;
            newRole.RoleDescription = String.Empty;
            newRole.SiteID = SiteContext.CurrentSiteID;

            RoleInfoProvider.SetRoleInfo(newRole);
        }


        /// <summary>
        /// Deletes specified role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="throwOnPopulatedRole">Throw exception on role which is populated</param>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            // Sets site name of current site
            string siteName = SiteContext.CurrentSiteName;

            // Check whether empty role should be tested
            if (throwOnPopulatedRole)
            {
                // Get role object
                RoleInfo ri = RoleInfoProvider.GetRoleInfo(roleName, siteName);
                // Check whether role exists
                if (ri != null)
                {
                    // Get users in specified role
                    DataTable dt = RoleInfoProvider.GetRoleUsers(ri.RoleID);
                    // If exists at least one user throw an exception
                    if (!DataHelper.DataSourceIsEmpty(dt))
                    {
                        throw new ProviderException("[CMSRoleProvider.DeleteRole] Role '" + roleName + " is not empty.");
                    }
                }
            }

            try
            {
                RoleInfoProvider.DeleteRole(roleName, siteName);
                return true;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CMSRoleProvider", "DeleteRole", ex);
                return false;
            }
        }


        /// <summary>
        /// Find all user in specified role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="usernameToMatch">User name to match</param>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            string siteName = SiteContext.CurrentSiteName;

            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleName, siteName);
            if (role != null)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@RoleID", role.RoleID);
                parameters.Add("@UserNameToMatch", "%" + usernameToMatch + "%");

                // Get the data
                DataSet ds = ConnectionHelper.ExecuteQuery("cms.user.findusersinrole", parameters);

                // Check whether exists at least one role
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    List<string> users = new List<string>(ds.Tables[0].Rows.Count);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        users.Add(Convert.ToString(dr["UserName"]));
                    }

                    return users.ToArray();
                }
            }

            return new string[0];
        }


        /// <summary>
        /// Returns all roles.
        /// </summary>
        public override string[] GetAllRoles()
        {
            // Get the data
            DataSet ds = RoleInfoProvider.GetAllRoles(SiteContext.CurrentSiteID);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                string[] rolesArray = new string[ds.Tables[0].Rows.Count];
                int index = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    rolesArray[index++] = dr["RoleName"].ToString();
                }
                return rolesArray;
            }
            else
            {
                return new string[0];
            }
        }


        /// <summary>
        /// Returns CMS roles, or Windows roles for specified user.
        /// </summary>
        /// <remarks>If user doesn't exists in the CMS, returns Windows roles.</remarks>
        /// <param name="username">User name</param>
        public override string[] GetRolesForUser(string username)
        {
            // Get safe username if is required
            string safeusername = ValidationHelper.UseSafeUserName ? ValidationHelper.GetSafeUserName(username, SiteContext.CurrentSiteName) : username;
            UserInfo ui = UserInfoProvider.GetUserInfo(safeusername);
            
            // If user doesn't exists, return Windows roles
            if (ui == null && RequestHelper.IsWindowsAuthentication())
            {
                // Get window roles for current user
                string[] winRoles = AuthenticationHelper.GetUserWindowsRoles(username);
                // Check whether roles exist and if should be in safe format
                if ((winRoles != null) && ValidationHelper.UseSafeRoleName)
                {
                    // Loop thru all roles and ensure safe role format
                    for (int i = 0; i < winRoles.Length; i++)
                    {
                        winRoles[i] = ValidationHelper.GetSafeRoleName(winRoles[i], SiteContext.CurrentSiteName);
                    }
                    return winRoles;
                }
            }
            else
            {
                return UserInfoProvider.GetRolesForUser(safeusername, SiteContext.CurrentSiteName);
            }
            return new string[0];
        }


        /// <summary>
        /// Returns all user in specified role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        public override string[] GetUsersInRole(string roleName)
        {
            string siteName = SiteContext.CurrentSiteName;

            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleName, siteName);
            if (role != null)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@RoleID", role.RoleID);

                // Get the data
                DataSet ds = ConnectionHelper.ExecuteQuery("cms.user.selectusersinrole", parameters);

                // Check whether exists at leat one role
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    List<string> users = new List<string>(ds.Tables[0].Rows.Count);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        users.Add(Convert.ToString(dr["UserName"]));
                    }
                    return users.ToArray();
                }
            }
            return new string[0];
        }


        /// <summary>
        /// Returns true if the user is a member of the role in context of the current site.
        /// </summary>
        /// <remarks>The check also accounts for membership roles and global roles.</remarks>
        /// <param name="username">User name</param>
        /// <param name="roleName">Role name</param>
        public override bool IsUserInRole(string username, string roleName)
        {
            return UserInfoProvider.IsUserInRole(username, roleName, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Removes user from role.
        /// </summary>
        /// <param name="usernames">User name</param>
        /// <param name="roleNames">Role name</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            // Keep current site name
            string siteName = SiteContext.CurrentSiteName;

            // Loop thru all users
            foreach (string user in usernames)
            {
                // Loop thru all roles
                foreach (string role in roleNames)
                {
                    // Remove the user from specific role
                    UserInfoProvider.RemoveUserFromRole(user, role, siteName);
                }
            }
        }


        /// <summary>
        /// Determines whether specified role exists.
        /// </summary>
        /// <param name="roleName">Role name</param>
        public override bool RoleExists(string roleName)
        {
            string siteName = SiteContext.CurrentSiteName;
            return RoleInfoProvider.RoleExists(roleName, siteName);
        }

        #endregion
    }
}