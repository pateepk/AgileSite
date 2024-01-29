using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.PortalEngine.Web.UI;
using System.Data;
using System.Text;
using CMS.PortalEngine;
using CMS.Helpers;


namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class PurlPop : CMSAbstractWebPart
    {
        // Base datasource instance
        private CMSBaseDataSource mDataSourceControl = null;
        
        #region "Properties"
        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string DataSourceName
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("DataSourceName"), "");
            }
            set
            {
                this.SetValue("DataSourceName", value);
            }
        }

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectURL"), "").Trim();
            }
            set
            {
                this.SetValue("RedirectURL", value);
            }
        }

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public Boolean TestMode
        {
            get
            {
                return ValidationHelper.GetBoolean(this.GetValue("TestMode"), false);
            }
            set
            {
                this.SetValue("TestMode", value);
            }
        }

        /// <summary>
        /// Control with data source.
        /// </summary>
        public CMSBaseDataSource DataSourceControl
        {
            get
            {
                // Check if control is empty and load it with the data
                if (mDataSourceControl == null)
                {
                    if (!String.IsNullOrEmpty(DataSourceName))
                    {
                        mDataSourceControl = CMSControlsHelper.GetFilter(DataSourceName) as CMSBaseDataSource;
                    }
                }

                return mDataSourceControl;
            }
            set
            {
                mDataSourceControl = value;
            }
        }

        #endregion

        private object GetDataSource()
        {
            DataView dv = null;
            DataSet ds = DataSourceControl.DataSource as DataSet;
            if (ds != null)
            {
                dv = new DataView(ds.Tables[0], "", "", DataViewRowState.CurrentRows);
            }
            return dv;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!IsDesign)
                //if (!(ParentZone.RequiresWebPartManagement() && (ViewMode != ViewModeEnum.DashboardWidgets)))
                {
                    DataSet ds = DataSourceControl.DataSource as DataSet;
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        DataRow row = ds.Tables[0].Rows[0];
                        StringBuilder sb = new StringBuilder();
                        for (int colNumber = 0; colNumber < dt.Columns.Count; colNumber++)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append("&");
                            }
                            sb.Append(string.Format("{0}={1}", dt.Columns[colNumber].ColumnName, Server.UrlEncode(Convert.ToString(row[colNumber]))));
                        }

                        if (TestMode)
                        {
                            lblMsg.Text = string.Format("would redirect to: {0}{1}{2}", RedirectURL, RedirectURL.Contains("?") ? "&" : "?", sb.ToString());
                        }
                        else
                        {
                            Response.Redirect(string.Format("{0}{1}{2}", RedirectURL, RedirectURL.Contains("?") ? "&" : "?", sb.ToString()));
                        }
                    }
                    else
                    {
                        lblMsg.Text = "No information returned by datasource in purl pop.";
                    }
                }
            }
        }
    }
}