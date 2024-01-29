using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFAccountDate : CMSAbstractWebPart
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string AccountType
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("AccountType"), "Cash");
            }
            set
            {
                this.SetValue("AccountType", value);
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                StringBuilder errorSB = new StringBuilder();

                string idString = "ShareAccountID";
                string statementDB = SettingsKeyInfoProvider.GetValue("StatementDatabase", SiteContext.CurrentSiteID);

                StringBuilder sql = new StringBuilder();

                if (AccountType.ToLower() == "cash")
                {
                    sql.Append("SELECT S.EndDate, SA.ShareAccountID");
                    sql.Append(" FROM ").Append(statementDB).Append(".dbo.ShareAccount SA");
                    sql.Append(" INNER JOIN ").Append(statementDB).Append(".dbo.Statement S ON SA.StatementID = S.StatementID");
                    sql.Append(" INNER JOIN ").Append(statementDB).Append(".dbo.Account A ON A.AccountID = SA.AccountID");
                    sql.Append(" INNER JOIN ").Append(statementDB).Append(".dbo.MemberToAccount MA ON A.AccountID = MA.AccountID");
                    sql.Append(" WHERE SA.ShareAccountID = @accountID");
                    sql.Append(" AND MA.MemberID = @MemberID");
                    sql.Append(" ORDER BY S.EndDate DESC");
                }
                else if (AccountType.ToLower() == "loan")
                {
                    idString = "LoanAccountID";

                    sql.Append("SELECT S.EndDate, LA.LoanAccountID");
                    sql.Append(" FROM ").Append(statementDB).Append(".dbo.LoanAccount LA");
                    sql.Append(" INNER JOIN ").Append(statementDB).Append(".dbo.Statement S ON LA.StatementID = S.StatementID");
                    sql.Append(" INNER JOIN ").Append(statementDB).Append(".dbo.Account A ON A.AccountID = LA.AccountID");
                    sql.Append(" INNER JOIN ").Append(statementDB).Append(".dbo.MemberToAccount MA ON A.AccountID = MA.AccountID");
                    sql.Append(" WHERE LA.LoanAccountID = @accountID");
                    sql.Append(" AND MA.MemberID = @MemberID");
                    sql.Append(" ORDER BY S.EndDate DESC");
                }

                string currentAccountID = "-1";
                if (!string.IsNullOrWhiteSpace(Request.QueryString[idString]))
                {
                    currentAccountID = Request.QueryString[idString];
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(Convert.ToString(SessionHelper.GetValue(idString))))
                    {
                        currentAccountID = Convert.ToString(SessionHelper.GetValue(idString));
                    }
                }

                string jumpLink = "";
                if (!string.IsNullOrWhiteSpace(Request.QueryString["aliaspath"]))
                {
                    jumpLink = Request.QueryString["aliaspath"];
                }

                jumpLink += "?tk=1"; //dummy to keep from substring when we add query params

                //populate drop down
                //String dataDB = SettingsKeyProvider.GetStringValue(CMSContext.CurrentSite.SiteName + ".StatementDatabase");

                QueryDataParameters qdp = new QueryDataParameters();
                qdp.Add("accountID", currentAccountID);
                qdp.Add("MemberID", MemberID);

                String[] queryStrings = Page.Request.Url.Query.Substring(1).Split("&".ToCharArray());
                string newQS = string.Empty;
                foreach (string qs in queryStrings)
                {
                    if (qs.StartsWith(idString + "=", StringComparison.InvariantCultureIgnoreCase)
                        || qs.StartsWith("aliaspath=", StringComparison.InvariantCultureIgnoreCase)
                        || qs.StartsWith("tk=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //skip
                    }
                    else
                    {
                        newQS += "&" + qs;
                    }
                }

                if (!String.IsNullOrWhiteSpace(newQS))
                {
                    jumpLink += newQS.Substring(1);
                }

                DataSet ds = ConnectionHelper.ExecuteQuery(sql.ToString(), qdp, QueryTypeEnum.SQLQuery);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        DateTime endDate = Convert.ToDateTime(row["EndDate"]);
                        String shareAccountID = Convert.ToString(row[idString]);

                        ListItem listItem = new ListItem(string.Format("{0:MM/yyyy}", endDate), String.Format("{0}&{1}={2}", jumpLink, idString, shareAccountID));
                        if (currentAccountID == shareAccountID)
                        {
                            listItem.Selected = true;
                        }

                        ddlAccountDate.Items.Add(listItem);
                    }
                }
                else
                {
                    errorSB.Append(string.Format("Did not find accounts for memberID:{0} and {1}:{2}<br />", MemberID, idString, currentAccountID));
                }
            }
        }
    }
}