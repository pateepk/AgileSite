using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface describing the external payment gateway supporting direct payments.
    /// </summary>
    public interface IDirectPaymentGatewayProvider : IPaymentGatewayProvider
    {
        /// <summary>
        /// Processes the payment in external gateway directly.
        /// </summary>
        /// <param name="paymentData">Additional payment data.</param>
        /// <returns>
        /// Instance of <see cref="PaymentResultInfo"/>.
        /// </returns>
        /// <seealso cref="CMSPaymentGatewayProvider.CreatePaymentResultInfo"/>
        PaymentResultInfo ProcessPayment(IDictionary<string, object> paymentData);
    }
}