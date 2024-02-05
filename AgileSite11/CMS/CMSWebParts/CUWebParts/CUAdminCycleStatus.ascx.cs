//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using CMS.Controls;
//using CMS.GlobalHelper;
//using CMS.Helpers;

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

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUAdminCycleStatus : CMSAbstractQueryFilterControl
    {
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
        #endregion

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
                SessionHelper.SetValue("ddlAdminCycleStatus", ddlCycleStatus.SelectedValue);
                SetFilter();
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
                Object o = SessionHelper.GetValue("ddlAdminCycleStatus");
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

                //populate drop down
                ddlCycleStatus.Items.Add(new ListItem("Any", "0"));
                ddlCycleStatus.Items.Add(new ListItem("Loading", "1"));
                ddlCycleStatus.Items.Add(new ListItem("New", "2"));
                ddlCycleStatus.Items.Add(new ListItem("Pending", "3"));
                ddlCycleStatus.Items.Add(new ListItem("Emailing", "4"));
                ddlCycleStatus.Items.Add(new ListItem("Approved", "5"));
                ddlCycleStatus.Items.Add(new ListItem("Rejected", "6"));
                ddlCycleStatus.Items.Add(new ListItem("Delete", "7"));

                ListItem li = ddlCycleStatus.Items.FindByValue(currentStatus.ToString());
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
            string where = "";

            // Generate WHERE condition based on team selected
            string selectedStatus = ddlCycleStatus.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selectedStatus) && selectedStatus != "0")
            {
                where = string.Format(" BS.ID = {0}", selectedStatus);
            }

            if (where != null)
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }
    }
}