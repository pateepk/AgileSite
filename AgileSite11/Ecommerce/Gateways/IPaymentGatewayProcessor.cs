using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IPaymentGatewayProcessor), typeof(PaymentGatewayProcessor), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Main entry point for processing payment on any payment gateways.
    /// </summary>
    public interface IPaymentGatewayProcessor
    {
        /// <summary>
        /// Process payment using given <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider"><see cref="IPaymentGatewayProvider"/> implementation.</param>
        /// <param name="paymentData">Additional payment data.</param>
        /// <returns>
        /// Result provided by given provider.
        /// </returns>
        PaymentResultInfo ProcessPayment(IPaymentGatewayProvider provider, IDictionary<string, object> paymentData);
    }
}