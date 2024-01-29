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
        /// Gets or sets the standard product catalog price. Does not contain any discounts.
        /// </summary>
        public decimal StandardPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the product catalog price. All the discounts mentioned in the <see cref="Discounts"/> property are included.
        /// </summary>
        public decimal Price
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the tax calculated from the <see cref="Price"/>.
        /// </summary>
        public decimal Tax
        {
            get;
            set;
        }


        /// <summary>
        /// Summary of catalog-level discounts included in the <see cref="Price"/>.
        /// </summary>
        public ValuesSummary Discounts
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductCatalogPrices"/>.
        /// </summary>
        public ProductCatalogPrices()
           : this(0)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductCatalogPrices"/> class with the specified <paramref name="price"/> and no discounts.
        /// </summary>
        /// <param name="price">Product price.</param>
        public ProductCatalogPrices(decimal price)
            : this(price, price, null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductCatalogPrices"/> class with the specified <paramref name="price"/>, 
        /// <paramref name="standardPrice"/> and <paramref name="discounts"/>.
        /// </summary>
        /// <param name="price">Product price.</param>
        /// <param name="standardPrice">Standard product price.</param>
        /// <param name="discounts">Summary of the discounts 'explaining' the difference between <paramref name="price"/> and <paramref name="standardPrice"/>.</param>
        public ProductCatalogPrices(decimal price, decimal standardPrice, ValuesSummary discounts)
        {
            Price = price;
            StandardPrice = standardPrice;
            Discounts = discounts ?? new ValuesSummary();
        }
    }
}