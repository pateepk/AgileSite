using CMS.DataEngine;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;

public partial class ssotest : System.Web.UI.Page
{
    /// <summary>
    /// Put this one on page loag
    /// </summary>
    private string LogFile = "";

    protected void CreateLogFile()
    {
        try
        {
            string path = Request.MapPath("logs");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            LogFile = path + "\\" + String.Format("{0:yyyy-MM-dd}.txt", DateTime.Now);
            if (!File.Exists(LogFile))
            {
                File.CreateText(LogFile);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// Call this one from anywhere
    /// </summary>
    /// <param name="message"></param>
    protected void LogThis(string message)
    {
        try
        {
            if (LogFile.Length > 0)
            {
                string sn = Request.ServerVariables["SCRIPT_NAME"];
                int idx = sn.LastIndexOf("/");
                if (idx > -1)
                {
                    sn = sn.Substring(idx + 1);
                }
                if (message.IndexOf("\r") > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} {1:HH:mm:ss} {2}: [BEGIN]\r\n", sn, DateTime.Now, Session.SessionID);
                    sb.Append(message);
                    sb.Append("\r\n[END]\r\n");
                    File.AppendAllText(LogFile, sb.ToString());
                }
                else
                {
                    File.AppendAllText(LogFile, String.Format("{0} {1:HH:mm:ss} {2}: {3}\r\n", sn, DateTime.Now, Session.SessionID, message));
                }
            }
        }
        catch
        {
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        CreateLogFile();
        LogThis("Ho ho ho");
    }

    protected void cmdCheckCompany_Click(object sender, EventArgs e)
    {
        LiteralMessage.Text = "";
        int intCompanyID = 0;
        int.TryParse(txtCompanyID.Text, out intCompanyID);

        DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ItemID, PartnerName, CustomerName FROM dbo.customtable_Customers WHERE ItemID = {0}", intCompanyID), null, QueryTypeEnum.SQLQuery);
        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        {
            DataRow row = ds.Tables[0].Rows[0];
            lblCompanyName.Text = String.Format("<b>{0} ({1})</b>", row["PartnerName"], row["CustomerName"]);
        }
        else
        {
            lblCompanyName.Text = "<i>Company not found</i>";
        }
        if (LiteralSSOLink.Text.Length > 0)
        {
            cmdCreateSSOLink_Click(sender, e);
        }
    }

    protected void cmdCreateSSOLink_Click(object sender, EventArgs e)
    {
        bool isOK = true;
        int intCompanyID = 0;
        int.TryParse(txtCompanyID.Text, out intCompanyID);
        LiteralSSOLink.Text = "";
        LiteralMessage.Text = "";
        if (intCompanyID <= 0)
        {
            LiteralMessage.Text = "Company ID should be an interger value.";
            isOK = false;
        }
        if (txtFullName.Text.Length == 0)
        {
            LiteralMessage.Text = "Please enter full name.";
            isOK = false;
        }
        if (txtEmailAddress.Text.Length == 0)
        {
            LiteralMessage.Text = "Please enter email address.";
            isOK = false;
        }
        if (isOK)
        {
            LiteralSSOLink.Text = String.Format("<a target=\"_blank\" href=\"{0}\">{0}</a>", String.Format("/sso/sso.aspx?name={0}&email={1}&CompanyID={2}", HttpUtility.UrlEncode(txtFullName.Text), HttpUtility.UrlEncode(txtEmailAddress.Text), intCompanyID));
        }
    }
}