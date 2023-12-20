using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Authentication data for gateway.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class MerchantAuthentication
    {
        /// <summary>
        /// API login ID.
        /// </summary>
        [DataMember(Name = "name", Order = 0, IsRequired = true, EmitDefaultValue = false)]
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Transaction key.
        /// </summary>
        [DataMember(Name = "transactionKey", Order = 1, IsRequired = true, EmitDefaultValue = false)]
        public string TransactionKey
        {
            get;
            set;
        }
    }
}
