using CMS.DataEngine;
using CMS.EventLog;
using CMS.Membership;
using CMS.SiteProvider;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;

public partial class sso_encrypt_old : System.Web.UI.Page
{
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
    private string SharedKey = "XJK531LHQE819KDQ"; // for testing it is should be same shared key. tak ea look at sso_encrypt_test.aspx.cs
    private string SSOErrorURL = "http://learn.streamery.co/system/SSOError?SSOLogID=";
    private string SSOOkURL = "http://learn.streamery.co/home.aspx";
    private string SSOLogMessage = "";
    private Int64 SSOLogID = 0;
    private string hash = "";

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

    protected bool VerifyHasInput(string CompanyID, string email, string hash)
    {
        string input = CompanyID + email + SharedKey; // email  + SharedKey
        MD5 md5Hash = MD5.Create();
        return VerifyMd5Hash(md5Hash, input, hash);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string strCompanyID = Request.QueryString["CompanyID"] == null ? "0" : Request.QueryString["CompanyID"].ToString();
        int.TryParse(strCompanyID, out CompanyID);
        // double check company
        CheckCompanyFromDB();

        FullName = Request.QueryString["name"] == null ? "" : Request.QueryString["name"].ToString();
        EmailAddress = Request.QueryString["email"] == null ? "" : Request.QueryString["email"].ToString();
        hash = Request.QueryString["hash"] == null ? "" : Request.QueryString["hash"].ToString();

        bool isOK = true;

        if (CompanyID <= 0)
        {
            SSOLogMessage += "CompanyID is 0 or not found.";
            isOK = false;
        }
        if (FullName.Length == 0)
        {
            SSOLogMessage += "Empty full name.";
            isOK = false;
        }
        if (EmailAddress.Length == 0)
        {
            SSOLogMessage += "Empty email address.";
            isOK = false;
        }

        if (hash.Length == 0)
        {
            SSOLogMessage += "Need hash value.";
            isOK = false;
        }
        else if (!VerifyHasInput(strCompanyID, EmailAddress, hash))
        {
            SSOLogMessage += "Invalid hash value.";
            isOK = false;
        }

        // if input is OK
        if (isOK)
        {
            // check current company from current EmailAddress
            CheckUserAndCompanyFromDB();

            int TotalOtherRoles = TotalRolesOtherThenDefault();
            if (TotalOtherRoles > 0)
            {
                SSOLogMessage += "This user has other roles, cannot use SSO.";
                isOK = false;
            }

            if (isOK)
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