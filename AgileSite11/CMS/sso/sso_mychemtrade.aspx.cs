using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

/// <summary>
/// Handles SSO (ADFS/O365 login specifically for chemtrade)
/// </summary>
public partial class sso_mychemtrade : System.Web.UI.Page
{
    #region "Constants"

    #region "Certificate Paths"

    /// ================ Paths ===================== ///
    private const string _pfxPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pfx";

    private const string _combPemPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.combined.pem";
    private const string _pemPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pem"; // requires password
    private const string _publicPemCert = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pub.crt";
    private const string _publicCert = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.pub.crt";
    private const string _privateKeyPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.key";
    private const string _publicKeyPath = "F:\\websites\\AS11_Production_TNN\\sso\\Certs\\trainingnetwork.key";

    /// =============== Passwords ================== ///
    private const string _pfxPassword = "TiR4DD3v0lp3R$";
    private const string _certSerial = "03ac2065a148ffcf075efadaf2db2ff1";

    #endregion "Certificate Paths"

    #region "URL Redirection"

    private const string SSOErrorURL = "/system/SSOError";
    private const string SSOOkURL2 = "/";
    private const string SSOOkURL = "/home.aspx";

    private const string SSOkURLHomePath = "/home";
    private const string SSOkURLLibPath = "/courses/library";

    #endregion "URL Redirection"

    #region "static certificates"

    /// =============== Cerfificates ================== ///
    private const string X509CertificateOneLogin = "MIIEJjCCAw6gAwIBAgIUC25vCBpNXcjj6SY8VNXrLvDTL5swDQYJKoZIhvcNAQEFBQAwXTELMAkGA1UEBhMCVVMxFjAUBgNVBAoMDVdURSBTb2x1dGlvbnMxFTATBgNVBAsMDE9uZUxvZ2luIElkUDEfMB0GA1UEAwwWT25lTG9naW4gQWNjb3VudCA4NTM3NzAeFw0yMTA2MDkyMzQ4MDlaFw0yNjA2MDkyMzQ4MDlaMF0xCzAJBgNVBAYTAlVTMRYwFAYDVQQKDA1XVEUgU29sdXRpb25zMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgODUzNzcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCgO8rrqiARDjhdcoeufMKYHPTsrWAr4j9Q5ATWNqeUTsaUsLGxZlX7uXmZmGpeSjiEnAk0h3s5K77xrr1Q3838Acl3zp7rLZwu0g/9mHRLrTCMrBLk41qB9GF06c8I+TgWc2QNBzW7+wS7hAlHjSke/ummE4UxY55mhkFqTfY+9BQJhfoh78dXGjxGRkHJzFqc8vx0f8VAiX/VTUf5ScOMuo1FofO+BCyXao5zZEut5PPtxe8f++OvhiOcjvzcBfAbyMUGaK7Cg3UiG5wr9fgotFfNqsTvCmMYtUuV3jxNTNQ4r4zr9pPQpqm/FQd0Y4CuTLGlgHzC/qpXHdIUkXzDAgMBAAGjgd0wgdowDAYDVR0TAQH/BAIwADAdBgNVHQ4EFgQUpQ7Tp+Nh8vM/NgS1cyrnPc/CA0IwgZoGA1UdIwSBkjCBj4AUpQ7Tp+Nh8vM/NgS1cyrnPc/CA0KhYaRfMF0xCzAJBgNVBAYTAlVTMRYwFAYDVQQKDA1XVEUgU29sdXRpb25zMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgODUzNzeCFAtubwgaTV3I4+kmPFTV6y7w0y+bMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAVHajxqhbkhuPKWHVWLKkbwKAST8WdY0Wwc+beoOnr19bsODmOe5usJInry/YslFmz9Le+JHh3xkri0eD5FGlEDouxk0eCJ3NhS+kH0VLoFyN2DoBwyHXfj0jSa30/cSV6R4bokbDGGBp3xa30JrvyQEOOqi63FJNXPvcF53pVJXN3072sDsZTVhNNup6CSD95fS5i6sr+tbwzSpl+tx3ldPJDLhugrSCADAAHYCxpCzqR0jVEJW3Vq3ewHmWKmEjxj1ld04HkvEsyGCxHPZ0BGsqU4LMk+OoqCW1Fu52RlF4atT2NsI79GvkhnHUyA+IcQBOQFFF3GS9haR+w+v6wA==";

    private const string X509CertificateOneO365 = "MIIC8DCCAdigAwIBAgIQHTKEFFWLiIhABrJj7FBBZjANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMTA2MTEwMTIyNTNaFw0yNDA2MTEwMTIyNTNaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr5jmUn/0e19DEf0PtHnMUN9Vejx9MKIcj28z1NTdKgbjZ3O3th6p+c2mSrbGkSiaZdmWUTjrSUDagUiDSVsEUvk/5q/dRWJhjb24X/oRHOHmKpaWvbwfS74IMYlUUaTF9RlRlYPFJ5SIjUi7WsOqbv0i6giK2gbvvJrz/3OuuFTltPdsJOuU0ct/q29mWRLIH+YrVw9l/REGMIj4ivhzaY64nStOEQMm64K1IqoK6bzZ5qYH+m4OZpzMMq+oWeEvRUB5vNqKqOL6JmSTRe4FqrxNxikVeUMqHsXuMVLK9czPD7ipVgqib74pWG3kNIAPzesH9RpKfVJxpmzdtB0B2QIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQA06N2he7SAunEwLP/tbG+OAV2mFue3ASw3yvZ7vEFlooLGU50Xur1/MD3KtbDNDS5Ph/Ps/FsbWB68/QFwjQ/OPJDNWgHZUixDy8A0cCBJXcURY9HjJhMn2bwSJvy0F+tabACFY+EAb6sB59Bfi90T3SxRIXCXiK7HNAS89blDVx5h9bUL/XZP/TrCkJF7KBqh9rmIW4cGUw0Xa7cgb+js97DIVZRJlWB6P7q42QbcbpRnEADP2x0xoeCGYaM5u+LIyTI4ObKoKn7tf7MAgNF27puL3qhmIYNskE9LM+BQAw4IoU5M8YmorHz6pb/ye1JsgCr/HuQ11nknN+ikvahI";
    private const string X509CertificateChemTrade = "MIIGHzCCBQegAwIBAgIQAxNPuFogrbE+YG6047ZxFDANBgkqhkiG9w0BAQsFADBeMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMR0wGwYDVQQDExRHZW9UcnVzdCBSU0EgQ0EgMjAxODAeFw0yMTAzMTEwMDAwMDBaFw0yMjA0MTEyMzU5NTlaMHoxCzAJBgNVBAYTAkNBMRAwDgYDVQQIEwdPbnRhcmlvMRMwEQYDVQQHEwpOb3J0aCBZb3JrMSEwHwYDVQQKExhDaGVtdHJhZGUgTG9naXN0aWNzIEluYy4xITAfBgNVBAMMGCouY2hlbXRyYWRlbG9naXN0aWNzLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJxBJNL5DYBJdrPgIrcXJ6j3iJdzkPWJdmn3TXrJnknhu6X7VwyQIWmln0a3+i8ZYB97Ck8z0mQvLGJWBE2QJXk6Y4NmkObBrsScRkmt7UDKOCEnUzM3HsE2wnvfOAiYM0qx8PQsUEyaArs4ZnxuDg8DfwQgFTrfrGEQUyeOn2G4MwnT3UJx4xkQBBf1Px/7PDg4GmjVBFjYG4HH45I5Bs1DmM5FWBAt1AwL7byFboIci6hZh4C4rtRB/S04Q6YUKQ7StKpp6XhSYL0ubUw/GSI6oUeM1WRKXqNwDTpm8P5RmK7dwjDeK8BmZ4ci8HrULtjDEPGBVcoMINwGlnaLv7UCAwEAAaOCArswggK3MB8GA1UdIwQYMBaAFJBY/7CcdahRVHex7fKjQxY4nmzFMB0GA1UdDgQWBBTIqc+rmbkE9LMi/ZIXPJY4Hsb9gTA7BgNVHREENDAyghgqLmNoZW10cmFkZWxvZ2lzdGljcy5jb22CFmNoZW10cmFkZWxvZ2lzdGljcy5jb20wDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjA+BgNVHR8ENzA1MDOgMaAvhi1odHRwOi8vY2RwLmdlb3RydXN0LmNvbS9HZW9UcnVzdFJTQUNBMjAxOC5jcmwwPgYDVR0gBDcwNTAzBgZngQwBAgIwKTAnBggrBgEFBQcCARYbaHR0cDovL3d3dy5kaWdpY2VydC5jb20vQ1BTMHUGCCsGAQUFBwEBBGkwZzAmBggrBgEFBQcwAYYaaHR0cDovL3N0YXR1cy5nZW90cnVzdC5jb20wPQYIKwYBBQUHMAKGMWh0dHA6Ly9jYWNlcnRzLmdlb3RydXN0LmNvbS9HZW9UcnVzdFJTQUNBMjAxOC5jcnQwDAYDVR0TAQH/BAIwADCCAQIGCisGAQQB1nkCBAIEgfMEgfAA7gB1ACl5vvCeOTkh8FZzn2Old+W+V32cYAr4+U1dJlwlXceEAAABeCLz/HEAAAQDAEYwRAIgbQ9x3SW3o/JTQfEohMe3aYJAVMO8YQ1rd2N3YTHMB6ECIGkezVyKQ6pkUj8Rqqy/rT5l4gbgu4DJIlF4B6oXEHWdAHUAIkVFB1lVJFaWP6Ev8fdthuAjJmOtwEt/XcaDXG7iDwIAAAF4IvP8gwAABAMARjBEAiA+IRj2Bk6k+rsZRMFRCnbdOLGhaeDaca7kEjE2wVnwOwIgATt3vOEwHhtQhXau7QoU7YVN4Bu9oEgLbSqV8KltBTIwDQYJKoZIhvcNAQELBQADggEBAHfXJxhjIbUVY/wlQD5KbgRbYQ4GvHJB6rXa+HG2y0F9OcCFL7opTnRWBpzvkdlrbY+bOUyzX9ndCdnJ0LJ8nbyNut7N88elgSIOXDb0/Q0tCf0SAuWuMs91O6hC1Pk2O7eLVO5/K/oDJEpA5ivnBcKPnDcjR3NQ8G82c6h4xKomVP7FHtaa86ixqNHBta9wJJINGSK5+N/I05fGQ1Eik960BWNVxCf7jivfdW6lw+oXEHwjLleQbQzSAudUtLFNTJCoMlLn4Z7ZfywuaxYopOgKw5EUtBB50sXy+q/ELUonKvEGo85BD995zjuntR/OM8t2FXxDWcXq1WdkDG5sTio=";

    #endregion "static certificates"

    #endregion "Constants"

    #region "Members"

    private XmlDocument _xmlDoc;

    private int[] allowed_companyID = new int[] { 1, 5051 };
    private int DefaultCompanyID = 5051;
    private int UserID = 0;
    private int CompanyID = 1;
    private int CurrentCompanyID = 0;
    private bool IsLoggedIn = false;
    private bool IsNewUser = false;
    private string FullName = null;
    private string EmailAddress = null;
    private string SAMLResponse = null;
    private string CurrentUsername = null;
    private string CurrentFullName = null;
    private string CurrentEmail = null;
    private string RedirectURL = null;

    private int GivenRoleID = 120; // RoleID given for this SSO
    //private string OtherRolesCantUseSSO = "2,3,53,119,118,142,117";
	private string OtherRolesCantUseSSO = "2";
    private string hash = null;

    #region "Requests"

    private string DecodedSAML = null;
    private string RawSAML = null;
    private string RawRequest = null;
    private string RequestURL = null;
    private string RequestReferrer = null;
    private string RequestHeader = null;
    private string RequestForm = null;
    private string RequestInputStream = null;
    private string RequestIP = null;
    private string RequestContentType = null;
    private string RequestContentLength = null;
    private string RequestUserAgent = null;
    private string RequestUserHostAddress = null;
    private string RequestUserHostName = null;
    private string Command = null;
    private string LoginMode = null;
    private string Mode = null;
    private string SSOmode = null;

    #endregion "Requests"

    #region "Logging"

    private Int64 SSOLogID = 0;
    private string SSOLogMessage = null;
    private string ErrorMessage = null;
    private bool HasError = false;
    private string LogFile = null;

    #endregion "Logging"

    #endregion "Members"

    #region "Page events"

    /// <summary>
    /// The page load event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load(object sender, EventArgs e)
    {
        string errorMessage = String.Empty;
        bool cancontinue = ProcessRequest(out errorMessage);
        ErrorMessage += errorMessage;

        if (cancontinue)
        {
            try
            {
                XmlDocument samlXMLDoc = null;
                try
                {
                    samlXMLDoc = new XmlDocument();
                    samlXMLDoc.LoadXml(DecodedSAML);
                }
                catch (Exception ex)
                {
                    // load XML failed (xml is malformed)
                    KenticoLogEvent(ex, "error loading XML");
                    cancontinue = false;
                }

                if (cancontinue)
                {
                    XmlNamespaceManager nsmanager = new XmlNamespaceManager(samlXMLDoc.NameTable);
                    nsmanager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
                    XmlNodeList nodelist = samlXMLDoc.SelectNodes("//saml2:Attribute", nsmanager);
                    foreach (XmlNode node in nodelist)
                    {
                        foreach (XmlAttribute attrib in node.Attributes)
                        {
                            if (attrib.Name.ToLower().Equals("name", StringComparison.InvariantCultureIgnoreCase))
                            {
                                string sPropertyName = attrib.Value;
                                string sPropertyValue = node.FirstChild.InnerText;
                                switch (sPropertyName.ToLower())
                                {
                                    case "fullname":
                                    case "http://schemas.microsoft.com/identity/claims/displayname":
                                        FullName = GetSafeString(sPropertyValue);
                                        break;

                                    case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress":
                                    case "email":
                                        EmailAddress = GetSafeString(sPropertyValue).Trim();
                                        // check for email without the domain?
                                        if (!String.IsNullOrWhiteSpace(EmailAddress) && EmailAddress.IndexOf('@') == -1)
                                        {
                                            EmailAddress = EmailAddress.Trim() + "@csod.local";
                                        }
                                        break;

                                    case "companyid":
                                        CompanyID = GetSafeInt(sPropertyValue);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    // validate the signature
                    cancontinue = CheckAndLogCert(samlXMLDoc, out errorMessage);
                    ErrorMessage += errorMessage;

                    if (cancontinue)
                    {
                        if (String.IsNullOrWhiteSpace(FullName) && !String.IsNullOrWhiteSpace(EmailAddress))
                        {
                            int idx = EmailAddress.IndexOf("@");
                            if (idx > -1)
                            {
                                FullName = EmailAddress.Substring(0, idx);
                            }
                        }

                        if (CompanyID <= 0 && !String.IsNullOrWhiteSpace(EmailAddress))
                        {
                            CompanyID = DefaultCompanyID;
                        }

                        if (CompanyID > 0 && !String.IsNullOrWhiteSpace(FullName) && !String.IsNullOrWhiteSpace(EmailAddress))
                        {
                            if (CompanyID > 0)
                            {
                                if (Array.IndexOf(allowed_companyID, CompanyID) > -1)
                                {
                                    if (EmailAddress.IndexOf('@') > -1)
                                    {
                                        //lblDebug.Text = lblDebug.Text + "<br> Found user...";
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
                                    cancontinue = false;
                                }
                            }
                            else
                            {
                                lblDebug.Text = lblDebug.Text + "<br>" + "Invalid Company ID....";
                                SSOLogMessage = "Invalid Company ID.";
                                cancontinue = false;
                            }
                        }
                        else
                        {
                            cancontinue = false;
                            lblDebug.Text = lblDebug.Text + "<br>" + "Please pass FullName, Email and CompanyID in the attribute node (saml2:Attribute)....";
                            SSOLogMessage = "Please pass FullName, Email and CompanyID in the attribute node (saml2:Attribute).";
                        }

                        if (cancontinue)
                        {
                            // check for existing user
                            CheckUserAndCompanyFromDB();

                            lblDebug.Text += "<br> Creds {{ UserID: " + GetSafeString(UserID) + ", Email: " + GetSafeString(EmailAddress) + ", UserName: " + GetSafeString(CurrentUsername) + "}}";

                            int TotalOtherRoles = TotalRolesOtherThenDefault();
                            if (TotalOtherRoles > 0)
                            {
                                lblDebug.Text += "<br>" + "This user has other roles, cannot use SSO.";
                                ErrorMessage += "This user has other roles, cannot use SSO.";
                                cancontinue = false;
                            }

                            if (cancontinue)
                            {
                                // grab the current user information from the database
                                UserInfo userInfo = null;

                                if (!String.IsNullOrWhiteSpace(CurrentUsername))
                                {
                                    // get the current user by username
                                    userInfo = UserInfoProvider.GetUserInfo(CurrentUsername);
                                }

                                if (userInfo == null && UserID > 0)
                                {
                                    // try getting the current user by the user id
                                    userInfo = UserInfoProvider.GetUserInfo(UserID);
                                }

                                if (userInfo == null)
                                {
                                    lblDebug.Text += "<br>" + "Creating new User...";
                                    // this is a new user
                                    userInfo = new UserInfo();
                                    userInfo.UserName = EmailAddress;
                                    userInfo.FullName = FullName;
                                    userInfo.Enabled = true;
                                    userInfo.Email = EmailAddress;
                                    UserInfoProvider.SetUserInfo(userInfo);
                                    Random random = new Random();
                                    int pass = random.Next(100000);
                                    UserInfoProvider.SetPassword(userInfo, "SSO_Pass" + pass);

                                    // set a new role
                                    UserRoleInfo roles = new UserRoleInfo();
                                    roles.UserID = userInfo.UserID;
                                    roles.RoleID = GivenRoleID;
                                    UserRoleInfoProvider.SetUserRoleInfo(roles);

                                    // set a company
                                    UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userInfo.UserID);
                                    userSettings.SetValue("UserCompany", CompanyID);
                                    UserSettingsInfoProvider.SetUserSettingsInfo(userSettings);
                                    UserInfoProvider.AddUserToSite(userInfo.UserName, SiteContext.CurrentSiteName);
                                    IsNewUser = true;
                                }

                                // login the user
                                if (userInfo != null)
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

                                    if (GetSafeString(Command, "login") == "logout")
                                    {
                                        // check to see if the user if already authenticated
                                        if (MembershipContext.AuthenticatedUser != null && MembershipContext.AuthenticatedUser.UserID == userInfo.UserID)
                                        {
                                            lblDebug.Text += "<br>" + "Logout mode:";
                                            lblDebug.Text += "<br>" + "logging off user..." + MembershipContext.AuthenticatedUser.UserName;
                                            AuthenticationHelper.SignOut(); // sign out the user
                                        }
                                    }
                                    else if (AuthenticationHelper.CanUserLogin(UserID))
                                    {
                                        lblDebug.Text += "<br>" + "User can login...";
                                        if (MembershipContext.AuthenticatedUser != null && MembershipContext.AuthenticatedUser.UserID != userInfo.UserID && MembershipContext.AuthenticatedUser.UserName != "public")
                                        {
                                            // the logged in user is not the same, we want to log out the previous user
                                            lblDebug.Text += "<br>" + "Different user logged in to session...";
                                            lblDebug.Text += "<br>" + "logging off user..." + MembershipContext.AuthenticatedUser.UserName;
                                            AuthenticationHelper.SignOut(); // sign out the user
                                        }

                                        // check to see if the user if already authenticated
                                        if (MembershipContext.AuthenticatedUser != null && MembershipContext.AuthenticatedUser.UserID == userInfo.UserID)
                                        {
                                            lblDebug.Text += "<br>" + "User is already Authenticated...";
                                            if (GetSafeString(LoginMode, "normal") == "r")
                                            {
                                                lblDebug.Text += "<br>" + "loging off user..." + MembershipContext.AuthenticatedUser.UserName;
                                                AuthenticationHelper.SignOut(); // sign out the user
                                            }
                                        }

                                        if (MembershipContext.AuthenticatedUser != null && MembershipContext.AuthenticatedUser.UserID != userInfo.UserID)
                                        {
                                            // now authenticate the user
                                            lblDebug.Text += "<br>" + "Logging in..." + CurrentUsername;
                                            AuthenticationHelper.AuthenticateUser(CurrentUsername, false);
                                            IsLoggedIn = true;
                                        }

                                        RedirectURL = AuthenticationHelper.GetUserAuthenticationUrl(userInfo, SSOkURLLibPath);
                                        lblDebug.Text += "<br>" + "url = " + RedirectURL;
                                    }
                                    else
                                    {
                                        // User is not allowed to login
                                        lblDebug.Text += "<br>" + "Requested User cannot login...";
                                    }
                                }
                            }
                            else
                            {
                                // failed role check
                            }
                        }
                        else
                        {
                            // failed, incompleted login info
                        }
                    }
                    else
                    {
                        // certificate or signature is invalid
                    }
                }
            }
            catch (ThreadAbortException tae)
            {
                KenticoLogEvent(tae, "Ignore this");
            }
            catch (Exception ex)
            {
                KenticoLogEvent(ex, "Unable to process SSO request");
            }
        }

        lblRedirectURL.Text = RedirectURL;

        ShowDebug();
        SendSSOLog();
        DoRedirect(RedirectURL);

        //if (!HasError && GetSafeString(Mode, "none") != "test" && !String.IsNullOrWhiteSpace(RedirectURL))
        //{
        //    URLHelper.Redirect(RedirectURL);
        //}
    }

    #endregion "Page events"

    #region "Page and Control Binding"

    /// <summary>
    /// Process the Request and save the data.
    /// </summary>
    /// <returns></returns>
    protected bool ProcessRequest(out string p_errorMessage)
    {
        bool canprocess = false;
        p_errorMessage = String.Empty;
        try
        {
            if (Request != null)
            {
                Command = Request.Params["c"];
                LoginMode = Request.Params["l"];
                Mode = Request.Params["m"];

                SSOmode = GetStringForDisplay(Command, "login") + "|" + GetStringForDisplay(LoginMode, "normal") + "|" + GetStringForDisplay(Mode, "none");

                if (Request.Form != null)
                {
                    SAMLResponse = Request.Form["SAMLResponse"];
                    RequestForm = Request.Form.ToString();
                }

                RawSAML = SAMLResponse;
                RequestURL = Request.Url.AbsoluteUri;

                if (Request.UrlReferrer != null)
                {
                    RequestReferrer = Request.UrlReferrer.AbsoluteUri;
                }

                if (Request.Headers != null)
                {
                    RequestHeader = Request.Headers.ToString();
                }

                RequestIP = Request.ServerVariables["REMOTE_ADDR"];

                RequestContentType = Request.ContentType;

                RequestContentLength = GetStringValueOrNull(Request.ContentLength);

                RequestUserAgent = Request.UserAgent;
                RequestUserHostAddress = Request.UserHostAddress;
                RequestUserHostName = Request.UserHostName;

                System.IO.Stream s = Request.InputStream;
                Encoding e = Request.ContentEncoding;
                System.IO.StreamReader r = new System.IO.StreamReader(s, e);
                RequestInputStream = r.ReadToEnd();
                r.Close();
                s.Close();

                if (!String.IsNullOrWhiteSpace(SAMLResponse))
                {
                    HasError = GetDecodedSAML(SAMLResponse, out DecodedSAML, out p_errorMessage);
                    canprocess = HasError;
                }
                else
                {
                    canprocess = false;
                    p_errorMessage += "NO SAML DETECTED IN REQUEST";
                }

                RawRequest = String.Format("URL: [\r\n{0}\r\n], Headers: [\r\n{1}\r\n], Form: [\r\n{2}\r\n], InputStream[\r\n{3}\r\n]", RequestURL, RequestHeader, RequestForm, RequestInputStream);
            }
        }
        catch (ThreadAbortException tae)
        {
            // catch and ignore it.
            KenticoLogEvent(tae, "Chemtrade SSO: Redirected before requests is completely processed");
        }
        catch (Exception ex)
        {
            canprocess = false;
            KenticoLogEvent(ex, "Unable to process SSO request");
        }

        return canprocess;
    }

    /// <summary>
    /// Show debug message
    /// </summary>
    protected void ShowDebug()
    {
        if (!String.IsNullOrWhiteSpace(ErrorMessage))
        {
            lblDebug.Text += "<p style=\"color:red;font-size:30px\">" + ErrorMessage + "</p>";
        }

        lblDebug.Text += GetDebugLine("IP", RequestIP, "N/A");
        lblDebug.Text += GetDebugLine("URL", RequestURL, "N/A");
        lblDebug.Text += GetDebugLine("REF URL", RequestReferrer, "N/A");
        lblDebug.Text += GetDebugLine("Request Headers", RequestHeader, "N/A");
        lblDebug.Text += GetDebugLine("Content Type", RequestContentType, "N/A");
        lblDebug.Text += GetDebugLine("Content Length", Request.ContentLength, "N/A");
        //lblDebug.Text += "<br><hr/><br>";
        //lblDebug.Text += GetDebugLine("Form Data", RequestForm, "N/A");
        txtFormData.Text += RequestForm;
        //lblDebug.Text += "<br><hr/><br>";
        //lblDebug.Text += GetDebugLine("Input Stream", RequestInputStream, "N/A");
        txtInputStream.Text = RequestInputStream;
        //lblDebug.Text += "<br><hr/><br>";
        //lblDebug.Text += GetDebugLine("SAMLResponse", SAMLResponse, "N/A");
        txtRAWSAML.Text = SAMLResponse;
        //lblDebug.Text += "<br><hr/><br>";
        //lblDebug.Text += GetDebugLine("Decode SAML", DecodedSAML, "N/A");
        TxtDecodeSAML.Text += DecodedSAML;
    }

    #endregion "Page and Control Binding"

    #region "X509 certificate validation"

    /// <summary>
    /// Check the ceritifcate
    /// </summary>
    /// <param name="samlXMLDoc"></param>
    /// <param name="message"></param>
    protected bool CheckAndLogCert(XmlDocument samlXMLDoc, out string message)
    {
        bool ret = false;
        message = String.Empty;

        // CHECK if doc is signed
        try
        {
            // validate against Chemetrade
            ret = VerifyXml(samlXMLDoc, GetX509Certificate2(X509CertificateChemTrade));

            if (!ret)
            {
                // check against WTE one login cert
                ret = VerifyXml(samlXMLDoc, GetX509Certificate2(X509CertificateOneLogin));
            }

            if (!ret)
            {
                // check again WTE O365 cert
                ret = VerifyXml(samlXMLDoc, GetX509Certificate2(X509CertificateOneO365));
            }

            if (!ret)
            {
                message = "X509 Certificate validation failed!";
            }
        }
        catch (Exception ex)
        {
            KenticoLogEvent(ex);
            message = ex.Message.ToString();
        }

        if (!ret && (GetSafeString(Mode, "none") == "test" || GetSafeString(Mode, "none") == "ooo"))
        {
            message += "(TEST MODE)";
            ret = true;
        }

        return ret;
    }

    /// <summary>
    /// Get X509 cert from a string
    /// </summary>
    /// <param name="X509Certificate"></param>
    /// <returns></returns>
    protected X509Certificate2 GetX509Certificate2(string X509Certificate)
    {
        return new X509Certificate2(Convert.FromBase64String(X509Certificate), _pfxPassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
    }

    /// <summary>
    /// Verify XML Signature
    /// </summary>
    /// <param name="Doc"></param>
    /// <param name="storeCert"></param>
    /// <returns></returns>
    protected static bool VerifyXml(XmlDocument Doc, X509Certificate2 storeCert)
    {
        if (Doc == null)
        {
            throw new ArgumentException("Doc");
        }

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

    #endregion "X509 certificate validation"

    #region "XML and SAML"

    /// <summary>
    /// Convert SAML data from base64 to string
    /// </summary>
    /// <param name="p_rawSAMLDATA"></param>
    /// <param name="p_DecodedSAML"></param>
    /// <param name="p_errorMessage"></param>
    /// <returns></returns>
    protected bool GetDecodedSAML(string p_rawSAMLDATA, out string p_DecodedSAML, out string p_errorMessage)
    {
        bool success = false;
        p_DecodedSAML = p_rawSAMLDATA;
        p_errorMessage = String.Empty;
        try
        {
            if (!String.IsNullOrWhiteSpace(p_rawSAMLDATA))
            {
                // Check if url encoded
                if (!String.IsNullOrWhiteSpace(p_rawSAMLDATA) && p_rawSAMLDATA.Contains('%'))
                {
                    p_rawSAMLDATA = HttpUtility.UrlDecode(p_rawSAMLDATA);
                }

                // check if true base64
                //bool isBase64 = false;

                try
                {
                    byte[] samlData = Convert.FromBase64String(p_rawSAMLDATA);
                    //return Encoding.UTF8.GetString(samlData);
                    p_DecodedSAML = Encoding.UTF8.GetString(samlData);
                }
                catch (Exception eb64)
                {
                    // if this fail, it means the string is not in Base64 (already decoded)
                    // KenticoLogEvent(eb64);
                }

                //p_DecodedSAML = p_rawSAMLDATA;
                success = true;
            }
            else
            {
                p_errorMessage = "Missing SAML DATA";
            }
        }
        catch (Exception ex)
        {
            // anything else:
            KenticoLogEvent(ex);
            p_errorMessage = "UNABLE TO DECODE SAML: " + ex.Message + " | " + ex.StackTrace;
        }
        return success;
    }

    /// <summary>
    /// load XML into Global xml Doc
    /// </summary>
    /// <param name="xml"></param>
    protected void LoadXml(string xml)
    {
        _xmlDoc = new XmlDocument();
        _xmlDoc.PreserveWhitespace = true;
        _xmlDoc.XmlResolver = null;
        _xmlDoc.LoadXml(xml);
        // _xmlNameSpaceManager = GetNamespaceManager(); //lets con struct a "manager" for XPath queries
    }

    /// <summary>
    /// Load XML from Base64 string?
    /// </summary>
    /// <param name="response"></param>
    protected void LoadXmlFromBase64(string response)
    {
        UTF8Encoding enc = new UTF8Encoding();
        LoadXml(enc.GetString(Convert.FromBase64String(response)));
    }

    #endregion "XML and SAML"

    #region "MD5 Hash Verification"

    /// <summary>
    /// Verify MD5 hash against a string.
    /// </summary>
    /// <param name="md5Hash"></param>
    /// <param name="input"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
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

    /// <summary>
    /// compute MD5 hash from the input string
    /// </summary>
    /// <param name="md5Hash"></param>
    /// <param name="input"></param>
    /// <returns></returns>
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

    #endregion "MD5 Hash Verification"

    #region "Data access"

    /// <summary>
    /// Check user roles.
    /// </summary>
    /// <returns></returns>
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
            ErrorMessage += ex.Message;
        }
        return Total;
    }

    /// <summary>
    /// Pull the Company ID from the data (if it exists)
    /// </summary>
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
            DoRedirect(SSOErrorURL + SSOLogID);
        }
    }

    /// <summary>
    /// Pull the user information from the data base using the username/email
    /// </summary>
    protected void CheckUserAndCompanyFromDB()
    {
        try
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ISNULL(US.FullName,'') As Fullname, US.UserID AS UserID, ISNULL(US.Username,'') As Username, ISNULL(UserCompany,0) AS CompanyID FROM CMS_User US LEFT OUTER JOIN CMS_UserSettings ST ON US.UserID = ST.UserSettingsUserID WHERE US.Email = '{0}'", EmailAddress), null, QueryTypeEnum.SQLQuery);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                CurrentCompanyID = GetSafeInt(row["CompanyID"]);
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
            ErrorMessage += ex.Message;
        }
    }

    /// <summary>
    /// Pull the user information from the database by the username/email using a stored procedure
    /// </summary>
    protected void CheckUserAndCompanyFromDB2()
    {
        try
        {
            QueryDataParameters prs = new QueryDataParameters();
            prs.Add(new DataParameter("UserName", EmailAddress));
            prs.Add(new DataParameter("EmailAddress", EmailAddress));
            prs.Add(new DataParameter("CompanyID", CompanyID));
            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.Proc_TN_SSO_GetSSOUser", prs, QueryTypeEnum.StoredProcedure);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                int.TryParse(GetSafeStringFromDataRow(row, "CompanyID"), out CurrentCompanyID);
                int.TryParse(GetSafeStringFromDataRow(row, "UserID"), out UserID);
                CurrentUsername = GetSafeStringFromDataRow(row, "Username");
                CurrentFullName = GetSafeStringFromDataRow(row, "Fullname");
            }
            else
            {
                CurrentCompanyID = 0;
            }
        }
        catch (Exception ex)
        {
            // won't be in the log
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            DoRedirect(SSOErrorURL + 0);
        }
    }

    /// <summary>
    /// Update the user with the new company
    /// </summary>
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
            DoRedirect(SSOErrorURL + SSOLogID);
        }
    }

    /// <summary>
    /// Update user full name.
    /// </summary>
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
            DoRedirect(SSOErrorURL + SSOLogID);
        }
    }

    /// <summary>
    /// Get null safe string
    /// </summary>
    /// <param name="p_row"></param>
    /// <returns></returns>
    protected string GetSafeStringFromDataRow(DataRow p_row, string p_colname)
    {
        string ret = String.Empty;
        try
        {
            if (p_row != null)
            {
                if (p_row[p_colname] != null)
                {
                    ret = p_row[p_colname].ToString();
                }
            }
        }
        catch (Exception ex)
        {
            KenticoLogEvent(ex);
            ErrorMessage += ex.Message;
        }
        return ret;
    }

    #endregion "Data access"

    #region "Logging and debugging"

    /// <summary>
    /// Call this one from anywhere
    /// </summary>
    /// <param name="message"></param>
    protected void LogThis(string message)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(LogFile))
            {
                CreateLogFile();
            }

            if (!String.IsNullOrWhiteSpace(LogFile))
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

    /// <summary>
    /// Create a blank log file
    /// </summary>
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
    /// Log SSO event to the database
    /// </summary>
    protected void SendSSOLog()
    {
        try
        {
            QueryDataParameters prs = new QueryDataParameters();
            prs.Add(new DataParameter("UserName", CurrentUsername));
            prs.Add(new DataParameter("EmailAddress", EmailAddress));
            prs.Add(new DataParameter("CurrentEmailAddress", CurrentEmail));
            prs.Add(new DataParameter("FullName", FullName));
            prs.Add(new DataParameter("CurrentFullName", CurrentFullName));
            prs.Add(new DataParameter("CompanyID", CompanyID));
            prs.Add(new DataParameter("CurrentCompanyID", CurrentCompanyID));
            prs.Add(new DataParameter("UserID", UserID));
            prs.Add(new DataParameter("IsLoggedIn", IsLoggedIn ? 1 : 0));
            prs.Add(new DataParameter("IsNewUser", IsNewUser ? 1 : 0));
            prs.Add(new DataParameter("LogMessage", SSOLogMessage));
            prs.Add(new DataParameter("ErrorMessage", ErrorMessage));
            prs.Add(new DataParameter("IPAddress", RequestIP));
            prs.Add(new DataParameter("RequestURL", RequestURL));
            prs.Add(new DataParameter("URLReferrer", RequestReferrer));
            prs.Add(new DataParameter("DecodedSAML", DecodedSAML));
            prs.Add(new DataParameter("RawSAML", RawSAML));
            prs.Add(new DataParameter("RawRequest", RawRequest));
            prs.Add(new DataParameter("RequestHeader", RequestHeader));
            prs.Add(new DataParameter("RequestUserAgent", RequestUserAgent));
            prs.Add(new DataParameter("SSOMode", SSOmode));
            prs.Add(new DataParameter("RedirectURL", RedirectURL));

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
            DoRedirect(SSOErrorURL + 0);
        }
    }

    /// <summary>
    /// Call Sproc to insert a log record in the database
    /// </summary>
    /// <param name="p_username"></param>
    /// <param name="p_emailAddress"></param>
    /// <param name="p_currentEmailAddress"></param>
    /// <param name="p_fullName"></param>
    /// <param name="p_currentFullName"></param>
    /// <param name="p_companyID"></param>
    /// <param name="p_currentCompanyID"></param>
    /// <param name="p_userID"></param>
    /// <param name="p_isLoggedIn"></param>
    /// <param name="p_isNewUser"></param>
    /// <param name="p_logMessage"></param>
    /// <param name="p_errorMessage"></param>
    /// <param name="p_iPAddress"></param>
    /// <param name="p_requestURL"></param>
    /// <param name="p_uRLReferrer"></param>
    /// <param name="p_DecodedSAML"></param>
    /// <param name="p_rawSAML"></param>
    /// <param name="p_rawRequest"></param>
    /// <param name="p_requestHeader"></param>
    /// <param name="p_requestUserAgent"></param>
    /// <param name="p_userHostAddress"></param>
    /// <param name="p_userHostName"></param>
    /// <param name="p_sSOMode"></param>
    /// <param name="p_redirectURL"></param>
    /// <returns></returns>
    protected bool TN_SSOLog_Insert(string p_username, string p_emailAddress, string p_currentEmailAddress, string p_fullName, string p_currentFullName, int? p_companyID, int? p_currentCompanyID, int? p_userID, bool? p_isLoggedIn, bool? p_isNewUser, string p_logMessage, string p_errorMessage, string p_iPAddress, string p_requestURL, string p_uRLReferrer, string p_DecodedSAML, string p_rawSAML, string p_rawRequest, string p_requestHeader, string p_requestUserAgent, string p_userHostAddress, string p_userHostName, string p_sSOMode, string p_redirectURL)
    {
        bool ret = false;

        try
        {
            QueryDataParameters prs = new QueryDataParameters();
            prs.Add(new DataParameter("UserName", p_username));
            prs.Add(new DataParameter("EmailAddress", p_emailAddress));
            prs.Add(new DataParameter("CurrentEmailAddress", p_currentEmailAddress));
            prs.Add(new DataParameter("FullName", p_fullName));
            prs.Add(new DataParameter("CurrentFullName", p_currentFullName));
            prs.Add(new DataParameter("CompanyID", p_companyID));
            prs.Add(new DataParameter("CurrentCompanyID", p_currentCompanyID));
            prs.Add(new DataParameter("UserID", p_userID));
            prs.Add(new DataParameter("IsLoggedIn", GetSafeBool(p_isLoggedIn) ? 1 : 0));
            prs.Add(new DataParameter("IsNewUser", GetSafeBool(p_isNewUser) ? 1 : 0));
            prs.Add(new DataParameter("LogMessage", p_logMessage));
            prs.Add(new DataParameter("ErrorMessage", p_errorMessage));
            prs.Add(new DataParameter("IPAddress", p_iPAddress));
            prs.Add(new DataParameter("RequestURL", p_requestURL));
            prs.Add(new DataParameter("URLReferrer", p_uRLReferrer));
            prs.Add(new DataParameter("DecodedSAML", p_DecodedSAML));
            prs.Add(new DataParameter("RawSAML", p_rawSAML));
            prs.Add(new DataParameter("RawRequest", p_rawRequest));
            prs.Add(new DataParameter("RequestHeader", p_requestHeader));
            prs.Add(new DataParameter("RequestUserAgent", p_requestUserAgent));
            prs.Add(new DataParameter("SSOMode", p_sSOMode));
            prs.Add(new DataParameter("RedirectURL", p_redirectURL));

            DataSet ds = ConnectionHelper.ExecuteQuery("dbo.Proc_TN_SSOLog_Insert", prs, QueryTypeEnum.StoredProcedure);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                Int64.TryParse(row["SSOLogID"].ToString(), out SSOLogID);
            }

            ret = true;
        }
        catch (Exception ex)
        {
            // won't be in the log
            KenticoLogEvent(ex);
            SSOLogMessage += ex.Message;
            DoRedirect(SSOErrorURL + 0);
        }

        return ret;
    }

    /// <summary>
    /// Get error message
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="p_note"></param>
    /// <returns></returns>
    protected string GetErrorMessage(Exception ex, string p_note = null)
    {
        string ret = String.Empty;
        if (ex != null)
        {
            if (!String.IsNullOrWhiteSpace(p_note))
            {
                ret += "Note: \r\n" + p_note + "\r\n";
            }
            ret += "Message: \r\n" + ex.Message + "\r\n";
            ret += "Stack: \r\n" + ex.StackTrace + "\r\n";
            if (ex.InnerException != null)
            {
                ret += "\r\n" + GetErrorMessage(ex.InnerException);
            }
        }
        return ret;
    }

    /// <summary>
    /// Log error as a kentico event
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="p_note"></param>
    protected void KenticoLogEvent(Exception ex, string p_note = null)
    {
        try
        {
            if (ex != null)
            {
                string errormessage = GetErrorMessage(ex, p_note);
                // how to log an event/error
                EventLogInfo log = new EventLogInfo();
                log.EventType = "E";
                log.Source = "SSO c# code error";
                log.EventCode = "EXCEPTION";
                log.EventDescription = errormessage;
                ErrorMessage += errormessage;
                EventLogProvider.SetEventLogInfo(log);

                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    HasError = true;
                }
            }
        }
        catch
        {
        }
    }

    #endregion "Logging and debugging"

    #region "Helpers"

    #region "url/redirect helper"

    /// <summary>
    /// Get the current query string
    /// </summary>
    /// <returns></returns>
    protected string GetQueryStrings()
    {
        string ret = String.Empty;
        foreach (String key in Request.QueryString.AllKeys)
        {
            //Response.Write("Key: " + key + " Value: " + Request.QueryString[key]);
            ret = ConcatQueryString(ret, key, Request.QueryString[key]);
        }
        return ret;
    }

    /// <summary>
    /// Add query string to url
    /// </summary>
    /// <param name="p_url"></param>
    /// <param name="p_queryString"></param>
    /// <returns></returns>
    protected string AddQueryString(string p_url, string p_queryString)
    {
        string ret = String.Empty;
        if (!String.IsNullOrWhiteSpace(p_queryString))
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                ret = p_url + "?" + p_queryString;
            }
        }
        return ret;
    }

    /// <summary>
    /// Concat 2 query string
    /// </summary>
    /// <param name="p_string"></param>
    /// <param name="p_key"></param>
    /// <param name="p_value"></param>
    /// <returns></returns>
    protected string ConcatQueryString(string p_string, string p_key, string p_value)
    {
        string ret = String.Empty;
        if (!String.IsNullOrWhiteSpace(p_string))
        {
            ret = p_string;
        }
        string qstring = MakeQueryString(p_key, p_value);
        if (!String.IsNullOrWhiteSpace(qstring))
        {
            if (!String.IsNullOrWhiteSpace(ret))
            {
                ret += "&";
            }
            ret += qstring;
        }
        return ret;
    }

    /// <summary>
    /// Make string value pair
    /// </summary>
    /// <param name="p_key"></param>
    /// <param name="p_value"></param>
    /// <returns></returns>
    protected string MakeQueryString(string p_key, string p_value)
    {
        string ret = String.Empty;
        if (!String.IsNullOrWhiteSpace(p_key) && !String.IsNullOrWhiteSpace(p_value))
        {
            ret = p_key + "=" + p_value;
        }
        return ret;
    }

    /// <summary>
    /// Get string value from the session
    /// </summary>
    /// <param name="p_key"></param>
    /// <returns></returns>
    protected string GetFromSession(string p_key)
    {
        Object o = SessionHelper.GetValue(p_key);
        string ret = "";
        if (o != null)
        {
            try
            {
                ret = Convert.ToString(o);
            }
            catch
            {
                //noop
            }
        }
        return ret;
    }

    /// <summary>
    /// Save a string value to session
    /// </summary>
    /// <param name="p_key"></param>
    /// <param name="p_value"></param>
    protected void SaveToSession(string p_key, string p_value)
    {
        SessionHelper.SetValue(p_key, p_value);
    }

    /// <summary>
    /// Perform URL redirection
    /// </summary>
    /// <param name="p_url"></param>
    /// <param name="p_useHelper"></param>
    protected void DoRedirect(string p_url, bool p_useHelper = true)
    {
        if (!String.IsNullOrWhiteSpace(p_url))
        {
            if (p_useHelper)
            {
                URLHelper.Redirect(p_url);
            }
            else
            {
                DoRedirect(p_url);
            }
        }
    }

    #endregion "url/redirect helper"

    #region "text helper"

    /// <summary>
    /// Get string for display
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    protected string GetStringForDisplay(object p_obj, string p_default)
    {
        string str = String.Empty;
        if (p_obj != null)
        {
            str = p_obj.ToString();
        }

        return !String.IsNullOrWhiteSpace(str) ? str : p_default;
    }

    /// <summary>
    /// Get debug line
    /// </summary>
    /// <param name="p_caption"></param>
    /// <param name="p_text"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected String GetDebugLine(string p_caption, object obj, string p_default, bool p_allowBlank = true)
    {
        string ret = string.Empty;
        if (p_allowBlank || !String.IsNullOrWhiteSpace(GetStringValueOrNull(obj)))
        {
            ret = String.Format("<br/><br/><b>{0}: </b> {1}", p_caption, GetStringForDisplay(obj, "N/A"));
        }
        return ret;
    }

    #endregion "text helper"

    #region "wte data helper"

    #region "get values"

    /// <summary>
    /// Safe cast an object to string
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static string GetString(object p_obj)
    {
        return GetString(p_obj, null);
    }

    /// <summary>
    ///  Get String value
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static string GetString(object p_obj, string p_default)
    {
        string ret = null;

        if (p_obj != null)
        {
            if (p_obj is Enum)
            {
                ret = ((int)(p_obj)).ToString();
            }
            else
            {
                ret = p_obj.ToString();
            }
        }

        // clean it up
        if (String.IsNullOrWhiteSpace(ret))
        {
            ret = p_default;
        }

        return ret;
    }

    /// <summary>
    /// Safe cast an object to nullable bool
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static bool? GetBool(object p_obj)
    {
        return GetBool(p_obj, null);
    }

    /// <summary>
    /// Get boolean value with default
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static bool? GetBool(object p_obj, bool? p_default)
    {
        bool? ret = null;
        string val = GetString(p_obj);
        if (!String.IsNullOrWhiteSpace(val))
        {
            // the setting could be bool (true/false) or (0/1)
            bool b = false;
            if (bool.TryParse(val, out b))
            {
                ret = b;
            }
            else
            {
                // failed, try parsing it as int
                int num = 0;
                if (int.TryParse(val, out num))
                {
                    ret = (num != 0);
                }
            }
        }
        if (ret == null)
        {
            ret = p_default;
        }
        return ret;
    }

    /// <summary>
    /// Safe cast an object to nullable int
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static int? GetInt(object p_obj)
    {
        return GetInt(p_obj, null);
    }

    /// <summary>
    /// Safe cast an object to nullable int
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static int? GetInt(object p_obj, int? p_default)
    {
        int? ret = null;
        string svalue = GetString(p_obj);
        if (!String.IsNullOrWhiteSpace(svalue))
        {
            int num = 0;
            if (int.TryParse(svalue, out num))
            {
                ret = num;
            }
        }

        if (ret == null)
        {
            ret = p_default;
        }

        return ret;
    }

    /// <summary>
    /// Get decimal
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static decimal? GetDecimal(object p_obj)
    {
        return GetDecimal(p_obj, null);
    }

    /// <summary>
    /// Get decimal
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static decimal? GetDecimal(object p_obj, decimal? p_default)
    {
        decimal? ret = null;
        string svalue = GetString(p_obj);
        if (!String.IsNullOrWhiteSpace(svalue))
        {
            decimal num = 0;
            if (decimal.TryParse(svalue, out num))
            {
                ret = num;
            }
        }

        if (ret == null)
        {
            ret = p_default;
        }

        return ret;
    }

    /// <summary>
    /// Get DateTime? value
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static DateTime? GetDateTime(object p_obj)
    {
        return GetDateTime(p_obj, null);
    }

    /// <summary>
    /// Get date time value
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static DateTime? GetDateTime(object p_obj, DateTime? p_default)
    {
        DateTime? ret = null;
        string val = GetString(p_obj);

        if (!String.IsNullOrWhiteSpace(val))
        {
            // default date is 01/01/1776

            // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
            // however DATETIME2 class does not have this limitation.

            DateTime d = DateTime.Now;
            if (DateTime.TryParse(val, out d))
            {
                ret = d;
            }
            else
            {
                ret = null;
            }
        }

        if (ret == null)
        {
            ret = p_default;
        }

        //return null if no date
        return ret;
    }

    #endregion "get values"

    #region "safe values"

    /// <summary>
    /// Convert object to string
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static string GetSafeString(object p_obj)
    {
        return GetSafeString(p_obj, String.Empty);
    }

    /// <summary>
    /// Convert object to string
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static string GetSafeString(object p_obj, string p_default)
    {
        string output = p_default;
        string value = GetString(p_obj, p_default);
        if (!String.IsNullOrWhiteSpace(value))
        {
            output = value.Trim();
        }
        return output;
    }

    /// <summary>
    /// Get string value or null
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static string GetStringValueOrNull(object p_obj)
    {
        string output = null;
        string value = GetString(p_obj);
        if (!String.IsNullOrWhiteSpace(value))
        {
            output = value;
        }
        return output;
    }

    /// <summary>
    /// Convert object to boolean
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static bool GetSafeBool(object p_obj)
    {
        return GetSafeBool(p_obj, false);
    }

    /// <summary>
    /// Get safe bool with default
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static bool GetSafeBool(object p_obj, bool p_default)
    {
        bool ret = p_default;
        bool? value = GetBool(p_obj, p_default);
        if (value.HasValue)
        {
            ret = value.Value;
        }
        return ret;
    }

    /// <summary>
    /// Get decimal or null (if value is false)
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static bool? GetBoolValueOrNull(object p_obj)
    {
        bool? ret = null;
        bool value = GetSafeBool(p_obj);
        if (value)
        {
            ret = value;
        }
        return ret;
    }

    /// <summary>
    /// Convert object to int
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static int GetSafeInt(object p_obj)
    {
        return GetSafeInt(p_obj, 0);
    }

    /// <summary>
    /// Get safe int with default
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static int GetSafeInt(object p_obj, int p_default)
    {
        int output = p_default;
        int? value = GetInt(p_obj, p_default);
        if (value.HasValue)
        {
            output = value.Value;
        }
        return output;
    }

    /// <summary>
    /// Get int or null (if value is 0)
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns>null if failed to parse or if value is 0</returns>
    protected static int? GetIntValueOrNull(object p_obj)
    {
        int? output = null;
        int value = GetSafeInt(p_obj);
        if (value > 0)
        {
            output = value;
        }
        return output;
    }

    /// <summary>
    /// Convert object to decimal
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static decimal GetSafeDecimal(object p_obj)
    {
        return GetSafeDecimal(p_obj, 0);
    }

    /// <summary>
    /// Get safe decimal with default
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static decimal GetSafeDecimal(object p_obj, decimal p_default)
    {
        decimal output = p_default;
        decimal? value = GetDecimal(p_obj, p_default);
        if (value.HasValue)
        {
            output = value.Value;
        }
        return output;
    }

    /// <summary>
    /// Get decimal or null (if value is 0)
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static decimal? GetDecimalValueOrNull(object p_obj)
    {
        decimal? output = null;
        decimal value = GetSafeDecimal(p_obj);
        if (value > 0)
        {
            output = value;
        }
        return output;
    }

    /// <summary>
    /// Convert object to date time
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static DateTime GetSafeDateTime(object p_obj)
    {
        return GetSafeDateTime(p_obj, DateTime.Now);
    }

    /// <summary>
    /// Get safe date time with default
    /// </summary>
    /// <param name="p_obj"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static DateTime GetSafeDateTime(object p_obj, DateTime p_default)
    {
        DateTime ret = p_default;
        DateTime? value = GetDateTimeValueOrNull(p_obj);
        if (value.HasValue)
        {
            ret = value.Value;
        }

        // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
        // however DATETIME2 class does not have this limitation.

        // clean up the value so that it is ms sql safe.
        if (ret.Year < 1900)
        {
            ret.AddYears(1900 - ret.Year);
        }

        return ret;
    }

    /// <summary>
    /// Get date time value or null!
    /// </summary>
    /// <param name="p_obj"></param>
    /// <returns></returns>
    protected static DateTime? GetDateTimeValueOrNull(object p_obj)
    {
        DateTime? ret = GetDateTime(p_obj, null);
        return ret;
    }

    #endregion "safe values"

    #region "math"

    /// <summary>
    /// Round a decimal
    /// </summary>
    /// <param name="p_number"></param>
    /// <param name="p_decimalPoints"></param>
    /// <returns></returns>
    protected static decimal Round(decimal p_number, int p_decimalPoints)
    {
        return Convert.ToDecimal(Round(Convert.ToDouble(p_number), p_decimalPoints));
    }

    /// <summary>
    /// Round a double
    /// </summary>
    /// <param name="p_number"></param>
    /// <param name="p_decimalPoints"></param>
    /// <returns></returns>
    protected static double Round(double p_number, int p_decimalPoints)
    {
        double decimalPowerOfTen = Math.Pow(10, p_decimalPoints);
        return Math.Floor(p_number * decimalPowerOfTen + 0.5) / decimalPowerOfTen;
    }

    #endregion "math"

    #region "utilities"

    /// <summary>
    /// Get bitwise int from an object
    /// </summary>
    /// <param name="p_valueSet"></param>
    /// <returns></returns>
    protected static int GetBitWiseInt(Object p_valueSet)
    {
        int val = 0;
        HashSet<int> valueSet = GetIntHashSet(p_valueSet);
        foreach (int v in valueSet)
        {
            val += v;
        }
        return val;
    }

    /// <summary>
    /// Get hashset from a comma delimited string
    /// </summary>
    /// <param name="p_valueSet"></param>
    /// <returns></returns>
    protected static HashSet<int> GetIntHashSet(Object p_valueSet)
    {
        HashSet<int> ret = new HashSet<int>();
        if (p_valueSet.GetType() == typeof(HashSet<int>))
        {
            ret = (HashSet<int>)p_valueSet;
        }
        else
        {
            List<string> ids = GetKeywordList(p_valueSet);
            foreach (string id in ids)
            {
                ret.Add(GetSafeInt(ids));
            }
        }
        return ret;
    }

    /// <summary>
    /// Get int list
    /// </summary>
    /// <param name="p_valueSet"></param>
    /// <returns></returns>
    protected static List<int> GetIntList(Object p_valueSet)
    {
        List<int> ret = new List<int>();

        if (p_valueSet != null)
        {
            if (p_valueSet.GetType() == typeof(List<int>))
            {
                ret = (List<int>)p_valueSet;
            }
            else
            {
                List<string> ids = GetKeywordList(p_valueSet);
                foreach (string id in ids)
                {
                    ret.Add(GetSafeInt(id));
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// Check ID to see if it's an int.
    /// </summary>
    /// <param name="p_keywordList"></param>
    /// <returns></returns>
    protected static bool ValidateIdString(Object p_keywordList)
    {
        bool valid = true;
        List<string> ids = GetKeywordList(p_keywordList);
        foreach (string id in ids)
        {
            int val = 0;
            if (!int.TryParse(id, out val))
            {
                valid = false;
                break;
            }
        }
        return valid;
    }

    /// <summary>
    /// Get display keyword string
    /// </summary>
    /// <param name="p_keywordList"></param>
    /// <returns></returns>
    protected static string GetDisplayKeywordString(Object p_keywordList)
    {
        return GetKeywordString(p_keywordList, ",", false, true);
    }

    /// <summary>
    /// Get keyword string
    /// </summary>
    /// <param name="p_keywordList"></param>
    /// <returns></returns>
    protected static string GetKeywordString(Object p_keywordList)
    {
        return GetKeywordString(p_keywordList, ",", false, false);
    }

    /// <summary>
    /// Get keyword string
    /// </summary>
    /// <param name="p_keywordList"></param>
    /// <param name="p_delimiter"></param>
    /// <param name="p_addQuotation"></param>
    /// <returns></returns>
    protected static string GetKeywordString(Object p_keywordList, string p_delimiter, bool p_addQuotation)
    {
        return GetKeywordString(p_keywordList, p_delimiter, p_addQuotation, false);
    }

    /// <summary>
    /// format keyword string to a proper string with delimiter
    /// </summary>
    /// <param name="p_keywordList"></param>
    /// <param name="p_delimiter"></param>
    /// <param name="p_addQuotation"></param>
    /// <param name="p_addSpace"></param>
    /// <returns></returns>
    protected static string GetKeywordString(Object p_keywordList, string p_delimiter, bool p_addQuotation, bool p_addSpace)
    {
        string ret = String.Empty;
        string delimiter = ",";
        if (p_keywordList != null)
        {
            if (!String.IsNullOrWhiteSpace(p_delimiter))
            {
                delimiter = p_delimiter.Trim();
            }

            List<string> keywords = GetKeywordList(p_keywordList);
            foreach (string keyword in keywords)
            {
                if (!String.IsNullOrWhiteSpace(keyword))
                {
                    if (!String.IsNullOrWhiteSpace(ret))
                    {
                        ret += delimiter;
                        if (p_addSpace)
                        {
                            ret += " ";
                        }
                    }
                    if (p_addQuotation)
                    {
                        ret += String.Format("'{0}'", keyword.Trim());
                    }
                    else
                    {
                        ret += keyword.Trim();
                    }
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// Create keyword list from 2 objects
    /// </summary>
    /// <param name="p_list1"></param>
    /// <param name="p_list2"></param>
    /// <returns></returns>
    protected static List<string> JoinKeyWordList(Object p_list1, Object p_list2)
    {
        return JoinKeyWordList(p_list1, p_list2, false);
    }

    /// <summary>
    /// Create keyword list from 2 objects
    /// </summary>
    /// <param name="p_list1"></param>
    /// <param name="p_list2"></param>
    /// <param name="p_filterZero"></param>
    /// <returns></returns>
    protected static List<string> JoinKeyWordList(Object p_list1, Object p_list2, bool p_filterZero)
    {
        List<string> ret = new List<string>();
        List<string> list1 = GetKeywordList(p_list1);
        List<string> list2 = GetKeywordList(p_list2);

        if (list1.Count > 0)
        {
            foreach (string item in list1)
            {
                if (!p_filterZero || item != "0")
                {
                    if (!ret.Contains(item))
                    {
                        ret.Add(item);
                    }
                }
            }
        }

        if (list2.Count > 0)
        {
            foreach (string item in list2)
            {
                if (!p_filterZero || item != "0")
                {
                    if (!ret.Contains(item))
                    {
                        ret.Add(item);
                    }
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// Get a list of string from a string value.
    /// </summary>
    /// <param name="p_keywordList"></param>
    /// <returns></returns>
    protected static List<string> GetKeywordList(Object p_keywordList)
    {
        List<string> keywordList = new List<string>();
        if (p_keywordList != null)
        {
            if (p_keywordList.GetType() == typeof(List<string>))
            {
                // return the keyword list
                keywordList = (List<string>)p_keywordList;
            }
            else if (p_keywordList.GetType() == typeof(HashSet<int>))
            {
                // add each item to the collection
                foreach (int keyword in (HashSet<int>)p_keywordList)
                {
                    string valueString = GetSafeString(keyword);
                    if (!keywordList.Contains(valueString))
                    {
                        keywordList.Add(valueString);
                    }
                }
            }
            else
            {
                #region any other type, convert to string parse and add each item to the list

                string tempValue = GetSafeString(p_keywordList);
                if (!String.IsNullOrWhiteSpace(tempValue))
                {
                    tempValue = tempValue.Replace("\\", "|").Replace("/", "|").Replace(" ", "|").Replace(",", "|").Trim();
                    string[] tempKeywords = tempValue.Split('|');
                    foreach (string keyword in tempKeywords)
                    {
                        if (!String.IsNullOrWhiteSpace(keyword))
                        {
                            if (!keywordList.Contains(keyword.Trim()))
                            {
                                keywordList.Add(keyword.Trim());
                            }
                        }
                    }
                }

                #endregion any other type, convert to string parse and add each item to the list
            }
        }
        return keywordList;
    }

    /// <summary>
    /// Check to see if the an object is nullable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p_object"></param>
    /// <returns></returns>
    protected static bool IsNullable<T>(T p_object)
    {
        if (!typeof(T).IsGenericType)
            return false;
        return typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Get value with default
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p_value"></param>
    /// <param name="p_default"></param>
    /// <returns></returns>
    protected static T GetDefaultValue<T>(T p_value, T p_default)
    {
        T ret = p_value;

        if (p_value == null)
        {
            // null just set to default
            ret = p_default;
        }
        else
        {
            Type t = p_value.GetType();
            if ((t == typeof(String)) || (t == typeof(string)))
            {
                if (String.IsNullOrWhiteSpace(p_value.ToString()))
                {
                    ret = p_default;
                }
                else
                {
                    ret = p_value;
                }
            }
            else
            {
                ret = p_value;
            }
        }

        return ret;
    }

    #endregion "utilities"

    #endregion "wte data helper"

    #endregion "Helpers"
}