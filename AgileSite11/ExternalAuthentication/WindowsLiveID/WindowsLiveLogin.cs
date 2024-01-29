using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.DataEngine;
using CMS.Membership;
using CMS.SiteProvider;

using Newtonsoft.Json;

using SystemIO = System.IO;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// Class for windows live login authentication.
    /// </summary>
    public class WindowsLiveLogin
    {
        #region "Constants"

        private const string TOKEN_URL = "https://login.live.com/oauth20_token.srf";
        private const string LOGOUT_URL = "https://login.live.com/oauth20_logout.srf?client_id={0}";
        private const string USER_INFO_URL = "https://apis.live.net/v5.0/me?access_token={0}";
        private const string LOGIN_PAGE = "~/CMSPages/LiveIDLogin.aspx";

        #endregion


        #region "Variables"

        private string mApplicationSecret;
        private string mApplicationID;
        private static bool? mUseServerSideAuthorization;

        #endregion


        #region "Properties"

        /// <summary>
        /// Application secret
        /// </summary>
        public string Secret
        {
            get
            {
                if (String.IsNullOrEmpty(mApplicationSecret) && !String.IsNullOrEmpty(SiteName))
                {
                    mApplicationSecret = SettingsKeyInfoProvider.GetValue(SiteName + ".CMSApplicationSecret");
                }

                return mApplicationSecret;
            }
            set
            {
                mApplicationSecret = value;
            }
        }


        /// <summary>
        /// Return true if user uses server side authorization scenario
        /// </summary>
        public static bool UseServerSideAuthorization
        {
            get
            {
                if (mUseServerSideAuthorization == null)
                {
                    mUseServerSideAuthorization = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseServerSideLiveIDAuthentication"], true);
                }

                return mUseServerSideAuthorization.Value;
            }
        }


        /// <summary>
        /// Application ID
        /// </summary>
        public string AppID
        {
            get
            {
                if (String.IsNullOrEmpty(mApplicationID) && !String.IsNullOrEmpty(SiteName))
                {
                    mApplicationID = SettingsKeyInfoProvider.GetValue(SiteName + ".CMSApplicationID");
                }

                return mApplicationID;
            }
            set
            {
                mApplicationID = value;
            }
        }


        /// <summary>
        /// Current site name
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Object constructor
        /// </summary>
        /// <param name="siteName">Current site name</param>
        public WindowsLiveLogin(String siteName)
        {
            SiteName = siteName;
        }


        /// <summary>
        /// Object constructor
        /// </summary>
        /// <param name="appID">Application Id (stored in settings)</param>
        /// <param name="secret">Application secret (stored in settings)</param>
        public WindowsLiveLogin(String appID, String secret)
        {
            Secret = secret;
            AppID = appID;
        }

        #endregion


        #region "User class"

        /// <summary>
        /// Holds the user information after a successful sign-in.
        /// </summary>
        [Serializable]
        public class User
        {
            #region "Properties"

            /// <summary>
            ///  Returns the timestamp as obtained from the SSO token.
            /// </summary>
            public DateTime Timestamp
            {
                get;
                private set;
            }


            /// <summary>
            /// Returns the pairwise unique ID for the user.
            /// </summary>
            public string Id
            {
                get;
                private set;
            }


            /// <summary>
            /// Returns the application context that was originally passed
            /// to the sign-in request, if any.
            /// </summary>
            public string Context
            {
                get;
                private set;
            }

            /// <summary>
            /// User token
            /// </summary>
            public string Token
            {
                get;
                private set;
            }

            #endregion


            /// <summary>
            /// User constructor.
            /// </summary>
            public User(string timestamp, string id, string context, string token)
            {
                Timestamp = ValidationHelper.GetDateTime(timestamp, DateTimeHelper.ZERO_TIME);
                SetUserData(id, context, token);
            }


            /// <summary>
            /// User constructor.
            /// </summary>
            public User(DateTime timestamp, string id, string context, string token)
            {
                Timestamp = timestamp;
                SetUserData(id, context, token);
            }


            private void SetUserData(string id, string context, string token)
            {
                SetId(id);
                Context = context;
                Token = token;

            }


            /// <summary>
            /// Sets the pairwise unique ID for the user.
            /// </summary>
            /// <param name="id">User ID</param>
            private void SetId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Error: User: Null id in token.");
                }

                Regex re = RegexHelper.GetRegex(@"^\w+$", RegexOptions.None);
                if (!re.IsMatch(id))
                {
                    throw new ArgumentException("Error: User: Invalid id: " + id);
                }

                Id = id;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// SignOut handler method
        /// </summary>
        /// <param name="e">SignOutEventArgs</param>
        internal static void SignOut(SignOutEventArgs e)
        {
            // If the user has registered Windows Live ID
            if (!String.IsNullOrEmpty(e.SignOutUrl) && !String.IsNullOrEmpty(e.User.UserSettings.WindowsLiveID))
            {
                // Get data from auth cookie
                string[] userData = AuthenticationHelper.GetUserDataFromAuthCookie();

                // If user has logged in using Windows Live ID, then sign him out from Live too
                if ((userData != null) && (Array.IndexOf(userData, "liveidlogin") >= 0))
                {
                    string siteName = SiteContext.CurrentSiteName;

                    // Get LiveID settings
                    string appId = SettingsKeyInfoProvider.GetValue(siteName + ".CMSApplicationID");
                    string secret = SettingsKeyInfoProvider.GetValue(siteName + ".CMSApplicationSecret");

                    // Check valid Windows LiveID parameters
                    if ((appId != string.Empty) && (secret != string.Empty))
                    {
                        // Set output LiveID sign out URL
                        WindowsLiveLogin wll = new WindowsLiveLogin(appId, secret);
                        e.SignOutUrl = wll.GetLogoutUrl(URLHelper.GetAbsoluteUrl(e.SignOutUrl));
                    }
                }
            }
        }


        /// <summary>
        /// Returns logout URL.
        /// </summary>
        /// <param name="redirectUrl">Redirect URL</param>
        public string GetLogoutUrl(string redirectUrl = null)
        {
            string url = String.Format(LOGOUT_URL, AppID);
            if (redirectUrl != null)
            {
                url += "&redirect_uri=" + HttpUtility.UrlEncode(redirectUrl);
            }

            return url;
        }


        /// <summary>
        /// Authorize user
        /// </summary>
        /// <param name="code">Code representing user</param>
        /// <param name="context">Context (url) for return page (that means LiveIDLogin webpart)</param>
        public User ProcessLogin(String code, String context)
        {
            try
            {
                // Get token from LiveID service
                String token = GetUserAccessToken(code);

                if (!String.IsNullOrEmpty(token))
                {
                    // User token for obtain user information
                    NameValueCollection coll = GetUserInformation(token);
                    User user = new User(coll["updated_time"], coll["id"], context, token);

                    return user;
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                using (var responseStream = response.GetResponseStream())
                using (var responseStreamReader = StreamReader.New(responseStream))
                {
                    var text = responseStreamReader.ReadToEnd();
                    EventLogProvider.LogException("LiveID login", EventType.ERROR, ex, additionalMessage: text);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("LiveID login", EventType.ERROR, ex);
            }

            return null;
        }


        /// <summary>
        /// Returns access token for user (represented by code)
        /// </summary>
        /// <param name="code">Code representing user (return after first live id request)</param>
        private string GetUserAccessToken(String code)
        {
            // Same URL path as in LiveID webpart
            var uriEnc = HttpUtility.UrlEncode(GetLoginUrl());
            var secret = HttpUtility.UrlEncode(Secret);

            // POST request data
            var post_data = new StringBuilder();
            post_data.Append("client_id=");
            post_data.Append(AppID);
            post_data.Append("&redirect_uri=");
            post_data.Append(uriEnc);
            post_data.Append("&client_secret=");
            post_data.Append(secret);
            post_data.Append("&code=");
            post_data.Append(code);
            post_data.Append("&grant_type=authorization_code");

            // Create byte array
            var postBytes = Encoding.ASCII.GetBytes(post_data.ToString());

            // Create and send request
            var request = (HttpWebRequest)WebRequest.Create(TOKEN_URL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(postBytes, 0, postBytes.Length);
            }

            // Get response
            var response = (HttpWebResponse)request.GetResponse();
            string responseStr;
            using (var stream = response.GetResponseStream())
            {
                responseStr = StreamReader.New(stream).ReadToEnd();
            }

            var accessTokenData = JsonConvert.DeserializeObject<OAuth2AccessTokenData>(responseStr);

            return (accessTokenData != null) ? accessTokenData.AccessToken : string.Empty;
        }


        /// <summary>
        /// Returns user information by given token.
        /// </summary>
        /// <param name="token">Token string</param>
        public NameValueCollection GetUserInformation(String token)
        {
            // User info access URL
            string url = String.Format(USER_INFO_URL, token);

            // Create request
            WebRequest wRequest = WebRequest.Create(url);

            // Get response
            var objStream = wRequest.GetResponse().GetResponseStream();

            // Store response stream to string
            String info = StreamReader.New(objStream).ReadToEnd();

            // Parse info string to name value collection
            return ParseUserInfo(info);
        }


        /// <summary>
        /// Create name-value collection from string with user data
        /// </summary>
        /// <param name="input">String to be parsed</param>
        private NameValueCollection ParseUserInfo(String input)
        {
            NameValueCollection pairs = new NameValueCollection();
            string[] kvs = input.Split(',');

            foreach (string kv in kvs)
            {
                string trimmed = Regex.Replace(kv, @"[{\r\n\t""]", "").Trim();
                int separator = trimmed.IndexOfCSafe(':');
                if ((separator == -1) || (separator == kv.Length))
                {
                    continue;
                }

                pairs[trimmed.Substring(0, separator).Trim()] = trimmed.Substring(separator + 1).Trim();
            }

            return pairs;
        }


        /// <summary>
        /// Returns live ID login URL 
        /// </summary>
        public String GetLoginUrl()
        {
            return URLHelper.GetAbsoluteUrl(LOGIN_PAGE);
        }

        #endregion


        #region "Helper static methods"

        /// <summary>
        /// Clear live id cookies and redirect to the logon page.
        /// </summary>
        public static void ClearCookieAndRedirect(String loginPage)
        {
            ClearLiveCookie();

            // Redirect to login page
            URLHelper.Redirect(loginPage);
        }


        /// <summary>
        /// Clears Live authentication cookie.
        /// </summary>
        public static void ClearLiveCookie()
        {
            HttpCookie loginCookie = new HttpCookie(CookieName.LiveID);
            loginCookie.Expires = DateTime.Now.AddYears(-10);

            CookieHelper.SetValue(loginCookie.Name, loginCookie.Value, loginCookie.Expires);
        }

        #endregion


        #region "Client authentication methods"

        /// <summary>
        /// Authenticate live ID user by authorization token
        /// </summary>
        /// <param name="token">Authorization token</param>
        /// <param name="context">Return URL</param>        
        /// <param name="accessToken">Access token - unique token for user, used in server authorization scenario. Store for possible future use</param>
        public User AuthenticateClientToken(string token, string context, string accessToken)
        {
            // No token found, return null
            if (String.IsNullOrEmpty(token))
            {
                return null;
            }

            // Get the token segments & perform validation
            string[] tokenSegments = token.Split('.');
            if (tokenSegments.Length != 3)
            {
                return null;
            }

            // Parse token data (envelope) from JSON format to class
            JsonWebTokenClaims claims = GetClaimsFromTokenSegment(tokenSegments[1]);

            // If parsing failed, return null
            if (claims == null)
            {
                return null;
            }

            // Validate hash at the end of token
            if (!ValidateSignature(tokenSegments[0], tokenSegments[1], tokenSegments[2]))
            {
                return null;
            }

            // If hash validation was OK, return Live ID user object
            return new User(claims.Expiration, claims.UserId, context, accessToken);
        }


        /// <summary>
        /// Parse token segment stored in JSON format to class
        /// </summary>
        /// <param name="claimsTokenSegment">Segment of data</param>
        private JsonWebTokenClaims GetClaimsFromTokenSegment(string claimsTokenSegment)
        {
            // Get byte array from segment
            byte[] claimsData = Base64UrlDecode(claimsTokenSegment);

            // Create JSON serializer
            DataContractJsonSerializer ClaimsJsonSerializer = new DataContractJsonSerializer(typeof(JsonWebTokenClaims));

            // Conversion token to class
            using (var memoryStream = new SystemIO.MemoryStream(claimsData))
            {
                return ClaimsJsonSerializer.ReadObject(memoryStream) as JsonWebTokenClaims;
            }
        }


        /// <summary>
        /// Validates the signature added to the end of token
        /// </summary>
        /// <param name="envelope">Envelope data segment</param>
        /// <param name="token">Token (body) data segment</param>
        /// <param name="signature">Hash signature</param>
        private bool ValidateSignature(string envelope, string token, string signature)
        {
            UTF8Encoding UTF8Encoder = new UTF8Encoding(true, true);
            SHA256Managed SHA256Provider = new SHA256Managed();

            // Derive signing key, Signing key = SHA256(secret + "JWTSIG")
            byte[] bytes = UTF8Encoder.GetBytes(Secret + "JWTSig");
            byte[] signingKey = SHA256Provider.ComputeHash(bytes);

            // To Validate:
            // 
            // 1. Take the bytes of the UTF-8 representation of the JWT Claim
            //  Segment and calculate an HMAC SHA-256 MAC on them using the
            //  shared key.
            //
            // 2. Base64url encode the previously generated HMAC as defined in this
            //  document.
            //
            // 3. If the JWT Crypto Segment and the previously calculated value
            //  exactly match in a character by character, case sensitive
            //  comparison, then one has confirmation that the key was used to
            //  generate the HMAC on the JWT and that the contents of the JWT
            //  Claim Segment have not be tampered with.
            //
            // 4. If the validation fails, the token MUST be rejected.

            // UFT-8 representation of the JWT envelope.claim segment
            byte[] input = UTF8Encoder.GetBytes(envelope + "." + token);

            // calculate an HMAC SHA-256 MAC
            using (HMACSHA256 hashProvider = new HMACSHA256(signingKey))
            {
                byte[] myHashValue = hashProvider.ComputeHash(input);

                // Base64 url encode the hash
                string base64urlEncodedHash = Base64UrlEncode(myHashValue);

                // Compare two hashes
                return base64urlEncodedHash == signature;
            }
        }


        #region Base64 Encode / Decode Functions

        /// <summary>
        /// Decode from Base 64
        /// </summary>
        /// <param name="encodedSegment">Segment to decode</param>
        public byte[] Base64UrlDecode(string encodedSegment)
        {
            string s = encodedSegment;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default: throw new Exception("Illegal base64url string");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }


        /// <summary>
        /// Encode to Base 64
        /// </summary>
        /// <param name="arg">Array of bytes to encode</param>
        public string Base64UrlEncode(byte[] arg)
        {
            string s = Convert.ToBase64String(arg); // Standard base64 encoder
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }
        #endregion

        #endregion
    }
}