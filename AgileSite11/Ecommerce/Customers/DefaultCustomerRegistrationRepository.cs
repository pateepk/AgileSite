using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="ICustomerRegistrationRepository"/>.
    /// </summary>
    internal class DefaultCustomerRegistrationRepository : ICustomerRegistrationRepository
    {
        private const string REGISTER_AFTER_CHECKOUT_KEY = "customer_registered_checkout";
        private const string REGISTER_AFTER_CHECKOUT_TEMPLATE_KEY = "customer_registered_checkout_template";

        private readonly ISettingService mSettingService;


        /// <summary>
        /// Creates a new instance of <see cref="DefaultCustomerRegistrationRepository"/>.
        /// </summary>
        public DefaultCustomerRegistrationRepository(ISettingServiceFactory settingServiceFactory, int siteId)
        {
            if (settingServiceFactory == null)
            {
                throw new ArgumentNullException(nameof(settingServiceFactory));
            }

            mSettingService = settingServiceFactory.GetSettingService(siteId);
        }


        /// <summary>
        /// Indicates that customer will be registered after checkout.
        /// </summary>
        public bool IsCustomerRegisteredAfterCheckout
        {
            get
            {
                var value = ValidationHelper.GetBoolean(SessionHelper.GetValue(REGISTER_AFTER_CHECKOUT_KEY), false);
                if (!value)
                {
                    value = mSettingService.GetBooleanValue(ECommerceSettings.AUTOMATIC_REGISTRATION_CUSTOMER);
                }

                return value;
            }
            set
            {
                SessionHelper.SetValue(REGISTER_AFTER_CHECKOUT_KEY, value);
            }
        }


        /// <summary>
        /// E-mail template code name used for registration after checkout.
        /// </summary>
        public string RegisteredAfterCheckoutTemplate
        {
            get
            {
                var value = ValidationHelper.GetString(SessionHelper.GetValue(REGISTER_AFTER_CHECKOUT_TEMPLATE_KEY), string.Empty);
                if (string.IsNullOrEmpty(value))
                {
                    value = mSettingService.GetStringValue(ECommerceSettings.AUTOMATIC_REGISTRATION_EMAIL_TEMPLATE);
                }

                return value;
            }
            set
            {
                SessionHelper.SetValue(REGISTER_AFTER_CHECKOUT_TEMPLATE_KEY, value);
            }
        }


        /// <summary>
        /// Clears repository values.
        /// </summary>
        public void Clear()
        {
            SessionHelper.Remove(REGISTER_AFTER_CHECKOUT_KEY);
            SessionHelper.Remove(REGISTER_AFTER_CHECKOUT_TEMPLATE_KEY);
        }
    }
}