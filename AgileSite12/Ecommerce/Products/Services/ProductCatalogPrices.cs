using System;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Container for product catalog prices (i.e. final prices shown to the customer).
    /// </summary>
    [DebuggerDisplay("Price = {" + nameof(Price) + "}")]
    public class ProductCatalogPrices
    {
        /// <summary>
        /// Price currency.
        /// </summary>
        public CurrencyInfo Currency
        {
            get;
        }


        /// <summary>
        /// Gets or sets the standard product catalog price. Does not contain any discounts.
        /// </summary>
        public decimal StandardPrice
        {
            get;
        }


        /// <summary>
        /// Gets or sets the product catalog price. All the discounts mentioned in the <see cref="Discounts"/> property are included.
        /// </summary>
        public decimal Price
        {
            get;
        }


        /// <summary>
        /// Gets or sets the tax calculated from the <see cref="Price"/>.
        /// </summary>
        public decimal Tax
        {
            get;
        }


        /// <summary>
        /// Summary of catalog-level discounts included in the <see cref="Price"/>.
        /// </summary>
        public ValuesSummary Discounts
        {
            get;
        }


        /// <summary>
        /// List price.
        /// </summary>
        /// <seealso cref="SKUInfo.SKURetailPrice"/>
        public decimal ListPrice
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductCatalogPrices"/> class with the specified <paramref name="price"/>, 
        /// <paramref name="standardPrice"/>, <paramref name="tax"/>, <paramref name="listPrice"/>, <paramref name="currency"/>, and <paramref name="discounts"/>.
        /// </summary>
        /// <param name="price">Product price.</param>
        /// <param name="standardPrice">Standard product price.</param>
        /// <param name="discounts">Summary of the discounts 'explaining' the difference between <paramref name="price"/> and <paramref name="standardPrice"/>.</param>
        /// <param name="currency">Price currency.</param>
        /// <param name="tax">Tax calculated from price.</param>
        /// <param name="listPrice">List price.</param>
        /// <exception cref="ArgumentNullException"><paramref name="currency"/></exception>
        public ProductCatalogPrices(decimal price, decimal standardPrice, decimal tax, decimal listPrice, CurrencyInfo currency, ValuesSummary discounts)
        {
            if (currency == null)
            {
                throw new ArgumentNullException(nameof(currency));
            }

            Price = price;
            StandardPrice = standardPrice;
            Discounts = discounts ?? new ValuesSummary();
            Currency = currency;
            Tax = tax;
            ListPrice = listPrice;
        }
    }
}