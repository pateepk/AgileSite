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
        /// Standard product price. Price does not contain any discounts.
        /// </summary>
        public decimal StandardPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Calculated product price. All discounts are included.
        /// </summary>
        public decimal Price
        {
            get;
            set;
        }


        /// <summary>
        /// Applied discounts summary. Sum of the discount is already applied in the <see cref="Price"/> property.
        /// </summary>
        public ValuesSummary AppliedDiscounts
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPrices"/>.
        /// </summary>
        public ProductPrices()
           : this(0)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPrices"/> class with the specified <paramref name="price"/> and no discounts.
        /// </summary>
        /// <param name="price">Product price.</param>
        public ProductPrices(decimal price)
            : this(price, price, null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPrices"/> class with the specified <paramref name="price"/>, 
        /// <paramref name="standardPrice"/> and <paramref name="discounts"/>.
        /// </summary>
        /// <param name="price">Product price.</param>
        /// <param name="standardPrice">Standard product price.</param>
        /// <param name="discounts">Summary of the discounts 'explaining' the difference between <paramref name="price"/> and <paramref name="standardPrice"/>.</param>
        public ProductPrices(decimal price, decimal standardPrice, ValuesSummary discounts)
        {
            Price = price;
            StandardPrice = standardPrice;
            AppliedDiscounts = discounts ?? new ValuesSummary();
        }
    }
}
