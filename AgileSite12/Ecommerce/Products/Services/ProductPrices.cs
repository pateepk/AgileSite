using System;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Container for product prices and applied discounts.
    /// </summary>
    [DebuggerDisplay("Price = {" + nameof(Price) + "}")]
    public class ProductPrices
    {
        /// <summary>
        /// Price currency.
        /// </summary>
        public CurrencyInfo Currency
        {
            get;
        }


        /// <summary>
        /// Standard product price. Price does not contain any discounts.
        /// </summary>
        public decimal StandardPrice
        {
            get;
        }


        /// <summary>
        /// Calculated product price. All discounts are included.
        /// </summary>
        public decimal Price
        {
            get;
        }


        /// <summary>
        /// Applied discounts summary. Sum of the discount is already applied in the <see cref="Price"/> property.
        /// </summary>
        public ValuesSummary AppliedDiscounts
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPrices"/> class with the specified <paramref name="price"/>, 
        /// <paramref name="standardPrice"/>, <paramref name="currency"/> and <paramref name="discounts"/>.
        /// </summary>
        /// <param name="price">Product price.</param>
        /// <param name="standardPrice">Standard product price.</param>
        /// <param name="discounts">Summary of the discounts 'explaining' the difference between <paramref name="price"/> and <paramref name="standardPrice"/>.</param>
        /// <param name="currency">Price currency.</param>
        /// <exception cref="ArgumentNullException"><paramref name="currency"/></exception>
        public ProductPrices(decimal price, decimal standardPrice, CurrencyInfo currency, ValuesSummary discounts)
        {
            if (currency == null)
            {
                throw new ArgumentNullException(nameof(currency));
            }

            Price = price;
            StandardPrice = standardPrice;
            AppliedDiscounts = discounts ?? new ValuesSummary();
            Currency = currency;
        }
    }
}
