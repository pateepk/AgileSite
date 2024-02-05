using CMS.DataEngine;
using System;
using System.Data;
using System.IO;

//using System.Security.Cryptography.Xml;

public partial class sso_nebraska_test : System.Web.UI.Page
{
    private string SAML_A = "";
    private string SAML_B = "";
    private string SAML_C = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        string Date1 = DateTime.UtcNow.ToString("s") + "Z";
        string Date2 = DateTime.UtcNow.AddDays(1).ToString("s") + "Z";

        StreamReader FS = File.OpenText(Request.MapPath("App_Code/SAML_A.txt"));
        SAML_A = String.Format(
            FS.ReadToEnd(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Date1,
            Date2,
            Guid.NewGuid().ToString()
            );
        FS.Close();

        FS = File.OpenText(Request.MapPath("App_Code/SAML_B.txt"));
        SAML_B = FS.ReadToEnd();
        FS.Close();

        FS = File.OpenText(Request.MapPath("App_Code/SAML_C.txt"));
        SAML_C = FS.ReadToEnd();
        FS.Close();
    }

    protected string ToSAMLResponse(string X509CertificateString, string FullName, string Email, string CompanyID)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(SAML_A);
        sb.Append(string.Format(SAML_C, "FullName", FullName));
        sb.Append(string.Format(SAML_C, "Email", Email));
        sb.Append(string.Format(SAML_C, "CompanyID", CompanyID));
        sb.Append(SAML_B);

        if ((X509CertificateString.Length > 0))
        {
            ////  TODO: Need to sign the xml for
            //XmlDocument xdoc = new XmlDocument();
            //xdoc.LoadXml(sb.ToString());
            //SignedXml signedXml = new SignedXml(xdoc);
            //byte[] data = Convert.FromBase64String(X509CertificateString);
            //object x509 = new X509Certificate2(data);
            //signedXml.SigningKey = x509.PrivateKey;
            //Reference reference = new Reference();
            //reference.Uri = "";
            //XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            //reference.AddTransform(env);
            //signedXml.AddReference(reference);
            //KeyInfo keyInfo = new KeyInfo();
            //keyInfo.AddClause(new KeyInfoX509Data(x509));
            //signedXml.KeyInfo = keyInfo;
            //signedXml.ComputeSignature();
            //XmlElement xel = signedXml.GetXml();
            //xdoc.DocumentElement.AppendChild(xdoc.ImportNode(xel, true));
            //sb.Clear();
            //sb.Append(xdoc.ToString());
        }

        return sb.ToString();
    }

    protected string ToBase64String(string X509CertificateString, string FullName, string Email, string CompanyID)
    {
        byte[] bytes;

        bytes = System.Text.Encoding.UTF8.GetBytes(this.ToSAMLResponse(X509CertificateString, FullName, Email, CompanyID));

        return Convert.ToBase64String(bytes);
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
    }

    protected void btnGenerateSAML_Click(object sender, EventArgs e)
    {
        if (txtFullName.Text.Length > 0 && txtEmailAddress.Text.Length > 0 && txtCompanyID.Text.Length > 0)
        {
            SAMLRespDecoded.Text = ToBase64String("", txtFullName.Text, txtEmailAddress.Text, txtCompanyID.Text);
            SAMLRespEncoded.Text = ToSAMLResponse("", txtFullName.Text, txtEmailAddress.Text, txtCompanyID.Text);
            SAMLResponse.Value = SAMLRespDecoded.Text;
            btnSubmitLogin.Enabled = true;
        }
        else
        {
            SAMLRespDecoded.Text = "";
            SAMLRespEncoded.Text = "";
            SAMLResponse.Value = "";
            btnSubmitLogin.Enabled = false;
        }
    }

    protected void btnSubmitLogin_Click(object sender, EventArgs e)
    {
    }
}