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

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUReplicateUser : CMSAbstractWebPart
    {
        public enum ActionEnum { Unknown, Replicate };

        #region "Properties"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string CUDBName
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CUDBName"), "");
            }
            set
            {
                this.SetValue("CUDBName", value);
            }
        }

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

        public long MemberIDToReplicate
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string actionString = QueryHelper.GetString("action", "").ToLower();
                if (actionString == "replicate")
                {
                    string testID = QueryHelper.GetString("MemberIDToReplicate", "0");
                    long realID = 0;
                    MemberIDToReplicate = 0;
                    if (long.TryParse(testID, out realID))
                    {
                        MemberIDToReplicate = realID;
                    }

                    testID = QueryHelper.GetString("StatementID", "0");
                    realID = 0;
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

        protected void RunAction()
        {
            if (ActionRequested != ActionEnum.Unknown && MemberIDToReplicate != 0)
            {
                int adminUserID = CurrentUser.UserID;

                StringBuilder sb = new StringBuilder();

                //get user to replicate
                UserInfo userInfo = null;
                ObjectQuery<UserSettingsInfo> userSettingsSet = UserSettingsInfoProvider.GetUserSettings()
                    .WhereEquals("CUMemberNumber", MemberIDToReplicate);
                if (userSettingsSet != null && userSettingsSet.Count<UserSettingsInfo>() > 0)
                {
                    if (userSettingsSet.Count<UserSettingsInfo>() > 1)
                    {
                        //mulitple accounts with user id
                        sb.Append("Error: mulitple accounts found matching account number<br />\n");
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
                    QueryDataParameters qdp = new QueryDataParameters();
                    qdp.Add("membernumber", MemberIDToReplicate);
                    DataSet ds = ConnectionHelper.ExecuteQuery("SELECT * FROM " + CUDBName + ".dbo.Member where MemberNumber = @MemberNumber", qdp, QueryTypeEnum.SQLQuery);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];

                        userInfo = new UserInfo();
                        userInfo.UserName = "CU_" + Convert.ToString(dr["MemberNumber"]); //userName;
                        userInfo.FullName = Convert.ToString(dr["MemName"]);
                        userInfo.Enabled = true;
                        userInfo.Email = "";
                        UserInfoProvider.SetUserInfo(userInfo);
                        UserInfoProvider.SetPassword(userInfo, "CU_PWD_ZGPJ_" + userInfo.UserName);

                        UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userInfo.UserID);
                        userSettings.SetValue("CUMemberNumber", MemberIDToReplicate);
                        UserSettingsInfoProvider.SetUserSettingsInfo(userSettings);

                        UserInfoProvider.AddUserToSite(userInfo.UserName, SiteContext.CurrentSiteName);
                    }
                    else
                    {
                        sb.Append("Error: Unable to find member information in order to create local account access<br />\n");
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
                    Response.Redirect(newRedirectURL);
                }
            }
            else
            {
                lblMsg.Text = "Unknown managment action requested to be performed.";
            }
        }
    }
}