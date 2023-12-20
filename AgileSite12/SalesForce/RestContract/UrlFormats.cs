using System.Runtime.Serialization;

namespace CMS.SalesForce.RestContract
{

    /// <summary>
    /// Represents URL formats of SalesForce API entry points.
    /// </summary>
    [DataContract]
    public sealed class UrlFormats
    {

        #region "Public members"

        /// <summary>
        /// Partner integration API.
        /// </summary>
        [DataMember(Name="partner")]
        public string Partner;
        
        /// <summary>
        /// REST API.
        /// </summary>
        [DataMember(Name = "rest")]
        public string Rest;

        #endregion

    }

}