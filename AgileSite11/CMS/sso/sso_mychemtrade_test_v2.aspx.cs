using CMS.DataEngine;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

public partial class sso_mychemtrade_test_v2 : System.Web.UI.Page
{
    private string SAML_A_Swap = "";
    private string SAML_B = "";
    private string SAML_C = "";
    private RSACryptoServiceProvider _rsa;
    private RSAParameters _rsaParams;

    /// ================ Paths ===================== ///
    private const string _pfxPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pfx";

    private const string _combPemPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.combined.pem";
    private const string _pemPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pem"; // requires password
    private const string _publicPemCert = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pub.crt";
    private const string _publicCert = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pub.crt";
    private const string _privateKeyPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.key";
    private const string _publicKeyPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.key";

    /// =============== Passwords ================== ///
    private const string _pfxPasword = "TiR4DD3v0lp3R$";

    private const string _certSerial = "03ac2065a148ffcf075efadaf2db2ff1";

    // OneLogin Cert (eric)
    private const string _pemOneLogin = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\OneLogin.pem";

    private const string _pubOneLogin = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\OneLogin.pub";

    protected void Page_Load(object sender, EventArgs e)
    {
        string Date1 = DateTime.UtcNow.ToString("s") + "Z";
        string Date2 = DateTime.UtcNow.AddDays(1).ToString("s") + "Z";

        System.IO.StreamReader FS = System.IO.File.OpenText(Request.MapPath("App_Code/SAML_A_SWAP.txt"));
        SAML_A_Swap = String.Format(
            FS.ReadToEnd(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Date1,
            Date2,
            Guid.NewGuid().ToString()
            );
        FS.Close();

        // Change this if you create for a new company
        SAML_A_Swap = SAML_A_Swap.Replace("{CHANGEME}", "sso_mychemtrade");

        FS = System.IO.File.OpenText(Request.MapPath("App_Code/SAML_B.txt"));
        SAML_B = FS.ReadToEnd();
        FS.Close();

        FS = System.IO.File.OpenText(Request.MapPath("App_Code/SAML_C.txt"));
        SAML_C = FS.ReadToEnd();
        FS.Close();
    }

    protected string ToSAMLResponse(string FullName, string Email, string CompanyID)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(SAML_A_Swap);
        sb.Append(string.Format(SAML_C, "FullName", FullName));
        sb.Append(string.Format(SAML_C, "Email", Email));
        sb.Append(string.Format(SAML_C, "CompanyID", CompanyID));
        sb.Append(SAML_B);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(sb.ToString());
        X509Certificate2 cert = GetX509Certificate2();
        if (cert != null)
        {
            return SignXmlWithCertificate(doc, cert);
        }
        return doc.OuterXml;
    }

    protected X509Certificate2 GetX509Certificate2()
    {
        //var certPem = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDD+2b9q6UkU+MxldEA21TDl0HvbPzxPBpcGAv8Z3DQJ8bPkgD84EnMXqrcIc0x0PWurwT9J7aE0HNyf5MHvEm4nT+HGCTGkNUeIbTaBWb4vw5AdEFomYfSKVQ+Saea2LHxjxmlpKpSQpGqEhsKRbdM6MqPIXTHwauPjyLOIV6GAQIDAQAB";
        //return new X509Certificate2(Convert.FromBase64String(certPem));
        //X509Certificate cert1 = new X509Certificate2.CreateFromCertFile(_pemOneLogin);
        //X509Certificate2 cert2 = new X509Certificate2(cert1.Export(X509ContentType.Cert));
        //return cert2;
        X509Certificate2 cert = new X509Certificate2();
        X509Certificate2 combinedCertificate = new X509Certificate2(_pfxPath);
        X509KeyStorageFlags flags = X509KeyStorageFlags.Exportable;
        cert = new X509Certificate2(_pfxPath, _pfxPasword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        cert = new X509Certificate2(_pfxPath, _pfxPasword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        _rsa = (RSACryptoServiceProvider)cert.PrivateKey;
        _rsaParams = _rsa.ExportParameters(true);
        return cert;
        ///////////////////////////////////////////////////////////
    }

    protected string SignXmlWithCertificate(XmlDocument Document, X509Certificate2 cert)
    {
        lblError.Text = "";
        try
        {
            SignedXml signedXml = new SignedXml(Document);
            signedXml.SigningKey = _rsa;
            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env =
               new XmlDsigEnvelopedSignatureTransform(true);
            reference.AddTransform(env);

            // XmlDsigC14NTransform c14t = new XmlDsigC14NTransform();
            // reference.AddTransform(c14t);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate).
            //KeyInfoX509Data keyInfoData = new KeyInfoX509Data(cert);
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new RSAKeyValue((RSA)_rsa));
            signedXml.KeyInfo = keyInfo;

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            Document.DocumentElement.AppendChild(
                Document.ImportNode(xmlDigitalSignature, true));
        }
        catch (Exception ex)
        {
            lblErrorTitle.Visible = true;
            lblError.Visible = true;
            lblError.Text = ex.ToString();
        }
        VerifyXmlFile(Document);
        return Document.OuterXml;
    }

    // Verify the signature of an XML file and return the result.
    protected Boolean VerifyXmlFile(XmlDocument xmlDocument)
    {
        keyVerified.Visible = false;
        try
        {
            // Format using white spaces.
            xmlDocument.PreserveWhitespace = true;

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(xmlDocument);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");

            // Load the signature node.
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            var isSigned = signedXml.CheckSignature();

            if (isSigned)
            {
                keyVerified.Visible = true;
                return true;
            }
            else
            {
                keyVerified.Text = "XML is not signed";
                keyVerified.Visible = true;
                return false;
            }
        }
        catch (Exception ex)
        {
            lblErrorTitle.Visible = true;
            lblError.Visible = true;
            lblError.Text = ex.ToString();
            return false;
        }
    }

    protected byte[] GetBytesFromPEM(string pemString, string section)
    {
        var header = String.Format("-----BEGIN {0}-----", section);
        var footer = String.Format("-----END {0}-----", section);

        var start = pemString.IndexOf(header, StringComparison.Ordinal);
        if (start < 0)
            return null;

        start += header.Length;
        var end = pemString.IndexOf(footer, start, StringComparison.Ordinal) - start;

        if (end < 0)
            return null;

        return Convert.FromBase64String(pemString.Substring(start, end));
    }

    protected string ToBase64String(string FullName, string Email, string CompanyID)
    {
        byte[] bytes;

        bytes = System.Text.Encoding.UTF8.GetBytes(this.ToSAMLResponse(FullName, Email, CompanyID));

        return Convert.ToBase64String(bytes);
    }

    protected void cmdCheckCompany_Click(object sender, EventArgs e)
    {
        LiteralMessage.Text = "";
        int intCompanyID = 0;
        int.TryParse(txtCompanyID.Text, out intCompanyID);
        // Database is on TNN_CLONE
        DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ItemID, PartnerName, CustomerName FROM dbo.customtable_Customers WHERE ItemID = {0}", txtCompanyID.Text.ToString()), null, QueryTypeEnum.SQLQuery);
        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        {
            DataRow row = ds.Tables[0].Rows[0];
            // lblCompanyName.Text = String.Format("<b>{0} ({1})</b>", row["PartnerName"], row["CustomerName"]);
            lblCompanyName.Text = String.Format("<b>{0} {1}</b>", row["PartnerName"], row["CustomerName"]);
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
            SAMLRespDecoded.Text = ToBase64String(txtFullName.Text, txtEmailAddress.Text, txtCompanyID.Text);
            SAMLRespEncoded.Text = ToSAMLResponse(txtFullName.Text, txtEmailAddress.Text, txtCompanyID.Text);
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
        if (Response.IsClientConnected)
        {
            Response.Redirect("https://www.trainingnetworknow.com/sso/sso_mychemtrade.aspx", false);
        }
    }
}