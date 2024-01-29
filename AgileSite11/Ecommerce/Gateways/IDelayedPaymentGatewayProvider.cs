using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface describing the external payment gateway supporting direct payments.
    /// </summary>
    public interface IDelayedPaymentGatewayProvider : IPaymentGatewayProvider
    {
        /// <summary>
        /// Indicates whether the payment is already authorized meaning that payment capture is possible.
        /// </summary>
        bool IsPaymentAuthorized
        {
            get;
        }


        /// <summary>
        /// Authorizes a payment.
        /// </summary>
        /// <param name="paymentData">Additional payment data.</param>
        /// <remarks>
        /// Returned object contains transaction identifier which needs to be persisted to allow further manipulation.
        /// </remarks>
        /// <returns>Instance of <see cref="PaymentResultInfo"/>.</returns>
        PaymentResultInfo AuthorizePayment(IDictionary<string, object> paymentData);


        /// <summary>
        /// Captures a payment.
        /// </summary>
        /// <remarks>
        /// Previously received transaction identifier is required to perform a capture of previously authorized transaction.
        /// </remarks>
        /// <returns>Instance of <see cref="PaymentResultInfo"/>.</returns>
        PaymentResultInfo CapturePayment();
    }
}