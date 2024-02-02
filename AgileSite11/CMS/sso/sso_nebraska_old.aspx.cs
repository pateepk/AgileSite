using CMS.DataEngine;
using CMS.EventLog;
using CMS.IO;
using CMS.Membership;
using CMS.SiteProvider;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

public partial class sso_nebraska_old : System.Web.UI.Page
{
    private int[] Nebraska_CompanyID = new int[] { 1427, 1452, 2217, 2297 };

    private int UserID = 0;
    private int CompanyID = 0;
    private int CurrentCompanyID = 0;
    private bool IsLoggedIn = false;
    private string FullName = "";
    private string EmailAddress = "";
    private string CurrentUsername = "";
    private string CurrentFullName = "";
    private int GivenRoleID = 120; // RoleID given for this SSO
    private string OtherRolesCantUseSSO = "2,3,53,119,118,142,117";
    private string X509Certificate = "MIIEJjCCAw6gAwIBAgIUCJMtRn7NCy6pqXCN8AD5aMqFh0cwDQYJKoZIhvcNAQEFBQAwXTELMAkGA1UEBhMCVVMxFjAUBgNVBAoMDVdURSBTb2x1dGlvbnMxFTATBgNVBAsMDE9uZUxvZ2luIElkUDEfMB0GA1UEAwwWT25lTG9naW4gQWNjb3VudCA4NTM3NzAeFw0xNjA1MzAxMzA4MDFaFw0yMTA1MzExMzA4MDFaMF0xCzAJBgNVBAYTAlVTMRYwFAYDVQQKDA1XVEUgU29sdXRpb25zMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgODUzNzcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDA2LQCxsUbPz5iFIXKp+soz6DQOvnsrlwJ3VfOz8GeqHIPOOXHFTfP17gDth3gT5S2j7Bs+g5FCoWHG5XwpYxhJDi7Y2rCzZrMpo+4S4yD5mNrQReeYlYHi/R63FrxZwXrbxG8BOa9gMPspU4srO4xp3xt/0f6vDQcX/1STd0ga0yxLgp0q83a1JxFDkjQEBGf5Kfx1BcCNTn7Ry+2kwGfFXv0V0wwGBZScemnMe8s8H2/rl9sSI3dKnQSqkpu8CCWKs0uxHbrnYmqWFX9Zw03tfe3xEbx58El1tnA3xJYERBJXGYdH1q5lBL/k3uuL1/CpNYlxO98LLcptjmR/HDRAgMBAAGjgd0wgdowDAYDVR0TAQH/BAIwADAdBgNVHQ4EFgQUjsiH6C3m54gEO9AYspoN0Sn1pOMwgZoGA1UdIwSBkjCBj4AUjsiH6C3m54gEO9AYspoN0Sn1pOOhYaRfMF0xCzAJBgNVBAYTAlVTMRYwFAYDVQQKDA1XVEUgU29sdXRpb25zMRUwEwYDVQQLDAxPbmVMb2dpbiBJZFAxHzAdBgNVBAMMFk9uZUxvZ2luIEFjY291bnQgODUzNzeCFAiTLUZ+zQsuqalwjfAA+WjKhYdHMA4GA1UdDwEB/wQEAwIHgDANBgkqhkiG9w0BAQUFAAOCAQEAMg7kAzYWZe7NX9U48MuoewIoZtkUCD8vOomL+THTpDjhqRoUdlTlrPsxf36asmN8bVvMVetjEdVrJdENNP34wa8TCCER/gCGDU2i0pgewy0OujREG8m0RlWUSWb4JqAdjzPOULzWrfsO7OR3OUcHu8c6I1FBGlTFxULA7qsjKd/1rKjZn1tusw/HkVELzGFT4yNp5f6SZ9i2MksnXHtnAvPejJluYSIPmLNpTNs3OOwjEg2Z5y6uoEFlbzf/e5/eWMg/E5QCH+kjbeDmiosL5pL0HTUoJt07UrrG7xD/lTlwKOPQndH7ZtveLCuIypI9pQkqcC4F6BgTXjXX+UO5hQ==";
    private string SSOErrorURL = "http://learn.streamery.co/system/SSOError?SSOLogID=";
    private string SSOOkURL = "http://learn.streamery.co/home.aspx";
    private string SSOLogMessage = "";
    private Int64 SSOLogID = 0;
    private string hash = "";
    private string LogFile = "";

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

    protected void KenticoLogEvent(Exception ex)
    {
        try
        {
            // how to log an event/error
            EventLogInfo log = new EventLogInfo();
            log.EventType = "E";
            log.Source = "SSO";
            log.EventCode = "EXCEPTION";
            log.EventDescription = ex.Message;
            EventLogProvider.SetEventLogInfo(log);
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

    protected void Page_Load(object sender, EventArgs e)
    {
        string SAMLResponse = Request.Form["SAMLResponse"];
        if (!String.IsNullOrEmpty(SAMLResponse))
        {
            LogThis(SAMLResponse);
        }
        if (!String.IsNullOrEmpty(SAMLResponse))
        {
            SAMLResponse = SAMLResponse.Replace("\r", "").Replace("\n", "");
            byte[] b = Convert.FromBase64String(SAMLResponse);
            string strSAMLXML = System.Text.Encoding.UTF8.GetString(b);
            LogThis(strSAMLXML);
            XmlDocument samlXMLDoc = new XmlDocument();
            samlXMLDoc.LoadXml(strSAMLXML);

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
                                FullName = sPropertyValue;
                                break;

                            case "email":
                                EmailAddress = sPropertyValue;
                                if (EmailAddress.Trim().Length > 0 && EmailAddress.IndexOf('@') == -1)
                                {
                                    EmailAddress = EmailAddress.Trim() + "@csod.local";
                                }
                                break;

                            case "companyid":
                                int.TryParse(sPropertyValue, out CompanyID);
                                break;
                        }
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
            {
                CompanyID = 2297;
                // thi is default, if they want to login to different site, need to pass different CompanyID
            }

            if (FullName.Length > 0 && EmailAddress.Length > 0 && CompanyID > 0)
            {
                if (CompanyID > 0)
                {
                    if (Array.IndexOf(Nebraska_CompanyID, CompanyID) > -1)
                    {
                        if (EmailAddress.IndexOf('@') > -1)
                        {
                            //ViewLiteral.Text = FullName + " " + Email + " " + CompanyID;
                        }
                        else
                        {
                            SSOLogMessage = "Invalid email address.";
                        }
                    }
                    else
                    {
                        SSOLogMessage = "Company ID is not in the list for this SSO.";
                    }
                }
                else
                {
                    SSOLogMessage = "Invalid Company ID.";
                }
            }
            else
            {
                SSOLogMessage = "Please pass FullName, Email and CompanyID in the attribute node (saml2:Attribute).";
            }

            if (SSOLogMessage.Length == 0)
            {
                // check current company from current EmailAddress
                CheckUserAndCompanyFromDB();

                int TotalOtherRoles = TotalRolesOtherThenDefault();
                if (TotalOtherRoles > 0)
                {
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

                    // if exists and companyid same, log in
                    if (UserID > 0 && CurrentUsername.Length > 0)
                    {
                        // Now Login
                        AuthenticationHelper.AuthenticateUser(CurrentUsername, false);
                        IsLoggedIn = true;
                        SendSSOLog();
                        Response.Redirect(SSOOkURL);
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
                        SendSSOLog();
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
        }
    }
}