using System;

using CMS.DocumentEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to page security and permissions.
    /// </summary>
    /// <pageTitle>Page security</pageTitle>
    internal class PageSecurity
    {
        /// <summary>
        /// Holds page ACL API examples.
        /// </summary>
        /// <groupHeading>Page-level permissions (ACLs)</groupHeading>
        private class PagePermissions
        {
            /// <heading>Making a page accessible only for authenticated users</heading>
            private void SetPageAuthenticationRequirement()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "en-us" culture version of the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Enables the "Requires authentication" property for the page
                    // Note: Setting the property to null makes the page inherit the "Requires authentication" value from its parent
                    page.IsSecuredNode = true;

                    // Saves the updated page to the database
                    page.Update();
                }
            }


            /// <heading>Setting page permissions for a user</heading>
            private void SetUserPermissions()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "en-us" culture version of the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Gets the user
                    UserInfo user = UserInfoProvider.GetUserInfo("Andy");

                    if (user != null)
                    {
                        // Prepares a value indicating that the 'Modify' permission is allowed
                        int allowed = DocumentSecurityHelper.GetNodePermissionFlags(NodePermissionsEnum.ModifyPermissions);
                        
                        // Prepares a value indicating that no page permissions are denied
                        int denied = 0;

                        // Sets the page's permission for the user (allows the 'Modify' permission)
                        AclItemInfoProvider.SetUserPermissions(page, allowed, denied, user);
                    }
                }
            }


            /// <heading>Setting page permissions for a role</heading>
            private void SetRolePermissions()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "en-us" culture version of the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Gets the role
                    RoleInfo role = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteName);

                    if (role != null)
                    {
                        // Prepares a value indicating that the 'Modify' permission is allowed
                        int allowed = DocumentSecurityHelper.GetNodePermissionFlags(NodePermissionsEnum.Modify);

                        // Prepares a value indicating that no page permissions are denied
                        int denied = 0;

                        // Sets the page's permission for the role (allows the 'Modify' permission)
                        AclItemInfoProvider.SetRolePermissions(page, allowed, denied, role);
                    }
                }
            }


            /// <heading>Breaking permission inheritance for a page</heading>
            private void BreakPermissionInheritance()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "en-us" culture version of the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Breaks permission inheritance for the page without copying parent permissions
                    bool copyParentPermissions = false;
                    AclInfoProvider.BreakInheritance(page, copyParentPermissions);
                }
            }


            /// <heading>Restoring permission inheritance for a page</heading>
            private void RestorePermissionInheritance()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "en-us" culture version of the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Restores permission inheritance for the page
                    AclInfoProvider.RestoreInheritance(page);
                }
            }


            /// <heading>Clearing the permission settings for a page</heading>
            private void DeletePermissions()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "en-us" culture version of the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Gets the ID of the ACL item that stores the page's permission settings
                    int nodeACLID = ValidationHelper.GetInteger(page.GetValue("NodeACLID"), 0);

                    // Deletes the page's ACL item
                    // Removes the page's permission settings for all users and roles
                    AclItemInfoProvider.DeleteAclItems(nodeACLID);
                }
            }
        }


        /// <summary>
        /// Holds API examples of checking permissions for pages.
        /// </summary>
        /// <groupHeading>Page permission checks</groupHeading>
        private class PagePermissionChecks
        {
            /// <heading>Checking permissions for the content module</heading>
            private void CheckContentModulePermissions()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Andy");

                if (user != null)
                {
                    // Checks whether the user has the Read permission for the Content module
                    if (UserInfoProvider.IsAuthorizedPerResource("CMS.Content", "Read", SiteContext.CurrentSiteName, user))
                    {
                        // Perform an action (the user has the read permission for content)
                    }
                }
            }


            /// <heading>Checking permissions for a page type</heading>
            private void CheckPageTypePermissions()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Andy");

                if (user != null)
                {
                    // Checks whether the user has the Read permission for the CMS.MenuItem page type
                    if (UserInfoProvider.IsAuthorizedPerClass(SystemDocumentTypes.MenuItem, "Read", SiteContext.CurrentSiteName, user))
                    {
                        // Perform an action (the user is authorized to read CMS.MenuItem page types)
                    }
                }
            }


            /// <heading>Checking permissions for specific pages (ACLs)</heading>
            private void CheckPagePermissions()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "en-us" culture version of the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Gets the user
                    UserInfo user = UserInfoProvider.GetUserInfo("Andy");

                    if (user != null)
                    {
                        // Checks whether the user is authorized to modify the page
                        if (TreeSecurityProvider.IsAuthorizedPerNode(page, NodePermissionsEnum.Modify, user) == AuthorizationResultEnum.Allowed)
                        {
                            // Perform an action (the user is allowed to modify the page)
                        }
                    }
                }
            }


            /// <heading>Filtering loaded pages according to permissions</heading>
            private void FilterDataSet()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Andy");
                
                // Sets the action context to the specified user
                using (new CMSActionContext(user))
                {
                    // Gets all news pages under the current site's "/News" section for which the user has Read permissions
                    var newsPages = tree.SelectNodes("CMS.News")
                                            .OnSite(SiteContext.CurrentSiteName)
                                            .Path("/News", PathTypeEnum.Children)
                                            .CheckPermissions();
                }
            }
        }
    }
}
