using PaymentProcessor.Web.Applications;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;


namespace ExcellaLite.Account
{
    public partial class UserActivityLogs : BasePage
    {

        protected string ActivityHistoryServiceURL = Utils.ResolveURL("webservices/Excellalite.asmx/GetDataXML");

        public struct Columns
        {
            public const string TimeAgo = "TimeAgo";
            public const string ShowData = "ShowData";
        }

        protected void Page_Load()
        {

            if ((user.isLogin) && (user.isUserAdministrator))
            {

                // todo: paging 
                int MaxResult = 100;
                DRspActivityHistory_Select ulogs = SQLData.spActivityHistory_Select(MaxResult);
                DataTable dt = ulogs.DataSource;
                if (ulogs.Count > 0)
                {
                    dt.Columns.Add(Columns.TimeAgo);
                    dt.Columns.Add(Columns.ShowData);
                    for (int i = 0; i < ulogs.Count; i++)
                    {
                        dt.Rows[i][Columns.TimeAgo] = Utils.FormatTimeAgo(ulogs.ActivityDate(i));
                        if (ulogs.IsAnyData(i) && (user.isUserAdministrator)) // only admin can see data here
                        {
                            dt.Rows[i][Columns.ShowData] = String.Format("<img onclick=\"showData({1});\" src=\"{0}\">", Utils.ResolveURL("images/xml.gif"), ulogs.ActivityHistoryID(i));
                        }
                        else
                        {
                            dt.Rows[i][Columns.ShowData] = "-";
                        }
                    }
                    Repeater1.DataSource = ulogs.DataSource;
                    Repeater1.DataBind();
                }
            }
            else
            {
                Utils.responseRedirect("/Default.aspx", true);
            }
        }



    }
}