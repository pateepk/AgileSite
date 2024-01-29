using System;
using System.Data;
using System.Collections.Generic;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFNoticeAddEdit : CMSAbstractWebPart
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
        /// The selected record key?
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
        /// Page load event
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
                    string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@RecordKey", RecordKey);
                    DataSet ds = ConnectionHelper.ExecuteQuery("select * from " + statementDB + ".dbo.NoticeType where NoticeTypeID=@RecordKey", parameters, QueryTypeEnum.SQLQuery);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 1)
                    {
                        DataRow row = ds.Tables[0].Rows[0];
                        txtFriendlyName.Text = Convert.ToString(row["FriendlyName"]);
                        txtTableName.Text = Convert.ToString(row["TableName"]);
                        reHTML.Content = Convert.ToString(row["HTML"]);
                        reHTMLPrint.Content = Convert.ToString(row["HTMLPrint"]);
                        btnSave.Text = "Save Notice Changes";
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
        /// Save button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SaveBtn_Click(Object sender, EventArgs e)
        {
            string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@FriendlyName", txtFriendlyName.Text);
            parameters.Add("@TableName", txtTableName.Text);
            parameters.Add("@HTML", reHTML.Content);
            parameters.Add("@HTMLPrint", reHTMLPrint.Content);
            parameters.Add("@RecordKey", RecordKey);

            string query = string.Empty;
            if (RecordKey == 0)
            {
                query = "INSERT INTO " + statementDB + ".dbo.NoticeType ( FriendlyName , TableName ,  HTML , HTMLprint ) VALUES  ( @FriendlyName, @TableName, @HTML, @HTMLPrint)";
                lblMessage.Text = "<font color=\"green\">Notice added.</font>";
            }
            else
            {
                query = "UPDATE " + statementDB + ".dbo.NoticeType set FriendlyName=@FriendlyName , TableName=@TableName ,  HTML=@HTML , HTMLprint=@HTMLPrint where NoticeTypeID = @RecordKey";
                lblMessage.Text = "<font color=\"green\">Notice updated.</font>";
            }

            try
            {
                ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);

                if (RecordKey == 0)
                {
                    query = "select max(noticeTypeid) maxid from " + statementDB + ".dbo.NoticeType";
                    DataSet ds = ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        RecordKey = Convert.ToInt32(ds.Tables[0].Rows[0]["maxid"]);
                        btnSave.Text = "Save Notice Changes";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "<font color=\"red\">Unable to update notice." + ex.Message + "</font>";
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
            Response.Redirect("~/Secure/sadmin/Notice-Management");
        }

        #endregion general events
    }
}