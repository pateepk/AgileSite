using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Information about part of payment.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class PartialAmount
    {
        /// <summary>
        /// Amount of partial payment.
        /// </summary>
        [DataMember(Name = "amount", Order = 0, EmitDefaultValue = false)]
        public string Amount
        {
            get;
            set;
        }


        /// <summary>
        /// Name of partial payment.
        /// </summary>
        [DataMember(Name = "name", Order = 1, EmitDefaultValue = false)]
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Description of partial payment.
        /// </summary>
        [DataMember(Name = "description", Order = 2, EmitDefaultValue = false)]
        public string Description
        {
            get;
            set;
        }
    }
}
