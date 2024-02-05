using System;
using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a calculator of product catalog prices.
    /// </summary>
    /// <remarks>
    /// An implementation of this interface is used to obtain product prices in product catalogs and product details.
    /// </remarks>
    /// <seealso cref="ICatalogPriceCalculatorFactory"/>
    public interface ICatalogPriceCalculator
    {
        /// <summary>
        /// Returns product prices for the specified product or variant.
        /// </summary>
        /// <param name="sku">A product or a product variant.</param>
        /// <param name="options">Product options of all types (accessory, attribute, text). Use <c>null</c> to get prices without options.</param>
        /// <param name="cart">A shopping cart providing context of calculation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sku"/> or <paramref name="cart"/> is <c>null</c>.</exception>
        ProductCatalogPrices GetPrices(SKUInfo sku, IEnumerable<SKUInfo> options, ShoppingCartInfo cart);


        /// <summary>
        /// Returns a price adjustment corresponding to <paramref name="option"/> when bought with a <paramref name="parent"/>.
        /// </summary>
        /// <param name="option">A product option.</param>
        /// <param name="parent">A product or a product variant.</param>
        /// <param name="cart">A shopping cart providing context of calculation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="option"/>, <paramref name="parent"/> or <paramref name="cart"/> is <c>null</c>.</exception>
        decimal GetAdjustment(SKUInfo option, SKUInfo parent, ShoppingCartInfo cart);
    }
}
