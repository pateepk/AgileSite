﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;



//using Telerik.Web.UI;

namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
{
    public partial class CUFCDeliveryPreferences : CMSAbstractWebPart
    {
        private bool testMode = false;

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

        #region page events

        /// <summary>
        /// The page load event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #endregion page events

        #region grid event

        /// <summary>
        /// Account grid need data source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void RGAccounts_NeedDataSource(Object sender, GridNeedDataSourceEventArgs args)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@MemberID", MemberID);

            string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", CurrentSiteID);

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

        /// <summary>
        /// Account grid data bind items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #endregion grid event

        /// <summary>
        /// validate "agree" check box
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        protected void cvAgree_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = cbAgree.Checked;
        }

        /// <summary>
        /// Save change clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbSaveChanges_Click(Object sender, EventArgs e)
        {
            if (IsVisible || !HideOnCurrentPage || PageManager.ViewMode != ViewModeEnum.Design)
            {
                if (Page.IsValid)
                {
                    try
                    {
                        #region get the replicating admin info

                        // get the replicating admin's info

                        string adminUserName = "N/A";
                        UserInfo adminInfo = null;
                        if (ReplicatingAdmin != 0)
                        {
                            adminInfo = UserInfoProvider.GetUserInfo(ReplicatingAdmin);

                            if (adminInfo != null)
                            {
                                adminUserName = adminInfo.UserName;
                            }
                        }

                        #endregion get the replicating admin info

                        #region process the new info

                        #region save the new e-mail?

                        bool loadUserData = false;
                        CurrentUserInfo currentUserInfo = AuthenticationHelper.GetCurrentUser(out loadUserData);

                        string oldEmail = currentUserInfo.Email;
                        currentUserInfo.Email = rtbEmail.Text;
                        UserInfoProvider.SetUserInfo(currentUserInfo);

                        if (rtbEmail.Text != oldEmail)
                        {
                            // log the e-mail change
                            LogCUMembershipChanges(204, SiteContext.CurrentSiteID, "Email Changed", "Email Changed", adminUserName, currentUserInfo.UserName, MemberID, null, null, null, DateTime.Now, null, null);
                        }

                        #endregion save the new e-mail?

                        StringBuilder sbt = new StringBuilder();

                        //save delivery preferences
                        foreach (GridDataItem item in rgAccounts.MasterTableView.Items)
                        {
                            #region update and log individual account changes

                            //int accountID = Convert.ToInt32(item.GetDataKeyValue("AccountID"));
                            int masterAccountID = Convert.ToInt32(item.GetDataKeyValue("MasterAccountID"));
                            bool currentIsE = Convert.ToBoolean(item.GetDataKeyValue("IsE"));
                            bool currentIsP = Convert.ToBoolean(item.GetDataKeyValue("IsP"));
                            string AccountName = Convert.ToString(item.GetDataKeyValue("AccountName"));
                            string MasterAccountNumber = Convert.ToString(item.GetDataKeyValue("MasterAccountNumber"));
                            string AccountNumber = Convert.ToString(item.GetDataKeyValue("AccountNumber"));

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

                                    string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", CurrentSiteID);

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

                                    string changeMessage = GetDeliveryPreferenceChangeText(AccountNumber, currentIsP, isP, currentIsE, isE);
                                    LogCUMembershipChanges(210, SiteContext.CurrentSiteID, "Delivery Preference changed", changeMessage, adminUserName, currentUserInfo.UserName, MemberID, null, null, null, DateTime.Now, null, null);
                                }
                            }

                            #endregion update and log individual account changes
                        }

                        cbAgree.Checked = false;

                        lblMessage.Text = "<font color=\"green\">Settings Updated</font>";

                        if (testMode)
                        {
                            lblMessage.Text = sbt.ToString();
                        }

                        #endregion process the new info
                    }
                    catch (Exception ex)
                    {
                        LogMessage(EventType.ERROR, "CUFDeliveryPreferences", "Save", ex.Message + "<br/>" + ex.StackTrace);
                        lblMessage.Text = "<font color=\"red\">There was a problem saving your changes.</font>" + ex.Message + "<br/>" + ex.StackTrace;
                    }
                }
            }
            else
            {
                lblMessage.Text = "<font color=\"red\">You changes can not be update at this time, please try again later.</font>";
            }
        }

        #region helpers

        /// <summary>
        /// log message
        /// </summary>
        /// <param name="p_eventType"></param>
        /// <param name="p_source"></param>
        /// <param name="p_eventCode"></param>
        /// <param name="p_eventDescription"></param>
        protected void LogMessage(string p_eventType, string p_source, string p_eventCode, string p_eventDescription)
        {
            EventLogProvider.LogEvent(p_eventType, p_source, p_eventCode, HttpContext.Current.Server.HtmlEncode(p_eventDescription), null,
                0, null, 0, null, null, CurrentSiteID);
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
            {
                lblMessage.Text = String.Format("<font color=\"red\">{0}</font>", message);
            }
        }

        /// <summary>
        /// Get Log discription
        /// </summary>
        /// <param name="p_accountNumber"></param>
        /// <param name="p_oldIsP"></param>
        /// <param name="p_isP"></param>
        /// <param name="p_oldIsE"></param>
        /// <param name="p_isE"></param>
        /// <returns></returns>
        private string GetDeliveryPreferenceChangeText(string p_accountNumber, bool p_oldIsP, bool p_isP, bool p_oldIsE, bool p_isE)
        {
            string desc = String.Empty;

            if (p_isP && p_oldIsP != p_isP)
            {
                desc += String.Format("{0} changed to Paper Statement", p_accountNumber);
            }

            if (p_isE && p_oldIsE != p_isE)
            {
                desc += String.Format("{0} changed to eStatement/eBill", p_accountNumber);
            }

            return desc;
        }

        #endregion helpers
    }
}