using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFAdminCycleStatus : CMSAbstractQueryFilterControl
    {
        private const string cMostRecentString = "50 Most Recent";

        #region "Properties"

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

        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            SetupControl();

            base.OnInit(e);
        }

        /// <summary>
        /// Pre render event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Set filter settings
            if (RequestHelper.IsPostBack())
            {
                SessionHelper.SetValue("ddlAdminCycleType", ddlCycleType.SelectedValue);
                SessionHelper.SetValue("ddlAdminCycleStatus", ddlCycleStatus.SelectedValue);
                Response.Redirect(string.Format("{0}?CycleType={1}", QueryHelper.GetString("aliaspath", "xx"), ddlCycleType.SelectedValue), true);
                //SetFilter();
            }
            else
            {
                SetWhere();
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Setup the inner controls.
        /// </summary>
        private void SetupControl()
        {
            if (this.StopProcessing)
            {
                this.Visible = false;
            }
            else if (!RequestHelper.IsPostBack())
            {
                Object o = SessionHelper.GetValue("ddlAdminCycleType");
                string currentType = "";
                if (o != null)
                {
                    try
                    {
                        currentType = Convert.ToString(o);
                    }
                    catch
                    {
                        //noop
                    }
                }

                currentType = SqlHelper.EscapeLikeText(QueryHelper.GetString("CycleType", currentType));

                //populate type drop down
                String dataDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);

                DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("{0}.dbo.GetCycleTypeList", dataDB), null, QueryTypeEnum.StoredProcedure);
                if (ds.Tables.Count > 0)
                {
                    ddlCycleType.DataSource = ds.Tables[0];
                    ddlCycleType.DataValueField = "mastertype";
                    ddlCycleType.DataTextField = "mastertype";
                    ddlCycleType.DataBind();

                    ddlCycleType.Items.Insert(0, new ListItem(cMostRecentString, cMostRecentString));
                }

                ListItem li = ddlCycleType.Items.FindByValue(currentType);
                if (li != null)
                {
                    li.Selected = true;
                }

                ddlCycleStatus.SelectedValue = currentType;

                o = SessionHelper.GetValue("ddlAdminCycleStatus");
                int currentStatus = 0;
                if (o != null)
                {
                    try
                    {
                        currentStatus = Convert.ToInt16(o);
                    }
                    catch
                    {
                        //noop
                    }
                }

                //populate status drop down
                ddlCycleStatus.Items.Add(new ListItem("Any", "0"));
                ddlCycleStatus.Items.Add(new ListItem("Loading", "1"));
                ddlCycleStatus.Items.Add(new ListItem("New", "2"));
                ddlCycleStatus.Items.Add(new ListItem("Pending", "3"));
                ddlCycleStatus.Items.Add(new ListItem("Emailing", "4"));
                ddlCycleStatus.Items.Add(new ListItem("Approved", "5"));
                ddlCycleStatus.Items.Add(new ListItem("Rejected", "6"));
                ddlCycleStatus.Items.Add(new ListItem("Delete", "7"));

                li = ddlCycleStatus.Items.FindByValue(currentStatus.ToString());
                if (li != null)
                {
                    li.Selected = true;
                }
            }
        }

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetFilter()
        {
            SetWhere();

            this.OrderBy = null;

            this.RaiseOnFilterChanged();
        }

        /// <summary>
        /// Generates WHERE condition based on current selection.
        /// </summary>
        private void SetWhere()
        {
            StringBuilder where = new StringBuilder();

            string selectedType = ddlCycleType.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selectedType))
            {
                if (selectedType != cMostRecentString)
                {
                    where.Append(string.Format(" BT.MasterType = '{0}'", selectedType));
                }
                else
                {
                    this.TopN = 50;
                    this.OrderBy = "ProcessedDate desc";
                }
            }

            string selectedStatus = ddlCycleStatus.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selectedStatus) && selectedStatus != "0")
            {
                if (where.Length > 0)
                {
                    where.Append(" AND ");
                }
                where.Append(string.Format(" BS.BatchStatusID = {0}", selectedStatus));
            }

            if (where.Length > 0)
            {
                // Set where condition
                this.WhereCondition = where.ToString();
            }
        }
    }
}