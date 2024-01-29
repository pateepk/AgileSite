using System;
using System.Data;

using CMS.Membership;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds user-related API examples.
    /// </summary>
    /// <pageTitle>Users</pageTitle>
    internal class UsersMain
    {
        /// <summary>
        /// Holds user API examples.
        /// </summary>
        /// <groupHeading>Users</groupHeading>
        private class Users
        {
            /// <heading>Creating a new user</heading>
            private void CreateUser()
            {
                // Creates a new user object
                UserInfo newUser = new UserInfo();

                // Sets the user properties
                newUser.FullName = "New user";
                newUser.UserName = "NewUser";
                newUser.Email = "new.user@domain.com";
                newUser.PreferredCultureCode = "en-us";

                // Sets the user's privilege level to 'Editor'
                newUser.SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.Editor;

                // Saves the user to the database
                UserInfoProvider.SetUserInfo(newUser);
            }


            /// <heading>Updating an existing user</heading>
            private void GetAndUpdateUser()
            {
                // Gets the user
                UserInfo updateUser = UserInfoProvider.GetUserInfo("NewUser");
                if (updateUser != null)
                {
                    // Updates the user's properties
                    updateUser.FullName = updateUser.FullName.ToLowerCSafe();

                    // Saves the changes to the database
                    UserInfoProvider.SetUserInfo(updateUser);
                }
            }


            /// <heading>Updating multiple users</heading>
            private void GetAndBulkUpdateUsers()
            {
                // Gets all users whose username starts with 'NewUser'
                var users = UserInfoProvider.GetUsers().WhereStartsWith("UserName", "NewUser");

                // Loops through individual users
                foreach (UserInfo modifyUser in users)
                {
                    // Updates the user properties
                    modifyUser.FullName = modifyUser.FullName.ToUpper();

                    // Saves the changes to the database
                    UserInfoProvider.SetUserInfo(modifyUser);
                }
            }


            /// <heading>Working with custom user fields</heading>
            private void CustomUserFields()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");

                if (user != null)
                {
                    // Attempts to retrieve a value from a custom text field named 'CustomField'
                    string value = user.GetValue("CustomField", "");

                    // Sets a modified value for the custom field
                    user.SetValue("CustomField", value + "_customSuffix");

                    // Saves the changes to the database
                    UserInfoProvider.SetUserInfo(user);
                }
            }            


            /// <heading>Deleting a user</heading>
            private void DeleteUser()
            {
                // Gets the user
                UserInfo deleteUser = UserInfoProvider.GetUserInfo("NewUser");

                if (deleteUser != null)
                {
                    // Deletes the user
                    UserInfoProvider.DeleteUser(deleteUser);
                }
            }
        }

        /// <summary>
        /// Holds user-site API examples.
        /// </summary>
        /// <groupHeading>User-site relationships</groupHeading>
        private class UserSite
        {
            /// <heading>Getting all sites to which a user is assigned</heading>
            private void GetUserSites()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");

                if (user != null)
                {
                    // Gets the sites to which the user is assigned
                    var userSiteIDs = UserSiteInfoProvider.GetUserSites().Column("SiteID").WhereEquals("UserID", user.UserID);
                    var sites = SiteInfoProvider.GetSites().WhereIn("SiteID", userSiteIDs);

                    // Loops through the sites
                    foreach (SiteInfo site in sites)
                    {
                        // Process the site
                    }
                }
            }

            /// <heading>Assigning a user to a site</heading>
            private void AddUserToSite()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");
                if (user != null)
                {
                    // Adds the user to the site
                    UserInfoProvider.AddUserToSite(user.UserName, SiteContext.CurrentSiteName);
                }
            }

            /// <heading>Removing a user from a site</heading>
            private void RemoveUserFromSite()
            {
                // Gets the user
                UserInfo removeUser = UserInfoProvider.GetUserInfo("NewUser");
                if (removeUser != null)
                {
                    // Removes the user from the site
                    UserInfoProvider.RemoveUserFromSite(removeUser.UserName, SiteContext.CurrentSiteName);
                }
            }
        }


        /// <summary>
        /// Holds user authentication API examples.
        /// </summary>
        /// <groupHeading>User authentication</groupHeading>
        private class UserAuthentication
        {
            /// <heading>Authenticating user credentials</heading>
            private void AuthenticateUser()
            {
                UserInfo user = null;

                // Attempts to authenticate user credentials (username and password) against the current site
                user = AuthenticationHelper.AuthenticateUser("username", "password", SiteContext.CurrentSiteName);

                if (user != null)
                {
                    // Authentication was successful
                }
            }


            /// <heading>Signing in a user via forms authentication</heading>
            private void SignInUser()
            {
                UserInfo user = null;

                // Attempts to authenticate user credentials (username and password) against the current site
                user = AuthenticationHelper.AuthenticateUser("username", "password", SiteContext.CurrentSiteName);

                if (user != null)
                {
                    // Sets the forms authentication cookie
                    System.Web.Security.FormsAuthentication.SetAuthCookie(user.UserName, false);

                    // Redirects (or refreshes) the page to apply the authentication cookie
                    URLHelper.Redirect(RequestContext.CurrentURL);
                }
            }
        }


        /// <summary>
        /// Holds user authorization API examples.
        /// </summary>
        /// <groupHeading>User authorization</groupHeading>
        private class UserAuthorization
        {
            /// <heading>Checking the user privilege level</heading>
            private void CheckPrivilegeLevel()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");

                if (user != null)
                {
                    // Checks whether the user has the Editor privilege level or higher
                    if (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, SiteContext.CurrentSiteName))
                    {
                        // Perform an action (the user has the required privilege level)
                    }
                }
            }

            /// <heading>Checking permissions for a module</heading>
            private void CheckModulePermissions()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");

                if (user != null)
                {
                    // Checks whether the user has the Read permission for the Content module
                    if (UserInfoProvider.IsAuthorizedPerResource("CMS.Content", "Read", SiteContext.CurrentSiteName, user))
                    {
                        // Perform an action (the user has the required module permission)
                    }
                }
            }

            /// <heading>Checking permissions for a page type or custom table</heading>
            private void CheckPageTypePermissions()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");

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
            private void IsAuthorizedToDocument()
            {
                // Creates a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the Example page
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Gets the user
                    UserInfo user = UserInfoProvider.GetUserInfo("NewUser");

                    if (user != null)
                    {
                        // Checks whether the user is authorized to read the page
                        if (user.IsAuthorizedPerTreeNode(page, NodePermissionsEnum.Read) == AuthorizationResultEnum.Allowed)
                        {
                            // Perform an action (the user is allowed to read the page)
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Holds online user API examples.
        /// </summary>
        /// <groupHeading>Online users</groupHeading>
        private class UserOnline
        {
            /// <heading>Getting and updating online users</heading>
            private void GetOnlineUsers()
            {
                string where = "";
                int topN = 10;
                string orderBy = "";
                string location = "";
                string siteName = SiteContext.CurrentSiteName;
                bool includeHidden = true;
                bool includeKicked = false;

                // Gets DataSet of online users
                DataSet users = SessionManager.GetOnlineUsers(where, orderBy, topN, location, siteName, includeHidden, includeKicked);
                if (!DataHelper.DataSourceIsEmpty(users))
                {
                    // Loops through the online user data
                    foreach (DataRow userDr in users.Tables[0].Rows)
                    {
                        // Creates a user from the DataRow
                        UserInfo modifyUser = new UserInfo(userDr);

                        // Updates the user's properties
                        modifyUser.FullName = modifyUser.FullName.ToUpper();

                        // Saves the changes to the database
                        UserInfoProvider.SetUserInfo(modifyUser);
                    }
                }
            }

            /// <heading>Checking if a user is online</heading>
            private void IsUserOnline()
            {
                bool includeHidden = true;

                // Gets user and site objects
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");
                SiteInfo site = SiteInfoProvider.GetSiteInfo(SiteContext.CurrentSiteName);

                if ((user != null) && (site != null))
                {
                    // Checks if the user is online
                    bool userIsOnline = SessionManager.IsUserOnline(site.SiteName, user.UserID, includeHidden);
                }
            }

            /// <heading>Kicking an online user</heading>
            private void KickUser()
            {
                // Gets the user 
                UserInfo kickedUser = UserInfoProvider.GetUserInfo("NewUser");

                if (kickedUser != null)
                {
                    // Kicks the user
                    SessionManager.KickUser(kickedUser.UserID);
                }
            }
        }


        /// <summary>
        /// Holds API examples related to user macro signature identities.
        /// </summary>
        /// <groupHeading>User macro signature identities</groupHeading>
        private class UserMacroSignatures
        {
            /// <heading>Creating a macro signature identity</heading>
            private void CreateMacroSignatureIdentity()
            {
                // Creates a new identity object
                MacroIdentityInfo newMacroIdentity = new MacroIdentityInfo();

                // Sets the identity name
                newMacroIdentity.MacroIdentityName = "CustomIdentity";

                // Gets a user and assigns them as the identity's effective user
                UserInfo effectiveUser = UserInfoProvider.GetUserInfo("administrator");
                newMacroIdentity.MacroIdentityEffectiveUserID = effectiveUser.UserID;

                // Saves the identity to the database
                MacroIdentityInfoProvider.SetMacroIdentityInfo(newMacroIdentity);
            }

            /// <heading>Assigning a signature identity to a user</heading>
            private void AssignSignatureIdentity()
            {
                // Gets a user
                UserInfo user = UserInfoProvider.GetUserInfo("NewUser");

                // Gets a macro signature identity
                MacroIdentityInfo macroIdentity = MacroIdentityInfoProvider.GetMacroIdentityInfo("CustomIdentity");

                // Assigns the macro signature identity to the user
                UserMacroIdentityHelper.SetMacroIdentity(user, macroIdentity.MacroIdentityID);
            }
        }
    }
}
