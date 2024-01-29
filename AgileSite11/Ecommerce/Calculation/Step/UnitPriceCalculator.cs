using System.Collections.Generic;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides calculation of cart items' unit prices.
    /// </summary>
    public class UnitPriceCalculator : IShoppingCartCalculator
    {
        /// <summary>
        /// Calculates the unit price for the each item in the <see cref="CalculationRequest.Items"/> collection.
        /// Unit price including catalog-level discounts is stored in the corresponding <see cref="CalculationResult.Items"/>.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        public void Calculate(CalculatorData calculationData)
        {
            var request = calculationData.Request;
            var result = calculationData.Result;

            foreach (var item in request.Items)
            {
                CalculateUnitPrice(item, request, result);
            }
        }


        /// <summary>
        /// Calculates the price of one shopping cart item unit based on calculation request data.
        /// </summary>
        /// <param name="item">Item to be calculated</param>
        /// <param name="request">Calculation request data</param>
        /// <param name="result">Calculation result</param>
        private void CalculateUnitPrice(CalculationRequestItem item, CalculationRequest request, CalculationResult result)
        {
            if (item?.SKU == null)
            {
                return;
            }

            var pricing = Service.Resolve<IProductPricingService>();
            var options = item.Options?.Select(o => o.SKU);

            ProductPrices prices;

            if (item.PriceOverride != null)
            {
                prices = item.PriceOverride;
            }
            else
            {
                prices = pricing.GetPrices(item.SKU, options, new PriceParameters
                {
                    Currency = request.Currency,
                    SiteID = request.Site,
                    User = request.User,
                    Customer = request.Customer,
                    Quantity = GetUnitsCount(item, request.Items),
                    CalculationDate = request.CalculationDate
                });
            }

            // Apply the unit price to the result item
            var resItem = result.Items.FirstOrDefault(r => r.ItemGuid == item.ItemGuid);
            if (resItem != null)
            {
                resItem.ItemUnitPrice = prices.Price;
                resItem.UnitDiscount = prices.StandardPrice - prices.Price;
                resItem.UnitDiscountSummary.Merge(prices.AppliedDiscounts);
            }
        }


        /// <summary>
        /// Gets the units count for specified cart item.
        /// </summary>
        /// <param name="item">Item for which units count is calculated</param>
        /// <param name="items">All items from the calculation request</param>
        private static decimal GetUnitsCount(CalculationRequestItem item, IEnumerable<CalculationRequestItem> items)
        {
            return items.Where(cartItem => cartItem.SKU.SKUID == item.SKU.SKUID).Sum(cartItem => cartItem.Quantity);
        }
    }
}
