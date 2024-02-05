using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using Newtonsoft.Json.Linq;

using StreamReader = CMS.IO.StreamReader;
using StreamWriter = CMS.IO.StreamWriter;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// LinkedIn helper class.
    /// </summary>
    public class LinkedInHelper
    {
        #region "Constants"

        /// <summary>
        /// Represents LinkedIn response "NotAuthenticated".
        /// </summary>
        public const string RESPONSE_NOTAUTHENTICATED = "NotAuthenticated";

        /// <summary>
        /// Represents LinkedIn response "Authenticated".
        /// </summary>
        public const string RESPONSE_AUTHENTICATED = "Authenticated";

        /// <summary>
        /// Represents LinkedIn BASIC profile permission access (only information related to name)
        /// </summary>
        public const string BASIC_PROFILE_PERMISSION_LEVEL = "r_basicprofile";

        /// <summary>
        /// Represents LinkedIn FULL profile permission access (all information available)
        /// </summary>
        public const string FULL_PROFILE_PERMISSION_LEVEL = "r_fullprofile";

        internal const string AUTHORIZATION_ENDPOINT = "https://www.linkedin.com/uas/oauth2/authorization";

        internal const string TOKEN_ENDPOINT = "https://www.linkedin.com/uas/oauth2/accessToken";

        #endregion


        #region "Private variables"

        private string mMemberId = "";
        private string mFirstName = "";
        private string mLastName = "";
        private DateTime mBirthDate = DateTimeHelper.ZERO_TIME;
        private XmlDocument mLinkedInResponse;

        #endregion


        #region "Properties"

        /// <summary>
        /// User member id.
        /// </summary>
        public string MemberId
        {
            get
            {
                return mMemberId;
            }
            set
            {
                mMemberId = value;
            }
        }


        /// <summary>
        /// User first name.
        /// </summary>
        public string FirstName
        {
            get
            {
                return mFirstName;
            }
            set
            {
                mFirstName = value;
            }
        }


        /// <summary>
        /// User last name.
        /// </summary>
        public string LastName
        {
            get
            {
                return mLastName;
            }
            set
            {
                mLastName = value;
            }
        }


        /// <summary>
        /// User date of birth.
        /// </summary>
        public DateTime BirthDate
        {
            get
            {
                return mBirthDate;
            }
            set
            {
                mBirthDate = value;
            }
        }


        /// <summary>
        /// Gets or sets LinkedIn Response as XmlDocument object.
        /// </summary>
        public XmlDocument LinkedInResponse
        {
            get
            {
                return mLinkedInResponse;
            }
            set
            {
                mLinkedInResponse = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Redirects user to LinkedIn with received authToken.
        /// </summary>
        /// <param name="requestParameters">Dictionary with additional request parameters</param>
        /// <param name="redirectParameters">Dictionary with additional redirect parameters</param>
        public void SendRequest(Dictionary<string, string> requestParameters = null, Dictionary<string, string> redirectParameters = null)
        {
            var manager = CreateManager();

            // Get callback URL
            Uri callback = new Uri(URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));

            try
            {
                var url = AUTHORIZATION_ENDPOINT;
                url = URLHelper.AddParameterToUrl(url, "response_type", "code");
                url = URLHelper.AddParameterToUrl(url, "client_id", manager.ConsumerKey);
                url = URLHelper.AddParameterToUrl(url, "redirect_uri", HttpUtility.UrlEncode(callback.GetLeftPart(UriPartial.Path)));
                url = URLHelper.AddParameterToUrl(url, "state", Guid.NewGuid().ToString());

                URLHelper.Redirect(url);
            }
            catch
            {
            }
        }


        /// <summary>
        /// Checks status of current user. Returns LinkedIn response status.
        /// </summary>
        /// <param name="requireFirstName">Require first name</param>
        /// <param name="requireLastName">Require last name</param>
        /// <param name="requireBirthDate">Require birth date</param>
        /// <param name="customFields">Array of custom LinkedIn profile field names</param>
        public string CheckStatus(bool requireFirstName, bool requireLastName, bool requireBirthDate, string[] customFields)
        {
            var manager = SessionHelper.GetValue("WcfTokenManager") as InMemoryTokenManager;
            if (manager != null)
            {
                string accessToken = null;

                // Get current URL
                var currentUrl = new Uri(URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL));

                // Create consumer from WcfTokenManager and remove manager 
                SessionHelper.Remove("WcfTokenManager");

                try
                {
                    var data = HttpUtility.ParseQueryString(String.Empty);
                    data.Add(new NameValueCollection
                    {
                        { "grant_type", "authorization_code" },
                        { "code", QueryHelper.GetString("code", String.Empty) },
                        { "redirect_uri", currentUrl.GetLeftPart(UriPartial.Path) },
                        { "client_id", manager.ConsumerKey },
                        { "client_secret", manager.ConsumerSecret }
                    });

                    var webRequest = (HttpWebRequest)WebRequest.Create(TOKEN_ENDPOINT);
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";

                    using (var stream = webRequest.GetRequestStream())
                    {
                        using (var writer = StreamWriter.New(stream))
                        {
                            writer.Write(data.ToString());
                        }
                    }

                    using (var webResponse = webRequest.GetResponse())
                    {
                        var responseStream = webResponse.GetResponseStream();
                        if (responseStream == null)
                        {
                            return null;
                        }

                        using (var reader = StreamReader.New(responseStream))
                        {
                            var response = reader.ReadToEnd();
                            var json = JObject.Parse(response);
                            accessToken = json.Value<string>("access_token");
                        }
                    }
                }
                catch
                {
                    URLHelper.Redirect(currentUrl.GetLeftPart(UriPartial.Path));
                }

                if (accessToken != null)
                {
                    string endPointUrl = "https://api.linkedin.com/v1/people/~:(id";

                    if (requireBirthDate)
                    {
                        endPointUrl += ",date-of-birth";
                    }
                    if (requireFirstName)
                    {
                        endPointUrl += ",first-name";
                    }
                    if (requireLastName)
                    {
                        endPointUrl += ",last-name";
                    }

                    if (customFields != null)
                    {
                        foreach (string item in customFields)
                        {
                            endPointUrl += "," + item;
                        }
                    }

                    endPointUrl += ")";

                    try
                    {
                        var webRequest = (HttpWebRequest)WebRequest.Create(endPointUrl);
                        webRequest.PreAuthenticate = true;
                        webRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        webRequest.Accept = "application/json";

                        using (var webResponse = webRequest.GetResponse())
                        {
                            var responseStream = webResponse.GetResponseStream();
                            if (responseStream == null)
                            {
                                return RESPONSE_NOTAUTHENTICATED;
                            }

                            Initialize(GetXmlResponse(responseStream));

                            return RESPONSE_AUTHENTICATED;
                        }
                    }
                    catch (Exception exception)
                    {
                        Service.Resolve<IEventLogService>().LogException(nameof(LinkedInHelper), nameof(CheckStatus), exception);
                        URLHelper.Redirect(currentUrl.ToString());
                    }
                }
            }

            return RESPONSE_NOTAUTHENTICATED;
        }


        /// <summary>
        /// Indicates if LinkedIn is available/enabled on specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool LinkedInIsAvailable(string siteName)
        {
            // Check all necessary settings values
            bool enabled = GetLinkedInEnabled(siteName);
            string apiKey = GetLinkedInApiKey(siteName);
            string secret = GetLinkedInSecretKey(siteName);

            return enabled && !String.IsNullOrEmpty(apiKey) && !String.IsNullOrEmpty(secret);
        }


        /// <summary>
        /// Indicates if LinkedIn is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool GetLinkedInEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableLinkedIn");
        }


        /// <summary>
        /// Returns api key.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetLinkedInApiKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSLinkedInApiKey");
        }


        /// <summary>
        /// Returns application secret.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetLinkedInSecretKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSLinkedInApplicationSecret");
        }


        /// <summary>
        /// Loads LinkedIn response.
        /// </summary>
        /// <param name="response">LinkedIn response as XmlDocument object</param>
        public void Initialize(XmlDocument response)
        {
            if (response != null)
            {
                LinkedInResponse = response;

                XmlNode node = response.DocumentElement;

                if (node != null)
                {
                    // MemberID
                    XmlNode idNode = node.SelectSingleNode("id");
                    if (idNode != null)
                    {
                        MemberId = idNode.InnerText;
                    }
                    // First name
                    XmlNode firstNameNode = node.SelectSingleNode("first-name");
                    if (firstNameNode != null)
                    {
                        FirstName = firstNameNode.InnerText;
                    }
                    // Last name
                    XmlNode lastNameNode = node.SelectSingleNode("last-name");
                    if (lastNameNode != null)
                    {
                        LastName = lastNameNode.InnerText;
                    }
                    // Birth date
                    XmlNode birthDateNode = node.SelectSingleNode("date-of-birth");
                    if (birthDateNode != null)
                    {
                        int year = DateTime.MinValue.Year;
                        int month = DateTime.MinValue.Month;
                        int day = DateTime.MinValue.Day;

                        // Year
                        XmlNode yearNode = birthDateNode.SelectSingleNode("year");
                        if (yearNode != null)
                        {
                            year = ValidationHelper.GetInteger(yearNode.InnerText, DateTime.MinValue.Year);
                        }
                        // Month
                        XmlNode monthNode = birthDateNode.SelectSingleNode("month");
                        if (monthNode != null)
                        {
                            month = ValidationHelper.GetInteger(monthNode.InnerText, DateTime.MinValue.Year);
                        }
                        // Day
                        XmlNode dayNode = birthDateNode.SelectSingleNode("day");
                        if (dayNode != null)
                        {
                            day = ValidationHelper.GetInteger(dayNode.InnerText, DateTime.MinValue.Year);
                        }

                        BirthDate = new DateTime(year, month, day);
                    }
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates LinkedIn WebConsumer.
        /// </summary>
        private InMemoryTokenManager CreateManager()
        {
            // Try to get InMemoryTokenManager from session
            InMemoryTokenManager tokenManager = SessionHelper.GetValue("WcfTokenManager") as InMemoryTokenManager;
            // If not found, create new and store in session
            if (tokenManager == null)
            {
                tokenManager = new InMemoryTokenManager();
                SessionHelper.SetValue("WcfTokenManager", tokenManager);
            }

            return tokenManager;
        }


        /// <summary>
        /// Returns LinkedIn response as XmlDocument.
        /// </summary>
        /// <param name="stream">LinkedIn response as IncomingWebResponse object</param>
        private static XmlDocument GetXmlResponse(Stream stream)
        {
            if (stream != null)
            {
                // Load LinkedIn response into Xml document
                XmlDocument linkedInProfile = new XmlDocument();

                linkedInProfile.Load(stream);

                return linkedInProfile;
            }
            return null;
        }

        #endregion
    }
}