using System;

using CMS.Base;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Modules;

namespace APIExamples
{
    /// <summary>
    /// Holds role-related API examples.
    /// </summary>
    /// <pageTitle>Roles</pageTitle>
    internal class RolesMain
    {
        /// <summary>
        /// Holds role API examples.
        /// </summary>
        /// <groupHeading>Roles</groupHeading>
        private class Roles
        {
            /// <heading>Creating a new role</heading>
            private void CreateRole()
            {
                // Creates a new role object
                RoleInfo newRole = new RoleInfo();

                // Sets the role properties
                newRole.RoleDisplayName = "New role";
                newRole.RoleName = "NewRole";
                newRole.SiteID = SiteContext.CurrentSiteID;

                // Verifies that the role is unique for the current site
                if (!RoleInfoProvider.RoleExists(newRole.RoleName, SiteContext.CurrentSiteName))
                {
                    // Saves the role to the database
                    RoleInfoProvider.SetRoleInfo(newRole);
                }
                else
                {
                    // A role with the same name already exists on the site
                }
            }

            /// <heading>Updating an existing role</heading>
            private void GetAndUpdateRole()
            {
                // Gets the role
                RoleInfo updateRole = RoleInfoProvider.GetRoleInfo("NewRole", SiteContext.CurrentSiteID);
                if (updateRole != null)
                {
                    // Updates the role's properties
                    updateRole.RoleDisplayName = updateRole.RoleDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    RoleInfoProvider.SetRoleInfo(updateRole);
                }
            }

            /// <heading>Updating multiple roles</heading>
            private void GetAndBulkUpdateRoles()
            {
                // Gets all roles whose name starts with 'NewRole'
                var roles = RoleInfoProvider.GetRoles().WhereStartsWith("RoleName", "NewRole");

                // Loops through individual roles
                foreach (RoleInfo modifyRole in roles)
                {
                    // Updates the role properties
                    modifyRole.RoleDisplayName = modifyRole.RoleDisplayName.ToUpper();

                    // Saves the changes
                    RoleInfoProvider.SetRoleInfo(modifyRole);
                }
            }

            /// <heading>Deleting a role</heading>
            private void DeleteRole()
            {
                // Gets the role
                RoleInfo deleteRole = RoleInfoProvider.GetRoleInfo("NewRole", SiteContext.CurrentSiteID);

                if (deleteRole != null)
                {
                    // Deletes the role
                    RoleInfoProvider.DeleteRoleInfo(deleteRole);
                }                
            }
        }


        /// <summary>
        /// Holds user-role API examples.
        /// </summary>
        /// <groupHeading>User-role relationships</groupHeading>
        private class UserRole
        {
            /// <heading>Checking if a user is in a role</heading>
            private void CheckUserRole()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Username");

                bool checkGlobalRoles = true;
                bool checkMembership = true;

                // Checks whether the user is assigned to a role with the "Rolename" code name
                // The role can be assigned for the current site, as a global role, or indirectly through a membership
                bool result = user.IsInRole("Rolename", SiteContext.CurrentSiteName, checkGlobalRoles, checkMembership);
            }

            /// <heading>Getting all roles of a user</heading>
            private void GetUserRoles()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Username");

                if (user != null)
                {
                    // Gets the user's roles
                    var userRoleIDs = UserRoleInfoProvider.GetUserRoles().Column("RoleID").WhereEquals("UserID", user.UserID);
                    var roles = RoleInfoProvider.GetRoles().WhereIn("RoleID", userRoleIDs);

                    // Loops through the roles
                    foreach (RoleInfo role in roles)
                    {                        
                        // Process the role
                    }
                }
            }
            
            /// <heading>Getting all users in a role</heading>
            private void GetRoleUsers()
            {
                // Gets the role from the current site
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteName);

                if (role != null)
                {
                    // Gets the role's users
                    var roleUserIDs = UserRoleInfoProvider.GetUserRoles().Column("UserID").WhereEquals("RoleID", role.RoleID);
                    var users = UserInfoProvider.GetUsers().WhereIn("UserID", roleUserIDs);
                    
                    // Loops through the users
                    foreach (UserInfo user in users)
                    {
                        // Process the user
                    }
                }
            }

            /// <heading>Adding a user to a role</heading>
            private void AddUserToRole()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Username");

                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteName);

                if ((user != null) && (role != null))
                {
                    // Adds the user to the role
                    UserInfoProvider.AddUserToRole(user.UserName, role.RoleName, SiteContext.CurrentSiteName);
                }
            }


            /// <heading>Removing a user from a role</heading>
            private void RemoveUserFromRole()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Username");

                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteName);

                if ((user != null) && (role != null))
                {
                    // Removes the user from the role
                    UserInfoProvider.RemoveUserFromRole(user.UserName, role.RoleName, SiteContext.CurrentSiteName);
                }
            }
        }


        /// <summary>
        /// Holds role permission API examples.
        /// </summary>
        /// <groupHeading>Role permissions</groupHeading>
        private class RolePermission
        {
            /// <heading>Assigning a module permission to a role</heading>
            private void AddPermissionToRole()
            {
                // Gets the module permission
                PermissionNameInfo permission = PermissionNameInfoProvider.GetPermissionNameInfo("Read", "CMS.Content", null);

                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteID);

                if ((permission != null) && (role != null))
                {
                    // Creates an object representing the role-permission relationship
                    RolePermissionInfo newRolePermission = new RolePermissionInfo();

                    // Assigns the permission to the role
                    newRolePermission.PermissionID = permission.PermissionId;
                    newRolePermission.RoleID = role.RoleID;

                    // Saves the role-permission relationship into the database
                    RolePermissionInfoProvider.SetRolePermissionInfo(newRolePermission);
                }
            }


            /// <heading>Removing a module permission from a role</heading>
            private void RemovePermissionFromRole()
            {
                // Gets the module permission
                PermissionNameInfo permission = PermissionNameInfoProvider.GetPermissionNameInfo("Read", "CMS.Content", null);

                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteID);

                if ((permission != null) && (role != null))
                {
                    // Gets the object representing the role-permission relationship
                    RolePermissionInfo deleteRolePermission = RolePermissionInfoProvider.GetRolePermissionInfo(role.RoleID, permission.PermissionId);

                    if (deleteRolePermission != null)
                    {
                        // Removes the permission from the role
                        RolePermissionInfoProvider.DeleteRolePermissionInfo(deleteRolePermission);
                    }
                }
            }
        }


        /// <summary>
        /// Holds role UI personalization API examples.
        /// </summary>
        /// <groupHeading>UI personalization</groupHeading>
        private class RoleUIPersonalization
        {
            /// <heading>Assigning a UI element to a role</heading>
            private void AddUIElementToRole()
            {
                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteID);

                // Gets the UI element (the element representing the Design tab in the Pages application in this case)
                UIElementInfo element = UIElementInfoProvider.GetUIElementInfo("CMS.Design", "Design");

                if ((role != null) && (element != null))
                {
                    // Creates an object representing the role-UI element relationship
                    RoleUIElementInfo newRoleElement = new RoleUIElementInfo();

                    // Assigns the UI element to the role
                    newRoleElement.RoleID = role.RoleID;
                    newRoleElement.ElementID = element.ElementID;

                    // Saves the new relationship to the database
                    RoleUIElementInfoProvider.SetRoleUIElementInfo(newRoleElement);
                }
            }


            /// <heading>Removing a UI element from a role</heading>
            private void RemoveUIElementFromRole()
            {
                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteID);

                // Gets the UI element (the element representing the Design tab in the Pages application in this case)
                UIElementInfo element = UIElementInfoProvider.GetUIElementInfo("CMS.Design", "Design");

                if ((role != null) && (element != null))
                {
                    // Gets the object representing the relationship between the role and the UI element
                    RoleUIElementInfo deleteRoleElement = RoleUIElementInfoProvider.GetRoleUIElementInfo(role.RoleID, element.ElementID);

                    if (deleteRoleElement != null)
                    {
                        // Removes the UI element from the role
                        RoleUIElementInfoProvider.DeleteRoleUIElementInfo(deleteRoleElement);
                    }
                }
            }
        }
    }
}
