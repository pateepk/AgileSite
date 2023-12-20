using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Information about customer.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class Customer
    {
        /// <summary>
        /// Customer ID.
        /// </summary>
        [DataMember(Name = "id", Order = 0, EmitDefaultValue = false)]
        public string Id
        {
            get;
            set;
        }


        /// <summary>
        /// Customer email.
        /// </summary>
        [DataMember(Name = "email", Order = 1, EmitDefaultValue = false)]
        public string Email
        {
            get;
            set;
        }
    }
}
