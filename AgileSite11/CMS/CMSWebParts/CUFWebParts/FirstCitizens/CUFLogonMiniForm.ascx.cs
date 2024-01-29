using System;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.MembershipProvider;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
{
    public partial class CUFLogonMiniForm : CMSAbstractWebPart, ICallbackEventHandler
    {
        #region "Local variables"

        private TextBox user = null;
        private TextBox pass = null;
        private LocalizedButton login = null;
        private LocalizedLabel lblUserName = null;
        private LocalizedLabel lblPassword = null;
        private ImageButton loginImg = null;
        private RequiredFieldValidator rfv = null;
        private Panel container = null;
        private string mDefaultTargetUrl = string.Empty;
        private string mUserNameText = String.Empty;
        private bool mShowUserNameLabel = false;
        private bool mShowPasswordLabel = false;

        #endregion "Local variables"

        #region "Private properties"

        /// <summary>
        /// Gets error displayed by login control
        /// </summary>
        private string DisplayedError
        {
            get
            {
                var failureLit = loginElem.FindControl("FailureText") as LocalizedLabel;
                if (failureLit != null)
                {
                    return failureLit.Text;
                }

                return null;
            }
        }

        #endregion "Private properties"

        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates if the username label should be displayed.
        /// </summary>
        public bool ShowUserNameLabel
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowUserNameLabel"), mShowUserNameLabel);
            }
            set
            {
                SetValue("ShowUserNameLabel", value);
                mShowUserNameLabel = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates if the password label should be displayed.
        /// </summary>
        public bool ShowPasswordLabel
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowPasswordLabel"), mShowPasswordLabel);
            }
            set
            {
                SetValue("ShowPasswordLabel", value);
                mShowPasswordLabel = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether image button is displayed instead of regular button.
        /// </summary>
        public bool ShowImageButton
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowImageButton"), false);
            }
            set
            {
                SetValue("ShowImageButton", value);
                login.Visible = !value;
                loginImg.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets an Image button URL.
        /// </summary>
        public string ImageUrl
        {
            get
            {
                return ResolveUrl(ValidationHelper.GetString(GetValue("ImageUrl"), loginImg.ImageUrl));
            }
            set
            {
                SetValue("ImageUrl", value);
                loginImg.ImageUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the logon failure text.
        /// </summary>
        public string FailureText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FailureText"), string.Empty);
            }
            set
            {
                if (!string.IsNullOrEmpty(value.Trim()))
                {
                    SetValue("FailureText", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the default target url (rediredction when the user is logged in).
        /// </summary>
        public string DefaultTargetUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DefaultTargetUrl"), mDefaultTargetUrl);
            }
            set
            {
                SetValue("DefaultTargetUrl", value);
                mDefaultTargetUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the username text.
        /// </summary>
        public string UserNameText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UserNameText"), mUserNameText);
            }
            set
            {
                if (value.Trim() != string.Empty)
                {
                    SetValue("UserNameText", value);
                    mUserNameText = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether show error as popup window.
        /// </summary>
        public bool ErrorAsPopup
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ErrorAsPopup"), false);
            }
            set
            {
                SetValue("ErrorAsPopup", value);
            }
        }

        /// <summary>
        /// Gets or sets whether make login persistent.
        /// </summary>
        public bool PersistentLogin
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("PersistentLogin"), false);
            }
            set
            {
                SetValue("PersistentLogin", value);
            }
        }

        /// <summary>
        /// URL to the security question page
        /// </summary>
        public string SecurityQuestionAnswerURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("SecurityQuestionAnswerURL"), "/Secure/Member/My-Profile/Security-Questions");
            }
            set
            {
                this.SetValue("SecurityQuestionAnswerURL", value);
            }
        }

        /// <summary>
        /// URL to the security question challenge page
        /// </summary>
        public string SecurityQuestionAnswerChallengeURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("SecurityQuestionAnswerChallengeURL"), "/Secure/Member/Security-Question");
            }
            set
            {
                this.SetValue("SecurityQuestionAnswerChallengeURL", value);
            }
        }

        /// <summary>
        /// Need redirect to security question challenge page
        /// </summary>
        public bool NeedSecurityQuestionChallenge
        {
            get
            {
                return ValidationHelper.GetBoolean(this.GetValue("NeedSecurityQuestionChallenge"), false);
            }
            set
            {
                this.SetValue("NeedSecurityQuestionChallenge", value);
            }
        }

        /// <summary>
        /// Need redirect to security question answer page after login
        /// </summary>
        public bool NeedSecurityQuestionAnswer
        {
            get
            {
                return ValidationHelper.GetBoolean(this.GetValue("NeedSecurityQuestionAnswer"), false);
            }
            set
            {
                this.SetValue("NeedSecurityQuestionAnswer", value);
            }
        }

        /// <summary>
        /// Does the site has security question and answer enabled
        /// </summary>
        public bool SecurityQuestionEnabled
        {
            get
            {
                return GetSafeBoolValue("SecurityQuestionEnabled", false);
            }
            set
            {
                this.SetValue("SecurityQuestionEnabled", value);
            }
        }

        #endregion "Public properties"

        #region "Overridden methods"

        /// <summary>
        /// Applies given stylesheet skin.
        /// </summary>
        public override void ApplyStyleSheetSkin(Page page)
        {
            SetSkinID(SkinID);
            base.ApplyStyleSheetSkin(page);
        }

        /// <summary>
        /// Content loaded event handler.
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();
        }

        /// <summary>
        /// Reloads data.
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();
            SetupControl();
        }

        /// <summary>
        /// Pre render event handler.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Hide webpart for non-public users
            Visible &= MembershipContext.AuthenticatedUser.IsPublic();
        }

        #endregion "Overridden methods"

        #region "SetupControl and SetSkinID"

        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (StopProcessing)
            {
                // Do nothing
            }
            else
            {
                // WAI validation
                lblUserName = (LocalizedLabel)loginElem.FindControl("lblUserName");
                if (lblUserName != null)
                {
                    lblUserName.Text = GetString("general.username");
                    if (!ShowUserNameLabel)
                    {
                        lblUserName.Attributes.Add("style", "display: none;");
                    }
                }
                lblPassword = (LocalizedLabel)loginElem.FindControl("lblPassword");
                if (lblPassword != null)
                {
                    lblPassword.Text = GetString("general.password");
                    if (!ShowPasswordLabel)
                    {
                        lblPassword.Attributes.Add("style", "display: none;");
                    }
                }

                // Set properties for validator
                rfv = (RequiredFieldValidator)loginElem.FindControl("rfvUserNameRequired");
                rfv.ToolTip = GetString("LogonForm.NameRequired");
                rfv.Text = rfv.ErrorMessage = GetString("LogonForm.EnterName");
                rfv.ValidationGroup = ClientID + "_MiniLogon";

                // Set visibility of buttons
                login = (LocalizedButton)loginElem.FindControl("btnLogon");
                if (login != null)
                {
                    login.Visible = !ShowImageButton;
                    login.ValidationGroup = ClientID + "_MiniLogon";
                }

                loginImg = (ImageButton)loginElem.FindControl("btnImageLogon");
                if (loginImg != null)
                {
                    loginImg.Visible = ShowImageButton;
                    loginImg.ImageUrl = ImageUrl;
                    loginImg.ValidationGroup = ClientID + "_MiniLogon";
                }

                // Ensure display control as inline and is used right default button
                container = (Panel)loginElem.FindControl("pnlLogonMiniForm");
                if (container != null)
                {
                    container.Attributes.Add("style", "display: inline;");
                    if (ShowImageButton)
                    {
                        if (loginImg != null)
                        {
                            container.DefaultButton = loginImg.ID;
                        }
                        else if (login != null)
                        {
                            container.DefaultButton = login.ID;
                        }
                    }
                }

                CMSTextBox txtUserName = (CMSTextBox)loginElem.FindControl("UserName");
                if (txtUserName != null)
                {
                    txtUserName.EnableAutoComplete = SecurityHelper.IsAutoCompleteEnabledForLogin(SiteContext.CurrentSiteName);
                }

                if (!string.IsNullOrEmpty(UserNameText))
                {
                    // Initialize javascript for focus and blur UserName textbox
                    user = (TextBox)loginElem.FindControl("UserName");
                    user.Attributes.Add("onfocus", "MLUserFocus_" + ClientID + "('focus');");
                    user.Attributes.Add("onblur", "MLUserFocus_" + ClientID + "('blur');");
                    string focusScript = "function MLUserFocus_" + ClientID + "(type)" +
                                         "{" +
                                         "var userNameBox = document.getElementById('" + user.ClientID + "');" +
                                         "if(userNameBox.value == '" + UserNameText + "' && type == 'focus')" +
                                         "{userNameBox.value = '';}" +
                                         "else if (userNameBox.value == '' && type == 'blur')" +
                                         "{userNameBox.value = '" + UserNameText + "';}" +
                                         "}";

                    ScriptHelper.RegisterClientScriptBlock(this, GetType(), "MLUserNameFocus_" + ClientID,
                                                           ScriptHelper.GetScript(focusScript));
                }
                loginElem.LoggedIn += loginElem_LoggedIn;
                loginElem.LoggingIn += loginElem_LoggingIn;
                loginElem.LoginError += loginElem_LoginError;
                loginElem.Authenticate += loginElem_Authenticate;

                if (!RequestHelper.IsPostBack())
                {
                    // Set SkinID properties
                    if (!StandAlone && (PageCycle < PageCycleEnum.Initialized) && (ValidationHelper.GetString(Page.StyleSheetTheme, string.Empty) == string.Empty))
                    {
                        SetSkinID(SkinID);
                    }
                }

                if (string.IsNullOrEmpty(loginElem.UserName))
                {
                    loginElem.UserName = UserNameText;
                }

                // Register script to update logon error message
                LocalizedLabel failureLit = loginElem.FindControl("FailureText") as LocalizedLabel;
                if (failureLit != null)
                {
                    StringBuilder sbScript = new StringBuilder();
                    sbScript.Append(@"
function UpdateLabel_", ClientID, @"(content, context) {
    var lbl = document.getElementById(context);
    if(lbl)
    {
        lbl.innerHTML = content;
        lbl.className = ""InfoLabel"";
    }
}");
                    ScriptHelper.RegisterClientScriptBlock(this, GetType(), "InvalidLogonAttempts_" + ClientID, sbScript.ToString(), true);
                }
            }
        }

        /// <summary>
        /// Displays error.
        /// </summary>
        /// <param name="msg">Message.</param>
        private void DisplayError(string msg)
        {
            var failureLit = loginElem.FindControl("FailureText") as LocalizedLabel;

            if (failureLit != null)
            {
                failureLit.Text = msg;
                failureLit.Visible = !string.IsNullOrEmpty(msg);
            }
        }

        /// <summary>
        /// Hides displayed error.
        /// </summary>
        private void HideError()
        {
            DisplayError("");
        }

        /// <summary>
        /// Sets SkinId to all controls in logon form.
        /// </summary>
        private void SetSkinID(string skinId)
        {
            if (skinId != string.Empty)
            {
                loginElem.SkinID = skinId;

                user = (TextBox)loginElem.FindControl("UserName");
                if (user != null)
                {
                    user.SkinID = skinId;
                }

                pass = (TextBox)loginElem.FindControl("Password");
                if (pass != null)
                {
                    pass.SkinID = skinId;
                }

                login = (LocalizedButton)loginElem.FindControl("btnLogon");
                if (login != null)
                {
                    login.SkinID = skinId;
                }

                loginImg = (ImageButton)loginElem.FindControl("btnImageLogon");
                if (loginImg != null)
                {
                    loginImg.SkinID = skinId;
                }
            }
        }

        #endregion "SetupControl and SetSkinID"

        #region "Loggin handlers"

        /// <summary>
        /// Logged in handler.
        /// </summary>
        private void loginElem_LoggedIn(object sender, EventArgs e)
        {
            // Set view mode to live site after login to prevent bar with "Close preview mode"
            PortalContext.ViewMode = ViewModeEnum.LiveSite;

            // Ensure response cookie
            CookieHelper.EnsureResponseCookie(FormsAuthentication.FormsCookieName);

            // Set cookie expiration
            if (loginElem.RememberMeSet)
            {
                CookieHelper.ChangeCookieExpiration(FormsAuthentication.FormsCookieName, DateTime.Now.AddYears(1), false);
            }
            else
            {
                // Extend the expiration of the authentication cookie if required
                if (!AuthenticationHelper.UseSessionCookies && (HttpContext.Current != null) && (HttpContext.Current.Session != null))
                {
                    CookieHelper.ChangeCookieExpiration(FormsAuthentication.FormsCookieName, DateTime.Now.AddMinutes(Session.Timeout), false);
                }
            }

            // Current username
            string userName = loginElem.UserName;

            // Get user name (test site prefix too)
            UserInfo ui = UserInfoProvider.GetUserInfoForSitePrefix(userName, SiteContext.CurrentSite);

            // Check whether safe user name is required and if so get safe username
            if (RequestHelper.IsMixedAuthentication() && UserInfoProvider.UseSafeUserName)
            {
                // User stored with safe name
                userName = ValidationHelper.GetSafeUserName(loginElem.UserName, SiteContext.CurrentSiteName);

                // Find user by safe name
                ui = UserInfoProvider.GetUserInfoForSitePrefix(userName, SiteContext.CurrentSite);
                if (ui != null)
                {
                    // Authenticate user by site or global safe username
                    AuthenticationHelper.AuthenticateUser(ui.UserName, loginElem.RememberMeSet);
                }
            }

            // Log activity (warning: CMSContext contains info of previous user)
            if (ui != null)
            {
                // If user name is site prefixed, authenticate user manually
                if (UserInfoProvider.IsSitePrefixedUser(ui.UserName))
                {
                    AuthenticationHelper.AuthenticateUser(ui.UserName, loginElem.RememberMeSet);
                }

                // Log activity
                int contactID = ModuleCommands.OnlineMarketingGetUserLoginContactID(ui);
                Activity activityLogin = new ActivityUserLogin(contactID, ui, DocumentContext.CurrentDocument, AnalyticsContext.ActivityEnvironmentVariables);
                activityLogin.Log();
            }

            if (NeedSecurityQuestionAnswer && SecurityQuestionEnabled)
            {
                string redirectUrl = URLHelper.AddParameterToUrl(SecurityQuestionAnswerChallengeURL, "ReturnURL", QueryHelper.GetString("ReturnURL", string.Empty));
                RedirectToURL(redirectUrl);
            }
            else if (NeedSecurityQuestionChallenge && SecurityQuestionEnabled)
            {
                RedirectToURL(SecurityQuestionAnswerURL);
            }
            else
            {
                // Redirect user to the return url, or if is not defined redirect to the default target url
                string url = QueryHelper.GetString("ReturnURL", string.Empty);
                if (!string.IsNullOrEmpty(url))
                {
                    if (url.StartsWithCSafe("~") || url.StartsWithCSafe("/") || QueryHelper.ValidateHash("hash"))
                    {
                        URLHelper.Redirect(ResolveUrl(QueryHelper.GetString("ReturnURL", string.Empty)));
                    }
                    else
                    {
                        URLHelper.Redirect(ResolveUrl("~/CMSMessages/Error.aspx?title=" + ResHelper.GetString("general.badhashtitle") + "&text=" + ResHelper.GetString("general.badhashtext")));
                    }
                }
                else
                {
                    if (DefaultTargetUrl != "")
                    {
                        URLHelper.Redirect(ResolveUrl(DefaultTargetUrl));
                    }
                    else
                    {
                        URLHelper.Redirect(RequestContext.CurrentURL);
                    }
                }
            }
        }

        /// <summary>
        /// Logging in handler.
        /// </summary>
        private void loginElem_LoggingIn(object sender, LoginCancelEventArgs e)
        {
            loginElem.RememberMeSet = PersistentLogin;
        }

        /// <summary>
        /// Handling login authenticate event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Authenticate event arguments.</param>
        private void loginElem_Authenticate(object sender, AuthenticateEventArgs e)
        {
            // Get the user
            SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteContext.CurrentSiteName);
            UserInfo user = UserInfoProvider.GetUserInfoForSitePrefix(loginElem.UserName, si);

            // check security question
            if (user != null)
            {
                //CUFSecurityQuestionAnswers sqa = new CUFSecurityQuestionAnswers(user);
                //if (sqa.HasMinimumAnswers)
                //{
                //    NeedSecurityQuestionChallenge = sqa.NeedSecurityQuestionChallenge;
                //}
                //else
                //{
                //    NeedSecurityQuestionAnswer = true;
                //}
            }

            if (MFAuthenticationHelper.IsMultiFactorRequiredForUser(loginElem.UserName))
            {
                var plcPasscodeBox = loginElem.FindControl("plcPasscodeBox");
                var plcLoginInputs = loginElem.FindControl("plcLoginInputs");
                var txtPasscode = loginElem.FindControl("txtPasscode") as CMSTextBox;

                if (txtPasscode == null)
                {
                    return;
                }
                if (plcPasscodeBox == null)
                {
                    return;
                }
                if (plcLoginInputs == null)
                {
                    return;
                }

                // Handle passcode
                string passcode = txtPasscode.Text;
                txtPasscode.Text = string.Empty;

                var provider = new CMSMembershipProvider();

                // Validate username and password
                if (plcLoginInputs.Visible)
                {
                    if (provider.MFValidateCredentials(loginElem.UserName, loginElem.Password))
                    {
                        // Show passcode screen
                        plcLoginInputs.Visible = false;
                        plcPasscodeBox.Visible = true;
                    }
                }
                // Validate passcode
                else
                {
                    if (provider.MFValidatePasscode(loginElem.UserName, passcode))
                    {
                        e.Authenticated = true;
                    }
                }
            }
            else
            {
                e.Authenticated = Membership.Provider.ValidateUser(loginElem.UserName, loginElem.Password);
            }
        }

        /// <summary>
        /// Login error handler.
        /// </summary>
        protected void loginElem_LoginError(object sender, EventArgs e)
        {
            bool showError = true;

            // Ban IP addresses which are blocked for login
            if (MembershipContext.UserIsBanned)
            {
                DisplayError(GetString("banip.ipisbannedlogin"));
            }
            // Check if account locked due to reaching maximum invalid logon attempts
            else if (AuthenticationHelper.DisplayAccountLockInformation(SiteContext.CurrentSiteName) && MembershipContext.UserAccountLockedDueToInvalidLogonAttempts)
            {
                string msg = GetString("invalidlogonattempts.unlockaccount.accountlocked");

                if (!ErrorAsPopup)
                {
                    msg += " " + string.Format(GetString("invalidlogonattempts.unlockaccount.accountlockedlink"), GetLogonAttemptsUnlockingLink());
                }
                DisplayError(msg);
            }
            // Check if account locked due to password expiration
            else if (AuthenticationHelper.DisplayAccountLockInformation(SiteContext.CurrentSiteName) && MembershipContext.UserAccountLockedDueToPasswordExpiration)
            {
                string msg = GetString("passwordexpiration.accountlocked");

                if (!ErrorAsPopup)
                {
                    msg += " " + string.Format(GetString("invalidlogonattempts.unlockaccount.accountlockedlink"), GetLogonAttemptsUnlockingLink());
                }
                DisplayError(msg);
            }
            else if (MembershipContext.UserIsPartiallyAuthenticated && !MembershipContext.UserAuthenticationFailedDueToInvalidPasscode)
            {
                if (MembershipContext.MFAuthenticationTokenNotInitialized && MFAuthenticationHelper.DisplayTokenID)
                {
                    var plcTokenInfo = loginElem.FindControl("plcTokenInfo");
                    var lblTokenID = loginElem.FindControl("lblTokenID") as LocalizedLabel;

                    if (lblTokenID != null)
                    {
                        lblTokenID.Text = string.Format("{0} {1}", GetString("mfauthentication.label.token"), MFAuthenticationHelper.GetTokenIDForUser(loginElem.UserName));
                    }

                    if (plcTokenInfo != null)
                    {
                        plcTokenInfo.Visible = true;
                    }
                }

                if (string.IsNullOrEmpty(DisplayedError))
                {
                    HideError();
                }

                showError = false;
            }
            else if (!MembershipContext.UserIsPartiallyAuthenticated)
            {
                // Show login and password screen
                var plcPasscodeBox = loginElem.FindControl("plcPasscodeBox");
                var plcLoginInputs = loginElem.FindControl("plcLoginInputs");
                var plcTokenInfo = loginElem.FindControl("plcTokenInfo");
                if (plcLoginInputs != null)
                {
                    plcLoginInputs.Visible = true;
                }
                if (plcPasscodeBox != null)
                {
                    plcPasscodeBox.Visible = false;
                }
                if (plcTokenInfo != null)
                {
                    plcTokenInfo.Visible = false;
                }
            }

            if (showError && string.IsNullOrEmpty(DisplayedError))
            {
                DisplayError(DataHelper.GetNotEmpty(FailureText, GetString("Login_FailureText")));
            }

            // Display the failure message in a client-side alert box
            if (ErrorAsPopup)
            {
                if (string.IsNullOrEmpty(DisplayedError))
                {
                    return;
                }
                ScriptHelper.RegisterStartupScript(this, GetType(), "LoginError", ScriptHelper.GetScript("alert(" + ScriptHelper.GetString(HTMLHelper.StripTags(DisplayedError)) + ");"));

                // Hide error message
                HideError();
            }
        }

        /// <summary>
        /// Return link for unlocking logon attempts.
        /// </summary>
        private string GetLogonAttemptsUnlockingLink()
        {
            var failureLit = loginElem.FindControl("FailureText") as LocalizedLabel;
            if (failureLit != null)
            {
                return "<a href=\"#\" onclick=\"" + Page.ClientScript.GetCallbackEventReference(this, "null", "UpdateLabel_" + ClientID, "'" + failureLit.ClientID + "'") + ";\">" + GetString("general.clickhere") + "</a>";
            }
            return string.Empty;
        }

        #endregion "Loggin handlers"

        #region "ICallbackEventHandler Members"

        public string GetCallbackResult()
        {
            string result = "";
            bool outParam = true;
            UserInfo ui = UserInfoProvider.GetUserInfo(loginElem.UserName);
            if (ui != null)
            {
                string siteName = SiteContext.CurrentSiteName;

                // Prepare return URL
                string returnUrl = RequestContext.CurrentURL;
                if (!string.IsNullOrEmpty(loginElem.UserName))
                {
                    returnUrl = URLHelper.AddParameterToUrl(returnUrl, "username", loginElem.UserName);
                }

                switch (UserAccountLockCode.ToEnum(ui.UserAccountLockReason))
                {
                    case UserAccountLockEnum.MaximumInvalidLogonAttemptsReached:
                        result = AuthenticationHelper.SendUnlockAccountRequest(ui, siteName, "USERLOGON", SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendPasswordEmailsFrom"), MacroContext.CurrentResolver, returnUrl);
                        break;

                    case UserAccountLockEnum.PasswordExpired:
                        result = AuthenticationHelper.SendPasswordRequest(ui, siteName, "USERLOGON", SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendPasswordEmailsFrom"), "Membership.PasswordExpired", null, AuthenticationHelper.GetResetPasswordUrl(siteName), out outParam, returnUrl);
                        break;
                }
            }

            return result;
        }

        public void RaiseCallbackEvent(string eventArgument)
        {
            return;
        }

        #endregion "ICallbackEventHandler Members"

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