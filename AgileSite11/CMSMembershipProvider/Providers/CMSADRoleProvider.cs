using System;
using System.Linq;
using System.Globalization;
using System.Web.Security;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Collections.Specialized;
using System.DirectoryServices.AccountManagement;

using CMS.Base;
using CMS.Membership;

namespace CMS.MembershipProvider
{
    /// <summary>
    /// Active directory role provider.
    /// </summary>
    public class CMSADRoleProvider : RoleProvider
    {
        #region "Constants"

        /// <summary>
        /// Role name format Domain - SAM
        /// </summary>
        private const string DOMAIN_SAM_FORMAT = "DomainSAM";


        /// <summary>
        /// Role name format SAM
        /// </summary>
        private const string SAM_FORMAT = "SAM";

        #endregion

        
        #region "Variables"

        private string mAttributeMapUsername = "";
        private string mApplicationName = "";
        private string mName = "";
        private string mDescription = "";

        #endregion


        #region "Properties"


        /// <summary>
        /// Attribute to map username.
        /// </summary>
        private string AttributeMapUsername
        {
            get
            {
                return mAttributeMapUsername;
            }
            set
            {
                mAttributeMapUsername = value;
            }
        }


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


        #region "Public properties"

        /// <summary>
        /// Format of imported roles. This applies for both display name and code name.
        /// </summary>
        public string RoleImportFormat
        {
            get
            {
                return SettingsHelper.AppSettings["CMSRoleImportFormat"];
            }
        }


        /// <summary>
        /// Import user Ad role according to CMSRoleImportFormat.
        /// </summary>
        public Func<string, string[]> RoleNameImporter
        {
            get
            {
                switch (RoleImportFormat)
                {
                    case DOMAIN_SAM_FORMAT:
                        return GetRolesForUserInDomainSamFormat;
                    case SAM_FORMAT:
                        return GetRolesForUserInSamFormat;
                    default:
                        return GetRolesForUserInDefaultFormat;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="config">Configuration</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            mName = name;
            mApplicationName = "/";

            AuthenticationHelper.ADConnectionStringName = config["connectionStringName"];
            AuthenticationHelper.ADUsername = config["connectionUsername"];
            AuthenticationHelper.ADPassword = config["connectionPassword"];

            // User name map specification
            if (String.IsNullOrEmpty(config["attributeMapUsername"]) && (CMSMembershipHelper.ADDefaultMapUserNameInternal != ""))
            {
                config["attributeMapUsername"] = CMSMembershipHelper.ADDefaultMapUserNameInternal;
            }
            AttributeMapUsername = config["attributeMapUsername"];

            base.Initialize(name, config);
        }


        /// <summary>
        /// Returns true if the user is in specific role.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="roleName">Role name</param>
        public override bool IsUserInRole(string username, string roleName)
        {
            // Get the roles
            string[] roles = GetRolesForUser(username);
            return roles.Any(role => role.EqualsCSafe(roleName, true));
        }


        /// <summary>
        /// Get roles for specified user
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>Roles for specified user</returns>
        public override string[] GetRolesForUser(string username)
        {
            return RoleNameImporter.Invoke(username);
        }


        /// <summary>
        /// Gets the roles for specified user.
        /// </summary>
        /// <param name="username">User name</param>
        public string[] GetRolesForUserInDefaultFormat(string username)
        {
            // Get the list of roles
            List<string> allRoles = new List<string>();
            DirectoryEntry root = new DirectoryEntry(SettingsHelper.ConnectionStrings[AuthenticationHelper.ADConnectionStringName].ConnectionString, AuthenticationHelper.ADUsername, AuthenticationHelper.ADPassword);
            DirectorySearcher searcher = new DirectorySearcher(root, string.Format(CultureInfo.InvariantCulture, "(&(objectClass=user)({0}={1}))", AttributeMapUsername, username));
            searcher.PropertiesToLoad.Add("memberOf");

            SearchResult result = searcher.FindOne();
            if ((result != null) && !string.IsNullOrEmpty(result.Path))
            {
                DirectoryEntry user = result.GetDirectoryEntry();
                PropertyValueCollection groups = user.Properties["memberOf"];

                // Process the groups
                foreach (string path in groups)
                {
                    string cn = null;
                    string dc = null;

                    // Parse the group
                    string[] parts = path.Split(',');
                    if (parts.Length > 0)
                    {
                        foreach (string part in parts)
                        {
                            string[] p = part.Split('=');
                            if (p[0].EqualsCSafe("cn", true) && (cn == null))
                            {
                                // Role name
                                cn = p[1];
                            }
                            else if (p[0].EqualsCSafe("dc", true) && (dc == null))
                            {
                                // Domain
                                dc = p[1];
                            }
                        }
                    }

                    // Add role
                    if (cn != null)
                    {
                        // Build full role name
                        string roleName = cn;
                        if (dc != null)
                        {
                            roleName = dc + "\\" + roleName;
                        }

                        allRoles.Add(roleName);
                    }
                }
            }

            return allRoles.ToArray();
        }

       
        /// <summary>
        /// Get roles for specified user in NetbiosDomainName\SamAccountName format.
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>Roles for specified user</returns>
        public string[] GetRolesForUserInDomainSamFormat(string username)
        {
            var principalContext = AuthenticationHelper.PrincipalContext;
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, username);
            var allRoles = new List<string>();

            if (userPrincipal != null)
            {
                // find the roles....
                var roles = userPrincipal.GetGroups();

                // enumerate over them
                allRoles.AddRange(from GroupPrincipal @group in roles select AuthenticationHelper.GetDomainSamAccountName(@group));
            }
            return allRoles.ToArray();
        }


        /// <summary>
        /// Get roles for specified user in SamAccountName format.
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>Roles for specified user</returns>
        public string[] GetRolesForUserInSamFormat(string username)
        {
            var principalContext = AuthenticationHelper.PrincipalContext;
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, username);
            var allRoles = new List<string>();

            if (userPrincipal != null)
            {
                // find the roles....
                var roles = userPrincipal.GetGroups();
                allRoles.AddRange(roles.Select(role => role.SamAccountName));
            }
            return allRoles.ToArray();
        }


        /// <summary>
        /// Adds user to role.
        /// </summary>
        /// <param name="usernames">User names</param>
        /// <param name="roleNames">Role names</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            ThrowNotSupported("AddUsersToRoles");
        }


        /// <summary>
        /// Creates role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        public override void CreateRole(string roleName)
        {
            ThrowNotSupported("CreateRole");
        }


        /// <summary>
        /// Deletes specified role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="throwOnPopulatedRole">Throw exception on role which is populated</param>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            ThrowNotSupported("DeleteRole");

            return false;
        }


        /// <summary>
        /// Find all user in specified role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="usernameToMatch">User name to match</param>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            ThrowNotSupported("FindUsersInRole");

            return null;
        }


        /// <summary>
        /// Returns all roles.
        /// </summary>
        public override string[] GetAllRoles()
        {
            ThrowNotSupported("GetAllRoles");

            return null;
        }


        /// <summary>
        /// Returns all user in specified role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        public override string[] GetUsersInRole(string roleName)
        {
            ThrowNotSupported("GetUsersInRole");

            return null;
        }


        /// <summary>
        /// Removes user from role.
        /// </summary>
        /// <param name="usernames">User name</param>
        /// <param name="roleNames">Role name</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            ThrowNotSupported("RemoveUsersFromRoles");
        }


        /// <summary>
        /// Determines whether specified role exists.
        /// </summary>
        /// <param name="roleName">Role name</param>
        public override bool RoleExists(string roleName)
        {
            ThrowNotSupported("RoleExists");

            return false;
        }


        /// <summary>
        /// Throws not supported exception.
        /// </summary>
        /// <param name="method">Method name</param>
        private void ThrowNotSupported(string method)
        {
            throw new Exception("[CMSADRoleProvider." + method + "]: Not supported method.");
        }

        #endregion
    }
}