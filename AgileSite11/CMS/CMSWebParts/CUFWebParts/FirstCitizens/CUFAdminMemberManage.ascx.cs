using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;


namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
{
    public partial class CUFAdminMemberManage : CMSAbstractWebPart
    {
        #region "Variables"

        private int mSiteID = -1;
        private int mSelectedSiteID = -2;
        private string mSiteName = null;
        protected CurrentUserInfo mCurrentAdminUser = null;
        protected int userId = 0;
        protected string password;
        protected string myCulture = string.Empty;
        protected string myUICulture = string.Empty;
        private UserInfo ui = null;

        #endregion "Variables"

        #region "Properties"

        #region Properties

        /// <summary>
        /// Get the current site site ID
        /// </summary>
        protected static int CurrentSiteID
        {
            get
            {
                int currentSiteID = SiteContext.CurrentSiteID;
                if (currentSiteID <= 0)
                {
                    SiteInfo test = SiteInfoProvider.GetRunningSiteInfo(RequestContext.CurrentDomain, "");
                    if (test == null)
                    {
                        //test
                        test = SiteInfoProvider.GetRunningSiteInfo("firstcitizens.cufs.ccesvc.com", "");
                    }
                    currentSiteID = test.SiteID;
                }

                return currentSiteID;
            }
        }

        #endregion Properties

        /// <summary>
        /// Returns the site name for current user, based on SiteID.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return mSiteName ?? (mSiteName = SiteInfoProvider.GetSiteName(SiteID));
            }
        }

        /// <summary>
        /// Returns correct site id for current user.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                if (mSiteID == -1)
                {
                    mSiteID = GetSiteID(QueryHelper.GetString("siteid", string.Empty));
                }

                return mSiteID;
            }
            set
            {
                mSiteID = value;
            }
        }

        /// <summary>
        /// Returns correct selected site id for current user.
        /// </summary>
        public virtual int SelectedSiteID
        {
            get
            {
                if (mSelectedSiteID == -2)
                {
                    mSelectedSiteID = GetSiteID(QueryHelper.GetString("selectedsiteid", string.Empty));
                }

                return mSelectedSiteID;
            }
            set
            {
                mSelectedSiteID = value;
            }
        }

        /// <summary>
        /// Current user
        /// </summary>
        public CurrentUserInfo CurrentAdminUser
        {
            get
            {
                // Get the user form context
                return mCurrentAdminUser ?? (mCurrentAdminUser = MembershipContext.AuthenticatedUser);
            }
            set
            {
                mCurrentAdminUser = value;
            }
        }

        /// <summary>
        /// Hide the cancel button?
        /// </summary>
        protected bool HideCancelButton
        {
            get
            {
                return GetBoolObjectValue("HideCancelButton", true);
            }
        }

        /// <summary>
        /// Redirect after update URL
        /// </summary>
        protected string RedirectAfterUpdateURL
        {
            get
            {
                return GetStringObjectValue("RedirectAfterUpdateURL", String.Empty);
                //return ValidationHelper.GetString(this.GetValue("RedirectPageURL"), "/Secure/Member/Statements");
            }
            set
            {
                this.SetValue("RedirectAfterUpdateURL", value);
            }
        }

        /// <summary>
        /// Redirect after cancel URL
        /// </summary>
        protected string RedirectAfterCancelURL
        {
            get
            {
                return GetStringObjectValue("RedirectAfterCancelURL", String.Empty);
            }
        }

        /// <summary>
        /// Create user if login does not exists?
        /// </summary>
        protected bool CreateNonExistingLogin
        {
            get
            {
                return GetBoolObjectValue("CreateNonExistingLogin", false);
            }
        }

        /// <summary>
        /// Gets or sets the sender e-mail (from).
        /// </summary>
        public string SendEmailFrom
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("SendEmailFrom"), SettingsKeyInfoProvider.GetValue("CMSSendPasswordEmailsFrom", SiteContext.CurrentSiteID));
            }
            set
            {
                SetValue("SendEmailFrom", value);
            }
        }

        /// <summary>
        /// Gets or sets reset password url - this url is sent to user in e-mail when he wants to reset his password.
        /// </summary>
        public string ResetPasswordURL
        {
            get
            {
                string value = GetStringObjectValue("ResetPasswordURL", String.Empty);
                if (!String.IsNullOrWhiteSpace(value))
                {
                    return URLHelper.GetAbsoluteUrl(value);
                }
                else
                {
                    return AuthenticationHelper.GetResetPasswordUrl(SiteContext.CurrentSiteName);
                }
            }
            set
            {
                SetValue("ResetPasswordURL", value);
            }
        }

        /// <summary>
        /// The CUF member ID
        /// </summary>
        public int CUFMemberIDToManage
        {
            get;
            set;
        }

        #endregion "Properties"

        #region "Page events"

        /// <summary>
        /// OnLoad override (show hide password retrieval).
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            bool createUser = CreateNonExistingLogin;
            CUFMemberIDToManage = QueryHelper.GetInteger("objectid", 0);

            StringBuilder sb = new StringBuilder();

            //get user to replicate
            UserInfo userInfo = null;
            ObjectQuery<UserSettingsInfo> userSettingsSet = UserSettingsInfoProvider.GetUserSettings()
                .WhereEquals("CUMemberID", CUFMemberIDToManage);
            if (userSettingsSet != null && userSettingsSet.Count<UserSettingsInfo>() > 0)
            {
                if (userSettingsSet.Count<UserSettingsInfo>() > 1)
                {
                    //mulitple accounts with user id
                    sb.Append("Error: mulitple accounts found matching account number<br />\n");
                }
                else
                {
                    userId = userSettingsSet.First<UserSettingsInfo>().UserSettingsUserID;
                    userInfo = UserInfoProvider.GetUserInfo(userId);
                }
            }

            if (userInfo == null && sb.Length == 0)
            {
                if (createUser)
                {
                    #region user info does not exist, create a new user ?

                    //find member in statement database
                    string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", CurrentSiteID);
                    QueryDataParameters qdp = new QueryDataParameters();
                    qdp.Add("MemberID", CUFMemberIDToManage);
                    DataSet ds = ConnectionHelper.ExecuteQuery("SELECT * FROM " + statementDB + ".dbo.Member where MemberID = @MemberID", qdp, QueryTypeEnum.SQLQuery);
                    //string sql = string.Format("SELECT * FROM {0}.dbo.Member where MemberID = {1}\n",CUDBName, MemberIDToReplicate);
                    //sb.Append(sql);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];

                        String memberNumber = Convert.ToString(dr["MemberNumber"]);

                        userInfo = new UserInfo();
                        userInfo.UserName = "CU_" + memberNumber; //userName;
                        userInfo.FullName = Convert.ToString(dr["Name"]);
                        userInfo.Enabled = true;
                        userInfo.Email = "";
                        UserInfoProvider.SetUserInfo(userInfo);
                        UserInfoProvider.SetPassword(userInfo, "CU_PWD_ZGPJ_" + userInfo.UserName);

                        UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userInfo.UserID);
                        userSettings.SetValue("CUMemberNumber", memberNumber);
                        userSettings.SetValue("CUMemberID", CUFMemberIDToManage);
                        UserSettingsInfoProvider.SetUserSettingsInfo(userSettings);

                        UserInfoProvider.AddUserToSite(userInfo.UserName, SiteContext.CurrentSiteName);
                    }
                    else
                    {
                        sb.Append("Error: Unable to find member information in order to create local account access<br />\n");
                        //.Append("SELECT * FROM " + CUDBName + ".dbo.Member where MemberNumber = ").Append(MemberNumberToReplicate);
                    }

                    #endregion user info does not exist, create a new user ?
                }
            }

            // Get user info object and check if UI should be displayed
            ui = userInfo;
            if (!CheckEditUser(ui))
            {
                // stop here
                return;
            }

            EditedObject = ui;

            ucUserName.UseDefaultValidationGroup = false;
            cultureSelector.DisplayAllCultures = true;
            lblResetToken.Text = GetString("mfauthentication.token.reset");

            // Register picture delete script
            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "PictDelConfirm",
                                                   ScriptHelper.GetScript("function DeleteConfirmation(){ return confirm(" + ScriptHelper.GetString(GetString("MyProfile.PictDeleteConfirm")) + ");}"));

            // Check that only global administrator can edit global administrator's accounts
            //if (!CheckGlobalAdminEdit(ui))
            //{
            //    plcTable.Visible = false;
            //    ShowError(GetString("Administration-User_List.ErrorGlobalAdmin"));
            //}

            if (!RequestHelper.IsPostBack())
            {
                LoadData();
            }

            // Set hide action if user extend validity of his own account
            if (ui.UserID == CurrentAdminUser.UserID)
            {
                btnExtendValidity.OnClientClick = "window.top.HideWarning()";
            }

            // Register help variable for user is external confirmation
            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "IsExternal", ScriptHelper.GetScript("var isExternal = " + chkIsExternal.Checked.ToString().ToLower() + ";"));

            // Javascript code for "Is external user" confirmation
            string javascript = ScriptHelper.GetScript(
                @"function CheckExternal() {
                            var checkbox = document.getElementById('" + chkIsExternal.ClientID + @"')
                            if(checkbox.checked && !isExternal) {
                                if(!confirm('" + GetString("user.confirmexternal") + @"')) {
                                    checkbox.checked = false ;
                                }
                            }}");

            // Register script to the page
            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ClientID + "CheckExternal", javascript);

            // Assign to ok button
            if (!chkIsExternal.Checked)
            {
                btnOk.OnClientClick = "CheckExternal()";
            }

            // Display impersonation link if current user is global administrator and edited user is not global admin
            if (CurrentAdminUser.IsGlobalAdministrator && (ui.UserID != CurrentAdminUser.UserID) && !ui.IsPublic())
            {
                string message = GetImpersonalMessage(ui);

                //HeaderAction action = new HeaderAction();
                //action.Text = GetString("Membership.Impersonate");
                //action.Tooltip = GetString("Membership.Impersonate");
                //action.OnClientClick = "if (!confirm('" + message + "')) { return false; }";
                //action.CommandName = "impersonate";

                //CurrentMaster.HeaderActions.AddAction(action);
                //CurrentMaster.HeaderActions.ActionPerformed += HeaderActions_ActionPerformed;
            }
        }

        /// <summary>
        /// Page pre render event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (ui != null)
            {
                // Reset flag
                CheckBoxEnabled.Enabled = true;

                // Show warning message
                if (!ui.Enabled)
                {
                    string description = null;
                    if (ui.UserSettings.UserWaitingForApproval)
                    {
                        description = GetString("Administration-User_List.AccountLocked.WaitingForApproval");
                    }
                    else
                    {
                        switch (UserAccountLockCode.ToEnum(ui.UserAccountLockReason))
                        {
                            case UserAccountLockEnum.MaximumInvalidLogonAttemptsReached:
                                description = GetString("Administration-User_List.AccountLocked.MaximumInvalidPasswordAttempts");
                                CheckBoxEnabled.Enabled = false;
                                break;

                            case UserAccountLockEnum.PasswordExpired:
                                description = GetString("Administration-User_List.AccountLocked.PasswordExpired");
                                CheckBoxEnabled.Enabled = false;
                                break;

                            case UserAccountLockEnum.DisabledManually:
                                description = GetString("Administration-User_List.AccountLocked.Disabledmanually");
                                break;
                        }
                    }
                    SetMessage(description);
                }

                // Check "modify" permission
                if (!CurrentAdminUser.IsAuthorizedPerResource("CMS.Users", "Modify"))
                {
                    btnExtendValidity.Enabled = btnResetLogonAttempts.Enabled = false;
                    btnResetToken.Enabled = false;
                }

                // Display impersonation link if current user is global administrator
                //if (CurrentMaster.HeaderActions.ActionsList != null)
                //{
                //    var impersonateAction = CurrentMaster.HeaderActions.ActionsList.Find(a => a.CommandName == "impersonate");

                //    if (impersonateAction != null)
                //    {
                //        if (CurrentUser.IsGlobalAdministrator && (ui != null) && (ui.UserID != CurrentUser.UserID) && !ui.IsPublic() && (!ui.IsGlobalAdministrator))
                //        {
                //            string message = GetImpersonalMessage(ui);
                //            impersonateAction.OnClientClick = "if (!confirm('" + message + "')) { return false; }";
                //        }
                //        else
                //        {
                //            impersonateAction.Visible = false;
                //        }
                //    }
                //}
            }
        }

        /// <summary>
        /// Users actions.
        /// </summary>
        private void HeaderActions_ActionPerformed(object sender, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "impersonate":
                    // Use user impersonate
                    UserInfo ui = UserInfoProvider.GetUserInfo(userId);
                    AuthenticationHelper.ImpersonateUser(ui);
                    break;

                case ComponentEvents.SAVE:
                    btnOk_Click(sender, EventArgs.Empty);
                    break;
            }
        }

        #endregion "Page events"

        #region general events

        /// <summary>
        /// Saves data of edited user from TextBoxes into DB.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnOk_Click(object sender, EventArgs e)
        {
            UserPrivilegeLevelEnum privilegeLevel = (UserPrivilegeLevelEnum)Convert.ToInt32(drpPrivilege.Value);

            // Check "modify" permission
            if (!CurrentAdminUser.IsAuthorizedPerResource("CMS.Users", "Modify"))
            {
                //RedirectToAccessDenied("CMS.Users", "Modify");
            }

            string result = ValidateGlobalAndDeskAdmin(userId);

            // Find whether user name is valid
            if (result == String.Empty)
            {
                if (!ucUserName.IsValid())
                {
                    result = ucUserName.ValidationError;
                }
            }

            String userName = ValidationHelper.GetString(ucUserName.Value, String.Empty);
            if (result == String.Empty)
            {
                // Finds whether required fields are not empty
                result = new Validator().NotEmpty(txtFullName.Text, GetString("Administration-User_New.RequiresFullName")).Result;
            }

            // Store the old display name
            var oldDisplayName = ui.Generalized.ObjectDisplayName;

            string oldUserName = ui.UserName;
            string oldEmail = ui.Email;
            bool oldEnabled = ui.Enabled;

            if ((result == String.Empty) && (ui != null))
            {
                // If site prefixed allowed - ad site prefix
                if ((SiteID != 0) && UserInfoProvider.UserNameSitePrefixEnabled(SiteContext.CurrentSiteName))
                {
                    if (!UserInfoProvider.IsSitePrefixedUser(userName))
                    {
                        userName = UserInfoProvider.EnsureSitePrefixUserName(userName, SiteContext.CurrentSite);
                    }
                }

                // Validation for site prefixed users
                if (!UserInfoProvider.IsUserNamePrefixUnique(userName, ui.UserID))
                {
                    SetErrorMessage(GetString("Administration-User_New.siteprefixeduserexists"));
                    return;
                }

                // Ensure same password
                password = ui.GetValue("UserPassword").ToString();

                // Test for unique username
                UserInfo uiTest = UserInfoProvider.GetUserInfo(userName);
                if ((uiTest == null) || (uiTest.UserID == userId))
                {
                    if (ui == null)
                    {
                        ui = new UserInfo();
                    }

                    bool globAdmin = ui.IsGlobalAdministrator;
                    bool editor = ui.IsEditorInternal;

                    // Email format validation
                    string email = txtEmail.Text.Trim();
                    if ((email != string.Empty) && (!ValidationHelper.IsEmail(email)))
                    {
                        SetErrorMessage(GetString("Administration-User_New.WrongEmailFormat"));
                        return;
                    }

                    bool oldGlobal = ui.IsGlobalAdministrator;
                    bool oldEditor = ui.IsEditorInternal;

                    // Define domain variable
                    string domains = null;

                    // Get all user sites
                    DataTable ds = UserInfoProvider.GetUserSites(userId, null, null, 0, "SiteDomainName");
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        foreach (DataRow dr in ds.Rows)
                        {
                            domains += ValidationHelper.GetString(dr["SiteDomainName"], string.Empty) + ";";
                        }

                        // Remove  ";" at the end
                        if (domains != null)
                        {
                            domains = domains.Remove(domains.Length - 1);
                        }
                    }
                    else
                    {
                        DataSet siteDs = SiteInfoProvider.GetSites().Columns("SiteDomainName");
                        if (!DataHelper.DataSourceIsEmpty(siteDs))
                        {
                            // Create list of available site domains
                            domains = TextHelper.Join(";", DataHelper.GetStringValues(siteDs.Tables[0], "SiteDomainName"));
                        }
                    }

                    // Check limitations for Global administrator
                    if (CurrentAdminUser.IsGlobalAdministrator && ((privilegeLevel == UserPrivilegeLevelEnum.GlobalAdmin) || (privilegeLevel == UserPrivilegeLevelEnum.Admin)) && !oldGlobal)
                    {
                        //if (!UserInfoProvider.LicenseVersionCheck(domains, FeatureEnum.GlobalAdmininistrators, ObjectActionEnum.Insert, globAdmin))
                        //{
                        //    SetErrorMessage(GetString("License.MaxItemsReachedGlobal"));
                        //    return;
                        //}
                    }

                    // Check limitations for editors
                    if ((privilegeLevel == UserPrivilegeLevelEnum.Editor) && !oldEditor)
                    {
                        if (!UserInfoProvider.LicenseVersionCheck(domains, FeatureEnum.Editors, ObjectActionEnum.Insert, editor))
                        {
                            SetErrorMessage(GetString("License.MaxItemsReachedEditor"));
                            return;
                        }
                    }

                    // Check whether email is unique if it is required
                    if (!UserInfoProvider.IsEmailUnique(email, ui))
                    {
                        SetErrorMessage(GetString("UserInfo.EmailAlreadyExist"));
                        return;
                    }

                    // Set properties
                    ui.Email = email;
                    ui.FirstName = txtFirstName.Text.Trim();
                    ui.FullName = txtFullName.Text.Trim();
                    ui.LastName = txtLastName.Text.Trim();
                    ui.MiddleName = txtMiddleName.Text.Trim();
                    ui.UserName = userName;
                    ui.Enabled = CheckBoxEnabled.Checked;
                    ui.UserIsHidden = chkIsHidden.Checked;
                    ui.IsExternal = chkIsExternal.Checked;
                    ui.UserIsDomain = chkIsDomain.Checked;
                    ui.SetValue("UserPassword", password);
                    ui.UserID = userId;
                    ui.UserStartingAliasPath = txtUserStartingPath.Text.Trim();
                    ui.UserMFRequired = chkIsMFRequired.Checked;

                    // Global admin can set anything
                    if (CurrentAdminUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin)
                        // Other users can set only editor and non privileges
                        || ((privilegeLevel != UserPrivilegeLevelEnum.Admin) && (privilegeLevel != UserPrivilegeLevelEnum.GlobalAdmin))
                        // Admin can manage his own privilege
                        || ((privilegeLevel == UserPrivilegeLevelEnum.Admin) && (CurrentAdminUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && (CurrentAdminUser.UserID == ui.UserID))))
                    {
                        ui.SetPrivilegeLevel(privilegeLevel);
                    }

                    LoadUserLogon(ui);

                    // Set values of cultures.
                    string culture = ValidationHelper.GetString(cultureSelector.Value, "");
                    ui.PreferredCultureCode = culture;

                    if (lstUICulture.SelectedValue == "0")
                    {
                        ui.PreferredUICultureCode = "";
                    }
                    else
                    {
                        //// Set preferred UI culture
                        //CultureInfo ci = CultureInfoProvider.GetCultureInfo(ValidationHelper.GetInteger(lstUICulture.SelectedValue, 0));
                        //ui.PreferredUICultureCode = ci.CultureCode;
                    }

                    // Refresh page breadcrumbs if display name changed
                    if (ui.Generalized.ObjectDisplayName != oldDisplayName)
                    {
                        ScriptHelper.RefreshTabHeader(Page, ui.FullName);
                    }

                    //using (CMSActionContext context = new CMSActionContext())
                    //{
                    //    // Check whether the username of the currently logged user has been changed
                    //    if (CurrentUserChangedUserName())
                    //    {
                    //        // Ensure that an update search task will be created but NOT executed when updating the user
                    //        context.EnableSmartSearchIndexer = false;
                    //    }

                    //    // Update the user
                    //    UserInfoProvider.SetUserInfo(ui);

                    //    // Check whether the username of the currently logged user has been changed
                    //    if (CurrentUserChangedUserName())
                    //    {
                    //        // Ensure that current user is not logged out if he changes his user name
                    //        if (RequestHelper.IsFormsAuthentication())
                    //        {
                    //            FormsAuthentication.SetAuthCookie(ui.UserName, false);

                    //            // Update current user
                    //            MembershipContext.AuthenticatedUser = new CurrentUserInfo(ui, true);

                    //            // Reset current user
                    //            CurrentAdminUser = null;
                    //        }
                    //    }
                    //}

                    #region logging

                    if (oldUserName != ui.UserName)
                    {
                        LogCUMembershipChanges(204, SiteContext.CurrentSiteID, "UserName Changed", "UserName Changed", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
                    }

                    if (oldEmail != ui.Email)
                    {
                        LogCUMembershipChanges(204, SiteContext.CurrentSiteID, "Email Changed", "Email Changed", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
                    }

                    if (oldEnabled != ui.Enabled)
                    {
                        if (ui.Enabled)
                        {
                            LogCUMembershipChanges(205, SiteContext.CurrentSiteID, "Account Unlocked", "Account Unlocked", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
                        }
                        else
                        {
                            LogCUMembershipChanges(206, SiteContext.CurrentSiteID, "Account Locked", "Account Locked", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
                        }
                    }

                    #endregion logging

                    ShowChangesSaved1();
                }
                else
                {
                    // If user exists
                    SetErrorMessage(GetString("Administration-User_New.UserExists"));
                }
            }
            else
            {
                SetErrorMessage(result);
            }

            if ((ui.UserInvalidLogOnAttempts == 0) && (ui.UserAccountLockReason != UserAccountLockCode.FromEnum(UserAccountLockEnum.MaximumInvalidLogonAttemptsReached)))
            {
                btnResetLogonAttempts.Enabled = false;
            }

            LoadPasswordExpiration(ui);
        }

        /// <summary>
        /// Reset password button clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            string returnUrl = RequestContext.CurrentURL;
            bool success = false;
            //string message = AuthenticationHelper.ForgottenEmailRequest(ui.UserName, SiteContext.CurrentSiteName, "LOGONFORM", SendEmailFrom, MacroContext.CurrentResolver, ResetPasswordURL, out success, returnUrl);
            string message = String.Empty;
            if (success)
            {
                SetMessage(message);
                LogCUMembershipChanges(207, SiteContext.CurrentSiteID, "Password Reset", "Password Reset", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
            }
            else
            {
                SetErrorMessage(message);
            }
        }

        /// <summary>
        /// Reset security question answers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnResetSecurityAnswer_Click(object sender, EventArgs e)
        {
            string returnUrl = RequestContext.CurrentURL;
            bool success = false;

            string message = "Security Question Answers Reset";
            try
            {
                //CUFSecurityQuestionAnswers.ResetSecurityQuestionAnswer(ui);
                success = true;
            }
            catch (Exception ex)
            {
                message = "Unable to reset security QA:" + ex.Message;
                success = false;
            }

            if (success)
            {
                SetMessage(message);
                LogCUMembershipChanges(207, SiteContext.CurrentSiteID, "Security Question Answer Reset", "Security Question Answer Reset", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
            }
            else
            {
                SetErrorMessage(message);
            }
        }

        /// <summary>
        /// Button cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            RedirectToURL(RedirectAfterCancelURL);
        }

        #endregion general events

        #region "Methods"

        /// <summary>
        /// Returns the impersonalization message for current user.
        /// </summary>
        /// <param name="ui">User info</param>
        protected string GetImpersonalMessage(UserInfo ui)
        {
            string message = String.Empty;

            // Editor message
            if (ui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, CurrentSiteName))
            {
                message = GetString("Membership.ImperConfirmEditor");
            }
            // Default user message
            else
            {
                message = GetString("Membership.ImperConfirmDefault");
            }

            return message;
        }

        /// <summary>
        /// Loads data of edited user from DB into TextBoxes.
        /// </summary>
        protected void LoadData()
        {
            // Fill lstUICulture (loop over and localize them first)
            //DataSet uiCultures = CultureInfoProvider.GetUICultures(orderBy: "CultureName ASC");
            //LocalizeCultureNames(uiCultures);
            //lstUICulture.DataSource = uiCultures.Tables[0].DefaultView;
            //lstUICulture.DataTextField = "CultureName";
            //lstUICulture.DataValueField = "CultureID";
            //lstUICulture.DataBind();

            //lstUICulture.Items.Insert(0, GetString("Administration-User_Edit.Default"));
            //lstUICulture.Items[0].Value = "0";

            if (ui != null)
            {
                // Get user info properties
                txtEmail.Text = ui.Email;
                txtFirstName.Text = ui.FirstName;
                txtFullName.Text = ui.FullName;
                txtLastName.Text = ui.LastName;
                txtMiddleName.Text = ui.MiddleName;
                ucUserName.Value = ui.UserName;

                CheckBoxEnabled.Checked = ui.Enabled;
                chkIsExternal.Checked = ui.IsExternal;
                chkIsDomain.Checked = ui.UserIsDomain;
                chkIsHidden.Checked = ui.UserIsHidden;
                chkIsMFRequired.Checked = ui.UserMFRequired;

                // Privilege drop down check
                if (!CurrentAdminUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                {
                    // Disable for global admins
                    if (ui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                    {
                        drpPrivilege.Enabled = false;
                    }
                    else
                        // Only global admin can manage other admins.
                        if (ui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                        {
                            // Allow manage only for user himself
                            if (ui.UserID != CurrentAdminUser.UserID)
                            {
                                drpPrivilege.Enabled = false;
                            }
                            else
                            {
                                drpPrivilege.ExcludedValues = ((int)UserPrivilegeLevelEnum.GlobalAdmin).ToString();
                            }
                        }
                        else
                        {
                            drpPrivilege.ExcludedValues = (int)UserPrivilegeLevelEnum.GlobalAdmin + ";" + (int)UserPrivilegeLevelEnum.Admin;
                        }
                }

                if (ui.IsGlobalAdministrator)
                {
                    drpPrivilege.Value = ui.UserGlobalAccessDisabled ? (int)UserPrivilegeLevelEnum.Admin : (int)UserPrivilegeLevelEnum.GlobalAdmin;
                }
                else if (ui.IsEditorInternal)
                {
                    drpPrivilege.Value = (int)UserPrivilegeLevelEnum.Editor;
                }

                password = ui.GetValue("UserPassword").ToString();

                // Disable username textbox for public user
                //if (ui.IsPublic())
                //{
                //    ucUserName.Enabled = false;
                //}

                // always disabled username for now.
                ucUserName.Enabled = false;

                myCulture = ui.PreferredCultureCode;
                myUICulture = ui.PreferredUICultureCode;

                lblInvalidLogonAttemptsNumber.Text = string.Format(GetString("general.attempts"), ui.UserInvalidLogOnAttempts);
                if (ui.UserInvalidLogOnAttempts > 0)
                {
                    lblInvalidLogonAttemptsNumber.Style.Add(HtmlTextWriterStyle.Color, "Red");
                }
                else
                {
                    btnResetLogonAttempts.Enabled = (ui.UserAccountLockReason == UserAccountLockCode.FromEnum(UserAccountLockEnum.MaximumInvalidLogonAttemptsReached));
                }

                LoadPasswordExpiration(ui);

                txtUserStartingPath.Text = ui.UserStartingAliasPath;
            }

            // Set content culture
            cultureSelector.Value = myCulture;

            if (!string.IsNullOrEmpty(myUICulture))
            {
                // Set UI culture
                try
                {
                    //CultureInfo ciUI = CultureInfoProvider.GetCultureInfo(myUICulture);
                    //lstUICulture.SelectedIndex = lstUICulture.Items.IndexOf(lstUICulture.Items.FindByValue(ciUI.CultureID.ToString()));
                }
                catch
                {
                    lstUICulture.SelectedIndex = lstUICulture.Items.IndexOf(lstUICulture.Items.FindByValue("0"));
                }
            }
            else
            {
                lstUICulture.SelectedIndex = lstUICulture.Items.IndexOf(lstUICulture.Items.FindByValue("0"));
            }

            if (ui != null)
            {
                // If new user
                lblCreatedInfo.Text = ui.UserCreated.ToString();
                lblLastLogonTime.Text = ui.LastLogon.ToString();

                LoadUserLogon(ui);

                if (ui.UserCreated == DateTimeHelper.ZERO_TIME)
                {
                    lblCreatedInfo.Text = GetString("general.na");
                }

                if (ui.LastLogon == DateTimeHelper.ZERO_TIME)
                {
                    lblLastLogonTime.Text = GetString("general.na");
                }
            }
        }

        /// <summary>
        /// Displays user's last logon information.
        /// </summary>
        /// <param name="ui">User info</param>
        protected void LoadUserLogon(UserInfo ui)
        {
            if ((ui.UserLastLogonInfo != null) && (ui.UserLastLogonInfo.ColumnNames != null) && (ui.UserLastLogonInfo.ColumnNames.Count > 0))
            {
                foreach (string column in ui.UserLastLogonInfo.ColumnNames)
                {
                    // Create new control to display last logon information
                    Panel grp = new Panel
                    {
                        CssClass = "control-group-inline"
                    };
                    plcUserLastLogonInfo.Controls.Add(grp);
                    Label lbl = new Label();
                    grp.Controls.Add(lbl);
                    lbl.CssClass = "form-control-text";
                    lbl.Text = HTMLHelper.HTMLEncode(TextHelper.LimitLength((string)ui.UserLastLogonInfo[column], 80, "..."));
                    lbl.ToolTip = HTMLHelper.HTMLEncode(column + " - " + (string)ui.UserLastLogonInfo[column]);
                }
            }
            else
            {
                //plcUserLastLogonInfo.Controls.Add(new LocalizedLabel
                //{
                //    ResourceString = "general.na",
                //    CssClass = "form-control-text"
                //});
            }
        }

        /// <summary>
        /// Check whether current user is allowed to modify another user. Return "" or error message.
        /// </summary>
        /// <param name="userId">Modified user</param>
        protected string ValidateGlobalAndDeskAdmin(int userId)
        {
            string result = String.Empty;

            if (CurrentAdminUser.IsGlobalAdministrator)
            {
                // User is global admin
                return result;
            }

            UserInfo userInfo = UserInfoProvider.GetUserInfo(userId);
            if (userInfo == null)
            {
                result = GetString("Administration-User.WrongUserId");
            }
            else if (userInfo.IsGlobalAdministrator)
            {
                // Current user has lower permissions than given user
                result = GetString("Administration-User.NotAllowedToModify");
            }
            return result;
        }

        /// <summary>
        /// Localizes culture names and sorts them in ascending order.
        /// </summary>
        /// <param name="uiCultures">DataSet containing the UI cultures</param>
        private void LocalizeCultureNames(DataSet uiCultures)
        {
            // Localize all available UI cultures
            if (!DataHelper.DataSourceIsEmpty(uiCultures))
            {
                for (int i = 0; i < uiCultures.Tables[0].Rows.Count; i++)
                {
                    uiCultures.Tables[0].Rows[i]["CultureName"] = ResHelper.LocalizeString(uiCultures.Tables[0].Rows[i]["CultureName"].ToString());
                }
            }

            uiCultures.Tables[0].DefaultView.Sort = "CultureName ASC";
        }

        /// <summary>
        /// Load user password expiration
        /// </summary>
        /// <param name="ui">User info</param>
        protected void LoadPasswordExpiration(UserInfo ui)
        {
            int expDays = 0;

            lblExpireIn.Style.Clear();
            lblPassExpiration.Text = GetString("Administration-User_Edit_General.PasswordExpireIn");

            if (!AuthenticationHelper.IsPasswordExpirationEnabled(SiteContext.CurrentSiteName, out expDays))
            {
                lblExpireIn.Text = GetString("security.never");
                btnExtendValidity.Enabled = (ui.UserAccountLockReason == UserAccountLockCode.FromEnum(UserAccountLockEnum.PasswordExpired));
            }
            else
            {
                // Get password expiration, negative number means not expired, positive means expired, DateTime.Min means not expired but never changed password
                int dayDiff = (ui.UserPasswordLastChanged == DateTime.MinValue) ? -expDays : ((DateTime.Now - ui.UserPasswordLastChanged).Days - expDays);
                if (dayDiff >= 0)
                {
                    lblPassExpiration.Text = GetString("Administration-User_Edit_General.PasswordExpired");
                    lblExpireIn.Style.Add(HtmlTextWriterStyle.Color, "Red");
                }

                lblExpireIn.Text = string.Format(GetString("general.validity.days"), Math.Abs(dayDiff));
            }
        }

        /// <summary>
        /// Reset user account lock status
        /// </summary>
        protected void btnResetLogonAttempts_Click(object sender, EventArgs e)
        {
            // Check "modify" permission
            if (!CurrentAdminUser.IsAuthorizedPerResource("CMS.Users", "Modify"))
            {
                //RedirectToAccessDenied("CMS.Users", "Modify");
            }

            bool unlocked = false;

            if (ui.UserAccountLockReason == UserAccountLockCode.FromEnum(UserAccountLockEnum.MaximumInvalidLogonAttemptsReached))
            {
                AuthenticationHelper.UnlockUserAccount(ui);
                unlocked = true;
            }
            else
            {
                ui.UserInvalidLogOnAttempts = 0;
                UserInfoProvider.SetUserInfo(ui);
            }

            LoadData();
            lblInvalidLogonAttemptsNumber.Style.Clear();

            if (unlocked)
            {
                LogCUMembershipChanges(210, SiteContext.CurrentSiteID, "Invalid Logon Attempts Reset Unlock", "Invalid Logon Attempts Reset Unlock", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
                SetMessage(GetString("Administration-User.InvalidLogonAttemptsResetUnlock"));
            }
            else
            {
                LogCUMembershipChanges(211, SiteContext.CurrentSiteID, "Invalid Logon Attempts Reset", "Invalid Logon Attempts Reset", CurrentAdminUser.UserName, ui.UserName, CUFMemberIDToManage, null, null, null, DateTime.Now, null, null);
                SetMessage(GetString("Administration-User.InvalidLogonAttemptsReset"));
            }
        }

        /// <summary>
        /// Reset user account lock status
        /// </summary>
        protected void btnExtendValidity_Click(object sender, EventArgs e)
        {
            // Check "modify" permission
            if (!CurrentAdminUser.IsAuthorizedPerResource("CMS.Users", "Modify"))
            {
                //RedirectToAccessDenied("CMS.Users", "Modify");
            }

            bool unlocked = false;

            ui.UserPasswordLastChanged = DateTime.Now;
            if (ui.UserAccountLockReason == UserAccountLockCode.FromEnum(UserAccountLockEnum.PasswordExpired))
            {
                AuthenticationHelper.UnlockUserAccount(ui);
                unlocked = true;
            }
            else
            {
                UserInfoProvider.SetUserInfo(ui);
            }

            LoadData();

            if (unlocked)
            {
                SetMessage(GetString("Administration-User.ExtendPasswordUnlock"));
                //ShowConfirmation(GetString("Administration-User.ExtendPasswordUnlock"));
            }
            else
            {
                SetMessage(GetString("Administration-User.ExtendPassword"));
                // ShowConfirmation(GetString("Administration-User.ExtendPassword"));
            }
        }

        /// <summary>
        /// Reset token to initial state.
        /// </summary>
        protected void btnResetToken_Click(object sender, EventArgs e)
        {
            // Check "modify" permission
            if (!CurrentAdminUser.IsAuthorizedPerResource("CMS.Users", "Modify"))
            {
                //RedirectToAccessDenied("CMS.Users", "Modify");
            }

            MFAuthenticationHelper.ResetTokenAndIterationForUser(ui);
            LoadData();
            SetMessage(GetString("administration-user.token.reset"));
        }

        /// <summary>
        /// Checks if currently logged user changes his user name.
        /// </summary>
        private bool CurrentUserChangedUserName()
        {
            return (CurrentAdminUser != null) && (CurrentAdminUser.UserID == ui.UserID) && (CurrentAdminUser.UserName != ui.UserName);
        }

        /// <summary>
        /// Test if edited user belongs to current site
        /// </summary>
        /// <param name="ui">User info object</param>
        protected bool CheckEditUser(UserInfo ui)
        {
            bool isvalid = false;
            if (ui != null)
            {
                btnCancel.Visible = !HideCancelButton;
                if (!MembershipContext.AuthenticatedUser.IsGlobalAdministrator && !ui.IsInSite(SiteContext.CurrentSiteName))
                {
                    SetErrorMessage(GetString("user.notinsite"));
                    divEditMain.Visible = false;
                    btnOk.Visible = false;
                }
                else
                {
                    divEditMain.Visible = true;
                    btnOk.Visible = true;
                    isvalid = true;
                }
            }
            else
            {
                SetInfoMessage("This member can not be managed using this function: The customer has not registered for online access. Please click cancel to go back to the member search page.");
                divEditMain.Visible = false;
                btnOk.Visible = false;
                btnCancel.Visible = true;
            }
            return isvalid;
        }

        /// <summary>
        /// Returns false if edited user is global admin and current user can't edit admin's account.
        /// </summary>
        /// <param name="editedUser">Edited user info</param>
        protected bool CheckGlobalAdminEdit(UserInfo editedUser)
        {
            var currentUser = MembershipContext.AuthenticatedUser;
            if (!currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) && (editedUser != null) && editedUser.UserIsGlobalAdministrator && (editedUser.UserID != currentUser.UserID))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets valid site id for current user.
        /// </summary>
        /// <param name="queryStringSiteId">Site id from querystring</param>
        public int GetSiteID(string queryStringSiteId)
        {
            // Get site id from querystring
            int siteId = ValidationHelper.GetInteger(queryStringSiteId, Int32.MinValue);

            // Global administrator can edit everything
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                // There is site id in the querystring
                if (siteId != Int32.MinValue)
                {
                    return siteId;
                }
                return 0;
            }

            // Editor can edit only current site
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, SiteContext.CurrentSiteName))
            {
                if (SiteContext.CurrentSite != null)
                {
                    return SiteContext.CurrentSiteID;
                }
            }

            return -1;
        }

        #endregion "Methods"

        #region helpers

        /// <summary>
        /// Get string property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_defaultValue"></param>
        /// <returns></returns>
        protected string GetStringObjectValue(string p_key, string p_defaultValue)
        {
            string value = p_defaultValue;

            object obj = GetValue(p_key);

            if (obj != null)
            {
                value = obj.ToString();
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = p_defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Get Bool property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_defaultValue"></param>
        /// <returns></returns>
        protected bool GetBoolObjectValue(string p_key, bool p_defaultValue)
        {
            bool value = p_defaultValue;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!bool.TryParse(obj.ToString(), out value))
                {
                    value = p_defaultValue;
                }
            }
            return value;
        }

        /// <summary>
        /// Set error message
        /// </summary>
        /// <param name="p_message"></param>
        protected void SetErrorMessage(string p_message)
        {
            lblError.Text = p_message;
        }

        /// <summary>
        /// Set info message
        /// </summary>
        /// <param name="p_message"></param>
        protected void SetInfoMessage(string p_message)
        {
            lblInfoMessage.Text = p_message;
        }

        /// <summary>
        /// Shows the specified warning message, optionally with a tooltip text.
        /// </summary>
        /// <param name="p_message">Warning message text</param>
        protected void SetMessage(string p_message)
        {
            lblMessage.Text = p_message;
        }

        /// <summary>
        /// User account updated
        /// </summary>
        protected void ShowChangesSaved1()
        {
            SetMessage("User account updated");
        }

        /// <summary>
        /// Log changes
        /// </summary>
        /// <param name="p_LogTypeID"></param>
        /// <param name="SiteID"></param>
        /// <param name="p_shortDescription"></param>
        /// <param name="p_description"></param>
        /// <param name="p_userName"></param>
        /// <param name="p_updatedUser"></param>
        /// <param name="p_CUFMemberID"></param>
        /// <param name="p_IPAddress"></param>
        /// <param name="p_machineName"></param>
        /// <param name="p_urlReferrer"></param>
        /// <param name="p_eventDate"></param>
        /// <param name="p_message"></param>
        /// <param name="p_extraInfo"></param>
        private void LogCUMembershipChanges(int? p_logTypeID, int? p_siteID, string p_shortDescription, string p_description, string p_userName, string p_updatedUser, int? p_CUFMemberID, string p_IPAddress, string p_machineName, string p_urlReferrer, DateTime p_eventDate, string p_message, string p_extraInfo)
        {
            string message = String.Empty;
            //if (!LogCUMembershipChanges(p_logTypeID, p_siteID, p_shortDescription, p_description, p_userName, p_updatedUser, p_CUFMemberID, p_IPAddress, p_machineName, p_urlReferrer, p_eventDate, p_message, p_extraInfo, out message))
            //{
            //    lblError.Text = message;
            //}
        }

        #endregion helpers

        #region text helper

        /// <summary>
        /// Get string property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected string GetSafeStringValue(string p_key, string p_default)
        {
            string value = p_default;

            object obj = GetValue(p_key);

            if (obj != null)
            {
                value = obj.ToString();
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = p_default;
            }

            return value;
        }

        /// <summary>
        /// Get Bool property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected bool GetSafeBoolValue(string p_key, bool p_default)
        {
            bool value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!bool.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        /// <summary>
        /// Get int value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected int GetSafeIntValue(string p_key, int p_default)
        {
            int value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!int.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        #endregion text helper

        #region redirection

        /// <summary>
        /// Redirect to a page
        /// </summary>
        /// <param name="p_url"></param>
        protected void RedirectToURL(string p_url)
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                URLHelper.Redirect(ResolveUrl(p_url));
            }
        }

        #endregion redirection
    }
}