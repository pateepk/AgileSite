using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using CMS.PortalEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.DataEngine;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUAccountDate : CMSAbstractWebPart
    {
        #region "Properties"
        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string CUDBName
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CUDBName"), "");
            }
            set
            {
                this.SetValue("CUDBName", value);
            }
        }

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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                StringBuilder errorSB = new StringBuilder();

                string idString = "ShareAccountID";
                StringBuilder sql = new StringBuilder();
                
                if (AccountType.ToLower() == "cash")
                {
                    sql.Append("SELECT S.EndDate, SA.ID ShareAccountID");
                    sql.Append(" FROM ").Append(CUDBName).Append(".dbo.ShareAccount SA");
                    sql.Append(" INNER JOIN ").Append(CUDBName).Append(".dbo.Statement S ON SA.StatementID = S.ID");
                    sql.Append(" WHERE SA.AccountNumber = (select accountNumber");
                    sql.Append(" FROM ").Append(CUDBName).Append(".dbo.ShareAccount where ID = @accountID)");
                    sql.Append(" AND S.MemberNumber = @memberNumber");
                    sql.Append(" ORDER BY S.EndDate DESC");

                }
                else if (AccountType.ToLower() == "loan")
                {
                    idString = "LoanAccountID";
                    sql.Append("SELECT S.EndDate, SA.ID LoanAccountID");
                    sql.Append(" FROM ").Append(CUDBName).Append(".dbo.LoanAccount SA");
                    sql.Append(" INNER JOIN ").Append(CUDBName).Append(".dbo.Statement S ON SA.StatementID = S.ID");
                    sql.Append(" WHERE SA.AccountNumber = (select accountNumber");
                    sql.Append(" FROM ").Append(CUDBName).Append(".dbo.LoanAccount where ID = @accountID)");
                    sql.Append(" AND S.MemberNumber = @memberNumber");
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
                qdp.Add("memberNumber", MemberNumber);

                
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
                    errorSB.Append(string.Format("Did not find accounts for memberNumber:{0} and {1}:{2}<br />", MemberNumber, idString, currentAccountID));
                }
            }
        }
    }
}