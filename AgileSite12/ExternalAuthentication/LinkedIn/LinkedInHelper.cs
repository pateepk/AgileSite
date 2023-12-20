using System;
using System.Net;
using System.Xml;

using CMS.Core;
using CMS.DataEngine;
using CMS.ExternalAuthentication.LinkedIn;
using CMS.Helpers;
using CMS.IO;

using Newtonsoft.Json;

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
        /// API endpoint for the LinkedIn profile request.
        /// </summary>
        private const string PROFILE_API_ENDPOINT = "https://api.linkedin.com/v2/me";

        #endregion


        #region "Properties"

        /// <summary>
        /// User member id.
        /// </summary>
        [Obsolete("This property is obsolete and will be removed in the next version. Use member LinkedInProfile.Id instead. LinkedInProfile is returned as an output parameter of CheckStatus method.")]
        public string MemberId
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// User first name.
        /// </summary>
        [Obsolete("This property is obsolete and will be removed in the next version. Use member LinkedInProfile.LocalizedFirstName instead. LinkedInProfile is returned as an output parameter of CheckStatus method")]
        public string FirstName
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// User last name.
        /// </summary>
        [Obsolete("This property is obsolete and will be removed in the next version. Use member LinkedInProfile.LocalizedLastName instead. LinkedInProfile is returned as an output parameter of CheckStatus method")]
        public string LastName
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// User date of birth.
        /// </summary>
        [Obsolete("This property is obsolete and will be removed in the next version. Use code LinkedInProfile.BirthDate?.ToDateTime() instead. LinkedInProfile is returned as an output parameter of CheckStatus method")]
        public DateTime BirthDate
        {
            get;
            set;
        } = DateTimeHelper.ZERO_TIME;


        /// <summary>
        /// Gets or sets LinkedIn Response as XmlDocument object.
        /// </summary>
        [Obsolete("This property is not compatible with LinkedIn v2 API and should be no longer used. It will be removed in the next version.")]
        public XmlDocument LinkedInResponse
        {
            get;
            private set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Redirects user to LinkedIn with received authToken.
        /// </summary>
        /// <param name="data">Linked in related data.</param>
        public void SendRequest(ILinkedInData data)
        {
            var returnUrl = GetPurifiedUrl();

            LinkedInProvider.OpenAuthorizationPage(data, returnUrl);
        }


        /// <summary>
        /// Checks status of current user. Returns LinkedIn response status.
        /// </summary>
        /// <param name="requireFirstName">Require first name</param>
        /// <param name="requireLastName">Require last name</param>
        /// <param name="requireBirthDate">Require birth date</param>
        /// <param name="customFields">Array of custom LinkedIn profile field names</param>
        [Obsolete("This method is not compatible with LinkedIn v2 API and should be no longer used. It will be removed in the next version. Use method CheckStatus(bool, bool, bool, out LinkedInProfile) instead.")]
        public string CheckStatus(bool requireFirstName, bool requireLastName, bool requireBirthDate, string[] customFields)
        {
            return CheckStatus(requireFirstName, requireLastName, requireBirthDate, out _);
        }


        /// <summary>
        /// Checks status of current user. Returns LinkedIn response status.
        /// </summary>
        /// <param name="requireFirstName">Require first name.</param>
        /// <param name="requireLastName">Require last name.</param>
        /// <param name="requireBirthDate">Require birth date.</param>
        /// <param name="linkedInProfile">Output parameter for LinkedIn profile.</param>
        public string CheckStatus(bool requireFirstName, bool requireLastName, bool requireBirthDate, out LinkedInProfile linkedInProfile)
        {
            var returnUrl = GetPurifiedUrl();
            linkedInProfile = null;

            var data = LinkedInProvider.GetLinkedInData();
            if (!data.IsEmpty && !data.UserDeniedAccess && LinkedInProvider.Authorize(data, returnUrl, out var token))
            {
                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(PROFILE_API_ENDPOINT);
                    webRequest.PreAuthenticate = true;
                    webRequest.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
                    webRequest.Accept = "application/json";

                    // Need to specify protocol version to use v2 API, more info here https://developer.linkedin.com/docs/guide/v2/concepts/protocol-version
                    webRequest.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

                    using (var webResponse = webRequest.GetResponse())
                    {
                        var responseStream = webResponse.GetResponseStream();
                        if (responseStream == null)
                        {
                            return RESPONSE_NOTAUTHENTICATED;
                        }

                        using (var streamReader = StreamReader.New(responseStream))
                        {
                            var response = streamReader.ReadToEnd();
#pragma warning disable 618
                            linkedInProfile = InitializeProfile(requireFirstName, requireLastName, requireBirthDate, response);
#pragma warning restore 618
                        }

                        return RESPONSE_AUTHENTICATED;
                    }
                }
                catch (Exception exception)
                {
                    Service.Resolve<IEventLogService>().LogException(nameof(LinkedInHelper), "CheckStatus", exception);
                    URLHelper.Redirect(GetPurifiedUrl().ToString());
                }
            }

            return RESPONSE_NOTAUTHENTICATED;
        }


        /// <summary>
        /// Returns <see cref="RequestContext.CurrentURL"/> with stripped OAuth parameters. 
        /// </summary>
        public static Uri GetPurifiedUrl()
        {
            var url = RequestContext.CurrentURL;
            url = URLHelper.RemoveParameterFromUrl(url, "code");
            url = URLHelper.RemoveParameterFromUrl(url, "state");
            url = URLHelper.RemoveParameterFromUrl(url, "error");
            url = URLHelper.RemoveParameterFromUrl(url, "error_description");

            return new Uri(URLHelper.GetAbsoluteUrl(url));
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
            return SettingsKeyInfoProvider.GetBoolValue("CMSEnableLinkedIn", siteName);
        }


        /// <summary>
        /// Returns api key.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetLinkedInApiKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue("CMSLinkedInApiKey", siteName);
        }


        /// <summary>
        /// Returns application secret.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetLinkedInSecretKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue("CMSLinkedInApplicationSecret", siteName);
        }


        /// <summary>
        /// Loads LinkedIn response.
        /// </summary>
        /// <param name="response">LinkedIn response as XmlDocument object</param>
        [Obsolete("This method is not compatible with LinkedIn v2 API and should be no longer used. It will be removed in the next version.")]
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

        [Obsolete("Initialization of obsolete properties should be removed.")]
        private LinkedInProfile InitializeProfile(bool requireFirstName, bool requireLastName, bool requireBirthDate, string response)
        {
            var profile = JsonConvert.DeserializeObject<LinkedInProfile>(response);

            MemberId = profile.Id;

            if (requireFirstName)
            {
                FirstName = profile.LocalizedFirstName;
            }

            if (requireLastName)
            {
                LastName = profile.LocalizedLastName;
            }

            if (requireBirthDate && profile.BirthDate != null)
            {
                BirthDate = profile.BirthDate.ToDateTime();
            }

            return profile;
        }

        #endregion
    }
}