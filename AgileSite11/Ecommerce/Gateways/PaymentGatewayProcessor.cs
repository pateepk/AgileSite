using System;
using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="IPaymentGatewayProcessor"/>.
    /// </summary>
    public class PaymentGatewayProcessor : IPaymentGatewayProcessor
    {
        /// <summary>
        /// Process payment using given <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">
        ///     <see cref="IPaymentGatewayProvider"/> implementation.
        /// </param>
        /// <param name="paymentData">
        /// Additional payment data.
        /// </param>
        /// <returns>
        /// Instance of <see cref="PaymentResultInfo"/> provided by given provider.
        /// </returns>
        /// <remarks>
        /// Implementation tries to use authorize and capture payment method base on given <paramref name="provider"/>.
        /// If provider implements <see cref="IDelayedPaymentGatewayProvider"/> and allows to delayed capture <see cref="IPaymentGatewayProvider.UseDelayedPayment"/> only capture and returned <see cref="PaymentResultInfo"/> contains transaction identifier.
        /// If provider implements <see cref="IDirectPaymentGatewayProvider"/> payment is done right-away.
        /// If neither <see cref="IDelayedPaymentGatewayProvider"/> or <see cref="IDirectPaymentGatewayProvider"/> is implemented, null value is returned.
        /// </remarks>
        public PaymentResultInfo ProcessPayment(IPaymentGatewayProvider provider, IDictionary<string, object> paymentData)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return HandleDelayedPayment(provider, paymentData) ?? HandleDirectPayment(provider, paymentData);
        }


        private static PaymentResultInfo HandleDelayedPayment(IPaymentGatewayProvider provider, IDictionary<string, object> paymentData)
        {
            if (!provider.UseDelayedPayment())
            {
                return null;
            }

            var delayedProvider = provider as IDelayedPaymentGatewayProvider;
            return delayedProvider?.AuthorizePayment(paymentData);
        }


        private static PaymentResultInfo HandleDirectPayment(IPaymentGatewayProvider provider, IDictionary<string, object> paymentData)
        {
            var directProvider = provider as IDirectPaymentGatewayProvider;

            return directProvider?.ProcessPayment(paymentData);
        }
    }
}
