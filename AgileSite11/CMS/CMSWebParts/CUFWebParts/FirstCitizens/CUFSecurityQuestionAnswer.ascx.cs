using System;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
{
    /// <summary>
    /// Webpart for security question challenge
    /// </summary>
    public partial class CUFSecurityQuestionAnswerControl : CMSAbstractWebPart
    {
        #region properties

        #region custom data field from CMS

        /// <summary>
        /// Redirect URL on success
        /// </summary>
        protected string RedirectPageURL
        {
            get
            {
                string redirectTemplate = GetSafeStringValue("RedirectPageUrl", String.Empty);
                return redirectTemplate;
            }
            set
            {
            }
        }

        /// <summary>
        /// How many times can the user answer wrong
        /// </summary>
        protected int LockOutLimit
        {
            get
            {
                return GetSafeIntValue("LockOutLimit", 3);
            }
        }

        #endregion custom data field from CMS

        /// <summary>
        /// The last question ID
        /// </summary>
        public int? LastQuestionID
        {
            get
            {
                object o = SessionHelper.GetValue("LastQuestionID");
                if (o != null)
                {
                    int val = 0;
                    if (int.TryParse(o.ToString(), out val))
                    {
                        if (val > 0)
                        {
                            return val;
                        }
                    }
                }
                return null;
            }
            set
            {
                SessionHelper.SetValue("LastQuestionID", value);
            }
        }

        /// <summary>
        /// Count of how many failed challege
        /// </summary>
        public int? FailedChallengeCount
        {
            get
            {
                object o = SessionHelper.GetValue("FailedChallengeCount");
                if (o != null)
                {
                    int val = 0;
                    if (int.TryParse(o.ToString(), out val))
                    {
                        if (val > 0)
                        {
                            return val;
                        }
                    }
                }
                return 0;
            }
            set
            {
                SessionHelper.SetValue("FailedChallengeCount", value);
            }
        }

        #endregion properties

        #region page event

        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            lblError.Text = String.Empty;
            if (!RequestHelper.IsPostBack())
            {
                BindSecurityQuestionAnswer();
            }
        }

        #endregion page event

        #region data binding

        /// <summary>
        /// Bind the security question and answer challenge
        /// </summary>
        private void BindSecurityQuestionAnswer()
        {
            //CUFSecurityQuestionAnswers securityQuestionAnswer = new CUFSecurityQuestionAnswers();
            //CUFSecurityQuestionAnswer answer = securityQuestionAnswer.GetChallengeQuestion(LastQuestionID);
            //if (answer != null)
            //{
            //    LastQuestionID = answer.QuestionID;
            //    lblSecurityQuestion.Text = answer.QuestionText;
            //    txtSecurityQuestionAnswer.Text = String.Empty;
            //}
            //else
            //{
            //    // send the user to answer the question?
            //    // should not happen, unless this page was accessed directly
            //}
        }

        #endregion data binding

        #region general events

        /// <summary>
        /// The Ok button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ValidateAnswer();
        }

        #endregion general events

        #region methods

        /// <summary>
        /// Validate the answer
        /// </summary>
        private void ValidateAnswer()
        {
            //string answer = txtSecurityQuestionAnswer.Text;
            //lblError.Text = String.Empty;
            //if (!String.IsNullOrWhiteSpace(answer))
            //{
            //    CUFSecurityQuestionAnswers answers = new CUFSecurityQuestionAnswers();
            //    bool isValidAnswer = answers.ValidateAnswer(LastQuestionID, answer);
            //    if (isValidAnswer)
            //    {
            //        // make sure the member is logged in.
            //        AuthenticationHelper.AuthenticateUser(MembershipContext.AuthenticatedUser.UserName, false);

            //        // set the cookie
            //        answers.SetCookie();

            //        // redirect to the member page or return URL
            //        // Redirect user to the return url, or if is not defined redirect to the default target url
            //        string url = QueryHelper.GetString("ReturnURL", string.Empty);
            //        if (!String.IsNullOrWhiteSpace(url))
            //        {
            //            RedirectToURL(url);
            //        }
            //        else
            //        {
            //            // redirect to root?
            //            RedirectToURL(RedirectPageURL);
            //        }
            //    }
            //    else
            //    {
            //        FailedChallengeCount = FailedChallengeCount + 1;
            //        if (FailedChallengeCount >= 3)
            //        {
            //            // locked out the user
            //            //CUFSecurityQuestionAnswers.LockoutUser(MembershipContext.AuthenticatedUser, CurrentSite.SiteName);

            //            //// log out the user
            //            //AuthenticationHelper.LogoutUser();
            //            divQuestionAnswer.Visible = false;
            //            lblError.Text = "Challenge limit reached, your account had been locked out";
            //        }
            //        else
            //        {
            //            // show error message
            //            lblError.Text = String.Format("Answer do not match, please answer the next question. You account will be locked out after {0} attempts.", answers.MaximumFailedCount);
            //            BindSecurityQuestionAnswer();
            //        }
            //    }
            //}
            //else
            //{
            //    lblError.Text = "Please enter an answer.";
            //}
        }

        #endregion methods

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
            //lblInfoMessage.Text = p_message;
        }

        /// <summary>
        /// Shows the specified warning message, optionally with a tooltip text.
        /// </summary>
        /// <param name="p_message">Warning message text</param>
        protected void SetMessage(string p_message)
        {
            //lblMessage.Text = p_message;
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