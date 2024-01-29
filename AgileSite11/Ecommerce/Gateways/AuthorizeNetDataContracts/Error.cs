using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Information about error.
    /// </summary>
    [DataContract(Name = "error", Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class Error
    {
        /// <summary>
        /// Error code.
        /// </summary>
        [DataMember(Name = "errorCode", EmitDefaultValue = false)]
        public string ErrorCode
        {
            get;
            set;
        }


        /// <summary>
        /// Error text.
        /// </summary>
        [DataMember(Name = "errorText", EmitDefaultValue = false)]
        public string ErrorText
        {
            get;
            set;
        }
    }
}