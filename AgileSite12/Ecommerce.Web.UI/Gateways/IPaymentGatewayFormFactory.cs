using CMS;
using CMS.Ecommerce.Web.UI;

[assembly: RegisterImplementation(typeof(IPaymentGatewayFormFactory), typeof(PaymentGatewayFormFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Defines factory for providing payment gateways form controls.
    /// </summary>
    public interface IPaymentGatewayFormFactory
    {
        /// <summary>
        /// Returns instance of <see cref="CMSPaymentGatewayForm"/> based on given <paramref name="provider"/> or null when no form is bound to given provider.
        /// </summary>
        /// <param name="provider">Instance of <see cref="CMSPaymentGatewayProvider"/>.</param>
        /// <param name="loader">Instance of <see cref="IGatewayFormLoader"/> used to load form control.</param>
        CMSPaymentGatewayForm GetPaymentGatewayForm(CMSPaymentGatewayProvider provider, IGatewayFormLoader loader);
    }
}