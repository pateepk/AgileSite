using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Activities.Loggers;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Protection;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using WTE.SVC;

/// <summary>
/// Summary description for MoblzAuthentication
/// </summary>
public class MoblzAuthentication
{
    public MoblzAuthentication()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="p_userName"></param>
    /// <param name="p_password"></param>
    /// <returns></returns>
    public static bool Login(string p_userName, string p_password)
    {
        LoginData data = new LoginData();
        data.UserName = p_userName;
        data.Password = p_password;
        return AuthenticateUser(data);
    }

    /// <summary>
    /// Authenicate a user with full info.
    /// </summary>
    /// <param name="p_loginInfo"></param>
    /// <returns></returns>
    public static bool AuthenticateUser(LoginData p_loginInfo)
    {
        bool authenticated = false;
        if (p_loginInfo != null)
        {
            authenticated = Membership.Provider.ValidateUser(p_loginInfo.UserName, p_loginInfo.Password);
            if (authenticated)
            {
                MembershipActivityLogger.LogLogin(p_loginInfo.UserName, null);
            }
        }
        return authenticated;
    }

    /// <summary>
    /// Register a user
    /// </summary>
    /// <param name="p_userName"></param>
    /// <param name="p_password"></param>
    /// <param name="p_email"></param>
    /// <param name="p_firstName"></param>
    /// <param name="p_lastname"></param>
    /// <param name="p_id"></param>
    /// <returns></returns>
    public static MOBLZAuthenticationInfo RegisterUser(string p_userName, string p_password, string p_email, string p_firstName, string p_lastname, string p_id)
    {
        MOBLZAuthenticationInfo data = new MOBLZAuthenticationInfo();
        data.LoginInfo.LoginValid = false;
        data.LoginInfo.UserName = p_userName;
        data.LoginInfo.Password = p_password;
        data.LoginInfo.MobileDeviceID = p_id;
        data.UserInfo.EmailAddress = p_email;
        data.UserInfo.FirstName = p_firstName;
        data.UserInfo.LastName = p_lastname;
        return RegisterUserAccount(data);
    }

    /// <summary>
    /// Register a user
    /// </summary>
    /// <param name="p_userName"></param>
    /// <param name="p_password"></param>
    /// <param name="p_email"></param>
    /// <param name="p_firstName"></param>
    /// <param name="p_lastName"></param>
    /// <returns></returns>
    public static MOBLZAuthenticationInfo RegisterUserAccount(MOBLZAuthenticationInfo p_registrationinfo)
    {
        MOBLZAuthenticationInfo p_newAccountInfo = new MOBLZAuthenticationInfo();
        p_newAccountInfo.LoginInfo.LoginValid = false;
        p_newAccountInfo.LoginInfo.ErrorMessage = "Account Info Required for registration";
        p_newAccountInfo.LoginInfo.LoginStatusCode = 9999;

        bool canContinue = p_registrationinfo != null;

        SiteInfo site = SiteInfoProvider.GetSiteInfo(19);
        String siteName = site.SiteName;

        if (canContinue)
        {
            string userName = p_registrationinfo.LoginInfo.UserName;
            string userEmail = p_registrationinfo.UserInfo.EmailAddress;
            string userNickName = null;

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = userEmail;
            }

            if (String.IsNullOrWhiteSpace(userName))
            {
                // we have an error
                p_newAccountInfo.LoginInfo.LoginValid = false;
                p_newAccountInfo.LoginInfo.ErrorMessage = "Username/Email required to register";
                p_newAccountInfo.LoginInfo.LoginStatusCode = 9998;
                canContinue = false;
            }

            // check the user name
            // Check for reserved user names like administrator, sysadmin, ...
            if (UserInfoProvider.NameIsReserved(siteName, userName))
            {
                p_newAccountInfo.LoginInfo.LoginValid = false;
                p_newAccountInfo.LoginInfo.ErrorMessage = "user name is reserved";
                p_newAccountInfo.LoginInfo.LoginStatusCode = 9997;
                canContinue = false;
            }

            if (UserInfoProvider.NameIsReserved(siteName, userNickName))
            {
                p_newAccountInfo.LoginInfo.LoginValid = false;
                p_newAccountInfo.LoginInfo.ErrorMessage = "user name is reserved";
                p_newAccountInfo.LoginInfo.LoginStatusCode = 9997;
                canContinue = false;
            }

            //// Check whether email is unique if it is required
            //if (!UserInfoProvider.IsEmailUnique(userEmail, siteList, 0))
            //{
            //    ShowError(GetString("UserInfo.EmailAlreadyExist"));
            //    return;
            //}

            if (canContinue)
            {
                // Check whether another user with this user name (which is effectively email) does not exist
                UserInfo ui = UserInfoProvider.GetUserInfo(userName);
                SiteInfo si = SiteContext.CurrentSite;
                UserInfo siteui = UserInfoProvider.GetUserInfo(UserInfoProvider.EnsureSitePrefixUserName(userName, si));

                if ((ui != null) || (siteui != null))
                {
                    // we have an error
                    p_newAccountInfo.LoginInfo.LoginValid = false;
                    p_newAccountInfo.LoginInfo.ErrorMessage = "UserName/Email already exists";
                    p_newAccountInfo.LoginInfo.LoginStatusCode = 9996;
                    canContinue = false;
                }

                if (canContinue)
                {
                    // Need to check password strength here? - pk
                    // do we need to check the email address length? -pk

                    // create the user in memory
                    ui = new UserInfo();
                    ui.PreferredCultureCode = "";
                    ui.Email = userEmail;
                    ui.FirstName = p_registrationinfo.UserInfo.FirstName;
                    ui.LastName = p_registrationinfo.UserInfo.LastName;
                    ui.FullName = UserInfoProvider.GetFullName(ui.FirstName, String.Empty, ui.LastName);
                    ui.MiddleName = "";
                    ui.UserMFRequired = false;

                    //check to see if the username is actually valid
                    if (!ValidationHelper.IsUserName(userName))
                    {
                        // we have an error
                        p_newAccountInfo.LoginInfo.LoginValid = false;
                        p_newAccountInfo.LoginInfo.ErrorMessage = "Invalid user name/email";
                        p_newAccountInfo.LoginInfo.LoginStatusCode = 9995;
                        canContinue = false;
                    }

                    if (canContinue)
                    {
                        ui.UserName = userName;
                        // Ensure site prefixes
                        if (UserInfoProvider.UserNameSitePrefixEnabled(siteName))
                        {
                            ui.UserName = UserInfoProvider.EnsureSitePrefixUserName(userName, si);
                        }

                        ui.Enabled = true;
                        ui.UserURLReferrer = CookieHelper.GetValue(CookieName.UrlReferrer);
                        ui.UserCampaign = Service.Resolve<ICampaignService>().CampaignCode;

                        ui.SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.None;

                        ui.UserSettings.UserRegistrationInfo.IPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                        ui.UserSettings.UserRegistrationInfo.Agent = HttpContext.Current.Request.UserAgent;

                        // Check whether confirmation is required
                        bool requiresConfirmation = false;  //SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationEmailConfirmation");
                        bool requiresAdminApprove = false;
                        string startingAliasPath = null;

                        if (!requiresConfirmation)
                        {
                            // If confirmation is not required check whether administration approval is required
                            requiresAdminApprove = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationAdministratorApproval");
                            if (requiresAdminApprove)
                            {
                                ui.Enabled = false;
                                ui.UserSettings.UserWaitingForApproval = true;
                            }
                        }
                        else
                        {
                            // EnableUserAfterRegistration is overridden by requiresConfirmation - user needs to be confirmed before enable
                            ui.Enabled = false;
                        }

                        // Set user's starting alias path
                        if (!String.IsNullOrEmpty(startingAliasPath))
                        {
                            ui.UserStartingAliasPath = MacroResolver.ResolveCurrentPath(startingAliasPath);
                        }

                        #region "License limitations"

                        string errorMessage = String.Empty;
                        UserInfoProvider.CheckLicenseLimitation(ui, ref errorMessage);

                        if (!String.IsNullOrEmpty(errorMessage))
                        {
                            p_newAccountInfo.LoginInfo.LoginValid = false;
                            p_newAccountInfo.LoginInfo.ErrorMessage = errorMessage;
                            p_newAccountInfo.LoginInfo.LoginStatusCode = 999;
                            canContinue = false;
                        }

                        #endregion "License limitations"

                        if (canContinue)
                        {
                            // Set password
                            UserInfoProvider.SetPassword(ui, p_registrationinfo.LoginInfo.Password);

                            // Assign the user to the site.
                            UserInfoProvider.AddUserToSite(ui.UserName, si.SiteName);

                            // assign user to site and role (we need to be able to pass in this?)
                            string mobileUser = "MOBLZMobileAppUser";
                            string occupant = "Occupant";
                            string member = "Member";
                            string msp = "MSP";
                            string PropertyManager = "PropertyManager";
                            string tenant = "Tenant";

                            if (!String.IsNullOrWhiteSpace(p_registrationinfo.LoginInfo.AssignRoles))
                            {
                                p_newAccountInfo.LoginInfo.AssignRoles = p_registrationinfo.LoginInfo.AssignRoles.Trim();
                            }
                            else
                            {
                                p_newAccountInfo.LoginInfo.AssignRoles = "MOBLZMobileAppUser;Occupant;Member";
                            }

                            string[] roleList = p_newAccountInfo.LoginInfo.AssignRoles.Split(';');
                            foreach (string role in roleList)
                            {
                                // add the role if it exists on the site.
                                if (RoleInfoProvider.RoleExists(role, si.SiteName))
                                {
                                    UserInfoProvider.AddUserToRole(ui.UserName, role, si.SiteName);
                                }
                            }

                            if (!String.IsNullOrWhiteSpace(p_registrationinfo.LoginInfo.MobileDeviceID))
                            {
                                p_newAccountInfo.LoginInfo.MobileDeviceID = p_registrationinfo.LoginInfo.MobileDeviceID;
                                string moblzdeviceidColumn = "UserMOBLZAppID";
                                //object oldval = String.Empty;
                                //ui.TryGetValue(moblzdeviceidColumn, out oldval);
                                ui.SetValue(moblzdeviceidColumn, p_newAccountInfo.LoginInfo.MobileDeviceID);
                                ui.Update();
                            }
                        }
                    }

                    if (!canContinue)
                    {
                        // we must delete the user?
                        UserInfoProvider.DeleteUser(ui);
                        // error already set.
                        //p_newAccountInfo.LoginInfo.LoginValid = false;
                        //p_newAccountInfo.LoginInfo.ErrorMessage = "Unable to create user";
                        //p_newAccountInfo.LoginInfo.LoginStatusCode = 9996;
                    }

                    if (ui != null)
                    {
                        UserInfo info = UserInfoProvider.GetUserInfo(ui.UserName);
                        if (info != null)
                        {
                            p_newAccountInfo.LoginInfo.LoginValid = true;
                            p_newAccountInfo.LoginInfo.LoginStatusCode = 1;
                            p_newAccountInfo.LoginInfo.ErrorMessage = "Success";
                            p_newAccountInfo.LoginInfo.Token = info.UserGUID.ToString();
                            p_newAccountInfo.UserInfo.EmailAddress = info.Email;
                            p_newAccountInfo.UserInfo.FirstName = info.FirstName;
                            p_newAccountInfo.UserInfo.LastName = info.LastName;
                            p_newAccountInfo.UserInfo.UserID = info.UserID;
                        }

                        // success log it.
                        AnalyticsHelper.LogRegisteredUser(siteName, ui);
                    }
                }
            }
        }
        return p_newAccountInfo;
    }

    /// <summary>
    /// Get authenticated account info
    /// </summary>
    /// <param name="p_userName"></param>
    /// <param name="p_password"></param>
    /// <returns></returns>
    public static MOBLZAuthenticationInfo GetAuthenticatedAccount(string p_userName, string p_password)
    {
        MOBLZAuthenticationInfo data = new MOBLZAuthenticationInfo();
        if (Login(p_userName, p_password))
        {
            UserInfo info = UserInfoProvider.GetUserInfo(p_userName);
            if (info != null)
            {
                data.LoginInfo.LoginValid = true;
                data.LoginInfo.Token = info.UserGUID.ToString();
                data.UserInfo.EmailAddress = info.Email;
                data.UserInfo.FirstName = info.FirstName;
                data.UserInfo.LastName = info.LastName;
                data.UserInfo.UserID = info.UserID;
            }
        }
        return data;
    }
}