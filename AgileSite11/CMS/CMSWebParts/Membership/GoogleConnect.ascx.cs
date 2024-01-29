using CMS.Activities.Loggers;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.Protection;
using CMS.SiteProvider;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

public partial class CMSWebParts_Membership_GoogleConnect : CMSAbstractWebPart
{

    #region "Private fields"

    public string _url = "https://accounts.google.com/o/oauth2/token";

    public string error = string.Empty;

    string _buttonText = "Google Login";

    string _userRole = "CMSGoogleUsers";
    
    #endregion


    #region "Properties"

    /// <summary>
    /// 
    /// </summary>
    public string UserRole
    {
        get
        {
            if (!string.IsNullOrEmpty(ValidationHelper.GetString(this.GetValue("AssignRole"), string.Empty)))
            {
                return ValidationHelper.GetString(this.GetValue("AssignRole"), string.Empty);
            }
            return _userRole;
        }
        set
        {
            this.SetValue("AssignRole", value);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public string Url
    {
        get
        {
            if (!string.IsNullOrEmpty(ValidationHelper.GetString(this.GetValue("TokenUrl"), string.Empty)))
            {
                return ValidationHelper.GetString(this.GetValue("TokenUrl"), string.Empty);
            }
            return _url;
        }
        set
        {
            this.SetValue("TokenUrl", value);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public string ButtonText
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ButtonText"), "");
        }
        set
        {
            _buttonText = value;
            this.SetValue("ButtonText", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string ButtonCSS
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ButtonCSS"), "");
        }
        set
        {
            this.SetValue("ButtonCSS", value);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public bool LinkButton
    {
        get
        {
            return ValidationHelper.GetBoolean(this.GetValue("LinkButton"), false);
        }
        set
        {
            this.SetValue("LinkButton", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string GoogleClientID
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ClientId"), "");
        }
        set
        {
            this.SetValue("ClientId", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string ClientSecret
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ClientSecret"), "");
        }
        set
        {
            this.SetValue("ClientSecret", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string RedirectionUrl
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("RedirectionUrl"), "");
        }
        set
        {
            this.SetValue("RedirectionUrl", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string RedirectAfterLogin
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("RedirectAfterLogin"), "");
        }
        set
        {
            this.SetValue("RedirectAfterLogin", value);
        }
    }



    #endregion

    #region "Methods"

    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["code"] != null)
            {
                GetToken(Request.QueryString["code"].ToString());

            }
        }

        if (this.StopProcessing)
        {
            // Do not process
        }
        else
        {
            lnkGoogleBtn.Text = ButtonText;
            lnkGoogleBtn.CssClass = ButtonCSS;
            btnGoogle.CssClass = ButtonCSS;
            btnGoogle.Text = ButtonText;

            if (LinkButton)
            {
                lnkGoogleBtn.Visible = true;
                btnGoogle.Visible = false;
            }
            else
            {
                btnGoogle.Visible = true;
                lnkGoogleBtn.Visible = false;
            }
        }
    }


    /// <summary>
    /// Reloads the control data.
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();

        SetupControl();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="googleUserID"></param>
    /// <param name="siteName"></param>
    /// <param name="generatePassword"></param>
    /// <param name="disableConfirmation"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public UserInfo AuthenticateGoogleConnectUser(string googleUserID, string siteName, Userclass google, bool generatePassword, bool disableConfirmation, ref string error)
    {
        // Do not initialize context with current user -> this could lead to stack overflow
        using (new CMSActionContext { AllowInitUser = false })
        {


            // Check if parameters are set
            if (!String.IsNullOrEmpty(googleUserID))
            {
                // Try to find Facebook user ID in DB
                UserInfo ui = GetUserInfoByGoogleConnectID(googleUserID);
                if (ui == null)
                {
                    // User doesn't exist in DB = create a new one only if user with specified FacebookID name doesn't exist
                    if (UserInfoProvider.GetUserInfo("google_" + googleUserID) == null)
                    {
                        // Create user info
                        ui = new UserInfo();

                        // google user will have special prefix, unless they change it later
                        ui.UserName = "google_" + googleUserID;
                        ui.FullName = google.name;
						ui.Email = google.email;
                        ui.FirstName = google.given_name;
						ui.LastName = google.family_name;
                        ui.IsExternal = true;
                        ui.UserSettings.SetValue("UserGoogleID", googleUserID);

                        UserInfoProvider.SetUserInfo(ui);

                        // Generate random password for newly created Google user
                        if (generatePassword)
                        {
                            UserInfoProvider.SetPassword(ui.UserName, UserInfoProvider.GenerateNewPassword(siteName));
                            // Update/reload password to user info class
                            ui = UserInfoProvider.GetUserInfo(ui.UserID);
                        }

                        // Add current site
                        var rolesTable = ui.SitesRoles[siteName.ToLowerInvariant()];
                        if (rolesTable == null)
                        {
                            rolesTable = ui.CreateNewRolesDictionary();
                            ui.SitesRoles[siteName.ToLowerInvariant()] = rolesTable;
                        }

                        // Assign the default roles

                        string[] roles = UserRole.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string role in roles)
                        {
                            string roleName = role.Trim().ToLowerInvariant();
                            if (rolesTable[roleName] == null)
                            {
                                rolesTable[roleName] = 0;
                            }
                        }

                        // Ensure the user roles
                        UserInfoProvider.EnsureRolesAndSites(ui);

                        return AuthenticateMembershipUser(ui, true, siteName, disableConfirmation, ref error);
                    }
                }
                // User already exists in DB = authenticate user
                else
                {
                    // Ban IP addresses which are blocked for login
                    BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Login);

                    return AuthenticateMembershipUser(ui, false, siteName, disableConfirmation, ref error);
                }
            }
        }

        return null;
    }


    /// <summary>
    /// Returns user with specified Google Connect ID.
    /// </summary>
    /// <param name="googleUserId">Google Connect ID</param>
    public UserInfo GetUserInfoByGoogleConnectID(string googleUserId)
    {
        if (!string.IsNullOrEmpty(googleUserId))
        {
            return GetUsersDataWithSettings()
                .WhereEquals("UserGoogleID", googleUserId)
                .FirstObject;
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static ObjectQuery<UserInfo> GetUsersDataWithSettings()
    {
        return UserInfoProvider.GetUsers()
            .From("View_CMS_User");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="isNew"></param>
    /// <param name="siteName"></param>
    /// <param name="disableConfirmation"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private static UserInfo AuthenticateMembershipUser(UserInfo ui, bool isNew, string siteName, bool disableConfirmation, ref string error)
    {
        if (ui != null)
        {
            // Authenticate existing user
            if (!isNew)
            {
                // Do site check
                ui = UserInfoProvider.CheckUserBelongsToSite(ui, siteName);

                // Check whether user is kicked or disabled
                if ((ui != null) && (!AuthenticationHelper.CanUserLogin(ui.UserID) || !ui.Enabled))
                {
                    ui = null;
                    error = ResHelper.GetString("membership.userdisabled");
                }
            }
            // Authenticate new user
            else
            {
                bool requiresConfirmation = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationEmailConfirmation");
                requiresConfirmation &= !disableConfirmation;
                bool requiresAdminApprove = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationAdministratorApproval");

                // Check if user is enabled after creation
                if (requiresConfirmation || requiresAdminApprove)
                {
                    ui.Enabled = false;

                    // User is waiting for admin's approval
                    if (requiresAdminApprove && !requiresConfirmation)
                    {
                        ui.UserSettings.UserWaitingForApproval = true;
                        if (String.IsNullOrEmpty(error))
                        {
                            error = ResHelper.GetString("membership.usercreateddisabled");
                        }
                    }
                    // User is waiting for e-mail confirmation
                    else if (String.IsNullOrEmpty(error))
                    {
                        error = ResHelper.GetString("membership.requiresconfirmation");
                    }
                }
                else
                {
                    ui.Enabled = true;
                }

                UserInfoProvider.SetUserInfo(ui);


                // Check license limitation
                UserInfoProvider.CheckLicenseLimitation(ui, ref error);

                // Update registration information
                ui.UserSettings.UserRegistrationInfo.IPAddress = RequestContext.UserHostAddress;
                ui.UserSettings.UserRegistrationInfo.Agent = HttpContext.Current.Request.UserAgent;

            }
        }

        // Update the user record
        if (ui != null)
        {
            AuthenticationHelper.UpdateLastLogonInformation(ui);

            using (CMSActionContext context = new CMSActionContext())
            {
                // Disable logging of tasks
                context.DisableLogging();
                UserInfoProvider.SetUserInfo(ui);
            }

            if (ui.Enabled)
            {
                UserInfoProvider.SetPreferredCultures(ui);
            }
        }

        return ui;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    public void GetToken(string code)
    {
        string poststring = "grant_type=authorization_code&code=" + code + "&client_id=" + this.GoogleClientID + "&client_secret=" + this.ClientSecret + "&redirect_uri=" + this.RedirectionUrl + "";
        var request = (HttpWebRequest)WebRequest.Create(Url);
        request.ContentType = "application/x-www-form-urlencoded";
        request.Method = "POST";
        UTF8Encoding utfenc = new UTF8Encoding();
        byte[] bytes = utfenc.GetBytes(poststring);
        Stream outputstream = null;
        try
        {
            request.ContentLength = bytes.Length;
            outputstream = request.GetRequestStream();
            outputstream.Write(bytes, 0, bytes.Length);
        }
        catch
        { }
        var response = (HttpWebResponse)request.GetResponse();
        var streamReader = new StreamReader(response.GetResponseStream());
        string responseFromServer = streamReader.ReadToEnd();
        JavaScriptSerializer js = new JavaScriptSerializer();
        Tokenclass obj = js.Deserialize<Tokenclass>(responseFromServer);
        GetuserProfile(obj.access_token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accesstoken">Oauth aceesstoken of google after successfully authorization </param>
    public void GetuserProfile(string accesstoken)
    {
        string url = "https://www.googleapis.com/oauth2/v2/userinfo?alt=json&access_token=" + accesstoken + "";
        WebRequest request = WebRequest.Create(url);
        request.Credentials = CredentialCache.DefaultCredentials;
        WebResponse response = request.GetResponse();
        Stream dataStream = response.GetResponseStream();
        StreamReader reader = new StreamReader(dataStream);
        string responseFromServer = reader.ReadToEnd();
        reader.Close();
        response.Close();
        JavaScriptSerializer js = new JavaScriptSerializer();
        Userclass userinfo = js.Deserialize<Userclass>(responseFromServer);

        UserInfo ui = AuthenticateGoogleConnectUser(userinfo.id, CurrentSiteName, userinfo, false, true, ref error);
        if (ui != null)
        {
            SignInUser(ui, userinfo.id);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkGoogleBtn_Click(object sender, EventArgs e)
    {
        string url = "https://accounts.google.com/o/oauth2/v2/auth?scope=profile+email&include_granted_scopes=true&redirect_uri=" + this.RedirectionUrl + "&response_type=code&client_id=" + this.GoogleClientID + "";
        Response.Redirect(url);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnGoogle_Click(object sender, EventArgs e)
    {
        string url = "https://accounts.google.com/o/oauth2/v2/auth?scope=profile+email&include_granted_scopes=true&redirect_uri=" + this.RedirectionUrl + "&response_type=code&client_id=" + this.GoogleClientID + "";
        Response.Redirect(url);
    }


    /// <summary>
    /// Signs in given user.
    /// </summary>
    /// <param name="ui">User that will be signed in.</param>
    /// <param name="googleUserId">The user's Facebook ID</param>

    private void SignInUser(UserInfo ui, string googleUserId)
    {
        // Login existing user
        if (ui.Enabled)
        {

            // Ban IP addresses which are blocked for login
            BannedIPInfoProvider.CheckIPandRedirect(SiteContext.CurrentSiteName, BanControlEnum.Login);

            // Create authentication cookie
            AuthenticationHelper.SetAuthCookieWithUserData(ui.UserName, true, Session.Timeout, new[] { "googleLogan" });
            UserInfoProvider.SetPreferredCultures(ui);

            MembershipActivityLogger.LogLogin(ui.UserName, DocumentContext.CurrentDocument);

            // Redirect user
            string returnUrl = QueryHelper.GetString("returnurl", null);
            if (URLHelper.IsLocalUrl(returnUrl))
            {
                URLHelper.Redirect(returnUrl);
            }
            else
            {
                string currentUrl = URLHelper.RemoveParameterFromUrl(RequestContext.CurrentURL, "Code");
                currentUrl = URLHelper.RemoveParameterFromUrl(currentUrl, "Scope");
                if (!string.IsNullOrEmpty(RedirectAfterLogin))
                    currentUrl = RedirectAfterLogin;
                URLHelper.Redirect(ResolveUrl(currentUrl));
            }
        }

    }



    #endregion

}

/// <summary>
/// 
/// </summary>
public class Tokenclass
{
    /// <summary>
    /// google access token
    /// </summary>
    public string access_token { get; set; }

    /// <summary>
    /// type of access token (JWT)
    /// </summary>
    public string token_type { get; set; }

    /// <summary>
    /// token valid duration
    /// </summary>
    public int expires_in { get; set; }

    /// <summary>
    /// refresh token for extending the duration of access token
    /// </summary>
    public string refresh_token { get; set; }
}

/// <summary>
/// 
/// </summary>
public class Userclass
{
    /// <summary>
    /// Google user id 
    /// </summary>
    public string id { get; set; }
	
	/// <summary>
    /// Google email
    /// </summary>
    public string email { get; set; }

    /// <summary>
    /// Google name
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Google display name
    /// </summary>
    public string given_name { get; set; }

    /// <summary>
    /// Google user last name
    /// </summary>
    public string family_name { get; set; }

    /// <summary>
    /// Google profile url
    /// </summary>
    public string link { get; set; }

    /// <summary>
    /// Google user avatar url
    /// </summary>
    public string picture { get; set; }

    /// <summary>
    /// User gender 
    /// </summary>
    public string gender { get; set; }

    /// <summary>
    /// Country language code
    /// </summary>
    public string locale { get; set; }
}