using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents an other payment application (e.g. a gift card)
    /// </summary>
    [Serializable]
    public abstract class OtherPaymentApplication
    {
        /// <summary>
        /// Other payment name.
        /// </summary>
        public string PaymentName
        {
            get;
            set;
        }


        /// <summary>
        /// Value of the other payment.
        /// </summary>
        public decimal PaymentValue
        {
            get;
            set;
        }
    }
}
