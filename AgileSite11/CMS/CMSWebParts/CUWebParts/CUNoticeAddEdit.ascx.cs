using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.PortalEngine.Web.UI;
using System.Data;
using CMS.Helpers;
using CMS.DataEngine;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUNoticeAddEdit : CMSAbstractWebPart
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
        #endregion

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
                    DataSet ds = ConnectionHelper.ExecuteQuery("select * from " + CUDBName + ".dbo.NoticeType where ID=@RecordKey", parameters, QueryTypeEnum.SQLQuery);
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

        protected void SaveBtn_Click(Object sender, EventArgs e)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@FriendlyName", txtFriendlyName.Text);
            parameters.Add("@TableName", txtTableName.Text);
            parameters.Add("@HTML", reHTML.Content);
            parameters.Add("@HTMLPrint", reHTMLPrint.Content);
            parameters.Add("@RecordKey", RecordKey);

            string query = string.Empty;
            if (RecordKey == 0)
            {
                query = "INSERT INTO " + CUDBName + ".dbo.NoticeType ( FriendlyName , TableName ,  HTML , HTMLprint ) VALUES  ( @FriendlyName, @TableName, @HTML, @HTMLPrint)";
                lblMessage.Text = "<font color=\"green\">Notice added.</font>";
            }
            else
            {
                query = "UPDATE " + CUDBName + ".dbo.NoticeType set FriendlyName=@FriendlyName , TableName=@TableName ,  HTML=@HTML , HTMLprint=@HTMLPrint where ID = @RecordKey";
                lblMessage.Text = "<font color=\"green\">Notice updated.</font>";
            }

            try
            {
                ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);

                if (RecordKey == 0)
                {
                    query = "select max(id) maxid from " + CUDBName + ".dbo.NoticeType";
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

        protected void CancelBtn_Click(Object sender, EventArgs e)
        {
            Response.Redirect("~/Secure/sadmin/Notice-Management");
        }
    }
}