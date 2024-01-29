using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service calculating the <see cref="ShippingPrices"/> including the discount applications.
    /// </summary>
    internal class ShippingPriceService : IShippingPriceService
    {
        private readonly IShippingDiscountSource mShippingDiscountSource;
        private readonly IShippingPriceCalculatorFactory mShippingPriceCalculatorFactory;
        private readonly IRoundingServiceFactory mRoundingServiceFactory;
        private readonly IDiscountApplicator mDiscountApplicator;


        /// <summary>
        /// Creates an instance of <see cref="ShippingPriceService"/>.
        /// </summary>
        /// <param name="discountSource">Shipping discount source.</param>
        /// <param name="priceCalculatorFactory">Shipping price calculation factory.</param>
        /// <param name="roundingServiceFactory">Rounding service.</param>
        /// <param name="discountApplicator">Discount applicator.</param>
        public ShippingPriceService(
            IShippingDiscountSource discountSource,
            IShippingPriceCalculatorFactory priceCalculatorFactory,
            IRoundingServiceFactory roundingServiceFactory,
            IDiscountApplicator discountApplicator
            )
        {
            mDiscountApplicator = discountApplicator;
            mRoundingServiceFactory = roundingServiceFactory;
            mShippingPriceCalculatorFactory = priceCalculatorFactory;
            mShippingDiscountSource = discountSource;
        }


        /// <summary>
        /// Calculates the price of the <see cref="CalculationRequest.ShippingOption"/> in the <see cref="CalculationRequest.Currency"/> including all discounts.
        /// Method uses the implementation of <see cref="IShippingPriceCalculatorFactory"/> to determine the base price 
        /// and applies the shipping discount returned by implementation of the <see cref="IShippingDiscountSource"/>.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderAmount">Actual order amount for evaluation of the free shipping offers.</param>
        public ShippingPrices GetShippingPrice(CalculatorData data, decimal orderAmount)
        {
            var baseShippingPrice = CalculateShipping(data.Request);

            // Get shipping discounts suitable for current shopping cart and apply them on the base price
            var discountApplications = mShippingDiscountSource.GetDiscounts(data, orderAmount).ToList();
            var shippingGroups = discountApplications.Select(d => new DiscountCollection(new[] { d }));
            var discountSummary = mDiscountApplicator.ApplyDiscounts(baseShippingPrice, shippingGroups);

            var discountSum = discountSummary.Sum(x => x.Value);

            var totalPrice = (discountSum > baseShippingPrice) ? 0 : baseShippingPrice - discountSum;


            return new ShippingPrices
            {
                StandardPrice = baseShippingPrice,
                Price = totalPrice,
                AppliedDiscounts = discountApplications,
                ShippingDiscountSummary = discountSummary
            };
        }


        private decimal CalculateShipping(CalculationRequest request)
        {
            var option = request.ShippingOption;

            if (option == null)
            {
                return 0m;
            }

            var shippingCalculator = mShippingPriceCalculatorFactory.GetCalculator(option);
            if (shippingCalculator == null)
            {
                return 0m;
            }

            // Get shipping price for chosen shipping option
            var deliveryBuilder = Service.Resolve<IDeliveryBuilder>();
            deliveryBuilder.SetFromCalculationRequest(request);
            var package = deliveryBuilder.BuildDelivery();

            var shippingPrice = shippingCalculator.GetPrice(package, request.Currency.CurrencyCode);

            var roundingService = GetRoundingService(option.ShippingOptionSiteID);
            return roundingService.Round(shippingPrice, request.Currency);
        }


        private IRoundingService GetRoundingService(int siteId)
        {
            return mRoundingServiceFactory.GetRoundingService(siteId);
        }

    }
}
