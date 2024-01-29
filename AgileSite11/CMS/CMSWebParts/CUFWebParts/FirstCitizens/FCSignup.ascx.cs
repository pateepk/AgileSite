using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
{
    public partial class FCSignup : CMSAbstractWebPart
    {
        private bool testMode = false;

        #region Properties

        #region "Properties"

        /// <summary>
        /// The statement page URL
        /// </summary>
        public string RedirectPageURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectPageURL"), "/Secure/Member/Statements");
            }
            set
            {
                this.SetValue("RedirectPageURL", value);
            }
        }

        /// <summary>
        /// Get the URL of the disclosure page
        /// </summary>
        public string DisclosurePagedURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("DisclosurePagedURL"), "/Secure/Member/Internal/Disclosure-Statement");
            }
            set
            {
                this.SetValue("DisclosurePagedURL", value);
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
        /// URL to the statement page
        /// </summary>
        public string StatementPageURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("StatementPageURL"), "/Secure/Member/Statements");
            }
            set
            {
                this.SetValue("StatementPageURL", value);
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

        #endregion "Properties"

        #endregion Properties

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int autoCloseDelay = 10000;
                rttuserName.AutoCloseDelay = autoCloseDelay;
                rttPassword.AutoCloseDelay = autoCloseDelay;
                rttEmail.AutoCloseDelay = autoCloseDelay;
                rttTaxID.AutoCloseDelay = autoCloseDelay;
                rttAccountNumber.AutoCloseDelay = autoCloseDelay;
            }
        }

        protected void rbCreateAccount_Click(Object sender, EventArgs e)
        {
            bool TestMode = false;
            StringBuilder sbTest = new StringBuilder();
            StringBuilder sbError = new StringBuilder();

            if (Page.IsValid)
            {
                if ((PageManager.ViewMode == ViewModeEnum.Design) || (HideOnCurrentPage) || (!IsVisible))
                {
                    // Do not process
                }
                else
                {
                    UserInfo userInfo = null;
                    string statementDB = String.Empty; // SettingsKeyInfoProvider.GetValue("StatementDatabase", CurrentSiteID);
                    int memberID = -1;

                    try
                    {
                        //pad account numbers to 10 characters
                        string accountNumber = rtbAccountNumber.Text.PadLeft(10, '0');
                        string taxID = rtbTaxID.Text;
                        string emailAddress = rtbEmail.Text;
                        string password = rtbPassword.Text;
                        string login = rtbLogin.Text;
                        string memberNumber = "";
                        string memberName = "";

                        //is there a member already associated with this taxid / account
                        if (sbError.Length == 0)
                        {
                            QueryDataParameters parameters = new QueryDataParameters();
                            parameters.Add("@TaxID", taxID);
                            parameters.Add("@AccountNumber", accountNumber);

                            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("{0}.dbo.sproc_FCFCU_signup_authenticate", statementDB), parameters, QueryTypeEnum.StoredProcedure);
                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow row = ds.Tables[0].Rows[0];
                                memberID = Convert.ToInt32(row["MemberId"]);
                                memberNumber = Convert.ToString(row["MemberNumber"]);
                                memberName = Convert.ToString(row["MemberName"]);
                            }
                            else
                            {
                                sbError.Append("We were unable to verify that the account number entered is associated with the entered SSN/Tax ID. Please check your information and try again.");
                            }
                        }

                        if (sbError.Length == 0)
                        {
                            //does the login already exist?
                            UserInfo existingLoginAccount = UserInfoProvider.GetUserInfo(login);

                            if (existingLoginAccount != null && existingLoginAccount.UserSettings.GetIntegerValue("CUMemberID", -1) != memberID)
                            {
                                sbError.Append("The User Name you selected is already in use. Please choose another User Name.");
                            }
                        }

                        if (sbError.Length == 0)
                        {
                            //have member record, do we have an AS user account?
                            //Get AS user info
                            ObjectQuery<UserSettingsInfo> userSettingsSet = UserSettingsInfoProvider.GetUserSettings()
                                .WhereEquals("CUMemberID", memberID);
                            if (userSettingsSet != null && userSettingsSet.Count<UserSettingsInfo>() > 0)
                            {
                                if (userSettingsSet.Count<UserSettingsInfo>() > 1)
                                {
                                    //mulitple accounts with user id
                                    //@todo Error: mulitple accounts found matching member ID.
                                    sbError.Append("Error: mulitple accounts found matching member ID. We were unable to create your user account.");
                                }
                                else
                                {
                                    int userId = userSettingsSet.First<UserSettingsInfo>().UserSettingsUserID;
                                    userInfo = UserInfoProvider.GetUserInfo(userId);
                                }
                            }
                        }

                        bool foundExistingUserInfo = (userInfo != null);

                        if (foundExistingUserInfo)
                        {
                            //same user name?
                            if (userInfo.UserName != login && !userInfo.UserName.StartsWith("CU_"))
                            {
                                sbError.Append("The entered SSN/Tax ID is already associated to an existing User Name.");
                            }

                            //if SSO created account and email is different then deny
                            if (!string.IsNullOrWhiteSpace(userInfo.Email) && userInfo.Email != emailAddress && userInfo.UserSettings.GetStringValue("CUUserCreatedBy", "self registration") == "SSO")
                            {
                                sbError.Append("This account is associated to an online banking account. You must register using the same email used by your online banking account.");
                            }
                        }

                        //we can create the account
                        if (sbError.Length == 0)
                        {
                            //create an AS user account
                            if (!foundExistingUserInfo)
                            {
                                //find member in statement database
                                QueryDataParameters parameters = new QueryDataParameters();
                                parameters = new QueryDataParameters();
                                parameters.Add("membernumber", memberNumber);

                                userInfo = new UserInfo();
                                userInfo.UserName = login;
                                userInfo.FullName = memberName;
                                userInfo.Enabled = true;
                                if (!string.IsNullOrWhiteSpace(emailAddress))
                                {
                                    userInfo.Email = emailAddress;
                                }

                                try
                                {
                                    UserInfoProvider.SetUserInfo(userInfo);
                                    UserInfoProvider.SetPassword(userInfo, password);

                                    UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userInfo.UserID);
                                    userSettings.SetValue("CUMemberNumber", memberNumber);
                                    userSettings.SetValue("CUMemberID", memberID);
                                    userSettings.SetValue("CUUserCreatedBy", "self registration");
                                    UserSettingsInfoProvider.SetUserSettingsInfo(userSettings);

                                    UserInfoProvider.AddUserToSite(userInfo.UserName, SiteContext.CurrentSiteName);
                                }
                                catch (CodeNameNotUniqueException)
                                {
                                    //user must have been created while this one was attempted to be created
                                    userInfo = GetUserInfoForMemberID(memberID);
                                    if (userInfo != null)
                                    {
                                        foundExistingUserInfo = true;
                                    }
                                }
                            }
                            else
                            {
                                //do any updates
                                //update email if necessary
                                bool doUserUpdate = false;
                                if (!string.IsNullOrWhiteSpace(login) && userInfo.UserName != login)
                                {
                                    userInfo.UserName = login;
                                    doUserUpdate = true;
                                }

                                if (!string.IsNullOrWhiteSpace(emailAddress) && userInfo.Email != emailAddress)
                                {
                                    userInfo.Email = emailAddress;
                                    doUserUpdate = true;
                                }

                                //update the password
                                UserInfoProvider.SetPassword(userInfo, password);

                                if (doUserUpdate)
                                {
                                    if (TestMode)
                                    {
                                        sbTest.Append(string.Format("updating agilesite user email:{0} and/or user name:{1} for member number:{2}<br />", userInfo.Email, userInfo.FullName, memberNumber));
                                    }

                                    if (userInfo.UserID == 0)
                                    {
                                        sbError.Append("Found userinfo but userid is 0!");
                                    }
                                    else
                                    {
                                        UserInfoProvider.SetUserInfo(userInfo);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sbError.Append(ex.Message);
                    }

                    if (sbError.Length > 0)
                    {
                        lblMessage.Text = string.Format("<font color=\"red\">{0}</font>", sbError.ToString());
                    }
                    else
                    {
                        lblMessage.Text = string.Format("<font color=\"green\">{0}</font>", "Account created successfully.");

                        //authenticate user
                        AuthenticationHelper.AuthenticateUser(userInfo.UserName, false);

                        UserSettingsInfo userSettingsInfo = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userInfo.UserID);

                        //check to see if they have accepted the disclosure
                        DateTime disclosureDateAccepted = userSettingsInfo.GetDateTimeValue("CUDisclosureDate", DateTime.Now.AddYears(1));

                        bool showDisclosure = true;
                        if (disclosureDateAccepted <= DateTime.Now)
                        {
                            showDisclosure = false;
                            if (testMode)
                            {
                                sbTest.Append(string.Format("disclosure date valid: {0:MM/dd/yyyy}<br />", disclosureDateAccepted));
                            }
                        }

                        if (showDisclosure == true)
                        {
                            //see if member has previously accepted disclosure on old system
                            QueryDataParameters qdp = new QueryDataParameters();
                            qdp.Add("MemberID", memberID);
                            string disclosureSQL = string.Format("{0}.dbo.sproc_showDisclosure", statementDB);
                            if (testMode)
                            {
                                sbTest.Append(string.Format("Running disclosure query {0} for member ID: {1}<br />", disclosureSQL, memberID));
                            }

                            showDisclosure = Convert.ToBoolean(ConnectionHelper.ExecuteScalar(disclosureSQL, qdp, QueryTypeEnum.StoredProcedure));

                            if (showDisclosure == false)
                            {
                                //update disclosure date so we do not have to run query again
                                userSettingsInfo.SetValue("CUDisclosureDate", DateTime.Now);
                                UserSettingsInfoProvider.SetUserSettingsInfo(userSettingsInfo);
                            }
                        }
                        if (testMode)
                        {
                            sbTest.Append("show disclosure?" + showDisclosure + "<br />");
                        }
                        else
                        {
                            if (showDisclosure == true)
                            {
                                Response.Redirect(DisclosurePagedURL);
                            }
                            else
                            {
                                // check security question and answer
                                //if (SecurityQuestionEnabled && !CUFSecurityQuestionAnswers.CheckUserSecurityQuestionAnswer())
                                {
                                    RedirectToURL(SecurityQuestionAnswerURL);
                                }
                                //else
                                {
                                    RedirectToURL(StatementPageURL);
                                }
                            }
                        }
                    }
                }
            }
        }

        private UserInfo GetUserInfoForMemberID(int memberID)
        {
            //Get user info
            //UserInfo userInfo = UserInfoProvider.GetUserInfo(userName);
            UserInfo userInfo = null;

            ObjectQuery<UserSettingsInfo> userSettingsSet = UserSettingsInfoProvider.GetUserSettings()
                    .WhereEquals("CUMemberID", memberID);

            if (userSettingsSet != null && userSettingsSet.Count<UserSettingsInfo>() > 0)
            {
                if (userSettingsSet.Count<UserSettingsInfo>() > 1)
                {
                    //mulitple accounts with user id
                    //errorSB.Append("Error: mulitple accounts found matching account number<br />\n");
                }
                else
                {
                    int userId = userSettingsSet.First<UserSettingsInfo>().UserSettingsUserID;
                    //this call was returning bad userinfo record in some cases, don't know why, retrying with v8.1
                    userInfo = UserInfoProvider.GetUserInfo(userId);

                    //if (TestMode)
                    //{
                    //    testSB.Append(string.Format("Found userid:{0} for uid:{1}<br />", userInfo.UserID, userId));
                    //}
                }
            }

            return userInfo;
        }

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