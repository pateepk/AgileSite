﻿using System;
using System.Data;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFNotificationAddEdit : CMSAbstractWebPart
    {
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
        /// The record key
        /// </summary>
        public int RecordKey
        {
            get
            {
                int ret = 0;
                object o = ViewState["RecordKey"];
                if (o != null)
                {
                    ret = Convert.ToInt32(o);
                }
                return ret;
            }
            set
            {
                ViewState["RecordKey"] = value;
            }
        }

        /// <summary>
        /// URL to redirect to after save
        /// </summary>
        public string RedirectAfterSaveURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectAfterSaveURL"), String.Empty);
            }
            set
            {
                this.SetValue("RedirectAfterSaveURL", value);
            }
        }

        /// <summary>
        /// URL to redirect to after cancel
        /// </summary>
        public string RedirectAfterCancelURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectAfterCancelURL"), "/Secure/sadmin/Notice-Management");
            }
            set
            {
                this.SetValue("RedirectAfterCancelURL", value);
            }
        }

        #endregion "Properties"

        #region page events

        /// <summary>
        /// The page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                RecordKey = 0;
                if (QueryHelper.GetString("action", "").ToLower() == "edit")
                {
                    RecordKey = QueryHelper.GetInteger("RecordKey", 0); //Get query parameter from URL
                }

                if (RecordKey != 0)
                {
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@RecordKey", RecordKey);
                    DataSet ds = ConnectionHelper.ExecuteQuery("select * from " + CUDBName + ".dbo.NotificationType where NotificationTypeID=@RecordKey", parameters, QueryTypeEnum.SQLQuery);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 1)
                    {
                        DataRow row = ds.Tables[0].Rows[0];
                        txtFriendlyName.Text = Convert.ToString(row["FriendlyName"]);
                        txtSubject.Text = Convert.ToString(row["Subject"]);
                        reBody.Content = Convert.ToString(row["Body"]);
                        txtFromEmail.Text = Convert.ToString(row["FromEmail"]);
                        txtFromName.Text = Convert.ToString(row["FromName"]);
                        txtReplyToEmail.Text = Convert.ToString(row["ReplyToEmail"]);

                        btnSave.Text = "Save Notification Changes";
                    }
                    else
                    {
                        //no record found for key, default to add mode
                        lblMessage.Text = "Requested record was not found. Use this form to add a new record.";
                        RecordKey = 0;
                    }
                }
            }
        }

        #endregion page events

        #region general events

        /// <summary>
        /// Send test e-mail?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SendTestBtn_Click(Object sender, EventArgs e)
        {
            //save changes
            SaveBtn_Click(sender, e);

            //send test
            if (!string.IsNullOrWhiteSpace(txtTestEmail.Text))
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ForDate", DateTime.Now.Date);
                parameters.Add("@TestMode", 1);
                parameters.Add("@TestEmail", txtTestEmail.Text);
                parameters.Add("@TestName", txtTestName.Text);
                parameters.Add("@NotificationTypeID", RecordKey);

                try
                {
                    ConnectionHelper.ExecuteQuery(CUDBName + ".dbo.sproc_sendNotifications", parameters, QueryTypeEnum.StoredProcedure);
                    lblMessage.Text = "<font color=\"green\">Notification saved and test email sent.</font>";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "<font color=\"red\">Unable to send email test notification." + ex.Message + "</font>";
                }
            }
            else
            {
                lblMessage.Text = "<font color=\"red\">Notification saved. No test email was entered therefore no test email was sent.</font>";
            }
        }

        /// <summary>
        /// Save button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SaveBtn_Click(Object sender, EventArgs e)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@FriendlyName", txtFriendlyName.Text);
            parameters.Add("@Subject", txtSubject.Text);
            parameters.Add("@Body", reBody.Content);
            parameters.Add("@FromEmail", txtFromEmail.Text);
            parameters.Add("@FromName", txtFromName.Text);
            parameters.Add("@ReplyToEmail", txtReplyToEmail.Text);
            parameters.Add("@RecordKey", RecordKey);

            string query = string.Empty;
            if (RecordKey == 0)
            {
                query = "INSERT INTO " + CUDBName + ".dbo.NotificationType ( FriendlyName , Subject ,  Body, FromName, FromEmail, ReplyToEmail ) VALUES  ( @FriendlyName, @Subject, @Body, @FromName, @FromEmail, @ReplyToEmail)";
                lblMessage.Text = "<font color=\"green\">Notification added.</font>";
            }
            else
            {
                query = "UPDATE " + CUDBName + ".dbo.NotificationType set FriendlyName=@FriendlyName , Subject=@Subject ,  Body=@Body , FromName=@FromName, FromEmail=@FromEmail, ReplyToEmail=@ReplyToEmail where NotificationTypeID = @RecordKey";
                lblMessage.Text = "<font color=\"green\">Notification updated.</font>";
            }

            try
            {
                ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);

                if (RecordKey == 0)
                {
                    query = "select max(NotificationTypeid) maxid from " + CUDBName + ".dbo.NotificationType";
                    DataSet ds = ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        RecordKey = Convert.ToInt32(ds.Tables[0].Rows[0]["maxid"]);
                        btnSave.Text = "Save Notification Changes";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "<font color=\"red\">Unable to update notification." + ex.Message + "</font>";
            }
        }

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CancelBtn_Click(Object sender, EventArgs e)
        {
            //RedirectToURL(RedirectAfterCancelURL);
        }

        #endregion general events
    }
}