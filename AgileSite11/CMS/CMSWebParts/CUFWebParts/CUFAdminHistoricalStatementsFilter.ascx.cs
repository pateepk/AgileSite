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
    public partial class CUFAdminHistoricalStatementsFilter : CMSAbstractQueryFilterControl
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
            SetWhere();

            SetupControl();

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (Page.IsPostBack)
            {
                SessionHelper.SetValue("AdminMemberSearch", txtMemberNumber.Text.Trim());
                SessionHelper.SetValue("StatementDateSearch", radDatetime.SelectedDate);
                SessionHelper.SetValue("ddlStatementDate", ddlStatementDate.SelectedValue);
                SessionHelper.SetValue("ddlSearchType", ddlSearchType.SelectedValue);
            }
            else
            {
                string test = Convert.ToString(SessionHelper.GetValue("AdminMemberSearch"));
                if (!String.IsNullOrWhiteSpace(test))
                {
                    //txtMemberNumber.Text = test;
                }
            }

            base.OnLoad(e);
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
                lblMsg.Text = String.Empty;
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
            this.WhereCondition = "1=0";

            StringBuilder sb = new StringBuilder();

            // Generate WHERE condition based on team selected
            string searchString = txtMemberNumber.Text.Trim();
            string searchType = ddlSearchType.SelectedValue;

            if (Page.IsPostBack)
            {
                object o = SessionHelper.GetValue("AdminMemberSearch");
                if (o != null)
                {
                    searchString = Convert.ToString(o);
                    //txtMemberNumber.Text = searchString;
                }
                object o1 = SessionHelper.GetValue("StatementDateSearch");
                if (o1 != null)
                {
                    //radDatetime.SelectedDate = (DateTime?)o1;
                }
                object o3 = SessionHelper.GetValue("ddlStatementDate");
                if (o3 != null)
                {
                    //ddlStatementDate.SelectedValue = Convert.ToString(o3);
                }
                object o4 = SessionHelper.GetValue("ddlSearchType");
                if (o4 != null)
                {
                    searchType = Convert.ToString(o4);
                    //if (!String.IsNullOrWhiteSpace(searchType))
                    //{
                    //    ddlSearchType.SelectedValue = searchType;
                    //}
                }
            }

            // check for empty search string
            if (String.IsNullOrWhiteSpace(searchString))
            {
                if (IsPostBack)
                {
                    #region set empty search error message

                    switch (searchType)
                    {
                        case "1": // TaxID
                            lblMsg.Text = "Please enter Tax ID";
                            break;

                        case "2": // Member Name
                            lblMsg.Text = "Please enter member name";
                            break;

                        case "3": // Account number
                            lblMsg.Text = "Please enter account #";
                            break;

                        case "0": // Any
                        default:
                            lblMsg.Text = "Please enter Tax ID, member name, or account #";
                            break;
                    }

                    #endregion set empty search error message
                }
                return;
            }

            // check for less than 3 character search string
            if (searchString.Length < 3)
            {
                if (IsPostBack)
                {
                    #region set to few character search error message

                    switch (searchType)
                    {
                        case "1": // TaxID
                            lblMsg.Text = "Tax ID must be longer than two characters.";
                            break;

                        case "2": // Member Name
                            lblMsg.Text = "Member name must be longer than two characters.";
                            break;

                        case "3": // Account number
                            lblMsg.Text = "Account # must be longer than two characters.";
                            break;

                        case "0": // Any
                        default:
                            lblMsg.Text = "Tax ID, member name, or account # must be longer than two characters.";
                            break;
                    }

                    #endregion set to few character search error message
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(searchString) && (searchString.Length >= 3))
            {
                string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
                if (string.IsNullOrWhiteSpace(statementDB))
                {
                    sb.Append("Statement Database custom setting in CMS desk was not found. Please add in CMS Site Manager | Settings | Custom Settings");
                }

                string memberAccountSQL = String.Empty;
                searchString = searchString.Replace("'", "''");

                switch (searchType)
                {
                    case "1": // TaxID
                        searchString = searchString.Replace("-", ""); // strip out any "-"
                        memberAccountSQL = string.Format("T.MemberID IN (select MemberId FROM {0}.dbo.fn_getMemberIdsByTaxID('{1}'))", statementDB, searchString);
                        break;

                    case "2": // Member Name
                        memberAccountSQL = String.Format("T.Name = '{0}'", searchString);
                        break;

                    case "3": // Account number
                        memberAccountSQL = string.Format("T.AccountNumber LIKE '%{0}'", searchString);
                        break;

                    case "0": // Any
                    default:
                        memberAccountSQL = string.Format("T.MemberID IN (select MemberId FROM {0}.dbo.fn_getMemberIdsForSearch('{1}'))", statementDB, searchString);
                        break;
                }

                sb.Append(memberAccountSQL);
                lblMsg.Text = String.Empty;
            }

            string where = sb.ToString();
            string dateFilter = String.Empty;
            DateTime? selectedDate = radDatetime.SelectedDate;

            if (selectedDate != null)
            {
                //dateFilter = " CONVERT(DATE, EndDate) = CONVERT(DATE, '{0}')";
                //dateFilter = String.Format(dateFilter, selectedDate.Value.Date.ToShortDateString());

                //if (String.IsNullOrWhiteSpace(where))
                //{
                //    where = dateFilter;
                //}
                //else
                //{
                //    where += " AND " + dateFilter;
                //}
            }

            // Generate WHERE condition based on item selected
            string endDateString = ddlStatementDate.SelectedValue;
            if (!string.IsNullOrWhiteSpace(endDateString))
            {
                if (endDateString != "0")
                {
                    DateTime endDate = ValidationHelper.GetDateTime(endDateString, DateTime.Now);
                    dateFilter = string.Format("MONTH(ISNull(T.[EndDate], T.StartDate)) = MONTH(CONVERT(Date, '{0}')) AND YEAR(ISNull(T.[EndDate], T.StartDate)) = YEAR(CONVERT(Date, '{1}'))", endDate, endDate);

                    if (String.IsNullOrWhiteSpace(where))
                    {
                        where = dateFilter;
                    }
                    else
                    {
                        where += " AND " + dateFilter;
                    }
                }
                //if not replicating admin only show approved batches
                //disabled for now, not going to show approved while replicating.
                //if (ReplicatingAdmin == 0)
                //{
                //    where += " and T.BatchStatusID = 5";
                //}
            }

            if (!string.IsNullOrWhiteSpace(where))
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

            StringBuilder errorSB = new StringBuilder();
            DataSet ds = GetStatementDates();
            List<String> existingDates = new List<string>();

            ddlStatementDate.Items.Add(new ListItem("ALL", "0"));
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    DateTime endDate = Convert.ToDateTime(row["EndDate"]);
                    string dateDisplayString = string.Format("{0:yyyy MMMM}", endDate);
                    if (!existingDates.Contains(dateDisplayString))
                    {
                        ListItem listItem = new ListItem(dateDisplayString, endDate.ToShortDateString());
                        //if (endDate.Date == lastDateSelection.GetValueOrDefault().Date)
                        //{
                        //    listItem.Selected = true;
                        //}
                        ddlStatementDate.Items.Add(listItem);
                        existingDates.Add(dateDisplayString);
                    }
                }
            }
            else
            {
                //errorSB.Append(string.Format("Did not find accounts for memberID:{0}<br />", MemberID));
            }
        }

        /// <summary>
        /// Bind the search type drop down.
        /// </summary>
        private void BindSearchTypeDropDown()
        {
            Object o = SessionHelper.GetValue("ddlSearchType");
            string lastSearchTypeSelection = null;
            if (o != null)
            {
                try
                {
                    lastSearchTypeSelection = Convert.ToString(o);
                }
                catch
                {
                    //noop
                }
            }

            if (!String.IsNullOrWhiteSpace(lastSearchTypeSelection))
            {
                //ddlSearchType.SelectedValue = lastSearchTypeSelection;
            }
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
            oldestViewableDate = DateTime.Now.Date.AddYears(-7);
            //}

            QueryDataParameters qdp = new QueryDataParameters();
            //qdp.Add("MemberID", MemberID);
            qdp.Add("OldestViewableDate", oldestViewableDate);
            qdp.Add("viewOverride", true);
            DataSet ds = ConnectionHelper.ExecuteQuery(String.Format("{0}.dbo.sproc_getHistoricalStatementViewDates", dataDB), qdp, QueryTypeEnum.StoredProcedure);
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