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
    public partial class CUMemberStatementsFilter : CMSAbstractQueryFilterControl
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

        public string MemberNumber
        {
            get
            {
                string defaultValue = "-1";
                string ret = defaultValue;
                object o = MembershipContext.AuthenticatedUser.UserSettings.GetValue("CUMemberNumber");
                if (o != null)
                {
                    ret = o.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(ret))
                    {
                        ret = defaultValue;
                    }
                    else if (ret == "0")
                    {
                        ret = "-1";
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
        private void SetupControl_old()
        {
            if (this.StopProcessing)
            {
                this.Visible = false;
            }
            else if (!RequestHelper.IsPostBack())
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

                //populate drop down starting with previous month
                DateTime curDate = DateTime.Now.AddMonths(-1);

                if (curDate.Day < 3)
                {
                    //new statements are not out yet, leave previous month out of drop down
                    curDate = curDate.AddMonths(-1);
                }

                //using end dates
                //curDate = curDate.AddDays(-curDate.Day).AddMonths(1);

                while (curDate.Day != 1)
                {
                    curDate = curDate.AddDays(1);
                }
                curDate = curDate.AddDays(-1); //get the end date

                DateTime minDateTime = new DateTime(2010, 11, 1);

                int count = 0;
                //commented out as requested by Mary/Jennifer
                //for (count=0; count<24; count++)
                {
                    while (curDate >= minDateTime)
                    {
                        ListItem listItem = new ListItem(string.Format("{0:yyyy MMMM}", curDate), curDate.ToShortDateString());
                        if (count == 0 && lastDateSelection == null)
                        {
                            count = 1;
                            listItem.Selected = true;
                        }
                        else if (lastDateSelection != null)
                        {
                            if (curDate.Date == lastDateSelection.GetValueOrDefault().Date)
                            {
                                listItem.Selected = true;
                            }
                        }
                        ddlStatementDate.Items.Add(listItem);
                        //curDate = curDate.AddDays(-curDate.Day);
                        curDate = curDate.AddMonths(-1);

                        while (curDate.Day != 1)
                        {
                            curDate = curDate.AddDays(1);
                        }
                        curDate = curDate.AddDays(-1); //get the end date
                    }
                }
            }
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

                //populate drop down with available list of statements
                String dataDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);
                StringBuilder errorSB = new StringBuilder();
                StringBuilder sql = new StringBuilder();

                sql.Append("SELECT DISTINCT T.EndDate FROM");
                sql.Append(" (SELECT S.ENDdate ");
                sql.Append(" FROM ").Append(dataDB).Append(".dbo.Statement S");
                sql.Append(" INNER JOIN ").Append(dataDB).Append(".dbo.Batch B ON B.ID = S.BatchID");
                sql.Append(" WHERE S.MemberNumber = @memberNumber");
                if (ReplicatingAdmin == 0)
                {
                    sql.Append(" AND B.BatchStatusID = 5");
                }
                sql.Append(" UNION");
                sql.Append(" SELECT S.EndDate ");
                sql.Append(" FROM ").Append(dataDB).Append(".dbo.AssociatedAccountLookup AL");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.ShareAccount SA on AL.AccountNumber = SA.AccountNumber");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.Statement S on SA.StatementID = S.ID");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.StatementType ST ON S.StatementTypeID = ST.StatementTypeID");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.Batch B ON B.ID = S.BatchID ");
                sql.Append(" where AL.MemberNumber = @MemberNumber");
                if (ReplicatingAdmin == 0)
                {
                    sql.Append(" AND B.BatchStatusID = 5");
                }
                sql.Append(" ) T");
                sql.Append(" ORDER BY T.EndDate DESC");

                QueryDataParameters qdp = new QueryDataParameters();
                qdp.Add("memberNumber", MemberNumber);

                DataSet ds = ConnectionHelper.ExecuteQuery(sql.ToString(), qdp, QueryTypeEnum.SQLQuery);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        DateTime endDate = Convert.ToDateTime(row["EndDate"]);

                        ListItem listItem = new ListItem(string.Format("{0:yyyy MMMM}", endDate), endDate.ToShortDateString());
                        if (endDate.Date == lastDateSelection.GetValueOrDefault().Date)
                        {
                            listItem.Selected = true;
                        }

                        ddlStatementDate.Items.Add(listItem);
                    }
                }
                else
                {
                    errorSB.Append(string.Format("Did not find accounts for memberNumber:{0}<br />", MemberNumber));
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
            string where = "B.BatchStatusID = 5";

            // Generate WHERE condition based on team selected
            string endDateString = ddlStatementDate.SelectedValue;
            if (!string.IsNullOrWhiteSpace(endDateString))
            {
                DateTime endDate = ValidationHelper.GetDateTime(endDateString, DateTime.Now);
                where = string.Format("S.EndDate = '{0:yyyy-MM-dd}'", endDate);

                //if not replicating admin only show approved batches
                if (ReplicatingAdmin == 0)
                {
                    where += " and B.BatchStatusID = 5";
                }
            }

            if (where != null)
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }
    }
}