using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Payment request data.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class TransactionRequest
    {
        /// <summary>
        /// Type of request.
        /// </summary>
        [DataMember(Name = "transactionType", Order = 0, EmitDefaultValue = false)]
        public TransactionType TransactionType
        {
            get;
            set;
        }


        /// <summary>
        /// Grand total price for order including tax and shipping.
        /// </summary>
        [DataMember(Name = "amount", Order = 1, IsRequired = true, EmitDefaultValue = false)]
        public string Amount
        {
            get;
            set;
        }


        /// <summary>
        /// Data about payment - credit card data.
        /// </summary>
        [DataMember(Name = "payment", Order = 2, EmitDefaultValue = false)]
        public Payment Payment
        {
            get;
            set;
        }


        /// <summary>
        /// Transaction ID of previous transaction. In case of capture request - ID of authorize transaction.
        /// </summary>
        [DataMember(Name = "refTransId", Order = 3, EmitDefaultValue = false)]
        public string RefTransId
        {
            get;
            set;
        }


        /// <summary>
        /// Data about order.
        /// </summary>
        [DataMember(Name = "order", Order = 4, EmitDefaultValue = false)]
        public Order Order
        {
            get;
            set;
        }


        /// <summary>
        /// Information about tax.
        /// </summary>
        [DataMember(Name = "tax", Order = 5, EmitDefaultValue = false)]
        public PartialAmount Tax
        {
            get;
            set;
        }


        /// <summary>
        /// Information about shipping.
        /// </summary>
        [DataMember(Name = "shipping", Order = 6, EmitDefaultValue = false)]
        public PartialAmount Shipping
        {
            get;
            set;
        }


        /// <summary>
        /// Manually assigned purchase order number.
        /// </summary>
        [DataMember(Name = "poNumber", Order = 7, EmitDefaultValue = false)]
        public string PONumber
        {
            get;
            set;
        }


        /// <summary>
        /// Data about customer.
        /// </summary>
        [DataMember(Name = "customer", Order = 8, EmitDefaultValue = false)]
        public Customer Customer
        {
            get;
            set;
        }


        /// <summary>
        /// Billing address.
        /// </summary>
        [DataMember(Name = "billTo", Order = 9, EmitDefaultValue = false)]
        public Address BillTo
        {
            get;
            set;
        }


        /// <summary>
        /// Shipping address.
        /// </summary>
        [DataMember(Name = "shipTo", Order = 10, EmitDefaultValue = false)]
        public Address ShipTo
        {
            get;
            set;
        }
    }
}
