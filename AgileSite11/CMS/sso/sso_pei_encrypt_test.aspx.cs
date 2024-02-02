using CMS.DataEngine;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public partial class sso_pei_encrypt_test : System.Web.UI.Page
{
    //private string SharedKey = "XJK531LHQE819KDQ"; // for testing it is should be same shared key. tak ea look at sso_encrypt.aspx.cs
    private string SharedKey = "KbNcBur2aYeIj4Ku"; // shared key for PEI

    protected string GetMd5Hash(MD5 md5Hash, string input)
    {
        // Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // to generate a random SharedKey go to  - https://www.random.org/strings/?num=10&len=16&digits=on&upperalpha=on&loweralpha=on&unique=on&format=plain&rnd=new
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
        txtEmailAddress.Text = txtEmailAddress.Text.Trim();
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
            string hash = intCompanyID.ToString() + txtEmailAddress.Text + SharedKey;
            MD5 md5Hash = MD5.Create();
            hash = HttpUtility.UrlEncode(GetMd5Hash(md5Hash, hash));
            LiteralSSOLink.Text = String.Format("<a target=\"_blank\" href=\"{0}\">{0}</a>", String.Format("/sso/sso_pei_encrypt.aspx?name={0}&email={1}&CompanyID={2}&hash={3}", HttpUtility.UrlEncode(txtFullName.Text), HttpUtility.UrlEncode(txtEmailAddress.Text), intCompanyID, hash));
        }
    }
}