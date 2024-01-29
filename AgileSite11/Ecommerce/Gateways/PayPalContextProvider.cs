using System;
using System.Collections.Generic;

using PayPal.Api;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="IPayPalContextProvider"/>.
    /// </summary>
    internal class PayPalContextProvider : IPayPalContextProvider
    {
        private const string CLIENTID_SETTING_NAME = "CMSPayPalCredentialsClientId";
        private const string CLIENTSECRET_SETTING_NAME = "CMSPayPalCredentialsClientSecret";
        private const string ACCOUNTMODE_SETTING_NAME = "CMSPayPalCredentialsAccountType";

        private readonly ISettingServiceFactory mSettingServiceFactory;


        /// <summary>
        /// Creates a new instance of <see cref="PayPalContextProvider"/>.
        /// </summary>
        /// <param name="settingServiceFactory">Factory used to get instances of setting services.</param>
        public PayPalContextProvider(ISettingServiceFactory settingServiceFactory)
        {
            if (settingServiceFactory == null)
            {
                throw new ArgumentNullException(nameof(settingServiceFactory));
            }

            mSettingServiceFactory = settingServiceFactory;
        }


        /// <summary>
        /// Returns complete <see cref="APIContext"/> used in PayPal SDK for payment processing.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        public APIContext GetApiContext(int siteId)
        {
            var settingService = mSettingServiceFactory.GetSettingService(siteId);

            var payPalConfig = new Dictionary<string, string>();
            payPalConfig.Add("clientId", settingService.GetStringValue(CLIENTID_SETTING_NAME));
            payPalConfig.Add("clientSecret", settingService.GetStringValue(CLIENTSECRET_SETTING_NAME));
            payPalConfig.Add("mode", settingService.GetStringValue(ACCOUNTMODE_SETTING_NAME));

            var accessToken = new OAuthTokenCredential(payPalConfig).GetAccessToken();
            var apiContext = new APIContext(accessToken)
            {
                Config = ConfigManager.GetConfigWithDefaults(payPalConfig)
            };

            return apiContext;
        }
    }
}