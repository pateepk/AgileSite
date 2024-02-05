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

namespace CMSApp.CMSWebParts.CUFWebParts.RICU
{
    public partial class CUFCMemberStatementsFilter : CMSAbstractQueryFilterControl
    {
        #region "Properties"

        /// <summary>
        /// is replicating an admin
        /// </summary>
        public int IsReplicatingAdmin
        {
            get
            {
                int ret = 0;
                object o = SessionHelper.GetValue("ReplicatingAdmin");
                if (o != null)
                {
                    ret = 1;
                }

                return ret;
            }
        }

        /// <summary>
        /// Replicating admin ID
        /// </summary>
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

        /// <summary>
        /// Member ID
        /// </summary>
        public int MemberID
        {
            get
            {
                int defaultValue = -1;
                int ret = defaultValue;
                object o = MembershipContext.AuthenticatedUser.UserSettings.GetValue("CUMemberID");
				string mn = MembershipContext.AuthenticatedUser.UserName;
                if (o != null)
                {
                    ret = Convert.ToInt32(o);
                    if (ret == 0)
                    {
                        ret = -1;
                    }
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
                SetFilter();
                SessionHelper.SetValue("ddlStatementDate", ddlStatementDate.SelectedValue);
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
                BindStatementDateDropDown();
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
            //string where = "B.BatchStatusID = 5";
            string where = String.Empty;

            // Generate WHERE condition based on item selected
            string endDateString = ddlStatementDate.SelectedValue;
            if (!string.IsNullOrWhiteSpace(endDateString) && endDateString != "0")
            {
                DateTime endDate = ValidationHelper.GetDateTime(endDateString, DateTime.Now);
				//where += string.Format("CONVERT(DATE, dd.StatementEndDate) = CONVERT(DATE, '{0:yyyy-MM-dd}')", endDate);
				where += string.Format(" MONTH(dd.StatementEndDate) = MONTH(CONVERT(Date, '{0}')) AND YEAR(dd.StatementEndDate) = YEAR(CONVERT(Date, '{1}')) ", endDate, endDate);              
            }
			          

            if (!String.IsNullOrWhiteSpace(where))
            {
                //if not replicating admin only show approved batches
                //disabled for now, not going to show approved while replicating.
                //if (ReplicatingAdmin == 0)
                {
                    where += " AND b.BatchStatusID = 5";
                }
            }

            if (where != null)
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }

        /// <summary>
        /// Bind the statement date drop down
        /// </summary>
        private void BindStatementDateDropDown()
        {
            Object o = SessionHelper.GetValue("ddlStatementDate");
            DateTime? lastDateSelection = null;
            if (o != null)
            {
                try
                {
                    lastDateSelection = Convert.ToDateTime(o);
                }
                catch
                {
                    //noop
                }
            }

            DataSet ds = GetStatementDates();
            List<String> existingDates = new List<string>();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                ddlStatementDate.Items.Clear();
                bool isSelected = false;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
					DateTime endDate = Convert.ToDateTime(row["StatementDate"]);
                    string dateDisplayString = string.Format("{0:yyyy MMMM}", endDate);
                    if (!existingDates.Contains(dateDisplayString))
                    {
                        ListItem listItem = new ListItem(dateDisplayString, endDate.ToShortDateString());
                        if (endDate.Date == lastDateSelection.GetValueOrDefault().Date)
                        {
                            if (!isSelected)
                            {
                                listItem.Selected = true;
                                isSelected = true;
                            }
                        }
                        ddlStatementDate.Items.Add(listItem);
                        existingDates.Add(dateDisplayString);
                    }
                }
                ddlStatementDate.Items.Insert(1, new ListItem("ALL STATEMENTS", "0"));
            }

        }
		  
        /// <summary>
        /// Get statement dates
        /// </summary>
        /// <returns></returns>
        private DataSet GetStatementDates()
        {
            //populate drop down with available list of statements
            String dataDB = SettingsKeyInfoProvider.GetStringValue("SolimarDatabase", SiteContext.CurrentSiteID);
			
            QueryDataParameters qdp = new QueryDataParameters();
            qdp.Add("MemberID", MemberID);
			DataSet ds = ConnectionHelper.ExecuteQuery(String.Format("{0}.dbo.custom_SearchPdfsByMemberID", dataDB), qdp, QueryTypeEnum.StoredProcedure);
            return ds;
        }

    }
}