using System.Runtime.Serialization;

namespace CMS.SalesForce.RestContract
{

    /// <summary>
    /// Represents a SalesForce response to request for authentication tokens.
    /// </summary>
    [DataContract]
    public sealed class GetAuthenticationTokensResponse
    {

        #region "Public members"

        /// <summary>
        /// The URL with the SalesForce organization access details.
        /// </summary>
        [DataMember(Name="id")]
        public string IdentityUrl;

        /// <summary>
        /// The OAuth access tokens.
        /// </summary>
        [DataMember(Name = "access_token")]
        public string AccessToken;

        /// <summary>
        /// The OAuth refresh token.
        /// </summary>
        [DataMember(Name = "refresh_token")]
        public string RefreshToken;
        
        /// <summary>
        /// A base URL of the organization.
        /// </summary>
        [DataMember(Name = "instance_url")]
        public string InstanceBaseUrl;

        #endregion

    }

}