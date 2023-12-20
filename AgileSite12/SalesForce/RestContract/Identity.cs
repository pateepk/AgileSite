using System.Runtime.Serialization;

namespace CMS.SalesForce.RestContract
{

    /// <summary>
    /// Represents SalesForce organization access details.
    /// </summary>
    [DataContract]
    public sealed class Identity
    {

        #region "Public members"

        /// <summary>
        /// The identifier of the SalesForce user.
        /// </summary>
        [DataMember(Name="user_id")]
        public string UserId;

        /// <summary>
        /// The name of the SalesForce user.
        /// </summary>
        [DataMember(Name = "display_name")]
        public string UserName;

        /// <summary>
        /// The UI language name of the SalesForce user.
        /// </summary>
        [DataMember(Name = "language")]
        public string UserLanguageName;

        /// <summary>
        /// The locale name of the SalesForce user.
        /// </summary>
        [DataMember(Name = "locale")]
        public string UserLocaleName;

        /// <summary>
        /// The identifier of the SalesForce organization.
        /// </summary>
        [DataMember(Name = "organization_id")]
        public string OrganizationId;

        /// <summary>
        /// URL formats of SalesForce API entry points.
        /// </summary>
        [DataMember(Name = "urls")]
        public UrlFormats UrlFormats;

        #endregion

    }

}