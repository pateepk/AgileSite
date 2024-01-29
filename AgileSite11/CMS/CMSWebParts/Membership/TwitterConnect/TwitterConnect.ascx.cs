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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

public partial class CMSWebParts_Membership_TwitterConnect : CMSAbstractWebPart
{

    #region "Variable Declaration"

    JavaScriptSerializer serializer = new JavaScriptSerializer();

    string REQUEST_TOKEN = "https://api.twitter.com/oauth/request_token";
    string USERACCOUNT = "https://api.twitter.com/1.1/account/verify_credentials.json";
    string ACCESS_TOKEN = "https://api.twitter.com/oauth/access_token";


    string url = string.Empty;
    string xml = string.Empty;
    public string error = string.Empty;
    string _userRole = "CMSTwitterUser";
    string _buttonText = "Twitter Login";


    #endregion

    #region "Properties"

    /// <summary>
    /// Redirect page url after login
    /// </summary>
    public string RedirectAfterLogin
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("RedirectAfterLogin"), "").Trim();
        }
        set
        {
            this.SetValue("RedirectAfterLogin", value);
        }
    }


    /// <summary>
    /// assign default or configure role after twitter oAuth vaild authentication  
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
    /// twitter client key for oauth login
    /// </summary>
    public string TwitterConsumerKey
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ConsumerKey"), string.Empty).Trim();
        }
        set
        {
            this.SetValue("ConsumerKey", value);
        }
    }

    /// <summary>
    /// twitter client key secret for oauth login
    /// </summary>
    public string TwitterConsumerSecret
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ConsumerSecret"), string.Empty).Trim();
        }
        set
        {
            this.SetValue("ConsumerSecret", value);
        }
    }


    /// <summary>
    /// Redirect url from oAuth portal 
    /// </summary>
    public string TwitterCallbackURL
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("CallbackURL"), string.Empty);
        }
        set
        {
            this.SetValue("CallbackURL", value);
        }
    }

    /// <summary>
    /// button text of the link or button type
    /// </summary>
    public string ButtonText
    {
        get
        {
            if (!string.IsNullOrEmpty(ValidationHelper.GetString(this.GetValue("ButtonText"), string.Empty)))
            {
                return ValidationHelper.GetString(this.GetValue("ButtonText"), _buttonText);
            }
            return _buttonText;
        }
        set
        {
            _buttonText = value;
            this.SetValue("ButtonText", value);
        }
    }

    /// <summary>
    /// CSS class for the button 
    /// </summary>
    public string ButtonCSS
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ButtonCSS"), string.Empty);
        }
        set
        {
            this.SetValue("ButtonCSS", value);
        }
    }


    /// <summary>
    /// if button type is link
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
            if (Request.QueryString["oauth_token"] != null && Request.QueryString["oauth_verifier"] != null)
            {
                TwitterUser(ValidationHelper.GetString(Request.QueryString["oauth_verifier"], string.Empty), ValidationHelper.GetString(Request.QueryString["oauth_token"], string.Empty));

            }
        }

        if (this.StopProcessing)
        {
            // Do not process
        }
        else
        {
            btnTwitterLogin.Text = ButtonText;
            btnTwitterLogin.CssClass = ButtonCSS;
            lnkTwitterLogin.CssClass = ButtonCSS;
            lnkTwitterLogin.Text = ButtonText;

            if (LinkButton)
            {
                lnkTwitterLogin.Visible = true;
                btnTwitterLogin.Visible = false;
            }
            else
            {
                btnTwitterLogin.Visible = true;
                lnkTwitterLogin.Visible = false;
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
    /// twitter login button click event
    /// </summary>
    protected void btnTwitterLogin_Click(object sender, EventArgs e)
    {
        TwitterRequest();
    }



    /// <summary>
    /// Returns user with specified Twitter Connect ID.
    /// </summary>
    /// <param name="UserTwitterID">twitter Connect ID</param>
    public UserInfo GetUserInfoByTwitterConnectID(string twitterUserId)
    {
        if (!string.IsNullOrEmpty(twitterUserId))
        {
            return GetUsersDataWithSettings()
                .WhereEquals("UserTwitterID", twitterUserId)
                .FirstObject;
        }

        return null;
    }
    public static ObjectQuery<UserInfo> GetUsersDataWithSettings()
    {
        return UserInfoProvider.GetUsers()
            .From("View_CMS_User");
    }
    #endregion

    #region "Private Method"


    /// <summary>
    /// 
    /// </summary>
    /// <param name="twitterID"></param>
    /// <param name="siteName"></param>
    /// <param name="generatePassword"></param>
    /// <param name="disableConfirmation"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private UserInfo AuthenticateTwitterConnectUser(string twitterID, string siteName, TwitterUserclass twitter, bool generatePassword, bool disableConfirmation, ref string error)
    {
        // Do not initialize context with current user -> this could lead to stack overflow
        using (new CMSActionContext { AllowInitUser = false })
        {


            // Check if parameters are set
            if (!String.IsNullOrEmpty(twitterID))
            {
                // Try to find Facebook user ID in DB
                UserInfo ui = GetUserInfoByTwitterConnectID(twitterID);
                if (ui == null)
                {
                    // User doesn't exist in DB = create a new one only if user with specified FacebookID name doesn't exist
                    if (UserInfoProvider.GetUserInfo("twitter_" + twitterID) == null)
                    {
                        // Create user info
                        ui = new UserInfo();

                        // Facebook user will have special prefix, unless they change it later
                        ui.UserName = "twitter_" + Regex.Replace(twitter.name, @"\s+", "");
                        ui.FullName = twitter.name;
                        ui.Email = twitter.email;
                        ui.FirstName = twitter.name;
                        ui.IsExternal = true;
                        ui.UserSettings.SetValue("UserTwitterID", twitterID);

                        UserInfoProvider.SetUserInfo(ui);

                        // Generate random password for newly created Facebook user
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
    /// 
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="isNew"></param>
    /// <param name="siteName"></param>
    /// <param name="disableConfirmation"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private UserInfo AuthenticateMembershipUser(UserInfo ui, bool isNew, string siteName, bool disableConfirmation, ref string error)
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
    /// Signs in given user.
    /// </summary>
    /// <param name="ui">User that will be signed in.</param>
    /// <param name="twitterUserId">The user's Twitter ID</param>
    private void SignInUser(UserInfo ui, string twitterID)
    {
        // Login existing user
        if (ui.Enabled)
        {

            // Ban IP addresses which are blocked for login
            BannedIPInfoProvider.CheckIPandRedirect(SiteContext.CurrentSiteName, BanControlEnum.Login);

            // Create authentication cookie
            AuthenticationHelper.SetAuthCookieWithUserData(ui.UserName, true, Session.Timeout, new[] { "twitterLogin" });
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
                currentUrl = URLHelper.RemoveParameterFromUrl(currentUrl, "oauth_token");
                currentUrl = URLHelper.RemoveParameterFromUrl(currentUrl, "oauth_verifier");
                if (!string.IsNullOrEmpty(RedirectAfterLogin))
                    currentUrl = RedirectAfterLogin;
                URLHelper.Redirect(ResolveUrl(currentUrl));
            }
        }
        else
        {
            // User is disabled

        }
    }

    #endregion

    #region "External(Twitter) Method"

    /// <summary>
    /// Request for twitter for authentication
    /// </summary>
    public void TwitterRequest()
    {

        var objText = string.Empty;


        var oauth_consumer_key = TwitterConsumerKey;// = "insert here...";
        var oauth_consumer_secret = TwitterConsumerSecret;// = "insert here...";

        // oauth implementation details
        var oauth_version = "1.0";
        var oauth_signature_method = "HMAC-SHA1";

        // unique request details
        var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        var timeSpan = DateTime.UtcNow
            - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

        var redirectURL = TwitterCallbackURL;
        // create oauth signature
        var baseFormat = "oauth_callback={5}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                     "&oauth_timestamp={3}&oauth_version={4}";


        var baseString = string.Format(baseFormat,
                                   oauth_consumer_key,
                                   oauth_nonce,
                                   oauth_signature_method,
                                   oauth_timestamp,
                                   oauth_version,
                                   Uri.EscapeDataString(redirectURL)

                                   );

        baseString = string.Concat("POST&", Uri.EscapeDataString(REQUEST_TOKEN), "&", Uri.EscapeDataString(baseString));

        var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                "&");

        string oauth_signature;
        using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
        {
            oauth_signature = Convert.ToBase64String(
                hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
        }

        // create the request header
        var headerFormat = "OAuth oauth_callback=\"{6}\", oauth_consumer_key=\"{3}\", oauth_nonce=\"{0}\"," +
                       "oauth_signature=\"{4}\", oauth_signature_method=\"{1}\",  " +
                       "oauth_timestamp=\"{2}\", " + "oauth_version=\"{5}\"";

        var authHeader = string.Format(headerFormat,
                                Uri.EscapeDataString(oauth_nonce),
                                Uri.EscapeDataString(oauth_signature_method),
                                Uri.EscapeDataString(oauth_timestamp),
                                Uri.EscapeDataString(oauth_consumer_key),
                                Uri.EscapeDataString(oauth_signature),
                                Uri.EscapeDataString(oauth_version),
                                Uri.EscapeDataString(redirectURL)
                        );


        ServicePointManager.Expect100Continue = false;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(REQUEST_TOKEN);
        request.Headers.Add("Authorization", authHeader);
        request.Method = "POST";
        request.ContentType = "application/json; charset=utf-8";
        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());
        var value = reader.ReadToEnd().ToString();
        var valueJson = "{" + value.Replace("&", "',").Replace("=", ":'") + "'}";


        var routes_list = (IDictionary<string, object>)serializer.DeserializeObject(valueJson);

        if (Convert.ToBoolean(routes_list["oauth_callback_confirmed"]))
        {
            Response.Redirect("https://api.twitter.com/oauth/authenticate?oauth_token=" + routes_list["oauth_token"]);
        }

    }



    /// <summary>
    /// GEt Acess Token from Request Token
    /// </summary>
    /// <param name="oauth_verifier"></param>
    /// <param name="oauth_token"></param>
    /// <returns></returns>
    public void TwitterUser(string oauth_verifier, string oauth_token)
    {


        var objText = string.Empty;


        var oauth_consumer_key = TwitterConsumerKey;// = "insert here...";
        var oauth_consumer_secret = TwitterConsumerSecret;// = "insert here...";

        // oauth implementation details
        var oauth_version = "1.0";
        var oauth_signature_method = "HMAC-SHA1";

        // unique request details
        var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        var timeSpan = DateTime.UtcNow
            - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();


        // create oauth signature
        var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                     "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";


        var baseString = string.Format(baseFormat,
                                   oauth_consumer_key,
                                   oauth_nonce,
                                   oauth_signature_method,
                                   oauth_timestamp,
                                   oauth_token,
                                   oauth_version


                                   );

        baseString = string.Concat("POST&", Uri.EscapeDataString(ACCESS_TOKEN), "&", Uri.EscapeDataString(baseString));

        var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                "&");

        string oauth_signature;
        using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
        {
            oauth_signature = Convert.ToBase64String(
                hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
        }

        // create the request header
        var headerFormat = "OAuth oauth_consumer_key=\"{3}\", oauth_nonce=\"{0}\"," +
                       "oauth_signature=\"{4}\", oauth_signature_method=\"{1}\"," +
                       "oauth_timestamp=\"{2}\", oauth_token=\"{6}\"," +
                       "oauth_version=\"{5}\"";

        var authHeader = string.Format(headerFormat,
                                Uri.EscapeDataString(oauth_nonce),
                                Uri.EscapeDataString(oauth_signature_method),
                                Uri.EscapeDataString(oauth_timestamp),
                                Uri.EscapeDataString(oauth_consumer_key),
                                Uri.EscapeDataString(oauth_signature),
                                Uri.EscapeDataString(oauth_version),
                                Uri.EscapeDataString(oauth_token)

                        );
        var postData = "oauth_verifier=" + oauth_verifier;

        var data = Encoding.ASCII.GetBytes(postData);

        ServicePointManager.Expect100Continue = false;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ACCESS_TOKEN);
        request.Headers.Add("Authorization", authHeader);
        request.Method = "POST";

        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = data.Length;
        using (var stream = request.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
        }
        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());
        var value = reader.ReadToEnd().ToString();

        var valueJson = "{" + value.Replace("&", "',").Replace("=", ":'") + "'}";

        var routes_list = (IDictionary<string, object>)serializer.DeserializeObject(valueJson);

        var twUser = GetUserDetails(ValidationHelper.GetString(routes_list["oauth_token"], string.Empty), ValidationHelper.GetString(routes_list["oauth_token_secret"], string.Empty));
        UserInfo ui = AuthenticateTwitterConnectUser(twUser.id, CurrentSiteName, twUser, false, true, ref error);
        if (ui != null)
        {
            SignInUser(ui, twUser.id);
        }

    }

    /// <summary>
    /// Use Acess Token To get Client Details
    /// </summary>
    /// <param name="access_token"></param>
    /// <param name="token_seret"></param>
    /// <returns></returns>
    public TwitterUserclass GetUserDetails(string access_token, string token_seret)
    {

        var include_emails = "true"; // Will be used to Get Emails when APP is live.


        var objText = string.Empty;

        // oauth application keys
        var oauth_token = access_token; //"insert here...";
        var oauth_token_secret = token_seret; //"insert here...";
        var oauth_consumer_key = TwitterConsumerKey;// = "insert here...";
        var oauth_consumer_secret = TwitterConsumerSecret;// = "insert here...";

        // oauth implementation details
        var oauth_version = "1.0";
        var oauth_signature_method = "HMAC-SHA1";

        // unique request details
        var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        var timeSpan = DateTime.UtcNow
            - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

        /*
         IMPORTANT NOTE : The request is not getting any email rght now. To get an Email ID a Parameter "include_emails " must be sent with the request. 
         The Parameter should also be inculded in the signature base string at the beginning and as a Query Parameter to the resource URL.
        */


        // create oauth signature
        var baseFormat = "include_email={6}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                        "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";


        var baseString = string.Format(baseFormat,
                                   oauth_consumer_key,
                                   oauth_nonce,
                                   oauth_signature_method,
                                   oauth_timestamp,
                                   oauth_token,
                                   oauth_version,
                                   include_emails

                                   );

        baseString = string.Concat("GET&", Uri.EscapeDataString(USERACCOUNT), "&", Uri.EscapeDataString(baseString));

        var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                "&", Uri.EscapeDataString(oauth_token_secret));

        string oauth_signature;
        using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
        {
            oauth_signature = Convert.ToBase64String(
                hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
        }

        // create the request header
        var headerFormat = "OAuth oauth_consumer_key=\"{3}\", oauth_nonce=\"{0}\"," +
                       "oauth_signature=\"{4}\", oauth_signature_method=\"{1}\"," +
                       "oauth_timestamp=\"{2}\", oauth_token=\"{6}\"," +
                       "oauth_version=\"{5}\"";

        var authHeader = string.Format(headerFormat,
                                Uri.EscapeDataString(oauth_nonce),
                                Uri.EscapeDataString(oauth_signature_method),
                                Uri.EscapeDataString(oauth_timestamp),
                                Uri.EscapeDataString(oauth_consumer_key),
                                Uri.EscapeDataString(oauth_signature),
                                Uri.EscapeDataString(oauth_version),
                                Uri.EscapeDataString(oauth_token)

                        );



        ServicePointManager.Expect100Continue = false;



        var postBody = "include_email=" + include_emails;
        USERACCOUNT += "?" + postBody;

        ServicePointManager.Expect100Continue = false;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(USERACCOUNT);
        request.Headers.Add("Authorization", authHeader);
        request.Method = "GET";

        request.ContentType = "application/x-www-form-urlencoded";

        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());
        var value = reader.ReadToEnd().ToString();

        JavaScriptSerializer sr = new JavaScriptSerializer();
        TwitterUserclass twUser = sr.Deserialize<TwitterUserclass>(value);

        return twUser;

    }

    #endregion

}

/// <summary>
/// 
/// </summary>
public class TwitterUserclass
{
    /// <summary>
    /// 
    /// </summary>
    public string profile_image_url { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string screen_name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string statuses_count { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string followers_count { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string friends_count { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string favourites_count { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string location { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string given_name { get; set; }

    /// <summary>
    /// /
    /// </summary>
    public string email { get; set; }

}