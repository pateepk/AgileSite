using CMS.Activities.Loggers;
using CMS.Base;
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

using System;
using System.Data;
using System.Web;

namespace CMSApp.CMSWebParts.WTE.TrainingNetwork
{
    /// <summary>
    /// TN custom registration form
    /// </summary>
    public partial class TN_RegistrationForm : CMSAbstractWebPart
    {
        #region "classes"

        /// <summary>
        /// Company Registration information
        /// </summary>
        public class CompanyRegistrationInformation
        {
            #region members

            private int _loginCreds = 1;
            private string _companyID = String.Empty;
            private string _companyName = String.Empty;
            private string _roleName = String.Empty;
            private string _passwordEmail = String.Empty;
            private string _contactEmail = String.Empty;
            private string _TNRepEmail = String.Empty;
            private int _active = 0;

            #endregion members

            #region properties

            /// <summary>
            /// Login credential
            /// 1 = Email Address
            /// 2 = User Name
            /// </summary>
            public int LoginCreds
            {
                get
                {
                    return _loginCreds;
                }
                set
                {
                    _loginCreds = value;
                }
            }

            /// <summary>
            /// CompanyID
            /// </summary>
            public string CompanyID
            {
                get
                {
                    return _companyID;
                }
                set
                {
                    _companyID = value;
                }
            }

            /// <summary>
            /// Company Name
            /// </summary>
            public string CompanyName
            {
                get
                {
                    return _companyName;
                }
                set
                {
                    _companyName = value;
                }
            }

            /// <summary>
            /// Role Name to assign to the user
            /// </summary>
            public string RoleName
            {
                get
                {
                    return _roleName;
                }
                set
                {
                    _roleName = value;
                }
            }

            /// <summary>
            /// Is the company active?
            /// </summary>
            public int Active
            {
                get
                {
                    return _active;
                }
                set
                {
                    _active = value;
                }
            }

            /// <summary>
            /// The reset password email (if user does not  have one)
            /// </summary>
            public string PasswordEmail
            {
                get
                {
                    return _passwordEmail;
                }
                set
                {
                    _passwordEmail = value;
                }
            }

            /// <summary>
            /// Company contact email
            /// </summary>
            public string ContactEmail
            {
                get
                {
                    return _contactEmail;
                }
                set
                {
                    _contactEmail = value;
                }
            }

            /// <summary>
            /// Company TNRep Email
            /// </summary>
            public string TNRepEmail
            {
                get
                {
                    return _TNRepEmail;
                }
                set
                {
                    _TNRepEmail = value;
                }
            }

            /// <summary>
            /// Is the company active
            /// </summary>
            public bool IsActive
            {
                get
                {
                    return Active == 1;
                }
            }

            /// <summary>
            /// Is login credential for company email login
            /// </summary>
            public bool IsEmailLogin
            {
                get
                {
                    return LoginCreds == 1;
                }
            }

            /// <summary>
            /// Get the password reset email
            /// </summary>
            public string PasswordResetEmail
            {
                get
                {
                    string ret = String.Empty;
                    ret = PasswordEmail;
                    //if (String.IsNullOrWhiteSpace(ret))
                    //{
                    //    // assume that the email is the manager's email
                    //    ret = ContactEmail;
                    //}

                    //if (String.IsNullOrWhiteSpace(ret))
                    //{
                    //    // if no contact email specified send to it to the TNRep
                    //    ret = TNRepEmail;
                    //}

                    //if (String.IsNullOrWhiteSpace(ret))
                    //{
                    //    // no email on file?
                    //    ret = "nopasswordresetemail@tnn.local";
                    //}

                    return ret;
                }
            }

            #endregion properties

            #region methods

            /// <summary>
            /// Get Company Registration data
            /// </summary>
            /// <param name="p_companyID"></param>
            /// <returns></returns>
            public void SetRegistrationInformation(string p_companyID)
            {
                DataSet ds = null;
                try
                {
                    GeneralConnection conn = ConnectionHelper.GetConnection();
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@CompanyID", p_companyID);

                    QueryParameters qp = new QueryParameters("Proc_TN_customtable_Customers_GetRegistrationInfo", parameters, QueryTypeEnum.StoredProcedure, false);
                    ds = conn.ExecuteQuery(qp);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = ds.Tables[0].Rows[0];
                        if (row != null)
                        {
                            CompanyID = GetStringOrNull(row["CompanyID"]);
                            RoleName = GetStringOrNull(row["RegistrationRoleName"]);
                            LoginCreds = GetIntOrNull(row["RegistrationLoginCreds"]).GetValueOrDefault(1);
                            Active = GetIntOrNull(row["RegistrationActive"]).GetValueOrDefault(0);
                            CompanyName = GetStringOrNull(row["RegistrationCompanyName"]);
                            PasswordEmail = GetStringOrNull(row["PasswordEmail"]);
                            ContactEmail = GetStringOrNull(row["ContactEmail"]);
                            TNRepEmail = GetStringOrNull(row["TNRepEmail"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // no op
                }
            }

            /// <summary>
            /// Get Int value or null
            /// </summary>
            /// <param name="p_object"></param>
            /// <returns></returns>
            protected int? GetIntOrNull(object p_object)
            {
                int? ret = null;
                int val = 0;
                if (int.TryParse(GetStringOrNull(p_object), out val))
                {
                    ret = val;
                }
                return ret;
            }

            /// <summary>
            /// Get string value or null
            /// </summary>
            /// <param name="p_object"></param>
            /// <returns></returns>
            protected string GetStringOrNull(object p_object)
            {
                string ret = String.Empty;
                try
                {
                    if (p_object != null)
                    {
                        ret = p_object.ToString();
                    }
                }
                catch (Exception)
                {
                    // ignore it
                }
                return ret;
            }

            #endregion methods
        }

        #endregion "classes"

        #region "members"

        private CompanyRegistrationInformation _registrationInfo = new CompanyRegistrationInformation();

        #endregion "members"

        #region "properties"

        #region "Data"

        /// <summary>
        /// Company registration info
        /// </summary>
        protected CompanyRegistrationInformation RegistrationInfo
        {
            get
            {
                return _registrationInfo;
            }
            set
            {
                _registrationInfo = value;
            }
        }

        #endregion "Data"

        #region "Text properties"

        /// <summary>
        /// Gets or sets the Skin ID.
        /// </summary>
        public override string SkinID
        {
            get
            {
                return base.SkinID;
            }
            set
            {
                base.SkinID = value;
                SetSkinID(value);
            }
        }

        /// <summary>
        /// User Name Text
        /// </summary>
        public string UserNameText
        {
            get
            {
                //return DataHelper.GetNotEmpty(GetValue("UserNameText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.UserName$}"));
                return DataHelper.GetNotEmpty(GetValue("UserNameText"), "User Name");
            }
            set
            {
                SetValue("UerNameText", value);
                lblUserName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the first name text.
        /// </summary>
        public string FirstNameText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("FirstNameText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.FirstName$}"));
            }
            set
            {
                SetValue("FirstNameText", value);
                lblFirstName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name text.
        /// </summary>
        public string LastNameText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("LastNameText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.LastName$}"));
            }
            set
            {
                SetValue("LastNameText", value);
                lblLastName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the e-mail text.
        /// </summary>
        public string EmailText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("EmailText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Email$}"));
            }
            set
            {
                SetValue("EmailText", value);
                lblEmail.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the password text.
        /// </summary>
        public string PasswordText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("PasswordText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Password$}"));
            }
            set
            {
                SetValue("PasswordText", value);
                lblPassword.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the confirmation password text.
        /// </summary>
        public string ConfirmPasswordText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("ConfirmPasswordText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.ConfirmPassword$}"));
            }
            set
            {
                SetValue("ConfirmPasswordText", value);
                lblConfirmPassword.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        public string ButtonText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("ButtonText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Button$}"));
            }
            set
            {
                SetValue("ButtonText", value);
                btnOk.Text = value;
            }
        }

        /// <summary>
        /// Add Button Text (Text to display in add mode)
        /// </summary>
        public string AddButtonText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("AddButtonText"), ButtonText);
            }
            set
            {
                SetValue("AddButtonText", value);
                btnOk.Text = value;
            }
        }

        /// <summary>
        /// Update Button Text (Text to display in edit mode)
        /// </summary>
        public string UpdateButtonText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("UpdateButtonText"), ButtonText);
            }
            set
            {
                SetValue("UpdateButtonText", value);
                btnOk.Text = value;
            }
        }

        /// <summary>
        /// The Cancel Button Text
        /// </summary>
        public string CancelButtonText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("CancelButtonText"), "Cancel");
            }
            set
            {
                SetValue("CancelButtonText", value);
                btnOk.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the captcha label text.
        /// </summary>
        public string CaptchaText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("CaptchaText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Captcha$}"));
            }
            set
            {
                SetValue("CaptchaText", value);
                lblCaptcha.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets registration approval page URL.
        /// </summary>
        public string ApprovalPage
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("ApprovalPage"), "");
            }
            set
            {
                SetValue("ApprovalPage", value);
            }
        }

        #endregion "Text properties"

        #region "CSS Class"

        /// <summary>
        /// Wrapper CSS class
        /// </summary>
        public string WrapperCSSClass
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("WrapperCSSClass"), "");
            }
            set
            {
                SetValue("WrapperCSSClass", value);
            }
        }

        /// <summary>
        /// Button Style/CSS class for OK/submit button
        /// </summary>
        public string OkButtonCssClass
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("OkButtonCssClass"), "");
            }
            set
            {
                SetValue("OkButtonCssClass", value);
            }
        }

        #endregion "CSS Class"

        #region "Other properties"

        /// <summary>
        /// Is edit mode
        /// </summary>
        public bool IsEditMode
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IsEditMode"), false);
            }
            set
            {
                SetValue("IsEditMode", value);
            }
        }

        /// <summary>
        /// Allow user to edit the password
        /// </summary>
        public bool AllowEditPassword
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowEditPassword"), false);
            }
            set
            {
                SetValue("AllowEditPassword", value);
            }
        }

        /// <summary>
        /// Allow password no to be entered while editing.
        /// </summary>
        public bool IsPasswordOptionalInEditMode
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IsPasswordOptionalInEditMode"), false);
            }
            set
            {
                SetValue("IsPasswordOptionalInEditMode", value);
            }
        }

        /// <summary>
        /// Allow user to edit the user name
        /// </summary>
        public bool AllowEditUserName
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AllowEditUserName"), false);
            }
            set
            {
                SetValue("AllowEditUserName", value);
            }
        }

        /// <summary>
        /// Show Title
        /// </summary>
        public bool ShowTitle
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowTitle"), false);
            }
            set
            {
                SetValue("ShowTitle", value);
            }
        }

        /// <summary>
        /// The user id as string
        /// </summary>
        public string UserID
        {
            get
            {
                string ret = ResolveMacros(ValidationHelper.GetString(GetValue("UserID"), String.Empty));
                if (!String.IsNullOrWhiteSpace(ret) && ret.Trim().ToLower() == "{?eid?}")
                {
                    ret = String.Empty;
                }
                return ValidationHelper.GetString(GetValue("UserID"), String.Empty);
            }
            set
            {
                SetValue("UserGuid", value);
            }
        }

        /// <summary>
        /// user guid as string
        /// </summary>
        public string UserGuid
        {
            get
            {
                string ret = ResolveMacros(ValidationHelper.GetString(GetValue("UserGuid"), String.Empty));
                if (!String.IsNullOrWhiteSpace(ret) && ret.Trim().ToLower() == "{?eguid?}")
                {
                    ret = String.Empty;
                }
                return ret;
            }
            set
            {
                SetValue("UserGuid", value);
            }
        }

        /// <summary>
        /// The custom registration data (This currently hold The CompanyID field of the Company)
        /// </summary>
        public string CustomRegistrationData
        {
            get
            {
                string cdata = ResolveMacros(ValidationHelper.GetString(GetValue("CustomRegistrationData"), String.Empty));
                if (!String.IsNullOrWhiteSpace(cdata) && cdata.Trim().ToLower() == "{?CustomData?}")
                {
                    cdata = String.Empty;
                }
                string ret = ResolveMacros(cdata);
                return ret;
            }
            set
            {
                SetValue("CustomRegistrationData", value);
            }
        }

        /// <summary>
        /// The company ID - Correspond to the ItemID of the the Company/Client
        /// </summary>
        public string CompanyID
        {
            get
            {
                string cid = ResolveMacros(ValidationHelper.GetString(GetValue("cid"), String.Empty));
                if (!String.IsNullOrWhiteSpace(cid) && cid.Trim().ToLower() == "{?cid?}")
                {
                    cid = String.Empty;
                }
                string comid = ResolveMacros(ValidationHelper.GetString(GetValue("CompanyID"), cid));
                if (!String.IsNullOrWhiteSpace(comid) && comid.Trim().ToLower() == "{?companyid?}")
                {
                    comid = String.Empty;
                    comid = cid;
                }
                string ret = ResolveMacros(comid);
                return ret;
            }
            set
            {
                SetValue("CompanyID", value);
            }
        }

        /// <summary>
        /// Is this TNN custom registration
        /// </summary>
        public bool IsCustomRegistration
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IsCustomRegistration"), false);
            }
            set
            {
                SetValue("IsCustomRegistration", value);
            }
        }

        /// <summary>
        /// Log in the user on Custom Registration
        /// </summary>
        public bool LoginUserOnSuccess
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("LoginUserOnSuccess"), false);
            }
            set
            {
                SetValue("LoginUserOnSuccess", value);
            }
        }

        #endregion "Other properties"

        #region "Registration properties"

        /// <summary>
        /// The default password reset/default email
        /// </summary>
        public string DefaultPasswordResetEmail
        {
            get
            {
                string ret = RegistrationInfo.PasswordResetEmail;

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = ValidationHelper.GetString(GetValue("DefaultPasswordResetEmail"), "");
                }

                if (String.IsNullOrWhiteSpace(ret))
                {
                    // still nothing.
                    ret = "_NO_EMAIL_";
                }

                return ret;
            }
            set
            {
                SetValue("DefaultPasswordResetEmail", value);
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether email to user should be sent.
        /// </summary>
        public bool SendWelcomeEmail
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("SendWelcomeEmail"), true);
            }
            set
            {
                SetValue("SendWelcomeEmail", value);
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether user is enabled after registration.
        /// </summary>
        public bool EnableUserAfterRegistration
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("EnableUserAfterRegistration"), true);
            }
            set
            {
                SetValue("EnableUserAfterRegistration", value);
            }
        }

        /// <summary>
        /// Gets or sets the sender email (from).
        /// </summary>
        public string FromAddress
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("FromAddress"), SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSNoreplyEmailAddress"));
            }
            set
            {
                SetValue("FromAddress", value);
            }
        }

        /// <summary>
        /// Gets or sets the recipient email (to).
        /// </summary>
        public string ToAddress
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("ToAddress"), SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSAdminEmailAddress"));
            }
            set
            {
                SetValue("ToAddress", value);
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether after successful registration is
        /// notification email sent to the administrator
        /// </summary>
        public bool NotifyAdministrator
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("NotifyAdministrator"), false);
            }
            set
            {
                SetValue("NotifyAdministrator", value);
            }
        }

        /// <summary>
        /// Gets or sets the roles where is user assigned after successful registration.
        /// </summary>
        public string AssignRoles
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AssignToRoles"), "");
            }
            set
            {
                SetValue("AssignToRoles", value);
            }
        }

        /// <summary>
        /// Gets or sets the sites where is user assigned after successful registration.
        /// </summary>
        public string AssignToSites
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AssignToSites"), "");
            }
            set
            {
                SetValue("AssignToSites", value);
            }
        }

        /// <summary>
        /// Gets or sets the message which is displayed after successful registration.
        /// </summary>
        public string DisplayMessage
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DisplayMessage"), "");
            }
            set
            {
                SetValue("DisplayMessage", value);
            }
        }

        /// <summary>
        /// Gets or set the url where is user redirected after successful registration.
        /// </summary>
        public string RedirectToURL
        {
            get
            {
                return ResolveMacros(ValidationHelper.GetString(GetValue("RedirectToURL"), String.Empty));
            }
            set
            {
                SetValue("RedirectToURL", value);
            }
        }

        /// <summary>
        /// Cancel URL
        /// </summary>
        public string CancelURL
        {
            get
            {
                return ResolveMacros(ValidationHelper.GetString(GetValue("CancelURL"), String.Empty));
            }
            set
            {
                SetValue("CancelURL", value);
            }
        }

        /// <summary>
        /// Gets or sets value that indicates whether the captcha image should be displayed.
        /// </summary>
        public bool DisplayCaptcha
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("DisplayCaptcha"), false);
            }
            set
            {
                SetValue("DisplayCaptcha", value);
                plcCaptcha.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the default starting alias path for newly registered user.
        /// </summary>
        public string StartingAliasPath
        {
            get
            {
                return ValidationHelper.GetString(GetValue("StartingAliasPath"), "");
            }
            set
            {
                SetValue("StartingAliasPath", value);
            }
        }

        /// <summary>
        /// Gets or sets the password minimal length.
        /// </summary>
        public int PasswordMinLength
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PasswordMinLength"), 0);
            }
            set
            {
                SetValue("PasswordMinLength", 0);
            }
        }

        #endregion "Registration properties"

        #region "Conversion properties"

        /// <summary>
        /// Gets or sets the conversion track name used after successful registration.
        /// </summary>
        public string TrackConversionName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TrackConversionName"), "");
            }
            set
            {
                if (value.Length > 400)
                {
                    value = value.Substring(0, 400);
                }
                SetValue("TrackConversionName", value);
            }
        }

        /// <summary>
        /// Gets or sets the conversion value used after successful registration.
        /// </summary>
        public double ConversionValue
        {
            get
            {
                return ValidationHelper.GetDoubleSystem(GetValue("ConversionValue"), 0);
            }
            set
            {
                SetValue("ConversionValue", value);
            }
        }

        #endregion "Conversion properties"

        #endregion "properties"

        #region "Methods"

        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //// When HttpCacheability is set to NoCache or ServerAndNoCache
            //// the Expires HTTP header is set to -1 by default. This instructs
            //// the client to not cache responses in the History folder. Thus,
            //// each time you use the back/forward buttons, the client requests
            //// a new version of the response.
            //Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);

            //Response.Cache.SetNoStore();

            //// Override the ServerAndNoCache behavior by setting the SetAllowInBrowserHistory
            //// method to true. This directs the client browser to store responses in
            //// its History folder.
            //Response.Cache.SetAllowResponseInBrowserHistory(false);

            //if (!RequestHelper.IsPostBack())
            //{
            //    SetupControl();
            //}
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
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (StopProcessing)
            {
                // Do not process
                rfvUserName.Enabled = false;
                rfvFirstName.Enabled = false;
                rfvLastName.Enabled = false;
                rfvEmail.Enabled = false;
                rfvConfirmPassword.Enabled = false;
            }
            else
            {
                //if (!RequestHelper.IsPostBack())
                {
                    RegistrationInfo.SetRegistrationInformation(CompanyID);
                }

                // Set default visibility
                pnlForm.Visible = true;
                lblText.Visible = false;

                UserInfo ui = null;

                if (IsEditMode)
                {
                    if (UserGuid != null)
                    {
                        ui = UserInfoProvider.GetUserInfoByGUID(ValidationHelper.GetGuid(UserGuid, Guid.Empty));
                    }
                    else if (UserID != null)
                    {
                        ui = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);
                    }

                    if (ui == null)
                    {
                        IsEditMode = false;
                    }
                }

                if (!RequestHelper.IsPostBack())
                {
                    chkUpdatePassword.Visible = false;

                    if (IsEditMode)
                    {
                        if (ui != null)
                        {
                            // fill in the info
                            if (ui.Email != DefaultPasswordResetEmail)
                            {
                                txtEmail.Text = ui.Email;
                            }
                            txtUserName.Text = ui.UserName;
                            txtFirstName.Text = ui.FirstName;
                            txtLastName.Text = ui.LastName;
                            txtPassword.Text = (string)ui.GetValue("UserPassword");
                            chkEnableUser.Checked = ui.Enabled;

                            if (!AllowEditUserName)
                            {
                                txtUserName.Enabled = false;
                            }

                            if (!AllowEditPassword && IsEditMode)
                            {
                                lblPassword.Visible = false;
                                txtPassword.Visible = false;
                                lblConfirmPassword.Visible = false;
                                rfvConfirmPassword.Enabled = false;
                                divPassword.Visible = false;
                                divConfirmPassword.Visible = false;
                                divUpdatePassword.Visible = false;
                            }

                            // we want to make password optional in edit mode
                            if (IsPasswordOptionalInEditMode && IsEditMode)
                            {
                                rfvConfirmPassword.Enabled = false;
                                chkUpdatePassword.Visible = true;
                                divPassword.Visible = false;
                                divConfirmPassword.Visible = false;
                            }
                        }
                    }
                }

                string companyName = !String.IsNullOrWhiteSpace(RegistrationInfo.CompanyName) ? RegistrationInfo.CompanyName : "N/A";
                if (IsEditMode)
                {
                    lblTitle.Text = "Editing: " + ui.UserName + " (" + RegistrationInfo.CompanyName + ")";
                    divEnabledUser.Visible = true;
                }
                else
                {
                    lblTitle.Text = "Create User for: " + RegistrationInfo.CompanyName;
                    divEnabledUser.Visible = false;
                }

                lblTitle.Visible = ShowTitle;
                divTitle.Visible = ShowTitle;

                if (RegistrationInfo.IsEmailLogin)
                {
                    // we do not need the username field.
                    lblUserName.Visible = false;
                    rfvUserName.Enabled = false;
                    divUserName.Visible = false;
                }
                else
                {
                    // make email optional for user name registration.
                    rfvEmail.Enabled = false;
                    lblEmailNote.Text = "(optional)";
                }

                // Set texts
                lblFirstName.Text = FirstNameText;
                lblLastName.Text = LastNameText;
                lblPassword.Text = PasswordText;
                lblConfirmPassword.Text = ConfirmPasswordText;

                lblUserName.Text = UserNameText;

                if (RegistrationInfo.IsEmailLogin)
                {
                    lblEmail.Text = EmailText;
                }
                else
                {
                    lblEmail.Text = ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Email$}");
                }

                if (IsEditMode)
                {
                    btnOk.Text = UpdateButtonText;
                }
                else
                {
                    btnOk.Text = AddButtonText;
                }

                lblCaptcha.Text = CaptchaText;

                if (String.IsNullOrWhiteSpace(CancelURL))
                {
                    btnCancel.Visible = false;
                }
                else
                {
                    btnCancel.Text = CancelButtonText;
                }

                //if (CMS.Membership.MFAuthenticationHelper.IsMultiFactorAutEnabled && !CMS.Membership.MFAuthenticationHelper.IsMultiFactorAutRequired)
                //{
                //    plcMFIsRequired.Visible = true;
                //    chkUseMultiFactorAutentization.ToolTip = GetString("webparts_membership_registrationform.mfexplanationtext");
                //}
                // Set required field validators texts

                rfvFirstName.ErrorMessage = GetString("Webparts_Membership_RegistrationForm.rfvFirstName");
                rfvLastName.ErrorMessage = GetString("Webparts_Membership_RegistrationForm.rfvLastName");
                rfvEmail.ErrorMessage = GetString("Webparts_Membership_RegistrationForm.rfvEmail");
                rfvConfirmPassword.ErrorMessage = GetString("Webparts_Membership_RegistrationForm.rfvConfirmPassword");
                rfvUserName.ErrorMessage = "Please enter a User Name";

                // Add unique validation form
                rfvFirstName.ValidationGroup = ClientID + "_registration";
                rfvLastName.ValidationGroup = ClientID + "_registration";
                rfvEmail.ValidationGroup = ClientID + "_registration";
                txtPassword.ValidationGroup = ClientID + "_registration";
                rfvConfirmPassword.ValidationGroup = ClientID + "_registration";
                rfvUserName.ValidationGroup = ClientID + "_registration";
                btnOk.ValidationGroup = ClientID + "_registration";

                // Set SkinID
                if (!StandAlone && (PageCycle < PageCycleEnum.Initialized))
                {
                    SetSkinID(SkinID);
                }

                plcCaptcha.Visible = DisplayCaptcha;

                // WAI validation
                lblPassword.AssociatedControlClientID = txtPassword.InputClientID;
            }
        }

        /// <summary>
        /// Sets SkinID.
        /// </summary>
        private void SetSkinID(string skinId)
        {
            if (skinId != "")
            {
                lblUserName.SkinID = skinId;
                lblFirstName.SkinID = skinId;
                lblLastName.SkinID = skinId;
                lblEmail.SkinID = skinId;
                lblPassword.SkinID = skinId;
                lblConfirmPassword.SkinID = skinId;
                txtUserName.SkinID = skinId;
                txtFirstName.SkinID = skinId;
                txtLastName.SkinID = skinId;
                txtEmail.SkinID = skinId;
                txtPassword.SkinID = skinId;
                txtConfirmPassword.SkinID = skinId;
                btnOk.SkinID = skinId;
                btnCancel.SkinID = skinId;

                chkEnableUser.SkinID = skinId;
            }
        }

        /// <summary>
        /// Check if IP is allowed
        /// </summary>
        /// <param name="p_siteName"></param>
        /// <param name="p_controlEnum"></param>
        /// <returns></returns>
        protected bool IsIpAllowed(string p_siteName, BanControlEnum p_controlEnum, out string p_message)
        {
            bool ret = true;
            p_message = String.Empty;
            // Ban IP addresses which are blocked for registration
            if (!BannedIPInfoProvider.IsAllowed(p_siteName, p_controlEnum))
            {
                ret = false;
                p_message = GetString("banip.ipisbannedregistration");
            }
            return ret;
        }

        /// <summary>
        /// Check to see if the user already exists
        /// </summary>
        /// <param name="p_username"></param>
        /// <param name="p_compareUser"></param>
        /// <param name="p_siteInfo"></param>
        /// <returns></returns>
        protected bool IsUserAlreadyExists(string p_username, UserInfo p_compareUser, SiteInfo p_siteInfo, out string p_message)
        {
            p_message = String.Empty;
            bool ret = false;
            UserInfo ui = UserInfoProvider.GetUserInfo(p_username);
            UserInfo siteui = UserInfoProvider.GetUserInfo(UserInfoProvider.EnsureSitePrefixUserName(p_username, p_siteInfo));
            if ((ui != null) || (siteui != null))
            {
                if (p_compareUser != null)
                {
                    if (ui != null && ui.UserID != p_compareUser.UserID)
                    {
                        ret = true;
                    }

                    // if it's not already true
                    if (ret != true)
                    {
                        if (siteui != null && siteui.UserID != p_compareUser.UserID)
                        {
                            ret = true;
                        }
                    }
                }
                else
                {
                    ret = true;
                }
            }

            if (ret == true)
            {
                p_message = GetString("Webparts_Membership_RegistrationForm.UserAlreadyExists").Replace("%%name%%", HTMLHelper.HTMLEncode(p_username));
            }

            return ret;
        }

        /// <summary>
        /// Check to see if the user name is reserved
        /// </summary>
        /// <param name="p_username"></param>
        /// <param name="p_nickname"></param>
        /// <param name="p_sitename"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected bool IsReservedName(string p_username, string p_nickname, string p_sitename, out string p_message)
        {
            bool ret = false;
            p_message = String.Empty;

            // Check for reserved user names like administrator, sysadmin, ...
            if (UserInfoProvider.NameIsReserved(p_sitename, p_username))
            {
                ret = true;
                p_message = GetString("Webparts_Membership_RegistrationForm.UserNameReserved").Replace("%%name%%", HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(p_username, true)));
            }
            if (!ret)
            {
                if (UserInfoProvider.NameIsReserved(p_sitename, p_nickname))
                {
                    ret = true;
                    p_message = GetString("Webparts_Membership_RegistrationForm.UserNameReserved").Replace("%%name%%", HTMLHelper.HTMLEncode(p_nickname));
                }
            }
            return ret;
        }

        /// <summary>
        /// Check to see if the email is valid
        /// </summary>
        /// <param name="p_email"></param>
        /// <param name="p_compareUser"></param>
        /// <param name="p_siteList"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected bool IsValidEmail(string p_email, UserInfo p_compareUser, string[] p_siteList, out string p_message)
        {
            bool ret = true;
            p_message = String.Empty;

            if (!RegistrationInfo.IsEmailLogin && p_email == DefaultPasswordResetEmail)
            {
                ret = true;
            }
            else if (RegistrationInfo.IsEmailLogin || !String.IsNullOrWhiteSpace(p_email))
            {
                if (!ValidationHelper.IsEmail(p_email.ToLowerCSafe()))
                {
                    ret = false;
                    p_message = GetString("Webparts_Membership_RegistrationForm.EmailIsNotValid");
                }

                if (ret)
                {
                    // Check whether email is unique if it is required
                    if (!UserInfoProvider.IsEmailUnique(p_email, p_siteList, 0))
                    {
                        if (p_compareUser != null)
                        {
                            UserInfo existingUser = UserInfoProvider.GetUsers().Where("Email", QueryOperator.Equals, p_email).FirstObject;
                            if (existingUser != null)
                            {
                                // it shouldn't be null?
                                if (existingUser.UserID != p_compareUser.UserID)
                                {
                                    ret = false;
                                }
                            }
                        }
                        else
                        {
                            ret = false;
                        }

                        if (!ret)
                        {
                            p_message = GetString("UserInfo.EmailAlreadyExist");
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Check to see if the captcha is valid
        /// </summary>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected bool IsCaptchaValid(out string p_message)
        {
            bool ret = true;
            p_message = String.Empty;
            // Check if captcha is required and verifiy captcha text
            if (DisplayCaptcha && !scCaptcha.IsValid())
            {
                ret = false;
                p_message = GetString("Webparts_Membership_RegistrationForm.captchaError");
            }
            return ret;
        }

        /// <summary>
        /// Check to see if the password is valid
        /// </summary>
        /// <param name="p_password"></param>
        /// <param name="p_confirmPassword"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected bool IsValidPassword(string p_password, string p_confirmPassword, out string p_message)
        {
            bool ret = true;
            p_message = String.Empty;

            if (String.IsNullOrWhiteSpace(p_password) && IsEditMode && IsPasswordOptionalInEditMode)
            {
                // no need to check
                ret = true;
            }
            else if (!AllowEditPassword && IsEditMode)
            {
                ret = true; // password is not entered
            }
            else
            {
                // if they type in the passwords we need to validate it.
                if (p_password != p_confirmPassword)
                {
                    ret = false;
                    p_message = GetString("Webparts_Membership_RegistrationForm.PassworDoNotMatch");
                }

                if (ret)
                {
                    if ((PasswordMinLength > 0) && (p_password.Length < PasswordMinLength))
                    {
                        ret = false;
                        p_message = String.Format(GetString("Webparts_Membership_RegistrationForm.PasswordMinLength"), PasswordMinLength.ToString());
                    }

                    if (ret)
                    {
                        if (!txtPassword.IsValid())
                        {
                            ret = false;
                            p_message = AuthenticationHelper.GetPolicyViolationMessage(SiteContext.CurrentSiteName);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// OK click handler (Proceed registration).
        /// </summary>
        protected void btnOK_Click(object sender, EventArgs e)
        {
            if (PortalContext.IsDesignMode(PortalContext.ViewMode) || (HideOnCurrentPage) || (!IsVisible))
            {
                // Do not process
            }
            else
            {
                string errorMessage = String.Empty;
                bool hasError = false;

                hasError = !IsCaptchaValid(out errorMessage);

                if (!hasError)
                {
                    string username = !String.IsNullOrWhiteSpace(txtUserName.Text) ? txtUserName.Text.Trim() : String.Empty;
                    string nickname = String.Empty; // this should not exists on add mode (but may exists in edit mode)
                    string email = !String.IsNullOrWhiteSpace(txtEmail.Text) ? txtEmail.Text.Trim() : String.Empty;
                    string firstName = !String.IsNullOrWhiteSpace(txtFirstName.Text) ? txtFirstName.Text.Trim() : String.Empty;
                    string lastName = !String.IsNullOrWhiteSpace(txtLastName.Text) ? txtLastName.Text.Trim() : String.Empty;
                    string fullname = UserInfoProvider.GetFullName(firstName, String.Empty, lastName);
                    string password = !String.IsNullOrWhiteSpace(txtPassword.Text) ? txtPassword.Text : String.Empty;
                    string confirmpassword = !String.IsNullOrWhiteSpace(txtConfirmPassword.Text) ? txtConfirmPassword.Text : String.Empty;
                    string startingAliasPath = !String.IsNullOrEmpty(StartingAliasPath) ? MacroResolver.ResolveCurrentPath(StartingAliasPath) : String.Empty;

                    bool enabled = chkEnableUser.Checked;

                    if (!IsEditMode)
                    {
                        enabled = EnableUserAfterRegistration;
                    }

                    if (RegistrationInfo.IsEmailLogin && String.IsNullOrWhiteSpace(username))
                    {
                        username = email;
                    }

                    if (String.IsNullOrWhiteSpace(email) && !RegistrationInfo.IsEmailLogin)
                    {
                        email = DefaultPasswordResetEmail;

                        if (String.IsNullOrWhiteSpace(email))
                        {
                            // this is optional, set to the "manager" password.
                            email = "companyreset@tnn.local"; // for now, but need to pull in the mananger
                        }
                    }

                    String siteName = SiteContext.CurrentSiteName;
                    string[] siteList = { siteName };

                    // If AssignToSites field set
                    if (!String.IsNullOrEmpty(AssignToSites))
                    {
                        siteList = AssignToSites.Split(';');
                    }

                    SiteInfo si = SiteContext.CurrentSite;

                    // Get the user information
                    UserInfo ui = null;
                    UserInfo edituser = null;

                    // check for ban IP
                    hasError = !IsIpAllowed(siteName, BanControlEnum.Registration, out errorMessage);
                    if (!hasError)
                    {
                        if (IsEditMode)
                        {
                            #region bind the edit user

                            if (UserGuid != null)
                            {
                                edituser = UserInfoProvider.GetUserInfoByGUID(ValidationHelper.GetGuid(UserGuid, Guid.Empty));
                            }
                            else if (UserID != null)
                            {
                                edituser = UserInfoProvider.GetUserInfo(UserID);
                            }

                            if (edituser != null)
                            {
                                nickname = edituser.UserNickName;
                            }

                            #endregion bind the edit user
                        }

                        // check to see if the user is already exists
                        if (IsUserAlreadyExists(username, edituser, si, out errorMessage))
                        {
                            hasError = true;
                        }

                        if (!hasError)
                        {
                            hasError = !IsValidEmail(email, edituser, siteList, out errorMessage);
                        }

                        if (!hasError)
                        {
                            // check to see if the password is valid.
                            hasError = !IsValidPassword(password, confirmpassword, out errorMessage);
                        }

                        if (!hasError)
                        {
                            if (IsEditMode)
                            {
                                ui = edituser;
                            }
                            else
                            {
                                ui = new UserInfo();
                            }

                            // Ensure site prefixes
                            if (UserInfoProvider.UserNameSitePrefixEnabled(siteName))
                            {
                                username = UserInfoProvider.EnsureSitePrefixUserName(username, si);
                            }

                            if (!IsEditMode || AllowEditUserName || RegistrationInfo.IsEmailLogin)
                            {
                                // make sure the username is updated correctly.
                                if (RegistrationInfo.IsEmailLogin && ui.UserName == ui.Email)
                                {
                                    username = email;
                                }
                                ui.UserName = username;
                            }

                            ui.Email = email;
                            ui.FirstName = firstName;
                            ui.LastName = lastName;
                            ui.FullName = fullname;
                            ui.Enabled = enabled;

                            if (IsEditMode)
                            {
                                #region "edit mode: go ahead and update the info and set the passwords, we are done here."

                                string companyid = (string)ui.GetValue("UserCompany"); // test value

                                ui.Update();

                                if (AllowEditPassword)
                                {
                                    if (!String.IsNullOrWhiteSpace(password))
                                    {
                                        UserInfoProvider.SetPassword(ui, password);
                                    }
                                }

                                #endregion "edit mode: go ahead and update the info and set the passwords, we are done here."
                            }
                            else
                            {
                                #region "add mode"

                                ui.PreferredCultureCode = "";

                                //eg: mod for site
                                // using the Middle Name only
                                //ui.MiddleName = Request.QueryString["CompanyID"];

                                if (IsCustomRegistration && !String.IsNullOrWhiteSpace(CustomRegistrationData))
                                {
                                    ui.SetValue("UserCustomData", CustomRegistrationData); // set the company id
                                }
                                else
                                {
                                    ui.SetValue("UserCompany", CompanyID); // set the company id
                                }

                                ui.UserMFRequired = chkUseMultiFactorAutentization.Checked;

                                //ui.UserCampaign = Service<ICampaignService>.Entry().CampaignCode;

                                //commented during upgrade to v10
                                //ui.SetPrivilegeLevel(UserPrivilegeLevelEnum.None);

                                ui.UserSettings.UserRegistrationInfo.IPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                                ui.UserSettings.UserRegistrationInfo.Agent = HttpContext.Current.Request.UserAgent;

                                // Check whether confirmation is required
                                bool requiresConfirmation = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationEmailConfirmation");
                                bool requiresAdminApprove = false;

                                if (!requiresConfirmation)
                                {
                                    // If confirmation is not required check whether administration approval is reqiures
                                    if ((requiresAdminApprove = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationAdministratorApproval")))
                                    {
                                        ui.Enabled = false;
                                        ui.UserSettings.UserWaitingForApproval = true;
                                    }
                                }
                                else
                                {
                                    // EnableUserAfterRegistration is overrided by requiresConfirmation - user needs to be confirmed before enable
                                    ui.Enabled = false;
                                }

                                // Set user's starting alias path
                                if (!String.IsNullOrWhiteSpace(startingAliasPath))
                                {
                                    ui.UserStartingAliasPath = startingAliasPath;
                                }

                                #region "License limitations"

                                UserInfoProvider.CheckLicenseLimitation(ui, ref errorMessage);

                                if (!String.IsNullOrEmpty(errorMessage))
                                {
                                    hasError = true;
                                }

                                #endregion "License limitations"

                                if (!hasError)
                                {
                                    if (!String.IsNullOrWhiteSpace(txtPassword.Text))
                                    {
                                        // Set password
                                        UserInfoProvider.SetPassword(ui, txtPassword.Text);
                                    }

                                    #region "Welcome Emails (confirmation, waiting for approval)"

                                    bool error = false;
                                    EmailTemplateInfo template = null;

                                    string emailSubject = null;
                                    if (!String.IsNullOrWhiteSpace(ui.Email) && ui.Email != DefaultPasswordResetEmail)
                                    {
                                        // Send welcome message with username and password, with confirmation link, user must confirm registration
                                        if (requiresConfirmation)
                                        {
                                            template = EmailTemplateProvider.GetEmailTemplate("RegistrationConfirmation", siteName);
                                            emailSubject = EmailHelper.GetSubject(template, GetString("RegistrationForm.RegistrationConfirmationEmailSubject"));
                                        }
                                        // Send welcome message with username and password, with information that user must be approved by administrator
                                        else if (SendWelcomeEmail)
                                        {
                                            if (requiresAdminApprove)
                                            {
                                                template = EmailTemplateProvider.GetEmailTemplate("Membership.RegistrationWaitingForApproval", siteName);
                                                emailSubject = EmailHelper.GetSubject(template, GetString("RegistrationForm.RegistrationWaitingForApprovalSubject"));
                                            }
                                            // Send welcome message with username and password, user can logon directly
                                            else
                                            {
                                                template = EmailTemplateProvider.GetEmailTemplate("Membership.Registration", siteName);
                                                emailSubject = EmailHelper.GetSubject(template, GetString("RegistrationForm.RegistrationSubject"));
                                            }
                                        }

                                        if (template != null)
                                        {
                                            // Create relation between contact and user. This ensures that contact will be correctly recognized when user approves registration (if approval is required)
                                            int contactId = ModuleCommands.OnlineMarketingGetCurrentContactID();
                                            if (contactId > 0)
                                            {
                                                ModuleCommands.OnlineMarketingCreateRelation(ui.UserID, 0, contactId);
                                            }

                                            // default resolver (from CustomRegistration From)
                                            //var resolver = MembershipResolvers.GetMembershipRegistrationResolver(ui, txtPassword.Text, AuthenticationHelper.GetRegistrationApprovalUrl(ApprovalPage, ui.UserGUID, siteName, NotifyAdministrator));

                                            //var resolver = MembershipResolvers.GetMembershipRegistrationResolver(ui, txtPassword.Text, AuthenticationHelper.GetRegistrationApprovalUrl(ApprovalPage, ui.UserGUID, siteName, NotifyAdministrator));
                                            //var resolver = MembershipResolvers.GetMembershipRegistrationResolver(ui, txtPassword.Text);
                                            //var resolver = GetForgottenPasswordResolver.GetMembershipRegistrationResolver(ui, txtPassword.Text, "https://www.trainingnetworknow.com/");

                                            // The resolver below allow clear password to be included in the registration email
                                            MacroResolver resolver = MembershipResolvers.GetForgottenPasswordResolver(ui, txtPassword.Text, AuthenticationHelper.GetRegistrationApprovalUrl(ApprovalPage, ui.UserGUID, CurrentSiteName, NotifyAdministrator));

                                            // Email message
                                            EmailMessage emailm = new EmailMessage();
                                            emailm.EmailFormat = EmailFormatEnum.Default;
                                            emailm.Recipients = ui.Email;

                                            emailm.From = EmailHelper.GetSender(template, SettingsKeyInfoProvider.GetValue(siteName + ".CMSNoreplyEmailAddress"));

                                            // Enable macro encoding for body
                                            resolver.Settings.EncodeResolvedValues = true;
                                            emailm.Body = resolver.ResolveMacros(template.TemplateText);

                                            // Disable macro encoding for plaintext body and subject
                                            emailm.PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText);
                                            emailm.Subject = resolver.ResolveMacros(emailSubject);

                                            emailm.CcRecipients = template.TemplateCc;
                                            emailm.BccRecipients = template.TemplateBcc;

                                            try
                                            {
                                                EmailHelper.ResolveMetaFileImages(emailm, template.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
                                                // Send the e-mail immediately
                                                EmailSender.SendEmail(siteName, emailm, true);
                                            }
                                            catch (Exception ex)
                                            {
                                                EventLogProvider.LogException("E", "RegistrationForm - SendEmail", ex);
                                                error = true;
                                            }
                                        }
                                    }

                                    // If there was some error, user must be deleted
                                    if (error)
                                    {
                                        lblError.Visible = true;
                                        lblError.Text = GetString("RegistrationForm.UserWasNotCreated");

                                        // Email was not send, user can't be approved - delete it
                                        UserInfoProvider.DeleteUser(ui);
                                        return;
                                    }

                                    #endregion "Welcome Emails (confirmation, waiting for approval)"

                                    #region "Administrator notification email"

                                    // Notify administrator if enabled and e-mail confirmation is not required
                                    if (!requiresConfirmation && NotifyAdministrator && (FromAddress != String.Empty) && (ToAddress != String.Empty))
                                    {
                                        EmailTemplateInfo mEmailTemplate = null;
                                        MacroResolver resolver = MembershipResolvers.GetRegistrationResolver(ui);
                                        if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationAdministratorApproval"))
                                        {
                                            mEmailTemplate = EmailTemplateProvider.GetEmailTemplate("Registration.Approve", siteName);
                                        }
                                        else
                                        {
                                            mEmailTemplate = EmailTemplateProvider.GetEmailTemplate("Registration.New", siteName);
                                        }

                                        if (mEmailTemplate == null)
                                        {
                                            // Log missing e-mail template
                                            EventLogProvider.LogEvent(EventType.ERROR, "RegistrationForm", "GetEmailTemplate", eventUrl: RequestContext.RawURL);
                                        }
                                        else
                                        {
                                            EmailMessage message = new EmailMessage();

                                            message.EmailFormat = EmailFormatEnum.Default;
                                            message.From = EmailHelper.GetSender(mEmailTemplate, FromAddress);
                                            message.Recipients = ToAddress;

                                            // Enable macro encoding for body
                                            resolver.Settings.EncodeResolvedValues = true;
                                            message.Body = resolver.ResolveMacros(mEmailTemplate.TemplateText);

                                            // Disable macro encoding for plaintext body and subject
                                            resolver.Settings.EncodeResolvedValues = false;
                                            message.PlainTextBody = resolver.ResolveMacros(mEmailTemplate.TemplatePlainText);
                                            message.Subject = resolver.ResolveMacros(EmailHelper.GetSubject(mEmailTemplate, GetString("RegistrationForm.EmailSubject")));

                                            message.CcRecipients = mEmailTemplate.TemplateCc;
                                            message.BccRecipients = mEmailTemplate.TemplateBcc;

                                            try
                                            {
                                                // Attach template meta-files to e-mail
                                                EmailHelper.ResolveMetaFileImages(message, mEmailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
                                                EmailSender.SendEmail(siteName, message);
                                            }
                                            catch
                                            {
                                                EventLogProvider.LogEvent(EventType.ERROR, "Membership", "RegistrationEmail");
                                            }
                                        }
                                    }

                                    #endregion "Administrator notification email"

                                    #region "Web analytics"

                                    // Track successful registration conversion
                                    if (TrackConversionName != String.Empty)
                                    {
                                        if (AnalyticsHelper.AnalyticsEnabled(siteName) && !AnalyticsHelper.IsIPExcluded(siteName, RequestContext.UserHostAddress))
                                        {
                                            // Log conversion
                                            HitLogProvider.LogConversions(siteName, LocalizationContext.PreferredCultureCode, TrackConversionName, 0, ConversionValue);
                                        }
                                    }

                                    // Log registered user if confirmation is not required
                                    if (!requiresConfirmation)
                                    {
                                        AnalyticsHelper.LogRegisteredUser(siteName, ui);
                                    }

                                    #endregion "Web analytics"

                                    #region "On-line marketing - activity"

                                    // Log registered user if confirmation is not required
                                    if (!requiresConfirmation)
                                    {
                                        MembershipActivityLogger.LogRegistration(ui.UserName, DocumentContext.CurrentDocument, true);

                                        //if (activity.Data != null)
                                        //{
                                        //    activity.Data.ContactID = ModuleCommands.OnlineMarketingGetUserLoginContactID(ui);
                                        //    activity.Log();
                                        //}
                                        // Log login activity
                                        if (ui.Enabled)
                                        {
                                            // Log activity
                                            int contactID = ModuleCommands.OnlineMarketingGetUserLoginContactID(ui);
                                            MembershipActivityLogger.LogLogin(ui.UserName, DocumentContext.CurrentDocument);
                                        }
                                    }

                                    #endregion "On-line marketing - activity"

                                    #region "Roles & authentication"

                                    string[] roleList = AssignRoles.Split(';');

                                    string s = String.Empty;
                                    foreach (string sn in siteList)
                                    {
                                        // Add new user to the current site
                                        UserInfoProvider.AddUserToSite(ui.UserName, sn);

                                        foreach (string roleName in roleList)
                                        {
                                            if (!String.IsNullOrEmpty(roleName))
                                            {
                                                s = roleName.StartsWithCSafe(".") ? "" : sn;

                                                // Add user to desired roles
                                                if (RoleInfoProvider.RoleExists(roleName, s))
                                                {
                                                    UserInfoProvider.AddUserToRole(ui.UserName, roleName, s);
                                                }
                                            }

                                            if (!String.IsNullOrWhiteSpace(RegistrationInfo.RoleName))
                                            {
                                                s = RegistrationInfo.RoleName.StartsWithCSafe(".") ? "" : sn;
                                                // Add user to desired roles
                                                if (RoleInfoProvider.RoleExists(RegistrationInfo.RoleName, s))
                                                {
                                                    UserInfoProvider.AddUserToRole(ui.UserName, RegistrationInfo.RoleName, s);
                                                }
                                            }
                                        }
                                    }

                                    #endregion "Roles & authentication"
                                }

                                #endregion "add mode"
                            }
                        }

                        if (ui != null && ui.Enabled && IsCustomRegistration && LoginUserOnSuccess)
                        {
                            AuthenticationHelper.AuthenticateUser(ui.UserName, true);
                        }
                    }
                }

                if (!hasError)
                {
                    if (!String.IsNullOrWhiteSpace(DisplayMessage))
                    {
                        pnlForm.Visible = false;
                        lblText.Visible = true;
                        lblText.Text = DisplayMessage;
                    }
                    else
                    {
                        if (RedirectToURL != String.Empty)
                        {
                            URLHelper.Redirect(RedirectToURL);
                        }
                        else if (QueryHelper.GetString("ReturnURL", "") != String.Empty)
                        {
                            string url = QueryHelper.GetString("ReturnURL", "");

                            // Do url decode
                            url = Server.UrlDecode(url);

                            // Check that url is relative path or hash is ok
                            if (url.StartsWithCSafe("~") || url.StartsWithCSafe("/") || QueryHelper.ValidateHash("hash", "aliaspath"))
                            {
                                URLHelper.Redirect(url);
                            }
                            // Absolute path with wrong hash
                            else
                            {
                                URLHelper.Redirect(AdministrationUrlHelper.GetErrorPageUrl("dialogs.badhashtitle", "dialogs.badhashtext"));
                            }
                        }
                    }
                }
                else
                {
                    lblError.Visible = true;
                    lblError.Text = errorMessage;
                }
            }

            //lblError.Visible = false;
        }

        /// <summary>
        /// Cancel Button Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            if (PortalContext.IsDesignMode(PortalContext.ViewMode) || (HideOnCurrentPage) || (!IsVisible))
            {
                // Do not process
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(CancelURL))
                {
                    URLHelper.Redirect(CancelURL);
                }
                lblError.Visible = false;
            }
        }

        /// <summary>
        /// Check update password check box checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void chkUpdatePassword_CheckedChanged(object sender, EventArgs e)
        {
            // toggle

            divPassword.Visible = chkUpdatePassword.Checked;
            divConfirmPassword.Visible = chkUpdatePassword.Checked;

            /*
            if (chkUpdatePassword.Checked)
            {
                divPassword.Visible = false;
                divConfirmPassword.Visible = false;
            }
            else
            {
                divPassword.Visible = true;
                divConfirmPassword.Visible = true;
            }
            */
        }

        #endregion "Methods"

        #region "Utilities"

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetRequestParam(string p_key)
        {
            string ret = String.Empty;
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request != null)
                    {
                        ret = HttpContext.Current.Request.Params[p_key];
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetSession(string p_key)
        {
            string ret = String.Empty;
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        ret = (string)HttpContext.Current.Session[p_key];
                    }
                }
            }
            catch (Exception ex)
            {
                // do nothing
            }
            return ret;
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void SetSession(string p_key, object p_value)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session[p_key] = p_value;
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #endregion "Utilities"
    }
}