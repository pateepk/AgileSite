using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Envelope for error response.
    /// </summary>
    [DataContract(Name = "ErrorResponse", Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class ErrorResponse
    {
        /// <summary>
        /// Information about error.
        /// </summary>
        [DataMember(Name = "messages", IsRequired = true)]
        public MainMessages MainMessages
        {
            get;
            set;
        }
    }
}
