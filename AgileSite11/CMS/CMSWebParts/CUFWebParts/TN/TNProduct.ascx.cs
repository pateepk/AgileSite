using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using System;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFAdminCycleStatus : CMSAbstractQueryFilterControl
    {
        protected int nodeID;

        private const string cMostRecentString = "--Select Sub Category--";

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
                Response.Redirect(string.Format("{0}?NodeID={1}&sort={2}", QueryHelper.GetString("aliaspath", "xx"), ddlCycleType.SelectedValue, ddlCycleStatus.SelectedValue), true);
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
                String dataDB = "AS11_Production_TNN_clone";
                QueryDataParameters qdp = new QueryDataParameters();
                //nodeID = ValidationHelper.GetInteger(Form.GetFieldValue("NodeID"), 0)
                nodeID = ValidationHelper.GetInteger(DocumentContext.CurrentDocument.NodeID, 0);
                ///nodeID = 1458;

                qdp.Add("NodeID", nodeID);
                DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("{0}.dbo.GetCycleTypeList", dataDB), qdp, QueryTypeEnum.StoredProcedure, true);

                /// return ds;

                if (ds.Tables.Count > 0)
                {
                    ddlCycleType.DataSource = ds.Tables[0];
                    ddlCycleType.DataValueField = "nodeid";
                    ddlCycleType.DataTextField = "nodename";
                    ddlCycleType.DataBind();

                    ddlCycleType.Items.Insert(0, new ListItem(cMostRecentString, "0"));
                }

                ListItem li = ddlCycleType.Items.FindByValue(currentType);
                if (li != null)
                {
                    li.Selected = true;
                }

                ddlCycleStatus.SelectedValue = "0";

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
                ddlCycleStatus.Items.Add(new ListItem("Date Added", "0"));
                ddlCycleStatus.Items.Add(new ListItem("SKU", "1"));
                ddlCycleStatus.Items.Add(new ListItem("Title", "7"));
                ddlCycleStatus.Items.Add(new ListItem("Production Year (Newest)", "3"));
                //ddlCycleStatus.Items.Add(new ListItem("Production Year (Oldest)", "6"));
                ddlCycleStatus.Items.Add(new ListItem("Length (Shortest)", "5"));
                //ddlCycleStatus.Items.Add(new ListItem("Length (Longest)", "4"));

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
                    // where.Append(string.Format(" BT.MasterType = '{0}'", selectedType));
                }
                else
                {
                    this.TopN = 50;
                    this.OrderBy = "NodeName Asc";
                }
            }

            string selectedStatus = ddlCycleStatus.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selectedStatus) && selectedStatus != "0")
            {
                if (where.Length > 0)
                {
                    where.Append(" AND ");
                }
                where.Append(string.Format(" NodeParentID = {0}", selectedStatus));
            }

            if (where.Length > 0)
            {
                // Set where condition
                this.WhereCondition = where.ToString();
            }
        }
    }
}