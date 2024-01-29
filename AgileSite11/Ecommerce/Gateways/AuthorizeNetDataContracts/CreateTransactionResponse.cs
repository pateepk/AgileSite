using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Envelope for payment response.
    /// </summary>
    [DataContract(Name = "createTransactionResponse", Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class CreateTransactionResponse
    {
        /// <summary>
        /// ID of respective request, set in <see cref="CreateTransactionRequest.RefId"/>.
        /// </summary>
        [DataMember(Name = "refId", Order = 0, IsRequired = true)]
        public string RefId
        {
            get;
            set;
        }


        /// <summary>
        /// Information about transaction result.
        /// </summary>
        [DataMember(Name = "messages", Order = 1, IsRequired = true)]
        public MainMessages MainMessages
        {
            get;
            set;
        }


        /// <summary>
        /// Transaction response data.
        /// </summary>
        [DataMember(Name = "transactionResponse", Order = 2)]
        public TransactionResponse TransactionResponse
        {
            get;
            set;
        }
    }
}
