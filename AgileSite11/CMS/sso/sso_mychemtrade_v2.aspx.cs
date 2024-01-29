using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.SiteProvider;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Web;
using System.Xml;

public partial class sso_mychemtrade_v2 : System.Web.UI.Page
{
    #region Global Variables

    private int[] mychemtrade_CompanyID = new int[] { 1, 5051 };
    private int UserID = 0;
    private int CompanyID = 1;
    private int CurrentCompanyID = 0;
    private bool IsLoggedIn = false;
    private string FullName = "Eric Garrison";
    private string EmailAddress = "egarrison@wte.net";
    private string SAMLResponse = "";
    private string CurrentUsername = "";
    private string CurrentFullName = "";
    private int GivenRoleID = 120; // RoleID given for this SSO
    private string OtherRolesCantUseSSO = "2,3,53,119,118,142,117";

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

    private const string SSOErrorURL = "http://www.trainingnetworknow.com/system/SSOError?SSOLogID=";
    private const string SSOOkURL2 = "https://www.trainingnetworknow.com";
    private const string SSOOkURL = "https://www.trainingnetworknow.com/home.aspx";
    private const string SSOkURLHomePath = "/home.aspx";
    private const string SSOkURLLibPath = "/courses/library";

    /// =============== Cerfificates ================== ///
    private const string X509CertificateOneLogin = "MIIEJjCCAw6gAwIBAgIUC25vCBpNXcjj6SY8VNXrLvDTL5swDQYJKoZIhvcNAQEFBQAwXTELMAkGA1UEBhMCVVMxFjAUBgNVBAoMDVdURSBTb2x1dGlvbnMxFTATBgNVBAsMDE9uZUxvZ2luIElkUDEfMB0GA1UEAwwWT25lTG9naW4gQWNjb3VudCA4NTM3NzAeFw0yMTA2MDkyMzQ4MDlaFw0yNjA2MDkyMzQ4MDlaMF0xCzAJBgNVBAYTAlVTMRYwFAYDVQQKDA1XVEUgU29sdXRpb25zMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgODUzNzcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCgO8rrqiARDjhdcoeufMKYHPTsrWAr4j9Q5ATWNqeUTsaUsLGxZlX7uXmZmGpeSjiEnAk0h3s5K77xrr1Q3838Acl3zp7rLZwu0g/9mHRLrTCMrBLk41qB9GF06c8I+TgWc2QNBzW7+wS7hAlHjSke/ummE4UxY55mhkFqTfY+9BQJhfoh78dXGjxGRkHJzFqc8vx0f8VAiX/VTUf5ScOMuo1FofO+BCyXao5zZEut5PPtxe8f++OvhiOcjvzcBfAbyMUGaK7Cg3UiG5wr9fgotFfNqsTvCmMYtUuV3jxNTNQ4r4zr9pPQpqm/FQd0Y4CuTLGlgHzC/qpXHdIUkXzDAgMBAAGjgd0wgdowDAYDVR0TAQH/BAIwADAdBgNVHQ4EFgQUpQ7Tp+Nh8vM/NgS1cyrnPc/CA0IwgZoGA1UdIwSBkjCBj4AUpQ7Tp+Nh8vM/NgS1cyrnPc/CA0KhYaRfMF0xCzAJBgNVBAYTAlVTMRYwFAYDVQQKDA1XVEUgU29sdXRpb25zMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgODUzNzeCFAtubwgaTV3I4+kmPFTV6y7w0y+bMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAVHajxqhbkhuPKWHVWLKkbwKAST8WdY0Wwc+beoOnr19bsODmOe5usJInry/YslFmz9Le+JHh3xkri0eD5FGlEDouxk0eCJ3NhS+kH0VLoFyN2DoBwyHXfj0jSa30/cSV6R4bokbDGGBp3xa30JrvyQEOOqi63FJNXPvcF53pVJXN3072sDsZTVhNNup6CSD95fS5i6sr+tbwzSpl+tx3ldPJDLhugrSCADAAHYCxpCzqR0jVEJW3Vq3ewHmWKmEjxj1ld04HkvEsyGCxHPZ0BGsqU4LMk+OoqCW1Fu52RlF4atT2NsI79GvkhnHUyA+IcQBOQFFF3GS9haR+w+v6wA==";

    private const string X509CertificateOneO365 = "MIIC8DCCAdigAwIBAgIQHTKEFFWLiIhABrJj7FBBZjANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMTA2MTEwMTIyNTNaFw0yNDA2MTEwMTIyNTNaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr5jmUn/0e19DEf0PtHnMUN9Vejx9MKIcj28z1NTdKgbjZ3O3th6p+c2mSrbGkSiaZdmWUTjrSUDagUiDSVsEUvk/5q/dRWJhjb24X/oRHOHmKpaWvbwfS74IMYlUUaTF9RlRlYPFJ5SIjUi7WsOqbv0i6giK2gbvvJrz/3OuuFTltPdsJOuU0ct/q29mWRLIH+YrVw9l/REGMIj4ivhzaY64nStOEQMm64K1IqoK6bzZ5qYH+m4OZpzMMq+oWeEvRUB5vNqKqOL6JmSTRe4FqrxNxikVeUMqHsXuMVLK9czPD7ipVgqib74pWG3kNIAPzesH9RpKfVJxpmzdtB0B2QIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQA06N2he7SAunEwLP/tbG+OAV2mFue3ASw3yvZ7vEFlooLGU50Xur1/MD3KtbDNDS5Ph/Ps/FsbWB68/QFwjQ/OPJDNWgHZUixDy8A0cCBJXcURY9HjJhMn2bwSJvy0F+tabACFY+EAb6sB59Bfi90T3SxRIXCXiK7HNAS89blDVx5h9bUL/XZP/TrCkJF7KBqh9rmIW4cGUw0Xa7cgb+js97DIVZRJlWB6P7q42QbcbpRnEADP2x0xoeCGYaM5u+LIyTI4ObKoKn7tf7MAgNF27puL3qhmIYNskE9LM+BQAw4IoU5M8YmorHz6pb/ye1JsgCr/HuQ11nknN+ikvahI";
    private const string X509CertificateChemTrade = "MIIGHzCCBQegAwIBAgIQAxNPuFogrbE+YG6047ZxFDANBgkqhkiG9w0BAQsFADBeMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMR0wGwYDVQQDExRHZW9UcnVzdCBSU0EgQ0EgMjAxODAeFw0yMTAzMTEwMDAwMDBaFw0yMjA0MTEyMzU5NTlaMHoxCzAJBgNVBAYTAkNBMRAwDgYDVQQIEwdPbnRhcmlvMRMwEQYDVQQHEwpOb3J0aCBZb3JrMSEwHwYDVQQKExhDaGVtdHJhZGUgTG9naXN0aWNzIEluYy4xITAfBgNVBAMMGCouY2hlbXRyYWRlbG9naXN0aWNzLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJxBJNL5DYBJdrPgIrcXJ6j3iJdzkPWJdmn3TXrJnknhu6X7VwyQIWmln0a3+i8ZYB97Ck8z0mQvLGJWBE2QJXk6Y4NmkObBrsScRkmt7UDKOCEnUzM3HsE2wnvfOAiYM0qx8PQsUEyaArs4ZnxuDg8DfwQgFTrfrGEQUyeOn2G4MwnT3UJx4xkQBBf1Px/7PDg4GmjVBFjYG4HH45I5Bs1DmM5FWBAt1AwL7byFboIci6hZh4C4rtRB/S04Q6YUKQ7StKpp6XhSYL0ubUw/GSI6oUeM1WRKXqNwDTpm8P5RmK7dwjDeK8BmZ4ci8HrULtjDEPGBVcoMINwGlnaLv7UCAwEAAaOCArswggK3MB8GA1UdIwQYMBaAFJBY/7CcdahRVHex7fKjQxY4nmzFMB0GA1UdDgQWBBTIqc+rmbkE9LMi/ZIXPJY4Hsb9gTA7BgNVHREENDAyghgqLmNoZW10cmFkZWxvZ2lzdGljcy5jb22CFmNoZW10cmFkZWxvZ2lzdGljcy5jb20wDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjA+BgNVHR8ENzA1MDOgMaAvhi1odHRwOi8vY2RwLmdlb3RydXN0LmNvbS9HZW9UcnVzdFJTQUNBMjAxOC5jcmwwPgYDVR0gBDcwNTAzBgZngQwBAgIwKTAnBggrBgEFBQcCARYbaHR0cDovL3d3dy5kaWdpY2VydC5jb20vQ1BTMHUGCCsGAQUFBwEBBGkwZzAmBggrBgEFBQcwAYYaaHR0cDovL3N0YXR1cy5nZW90cnVzdC5jb20wPQYIKwYBBQUHMAKGMWh0dHA6Ly9jYWNlcnRzLmdlb3RydXN0LmNvbS9HZW9UcnVzdFJTQUNBMjAxOC5jcnQwDAYDVR0TAQH/BAIwADCCAQIGCisGAQQB1nkCBAIEgfMEgfAA7gB1ACl5vvCeOTkh8FZzn2Old+W+V32cYAr4+U1dJlwlXceEAAABeCLz/HEAAAQDAEYwRAIgbQ9x3SW3o/JTQfEohMe3aYJAVMO8YQ1rd2N3YTHMB6ECIGkezVyKQ6pkUj8Rqqy/rT5l4gbgu4DJIlF4B6oXEHWdAHUAIkVFB1lVJFaWP6Ev8fdthuAjJmOtwEt/XcaDXG7iDwIAAAF4IvP8gwAABAMARjBEAiA+IRj2Bk6k+rsZRMFRCnbdOLGhaeDaca7kEjE2wVnwOwIgATt3vOEwHhtQhXau7QoU7YVN4Bu9oEgLbSqV8KltBTIwDQYJKoZIhvcNAQELBQADggEBAHfXJxhjIbUVY/wlQD5KbgRbYQ4GvHJB6rXa+HG2y0F9OcCFL7opTnRWBpzvkdlrbY+bOUyzX9ndCdnJ0LJ8nbyNut7N88elgSIOXDb0/Q0tCf0SAuWuMs91O6hC1Pk2O7eLVO5/K/oDJEpA5ivnBcKPnDcjR3NQ8G82c6h4xKomVP7FHtaa86ixqNHBta9wJJINGSK5+N/I05fGQ1Eik960BWNVxCf7jivfdW6lw+oXEHwjLleQbQzSAudUtLFNTJCoMlLn4Z7ZfywuaxYopOgKw5EUtBB50sXy+q/ELUonKvEGo85BD995zjuntR/OM8t2FXxDWcXq1WdkDG5sTio=";
    private string DecodedSAML = "";
    private string RawSAML = "";
    private string RawRequest = "";

    private string SSOLogMessage = "";
    private string LogFile = "";

    private XmlDocument _xmlDoc;

    private Int64 SSOLogID = 0;
    private string hash = "";

    #endregion Global Variables

    /*
    FOR LOGGING ERRORS:
    SELECT TOP 100 * FROM TN_SSOLog ORDER BY SSOLogID DESC

    TO create your own version, you will need to Create your own private/private key
    */

    protected void Page_Load(object sender, EventArgs e)
    {
        SAMLResponse = Request.Form["SAMLResponse"];
        RawSAML = Request.Form["SAMLResponse"];

        if (String.IsNullOrEmpty(SAMLResponse))
        {
            lblDebug.Text = "<br><br>" + "<b>NO SAML INJECTED !</b>";
            RawRequest = "AbsolutURL: [" + HttpContext.Current.Request.Url.AbsoluteUri + "] Headers: [" + Request.Headers.ToString() + "] Inputstring: " + Request.Form.ToString() + "]";
            SSOLogMessage = "NO SAML INJECTED !";

            var body = Request.InputStream;
            var encoding = Request.ContentEncoding;
            var reader = new System.IO.StreamReader(body, encoding);

            if (Request.ContentType != null)
            {
                lblDebug.Text += "<br><br>" + "<b>Content Type: </b>" + Request.ContentType;
                // lblDebug.Text = "<br><br>" + "<b>Content Length: </b>" + Request.ContentLength64.ToString();
            }

            lblDebug.Text += "<br><br>" + "<b>Body: </b>" + reader.ReadToEnd();

            RawRequest = reader.ReadToEnd();
            lblDebug.Text += "<br><br>" + "<b>AbsoluteURL:</b> " + HttpContext.Current.Request.Url.AbsoluteUri;
            lblDebug.Text += "<br><br>" + "<b>Request Headers:</b> " + Request.Headers.ToString();
            lblDebug.Text += "<br><br>" + "<b>Request InputString:</b> " + Request.Form.ToString();
            RawRequest = "AbsolutURL: [" + HttpContext.Current.Request.Url.AbsoluteUri + "] Headers: [" + Request.Headers.ToString() + "] Inputstring: " + Request.Form.ToString() + "]";

            SendSSOLog();

            body.Close();
            reader.Close();
            // lblDebug.Text += "<br><br>" + "<b>Request Body:</b> " +  Request.Body.ToString();
            // check body
        }

        // Logging

        if (!String.IsNullOrEmpty(SAMLResponse))
        {
            var body2 = Request.InputStream;
            var encoding2 = Request.ContentEncoding;
            var reader2 = new System.IO.StreamReader(body2, encoding2);

            if (Request.ContentType != null)
            {
                lblDebug.Text += "<br><br>" + "<b>Content Type: </b>" + Request.ContentType;
                // lblDebug.Text = "<br><br>" + "<b>Content Length: </b>" + Request.ContentLength64.ToString();
            }

            lblDebug.Text += "<br><br>" + "<b>Body: </b>" + reader2.ReadToEnd();
            RawRequest = reader2.ReadToEnd();
            SSOLogMessage = reader2.ReadToEnd();

            body2.Close();
            reader2.Close();
        }

        if (!String.IsNullOrEmpty(SAMLResponse))
        {
            lblDebug.Text += "<br><br>" + "<b>SAML:</b> " + SAMLResponse.ToString();
        }
        lblDebug.Text += "<br><br>" + "<b>AbsoluteURL:</b> " + HttpContext.Current.Request.Url.AbsoluteUri;
        lblDebug.Text += "<br><br>" + "<b>Request Headers:</b> " + Request.Headers.ToString();
        lblDebug.Text += "<br><br>" + "<b>Request InputString:</b> " + Request.Form.ToString();

        // Get the url referrer to check the proper certificate
        // SSOLogMessage = "Referer: " + Request.UrlReferrer.ToString();
        // SendSSOLog();

        RawRequest += "AbsolutURL: [" + HttpContext.Current.Request.Url.AbsoluteUri + "] Headers: [" + Request.Headers.ToString() + "] Inputstring: " + Request.Form.ToString() + "]";

        // If not empty and is signed
        if (!String.IsNullOrEmpty(SAMLResponse))
        {
            lblDebug.Text = "<br>" + "Started...";
            SAMLResponse = SAMLResponse.Replace("\r", "").Replace("\n", "");

            // Fonz Testing
            lblDebug.Text += "<br><br>" + "<b>Fonz conversion test:</b> " + FonzGetSAMLConvert(SAMLResponse);
            DecodedSAML = FonzGetSAMLConvert(SAMLResponse);
            SendSSOLog();

            // ERIC TESTING
            lblDebug.Text += "<br><br>" + "<b>Eric:</b> " + Request.Form.ToString();
            lblDebug.Text += "<br><br>" + "<b>EricSAML:</b> " + Request.Form.ToString();

            // Convert

            try
            {
                // byte[] b = Convert.FromBase64String(DecodedSAML);
                // string strSAMLXML = System.Text.Encoding.UTF8.GetString(b);
                XmlDocument samlXMLDoc = new XmlDocument();
                samlXMLDoc.LoadXml(DecodedSAML);

                // if (VerifyXml(samlXMLDoc, GetX509Certificate2())) // Get cert then verify
                // {
                XmlNamespaceManager nsmanager = new XmlNamespaceManager(samlXMLDoc.NameTable);
                nsmanager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

                XmlNodeList nodelist = samlXMLDoc.SelectNodes("//saml2:Attribute", nsmanager);
                foreach (XmlNode node in nodelist)
                {
                    foreach (XmlAttribute sAttrib in node.Attributes)
                    {
                        if (sAttrib.Name.ToLower().Equals("name", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string sPropertyName = sAttrib.Value;
                            string sPropertyValue = node.FirstChild.InnerText;
                            switch (sPropertyName.ToLower())
                            {
                                case "fullname":
                                case "http://schemas.microsoft.com/identity/claims/displayname":
                                    FullName = sPropertyValue;
                                    break;

                                case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress":
                                case "email":
                                    EmailAddress = sPropertyValue;
                                    if (EmailAddress.Trim().Length > 0 && EmailAddress.IndexOf('@') == -1)
                                    {
                                        EmailAddress = EmailAddress.Trim() + "@csod.local";
                                    }
                                    break;

                                case "companyid":
                                    CompanyID = Convert.ToInt32(sPropertyValue);
                                    //int.TryParse(sPropertyValue, out CompanyID);
                                    break;

                                default:
                                    break;
                            }
                            CompanyID = 5051;
                        }
                    }
                }

                EmailAddress = EmailAddress.Trim();

                if (FullName.Length == 0 && EmailAddress.Length > 0)
                {
                    int idx = EmailAddress.IndexOf("@");
                    if (idx > -1)
                    {
                        FullName = EmailAddress.Substring(0, idx);
                    }
                }

                if (CompanyID == 0 && EmailAddress.Length > 0)
                    CompanyID = 5051;

                if (FullName.Length > 0 && EmailAddress.Length > 0 && CompanyID > 0)
                {
                    if (CompanyID > 0)
                    {
                        if (Array.IndexOf(mychemtrade_CompanyID, CompanyID) > -1)
                        {
                            if (EmailAddress.IndexOf('@') > -1)
                            {
                                lblDebug.Text = lblDebug.Text + "<br> Found user...";
                                //ViewLiteral.Text = FullName + " " + Email + " " + CompanyID;
                            }
                            else
                            {
                                lblDebug.Text = lblDebug.Text + "<br>" + "Invalid email address...";
                                SSOLogMessage = "Invalid email address.";
                            }
                        }
                        else
                        {
                            lblDebug.Text = lblDebug.Text + "<br>" + "Company ID is not in the list for this SSO...";
                            SSOLogMessage = "Company ID is not in the list for this SSO.";
                        }
                    }
                    else
                    {
                        lblDebug.Text = lblDebug.Text + "<br>" + "Invalid Company ID....";
                        SSOLogMessage = "Invalid Company ID.";
                    }
                }
                else
                {
                    lblDebug.Text = lblDebug.Text + "<br>" + "Please pass FullName, Email and CompanyID in the attribute node (saml2:Attribute)....";
                    SSOLogMessage = "Please pass FullName, Email and CompanyID in the attribute node (saml2:Attribute).";
                }

                if (SSOLogMessage.Length == 0)
                {
                    // Check current company from current EmailAddress
                    CheckUserAndCompanyFromDB();

                    int TotalOtherRoles = TotalRolesOtherThenDefault();
                    if (TotalOtherRoles > 0)
                    {
                        lblDebug.Text += "<br>" + "This user has other roles, cannot use SSO.";
                        SSOLogMessage += "This user has other roles, cannot use SSO.";
                    }

                    if (SSOLogMessage.Length == 0)
                    {
                        // if exists and companyid different, change companyid
                        if ((CurrentCompanyID != CompanyID) && (UserID > 0))
                        {
                            UpdateNewCompanyID();
                        }

                        // if exists and Fullname is different, change fullname
                        if ((CurrentFullName != FullName) && (UserID > 0))
                        {
                            UpdateNewFullname();
                        }

                        // === if exists and companyid same, log in
                        if (UserID > 0 && CurrentUsername.Length > 0)
                        {
                            lblDebug.Text += "<br> Creds {{ UserID: " + UserID.ToString() + ", Email: " + EmailAddress.ToString() + ", UserName: " + CurrentUsername.ToString() + "}}";
                            string url = "";
                            // === Check if User is allowed to login
                            if (AuthenticationHelper.CanUserLogin(UserID))
                            {
                                lblDebug.Text += "<br>" + "User can login...";
                                // === Login
                                if (AuthenticationHelper.IsAuthenticated())
                                {
                                    lblDebug.Text += "<br>" + "User is already Authenticated...";
                                }

                                //  === GET USER object and login the user
                                UserInfo userInfo = UserInfoProvider.GetUserInfo(CurrentUsername);

                                //==== Gets the authentication URL for a specified user and target URL

                                // ***** CHANGE URL LANDING HERE ****** //
                                // example: '/default.apx'
                                url = AuthenticationHelper.GetUserAuthenticationUrl(userInfo, SSOkURLLibPath);
                                // ************************************ //

                                lblDebug.Text += lblDebug.Text + "<br>" + "Logging in....";
                                lblDebug.Text += lblDebug.Text + "<br>" + "url = " + url;
                                SendSSOLog();

                                // Change the url to home
                                var testingMode = true;

                                // CHECK if SAML is signed
                                // CHECK if doc is signed
                                CheckAndLogCert(samlXMLDoc);

                                // Successfully login.. no user change
                                URLHelper.Redirect(url);
                            }
                            else // User is not allowed to login
                            {
                                lblDebug.Text += "<br>" + "Requested User cannot login...";
                            }
                        }

                        if (UserID == 0)
                        {
                            // how to create a user
                            UserInfo newUser = new UserInfo();
                            newUser.UserName = EmailAddress;
                            newUser.FullName = FullName;
                            newUser.Enabled = true;
                            newUser.Email = EmailAddress;
                            UserInfoProvider.SetUserInfo(newUser);
                            Random random = new Random();
                            int pass = random.Next(100000);
                            UserInfoProvider.SetPassword(newUser, "SSO_Pass" + pass);

                            // set a new role
                            UserRoleInfo roles = new UserRoleInfo();
                            roles.UserID = newUser.UserID;
                            roles.RoleID = GivenRoleID;
                            UserRoleInfoProvider.SetUserRoleInfo(roles);

                            // set a company
                            UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(newUser.UserID);
                            userSettings.SetValue("UserCompany", CompanyID);
                            UserSettingsInfoProvider.SetUserSettingsInfo(userSettings);

                            UserInfoProvider.AddUserToSite(newUser.UserName, SiteContext.CurrentSiteName);

                            // Now Login
                            AuthenticationHelper.AuthenticateUser(newUser.UserName, false);
                            IsLoggedIn = true;
                            lblDebug.Text = lblDebug.Text + "<br>" + "Successfuly LoggedIN";
                            SendSSOLog();

                            // CHECK if doc is signed
                            CheckAndLogCert(samlXMLDoc);

                            // User moded, now login
                            Response.Redirect(SSOOkURL);
                        }
                    }
                    else
                    {
                        SendSSOLog();
                        Response.Redirect(SSOErrorURL + SSOLogID);
                    }
                }
                else
                {
                    SendSSOLog();
                    Response.Redirect(SSOErrorURL + SSOLogID);
                }
                // }
            }
            catch (Exception ex)
            {
                SSOLogMessage = ex.Message.ToString();
                lblDebug.Text += "<br>" + "Cannot decode" + ex.Message;
                SendSSOLog();
            }
        }
    }

    protected void CheckAndLogCert(XmlDocument samlXMLDoc)
    {
        // CHECK if doc is signed
        try
        {
            if (VerifyXml(samlXMLDoc, GetX509Certificate2(X509CertificateOneLogin)))
            {
                SSOLogMessage = "X509 worked!";
                SendSSOLog();
            }
            else if (VerifyXml(samlXMLDoc, GetX509Certificate2(X509CertificateOneO365)))
            {
                SSOLogMessage = "X509 worked!";
                SendSSOLog();
            }
            else if (VerifyXml(samlXMLDoc, GetX509Certificate2(X509CertificateChemTrade)))
            {
                SSOLogMessage = "X509 worked!";
                SendSSOLog();
            }
            else
            {
                SSOLogMessage = "X509 Failed!";
                SendSSOLog();
                Response.Redirect(SSOErrorURL + SSOLogID);
            }
        }
        catch (Exception ex)
        {
            SSOLogMessage = ex.Message.ToString();
            SendSSOLog();
            Response.Redirect(SSOErrorURL + SSOLogID);
        }
    }

    protected string FonzGetSAMLConvert(string rawSAMLDATA)
    {
        // Check if url encoded
        if (rawSAMLDATA.Contains('%'))
        {
            rawSAMLDATA = HttpUtility.UrlDecode(rawSAMLDATA);
        }

        // check if true base64
        bool isBase64 = false;

        try
        {
            byte[] samlData = Convert.FromBase64String(rawSAMLDATA);
            return Encoding.UTF8.GetString(samlData);
        }
        catch (Exception ex)
        {
        }
        return rawSAMLDATA;
    }

    protected void LoadXml(string xml)
    {
        _xmlDoc = new XmlDocument();
        _xmlDoc.PreserveWhitespace = true;
        _xmlDoc.XmlResolver = null;
        _xmlDoc.LoadXml(xml);
        // _xmlNameSpaceManager = GetNamespaceManager(); //lets con struct a "manager" for XPath queries
    }

    protected void LoadXmlFromBase64(string response)
    {
        UTF8Encoding enc = new UTF8Encoding();
        LoadXml(enc.GetString(Convert.FromBase64String(response)));
    }

    protected X509Certificate2 GetX509Certificate2(string X509Certificate)
    {
        // O365 X509CertificateOneO365
        return new X509Certificate2(Convert.FromBase64String(X509Certificate), _pfxPasword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

        // OneLogin
    }

    protected static bool VerifyXml(XmlDocument Doc, X509Certificate2 storeCert)
    {
        if (Doc == null)
            throw new ArgumentException("Doc");
        SignedXml signedXml = new SignedXml(Doc);
        var nsManager = new XmlNamespaceManager(Doc.NameTable);
        nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
        var node = Doc.SelectSingleNode("//ds:Signature", nsManager);
        // find signature node
        var certElement = Doc.SelectSingleNode("//ds:X509Certificate", nsManager);
        // find certificate node
        signedXml.LoadXml((XmlElement)node);

        return signedXml.CheckSignature(storeCert, true);
        //^^^ If certificate is installed in the Root location then
        //this method returns true after validating it as well
        //In addition to validating the signature
    }

    protected int TotalRolesOtherThenDefault()
    {
        int Total = 0;
        // select * from CMS_UserRole WHERE UserID = 100 AND RoleID <> 121
        try
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT * FROM CMS_UserRole WHERE UserID = {0} AND RoleID IN ({1})", UserID, OtherRolesCantUseSSO), null, QueryTypeEnum.SQLQuery);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                Total = ds.Tables[0].Rows.Count;
            }
        }
        catch (Exception ex)
        {
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            SendSSOLog();
            Response.Redirect(SSOErrorURL + SSOLogID);
        }
        return Total;
    }

    protected void CheckUserAndCompanyFromDB()
    {
        try
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ISNULL(US.FullName,'') As Fullname, US.UserID AS UserID, ISNULL(US.Username,'') As Username, ISNULL(UserCompany,0) AS CompanyID FROM CMS_User US LEFT OUTER JOIN CMS_UserSettings ST ON US.UserID = ST.UserSettingsUserID WHERE US.Email = '{0}'", EmailAddress), null, QueryTypeEnum.SQLQuery);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                int.TryParse(row["CompanyID"].ToString(), out CurrentCompanyID);
                int.TryParse(row["UserID"].ToString(), out UserID);
                CurrentUsername = row["Username"].ToString();
                CurrentFullName = row["Fullname"].ToString();
            }
            else
            {
                CurrentCompanyID = 0;
            }
        }
        catch (Exception ex)
        {
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            SendSSOLog();
            Response.Redirect(SSOErrorURL + SSOLogID);
        }
    }

    protected void UpdateNewCompanyID()
    {
        try
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("UPDATE dbo.CMS_UserSettings SET UserCompany = {0} WHERE UserSettingsUserID = {1}", CompanyID, UserID), null, QueryTypeEnum.SQLQuery);
            SSOLogMessage += "CompanyID changed.";
        }
        catch (Exception ex)
        {
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            SendSSOLog();
            Response.Redirect(SSOErrorURL + SSOLogID);
        }
    }

    protected void UpdateNewFullname()
    {
        try
        {
            FullName = FullName.Replace(";", "").Replace("'", "");
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("UPDATE dbo.CMS_User SET Fullname = '{0}' WHERE UserID = {1}", FullName, UserID), null, QueryTypeEnum.SQLQuery);
            SSOLogMessage += "Fullname changed.";
        }
        catch (Exception ex)
        {
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            SendSSOLog();
            Response.Redirect(SSOErrorURL + SSOLogID);
        }
    }

    #region HASHVerification

    // Verify a hash against a string.
    protected bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
    {
        // Hash the input.
        string hashOfInput = GetMd5Hash(md5Hash, input);

        // Create a StringComparer an compare the hashes.
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        if (0 == comparer.Compare(hashOfInput, hash))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

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

    #endregion HASHVerification

    #region UnunsedFuctions

    protected void CheckCompanyFromDB()
    {
        try
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ItemID, PartnerName, CustomerName FROM dbo.customtable_Customers WHERE ItemID = {0}", CompanyID), null, QueryTypeEnum.SQLQuery);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                int.TryParse(row["ItemID"].ToString(), out CompanyID);
            }
            else
            {
                CompanyID = 0;
            }
        }
        catch (Exception ex)
        {
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            SendSSOLog();
            Response.Redirect(SSOErrorURL + SSOLogID);
        }
    }

    #endregion UnunsedFuctions

    #region Logging

    /// <summary>
    /// Call this one from anywhere
    /// </summary>
    /// <param name="message"></param>
    protected void LogThis(string message)
    {
        try
        {
            if (LogFile.Length == 0)
            {
                CreateLogFile();
            }
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

    protected void SendSSOLog()
    {
        try
        {
            QueryDataParameters prs = new QueryDataParameters();
            prs.Add(new DataParameter("EmailAddress", EmailAddress));
            prs.Add(new DataParameter("FullName", FullName));
            prs.Add(new DataParameter("CompanyID", CompanyID));
            prs.Add(new DataParameter("CurrentCompanyID", CurrentCompanyID));
            prs.Add(new DataParameter("UserID", UserID));
            prs.Add(new DataParameter("IPAddress", Request.ServerVariables["REMOTE_ADDR"]));
            prs.Add(new DataParameter("IsLoggedIn", IsLoggedIn ? 1 : 0));
            prs.Add(new DataParameter("LogMessage", SSOLogMessage));
            prs.Add(new DataParameter("DecodedSAML", DecodedSAML));
            prs.Add(new DataParameter("RawSAML", RawSAML));
            prs.Add(new DataParameter("RawRequest", RawRequest));

            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.Proc_TN_SSOLog_Insert", prs, QueryTypeEnum.StoredProcedure);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                Int64.TryParse(row["SSOLogID"].ToString(), out SSOLogID);
            }
        }
        catch (Exception ex)
        {
            // won't be in the log
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            Response.Redirect(SSOErrorURL + 0);
        }
    }

    protected void KenticoLogEvent(Exception ex)
    {
        try
        {
            // how to log an event/error
            EventLogInfo log = new EventLogInfo();
            log.EventType = "E";
            log.Source = "SSO c# code error";
            log.EventCode = "EXCEPTION";
            log.EventDescription = ex.Message;
            EventLogProvider.SetEventLogInfo(log);
        }
        catch
        {
        }
    }

    #endregion Logging
}