using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using CMS.Base;
using CMS.Helpers;
using CMS.SocialMarketing.LinkedInInternal;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides methods for publishing to LinkedIn.
    /// </summary>
    public class LinkedInHelper : AbstractHelper<LinkedInHelper>
    {
        #region "Constants"

        /// <summary>
        /// Max length of a LinkedIn share comment.
        /// </summary>
        public const long COMPANY_SHARE_COMMENT_MAX_LENGTH = 700L;


        // URL for posting a new LinkedIn company share. The company ID is the first parameter.
        private const string COMPANY_SHARE_URL_FORMAT = "https://api.linkedin.com/v1/companies/{0}/shares";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Publishes a new share on company profile identified by <see cref="LinkedInAccountInfo.LinkedInAccountID"/>.
        /// The share is visible to anyone.
        /// </summary>
        /// <param name="accountId">Identifier of the account info object (company profile).</param>
        /// <param name="comment">Comment text.</param>
        /// <returns>Update key of the published share.</returns>
        /// <exception cref="ArgumentException">Thrown when authorization or LinkedIn's company ID resulting from accountId is null, or when comment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when comment is longer than <see cref="COMPANY_SHARE_COMMENT_MAX_LENGTH"/></exception>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        public static string PublishShareOnLinkedInCompanyProfile(int accountId, string comment)
        {
            LinkedInAccountInfo accountInfo = LinkedInAccountInfoProvider.GetLinkedInAccountInfo(accountId);

            // If accountId is invalid, resulting authorization will be null
            LinkedInAuthorization authorization = LinkedInAuthorizationHelper.GetAuthorizationByAccountInfo(accountInfo);

            return HelperObject.PublishShareOnLinkedInCompanyProfileInternal(authorization, accountInfo.LinkedInAccountProfileID, comment);
        }


        /// <summary>
        /// Publishes a new share on company profile identified by LinkedIn's company ID.
        /// The share is visible to anyone.
        /// </summary>
        /// <param name="authorization">Authorization to be used for the publishing request.</param>
        /// <param name="companyId">Identifier of the company used by LinkedIn.</param>
        /// <param name="comment">Comment text.</param>
        /// <returns>Update key of the published share.</returns>
        /// <exception cref="ArgumentException">Thrown when authorization, companyId or comment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when comment is longer than <see cref="COMPANY_SHARE_COMMENT_MAX_LENGTH"/></exception>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        internal static string PublishShareOnLinkedInCompanyProfile(LinkedInAuthorization authorization, string companyId, string comment)
        {
            return HelperObject.PublishShareOnLinkedInCompanyProfileInternal(authorization, companyId, comment);
        }


        /// <summary>
        /// Begins authorization process by redirecting client to the LinkedIn authorization page.
        /// </summary>
        /// <param name="tokenManager">Token manager used for authorization - the same instance must be provided to CompleteAuthorization method.</param>
        /// <param name="callbackUrl">URL the client is redirected to after authorization.</param>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        /// <exception cref="Exception">Throw when unexpected error occurs.</exception>
        public static void BeginAuthorization(TokenManager tokenManager, string callbackUrl)
        {
            HelperObject.BeginAuthorizationInternal(tokenManager, callbackUrl);
        }


        /// <summary>
        /// Completes authorization and gets user's access token. All needed parameters are obtained from query string (LinkedIn adds these parameters when redirecting to callback URL).
        /// </summary>
        /// <param name="tokenManager">Token manager used for authorization - the same instance that was used to BeginAuthorization must be provided.</param>
        /// <param name="callbackUrl">Return url which will be used after successfull authorization.</param>
        /// <returns>LinkedInAccessToken structure.</returns>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        /// <exception cref="Exception">Throw when unexpected error occurs.</exception>
        public static LinkedInAccessToken CompleteAuthorization(TokenManager tokenManager, string callbackUrl = null)
        {
            return HelperObject.CompleteAuthorizationInternal(tokenManager, callbackUrl);
        }


        /// <summary>
        /// Gets all companies administrated by the user that owns given access token.
        /// </summary>
        /// <param name="consumerKey">Application API key.</param>
        /// <param name="consumerSecret">Application API secret.</param>
        /// <param name="accessToken">User's access token.</param>
        /// <param name="accessTokenSecret">User's access token secret.</param>
        /// <returns>Returns list of administrated companies or null if something went wrong.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrawn.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        /// <exception cref="Exception">Thrown when unexpected error occurs.</exception>
        public static List<LinkedInCompany> GetUserCompanies(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            return HelperObject.GetUserCompaniesInternal(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Processes POST request specified by URI and request body. The request is made on behalf of given authorization.
        /// </summary>
        /// <param name="authorization">Authorization to be used for the request</param>
        /// <param name="requestUri">URI which will be requested</param>
        /// <param name="requestBody">Request body</param>
        /// <returns>Response resulting from the request.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrawn.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        internal static ApiResponse ProcessPostRequest(LinkedInAuthorization authorization, Uri requestUri, string requestBody)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
            request.Method = "POST";
            request.Headers.Add("Authorization", $"Bearer {authorization.AccessToken}");
            request.ContentType = "text/xml";

            WriteRequest(request, requestBody);

            return ProcessApiResponse(request);
        }


        /// <summary>
        /// Processes GET request specified by URI. The request is made on behalf of given authorization.
        /// </summary>
        /// <param name="authorization">Authorization to be used for the request</param>
        /// <param name="requestUri">URI which will be requested</param>
        /// <returns>Response resulting from the request.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrawn.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        internal static ApiResponse ProcessGetRequest(LinkedInAuthorization authorization, Uri requestUri)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
            request.Headers.Add("Authorization", $"Bearer {authorization.AccessToken}");
            request.ContentType = "text/xml";
            return ProcessApiResponse(request);
        }


        /// <summary>
        /// Processes API response to given request.
        /// </summary>
        /// <param name="request">Request for which to process the response.</param>
        /// <returns>Response resulting from the request.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrawn.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        internal static ApiResponse ProcessApiResponse(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                bool protocolError;
                response = GetResponse(request, out protocolError);
                string responseBody = ReadReponse(response);

                if (protocolError)
                {
                    // Request resulted in a response, but was unsuccessful
                    Error error = LinkedInXmlSerializer.Deserialize<Error>(responseBody);

                    if (error.Status == 401)
                    {
                        throw new LinkedInApiUnauthorizedException(error.Status, error.ErrorCode, error.Message);
                    }
                    else if ((error.Status == 403) && "Throttle limit for calls to this resource is reached.".EqualsCSafe(error.Message))
                    {
                        throw new LinkedInApiThrottleLimitException(error.Status, error.ErrorCode, error.Message);
                    }
                    else
                    {
                        throw new LinkedInApiException(error.Status, error.ErrorCode, error.Message);
                    }
                }
                else
                {
                    // Request succeeded, return the body and status
                    ApiResponse result = new ApiResponse
                    {
                        HttpStatusCode = response.StatusCode,
                        ResponseBody = responseBody
                    };

                    return result;
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }


        /// <summary>
        /// Gets response for given HttpWebRequest.
        /// When a <see cref="WebException"/> with status <see cref="WebExceptionStatus.ProtocolError"/> occurs, the response is still returned.
        /// You should always check the response status after call to this method to see whether the request succeeded.
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="protocolError">True if protocol error occurred. The response may contain additional information describing the error condition.</param>
        /// <returns>Response for corresponding request</returns>
        /// <exception cref="WebException">Thrown when abort was previously called or timeout occurred or an error occurred while processing the request.</exception>
        /// <exception cref="ProtocolViolationException">When request was not issued correctly.</exception>
        internal static HttpWebResponse GetResponse(HttpWebRequest request, out bool protocolError)
        {
            protocolError = false;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                return response;
            }
            catch (WebException ex)
            {
                // The response received from the server was complete but indicated a protocol-level error.
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    protocolError = true;

                    return (HttpWebResponse)ex.Response;
                }

                throw;
            }
        }


        /// <summary>
        /// Writes data stored in a string to the request object.
        /// Uses UTF-8 encoding for the outgoing data.
        /// </summary>
        /// <param name="request">Request whose stream will be used</param>
        /// <param name="data">Data to be written to the stream</param>
        internal static void WriteRequest(HttpWebRequest request, string data)
        {
            byte[] requestData = Encoding.UTF8.GetBytes(data);
            int requestDataLength = requestData.Length;
            request.ContentLength = requestDataLength;
            request.ContentType = "text/xml";
            using (var stream = request.GetRequestStream())
            {
                stream.Write(requestData, 0, requestDataLength);
            }
        }


        /// <summary>
        /// Reads response content from a HttpWebResponse.
        /// </summary>
        /// <param name="response">HttpWebResponse from which to read the response stream</param>
        /// <returns>String obtained from response stream (UTF-8 encoded)</returns>
        /// <remarks>You should always call the response <see cref="HttpWebResponse.Close"/> method to make sure the connection is released even if getting the response stream fails.</remarks>
        internal static string ReadReponse(HttpWebResponse response)
        {
            Stream stream = null;
            try
            {
                stream = response.GetResponseStream();

#pragma warning disable BH1014 // Do not use System.IO
                var reader = new StreamReader(stream, Encoding.UTF8);
#pragma warning restore BH1014 // Do not use System.IO

                return reader.ReadToEnd();
            }
            finally
            {
                // Closing the stream closes the response's resources as well.
                stream?.Close();
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Publishes a new share on company profile identified by LinkedIn's company ID.
        /// </summary>
        /// <param name="authorization">Authorization to be used for the publishing request.</param>
        /// <param name="companyId">Identifier of the company used by LinkedIn.</param>
        /// <param name="comment">Comment text.</param>
        /// <returns>Update key of the published share.</returns>
        /// <exception cref="ArgumentException">Thrown when authorization, companyId or comment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when comment is longer than <see cref="COMPANY_SHARE_COMMENT_MAX_LENGTH"/></exception>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        protected virtual string PublishShareOnLinkedInCompanyProfileInternal(LinkedInAuthorization authorization, string companyId, string comment)
        {
            if (authorization == null)
            {
                throw new ArgumentException("Authorization must be provided.", nameof(authorization));
            }
            if (String.IsNullOrEmpty(companyId))
            {
                throw new ArgumentException("Company identifier must be provided.", nameof(companyId));
            }
            if (String.IsNullOrWhiteSpace(comment) || (comment.Length > COMPANY_SHARE_COMMENT_MAX_LENGTH))
            {
                throw new ArgumentOutOfRangeException(nameof(comment), String.Format("Comment must be provided. The max length of a comment is {0} characters.", COMPANY_SHARE_COMMENT_MAX_LENGTH));
            }

            // Prepare request URI and body
            Uri publishingUri = new Uri(String.Format(COMPANY_SHARE_URL_FORMAT, companyId));
            string xmlRequestBody = GetXmlForCompanyShare(comment, LinkedInVisibilityCodeEnum.Anyone);

            // Make request
            ApiResponse response = ProcessPostRequest(authorization, publishingUri, xmlRequestBody);
            if (response.HttpStatusCode == HttpStatusCode.Created)
            {
                Update update = LinkedInXmlSerializer.Deserialize<Update>(response.ResponseBody);

                return update.UpdateKey;
            }

            // The request should not succeed and return HTTP status code different than 201 (Created).
            throw new ProtocolViolationException(String.Format("Unexpected LinkedIn API response for publishing request. Request URI: {0}\nRequest body: {1}\nHTTP status code: {2}\nResponse body: {3}",
                publishingUri, xmlRequestBody, response.HttpStatusCode, response.ResponseBody));
        }


        /// <summary>
        /// Begins authorization process by redirecting client to the LinkedIn authorization page.
        /// </summary>
        /// <param name="tokenManager">Token manager used for authorization - the same instance must be provided to CompleteAuthorization method.</param>
        /// <param name="callbackUrl">URL the client is redirected to after authorization.</param>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs and the root cause is not being authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        protected void BeginAuthorizationInternal(TokenManager tokenManager, string callbackUrl)
        {
            var authorization = new LinkedInAuthorization(tokenManager, null);

            try
            {
                // Redirect to LinkedIn OAuth authorization page
                authorization.BeginAuthorize(new Uri(URLHelper.GetAbsoluteUrl(callbackUrl)));
            }
            catch (ThreadAbortException)
            {
                // Reset exception - this exception is expected because client is redirected to external page.
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException as WebException;
                if (innerException != null && innerException.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)innerException.Response;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // The cause is not being authorized - API key or Secret key is invalid
                        throw new LinkedInApiUnauthorizedException((int)HttpStatusCode.Unauthorized, 0, response.StatusDescription);
                    }
                }

                throw new LinkedInApiException("Protocol error occurs.", ex.InnerException);
            }
        }


        /// <summary>
        /// Completes authorization and gets user's access token. All needed parameters are obtained from query string (LinkedIn adds these parameters when redirecting to callback URL).
        /// </summary>
        /// <param name="tokenManager">Token manager used for authorization - the same instance that was used to BeginAuthorization must be provided.</param>
        /// <param name="callbackUrl">Return url which will be used after successfull authorization.</param>
        /// <returns>LinkedInAccessToken structure.</returns>
        /// <exception cref="InvalidOperationException">Throw when access token is not retrieved.</exception>
        /// <exception cref="Exception">Throw when unexpected error occurs.</exception>
        protected LinkedInAccessToken CompleteAuthorizationInternal(TokenManager tokenManager, string callbackUrl = null)
        {
            var url = callbackUrl != null ? new Uri(callbackUrl) : null;

            var authorization = new LinkedInAuthorization(tokenManager, null, url);

            string accessToken = authorization.CompleteAuthorize();
            if (String.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Access token wasn't retrieved.");
            }

            return new LinkedInAccessToken(accessToken, null, DateTime.Now.AddDays(60));
        }


        /// <summary>
        /// Gets all companies administrated by the user that owns given access token.
        /// </summary>
        /// <param name="consumerKey">Application API key.</param>
        /// <param name="consumerSecret">Application API secret.</param>
        /// <param name="accessToken">User's access token.</param>
        /// <param name="accessTokenSecret">User's access token secret.</param>
        /// <returns>Returns list of administrated companies.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrawn.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        /// <exception cref="InvalidOperationException">Thrown when unexpected error occurs.</exception>
        protected List<LinkedInCompany> GetUserCompaniesInternal(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var tokenManager = new TokenManager(consumerKey, consumerSecret);
            tokenManager.AddToken(accessToken, accessTokenSecret);

            var authorization = new LinkedInAuthorization(tokenManager, accessToken);
            ApiResponse response = ProcessGetRequest(authorization, new Uri("https://api.linkedin.com/v1/companies?is-company-admin=true"));

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                Companies res = LinkedInXmlSerializer.Deserialize<Companies>(response.ResponseBody);

                return res.CompanyList.Select(x => new LinkedInCompany() { ID = x.ID, Name = x.Name }).ToList();
            }

            throw new InvalidOperationException(" Administrated companies weren't retrieved.");
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets XML representation of a company share.
        /// </summary>
        /// <param name="comment">Text of company share.</param>
        /// <param name="visibilityCode">Visibility code.</param>
        /// <returns>XML suitable for LinkedIn API request.</returns>
        private string GetXmlForCompanyShare(string comment, LinkedInVisibilityCodeEnum visibilityCode)
        {
            Share share = new Share
            {
                Comment = comment,
                Visibility = new Visibility
                {
                    Code = visibilityCode.ToStringRepresentation()
                }
            };
            string serializedShare = LinkedInXmlSerializer.Serialize(share);

            return serializedShare;
        }

        #endregion
    }
}
