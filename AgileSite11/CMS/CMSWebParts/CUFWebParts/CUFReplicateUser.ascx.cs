using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFReplicateUser : CMSAbstractWebPart
    {
        public enum ActionEnum { Unknown, Replicate };

        #region "Properties"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectURL"), "");
            }
            set
            {
                this.SetValue("RedirectURL", value);
            }
        }

        public int MemberIDToReplicate
        {
            get;
            set;
        }

        public long StatementID
        {
            get;
            set;
        }

        public int TaxYear
        {
            get;
            set;
        }

        public string ReplicatingFrom
        {
            get;
            set;
        }

        public ActionEnum ActionRequested
        {
            get
            {
                int ret = 0;
                object o = ViewState["ActionRequested"];
                if (o != null)
                {
                    ret = Convert.ToInt32(o);
                }
                return (ActionEnum)ret;
            }
            set
            {
                ViewState["ActionRequested"] = (int)value;
            }
        }

        #endregion "Properties"

        #region page events

        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string actionString = QueryHelper.GetString("action", "").ToLower();
                if (actionString == "replicate")
                {
                    MemberIDToReplicate = QueryHelper.GetInteger("MemberIDToReplicate", 0);

                    string testID = QueryHelper.GetString("StatementID", "0");
                    long realID = 0;
                    StatementID = 0;
                    if (long.TryParse(testID, out realID))
                    {
                        if (realID > 0)
                        {
                            StatementID = realID;
                        }
                    }

                    testID = QueryHelper.GetString("TaxYear", "0");
                    int realInt = 0;
                    TaxYear = 0;
                    if (int.TryParse(testID, out realInt))
                    {
                        if (realInt > 0)
                        {
                            TaxYear = realInt;
                        }
                    }

                    ReplicatingFrom = QueryHelper.GetString("ReplicatingFrom", "");

                    if (MemberIDToReplicate != 0)
                    {
                        switch (actionString)
                        {
                            case "replicate":
                                ActionRequested = ActionEnum.Replicate;
                                break;
                        }
                        RunAction();
                    }
                    else
                    {
                        lblMsg.Text = "MemberIDToReplicate was not passed or was 0.";
                    }
                }
            }
        }

        #endregion page events

        #region methods

        /// <summary>
        /// Run action
        /// </summary>
        protected void RunAction()
        {
            if (ActionRequested != ActionEnum.Unknown && MemberIDToReplicate != 0)
            {
                int adminUserID = CurrentUser.UserID;
                string adminUserName = CurrentUser.UserName;

                StringBuilder sb = new StringBuilder();

                //get user to replicate
                UserInfo userInfo = null;
                ObjectQuery<UserSettingsInfo> userSettingsSet = UserSettingsInfoProvider.GetUserSettings()
                    .WhereEquals("CUMemberID", MemberIDToReplicate);
                if (userSettingsSet != null && userSettingsSet.Count<UserSettingsInfo>() > 0)
                {
                    if (userSettingsSet.Count<UserSettingsInfo>() > 1)
                    {
                        //multiple accounts with user id
                        sb.Append("Error: multiple accounts found matching account number<br />\n");
                    }
                    else
                    {
                        int userId = userSettingsSet.First<UserSettingsInfo>().UserSettingsUserID;
                        userInfo = UserInfoProvider.GetUserInfo(userId);
                    }
                }

                if (userInfo == null && sb.Length == 0)
                {
                    //find member in statement database
                    string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
                    QueryDataParameters qdp = new QueryDataParameters();
                    qdp.Add("MemberID", MemberIDToReplicate);
                    DataSet ds = ConnectionHelper.ExecuteQuery("SELECT * FROM " + statementDB + ".dbo.Member where MemberID = @MemberID", qdp, QueryTypeEnum.SQLQuery);
                    //string sql = string.Format("SELECT * FROM {0}.dbo.Member where MemberID = {1}\n",CUDBName, MemberIDToReplicate);
                    //sb.Append(sql);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];

                        String memberNumber = Convert.ToString(dr["MemberNumber"]);

                        userInfo = new UserInfo();

                        //SPECIAL CASE FOR RICU SO THERE IS NO OVERLAP. FUTURE SITES MAY NEED TO DO THE SAME
                        if (SiteContext.CurrentSite.SiteName == "RICU")
                        {
                            userInfo.UserName = "CU_" + SiteContext.CurrentSite.SiteName + "_" + memberNumber; //userName - add site name to make distinct;
                        }
                        else
                        {
                            userInfo.UserName = "CU_" + memberNumber; //userName - add site name to make distinct;
                        }

                        userInfo.FullName = Convert.ToString(dr["Name"]);
                        userInfo.Enabled = true;
                        userInfo.Email = "";
                        UserInfoProvider.SetUserInfo(userInfo);
                        UserInfoProvider.SetPassword(userInfo, "CU_PWD_ZGPJ_" + userInfo.UserName);

                        UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userInfo.UserID);
                        userSettings.SetValue("CUMemberNumber", memberNumber);
                        userSettings.SetValue("CUMemberID", MemberIDToReplicate);
                        UserSettingsInfoProvider.SetUserSettingsInfo(userSettings);

                        UserInfoProvider.AddUserToSite(userInfo.UserName, SiteContext.CurrentSiteName);
                    }
                    else
                    {
                        sb.Append("Error: Unable to find member information in order to create local account access<br />\n");
                        //.Append("SELECT * FROM " + CUDBName + ".dbo.Member where MemberNumber = ").Append(MemberNumberToReplicate);
                    }
                }

                if (sb.Length > 0)
                {
                    //error
                    lblMsg.Text = sb.ToString();
                }
                else if (userInfo == null)
                {
                    lblMsg.Text = "Did not find user matching memberid";
                }
                else
                {
                    //log out admin
                    //AuthenticationHelper.LogoutUser();

                    //log in replicated user
                    AuthenticationHelper.AuthenticateUser(userInfo.UserName, false);

                    //add admin to session ReplicatingAdmin
                    SessionHelper.SetValue("ReplicatingAdmin", adminUserID);

                    SessionHelper.SetValue("ReplicatingFrom", ReplicatingFrom);

                    if (TaxYear > 0)
                    {
                        SessionHelper.SetValue("ddlTaxYear", string.Format("1/1/{0:0000}", TaxYear));
                    }

                    // Update the log table
                    LogCUMembershipChanges(209,
                        SiteContext.CurrentSite.SiteID,
                        "Replicate User",
                        userInfo.FullName,
                        adminUserName,
                        userInfo.UserName,
                        MemberIDToReplicate,
                        null,
                        null,
                        null,
                        DateTime.Now,
                        String.Empty,
                        String.Empty);

                    #region eric's code

                    //Update Table Log
                    //string query = string.Empty;
                    ////string statementDB = SettingsKeyInfoProvider.GetStringValue("StatementDatabase", CurrentSiteID)
                    //QueryDataParameters parameters = new QueryDataParameters();
                    //parameters.Add("@LogShortD", "Replicate User");
                    //parameters.Add("@LogD", userInfo.FullName);
                    //parameters.Add("@LogUser", Convert.ToString(adminUserID));
                    //parameters.Add("@LogReplicate", userInfo.UserName);
                    //parameters.Add("@CUFMemberID", MemberIDToReplicate);

                    //query = "INSERT INTO customtable_CU_MemberManagementLog ( LogTypeID, SiteID, ShortDescription, Description, UserName, UpdatedUser, CUFMemberID ) VALUES  ( 209, 1, @LogShortD, @LogD, @LogUser, @LogReplicate, @CUFMemberID)";
                    //ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);

                    #endregion eric's code

                    //redirect to replicated user initial page
                    string newRedirectURL = RedirectURL;
                    if (StatementID > 0)
                    {
                        string joinChar = "?";
                        if (newRedirectURL.Contains("?"))
                        {
                            newRedirectURL += "&";
                        }
                        newRedirectURL = string.Format("{0}{1}StatementID={2}", newRedirectURL, joinChar, StatementID);
                    }
                    //lblMsg.Text = newRedirectURL;
                    //lblMsg.Text = " here we are";
                    Response.Redirect(newRedirectURL);
                }
            }
            else
            {
                lblMsg.Text = "Unknown managment action requested to be performed.";
            }
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
            //    lblMsg.Text = message;
            //}
        }

        #endregion methods
    }
}