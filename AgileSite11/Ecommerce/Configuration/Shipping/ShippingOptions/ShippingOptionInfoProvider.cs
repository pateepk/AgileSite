using System;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing ShippingOptionInfo management.
    /// </summary>
    public class ShippingOptionInfoProvider : AbstractInfoProvider<ShippingOptionInfo, ShippingOptionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ShippingOptionInfoProvider()
            : base(ShippingOptionInfo.TYPEINFO, new HashtableSettings
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
        /// Returns the query for all shipping options.
        /// </summary>
        public static ObjectQuery<ShippingOptionInfo> GetShippingOptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns shipping option with specified ID.
        /// </summary>
        /// <param name="optionId">Shipping option ID</param>        
        public static ShippingOptionInfo GetShippingOptionInfo(int optionId)
        {
            return ProviderObject.GetInfoById(optionId);
        }


        /// <summary>
        /// Returns shipping option with specified name.
        /// </summary>
        /// <param name="optionName">Shipping option name</param>                
        /// <param name="siteName">Site name</param>                
        public static ShippingOptionInfo GetShippingOptionInfo(string optionName, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(optionName, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Sets (updates or inserts) specified shipping option.
        /// </summary>
        /// <param name="optionObj">Shipping option to be set</param>
        public static void SetShippingOptionInfo(ShippingOptionInfo optionObj)
        {
            ProviderObject.SetInfo(optionObj);
        }


        /// <summary>
        /// Deletes specified shipping option.
        /// </summary>
        /// <param name="optionObj">Shipping option to be deleted</param>
        public static void DeleteShippingOptionInfo(ShippingOptionInfo optionObj)
        {
            ProviderObject.DeleteInfo(optionObj);
        }


        /// <summary>
        /// Deletes shipping option with specified ID.
        /// </summary>
        /// <param name="optionId">Shipping option ID</param>
        public static void DeleteShippingOptionInfo(int optionId)
        {
            var optionObj = GetShippingOptionInfo(optionId);
            DeleteShippingOptionInfo(optionObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all shipping options matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the options should be retrieved from. If set to 0, global shipping options are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled shipping options from the specified site are returned. False - all site options are returned</param>
        public static ObjectQuery<ShippingOptionInfo> GetShippingOptions(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetShippingOptionsInternal(siteId, onlyEnabled);
        }


        /// <summary>
        /// Returns weight formatted according site specific weight format string. If no format string specified, uses x.xx format instead.
        /// </summary>
        /// <param name="weight">Weight</param>
        /// <param name="siteName">Name of the site for obtaining format string</param>
        /// <param name="encode">Encode output</param>
        public static string GetFormattedWeight(double weight, string siteName, bool encode = true)
        {
            return ProviderObject.GetFormattedWeightInternal(weight, siteName, encode);
        }


        /// <summary>
        /// Check if shopping cart needs shipping, return true if it does.
        /// </summary>
        /// <param name="cart">Shopping cart object</param>
        public static bool IsShippingNeeded(ShoppingCartInfo cart)
        {
            return ProviderObject.IsShippingNeededInternal(cart);
        }


        /// <summary>
        /// Checks if shipping option is applicable in given shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart in which shipping option should be used.</param>
        /// <param name="shippingOption">Shipping option to check if it is applicable.</param>
        public static bool IsShippingOptionApplicable(ShoppingCartInfo cart, ShippingOptionInfo shippingOption)
        {
            return ProviderObject.IsShippingOptionApplicableInternal(cart, shippingOption);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ShippingOptionInfo info)
        {
            // Ensure null value - in case ShippingOptionTaxClassID is set to zero or negative value using SetValue
            info.ShippingOptionTaxClassID = info.ShippingOptionTaxClassID;

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all shipping options matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the options should be retrieved from. If set to 0, global shipping options are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled shipping options from the specified site are returned. False - all site options are returned</param>
        protected virtual ObjectQuery<ShippingOptionInfo> GetShippingOptionsInternal(int siteId, bool onlyEnabled)
        {
            // Get shipping options on requested site
            var query = GetShippingOptions().OnSite(siteId);

            if (onlyEnabled)
            {
                query.WhereTrue("ShippingOptionEnabled");
            }

            return query;
        }


        /// <summary>
        /// Returns weight formatted according site specific weight format string. If no format string specified, uses x.xx format instead.
        /// </summary>
        /// <param name="weight">Weight</param>
        /// <param name="siteName">Name of the site for obtaining format string</param>
        /// <param name="encode">Encode output</param>
        protected virtual string GetFormattedWeightInternal(double weight, string siteName, bool encode = true)
        {
            string format = ECommerceSettings.WeightFormattingString(siteName);
            if (string.IsNullOrEmpty(format))
            {
                return (weight.ToString("0.00"));
            }

            return (encode) ? HTMLHelper.HTMLEncode(string.Format(format, weight)) : string.Format(format, weight);
        }


        /// <summary>
        /// Check if shopping cart needs shipping, return true if it does.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        protected virtual bool IsShippingNeededInternal(ShoppingCartInfo cart)
        {
            // Check if there are shopping cart items that need shipping
            return cart.CartContentItems.Any(cartItem => (cartItem.SKU != null) && cartItem.SKU.SKUNeedsShipping);
        }


        /// <summary>
        /// Checks if shipping option is applicable for given shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart in which shipping option should be used.</param>
        /// <param name="shippingOption">Shipping option to check if it is applicable.</param>
        protected virtual bool IsShippingOptionApplicableInternal(ShoppingCartInfo cart, ShippingOptionInfo shippingOption)
        {
            // Shipping option is not selected yet
            if (shippingOption == null)
            {
                return true;
            }

            // Shipping option is from same site as shopping cart
            if (cart.ShoppingCartSiteID != shippingOption.ShippingOptionSiteID)
            {
                return false;
            }

            var shippingCarrier = CarrierInfoProvider.GetCarrierProvider(shippingOption.ShippingOptionCarrierID);
            if (shippingCarrier == null)
            {
                return true;
            }

            var deliveryBuilder = Service.Resolve<IDeliveryBuilder>();
            var request = Service.Resolve<IShoppingCartAdapterService>().GetCalculationRequest(cart);
            deliveryBuilder.SetFromCalculationRequest(request);
            deliveryBuilder.SetShippingOption(shippingOption);
            var delivery = deliveryBuilder.BuildDelivery();

            return shippingCarrier.CanDeliver(delivery);
        }

        #endregion
    }
}