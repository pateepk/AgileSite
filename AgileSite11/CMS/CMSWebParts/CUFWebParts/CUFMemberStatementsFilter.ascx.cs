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
    public partial class CUFMemberStatementsFilter : CMSAbstractQueryFilterControl
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
                sql.Append(" (SELECT isnull(S.ENDdate, S.StartDate) EndDate ");
                sql.Append(" FROM ").Append(dataDB).Append(".dbo.Statement S");
                sql.Append(" INNER JOIN ").Append(dataDB).Append(".dbo.Batch B ON B.BatchID = S.BatchID");
                sql.Append(" INNER JOIN ").Append(dataDB).Append(".dbo.MemberToStatement MS ON MS.StatementID = S.StatementID");
                sql.Append(" WHERE MS.MemberID = @MemberID");
                if (ReplicatingAdmin == 0)
                {
                    sql.Append(" AND B.BatchStatusID = 5");
                }
                /*
                sql.Append(" UNION");
                sql.Append(" SELECT S.EndDate ");
                sql.Append(" FROM ").Append(dataDB).Append(".dbo.AssociatedAccountLookup AL");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.Account A on AL.AccountNumber = SA.AccountNumber");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.ShareAccount SA on AL.AccountNumber = SA.AccountNumber");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.Statement S on SA.StatementID = S.ID");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.StatementType ST ON S.StatementTypeID = ST.StatementTypeID");
                sql.Append(" INNER JOIN  ").Append(dataDB).Append(".dbo.Batch B ON B.ID = S.BatchID ");
                sql.Append(" where AL.MemberNumber = @MemberNumber");
                if (ReplicatingAdmin==0)
                {
                    sql.Append(" AND B.BatchStatusID = 5");
                }
                */
                sql.Append(" ) T");
                sql.Append(" ORDER BY T.EndDate DESC");

                QueryDataParameters qdp = new QueryDataParameters();
                qdp.Add("MemberID", MemberID);

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
                    errorSB.Append(string.Format("Did not find accounts for memberID:{0}<br />", MemberID));
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
                where = string.Format("isnull(S.EnDdate, S.StartDate) = '{0:yyyy-MM-dd}'", endDate);

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