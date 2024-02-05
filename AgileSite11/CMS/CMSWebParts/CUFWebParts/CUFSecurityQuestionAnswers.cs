using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    /// <summary>
    /// Security Question and Answer list
    /// </summary>
    public class CUFSecurityQuestionAnswers
    {
        #region member

        private List<CUFSecurityQuestionAnswer> _questionAnswers = new List<CUFSecurityQuestionAnswer>();
        private int _minimumAnswerCount = 3;
        private int _maximumFailedCount = 3;
        private int? _siteID = null;
        private int? _cuMemberID = null;
        private int? _UserID = null;
        private string _statementDB = String.Empty;
        private string _cuMemberNumber = String.Empty;
        private string _authCookieKey = "CU_SQAC_COOKIE";
        private UserInfo _currentUserInfo = null;
        private int _maxDropDownItem = 4;
        private int _groupID = 0;

        #endregion member

        #region properties

        /// <summary>
        /// The question and answer
        /// </summary>
        public List<CUFSecurityQuestionAnswer> QuestionAnswers
        {
            get
            {
                return _questionAnswers;
            }
            set
            {
                _questionAnswers = value;
            }
        }

        /// <summary>
        /// The question as group for drop down repeater data source
        /// </summary>
        public List<CUFSecurityQuestionAnswersGroup> QuestionAnswersGroup
        {
            get
            {
                List<CUFSecurityQuestionAnswersGroup> answerGroups = new List<CUFSecurityQuestionAnswersGroup>();
                int index = 0;
                int groupIndex = 0;

                while (index < _questionAnswers.Count && groupIndex < MinimumAnswerCount)
                {
                    groupIndex++;

                    CUFSecurityQuestionAnswersGroup group = new CUFSecurityQuestionAnswersGroup();
                    while (group.Count < MaxDropDownItem && index < _questionAnswers.Count)
                    {
                        group.Add(_questionAnswers[index]);
                        index++;
                    }

                    if (group.Count > 0)
                    {
                        answerGroups.Add(group);
                    }
                }

                return answerGroups;
            }
        }

        /// <summary>
        /// Minimum answers required
        /// </summary>
        public int MinimumAnswerCount
        {
            get
            {
                return _minimumAnswerCount;
            }
            set
            {
                _minimumAnswerCount = value;
            }
        }

        /// <summary>
        /// Maximum number of failed responsed before the user the is locked out
        /// </summary>
        public int MaximumFailedCount
        {
            get
            {
                return _maximumFailedCount;
            }
            set
            {
                _maximumFailedCount = value;
            }
        }

        /// <summary>
        /// Maximum Drop down Item
        /// </summary>
        public int MaxDropDownItem
        {
            get
            {
                return _maxDropDownItem;
            }
            set
            {
                _maxDropDownItem = value;
            }
        }

        /// <summary>
        /// The site ID
        /// </summary>
        public int? SiteID
        {
            get
            {
                return _siteID;
            }
            set
            {
                _siteID = value;
            }
        }

        /// <summary>
        /// CU Member ID
        /// </summary>
        public int? CUMemberID
        {
            get
            {
                return _cuMemberID;
            }
            set
            {
                _cuMemberID = value;
            }
        }

        /// <summary>
        /// Agilesite User ID
        /// </summary>
        public int? UserID
        {
            get
            {
                return _UserID;
            }
            set
            {
                _UserID = value;
            }
        }

        /// <summary>
        /// The statement DB
        /// </summary>
        public string StatementDB
        {
            get
            {
                return _statementDB;
            }
            set
            {
                _statementDB = value;
            }
        }

        /// <summary>
        /// CUF member number
        /// </summary>
        public string CUMemberNumber
        {
            get
            {
                return _cuMemberNumber;
            }
            set
            {
                _cuMemberNumber = value;
            }
        }

        /// <summary>
        /// The current user info
        /// </summary>
        protected UserInfo CurrentUserInfo
        {
            get
            {
                return _currentUserInfo;
            }
            set
            {
                _currentUserInfo = value;
            }
        }

        /// <summary>
        /// Check to see if we have the minimum answers
        /// </summary>
        public bool HasMinimumAnswers
        {
            get
            {
                bool hasMin = false;
                List<CUFSecurityQuestionAnswer> validAnswer = _questionAnswers.FindAll(delegate(CUFSecurityQuestionAnswer a)
                {
                    return a.IsValidAnswer;
                });
                hasMin = validAnswer.Count >= MinimumAnswerCount;
                return hasMin;
            }
        }

        /// <summary>
        /// Check to see if the user need security question challenge
        /// </summary>
        public bool NeedSecurityQuestionChallenge
        {
            get
            {
                bool needChallenge = true;
                if (CheckCookie())
                {
                    if (CheckIP())
                    {
                        needChallenge = false;
                    }
                }
                return needChallenge;
            }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public CUFSecurityQuestionAnswers()
            : this((UserInfo)null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_userID"></param>
        /// <param name="p_cuMemberID"></param>
        public CUFSecurityQuestionAnswers(UserInfo p_userInfo)
        {
            BindUserInfo(p_userInfo);
            GetFromDB();
        }

        /// <summary>
        /// Create list of security questions and answers from a repeater
        /// </summary>
        /// <param name="p_repeater"></param>
        public CUFSecurityQuestionAnswers(Repeater p_repeater)
            : this(null, p_repeater)
        {
        }

        /// <summary>
        /// Create list of security questions and answers from a repeater
        /// </summary>
        /// <param name="p_userInfo"></param>
        /// <param name="p_repeater"></param>
        public CUFSecurityQuestionAnswers(UserInfo p_userInfo, Repeater p_repeater)
        {
            BindUserInfo(p_userInfo);
            GetFromRepeater(p_repeater);
        }

        #endregion constructor

        #region methods

        /// <summary>
        /// Get a question for challenge
        /// </summary>
        /// <param name="p_lastQuestionID"></param>
        /// <returns></returns>
        public CUFSecurityQuestionAnswer GetChallengeQuestion(int? p_lastQuestionID)
        {
            CUFSecurityQuestionAnswer question = _questionAnswers.Find(delegate(CUFSecurityQuestionAnswer a)
            {
                return (!p_lastQuestionID.HasValue || a.QuestionID > p_lastQuestionID)
                    && !String.IsNullOrWhiteSpace(a.Answer);
            });
            return question;
        }

        /// <summary>
        /// Check the security question answer
        /// </summary>
        /// <param name="p_questionID"></param>
        /// <param name="p_answer"></param>
        /// <returns></returns>
        public bool ValidateAnswer(int? p_questionID, string p_answer)
        {
            bool matched = false;

            CUFSecurityQuestionAnswer answer = _questionAnswers.Find(delegate(CUFSecurityQuestionAnswer a)
            {
                return a.QuestionID == p_questionID;
            });

            if (answer != null)
            {
                if (answer.Answer.Trim().ToLower() == p_answer.Trim().ToLower())
                {
                    matched = true;
                }
            }

            return matched;
        }

        /// <summary>
        /// Clear list
        /// </summary>
        public void Clear()
        {
            _questionAnswers.Clear();
        }

        /// <summary>
        /// Add item to list
        /// </summary>
        /// <param name="p_answer"></param>
        public void Add(CUFSecurityQuestionAnswer p_answer)
        {
            if (p_answer != null)
            {
                _questionAnswers.Add(p_answer);
            }
        }

        /// <summary>
        /// Save the answers
        /// </summary>
        public void Save()
        {
            Save(null);
        }

        /// <summary>
        /// Save the answers
        /// </summary>
        /// <param name="p_answers"></param>
        public void Save(List<CUFSecurityQuestionAnswer> p_answers)
        {
            if (p_answers == null)
            {
                UpdateSecurityQuestionAnswer(_questionAnswers);
            }
            else
            {
                UpdateSecurityQuestionAnswer(p_answers);
            }
        }

        /// <summary>
        /// Bind the user info
        /// </summary>
        /// <param name="p_ui"></param>
        private void BindUserInfo(UserInfo p_ui)
        {
            if (p_ui == null)
            {
                p_ui = UserInfoProvider.GetFullUserInfo(MembershipContext.AuthenticatedUser.UserID);
            }

            _currentUserInfo = p_ui;
            _siteID = SiteContext.CurrentSiteID;
            _statementDB = SettingsKeyInfoProvider.GetStringValue("StatementDatabase", _siteID);
            _maximumFailedCount = SettingsKeyInfoProvider.GetIntValue("MaximumSecurityChallengeFailureCount", _siteID);

            if (_maximumFailedCount == 0)
            {
                _maximumFailedCount = 3;
            }

            _minimumAnswerCount = SettingsKeyInfoProvider.GetIntValue("MinimumSecurityAnswersCount", _siteID);
            if (_minimumAnswerCount == 0)
            {
                _minimumAnswerCount = 3;
            }

            if (p_ui != null)
            {
                UserID = p_ui.UserID;
                int mid = p_ui.UserSettings.GetIntegerValue("CUMemberID", 0);
                if (mid > 0)
                {
                    _cuMemberID = mid;
                }
                _cuMemberNumber = p_ui.UserSettings.GetStringValue("CUMemberNumber", String.Empty);
            }
        }

        /// <summary>
        /// Get data from a repeater
        /// </summary>
        /// <param name="p_repeater"></param>
        private void GetFromRepeater(Repeater p_repeater)
        {
            Clear();
            if (p_repeater != null)
            {
                foreach (RepeaterItem item in p_repeater.Items)
                {
                    CUFSecurityQuestionAnswer answer = new CUFSecurityQuestionAnswer(item);
                    if (answer != null)
                    {
                        Add(answer);
                    }
                }
            }
        }

        /// <summary>
        /// Get data from a dataset
        /// </summary>
        /// <param name="p_dataSet"></param>
        private void GetFromDB()
        {
            Clear();
            DataSet ds = GetSecurityQuestionAnswers();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    CUFSecurityQuestionAnswer answer = new CUFSecurityQuestionAnswer(row);
                    Add(answer);
                }
            }
        }

        #region data access

        /// <summary>
        /// Get the member's security questions
        /// </summary>
        /// <returns></returns>
        private DataSet GetSecurityQuestionAnswers()
        {
            QueryDataParameters qdp = new QueryDataParameters();
            qdp.Add("UserID", _UserID);
            qdp.Add("SiteID", _siteID);
            qdp.Add("AnswerID", null);
            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.CU_Proc_GetSecurityQuestionAnswer", qdp, QueryTypeEnum.StoredProcedure);
            return ds;
        }

        /// <summary>
        /// Update the security question and answers
        /// </summary>
        /// <param name="p_answers"></param>
        private void UpdateSecurityQuestionAnswer(List<CUFSecurityQuestionAnswer> p_answers)
        {
            if (p_answers != null && p_answers.Count > 0)
            {
                foreach (CUFSecurityQuestionAnswer answer in p_answers)
                {
                    // only save out valid answers
                    if (answer.IsValidAnswer)
                    {
                        QueryDataParameters qdp = new QueryDataParameters();
                        qdp.Add("UserID", _UserID);
                        qdp.Add("CUMemberID", _cuMemberID);
                        qdp.Add("AnswerID", answer.AnswerID);
                        qdp.Add("SiteID", _siteID);
                        qdp.Add("QuestionID", answer.QuestionID);
                        qdp.Add("Answer", answer.Answer);
                        qdp.Add("EncryptionKey", answer.EncryptionKey);
                        ConnectionHelper.ExecuteQuery("dbo.CU_Proc_InsertUpdateSecurityQuestionAnswer", qdp, QueryTypeEnum.StoredProcedure);
                    }
                }
            }
        }

        /// <summary>
        /// Delete security question and answers for the user
        /// </summary>
        public void DeleteSecurityQuestionAnswers()
        {
            QueryDataParameters qdp = new QueryDataParameters();
            qdp.Add("UserID", _UserID);
            qdp.Add("SiteID", _siteID);
            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.CU_Proc_ResetSecurityQuestionAnswer", qdp, QueryTypeEnum.StoredProcedure);
        }

        #endregion data access

        #endregion methods

        #region static methods

        /// <summary>
        /// Check to see if the current user has valid security question and answers
        /// </summary>
        /// <returns></returns>
        public static bool CheckUserSecurityQuestionAnswer()
        {
            // check security question and answer
            CUFSecurityQuestionAnswers answers = new CUFSecurityQuestionAnswers();
            return answers.HasMinimumAnswers;
        }

        /// <summary>
        /// Check to see if the current user has valid security question and answers
        /// </summary>
        /// <returns></returns>
        public static bool CheckUserSecurityQuestionAnswer(UserInfo p_userInfo)
        {
            // check security question and answer
            CUFSecurityQuestionAnswers answers = new CUFSecurityQuestionAnswers(p_userInfo);
            return answers.HasMinimumAnswers;
        }

        /// <summary>
        /// Reset the security question answers for the current log in user
        /// </summary>
        public static void ResetSecurityQuestionAnswer()
        {
            ResetSecurityQuestionAnswer(null);
        }

        /// <summary>
        /// Reset security question and answer for a user
        /// </summary>
        /// <param name="p_userInfo"></param>
        public static void ResetSecurityQuestionAnswer(UserInfo p_userInfo)
        {
            CUFSecurityQuestionAnswers answers = new CUFSecurityQuestionAnswers(p_userInfo);
            answers.DeleteSecurityQuestionAnswers();
        }

        /// <summary>
        /// lock out the user
        /// </summary>
        /// <param name="p_user"></param>
        /// <param name="p_siteName"></param>
        /// <param name="p_returnUrl"></param>
        public static void LockoutUser(UserInfo p_user, string p_siteName, string p_returnUrl = null)
        {
            if ((p_user != null) && !String.IsNullOrEmpty(p_siteName))
            {
                p_user.Enabled = false;
                // do we need a new enum?
                p_user.UserAccountLockReason = UserAccountLockCode.FromEnum(UserAccountLockEnum.MaximumInvalidLogonAttemptsReached);
                if (SettingsKeyInfoProvider.GetBoolValue(p_siteName + ".CMSSendAccountUnlockEmail"))
                {
                    AuthenticationHelper.SendUnlockAccountRequest(p_user, p_siteName, "USERAUTHENTICATION", SettingsKeyInfoProvider.GetStringValue(p_siteName + ".CMSSendPasswordEmailsFrom"), null, p_returnUrl);
                }
                UserInfoProvider.SetUserInfo(p_user);
            }
        }

        /// <summary>
        /// Check the cookie
        /// </summary>
        /// <returns></returns>
        protected bool CheckCookie()
        {
            bool hasCookie = false;
            object cookie = GetCookie(_authCookieKey);
            if (cookie != null)
            {
                hasCookie = true;
            }
            return hasCookie;
        }

        /// <summary>
        /// Check the user IP
        /// </summary>
        /// <returns></returns>
        protected bool CheckIP()
        {
            bool hasIP = false;

            if (CurrentUserInfo != null)
            {
                string lastIp = CurrentUserInfo.UserLastLogonInfo.IPAddress;
                string currentIp = RequestContext.UserHostAddress;

                if (lastIp == currentIp)
                {
                    hasIP = true;
                }
            }

            return hasIP;
        }

        /// <summary>
        /// Set the cookie
        /// </summary>
        public void SetCookie()
        {
            if (CurrentUserInfo != null)
            {
                string lastIp = CurrentUserInfo.UserLastLogonInfo.IPAddress;
                string currentIp = RequestContext.UserHostAddress;
                AddCookie(_authCookieKey, currentIp);
            }
        }

        /// <summary>
        /// Get cookie
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        private static Object GetCookie(string p_key)
        {
            Object obj = null;
            if (!string.IsNullOrWhiteSpace(p_key))
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    if (HttpContext.Current.Request.Cookies.Get(p_key) != null)
                    {
                        obj = HttpContext.Current.Request.Cookies.Get(p_key);
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// Remove cookie
        /// </summary>
        /// <param name="p_key"></param>
        public static void RemoveCookie(string p_key)
        {
            if (!String.IsNullOrWhiteSpace(p_key))
            {
                if (HttpContext.Current != null && HttpContext.Current.Response != null)
                {
                    if (HttpContext.Current.Request != null)
                    {
                        if (HttpContext.Current.Request.Cookies[p_key] != null)
                        {
                            HttpContext.Current.Request.Cookies.Remove(p_key);
                            HttpCookie cookie = new HttpCookie(p_key);
                            cookie.Expires = DateTime.Now.AddDays(-1);
                            HttpContext.Current.Response.Cookies.Add(cookie);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Store data to a cookie
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_obj"></param>
        /// <param name="p_expire"></param>
        /// <param name="p_dependencyKeys"></param>
        private static void AddCookie(string p_key, object p_obj)
        {
            if (!string.IsNullOrWhiteSpace(p_key))
            {
                // Remove old cookie
                HttpContext.Current.Response.Cookies.Remove(p_key);

                // Create and add cookie
                HttpCookie cookie = new HttpCookie(p_key);

                if (p_obj != null)
                {
                    cookie.Value = p_obj.ToString();
                    cookie.Expires = DateTime.Now.AddYears(1);
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
                else
                {
                    // we want to remove it.
                    RemoveCookie(p_key);
                }
            }
        }

        #endregion static methods
    }
}