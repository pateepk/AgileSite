using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Response of payment transaction.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class TransactionResponse
    {
        /// <summary>
        /// Type of response.
        /// </summary>
        [DataMember(Name = "responseCode", Order = 0, IsRequired = true)]
        public TransactionResponseCode ResponseCode
        {
            get;
            set;
        }


        /// <summary>
        /// Authorization or approval code.
        /// </summary>
        [DataMember(Name = "authCode", Order = 1, IsRequired = true)]
        public string AuthCode
        {
            get;
            set;
        }
        

        /// <summary>
        /// ID of transaction assigned by gateway. Must be used in any follow-on transactions such as capture authorized payment.
        /// </summary>
        [DataMember(Name = "transId", Order = 2)]
        public string TransId
        {
            get;
            set;
        }

        
        /// <summary>
        /// Information about processed payment.
        /// </summary>
        [DataMember(Name = "messages", Order = 3)]
        public List<Message> MessagesMultiple
        {
            get;
            set;
        }


        /// <summary>
        /// List of errors occured during processing of payment.
        /// </summary>
        [DataMember(Name = "errors", Order = 4)]
        public List<Error> Errors
        {
            get;
            set;
        }
    }
}
