using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represent customer preferences. 
    /// </summary>
    [DebuggerDisplay("CurrencyID: {CurrencyID} ShippingID: {ShippingOptionID} PaymentID: {PaymentOptionID}")]
    public class CustomerPreferences
    {
        /// <summary>
        /// Predefined unknown customer preferences.
        /// </summary>
        public static readonly CustomerPreferences Unknown = new CustomerPreferences(null, null, null);


        /// <summary>
        /// ID of the preferred currency. <c>null</c> when preference is unknown.
        /// </summary>
        public int? CurrencyID
        {
            get;
        }


        /// <summary>
        /// ID of the preferred shipping option. <c>null</c> when preference is unknown.
        /// </summary>
        public int? ShippingOptionID
        {
            get;
        }


        /// <summary>
        /// ID of the preferred payment option. <c>null</c> when preference is unknown.
        /// </summary>
        public int? PaymentOptionID
        {
            get;
        }


        /// <summary>
        /// Creates a new instance of <see cref="CustomerPreferences"/> using given preferred options.
        /// </summary>
        /// <param name="currencyID">The identifier of the preferred currency.</param>
        /// <param name="shippingOptionID">The identifier of the preferred shipping option.</param>
        /// <param name="paymentOptionID">The identifier of the preferred payment option.</param>
        public CustomerPreferences(int? currencyID, int? shippingOptionID, int? paymentOptionID)
        {
            CurrencyID = currencyID;
            ShippingOptionID = shippingOptionID;
            PaymentOptionID = paymentOptionID;
        }
    }
}
