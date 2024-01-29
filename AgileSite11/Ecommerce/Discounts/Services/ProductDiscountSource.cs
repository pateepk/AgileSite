using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of discounts related to product units.
    /// </summary>
    public class ProductDiscountSource : IProductDiscountSource
    {
        private readonly ICatalogDiscountSource mCatalogDiscountSource;
        private readonly IVolumeDiscountSource mVolumeDiscountSource;
        private readonly ISiteMainCurrencySource mMainCurrencySource;
        private readonly ICurrencyConverterFactory mConverterFactory;
        private readonly IRoundingServiceFactory mRoundingServiceFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDiscountSource"/> class using the specified discount sources, main currency source, currency converter factory and rounding service factory.
        /// </summary>
        /// <param name="catalogDiscountSource">The source of catalog discounts.</param>
        /// <param name="volumeDiscountSource">The source of volume discounts.</param>
        /// <param name="mainCurrencySource">The source of main currencies.</param>
        /// <param name="converterFactory">The factory supplying currency converters.</param>
        /// <param name="roundingServiceFactory">The factory supplying service for price rounding.</param>
        public ProductDiscountSource(ICatalogDiscountSource catalogDiscountSource, IVolumeDiscountSource volumeDiscountSource, 
            ISiteMainCurrencySource mainCurrencySource, ICurrencyConverterFactory converterFactory, IRoundingServiceFactory roundingServiceFactory)
        {
            mCatalogDiscountSource = catalogDiscountSource;
            mVolumeDiscountSource = volumeDiscountSource;
            mMainCurrencySource = mainCurrencySource;
            mConverterFactory = converterFactory;
            mRoundingServiceFactory = roundingServiceFactory;
        }


        /// <summary>
        /// Returns groups of discounts for the specified <paramref name="sku"/> and price.
        /// </summary>
        /// <remarks>
        /// Override this method to change the order in which the product discounts are applied. The following code shows the default implementation.
        /// <code>
        /// public override IEnumerable&lt;DiscountCollection&gt; GetDiscounts(SKUInfo sku, decimal standardPrice, PriceParameters priceParams)
        /// {
        ///     var groups = new List&lt;DiscountCollection&gt;();
        /// 
        ///     AddCatalogDiscounts(groups, sku, priceParams);
        ///     AddVolumeDiscounts(groups, sku, priceParams);
        /// 
        ///     return groups;
        /// }
        /// </code>
        /// </remarks>
        /// <param name="sku">The SKU object.</param>
        /// <param name="standardPrice">The price of the <paramref name="sku"/>.</param>
        /// <param name="priceParams">Other parameters.</param>
        public virtual IEnumerable<DiscountCollection> GetDiscounts(SKUInfo sku, decimal standardPrice, PriceParameters priceParams)
        {
            var groups = new List<DiscountCollection>();

            AddCatalogDiscounts(groups, sku, priceParams);
            AddVolumeDiscounts(groups, sku, priceParams);

            return groups;
        }


        /// <summary>
        /// Adds grouped catalog discounts applicable to the specified <paramref name="sku"/> to the <paramref name="groups"/> collection.
        /// </summary>
        /// <param name="groups">Collection of discount groups.</param>
        /// <param name="sku">The product.</param>
        /// <param name="priceParams">Calculation parameters</param>
        protected void AddCatalogDiscounts(ICollection<DiscountCollection> groups, SKUInfo sku, PriceParameters priceParams)
        {
            var discounts = mCatalogDiscountSource.GetDiscounts(sku, priceParams);

            DiscountCollection group = null;

            IConditionalDiscount previousDiscount = null;
            foreach (var discount in discounts)
            {
                if (!discount.ApplyTogetherWith(previousDiscount))
                {
                    if (group != null)
                    {
                        groups.Add(group);
                    }

                    group = null;
                }

                group = group ?? new DiscountCollection();

                var productDiscount = CreateProductDiscount(priceParams, discount.DiscountDisplayName, discount.DiscountValue, discount.DiscountIsFlat, false);
                group.Add(productDiscount);

                previousDiscount = discount;
            }

            if (group != null)
            {
                groups.Add(group);
            }
        }


        /// <summary>
        /// Adds a new discount group containing the volume discount for the specified <paramref name="sku"/> in the given <see cref="PriceParameters.Quantity"/> quantity.
        /// </summary>
        /// <remarks>
        /// No discount group is added when no volume discount is applicable on the <paramref name="sku"/>.
        /// </remarks>
        /// <param name="groups">Collection of discount groups.</param>
        /// <param name="sku">The product.</param>
        /// <param name="priceParams">Calculation parameters</param>
        protected void AddVolumeDiscounts(ICollection<DiscountCollection> groups, SKUInfo sku, PriceParameters priceParams)
        {
            var discount = mVolumeDiscountSource.GetDiscount(sku, priceParams.Quantity);
            if (discount == null)
            {
                return;
            }

            var productDiscount = CreateProductDiscount(priceParams, discount.ItemDiscountDisplayName, discount.VolumeDiscountValue, discount.VolumeDiscountIsFlatValue, discount.IsGlobal);
            groups.Add(new DiscountCollection(new[] { productDiscount }));
            
        }


        private IDiscount CreateProductDiscount(PriceParameters priceParams, string displayName, decimal value, bool isFlat, bool isGlobal)
        {
            if (isFlat)
            {
                int siteId = priceParams.SiteID;
                var discountCurrency = mMainCurrencySource.GetSiteMainCurrencyCode(isGlobal ? 0 : siteId);
                var converter = mConverterFactory.GetCurrencyConverter(siteId);

                var valueInCalculationCurrency = converter.Convert(value, discountCurrency, priceParams.Currency.CurrencyCode);

                var roundingService = mRoundingServiceFactory.GetRoundingService(siteId);
                valueInCalculationCurrency = roundingService.Round(valueInCalculationCurrency, priceParams.Currency);

                return CreateFixedProductDiscount(displayName, valueInCalculationCurrency, priceParams);
            }

            var rate = value / 100m;

            return CreatePercentageProductDiscount(displayName, rate, priceParams);
        }


        /// <summary>
        /// Creates a new fixed product discount of the specified <paramref name="displayName"/> and <paramref name="value"/>.
        /// </summary>
        /// <remarks>
        /// Override this method to change the way the system calculates fixed product discounts.
        /// </remarks>
        /// <param name="displayName">The display name of the discount.</param>
        /// <param name="value">The value of the discount.</param>
        /// <param name="priceParams">Other parameters.</param>
        protected virtual IDiscount CreateFixedProductDiscount(string displayName, decimal value, PriceParameters priceParams)
        {
            return new FixedDiscountApplication(displayName, value, null);
        }


        /// <summary>
        /// Creates a new percentage product discount of the specified <paramref name="displayName"/>. 
        /// The amount of the discount is specified by <paramref name="rate"/> parameter.
        /// </summary>
        /// <remarks>
        /// Override this method to change the way the system calculates percentage product discounts.
        /// </remarks>
        /// <param name="displayName">The display name of the discount.</param>
        /// <param name="rate">The rate of the discount (use 0.2 for 20% discount).</param>
        /// <param name="priceParams">Other parameters.</param>
        protected virtual IDiscount CreatePercentageProductDiscount(string displayName, decimal rate, PriceParameters priceParams)
        {
            var roundingService = mRoundingServiceFactory.GetRoundingService(priceParams.SiteID);

            return new PercentageDiscountApplication(displayName, rate, priceParams.Currency, null, roundingService);
        }
    }
}