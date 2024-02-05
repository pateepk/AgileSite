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
    public partial class CUMemberTaxYearFilter : CMSAbstractQueryFilterControl
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
                SessionHelper.SetValue("ddlTaxYear", ddlTaxYear.SelectedValue);
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
                Object o = SessionHelper.GetValue("ddlTaxYear");
                string lastDateSelection = "1111";
                if (o != null)
                {
                    try
                    {
                        lastDateSelection = Convert.ToString(o);
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

                sql.Append("select distinct TaxYear");
                sql.Append(" from (SELECT");
                sql.Append(" CASE ");
                sql.Append(" WHEN NRS.EndDate is not null THEN DatePart(Year, NRS.EndDate)");
                sql.Append(" ELSE DatePart(Year, N.Date)");
                sql.Append(" END TaxYear");
                sql.Append(" from ").Append(dataDB).Append(".dbo.Notice N");
                sql.Append(" INNER JOIN ").Append(dataDB).Append(".dbo.NoticeType NT ON N.NoticeTypeID = NT.ID and NT.ID in (14, 10, 3)");
                sql.Append(" INNER JOIN ").Append(dataDB).Append(".dbo.Batch B on N.BatchID = B.ID");
                if (ReplicatingAdmin == 0)
                {
                    sql.Append(" AND B.BatchStatusID = 5");
                }
                sql.Append(" LEFT OUTER JOIN ").Append(dataDB).Append(".dbo.N_RetirementStatement NRS on NRS.NoticeID = N.id ");
                sql.Append(" WHERE N.MemberAgreeNbr = @memberNumber ");
                sql.Append(" union");
                sql.Append(" select TaxYear from ").Append(dataDB).Append(".dbo.TaxInfo TI");
                sql.Append(" INNER JOIN ").Append(dataDB).Append(".dbo.Batch B on TI.BatchID = B.ID");
                if (ReplicatingAdmin == 0)
                {
                    sql.Append(" AND B.BatchStatusID = 5");
                }
                sql.Append(" where MemberNumber = @memberNumber");
                sql.Append(" ) TY");
                sql.Append(" WHERE TaxYear > 2008");
                sql.Append(" order by TaxYear desc");

                QueryDataParameters qdp = new QueryDataParameters();
                qdp.Add("memberNumber", MemberNumber);

                DataSet ds = ConnectionHelper.ExecuteQuery(sql.ToString(), qdp, QueryTypeEnum.SQLQuery);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string taxYear = Convert.ToString(row["TaxYear"]);

                        ListItem listItem = new ListItem(taxYear, taxYear);
                        if (taxYear == lastDateSelection)
                        {
                            listItem.Selected = true;
                        }

                        ddlTaxYear.Items.Add(listItem);
                    }
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
            string selectedYear = ddlTaxYear.SelectedValue;
            if (!string.IsNullOrWhiteSpace(selectedYear))
            {
                where = string.Format("TaxYear = '{0}' and B.BatchStatusID = 5", selectedYear);
            }

            if (where != null)
            {
                // Set where condition
                this.WhereCondition = where;
            }
        }
    }
}