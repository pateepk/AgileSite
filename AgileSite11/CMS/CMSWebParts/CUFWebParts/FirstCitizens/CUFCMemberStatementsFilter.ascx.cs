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

namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
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
                SetFilter();
                SessionHelper.SetValue("ddlStatementDate", ddlStatementDate.SelectedValue);
                SessionHelper.SetValue("ddlAccountSelection", ddlAccounts.SelectedValue);
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
                BindAccountDropDown();
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
                //where = string.Format("CONVERT(DATE, IsNull(T.[EndDate], T.StartDate)) = CONVERT(DATE, '{0:yyyy-MM-dd}')", endDate);
                where += string.Format("MONTH(ISNull(T.[EndDate], T.StartDate)) = MONTH(CONVERT(Date, '{0}')) AND YEAR(ISNull(T.[EndDate], T.StartDate)) = YEAR(CONVERT(Date, '{1}'))", endDate, endDate);
            }

            string accountId = ddlAccounts.SelectedValue;
            if (!String.IsNullOrWhiteSpace(accountId) && accountId != "0")
            {
                if (!String.IsNullOrWhiteSpace(where))
                {
                    where += " AND ";
                }
                where += string.Format(" T.AccountID = {0}", accountId);
            }

            if (!String.IsNullOrWhiteSpace(where))
            {
                //if not replicating admin only show approved batches
                //disabled for now, not going to show approved while replicating.
                //if (ReplicatingAdmin == 0)
                {
                    where += " AND T.BatchStatusID = 5";
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
                    DateTime endDate = Convert.ToDateTime(row["EndDate"]);
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
        /// Bind account selection drop down
        /// </summary>
        private void BindAccountDropDown()
        {
            Object o = SessionHelper.GetValue("ddlAccountSelection");
            int? lastAccountSelection = null;
            if (o != null)
            {
                try
                {
                    lastAccountSelection = Convert.ToInt32(o);
                }
                catch
                {
                    //noop
                }
            }

            DataSet ds = GetMemberAccounts();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                ddlAccounts.Items.Clear();
                bool isSelected = false;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int accountID = Convert.ToInt32(row["MasterAccount"]);
                    string displayAccountName = Convert.ToString(row["AccountNames"]);
                    ListItem listItem = new ListItem(displayAccountName, accountID.ToString());
                    if (lastAccountSelection.GetValueOrDefault(0) == accountID)
                    {
                        if (!isSelected)
                        {
                            listItem.Selected = true;
                            isSelected = true;
                        }
                    }
                    ddlAccounts.Items.Add(listItem);
                }
                ddlAccounts.Items.Insert(0, new ListItem("ALL ACCOUNTS", "0"));
            }
        }

        /// <summary>
        /// Get member accounts
        /// </summary>
        /// <returns></returns>
        private DataSet GetMemberAccounts()
        {
            //populate drop down with available list of statements
            String dataDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
            QueryDataParameters qdp = new QueryDataParameters();
            qdp.Add("MemberID", MemberID);
            qdp.Add("CombineAccount", false);
            qdp.Add("viewOverride", false);
            DataSet ds = ConnectionHelper.ExecuteQuery(String.Format("{0}.dbo.sproc_getMemberAccountFilter", dataDB), qdp, QueryTypeEnum.StoredProcedure);
            return ds;
        }

        /// <summary>
        /// Get statement date
        /// </summary>
        /// <returns></returns>
        private DataSet GetStatementDates()
        {
            //populate drop down with available list of statements
            String dataDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);

            DateTime oldestViewableDate = DateTime.Now.Date.AddYears(-2);

            // This control is only for the member view page. per spec this is only limited to 2 years
            // when replicated as admin.
            //if (IsReplicatingAdmin == 1)
            //{
            //    oldestViewableDate = DateTime.Now.Date.AddYears(-7);
            //}

            QueryDataParameters qdp = new QueryDataParameters();
            qdp.Add("MemberID", MemberID);
            qdp.Add("OldestViewableDate", oldestViewableDate);
            qdp.Add("viewOverride", false);
            DataSet ds = ConnectionHelper.ExecuteQuery(String.Format("{0}.dbo.sproc_getStatementViewDates", dataDB), qdp, QueryTypeEnum.StoredProcedure);
            return ds;
        }

        /// <summary>
        /// Get fully qualified table name for the statement database
        /// </summary>
        /// <param name="p_tableName"></param>
        /// <returns></returns>
        private string GetQualifiedTableName(string p_tableName)
        {
            string qtableName = String.Empty;
            string dataDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
            qtableName = String.Format("{0}.dbo.{1}", dataDB, p_tableName);
            return qtableName;
        }
    }
}