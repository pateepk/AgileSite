using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Helper class that helps to map products to Google Tag Manager product object.
    /// </summary>
    public class GtmProductHelper : AbstractHelper<GtmProductHelper>
    {
        /// <summary>
        /// Maps <see cref="SKUInfo"/> to <see cref="GtmData"/> that represents properties and values of Google Tag Manger product.
        /// </summary>
        /// <param name="sku"><see cref="SKUInfo"/> to be mapped.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with <paramref name="sku"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger product object.</returns>
        /// <seealso cref="MapSKUInternal(SKUInfo, object, string)"/>
        public static GtmData MapSKU(SKUInfo sku, object additionalData = null, string purpose = null)
        {
            return HelperObject.MapSKUInternal(sku, additionalData, purpose);
        }


        /// <summary>
        /// Maps shopping cart items to collection of <see cref="GtmData"/> that represents properties and values of Google Tag Manger products.
        /// </summary>
        /// <param name="cartItems">Shopping cart items to be mapped.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with every <see cref="ShoppingCartItemInfo"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger product object.</returns>
        /// <seealso cref="MapShoppingCartItemsInternal(IEnumerable{ShoppingCartItemInfo}, object, string)"/>
        /// <seealso cref="MapSKU(SKUInfo, object, string)"/>
        public static IEnumerable<GtmData> MapShoppingCartItems(IEnumerable<ShoppingCartItemInfo> cartItems, object additionalData = null, string purpose = null)
        {
            return HelperObject.MapShoppingCartItemsInternal(cartItems, additionalData, purpose);
        }


        /// <summary>
        /// Maps <see cref="SKUInfo"/> to <see cref="GtmData"/> that represents properties and values of Google Tag Manger product.
        /// </summary>
        /// <param name="sku"><see cref="SKUInfo"/> to be mapped.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with <paramref name="sku"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <example>
        /// <para>To customize returned gtmData override this method in similar fashion.</para>
        /// <para>base.MapInternal(gtmObject, additionalData, purpose);</para>
        /// <para>gtmData.Add("key", "value");</para>
        /// <para>To customize the whole mapping process do not call base implementation.</para>
        /// </example>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger product object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sku"/> is null.</exception>
        protected virtual GtmData MapSKUInternal(SKUInfo sku, object additionalData = null, string purpose = null)
        {
            if (sku == null)
            {
                throw new ArgumentNullException(nameof(sku));
            }

            var mergedData = new GtmData();

            if (!String.IsNullOrWhiteSpace(sku.SKUNumber))
            {
                mergedData.Add("id", sku.SKUNumber);
            }

            if (!String.IsNullOrWhiteSpace(sku.SKUName))
            {
                mergedData.Add("name", sku.SKUName);
            }
            
            mergedData.Add("price", GetPrice(sku));

            var brand = BrandInfoProvider.GetBrandInfo(sku.SKUBrandID);
            if (!String.IsNullOrWhiteSpace(brand?.BrandDisplayName))
            {
                mergedData.Add("brand", brand.BrandDisplayName);
            }
            
            var gtmData = GtmPropertiesMerger.Merge(mergedData, additionalData);

            return gtmData;
        }


        /// <summary>
        /// Maps shopping cart items to collection of <see cref="GtmData"/> that represents properties and values of Google Tag Manger products.
        /// </summary>
        /// <param name="cartItems">Shopping cart items to be mapped.</param>
        /// <param name="additionalData">Data with additional non-conflicting key value pairs to be merged with every <see cref="ShoppingCartItemInfo"/>.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <example>
        /// <para>To customize returned gtmData override this method in similar fashion.</para>
        /// <para>base.MapCartItemsInternal(gtmObject, additionalData, purpose);</para>
        /// <para>gtmData.Add("key", "value");</para>
        /// <para>To customize the whole mapping process do not call base implementation.</para>
        /// </example>
        /// <returns>The <see cref="GtmData"/> that represents Google Tag Manger product object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cartItems"/> is null.</exception>
        protected virtual IEnumerable<GtmData> MapShoppingCartItemsInternal(IEnumerable<ShoppingCartItemInfo> cartItems, object additionalData = null, string purpose = null)
        {
            if (cartItems == null)
            {
                throw new ArgumentNullException(nameof(cartItems));
            }
            
            var skuItems = SKUInfoProvider.GetSKUs().WhereIn(nameof(SKUInfo.SKUID), cartItems.Select(i => i.SKUID).ToList()).ToDictionary(sku => sku.SKUID);
            foreach (var item in cartItems)
            {
                var gtmProduct = MapSKU(skuItems[item.SKUID], new { quantity = item.CartItemUnits }, purpose);
                yield return GtmPropertiesMerger.Merge(gtmProduct, additionalData);
            }
        }


        /// <summary>
        /// Returns price for <paramref name="sku"/>.
        /// </summary>
        private decimal GetPrice(SKUInfo sku)
        {
            var cart = ECommerceContext.CurrentShoppingCart;
            if (cart == null)
            {
                return sku.SKUPrice;
            }

            // Similar way for retrieving price is used UI by transformations
            return Service.Resolve<ICatalogPriceCalculatorFactory>()
                .GetCalculator(cart.ShoppingCartSiteID)
                .GetPrices(sku, null, cart).Price;
        }
    }
}
