using CMS.DataEngine;
using CMS.Helpers;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

public partial class sso_quality_refrig_services_test : System.Web.UI.Page
{
    #region constant

    private const string _ssoroot = "sso";
    private const string _SAMLroot = "App_Code";
    private const string _Certroot = "Certs";
    private const string _SampleRoot = "sampleSAML";



    #endregion constant

    #region members

    //private string _pfxFileName = "2022-trainingnetworknow.pfx";
    //private string _pfxPassword = "m0ns00n";
    private string _pfxFileName = "trainingnetwork.pfx";
    private string _pfxPassword = "TiR4DD3v0lp3R$";


    private string _testPage = "sso_quality_refrig_services_test.aspx";
    private string _ssoPage = "sso_quality_refrig_services.aspx";

    private bool _validateCert = true;
    private bool _useSampleSAML = false;
    private bool _enableTestMode = true; // enabled test only mode

    private string _SAML_A_Filename = "SAML_A_SWAP_2023.txt";
    private string _SAML_B_Filename = "SAML_B.txt";
    private string _SAML_C_Filename = "SAML_C.txt";

    #endregion members

    #region properties

    private string PfxFileName
    {
        get
        {
            return _pfxFileName != null ? _pfxFileName : "trainingnetwork.pfx";
        }
        set
        {
            _pfxFileName = value;
        }
    }

    private string PfxPassword
    {
        get
        {
            return _pfxPassword != null ? _pfxPassword : "TiR4DD3v0lp3R$";
        }
        set
        {
            _pfxPassword = value;
        }
    }

    private string SAMLPostBackAbsoluteUrl
    {
        get
        {
            string ret = GetSSORootUrl(_ssoPage, true);
            if (_enableTestMode)
            {
                ret = ret + "?m=test";
            }
            return ret;
        }
    }

    private string SAMLPostBackRelativeUrl
    {
        get
        {
            string ret = GetSSORootUrl(_ssoPage, false);
            if (_enableTestMode)
            {
                ret = ret + "?m=test";
            }
            return ret;
        }
    }

    #endregion properties

    private string SAML_A_Swap = "";
    private string SAML_B = "";
    private string SAML_C = "";
    private RSACryptoServiceProvider _rsa;
    private RSAParameters _rsaParams;

    /// <summary>
    /// Get the file in sso folder
    /// </summary>
    /// <param name="p_ssoPage"></param>
    /// <param name="p_getabsolute"></param>
    /// <returns></returns>
    protected string GetSSORootUrl(string p_ssoPage, bool p_getabsolute = false)
    {
        string ret = "~/" + _ssoroot + "/" + p_ssoPage;
        if (p_getabsolute)
        {
            ret = GetAbsoluteURL(ret);
        }
        return ret;
    }

    /// <summary>
    /// Get SAML file
    /// </summary>
    /// <param name="p_samlFile"></param>
    /// <param name="p_getphysicalPath"></param>
    /// <returns></returns>
    protected string GetSAMLRootUrl(string p_samlFile, bool p_getphysicalPath = false)
    {
        string ret = _SAMLroot + "/" + p_samlFile;
        if (p_getphysicalPath)
        {
            ret = GetPhysicalFilePath(ret);
        }
        return ret;
    }

    /// <summary>
    /// Get the certificate file url.
    /// </summary>
    /// <param name="p_filename"></param>
    /// <param name="p_getphysicalPath"></param>
    /// <returns></returns>
    protected string GetCertificateFileUrl(string p_filename, bool p_getphysicalPath = true)
    {
        string ret = _Certroot + "/" + p_filename;
        if (p_getphysicalPath)
        {
            ret = GetPhysicalFilePath(ret);
        }
        return ret;
    }

    #region page events

    protected void Page_Load(object sender, EventArgs e)
    {
        string Date1 = DateTime.UtcNow.ToString("s") + "Z";
        string Date2 = DateTime.UtcNow.AddDays(1).ToString("s") + "Z";

        System.IO.StreamReader FS = System.IO.File.OpenText(GetSAMLRootUrl(_SAML_A_Filename, true));
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
        SAML_A_Swap = SAML_A_Swap.Replace("{{Destination}}", GetSSORootUrl(_ssoPage, true));
        SAML_A_Swap = SAML_A_Swap.Replace("{{Recipient}}", GetSSORootUrl(_ssoPage, true));

        FS = System.IO.File.OpenText(GetSAMLRootUrl(_SAML_B_Filename, true));
        SAML_B = FS.ReadToEnd();
        FS.Close();

        FS = System.IO.File.OpenText(GetSAMLRootUrl(_SAML_C_Filename, true));
        SAML_C = FS.ReadToEnd();
        FS.Close();

        litPostbackUrl.Text = SAMLPostBackAbsoluteUrl;
        btnSubmitLogin.PostBackUrl = SAMLPostBackRelativeUrl;
    }

    #endregion page events

    #region general events

    protected void cmdCheckCompany_Click(object sender, EventArgs e)
    {
        LiteralMessage.Text = "";
        int intCompanyID = 0;
        int.TryParse(txtCompanyID.Text, out intCompanyID);
        // Database is on TNN_CLONE
        DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ItemID, PartnerName, CustomerName FROM dbo.customtable_Customers WHERE ItemID = {0}", intCompanyID.ToString()), null, QueryTypeEnum.SQLQuery);
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
            if (_useSampleSAML)
            {
                System.IO.StreamReader FS = System.IO.File.OpenText(GetPhysicalFilePath(_SampleRoot + "/" + "quality-refrig-service-1.xml"));
                SAMLResponse.Value = FS.ReadToEnd();
                FS.Close();
            }
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
            Response.Redirect(SAMLPostBackAbsoluteUrl, false);
        }
    }

    #endregion general events

    #region certicates and SAML

    /// <summary>
    /// Get X509 from a file.
    /// </summary>
    /// <returns></returns>
    public X509Certificate2 GetX509Certificate2()
    {
        X509Certificate2 cert = new X509Certificate2();
        string path = GetCertificateFileUrl(PfxFileName);
        string password = PfxPassword;
        cert = new X509Certificate2(path, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        _rsa = (RSACryptoServiceProvider)cert.PrivateKey;
        _rsaParams = _rsa.ExportParameters(true);
        return cert;
    }

    /// <summary>
    /// Get X509 cert from a string
    /// </summary>
    /// <param name="X509Certificate"></param>
    /// <returns></returns>
    public X509Certificate2 GetX509Certificate2(string X509Certificate)
    {
        X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(X509Certificate), _pfxPassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        _rsa = (RSACryptoServiceProvider)cert.PrivateKey;
        if (_rsa != null)
        {
            _rsaParams = _rsa.ExportParameters(true);
        }

        return cert;
    }

    public string SignXmlWithCertificate(XmlDocument Document, X509Certificate2 cert)
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

    /// <summary>
    /// Verify the signature of an XML file and return the result.
    /// </summary>
    /// <param name="xmlDocument"></param>
    /// <returns></returns>
    public Boolean VerifyXmlFile(XmlDocument xmlDocument)
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

    private byte[] GetBytesFromPEM(string pemString, string section)
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

    private string ToBase64String(string FullName, string Email, string CompanyID)
    {
        byte[] bytes;

        bytes = System.Text.Encoding.UTF8.GetBytes(this.ToSAMLResponse(FullName, Email, CompanyID));

        return Convert.ToBase64String(bytes);
    }

    public string ToSAMLResponse(string FullName, string Email, string CompanyID)
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
        //X509Certificate2 cert = GetX509Certificate2(x509);
        if (cert != null)
        {
            return SignXmlWithCertificate(doc, cert);
        }
        return doc.OuterXml;
    }

    #endregion certicates and SAML

    #region url helpers

    /// <summary>
    /// Get the file path
    /// </summary>
    /// <param name="p_filename"></param>
    /// <returns></returns>
    public string GetPhysicalFilePath(string p_filename)
    {
        string ret = p_filename;

        if (String.IsNullOrWhiteSpace(p_filename))
        {
            // never in this app.
            ret = "~/";
        }

        if (ret.Contains("~/"))
        {
            ret = URLHelper.GetPhysicalPath(ret);
        }
        else
        {
            ret = Request.MapPath(ret);
        }

        return ret;
    }

    /// <summary>
    /// Get the absolute path from virtual
    /// </summary>
    /// <param name="p_path"></param>
    /// <returns></returns>
    public string GetAbsoluteURL(string p_path)
    {
        string ret = String.Empty;
        ret = URLHelper.GetAbsoluteUrl(p_path);
        return ret;
    }

    #endregion url helpers
}