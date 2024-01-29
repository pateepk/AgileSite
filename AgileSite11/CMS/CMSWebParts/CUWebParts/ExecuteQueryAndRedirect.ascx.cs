using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.PortalEngine.Web.UI;
using CMS.GlobalHelper;
using System.Data;
using System.Text;
using CMS.SettingsProvider;
using CMS.CMSHelper;
using CMS.PortalEngine;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class ExecuteQueryAndRedirect : CMSAbstractWebPart
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

        public string Query
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("Query"), "");
            }
            set
            {
                this.SetValue("Query", value);
            }
        }

        /// <summary>
        /// Gets or sets URL to redirect to
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
                if (PortalContext.ViewMode.IsLiveSite())
                {
                    if (TestMode)
                    {
                        lblMsg.Text = string.Format("would redirect to: {0} after running query:<br />{1}", RedirectURL, Query);
                    }
                    else
                    {
                        ConnectionHelper.ExecuteQuery(Query, null, QueryTypeEnum.SQLQuery);

                        Response.Redirect(string.Format("{0}", RedirectURL));
                    }
                }
                else
                {
                    lblMsg.Text = string.Format("This text will not appear on live site.<br /> Execute Query and Redirect web part would redirect to: {0} after running query:<br />{1}", RedirectURL, Query);
                }
            }
        }
    }
}