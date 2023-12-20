using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing PaymentOptionInfo management.
    /// </summary>
    public class PaymentOptionInfoProvider : AbstractInfoProvider<PaymentOptionInfo, PaymentOptionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public PaymentOptionInfoProvider()
            : base(PaymentOptionInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all payment options.
        /// </summary>
        public static ObjectQuery<PaymentOptionInfo> GetPaymentOptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns payment option with specified ID.
        /// </summary>
        /// <param name="optionId">Payment option ID</param>
        public static PaymentOptionInfo GetPaymentOptionInfo(int optionId)
        {
            return ProviderObject.GetInfoById(optionId);
        }


        /// <summary>
        /// Returns payment option with specified name.
        /// </summary>
        /// <param name="optionName">Payment option name</param>
        /// <param name="siteName">Site name</param>
        public static PaymentOptionInfo GetPaymentOptionInfo(string optionName, string siteName)
        {
            return ProviderObject.GetPaymentOptionInfoInternal(optionName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified payment option.
        /// </summary>
        /// <param name="optionObj">Payment option to be set</param>
        public static void SetPaymentOptionInfo(PaymentOptionInfo optionObj)
        {
            ProviderObject.SetInfo(optionObj);
        }


        /// <summary>
        /// Deletes specified payment option.
        /// </summary>
        /// <param name="optionObj">Payment option to be deleted</param>
        public static void DeletePaymentOptionInfo(PaymentOptionInfo optionObj)
        {
            ProviderObject.DeleteInfo(optionObj);
        }


        /// <summary>
        /// Deletes payment option with specified ID.
        /// </summary>
        /// <param name="optionId">Payment option ID</param>
        public static void DeletePaymentOptionInfo(int optionId)
        {
            var optionObj = GetPaymentOptionInfo(optionId);
            DeletePaymentOptionInfo(optionObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns query for of all payment options matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the options should be retrieved from. If set to 0, global payment options are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled payment options from the specified site are returned. False - all site options are returned</param>
        public static ObjectQuery<PaymentOptionInfo> GetPaymentOptions(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetPaymentOptionsInternal(siteId, onlyEnabled);
        }


        /// <summary>
        /// Returns URL to payment gateway.
        /// </summary>
        /// <param name="cart">Shopping cart object</param>
        public static string GetPaymentURL(ShoppingCartInfo cart)
        {
            return ProviderObject.GetPaymentURLInternal(cart);
        }


        /// <summary>
        /// Resolves macros of payment gateway URL.
        /// </summary>
        /// <param name="url">URL to resolve</param>
        /// <param name="cart">Cart object</param>
        public static string ResolveUrlMacros(string url, ShoppingCartInfo cart)
        {
            return ProviderObject.ResolveUrlMacrosInternal(url, cart);
        }


        /// <summary>
        /// Checks if payment option is applicable for given shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart in which payment option should be used.</param>
        /// <param name="paymentOption">Payment option to check if it is applicable.</param>
        /// <returns>True if payment option exists, is enabled and is allowed.</returns>
        public static bool IsPaymentOptionApplicable(ShoppingCartInfo cart, PaymentOptionInfo paymentOption)
        {
            return ProviderObject.IsPaymentOptionApplicableInternal(cart, paymentOption);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns payment option with specified name.
        /// </summary>
        /// <param name="optionName">Payment option name</param>
        /// <param name="siteName">Site name</param>
        protected virtual PaymentOptionInfo GetPaymentOptionInfoInternal(string optionName, string siteName)
        {
            // Search for global payment options if site department not found
            bool searchGlobal = ECommerceSettings.AllowGlobalPaymentMethods(siteName);

            return GetInfoByCodeName(optionName, SiteInfoProvider.GetSiteID(siteName), true, searchGlobal);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns query for all payment options matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the options should be retrieved from. If set to 0, global payment options are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled payment options from the specified site are returned. False - all site options are returned</param>
        protected virtual ObjectQuery<PaymentOptionInfo> GetPaymentOptionsInternal(int siteId, bool onlyEnabled)
        {
            // Check if site uses site or global payment options
            var includeGlobal = ECommerceSettings.AllowGlobalPaymentMethods(siteId);

            // Get payment options on requested site
            var query = GetPaymentOptions().OnSite(siteId, includeGlobal);

            if (onlyEnabled)
            {
                query.WhereTrue("PaymentOptionEnabled");
            }

            return query;
        }


        /// <summary>
        /// Returns URL to payment gateway.
        /// </summary>
        /// <param name="cart">Shopping cart object</param>
        protected virtual string GetPaymentURLInternal(ShoppingCartInfo cart)
        {
            var payment = cart?.PaymentOption;
            if (payment != null)
            {
                // Prepare the payment URL
                return ResolveUrlMacros(payment.PaymentOptionPaymentGateUrl, cart);
            }

            return "";
        }


        /// <summary>
        /// Resolves macros of payment gateway URL.
        /// </summary>
        /// <param name="url">URL to resolve</param>
        /// <param name="cart">Cart object</param>
        protected virtual string ResolveUrlMacrosInternal(string url, ShoppingCartInfo cart)
        {
            if (string.IsNullOrEmpty(url) || (cart == null))
            {
                return "";
            }

            // Create macro resolver
            MacroResolver resolver = ShoppingCartInfoProvider.GetShoppingCartResolver(cart);

            // Backward compatibility (old macros)
            resolver.SetNamedSourceData("amount", cart.GrandTotal, false);
            resolver.SetNamedSourceData("currency", cart.Currency.CurrencyCode, false);

            // Resolve macros
            url = resolver.ResolveMacros(url);

            return url;
        }


        /// <summary>
        /// Checks if payment option is applicable for given shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart in which payment option should be used.</param>
        /// <param name="paymentOption">Payment option to check if it is applicable.</param>
        /// <returns>True if payment option exists, is enabled and is allowed.</returns>
        protected virtual bool IsPaymentOptionApplicableInternal(ShoppingCartInfo cart, PaymentOptionInfo paymentOption)
        {
            if (paymentOption != null && paymentOption.PaymentOptionEnabled)
            {
                if (!cart.IsShippingNeeded)
                {
                    return paymentOption.PaymentOptionAllowIfNoShipping;
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}