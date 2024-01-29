using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents an application of the gift card.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    [Serializable]
    public class GiftCardApplication : OtherPaymentApplication, ICouponCodeApplication
    {
        /// <summary>
        /// Gift card that the used code belonged to.
        /// </summary>
        public GiftCardInfo GiftCard
        {
            get;
            set;
        }


        /// <summary>
        /// Used gift card code.
        /// </summary>
        public string AppliedCode
        {
            get;
            set;
        }


        /// <summary>
        /// Value of the gift card correction in main currency.
        /// </summary>
        /// <remarks>
        /// If correction is positive, the coupon code remaining value will decrease upon order update.
        /// If correction is negative, the coupon code remaining value will increase upon order update.
        /// </remarks>
        [XmlIgnore]
        public decimal PaymentCorrectionInMainCurrency
        {
            get;
            set;
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{PaymentName}[\"{AppliedCode}\"]: {PaymentValue}";


        /// <summary>
        /// Value of the other payment in main currency.
        /// </summary>
        [XmlIgnore]
        public decimal PaymentValueInMainCurrency
        {
            get;
            set;
        }


        /// <summary>
        /// Logs gift card usage.
        /// </summary>
        public void Apply()
        {
            var valueToLog = PaymentCorrectionInMainCurrency != 0 ? PaymentCorrectionInMainCurrency : PaymentValueInMainCurrency;
            GiftCard?.LogUseOnce(AppliedCode, valueToLog);
        }
    }
}
