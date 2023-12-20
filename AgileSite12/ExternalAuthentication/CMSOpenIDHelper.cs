using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;

using CMS.Helpers;
using CMS.Membership;

using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// OpenID helper class.
    /// </summary>
    public class CMSOpenIDHelper
    {
        #region "Variables"

        private string mClaimedIdentifier;
        private OpenIdRelyingParty mOpenId;
        private IAuthenticationResponse mUserData;

        #endregion


        #region "Constants"

        /// <summary>
        /// Represents OpenID response "Canceled".
        /// </summary>
        public const string RESPONSE_CANCELED = "Canceled";

        /// <summary>
        /// Represents OpenID response "Failed".
        /// </summary>
        public const string RESPONSE_FAILED = "Failed";

        /// <summary>
        /// Represents OpenID response "SetupRequired".
        /// </summary>
        public const string RESPONSE_SETUPREQUIRED = "SetupRequired";

        /// <summary>
        /// Represents OpenID response "Authenticated".
        /// </summary>
        public const string RESPONSE_AUTHENTICATED = "Authenticated";

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns OpenID Claimed Identifier for current user.
        /// </summary>
        public string ClaimedIdentifier
        {
            get
            {
                return mClaimedIdentifier;
            }
            set
            {
                mClaimedIdentifier = value;
            }
        }


        /// <summary>
        /// Returns OpenID Response as IAuthenticationResponse object.
        /// </summary>
        public IAuthenticationResponse OpenIDResponse
        {
            get
            {
                return mUserData;
            }
            set
            {
                mUserData = value;
            }
        }


        /// <summary>
        /// User birthdate.
        /// </summary>
        public DateTime BirthDate
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return ValidationHelper.GetDateTime(OpenIDResponse.GetExtension<ClaimsResponse>().BirthDate, DateTime.MinValue);
                }

                return DateTime.MinValue;
            }
        }


        /// <summary>
        /// User country.
        /// </summary>
        public string Country
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().Country;
                }

                return null;
            }
        }


        /// <summary>
        /// User culture.
        /// </summary>
        public CultureInfo Culture
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().Culture;
                }

                return null;
            }
        }


        /// <summary>
        /// User e-mail.
        /// </summary>
        public string Email
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().Email;
                }

                return null;
            }
        }


        /// <summary>
        /// User full name.
        /// </summary>
        public string FullName
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().FullName;
                }

                return null;
            }
        }


        /// <summary>
        /// Integer code with user gender.
        /// </summary>
        public int? UserGender
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return GetGender(OpenIDResponse.GetExtension<ClaimsResponse>().Gender);
                }

                return null;
            }
        }


        /// <summary>
        /// User language.
        /// </summary>
        public string Language
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().Language;
                }

                return null;
            }
        }


        /// <summary>
        /// User mail address.
        /// </summary>
        public MailAddress MailAddress
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().MailAddress;
                }

                return null;
            }
        }


        /// <summary>
        /// User nickname.
        /// </summary>
        public string Nickname
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().Nickname;
                }

                return null;
            }
        }


        /// <summary>
        /// User postal code.
        /// </summary>
        public string PostalCode
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().PostalCode;
                }

                return null;
            }
        }


        /// <summary>
        /// User time zone.
        /// </summary>
        public string TimeZone
        {
            get
            {
                if ((OpenIDResponse != null) && (OpenIDResponse.GetExtension<ClaimsResponse>() != null))
                {
                    return OpenIDResponse.GetExtension<ClaimsResponse>().TimeZone;
                }

                return null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor initializes OpenID transfer and connection.
        /// </summary>
        public CMSOpenIDHelper()
        {
            mOpenId = new OpenIdRelyingParty();
            mOpenId = CreateRelyingParty();

            // Check if OpenID request was done
            if (mOpenId != null)
            {
                // Prepare response
                OpenIdRelyingParty openId = new OpenIdRelyingParty();
                var httpContext = CMSHttpContext.Current;

                // Prepare header
                WebHeaderCollection headers = new WebHeaderCollection();
                foreach (string header in httpContext.Request.Headers)
                {
                    headers.Add(header, httpContext.Request.Headers[header]);
                }

                // Change URL of request
                string requestUrl = URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL);
                HttpRequestInfo requestInfo = new HttpRequestInfo(httpContext.Request.HttpMethod,
                                                                  new Uri(requestUrl),
                                                                  httpContext.Request.RawUrl, headers,
                                                                  httpContext.Request.InputStream);

                // Get response
                OpenIDResponse = openId.GetResponse(requestInfo);

                if (OpenIDResponse != null)
                {
                    ClaimedIdentifier = OpenIDResponse.ClaimedIdentifier;
                }
            }
        }


        /// <summary>
        /// Manually sets OpenID response for this object.
        /// </summary>
        /// <param name="response">IAuthenticationResponse object</param>
        public void Initialize(object response)
        {
            OpenIDResponse = (IAuthenticationResponse)response;

            if (OpenIDResponse != null)
            {
                ClaimedIdentifier = OpenIDResponse.ClaimedIdentifier;
            }
        }


        /// <summary>
        /// Returns OpenID response as object.
        /// </summary>
        public object GetResponseObject()
        {
            return OpenIDResponse;
        }


        /// <summary>
        /// Checks status of current user.
        /// </summary>
        public string CheckStatus()
        {
            // Check if OpenID request was done
            if (OpenIDResponse != null)
            {
                switch (OpenIDResponse.Status)
                {
                    // User is authenticated
                    case AuthenticationStatus.Authenticated:
                        return RESPONSE_AUTHENTICATED;

                    // Authentication was cancelled
                    case AuthenticationStatus.Canceled:
                        return RESPONSE_CANCELED;

                    // Setup required
                    case AuthenticationStatus.SetupRequired:
                        return RESPONSE_SETUPREQUIRED;

                    // Authentication failed
                    default:
                    case AuthenticationStatus.Failed:
                        return RESPONSE_FAILED;
                }
            }
            return null;
        }


        /// <summary>
        /// Creates relying party for OpenID.
        /// </summary>
        /// <returns>OpenIdRelyingParty</returns>
        public OpenIdRelyingParty CreateRelyingParty()
        {
            int minsha, maxsha, minversion;

            // Check parameters from query string
            if (int.TryParse(QueryHelper.GetString("minsha", null), out minsha))
            {
                mOpenId.SecuritySettings.MinimumHashBitLength = minsha;
            }
            if (int.TryParse(QueryHelper.GetString("maxsha", null), out maxsha))
            {
                mOpenId.SecuritySettings.MaximumHashBitLength = maxsha;
            }
            if (int.TryParse(QueryHelper.GetString("minversion", null), out minversion))
            {
                switch (minversion)
                {
                    case 1:
                        mOpenId.SecuritySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V10;
                        break;
                    case 2:
                        mOpenId.SecuritySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("minversion");
                }
            }
            return mOpenId;
        }


        /// <summary>
        /// Send OpenID login request to specified openID provider with user demanded data.
        /// </summary>
        /// <param name="openIDprovider">URL of the OpenID provider</param>
        /// <param name="birthDateRequest">Demand level for user birth date</param>
        /// <param name="countryRequest">Demand level for user country</param>
        /// <param name="emailRequest">Demand level for user e-mail</param>
        /// <param name="fullNameRequest">Demand level for full name</param>
        /// <param name="genderRequest">Demand level for user gender</param>
        /// <param name="languageRequest">Demand level for user language</param>
        /// <param name="nicknameRequest">Demand level for user nick name</param>
        /// <param name="postalCodeRequest">Demand level for user postal code</param>
        /// <param name="timeZoneRequest">Demand level for user time zone</param>
        /// <returns>Returns NULL if request was sucessful, otherwise returns error message with additional data</returns>
        public string SendRequest(string openIDprovider, string birthDateRequest, string countryRequest, string emailRequest, string fullNameRequest, string genderRequest, string languageRequest, string nicknameRequest, string postalCodeRequest, string timeZoneRequest)
        {
            try
            {
                IAuthenticationRequest request = mOpenId.CreateRequest(openIDprovider, new Realm(URLHelper.GetFullApplicationUrl() + "/"), new Uri(URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL)));

                // Check if specified OpenID Claimed Identifier already exists in DB
                UserInfo ui = OpenIDUserInfoProvider.GetUserInfoByOpenID(openIDprovider);
                if (ui == null)
                {
                    // Create request to get requested information about user
                    ClaimsRequest cr = new ClaimsRequest();
                    cr.BirthDate = GetRequestValue(birthDateRequest);
                    cr.Country = GetRequestValue(countryRequest);
                    cr.Email = GetRequestValue(emailRequest);
                    cr.FullName = GetRequestValue(fullNameRequest);
                    cr.Gender = GetRequestValue(genderRequest);
                    cr.Language = GetRequestValue(languageRequest);
                    cr.Nickname = GetRequestValue(nicknameRequest);
                    cr.PostalCode = GetRequestValue(postalCodeRequest);
                    cr.TimeZone = GetRequestValue(timeZoneRequest);

                    // Create request
                    request.AddExtension(cr);
                }

                // Send your visitor to their Provider for authentication.
                request.RedirectToProvider();
            }
            catch (Exception ex)
            {
                // The user probably entered an Identifier that was not a valid OpenID endpoint.
                return ex.Message;
            }

            return null;
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Custom OpenID URL validation.
        /// </summary>
        public static bool IsValid(string openIDURL)
        {
            if (!String.IsNullOrEmpty(openIDURL))
            {
                return Identifier.IsValid(openIDURL);
            }
            return false;
        }


        /// <summary>
        /// Gets integer representation of Gender object.
        /// </summary>
        /// <param name="gender">Gender object</param>
        /// <returns>1 - Male, 2 - Female, null - if not set </returns>
        private static int? GetGender(Gender? gender)
        {
            if (gender == null)
            {
                return null;
            }

            // Male
            if (gender == Gender.Male)
            {
                return 1;
            }

            // Female
            if (gender == Gender.Female)
            {
                return 2;
            }
            return null;
        }


        /// <summary>
        /// Returns DemandLevel of given parameter.
        /// </summary>
        /// <param name="requestLevel">Name of requested level</param>
        /// <returns>Returns DemandLevel</returns>
        public static DemandLevel GetRequestValue(string requestLevel)
        {
            try
            {
                return (DemandLevel)Enum.Parse(typeof(DemandLevel), requestLevel, true);
            }
            catch
            {
                return DemandLevel.Request;
            }
        }

        #endregion
    }
}