using System.Runtime.Serialization;

namespace CMS.SalesForce.RestContract
{

    /// <summary>
    /// Represents SalesForce authentication and authorization REST API error.
    /// </summary>
    [DataContract]
    public sealed class AuthorizationError
    {

        #region "Public members"

        /// <summary>
        /// The error code.
        /// </summary>
        [DataMember(Name = "error")]
        public string Code;

        /// <summary>
        /// The error description.
        /// </summary>
        [DataMember(Name = "error_description")]
        public string Description;

        #endregion

    }

}