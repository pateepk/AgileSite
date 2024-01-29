using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Information about order.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class Order
    {
        /// <summary>
        /// Invoice number.
        /// </summary>
        [DataMember(Name = "invoiceNumber", Order = 0, EmitDefaultValue = false)]
        public string InvoiceNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Order description.
        /// </summary>
        [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
        public string Description
        {
            get;
            set;
        }
    }
}
