using System;

using CMS.Core;
using CMS.EventLog;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Default implementation of <see cref="IPaymentGatewayFormFactory"/>.
    /// </summary>
    public class PaymentGatewayFormFactory : IPaymentGatewayFormFactory
    {
        /// <summary>
        /// Returns instance of <see cref="CMSPaymentGatewayForm"/> based on given <paramref name="provider"/> or null when no form is bound to given provider.
        /// </summary>
        /// <param name="provider">Instance of <see cref="CMSPaymentGatewayProvider"/>.</param>
        /// <param name="loader">Instance of <see cref="IGatewayFormLoader"/> used to load form control.</param>
        public CMSPaymentGatewayForm GetPaymentGatewayForm(CMSPaymentGatewayProvider provider, IGatewayFormLoader loader)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (loader == null)
            {
                throw new ArgumentNullException(nameof(loader));
            }

            CMSPaymentGatewayForm form = null;

            var path = GetPath(provider);

            if (!string.IsNullOrEmpty(path))
            {
                form = loader.LoadFormControl(path);
            }

            if (form != null)
            {
                InitializeGatewayForm(provider, form);
            }
            else
            {
                LogMissingForm(provider, path);
            }

            return form;
        }


        /// <summary>
        /// Returns virtual path to form control based on given <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// Method should be overriden when using customized payment gateway providers.
        /// </remarks>
        protected virtual string GetPath(CMSPaymentGatewayProvider provider)
        {
            var path = "";

            if (provider is CMSCreditPaymentProvider)
            {
                path = "~/CMSModules/Ecommerce/Controls/PaymentGateways/CreditPaymentForm.ascx";
            }
            else if (provider is CMSAuthorizeNetProvider)
            {
                path = "~/CMSModules/Ecommerce/Controls/PaymentGateways/AuthorizeNetForm.ascx";
            }
            else if (provider is CMSPayPalProvider)
            {
                path = "~/CMSModules/Ecommerce/Controls/PaymentGateways/PayPalForm.ascx";
            }

            return path;
        }


        private static void InitializeGatewayForm(CMSPaymentGatewayProvider provider, CMSPaymentGatewayForm form)
        {
            form.PaymentProvider = provider;
            form.ID = "PaymentDataForm";
        }


        private static void LogMissingForm(CMSPaymentGatewayProvider provider, string path)
        {
            Service.Resolve<IEventLogService>().LogEvent(EventType.WARNING, "PaymentGatewayFormFactory", "Payment", $"Unable to load payment gateway form for provider: {provider.GetType().FullName} and path: {path}");
        }
    }
}
