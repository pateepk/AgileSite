using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a calculator of product catalog prices.
    /// </summary>
    internal class CatalogPriceCalculator : ICatalogPriceCalculator
    {
        private readonly IProductPricingService mPricingService;
        private readonly ICatalogTaxCalculator mTaxCalculator;
        private readonly ISKUPriceSourceFactory mSKUPriceSourceFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogPriceCalculator"/> class with the specified product pricing service.
        /// </summary>
        /// <param name="pricingService">A product pricing service.</param>
        /// <param name="taxCalculator">A catalog tax calculator.</param>
        /// <param name="skuPriceFactory">Site specific price source factory.</param>
        public CatalogPriceCalculator(IProductPricingService pricingService, ICatalogTaxCalculator taxCalculator, ISKUPriceSourceFactory skuPriceFactory)
        {
            mPricingService = pricingService;
            mTaxCalculator = taxCalculator ?? new DummyTaxCalculator();
            mSKUPriceSourceFactory = skuPriceFactory;
        }


        /// <summary>
        /// Returns product prices for the specified product or variant.
        /// </summary>
        /// <param name="sku">A product or a product variant.</param>
        /// <param name="options">Product options of all types (accessory, attribute, text).</param>
        /// <param name="cart">A shopping cart providing context of calculation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sku"/> or <paramref name="cart"/> is <c>null</c>.</exception>
        public ProductCatalogPrices GetPrices(SKUInfo sku, IEnumerable<SKUInfo> options, ShoppingCartInfo cart)
        {
            if (sku == null)
            {
                throw new ArgumentNullException(nameof(sku));
            }

            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            var priceParameters = GetPriceParameters(cart);
            var taxParameters = GetTaxParameters(cart);

            return GetPrices(sku, options, priceParameters, taxParameters);
        }


        /// <summary>
        /// Returns a price adjustment corresponding to <paramref name="option"/> when bought with a <paramref name="parent"/>.
        /// </summary>
        /// <param name="option">A product option.</param>
        /// <param name="parent">A product or a product variant.</param>
        /// <param name="cart">A shopping cart providing context of calculation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="option"/>, <paramref name="parent"/> or <paramref name="cart"/> is <c>null</c>.</exception>
        public decimal GetAdjustment(SKUInfo option, SKUInfo parent, ShoppingCartInfo cart)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            var productPrices = GetPrices(parent, null, cart);
            var productWithOptionsPrices = GetPrices(parent, new[] { option }, cart);

            return productWithOptionsPrices.Price - productPrices.Price;
        }


        private PriceParameters GetPriceParameters(ShoppingCartInfo cart)
        {
            return new PriceParameters
            {
                Currency = cart.Currency,
                SiteID = cart.ShoppingCartSiteID,
                User = cart.User,
                Customer = cart.Customer
            };
        }


        private TaxCalculationParameters GetTaxParameters(ShoppingCartInfo cart)
        {
            return new TaxCalculationParameters
            {
                Currency = cart.Currency,
                Customer = cart.Customer,
                CalculationDate = DateTime.Now,
                BillingAddress = cart.ShoppingCartBillingAddress,
                ShippingAddress = cart.ShoppingCartShippingAddress,
                SiteID = cart.ShoppingCartSiteID
            };
        }


        private ProductCatalogPrices GetPrices(SKUInfo sku, IEnumerable<SKUInfo> options, PriceParameters priceParameters, TaxCalculationParameters taxParameters)
        {
            ProductCatalogPrices result;

            if (options == null)
            {
                result = GetProductPrices(sku, priceParameters, taxParameters);
            }
            else
            {
                var accessories = new List<SKUInfo>();
                var otherOptions = new List<SKUInfo>();

                CategorizeOptions(options, accessories, otherOptions);

                var productPrices = GetProductPrices(sku, priceParameters, taxParameters, otherOptions);
                var accessoryPrices = GetAccessoryPrices(accessories, priceParameters, taxParameters);

                result = AddAccessoryPrices(productPrices, accessoryPrices);
            }

            return result;
        }


        private static void CategorizeOptions(IEnumerable<SKUInfo> options, List<SKUInfo> accessories, List<SKUInfo> otherOptions)
        {
            foreach (var option in options)
            {
                var category = option.IsAccessoryProduct ? accessories : otherOptions;

                category.Add(option);
            }
        }


        private List<ProductCatalogPrices> GetAccessoryPrices(List<SKUInfo> accessories, PriceParameters priceParameters, TaxCalculationParameters taxParameters)
        {
            return accessories.Select(accessory => GetProductPrices(accessory, priceParameters, taxParameters)).ToList();
        }


        private static ProductCatalogPrices AddAccessoryPrices(ProductCatalogPrices result, List<ProductCatalogPrices> accessoryPrices)
        {
            accessoryPrices.Add(result);

            var standardPrice = accessoryPrices.Sum(p => p.StandardPrice);
            var price = accessoryPrices.Sum(p => p.Price);
            var tax = accessoryPrices.Sum(p => p.Tax);

            return new ProductCatalogPrices(price, standardPrice, tax, result.ListPrice, result.Currency, result.Discounts);
        }


        private ProductCatalogPrices GetProductPrices(SKUInfo sku, PriceParameters priceParameters, TaxCalculationParameters taxParameters, IEnumerable<SKUInfo> attributeAndTextOptions = null)
        {
            var prices = mPricingService.GetPrices(sku, attributeAndTextOptions, priceParameters);

            decimal tax;

            var standardPrice = mTaxCalculator.ApplyTax(sku, prices.StandardPrice, taxParameters, out tax);
            var price = mTaxCalculator.ApplyTax(sku, prices.Price, taxParameters, out tax);

            var listPrice = mSKUPriceSourceFactory.GetSKUListPriceSource(priceParameters.SiteID).GetPrice(sku, priceParameters.Currency);
            var result = new ProductCatalogPrices(price, standardPrice, tax, listPrice, priceParameters.Currency, prices.AppliedDiscounts);

            return result;
        }


        private class DummyTaxCalculator : ICatalogTaxCalculator
        {
            public decimal ApplyTax(SKUInfo sku, decimal price, TaxCalculationParameters parameters, out decimal tax)
            {
                tax = 0m;

                return price;
            }
        }
    }
}