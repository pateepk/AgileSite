using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Credit card data.
    /// </summary>
    [DataContract(Name = "creditCard", Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class CreditCard
    {
        /// <summary>
        /// Card number.
        /// </summary>
        [DataMember(Name = "cardNumber", Order = 0, IsRequired = true, EmitDefaultValue = false)]
        public string CardNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Card expiration.
        /// </summary>
        [DataMember(Name = "expirationDate", Order = 1, IsRequired = true, EmitDefaultValue = false)]
        public string ExpirationDate
        {
            get;
            set;
        }


        /// <summary>
        /// CVV or CVC code.
        /// </summary>
        [DataMember(Name = "cardCode", Order = 2, EmitDefaultValue = false)]
        public string CardCode
        {
            get;
            set;
        }
    }
}
