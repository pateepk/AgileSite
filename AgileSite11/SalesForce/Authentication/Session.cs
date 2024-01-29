namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a SalesForce organization communication session.
    /// </summary>
    public sealed class Session
    {

        #region "Public properties"

        /// <summary>
        /// Gets the OAuth access token.
        /// </summary>
        public string AccessToken { get; internal set; }

        /// <summary>
        /// Gets the address of the server hosting the SalesForce organization.
        /// </summary>
        public string OrganizationBaseUrl { get; internal set; }

        /// <summary>
        /// Gets the login of the SalesForce user who authorized this session.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the name of the SalesForce user who authorized this session.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the language name of the SalesForce user who authorized this session.
        /// </summary>
        public string UserLanguageName { get; private set; }

        /// <summary>
        /// Gets the locale name of the SalesForce user who authorized this session.
        /// </summary>
        public string UserLocaleName { get; private set; }

        /// <summary>
        /// Gets the SalesForce organization unique identifier.
        /// </summary>
        public string OrganizationId { get; private set; }

        /// <summary>
        /// Gets the format of the Partner API endpoint address.
        /// </summary>
        public string PartnerEndpointUrlFormat { get; private set; }

        #endregion

        #region "Constructors"

        internal Session(RestContract.Identity details)
        {
            UserId = details.UserId;
            UserName = details.UserName;
            UserLanguageName = details.UserLanguageName;
            UserLocaleName = details.UserLocaleName;
            OrganizationId = details.OrganizationId;
            PartnerEndpointUrlFormat = details.UrlFormats.Partner;
        }

        #endregion

    }

}