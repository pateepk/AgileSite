using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

using Newtonsoft.Json;

[assembly: RegisterExtension(typeof(CMS.Ecommerce.Web.UI.EcommerceMethods), typeof(TransformationNamespace))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Ecommerce methods - wrapping methods for macro resolver.
    /// </summary>
    internal class EcommerceMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns link to "add to shopping cart".
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns link to \"add to shopping cart\".", 1)]
        [MacroMethodParam(0, "productId", typeof(object), "Product ID.")]
        [MacroMethodParam(1, "enabled", typeof(object), "Indicates whether product is enabled or not.")]
        public static object GetAddToShoppingCartLink(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.GetAddToShoppingCartLink(parameters[0]);

                case 2:
                    return EcommerceTransformationFunctions.GetAddToShoppingCartLink(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns link to add specified product to the user's wishlist.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns link to add specified product to the user's wishlist.", 1)]
        [MacroMethodParam(0, "productId", typeof(object), "Product ID.")]
        public static object GetAddToWishListLink(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.GetAddToWishListLink(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns link to remove specified product from the user's wishlist.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns link to remove specified product from the user's wishlist.", 1)]
        [MacroMethodParam(0, "productId", typeof(object), "Product ID.")]
        public static object GetRemoveFromWishListLink(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.GetRemoveFromWishListLink(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns formatted weight, based on mass unit format configuration.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns formatted weight, based on mass unit format configuration.", 1)]
        [MacroMethodParam(0, "weight", typeof(double), "Weight to be formatted.")]
        public static object GetFormattedWeight(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.GetFormattedWeight(ValidationHelper.GetDouble(parameters[0], 0.0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns SKU catalog price based on the SKU data and the data of the current shopping cart.
        /// Taxes are included based on the site settings.
        /// </summary>
        /// <param name="context">Evaluation context</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns SKU catalog price in given currency.", 1)]
        [MacroMethodParam(0, "skuId", typeof(object), "SKUID")]
        public static object GetSKUPrice(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new NotSupportedException();
            }

            // Get info from parameters
            var skuId = ValidationHelper.GetInteger(parameters[0], 0);
            var sku = SKUInfoProvider.GetSKUInfo(skuId);
            if (sku != null)
            {
                return EcommerceTransformationFunctions.GetSKUPrice(sku);
            }

            return 0;
        }


        /// <summary>
        /// Get SKU price in given currency. It's base SKU price without adjustments like taxes, discounts and options.
        /// </summary>
        /// <param name="context">Evaluation context</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns SKU price in given currency.", 2)]
        [MacroMethodParam(0, "sku", typeof(object), "SKU info object.")]
        [MacroMethodParam(1, "currency", typeof(object), "Currency info object.")]
        public static object GetPrice(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length != 2)
            {
                throw new NotSupportedException();
            }

            // Get infos from parameters
            var sku = parameters[0] as SKUInfo;
            var currency = parameters[1] as CurrencyInfo;

            if ((sku != null) && (currency != null))
            {
                var priceSource = Service.Resolve<ISKUPriceSourceFactory>().GetSKUPriceSource(SiteContext.CurrentSiteID);
                return priceSource.GetPrice(sku, currency);
            }

            return 0;
        }


        /// <summary>
        /// Returns amount of saved money based on the difference between product selling price and product list price or price before discounts.
        /// Taxes are included based on the site settings.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns amount of saved money based on the difference between product selling price and product list price or price before discounts.", 1)]
        [MacroMethodParam(0, "skuId", typeof(object), "SKUID")]
        public static object GetSKUPriceSaving(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Get infos from parameters
                    var skuId = ValidationHelper.GetInteger(parameters[0], 0);
                    var sku = SKUInfoProvider.GetSKUInfo(skuId);

                    if (sku != null)
                    {
                        return EcommerceTransformationFunctions.GetSKUPriceSaving(sku, false);
                    }

                    return 0;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns SKURetailPrice or price before discounts if saving is bigger.
        /// Returns zero if price saving is zero.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns SKURetailPrice or price before discounts if saving is bigger. Returns zero if price saving is zero.", 1)]
        [MacroMethodParam(0, "skuId", typeof(object), "SKUID")]
        public static object GetSKUOriginalPrice(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Get infos from parameters
                    var skuId = ValidationHelper.GetInteger(parameters[0], 0);
                    var sku = SKUInfoProvider.GetSKUInfo(skuId);

                    if (sku != null)
                    {
                        return EcommerceTransformationFunctions.GetSKUOriginalPrice(sku);
                    }

                    return 0;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns SKU tax.
        /// </summary>
        /// <param name="context">Evaluation context</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns SKU tax.", 1)]
        [MacroMethodParam(0, "skuId", typeof(object), "SKUID")]
        public static object GetSKUTax(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    var productId = ValidationHelper.GetInteger(parameters[0], 0);
                    var product = SKUInfoProvider.GetSKUInfo(productId);

                    return EcommerceTransformationFunctions.GetSKUTax(product);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns array of Google Tag Manager product objects in JSON format. 
        /// Array consists of products mapped from current customer's shopping cart. 
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns array of Google Tag Manager product objects in JSON format. Array consists of products mapped from current customer's shopping cart. ", 0)]
        [MacroMethodParam(0, "additionalDataJson", typeof(string), "JSON string with additional non-conflicting data to be merged with each product.")]
        [MacroMethodParam(1, "purpose", typeof(string), "Contextual information fitting for customizations.")]
        public static object GetGtmShoppingCartItemsJson(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 2)
            {
                throw new NotSupportedException();
            }

            string additionalDataJson = (parameters.Length > 0) ? ValidationHelper.GetString(parameters[0], "") : null;
            var additionalData = (!String.IsNullOrEmpty(additionalDataJson)) ? JsonConvert.DeserializeObject(additionalDataJson) : null;

            string purpose = (parameters.Length > 1) ? ValidationHelper.GetString(parameters[1], "") : null;

            var gtmData = GtmProductHelper.MapShoppingCartItems(ECommerceContext.CurrentShoppingCart?.CartItems, additionalData, purpose);

            return GtmDataHelper.SerializeToJson(gtmData, purpose);
        }


        /// <summary>
        /// Returns Google Tag Manager purchase object in JSON format. Purchase consists of order and its items. The order ID is inferred from query string parameter containing order hash.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns Google Tag Manager purchase object in JSON format. Purchase consists of order and its items. The order ID is inferred from query string parameter containing order hash.", 0)]
        [MacroMethodParam(0, "additionalDataJson", typeof(string), "JSON string with additional non-conflicting data to be merged with purchase")]
        [MacroMethodParam(1, "purpose", typeof(string), "Contextual information fitting for customizations")]
        public static object GetGtmPurchaseJson(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 2)
            {
                throw new NotSupportedException();
            }

            var orderHash = QueryHelper.GetString("o", "0");
            var orderId = ValidationHelper.GetInteger(WindowHelper.GetItem(orderHash), 0);
            var order = OrderInfoProvider.GetOrderInfo(orderId);
            if (order == null)
            {
                return "";
            }

            string additionalDataJson = (parameters.Length > 0) ? ValidationHelper.GetString(parameters[0], "") : null;
            var additionalData = (!String.IsNullOrEmpty(additionalDataJson)) ? JsonConvert.DeserializeObject(additionalDataJson) : null;

            string purpose = (parameters.Length > 1) ? ValidationHelper.GetString(parameters[1], "") : null;

            var gtmData = GtmOrderHelper.MapPurchase(order, additionalData, purpose);

            return GtmDataHelper.SerializeToJson(gtmData, purpose);
        }


        /// <summary>
        /// Returns Google Tag Manager product object in JSON format for <see cref="SKUInfo"/> identified by ID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns Google Tag Manager product object in JSON format for SKU identified by ID.", 1)]
        [MacroMethodParam(0, "skuId", typeof(int), "SKU ID")]
        [MacroMethodParam(1, "additionalDataJson", typeof(string), "JSON string with additional non-conflicting data to be merged with SKU")]
        [MacroMethodParam(2, "purpose", typeof(string), "Contextual information fitting for customizations")]
        public static object GetGtmProductJson(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 1 || parameters.Length > 3)
            {
                throw new NotSupportedException();
            }

            var skuId = ValidationHelper.GetInteger(parameters[0], 0);
            var sku = SKUInfoProvider.GetSKUInfo(skuId);
            if (sku == null)
            {
                return "";
            }

            string additionalDataJson = (parameters.Length > 1) ? ValidationHelper.GetString(parameters[1], "") : null;
            var additionalData = (!String.IsNullOrEmpty(additionalDataJson)) ? JsonConvert.DeserializeObject(additionalDataJson) : null;

            string purpose = (parameters.Length > 2) ? ValidationHelper.GetString(parameters[2], "") : null;

            var gtmData = GtmProductHelper.MapSKU(sku, additionalData, purpose);

            return GtmDataHelper.SerializeToJson(gtmData, purpose);
        }


        /// <summary>
        /// Returns URL to the shopping cart.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL to the shopping cart.", 1)]
        [MacroMethodParam(0, "siteName", typeof(string), "Site name.")]
        [MacroMethodParam(1, "cartGUID", typeof(object), "Cart GUID.")]
        public static object ShoppingCartURL(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.ShoppingCartURL(ValidationHelper.GetString(parameters[0], ""));

                case 2:
                    var cartURL = EcommerceTransformationFunctions.ShoppingCartURL(ValidationHelper.GetString(parameters[0], ""));
                    return URLHelper.AddParameterToUrl(cartURL, "cg", ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns products of the shopping cart. Bundle items and product options are excluded.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IEnumerable<ShoppingCartItemInfo>), "Returns products of the shopping cart. Bundle items and product options are excluded.", 1)]
        [MacroMethodParam(0, "cartGUID", typeof(object), "Cart GUID.")]
        public static object ShoppingCartProducts(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    var cartGUID = ValidationHelper.GetGuid(parameters[0], Guid.Empty);
                    var cart = ShoppingCartInfoProvider.GetShoppingCartInfo(cartGUID);

                    if (cart == null)
                    {
                        return string.Empty;
                    }

                    // Disable GlobalAdminContext to ensure that role-based discounts are applied correctly
                    using (new CMSActionContext { User = cart.User, UseGlobalAdminContext = false })
                    {
                        // Evaluate cart to ensure in-memory properties
                        cart.Evaluate();
                    }

                    return cart.CartProducts;
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL to the wish list.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL to the wish list.", 1)]
        [MacroMethodParam(0, "siteName", typeof(string), "Site name")]
        public static object WishlistURL(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.WishlistURL(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user friendly URL of the specified SKU and site name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns product URL.", 1)]
        [MacroMethodParam(0, "skuId", typeof(object), "SKU ID.")]
        public static object GetProductUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.GetProductUrl(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the specified product with feed parameter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the specified product with feed parameter.", 3, SpecialParameters = new[] { "FeedName", "InstanceGUID" })]
        [MacroMethodParam(0, "skuGUID", typeof(object), "SKU GUID.")]
        [MacroMethodParam(1, "skuName", typeof(object), "SKU name.")]
        [MacroMethodParam(2, "siteName", typeof(object), "Site name.")]
        public static object GetProductUrlForFeed(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 5:
                    return EcommerceTransformationFunctions.GetProductUrlForFeed(TransformationHelper.HelperObject.GetFeedName(parameters[0], parameters[1]), parameters[2], parameters[3], parameters[4]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user friendly URL of the specified SKU and site name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns user friendly URL of the specified SKU.", 2)]
        [MacroMethodParam(0, "skuGuid", typeof(object), "SKU GUID.")]
        [MacroMethodParam(1, "skuName", typeof(object), "SKU name.")]
        [MacroMethodParam(2, "siteName", typeof(object), "Site name.")]
        public static object GetProductUrlByGUID(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return EcommerceTransformationFunctions.GetProductUrl(parameters[0], parameters[1]);

                case 3:
                    return EcommerceTransformationFunctions.GetProductUrl(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets object from the specified column of the manufacturer with specific ID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets object from the specified column of the manufacturer with specific ID.", 2)]
        [MacroMethodParam(0, "id", typeof(int), "Manufacturer ID.")]
        [MacroMethodParam(1, "column", typeof(string), "Column name.")]
        public static object GetManufacturer(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return EcommerceTransformationFunctions.GetManufacturer(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets object from the specified column of the department with specific ID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets object from the specified column of the department with specific ID.", 2)]
        [MacroMethodParam(0, "id", typeof(int), "Department ID.")]
        [MacroMethodParam(1, "column", typeof(string), "Column name.")]
        public static object GetDepartment(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return EcommerceTransformationFunctions.GetDepartment(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets object from the specified column of the supplier with specific ID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets object from the specified column of the supplier with specific ID.", 2)]
        [MacroMethodParam(0, "id", typeof(int), "Supplier ID.")]
        [MacroMethodParam(1, "column", typeof(string), "Column name.")]
        public static object GetSupplier(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return EcommerceTransformationFunctions.GetSupplier(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets object from the specified column of the internal status with specific ID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets object from the specified column of the internal status with specific ID.", 2)]
        [MacroMethodParam(0, "id", typeof(int), "Internal status ID.")]
        [MacroMethodParam(1, "column", typeof(string), "Column name.")]
        public static object GetInternalStatus(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return EcommerceTransformationFunctions.GetInternalStatus(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets object from the specified column of the public status with specific ID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets object from the specified column of the public status with specific ID.", 2)]
        [MacroMethodParam(0, "id", typeof(int), "Public status ID.")]
        [MacroMethodParam(1, "column", typeof(string), "Column name.")]
        public static object GetPublicStatus(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return EcommerceTransformationFunctions.GetPublicStatus(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns complete HTML code of the specified resized product image, if not such image exists, default image is returned.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns complete HTML code of the specified resized product image, if not such image exists, default image is returned.", 2)]
        [MacroMethodParam(0, "imageUrl", typeof(object), "Product image url.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternate text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Max side size of the image.")]
        [MacroMethodParam(3, "width", typeof(object), "Width of the image.")]
        [MacroMethodParam(4, "height", typeof(object), "Height of the image.")]
        public static object GetProductImage(EvaluationContext context, params object[] parameters)
        {
            string img = "<img src=\"{0}\" alt=\"{1}\" />";

            switch (parameters.Length)
            {
                case 2:
                    return string.Format(img, EcommerceTransformationFunctions.GetSKUImageUrl(parameters[0], 0, 0, 0, 0), parameters[1]);

                case 3:
                    return string.Format(img, EcommerceTransformationFunctions.GetSKUImageUrl(parameters[0], 0, 0, parameters[2], 0), parameters[1]);

                case 4:
                    return string.Format(img, EcommerceTransformationFunctions.GetSKUImageUrl(parameters[0], parameters[2], parameters[3], 0, 0), parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the price formatted according to the properties of the current shopping cart's currency. Rounding is based on the settings of the shopping cart's site.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns the price formatted according to the properties of the current shopping cart's currency. Rounding is based on the settings of the shopping cart's site.", 1)]
        [MacroMethodParam(0, "price", typeof(decimal), "Price to be formatted")]
        [MacroMethodParam(1, "round", typeof(bool), "True - price is rounded before formatting, according to the settings of the cart's site")]
        public static string FormatPrice(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return GetFormattedPrice(parameters[0], true);

                case 2:
                    return GetFormattedPrice(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the price formatted according to the properties of the specified shopping cart's currency. Rounding is based on the settings of the shopping cart's site.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns the price formatted according to the properties of the specified shopping cart's currency. Rounding is based on the settings of the shopping cart's site.", 2)]
        [MacroMethodParam(0, "price", typeof(decimal), "Price to be formatted")]
        [MacroMethodParam(1, "cartID", typeof(int), "Shopping cart ID")]
        [MacroMethodParam(2, "round", typeof(bool), "True - price is rounded before formatting, according to the settings of the cart's site")]
        public static string FormatPriceForCart(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return GetFormattedPriceForCart(parameters[0], parameters[1],  true);

                case 3:
                    return GetFormattedPriceForCart(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns price rounded and formatted according to the current site's main currency properties.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns price rounded and formatted according to the current site's main currency properties.", 1)]
        [MacroMethodParam(0, "price", typeof(decimal), "Price to be formatted")]
        [MacroMethodParam(1, "round", typeof(bool), "True - price is rounded according to the site's main currency settings before formatting")]
        public static string FormatPriceInMainCurrency(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return GetFormattedPriceInMainCurrency(parameters[0], true);

                case 2:
                    return GetFormattedPriceInMainCurrency(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns main currency for a given site.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(CurrencyInfo), "Returns main currency info for a given site.", 1)]
        [MacroMethodParam(0, "siteID", typeof(int), "Site ID")]
        public static CurrencyInfo GetMainSiteCurrency(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return CurrencyInfoProvider.GetMainCurrency(ValidationHelper.GetInteger(parameters[0], 0));
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns number of products in current shopping cart.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns number of products in current shopping cart.", 0)]
        public static object GetShoppingCartItemsCount(EvaluationContext context, params object[] parameters)
        {
            if (ECommerceContext.CurrentShoppingCart != null)
            {
                return ECommerceContext.CurrentShoppingCart.CartItems.Count;
            }

            return 0;
        }


        /// <summary>
        /// Returns true if option is used in variant.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if option is used in variants.", 1)]
        [MacroMethodParam(0, "skuID", typeof(int), "SKU ID")]
        public static object IsOptionUsedInVariants(EvaluationContext context, params object[] parameters)
        {
            int optionID = ValidationHelper.GetInteger(parameters[0], 0);

            if (optionID > 0)
            {
                // Check if some variant is defined by this option
                DataSet variants = VariantOptionInfoProvider.GetVariantOptions()
                                                            .TopN(1)
                                                            .Column("VariantSKUID")
                                                            .WhereEquals("OptionSKUID", optionID);

                return !DataHelper.DataSourceIsEmpty(variants);
            }

            return false;
        }


        /// <summary>
        /// Returns info message about coupon usage.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns formatted message about coupons codes.", 1)]
        [MacroMethodParam(0, "Discount", typeof(object), "DISCOUNT")]
        public static object GetCouponsMessage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return CouponCodeInfoProvider.GetCouponUsageInfoMessage((DiscountInfo)parameters[0]);
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns info message about coupon usage.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns formatted message about coupons codes.", 1)]
        [MacroMethodParam(0, "MultiBuyDiscount", typeof(object), "MULTIBUYDISCOUNT")]
        public static object GetMultiBuyCouponsMessage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return MultiBuyCouponCodeInfoProvider.GetMultiBuyCouponUsageInfoMessage((MultiBuyDiscountInfo)parameters[0]);
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns info message about gift card coupon usage.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns formatted message about gift card coupons codes.", 1)]
        [MacroMethodParam(0, "GiftCard", typeof(object), "GIFTCARD")]
        public static object GetGiftCardCouponsMessage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return GiftCardCouponCodeInfoProvider.GetGiftCardCouponUsageInfoMessage((GiftCardInfo)parameters[0]);
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns mass unit (e.g. lb or kg) used for expressing masses.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns mass unit (e.g. lb or kg) used by the system.", 0)]
        public static string GetMassUnit(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return ECommerceSettings.MassUnit();

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns button usable for removing shopping cart coupon code.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns button usable for removing shopping cart coupon code.", 1)]
        [MacroMethodParam(0, "couponCode", typeof(string), "Discount coupon code value")]
        [MacroMethodParam(1, "cssClass", typeof(string), "Button's CSS class; 'btn btn-default' class is used by default")]
        [MacroMethodParam(2, "text", typeof(string), "Button's text; resource string with key 'general.remove' is used by default")]
        public static string GetDiscountCouponCodeRemoveButton(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return EcommerceTransformationFunctions.GetDiscountCouponCodeRemoveButton((string)parameters[0]);
                case 2:
                    return EcommerceTransformationFunctions.GetDiscountCouponCodeRemoveButton((string)parameters[0], (string)parameters[1]);
                case 3:
                    return EcommerceTransformationFunctions.GetDiscountCouponCodeRemoveButton((string)parameters[0], (string)parameters[1], (string)parameters[2]);
                default:
                    throw new NotSupportedException();
            }
        }


        #region "Private methods"

        /// <summary>
        /// Returns the price formatted according to the properties of the current shopping cart's currency. Rounding is based on the settings of the shopping cart's site.
        /// </summary>
        /// <param name="priceObj">Price to be formatted</param>
        /// <param name="roundObj">True - price is rounded before formatting, according to the settings of the cart's site</param>
        private static string GetFormattedPrice(object priceObj, object roundObj)
        {
            var price = ValidationHelper.GetDecimal(priceObj, 0m);
            var round = ValidationHelper.GetBoolean(roundObj, true);

            var cart = ECommerceContext.CurrentShoppingCart;

            if (round)
            {
                var rounder = Service.Resolve<IRoundingServiceFactory>().GetRoundingService(cart.ShoppingCartSiteID);
                price = rounder.Round(price, cart.Currency);
            }

            return CurrencyInfoProvider.GetFormattedPrice(price, cart.Currency);
        }


        /// <summary>
        /// Returns the price formatted according to the properties of the specified shopping cart's currency. Rounding is based on the settings of the cart's site.
        /// </summary>
        /// <param name="priceObj">Price to be formatted</param>
        /// <param name="cartIDObj">Shopping cart ID</param>
        /// <param name="roundObj">True - price is rounded before formatting, according to the settings of the cart's site</param>
        private static string GetFormattedPriceForCart(object priceObj, object cartIDObj, object roundObj)
        {
            var price = ValidationHelper.GetDecimal(priceObj, 0m);
            var cartID = ValidationHelper.GetInteger(cartIDObj, 0);
            var round = ValidationHelper.GetBoolean(roundObj, true);

            var cart = ShoppingCartInfoProvider.GetShoppingCartInfo(cartID);

            if (cart == null)
            {
                return String.Empty;
            }

            if (round)
            {
                var rounder = Service.Resolve<IRoundingServiceFactory>().GetRoundingService(cart.ShoppingCartSiteID);
                price = rounder.Round(price, cart.Currency);
            }

            return CurrencyInfoProvider.GetFormattedPrice(price, cart.Currency);
        }


        /// <summary>
        /// Returns price rounded and formatted according to the main currency properties.
        /// </summary>
        /// <param name="priceObj">Price to be formatted</param>
        /// <param name="roundObj">True - price is rounded according to the main currency settings before formatting</param>
        private static string GetFormattedPriceInMainCurrency(object priceObj, object roundObj)
        {
            var price = ValidationHelper.GetDecimal(priceObj, 0m);
            var round = ValidationHelper.GetBoolean(roundObj, true);

            var currentSiteID = SiteContext.CurrentSiteID;
            var mainCurrency = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrency(currentSiteID);

            if (round)
            {
                var rounder = Service.Resolve<IRoundingServiceFactory>().GetRoundingService(currentSiteID);
                price = rounder.Round(price, mainCurrency);
            }

            return CurrencyInfoProvider.GetFormattedPrice(price, mainCurrency);
        }

        #endregion
    }
}