using System;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;
using Telerik.Web.UI;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFDeliveryPreferences : CMSAbstractWebPart
    {
        private bool testMode = false;

        #region "Properties"

        /// <summary>
        /// Gets setting to allow both web and Print to be displayed
        /// </summary>
        public Boolean AllowWebAndPrint
        {
            get
            {
                return ValidationHelper.GetBoolean(this.GetValue("AllowWebAndPrint"), false);
            }
            set
            {
                this.SetValue("AllowWebAndPrint", value);
            }
        }

        /// <summary>
        /// Gets MemberID
        /// </summary>
        public int MemberID
        {
            get
            {
                int defaultValue = -1;
                int ret = defaultValue;
                CurrentUserInfo currentUserInfo = MembershipContext.AuthenticatedUser;
                if (currentUserInfo != null)
                {
                    object o = currentUserInfo.UserSettings.GetValue("CUMemberID");
                    if (o != null)
                    {
                        ret = Convert.ToInt32(o);
                        if (ret == 0)
                        {
                            ret = -1;
                        }
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// gets member id of replicating admin, if applicable
        /// </summary>
        public int ReplicatingAdmin
        {
            get
            {
                int ret = 0;

                object o = SessionHelper.GetValue("ReplicatingAdmin");
                if (o != null)
                {
                    ret = (int)o;
                }
                return ret;
            }
        }

        #endregion "Properties"

        protected static void LogMessage(string eventType, string source, string eventCode, string eventDescription)
        {
            EventLogProvider.LogEvent(eventType, source, eventCode, HttpContext.Current.Server.HtmlEncode(eventDescription), null,
                0, null, 0, null, null, SiteContext.CurrentSiteID);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                bool loadUserData = false;
                CurrentUserInfo currentUserInfo = AuthenticationHelper.GetCurrentUser(out loadUserData);
                if (currentUserInfo != null && !String.IsNullOrWhiteSpace(currentUserInfo.Email))
                {
                    rtbEmail.Text = currentUserInfo.Email;
                }
            }
        }

        protected void RGAccounts_NeedDataSource(Object sender, GridNeedDataSourceEventArgs args)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@MemberID", MemberID);

            string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT A.*, MAC.MasterAccountNumber ");
            sql.Append(string.Format(" FROM {0}.dbo.Account A", statementDB));
            sql.Append(string.Format(" INNER JOIN {0}.dbo.MasterAccount MAC ON MAC.MasterAccountID = A.MasterAccountID", statementDB));
            sql.Append(string.Format(" INNER JOIN {0}.dbo.MemberToAccount MA on MA.AccountID = A.AccountID", statementDB));
            sql.Append(" WHERE MA.MemberID = @MemberID");
            sql.Append(" AND A.IsMasterAccount = 1");
            sql.Append(" Order By A.AccountName");
            //lblMessage.Text = string.Format("memberid:{0} sql:{1}", MemberID, sql.ToString());

            DataSet ds = ConnectionHelper.ExecuteQuery(sql.ToString(), parameters, QueryTypeEnum.SQLQuery);

            if (ds != null && ds.Tables.Count > 0)
            {
                rgAccounts.DataSource = ds.Tables[0];
            }
        }

        protected void RGAccounts_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem item = e.Item as GridDataItem;

                RadioButtonList rblDeliveryPreferenceList = (RadioButtonList)item["SelectionColumn"].FindControl("rblDeliveryPreferenceList");
                if (rblDeliveryPreferenceList != null)
                {
                    if (!AllowWebAndPrint)
                    {
                        ListItem webAndPrintItem = rblDeliveryPreferenceList.Items.FindByValue("2");
                        if (webAndPrintItem != null)
                        {
                            rblDeliveryPreferenceList.Items.Remove(webAndPrintItem);
                        }
                    }

                    Boolean isE = Convert.ToBoolean(item.GetDataKeyValue("IsE"));
                    Boolean isP = Convert.ToBoolean(item.GetDataKeyValue("IsP"));

                    if (isE && isP && AllowWebAndPrint)
                    {
                        rblDeliveryPreferenceList.SelectedValue = "2";
                    }
                    else if (isP)
                    {
                        rblDeliveryPreferenceList.SelectedValue = "1";
                    }
                    else if (isE)
                    {
                        rblDeliveryPreferenceList.SelectedValue = "0";
                    }
                }
            }
        }

        protected void cvAgree_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = cbAgree.Checked;
        }

        protected void rbSaveChanges_Click(Object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if ((PageManager.ViewMode == ViewModeEnum.Design) || (HideOnCurrentPage) || (!IsVisible))
                {
                    // Do not process
                }
                else
                {
                    try
                    {
                        //save email
                        bool loadUserData = false;
                        CurrentUserInfo currentUserInfo = AuthenticationHelper.GetCurrentUser(out loadUserData);
                        currentUserInfo.Email = rtbEmail.Text;
                        UserInfoProvider.SetUserInfo(currentUserInfo);

                        StringBuilder sbt = new StringBuilder();

                        //save delivery preferences
                        foreach (GridDataItem item in rgAccounts.MasterTableView.Items)
                        {
                            //int accountID = Convert.ToInt32(item.GetDataKeyValue("AccountID"));
                            int masterAccountID = Convert.ToInt32(item.GetDataKeyValue("MasterAccountID"));
                            bool currentIsE = Convert.ToBoolean(item.GetDataKeyValue("IsE"));
                            bool currentIsP = Convert.ToBoolean(item.GetDataKeyValue("IsP"));

                            RadioButtonList rblDeliveryPreferenceList = (RadioButtonList)item["SelectionColumn"].FindControl("rblDeliveryPreferenceList");
                            if (rblDeliveryPreferenceList != null)
                            {
                                bool isE = false, isP = false;
                                switch (rblDeliveryPreferenceList.SelectedValue)
                                {
                                    case "0":
                                        isE = true;
                                        break;

                                    case "1":
                                        isP = true;
                                        break;

                                    case "2":
                                        if (AllowWebAndPrint)
                                        {
                                            isE = true;
                                        }
                                        isP = true;
                                        break;
                                }

                                if (testMode)
                                {
                                    sbt.AppendFormat("MasterAccountID:{0}, IsE(c:n):{1}:{2}, IsP(c:n):{3}:{4} <br/>", masterAccountID, currentIsE, isE, currentIsP, isP);
                                }

                                if (currentIsE != isE || currentIsP != isP)
                                {
                                    //there was a change
                                    QueryDataParameters parameters = new QueryDataParameters();
                                    parameters.Add("@MasterAccountID", masterAccountID);
                                    parameters.Add("@MemberID", MemberID);
                                    parameters.Add("@IsE", isE);
                                    parameters.Add("@IsP", isP);

                                    string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);

                                    StringBuilder sql = new StringBuilder();
                                    sql.Append("Update A");
                                    sql.Append(" SET IsE = @IsE, ");
                                    sql.Append(" IsP = @IsP ");
                                    sql.Append(string.Format(" FROM {0}.dbo.Account A", statementDB));
                                    sql.Append(string.Format(" INNER JOIN {0}.dbo.MemberToAccount MA on A.AccountID = MA.AccountID ", statementDB));
                                    sql.Append(" WHERE A.MasterAccountID = @MasterAccountID and MA.MemberID = @MemberID");

                                    //lblMessage.Text = string.Format("accountid:{0} sql:{1}", accountID, sql.ToString());
                                    //do Update
                                    ConnectionHelper.ExecuteNonQuery(sql.ToString(), parameters, QueryTypeEnum.SQLQuery);

                                    //log change
                                    ConnectionHelper.ExecuteNonQuery(string.Format("{0}.dbo.sproc_LogDeliveryMethodChange", statementDB), parameters, QueryTypeEnum.StoredProcedure);
                                }
                            }
                        }

                        cbAgree.Checked = false;

                        lblMessage.Text = "<font color=\"green\">Settings Updated</font>";

                        if (testMode)
                        {
                            lblMessage.Text = sbt.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage(EventType.ERROR, "CUFDeliveryPreferences", "Save", ex.Message + "<br/>" + ex.StackTrace);
                        lblMessage.Text = "<font color=\"red\">There was a problem saving your changes.</font>" + ex.Message + "<br/>" + ex.StackTrace;
                    }
                }
            }
        }
    }
}