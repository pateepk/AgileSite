<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DevCode1.aspx.cs" Inherits="DevCode1" %>

<%@ Import Namespace="CMS.Base" %>
<%@ Import Namespace="CMS.Membership" %>
<%@ Import Namespace="CMS.SiteProvider" %>
<%@ Import Namespace="CMS.EventLog" %>
<%@ Import Namespace="CMS.DataEngine" %>
<%@ Import Namespace="System.Data" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <a href="?Action=Logout">Logout</a><br />
            <a href="?Action=DevLogin">Chacha Login</a><br />
            <a href="?Action=UserLogin">User Login</a><br />
            <a href="?Action=CheckUser">Check User</a><br />
            <a href="?Action=CreateUser">Create User</a><br />
            <a href="?Action=LogEvent">Log Event</a><br />
            <a href="?Action=CheckCompany">Check Company</a><br />
            <a href="?Action=LogSSO">Log SSO</a><br />
            <br />
            <%
                string Action = Request.QueryString["Action"] == null ? "" : Request.QueryString["Action"].ToString();

                if (Action.Length > 0)
                {
                    switch (Action)
                    {
                        case "Logout":
                            // this is how to logout
                            AuthenticationHelper.SignOut();
                            Response.Redirect(Request.Path);
                            break;
                        case "DevLogin":
                            // this is how to force login to certain username
                            AuthenticationHelper.AuthenticateUser("chacha", false);
                            Response.Redirect(Request.Path);
                            break;
                        case "UserLogin":
                            // does not work
                            AuthenticationHelper.AuthenticateUser("chacha", "xxxx", SiteContext.CurrentSiteName, true);
                            Response.Redirect(Request.Path);
                            break;
                        case "CheckCompany":
                            // how to check company
                            int CompanyID = 1525;
                            // company ID has to be interger please
                            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ItemID, PartnerName, CustomerName FROM dbo.customtable_Customers WHERE ItemID = {0}", CompanyID), null, QueryTypeEnum.SQLQuery);
                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow row = ds.Tables[0].Rows[0];
                                Response.Write(string.Format("Company ID = {0} </br>", row["ItemID"]));
                                Response.Write(string.Format("PartnerName = {0} </br>", row["PartnerName"]));
                                Response.Write(string.Format("CustomerName = {0} </br>", row["CustomerName"]));
                            }
                            else
                            {
                                Response.Write("Company not found!");
                            }
                            break;
                        case "CheckUser":
                            // get a user from database
                            UserInfo myuser = UserInfoProvider.GetUserInfo(101);

                            Response.Write(string.Format("A UserID = {0} </br>", myuser.UserID));
                            Response.Write(string.Format("A FullName = {0} </br>", myuser.FullName));
                            Response.Write(string.Format("Email = {0} </br>", myuser.Email));
                            Response.Write("</br>");

                            break;
                        case "LogEvent":
                            // how to log an event/error
                            EventLogInfo log = new EventLogInfo();
                            log.EventType = "E";
                            log.Source = "SSO";
                            log.EventCode = "EXCEPTION";
                            log.EventDescription = "Blah blah blah";
                            EventLogProvider.SetEventLogInfo(log);
                            break;
                        case "LogSSO":
                            QueryDataParameters prs = new QueryDataParameters();
                            prs.Add(new DataParameter("EmailAddress", "chacha@dejava.com"));
                            prs.Add(new DataParameter("FullName", "Chacha"));
                            prs.Add(new DataParameter("CompanyID", 1525));
                            prs.Add(new DataParameter("CurrentCompanyID", 1));
                            prs.Add(new DataParameter("UserID", 100));
                            prs.Add(new DataParameter("IPAddress", Request.ServerVariables["REMOTE_ADDR"]));
                            prs.Add(new DataParameter("IsLoggedIn", true ? 1 : 0));
                            prs.Add(new DataParameter("LogMessage", "Okaie"));
                            DataSet dset = ConnectionHelper.ExecuteQuery("dbo.Proc_TN_SSOLog_Insert", prs, QueryTypeEnum.StoredProcedure);
                            break;
                        case "CreateUser":
                            // how to create a user
                            Random random = new Random();
                            int rdn = random.Next(10000);
                            int pass = random.Next(10000);
                            UserInfo newUser = new UserInfo();
                            newUser.UserName = "TESTING_" + rdn;
                            newUser.FullName = rdn + " TESTING";
                            newUser.Enabled = true;
                            newUser.Email = rdn.ToString() + "@mailinator.com";
                            UserInfoProvider.SetUserInfo(newUser);
                            UserInfoProvider.SetPassword(newUser, "PASS_" + pass);

                            // set a new role
                            UserRoleInfo roles = new UserRoleInfo();
                            roles.UserID = newUser.UserID;
                            roles.RoleID = 121;
                            UserRoleInfoProvider.SetUserRoleInfo(roles);

                            // set a company
                            UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(newUser.UserID);
                            userSettings.SetValue("UserCompany", 1525); // change later
                            UserSettingsInfoProvider.SetUserSettingsInfo(userSettings);

                            UserInfoProvider.AddUserToSite(newUser.UserName, SiteContext.CurrentSiteName);

                            // log in
                            AuthenticationHelper.AuthenticateUser(newUser.UserName, false);
                            break;
                        default:
                            break;
                    }

                }

                // this is how you get current (logged) user information
                Response.Write(string.Format("Current UserID = {0} </br>", MembershipContext.AuthenticatedUser.UserID));
                Response.Write(string.Format("FullName = {0} </br>", MembershipContext.AuthenticatedUser.FullName));
                Response.Write(string.Format("First Name = {0} </br>", MembershipContext.AuthenticatedUser.FirstName));
                Response.Write(string.Format("Last Name = {0} </br>", MembershipContext.AuthenticatedUser.LastName));
                Response.Write(string.Format("Last Logon = {0} </br>", MembershipContext.AuthenticatedUser.LastLogon));
                Response.Write(string.Format("GUID = {0} </br>", MembershipContext.AuthenticatedUser.UserGUID));
                Response.Write(string.Format("Username of UserID = {0} is {1} </br>", 101, UserInfoProvider.GetUserNameById(101)));

                Response.Write("</br>");

                // this is current website
                Response.Write(string.Format("Current SiteName = {0} </br>", SiteContext.CurrentSiteName));
                Response.Write(string.Format("Current Site Domain = {0} </br>", SiteContext.CurrentSite.DomainName));
                Response.Write(string.Format("Current Site Domain = {0} </br>", SiteContext.CurrentSite.SiteName));
                Response.Write(string.Format("Current SiteID = {0} </br>", SiteContext.CurrentSiteID));
            %>
        </div>
    </form>
</body>
</html>