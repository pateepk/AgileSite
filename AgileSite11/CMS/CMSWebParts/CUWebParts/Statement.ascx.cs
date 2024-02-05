using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using CMS.PortalControls;
//using CMS.GlobalHelper;
//using CMS.SettingsProvider;
using CMS.Helpers;
//using CMS.ExtendedControls;
//using CMS.FormControls;
using CMS.DataEngine;
using CMS.SiteProvider;

using CMS.PortalEngine.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;

using Telerik.Web.UI;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class Statement : CMSAbstractWebPart
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
        public string CheckImagePath
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CheckImagePath"), "");
            }
            set
            {
                this.SetValue("CheckImagePath", value);
            }
        }

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string CheckPageURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CheckPageURL"), "");
            }
            set
            {
                this.SetValue("CheckPageURL", value);
            }
        }

        protected string StatementID
        {
            get
            {
                string ret = string.Empty;
                object o = ViewState["StatementID"];
                if (o != null)
                {
                    ret = (string)o;
                }
                return ret;
            }

            set
            {
                ViewState["StatementID"] = value;
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
            bool bindControl = false;

            // Check whether postback was executed from current transformation item
            if (RequestHelper.IsPostBack())
            {
                // Indicates whether postback was fired from current control
 

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
            else
            {
                if (Request.QueryString["StatementID"] != null)
                {
                    this.StatementID = Request.QueryString["StatementID"];
                }
            }
            if (bindControl)
            {
            }
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
                this.Visible = false;
            }
            else
            {
                //bind data
                DataRow summaryRow = ((DataSet)ds).Tables[0].Rows[0];
                lblDepositsAndCredits.Text = string.Format("{0:C}", Convert.ToDecimal(summaryRow["PeriodTotalDepositAmount"]));
                lblWithdrawlsAndDebits.Text = string.Format("{0:C}", Convert.ToDecimal(summaryRow["PeriodTotalWithdrawalAmount"]));
                lblAvailableBalance.Text = string.Format("{0:C}", Convert.ToDecimal(summaryRow["EndingBalance"]));

                lblStatementDate.Text = string.Format("{0:MM/dd/yyyy} - {1:MM/dd/yyyy}", Convert.ToDateTime(summaryRow["StartDate"]), Convert.ToDateTime(summaryRow["EndDate"]));

                StringBuilder sb = new StringBuilder();
                bool addBr = false;
                for (int i = 1; i <= 5; i++)
                {
                    if (summaryRow["Address" + i] != DBNull.Value)
                    {
                        string tmp = Convert.ToString(summaryRow["Address" + i]);
                        if (!string.IsNullOrWhiteSpace(tmp))
                        {
                            if (addBr)
                            {
                                sb.Append("<br/>");
                            }
                            else
                            {
                                addBr = true;
                            }
                            sb.Append(tmp);
                        }
                    }
                }
                lblAddress.Text = sb.ToString();

                lblHeaderMessage.Text = Convert.ToString(summaryRow["HeaderMessage"]);
            }
        }

        protected void rgStatement_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            if (this.DataSourceControl != null)
            {
                // Get data from datasource
                object ds = this.DataSourceControl.DataSource;

                // Check whether data exist
                if ((!DataHelper.DataSourceIsEmpty(ds)) && (!binded))
                {
                    rgStatement.DataSource = ((DataSet)this.DataSourceControl.DataSource).Tables[1];
                }
            }
        }

        protected void rgStatement_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem item = e.Item as GridDataItem;

                Label label = item["Description"].FindControl("lblDescription") as Label;
                HyperLink hyperlink = item["Description"].FindControl("hlDescription") as HyperLink;

                string transactionType = Convert.ToString(item.GetDataKeyValue("Type"));
                string description = Convert.ToString(item.GetDataKeyValue("Description"));
                string transactionID = Convert.ToString(item.GetDataKeyValue("ID"));
                if (transactionType == "CK")
                {
                    hyperlink.Visible = true;

                    string joinChar = "?";
                    if (CheckPageURL.Contains("?"))
                    {
                        joinChar = "&";
                    }
                    hyperlink.NavigateUrl = string.Format("{0}{1}TransactionID={2}", this.CheckPageURL, joinChar, transactionID);

                    string checkDescription = description;
                    if (!string.IsNullOrWhiteSpace(this.CheckImagePath))
                    {
                        checkDescription = string.Format("<img src=\"{0}\" alt=\"check image\"/> {1}", this.CheckImagePath, description);
                    }
                    hyperlink.Text = checkDescription;
                }
                else
                {
                    label.Visible = true;
                    label.Text = description;
                }
            }
        }

        protected void rgStatement_RowCommand(object sender, GridCommandEventArgs e)
        {
        }
    }
}