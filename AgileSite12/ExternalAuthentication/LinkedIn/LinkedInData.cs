using System;
using System.Collections.Generic;

using CMS.Helpers;

namespace CMS.ExternalAuthentication.LinkedIn
{
    /// <summary>
    /// Represents data required to get LinkedIn access token.
    /// </summary>
    [Serializable]
    public sealed class LinkedInData : ILinkedInData
    {
        /// <summary>
        /// Creates a new instance of <see cref="LinkedInData"/>.
        /// </summary>
        /// <remarks>
        /// Values <see cref="EditorId"/>, <see cref="ApiKey"/> and <see cref="ApiSecret"/> are retrieved from query string.
        /// </remarks>
        public LinkedInData()
            : this(QueryHelper.GetString("apiKey", String.Empty), QueryHelper.GetString("apiSecret", String.Empty))
        {
            EditorId = QueryHelper.GetString("txtToken", String.Empty);
        }


        /// <summary>
        /// Creates a new instance of <see cref="LinkedInData"/>.
        /// </summary>
        public LinkedInData(string apiKey, string apiSecret)
        {
            AdditionalQueryParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            ApiKey = apiKey;
            ApiSecret = apiSecret;
        }


        /// <summary>
        /// Gets the editor id in order to set its value.
        /// </summary>
        public string EditorId
        {
            get;
        }


        /// <summary>
        /// Gets LinkedIn account api key.
        /// </summary>
        public string ApiKey
        {
            get;
        }


        /// <summary>
        /// Gets LinkedIn account api secret.
        /// </summary>
        public string ApiSecret
        {
            get;
        }


        /// <summary>
        /// Gets LinkedIn code required to generate access token.
        /// </summary>
        public string Code => QueryHelper.GetString("code", String.Empty);


        /// <summary>
        /// Gets the error message when access token was not retrieved.
        /// </summary>
        public string Error => QueryHelper.GetString("error", String.Empty);


        /// <summary>
        /// Returns true when all of <see cref="ApiKey"/>, <see cref="ApiSecret"/>, <see cref="Code"/> are empty.
        /// </summary>
        public bool IsEmpty => String.IsNullOrEmpty(ApiKey) && String.IsNullOrEmpty(ApiSecret) && String.IsNullOrEmpty(Code);


        /// <summary>
        /// Returns true when user does not allow access using LinkedIn page.
        /// </summary>
        public bool UserDeniedAccess => Error.Equals("access_denied", StringComparison.OrdinalIgnoreCase);


        /// <summary>
        /// Returns true when one of <see cref="ApiKey"/> and <see cref="ApiSecret"/> is empty or if <see cref="Code"/> or <see cref="Error"/> are empty.
        /// </summary>
        public bool SettingsMissing => (String.IsNullOrEmpty(ApiKey) || String.IsNullOrEmpty(ApiSecret)) && String.IsNullOrEmpty(Code) && String.IsNullOrEmpty(Error);


        /// <summary>
        /// Contains additional query parameters used when opening authorization page.
        /// </summary>
        public IDictionary<string, string> AdditionalQueryParameters
        {
            get;
        }
    }
}
