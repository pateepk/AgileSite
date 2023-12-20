using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Custom E-commerce helper.
    /// </summary>
    public static class ECommerceHelper
    {
        #region "Properties"

        /// <summary>
        /// Gets the comparer for coupon codes. Provides case-insensitive ordinal string comparison.
        /// </summary>
        public static IEqualityComparer<string> CouponCodeComparer => StringComparer.OrdinalIgnoreCase;

        #endregion


        #region "Security methods"

        /// <summary>
        /// Checks whether the user is authorized to modify site/global e-commerce configuration.
        /// </summary>
        /// <param name="global">True - permission 'ConfigurationGlobalModify' is checked, False - permission 'ConfigurationGlobalModify' is checked</param>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedToModifyConfiguration(bool global, string siteName, IUserInfo user, bool exceptionOnFailure)
        {
            // Site or global configuration
            string permission = global ? EcommercePermissions.CONFIGURATION_MODIFYGLOBAL : EcommercePermissions.CONFIGURATION_MODIFY;

            return user.IsAuthorizedPerResource(ModuleName.ECOMMERCE, permission, siteName, exceptionOnFailure);
        }


        /// <summary>
        /// Checks the specified ecommerce permission for the given user.
        /// </summary>
        /// <param name="permissionName">Permission name to be checked</param>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        public static bool IsUserAuthorizedForPermission(string permissionName, string siteName, IUserInfo user)
        {
            return IsUserAuthorizedForPermission(permissionName, siteName, user, false);
        }


        /// <summary>
        /// Checks the specified ecommerce permission for the given user.
        /// </summary>
        /// <param name="permissionName">Permission name to be checked</param>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedForPermission(string permissionName, string siteName, IUserInfo user, bool exceptionOnFailure)
        {
            if (String.IsNullOrEmpty(permissionName))
            {
                return false;
            }

            // If no user info object given, not authorized
            if (user == null)
            {
                return false;
            }

            // Check the priority permissions
            if (permissionName.StartsWith("modify", StringComparison.OrdinalIgnoreCase))
            {
                if (IsUserAuthorizedPerEcommerce(EcommercePermissions.ECOMMERCE_MODIFY, siteName, user, false))
                {
                    return true;
                }
            }
            else if (permissionName.StartsWith("read", StringComparison.OrdinalIgnoreCase))
            {
                if (IsUserAuthorizedPerEcommerce(EcommercePermissions.ECOMMERCE_READ, siteName, user, false))
                {
                    return true;
                }
            }

            return IsUserAuthorizedPerEcommerce(permissionName, siteName, user, exceptionOnFailure);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns 0 if specified site uses global objects based on the specified e-commerce setting, otherwise returns site ID of the specified site.
        /// </summary>
        /// <param name="siteIdentifier">Site identifier (site ID or site name)</param>
        /// <param name="keyName">One of the e-commerce settings keys which determine if specific global objects are used</param>
        public static int GetSiteID(SiteInfoIdentifier siteIdentifier, string keyName)
        {
            if ((siteIdentifier == null) || (siteIdentifier.ObjectID <= 0))
            {
                return 0;
            }

            return SettingsKeyInfoProvider.GetBoolValue(keyName, siteIdentifier) ? 0 : siteIdentifier.ObjectID;
        }


        /// <summary>
        /// Returns data query with all existing coupon codes from all existing discounts.
        /// </summary>
        /// <param name="site">Site identifier.</param>
        public static DataQuery GetAllCouponCodesQuery(SiteInfoIdentifier site)
        {
            if (site == null)
            {
                return null;
            }

            IDataQuery couponCodes = CouponCodeInfoProvider.GetCouponCodes(site)
                                                           .Column("CouponCodeCode");
            IDataQuery multiBuyCouponCodes = MultiBuyCouponCodeInfoProvider.GetMultiBuyCouponCodes(site)
                                                                     .Column(new QueryColumn("MultiBuyCouponCodeCode").As("CouponCodeCode"));
            IDataQuery giftCardCouponCodes = GiftCardCouponCodeInfoProvider.GetGiftCardCouponCodes(site)
                                                                     .Column(new QueryColumn("GiftCardCouponCodeCode").As("CouponCodeCode"));

            return DataQuery.Combine(new[]
            {
                couponCodes,
                multiBuyCouponCodes,
                giftCardCouponCodes
            }, new[]
            {
                SqlOperator.UNION
            });
        }


        /// <summary>
        /// Returns formatted value with removed trailing zero values and added trailing percentage character.
        /// </summary>
        /// <param name="value">Decimal number value.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        public static string GetFormattedPercentageValue(decimal value, IFormatProvider provider)
        {
            var valueWithoutZeros = value.TrimEnd().ToString(provider);
            return $"{valueWithoutZeros} %";
        }

        #endregion


        #region "Conversion tracking methods"

        /// <summary>
        /// Tracks order conversion.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="name">Order conversion name. If it is set, it is used instead of the 'Order' conversion name from the settings.</param>
        public static void TrackOrderConversion(ShoppingCartInfo cart, string name = null)
        {
            // Do not process
            if (cart == null)
            {
                return;
            }

            if (IsSiteContentOnly(cart.ShoppingCartSiteID))
            {
                return;
            }

            // Prepare resolver
            var resolver = ShoppingCartInfoProvider.GetShoppingCartResolver(cart);

            // Get conversion data
            var siteName = cart.SiteName;
            if (string.IsNullOrEmpty(name))
            {
                name = ECommerceSettings.OrderConversionName(siteName);
            }
            string value = ECommerceSettings.OrderConversionValue(siteName);

            TrackConversion(name, ValidationHelper.GetDouble(resolver.ResolveMacros(value), 0), 1, siteName);
        }


        /// <summary>
        /// Tracks registration conversion.
        /// </summary>
        /// <param name="siteName">Name of the site the registration should be tracked for.</param>
        /// <param name="name">Registration conversion name. If it is set, it is used instead of the 'Registration' conversion name from the settings.</param>
        public static void TrackRegistrationConversion(string siteName, string name = null)
        {
            // Do not process
            if (string.IsNullOrEmpty(siteName))
            {
                return;
            }

            // Prepare resolver
            MacroResolver mr = MacroContext.CurrentResolver.CreateChild();

            // Get conversion data
            if (string.IsNullOrEmpty(name))
            {
                name = ECommerceSettings.RegistrationConversionName(siteName);
            }
            string value = ECommerceSettings.RegistrationConversionValue(siteName);

            // Track conversion
            TrackConversion(name, ValidationHelper.GetDouble(mr.ResolveMacros(value), 0), 1, siteName);
        }


        /// <summary>
        /// Tracks add to shopping cart conversion.
        /// </summary>
        /// <param name="product">Product for which the adding to the shopping cart should be tracked.</param>
        /// <param name="name">Add to shopping cart conversion name. If it is set, it is used instead of the 'Add to shopping cart' conversion name from the settings.</param>
        public static void TrackAddToShoppingCartConversion(ShoppingCartItemInfo product, string name = null)
        {
            // Do not process
            var shoppingCart = product?.ShoppingCart;
            if (shoppingCart == null)
            {
                return;
            }

            if (IsSiteContentOnly(shoppingCart.ShoppingCartSiteID))
            {
                return;
            }

            var siteName = shoppingCart.SiteName;

            if (string.IsNullOrEmpty(name))
            {
                name = ECommerceSettings.AddToCartConversionName(siteName);
            }

            var macroResolver = ShoppingCartItemInfoProvider.GetShoppingCartItemResolver(product);
            var value = ECommerceSettings.AddToCartConversionValue(siteName);

            TrackConversion(name, ValidationHelper.GetDouble(macroResolver.ResolveMacros(value), 0), 1, siteName);
        }


        /// <summary>
        /// Tracks conversions of all order items
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        public static void TrackOrderItemsConversions(ShoppingCartInfo cart)
        {
            // Do not process
            if (cart == null)
            {
                return;
            }

            if (IsSiteContentOnly(cart.ShoppingCartSiteID))
            {
                return;
            }

            // Get site name
            string siteName = cart.SiteName;

            // Loop through all shopping cart items
            foreach (ShoppingCartItemInfo item in cart.CartItems)
            {
                // In case item is a variant and has parent product
                string variantParentConversionName = "";

                if (item?.SKU != null)
                {
                    // Get conversion name of item
                    string itemConversionName = item.SKU.SKUConversionName;

                    if (item.VariantParent != null)
                    {
                        variantParentConversionName = item.VariantParent.SKUConversionName;
                    }

                    if (!string.IsNullOrEmpty(itemConversionName) || (!string.IsNullOrEmpty(variantParentConversionName)))
                    {
                        // Prepare resolver
                        MacroResolver mr = ShoppingCartItemInfoProvider.GetShoppingCartItemResolver(item);

                        // Log order conversion for item
                        if (!string.IsNullOrEmpty(itemConversionName))
                        {
                            // Track item conversion
                            double value = ValidationHelper.GetDouble(mr.ResolveMacros(item.SKU.SKUConversionValue), 0);
                            TrackConversion(itemConversionName, value * item.CartItemUnits, item.CartItemUnits, siteName);
                        }

                        // Log order conversion for variant parent
                        if (!string.IsNullOrEmpty(variantParentConversionName))
                        {
                            // In case the item is a variant try to track its parent conversion
                            TrackVariantParentConversion(item, itemConversionName, variantParentConversionName, siteName, mr);
                        }
                    }
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks whether the user is authorized for given permission, returns true if so.
        /// </summary>
        /// <param name="permissionName">Permission name to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        private static bool IsUserAuthorizedPerEcommerce(string permissionName, string siteName, IUserInfo user, bool exceptionOnFailure)
        {
            if (user == null)
            {
                return false;
            }

            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsUserAuthorizedPerEcommerce");

            bool result = user.IsAuthorizedPerResource(ModuleName.ECOMMERCE, permissionName, siteName, false);

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, user.UserName, ModuleName.ECOMMERCE, permissionName, result, siteName);
            }

            if (exceptionOnFailure && !result)
            {
                throw new PermissionCheckException(ModuleName.ECOMMERCE, permissionName, siteName);
            }

            return result;
        }


        /// <summary>
        /// Logs conversion with the given name and value.
        /// </summary>
        /// <param name="objectName">Conversion name</param>
        /// <param name="value">Conversion value</param>
        /// <param name="count">Count of conversions to log</param>
        /// <param name="siteName">Name of the site the conversion should be logged for</param>
        internal static void TrackConversion(string objectName, double value, int count, string siteName)
        {
            // Do not process
            if (String.IsNullOrEmpty(objectName) || String.IsNullOrEmpty(siteName))
            {
                return;
            }

            if (!AnalyticsHelper.AnalyticsEnabled(siteName) || !Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            // Get current IP address
            string ip = "";
            if (CMSHttpContext.Current?.Request != null)
            {
                ip = RequestContext.UserHostAddress;
            }

            // Track conversion
            if (!AnalyticsHelper.IsIPExcluded(siteName, ip))
            {
                HitLogProvider.LogConversions(siteName, LocalizationContext.PreferredCultureCode, objectName, 0, count, value);
            }
        }


        /// <summary>
        /// Returns if site specified by <paramref name="siteId"/> is content only.
        /// </summary>
        private static bool IsSiteContentOnly(int siteId)
        {
            return SiteInfoProvider.GetSiteInfo(siteId).SiteIsContentOnly;
        }


        /// <summary>
        /// Tracks conversion of variant parent.
        /// </summary>
        /// <param name="item">Shopping cart item for which parent conversion should be tracked</param>
        /// <param name="itemConversionName">Conversion name of shopping cart item</param>
        /// <param name="variantParentConversionName">Conversion name of variants product</param>
        /// <param name="siteName">Shopping cart site name</param>
        /// <param name="mr">Macro resolver</param>
        private static void TrackVariantParentConversion(ShoppingCartItemInfo item, string itemConversionName, string variantParentConversionName, string siteName, MacroResolver mr)
        {
            // Conversion name of variants parent is different from item conversion name
            if (itemConversionName != variantParentConversionName)
            {
                double conversionValue = ValidationHelper.GetDouble(mr.ResolveMacros(item.VariantParent.SKUConversionValue), 0);

                TrackConversion(variantParentConversionName, conversionValue * item.CartItemUnits, item.CartItemUnits, siteName);
            }
        }


        /// <summary>
        /// Gets last used customer address if present or address with a default country defined in the 'CMSStoreDefaultCountryName' setting key.
        /// </summary>
        /// <param name="customerId">ID of the customer the address belongs to</param>
        /// <returns>AddressInfo object</returns>
        public static AddressInfo GetLastUsedOrDefaultAddress(int customerId)
        {
            var address = AddressInfoProvider.GetAddresses(customerId)
                                                 .OrderByDescending("AddressLastModified")
                                                 .TopN(1)
                                                 .FirstOrDefault();

            if (address != null)
            {
                return address;
            }

            CountryInfo defaultCountry = null;

            if (SiteContext.CurrentSite != null)
            {
                var countryName = ECommerceSettings.DefaultCountryName(SiteContext.CurrentSite.SiteName);
                defaultCountry = CountryInfoProvider.GetCountryInfo(countryName);
            }

            return new AddressInfo
            {
                AddressCountryID = defaultCountry?.CountryID ?? 0,
            };
        }


        /// <summary>
        /// Returns true when customer is going to be automatically registered after checkout process is finished.
        /// </summary>
        internal static bool IsCustomerRegisteredAfterCheckout(CustomerInfo customer)
        {
            var siteId = customer.CustomerSiteID > 0 ? customer.CustomerSiteID : SiteContext.CurrentSiteID;

            return IsCustomerRegisteredAfterCheckout(siteId);
        }


        /// <summary>
        /// Returns true when customer is going to be automatically registered after checkout process is finished.
        /// </summary>
        internal static bool IsCustomerRegisteredAfterCheckout(int siteId)
        {
            var factory = Service.Resolve<ICustomerRegistrationRepositoryFactory>();
            var repository = factory.GetRepository(siteId);

            return repository.IsCustomerRegisteredAfterCheckout;
        }

        #endregion
    }
}