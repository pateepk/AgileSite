//using System;
//using System.Web.UI.WebControls;
//using CMS.Controls;
//using CMS.ExtendedControls;
//using CMS.Helpers;
//using CMS.PortalControls;
//using Telerik.Web.UI;

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;


namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CashAccountGrid : CMSAbstractWebPart
    {
        #region "Variables"

        // Indicates whether control was binded
        private bool binded = false;

        // Base datasource instance
        private CMSBaseDataSource mDataSourceControl = null;

        #endregion "Variables"

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
        public string StatementPageURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("StatementPageURL"), "");
            }
            set
            {
                this.SetValue("StatementPageURL", value);
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
                if (this.mDataSourceControl == null)
                {
                    if (!String.IsNullOrEmpty(this.DataSourceName))
                    {
                        this.mDataSourceControl = CMSControlsHelper.GetFilter(this.DataSourceName) as CMSBaseDataSource;
                    }
                }

                return this.mDataSourceControl;
            }
            set
            {
                this.mDataSourceControl = value;
            }
        }

        #endregion "Properties"

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check whether postback was executed from current transformation item
            if (RequestHelper.IsPostBack())
            {
                // Indicates whether postback was fired from current control
                bool bindControl = false;

                // Check event target value and callback parameter value
                string eventTarget = ValidationHelper.GetString(this.Request.Form["__EVENTTARGET"], String.Empty);
                string callbackParam = ValidationHelper.GetString(this.Request.Form["__CALLBACKPARAM"], String.Empty);
                if (eventTarget.StartsWith(this.UniqueID) || callbackParam.StartsWith(this.UniqueID) || eventTarget.EndsWith(ContextMenu.CONTEXT_MENU_SUFFIX))
                {
                    bindControl = true;
                }
                // Check whether request key contains some control assigned to current control
                else
                {
                    foreach (string key in this.Request.Form.Keys)
                    {
                        if ((key != null) && key.StartsWith(this.UniqueID))
                        {
                            bindControl = true;
                            break;
                        }
                    }
                }
            }

            //base.OnLoad(e);
        }

        /// <summary>
        /// OnPreRender override.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Datasource data
            object ds = null;

            // Set transformations if data source is not empty
            if (this.DataSourceControl != null)
            {
                // Get data from datasource
                ds = this.DataSourceControl.DataSource;

                // Check whether data exist
                if ((!DataHelper.DataSourceIsEmpty(ds)) && (!binded))
                {
                    // Initilaize related data if provided
                    if (this.DataSourceControl.RelatedData != null)
                    {
                        this.RelatedData = this.DataSourceControl.RelatedData;
                    }
                }
            }

            base.OnPreRender(e);

            // Hide control for zero rows
            if (((this.DataSourceControl == null) || (DataHelper.DataSourceIsEmpty(ds))))
            {
                //this.Visible = false;
            }
        }

        protected void rgCashAccounts_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            if (this.DataSourceControl != null)
            {
                rgCashAccounts.DataSource = this.DataSourceControl.DataSource;
            }
        }

        protected void rgCashAccounts_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem item = e.Item as GridDataItem;

                HyperLink hyperLink = item.FindControl("hlView") as HyperLink;
                if (hyperLink != null)
                {
                    string joinChar = "?";
                    if (StatementPageURL.Contains("?"))
                    {
                        joinChar = "&";
                    }

                    string shareAccountID = Convert.ToString(item.GetDataKeyValue("ShareAccountID"));
                    hyperLink.NavigateUrl = string.Format("{0}{1}ShareAccountID={2}", StatementPageURL, joinChar, shareAccountID);
                }
            }
        }

        protected void rgCashAccounts_RowCommand(object sender, GridCommandEventArgs e)
        {
        }
    }
}