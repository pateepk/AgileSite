using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Information about response.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class MainMessages
    {
        /// <summary>
        /// Whether response contains error.
        /// </summary>
        [DataMember(Name = "resultCode", Order = 0, IsRequired = true)]
        public MainMessagesResultCode ResultCode
        {
            get;
            set;
        }


        /// <summary>
        /// Text representation.
        /// </summary>
        [DataMember(Name = "message", Order = 1, IsRequired = true)]
        public MainMessage MainMessage
        {
            get;
            set;
        }
    }
}
