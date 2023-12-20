using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Envelop for payment request.
    /// </summary>
    [DataContract(Name = "createTransactionRequest", Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class CreateTransactionRequest
    {
        /// <summary>
        /// Authentication data for gateway.
        /// </summary>
        [DataMember(Name = "merchantAuthentication", Order = 0, IsRequired = true, EmitDefaultValue = false)]
        public MerchantAuthentication MerchantAuthentication
        {
            get;
            set;
        }


        /// <summary>
        /// Manually assigned request ID. Will be returned in respective response.
        /// </summary>
        [DataMember(Name = "refId", Order = 1, EmitDefaultValue = false)]
        public string RefId
        {
            get;
            set;
        }


        /// <summary>
        /// Payment request data.
        /// </summary>
        [DataMember(Name = "transactionRequest", Order = 2, IsRequired = true, EmitDefaultValue = false)]
        public TransactionRequest TransactionRequest
        {
            get;
            set;
        }
    }
}
