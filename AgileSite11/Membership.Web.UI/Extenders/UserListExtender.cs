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
        #region "Variables"

        private CurrentUserInfo currentUser;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the current user info.
        /// </summary>
        protected CurrentUserInfo CurrentUserObj
        {
            get
            {
                return currentUser ?? (currentUser = MembershipContext.AuthenticatedUser);
            }
        }

        #endregion


        #region "Methods"

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

            switch (sourceName.ToLowerCSafe())
            {
                case "userenabled":
                    return UniGridFunctions.ColoredSpanYesNo(parameter);

                case "formattedusername":
                    return HTMLHelper.HTMLEncode(UserInfoProvider.GetFormattedUserName(Convert.ToString(parameter), null));

                case "userprivilegelevel":
                    user = GetUser((DataRowView)parameter);
                    var privilegeLevel = user.SiteIndependentPrivilegeLevel;
                    return privilegeLevel == UserPrivilegeLevelEnum.None ? String.Empty : privilegeLevel.ToLocalizedString(resourcePrefix: null);

                case "edit":
                    // Edit action
                    user = GetUser((GridViewRow)parameter);
                    button.Enabled = HasSufficientPrivilege(user);
                    break;

                case "delete":
                    // Delete action
                    user = GetUser((GridViewRow)parameter);
                    button.Enabled = HasSufficientPrivilege(user) && !user.IsPublic();
                    break;

                case "roles":
                    // Roles action
                    user = GetUser((GridViewRow)parameter);
                    button.Enabled = HasSufficientPrivilege(user);
                    break;

                case "haspassword":
                    // Has password action
                    user = GetUser((GridViewRow)parameter);
                    bool hasPassword = ValidationHelper.GetBoolean(((DataRowView)((GridViewRow)parameter).DataItem).Row["UserHasPassword"], true);

                    button.Visible = CurrentUserObj.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)
                        && !hasPassword && !user.IsPublic() && !user.IsExternal;
                    button.OnClientClick = "return false;";
                    break;

                case "#objectmenu":
                    user = GetUser((GridViewRow)parameter);
                    button.Visible = HasSufficientPrivilege(user);
                    break;
            }
            return parameter;
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
                if (userId == CurrentUserObj.UserID)
                {
                    ((CMSPage)Control.Page).ShowError(ResHelper.GetString("Administration-User_List.ErrorOwnAccount"));
                    return;
                }

                if (!userToDelete.CheckPermissions(PermissionsEnum.Delete, SiteContext.CurrentSiteName, CurrentUserObj))
                {
                    CMSPage.RedirectToAccessDenied("CMS.Users", "Modify");
                }

                // Global administrator account could be deleted only by global administrator
                if (userToDelete.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && !CurrentUserObj.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
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
        /// or <paramref name="user"/> to operate with is either <see cref="UserPrivilegeLevelEnum.Editor"/> or <see cref="UserPrivilegeLevelEnum.None"/>
        /// or the <paramref name="user"/> is themselves.
        /// </summary>
        private bool HasSufficientPrivilege(IUserInfo user)
        {
            return CurrentUserObj.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin)
                || !user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)
                || (user.UserID == CurrentUserObj.UserID);
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

        #endregion
    }
}