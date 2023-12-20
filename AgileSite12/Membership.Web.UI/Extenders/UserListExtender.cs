using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;


[assembly: RegisterCustomClass("UserListExtender", typeof(UserListExtender))]

namespace CMS.Membership.Web.UI
{
    /// <summary>
    /// User list <see cref="UniGrid"/> extender.
    /// </summary>
    public class UserListExtender : ControlExtender<UniGrid>
    {
        private CurrentUserInfo mAuthenticatedUser;


        /// <summary>
        /// Gets the current user info.
        /// </summary>
        protected CurrentUserInfo AuthenticatedUser => mAuthenticatedUser ?? (mAuthenticatedUser = MembershipContext.AuthenticatedUser);


        /// <summary>
        /// OnInit event.
        /// </summary>
        public override void OnInit()
        {
            string[] columns = {
                "UserID",
                "UserName",
                "FullName",
                "Email",
                "UserNickName",
                "UserCreated",
                "UserEnabled",
                @"(CASE 
                    WHEN UserPassword IS NULL OR UserPassword = '' THEN 0 
                    ELSE 1 
                    END) AS UserHasPassword",
                "UserIsExternal",
                "UserIsDomain",
                "UserPrivilegeLevel"
            };
            Control.Columns = columns.Join(", ");
            Control.IsLiveSite = false;

            Control.HideFilterButton = true;

            // Register scripts
            ScriptHelper.RegisterDialogScript(Control.Page);
            ScriptHelper.RegisterClientScriptBlock(Control.Page, typeof(string), "ManageRoles", ScriptHelper.GetScript(
                "function manageRoles(userId) {" +
                "    modalDialog('" + UrlResolver.ResolveUrl("~/CMSModules/Membership/Pages/Users/User_ManageRoles.aspx") + "?userId=' + userId, 'ManageUserRoles', 800, 440);" +
                "}"));

            Control.OnExternalDataBound += OnExternalDataBound;
            Control.OnAction += OnAction;
        }


        /// <summary>
        /// Handles external data-bound event of <see cref="UniGrid"/>.
        /// </summary>
        protected object OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            UserInfo user;
            var button = sender as CMSGridActionButton;

            switch (sourceName?.ToLowerInvariant())
            {
                case "userenabled":
                    return UniGridFunctions.ColoredSpanYesNo(parameter);

                case "formattedusername":
                    return HTMLHelper.HTMLEncode(UserInfoProvider.GetFormattedUserName(Convert.ToString(parameter), null));

                case "userprivilegelevel":
                    user = GetUser((DataRowView)parameter);
                    var privilegeLevel = user.SiteIndependentPrivilegeLevel;
                    return privilegeLevel == UserPrivilegeLevelEnum.None ? String.Empty : privilegeLevel.ToLocalizedString(null);

                case "edit":
                    // Edit action
                    user = GetUser((GridViewRow)parameter);
                    button.Enabled = HasSufficientPrivilege(user, PermissionsEnum.Read) || IsAuthenticatedUser(user);
                    break;

                case "delete":
                    // Delete action
                    user = GetUser((GridViewRow)parameter);
                    button.Enabled = HasSufficientPrivilege(user, PermissionsEnum.Delete)
                        && !user.IsPublic()
                        && !IsAuthenticatedUser(user);
                    break;

                case "roles":
                    // Roles action
                    user = GetUser((GridViewRow)parameter);
                    button.Enabled = (HasSufficientPrivilege(user, PermissionsEnum.Modify) || IsAuthenticatedUser(user))
                        && AuthenticatedUser.IsAuthorizedPerResource("CMS.Roles", "Read");
                    break;

                case "haspassword":
                    // Has password action
                    PreparePasswordButton(parameter, button);
                    break;

                case "#objectmenu":
                    user = GetUser((GridViewRow)parameter);
                    button.Visible = AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) || IsAuthenticatedUser(user);
                    break;
            }
            return parameter;
        }


        /// <summary>
        /// Prepares the button displaying information about user password.
        /// </summary>
        internal void PreparePasswordButton(object parameter, CMSGridActionButton button)
        {
            var user = GetUser((GridViewRow)parameter);
            var hasPassword = ValidationHelper.GetBoolean(((DataRowView)((GridViewRow)parameter).DataItem).Row["UserHasPassword"], true);
            var shouldHavePassword = !user.IsPublic() && !user.IsExternal && !user.UserIsDomain;
            var canModifyUser = HasSufficientPrivilege(user, PermissionsEnum.Modify) || IsAuthenticatedUser(user);
            button.Visible = canModifyUser && !hasPassword && shouldHavePassword;

            if (button.Visible)
            {
                // Bind redirect action to the editing password of user
                var editUserUrl = GetUserPasswordEditationUrl(user);
                button.OnClientClick = $"return {Control.GetJSModule()}.redir({ScriptHelper.GetString(editUserUrl)});";
            }
        }


        /// <summary>
        /// Returns url leading to password tab in user detail.
        /// </summary>
        private string GetUserPasswordEditationUrl(IUserInfo user)
        {
            var tabName = AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) ? "password" : "cmsdesk.password";

            var editUserUrl = ScriptHelper.ResolveUrl(String.Format(Control.EditActionUrl, user.UserID));
            editUserUrl = URLHelper.AddParameterToUrl(editUserUrl, "tabName", tabName);

            return editUserUrl;
        }


        /// <summary>
        /// Handles the UniGrid's OnAction event.
        /// </summary>
        /// <param name="actionName">Name of item (button) that threw event</param>
        /// <param name="actionArgument">ID (value of Primary key) of corresponding data row</param>
        protected void OnAction(string actionName, object actionArgument)
        {
            if (actionName != "delete")
            {
                return;
            }

            try
            {
                int userId = Convert.ToInt32(actionArgument);
                UserInfo userToDelete = UserInfoProvider.GetUserInfo(userId);
                if (userToDelete == null)
                {
                    return;
                }

                // It is not possible to delete own user account
                if (userId == AuthenticatedUser.UserID)
                {
                    ((CMSPage)Control.Page).ShowError(ResHelper.GetString("Administration-User_List.ErrorOwnAccount"));
                    return;
                }

                if (!userToDelete.CheckPermissions(PermissionsEnum.Delete, SiteContext.CurrentSiteName, AuthenticatedUser))
                {
                    CMSPage.RedirectToAccessDenied("CMS.Users", "Modify");
                }

                // Administrator account could be deleted only by global administrator
                if (userToDelete.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && !AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                {
                    ((CMSPage)Control.Page).ShowError(ResHelper.GetString("Administration-User_List.ErrorNoGlobalAdmin"));
                    return;
                }

                // It is not possible to delete default system administrator
                if (userId == UserInfoProvider.AdministratorUser.UserID)
                {
                    ((CMSPage)Control.Page).ShowError(ResHelper.GetString("Administration-User_List.ErrorDefaultUser"));
                    return;
                }

                SessionManager.RemoveUser(userId);
                UserInfoProvider.DeleteUser(userToDelete);
            }
            catch (Exception ex)
            {
                ((CMSPage)Control.Page).ShowError(ex.Message);
            }
        }


        /// <summary>
        /// Returns <c>true</c> if and only if the current user is <see cref="UserPrivilegeLevelEnum.GlobalAdmin"/>
        /// or has given <paramref name="permission"/> to the <paramref name="user"/>.
        /// </summary>
        private bool HasSufficientPrivilege(UserInfo user, PermissionsEnum permission)
        {
            return AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin)
                || user.CheckPermissions(permission, SiteContext.CurrentSiteName, AuthenticatedUser);
        }


        /// <summary>
        /// Returns <c>true</c> when given <paramref name="user"/> is equal to <see cref="MembershipContext.AuthenticatedUser"/>.
        /// </summary>
        private bool IsAuthenticatedUser(IUserInfo user)
        {
            return user.UserID == AuthenticatedUser.UserID;
        }


        /// <summary>
        /// Returns (eventually incomplete) <see cref="UserInfo"/> based on given <see cref="GridViewRow"/>.
        /// </summary>
        private static UserInfo GetUser(GridViewRow row)
        {
            var userDataItem = (DataRowView)row.DataItem;
            return GetUser(userDataItem);
        }


        /// <summary>
        /// Returns (eventually incomplete) <see cref="UserInfo"/> based on given <see cref="DataRowView"/>.
        /// </summary>
        private static UserInfo GetUser(DataRowView row)
        {
            return new UserInfo(row.Row);
        }
    }
}