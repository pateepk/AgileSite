using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Information about payment.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class Payment
    {
        /// <summary>
        /// Credit card data.
        /// </summary>
        [DataMember(Name = "creditCard", Order = 0, EmitDefaultValue = false)]
        public CreditCard CreditCard
        {
            get;
            set;
        }
    }
}
