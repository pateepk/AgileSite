using System;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides total shopping cart price calculation.
    /// </summary>
    public class TotalValuesCalculator : IShoppingCartCalculator
    {
        /// <summary>
        /// Runs shopping cart calculation based on given calculation related data.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        /// <remarks>
        /// Given <see cref="CalculatorData.Result"/> is modified during calculation process.
        /// Calculates <see cref="CalculationResultItem.LineSubtotal"/> values for each result item.
        /// </remarks>
        public void Calculate(CalculatorData calculationData)
        {
            if (calculationData.Request.Site == null)
            {
                throw new ArgumentException("Calculation request site has to be assigned.", nameof(calculationData));
            }

            var result = calculationData.Result;

            result.Subtotal = CalculateSubtotal(calculationData);
            result.Total = CalculateTotal(calculationData);
            result.GrandTotal = CalculateGrandTotal(calculationData);
        }


        /// <summary>
        /// Returns sum price of all items.
        /// </summary>
        private static decimal CalculateSubtotal(CalculatorData calculationData)
        {
            var requestItems = calculationData.Request.Items;
            var resultItems = calculationData.Result.Items;

            if (requestItems == null || resultItems == null)
            {
                return 0m;
            }

            var linePrices = requestItems.Join(resultItems,
                                req => req.ItemGuid,
                                res => res.ItemGuid,
                                (request, result) => result.LineSubtotal = request.Quantity * result.ItemUnitPrice - result.ItemDiscount);

            return linePrices.Sum();
        }


        /// <summary>
        /// Returns total price of all items including shipping and taxes.
        /// </summary>
        private static decimal CalculateTotal(CalculatorData calculationData)
        {
            var total = calculationData.Result.Subtotal - calculationData.Result.OrderDiscount + calculationData.Result.Shipping;

            if (!GetPriceIncludesTax(calculationData.Request.Site))
            {
                total += calculationData.Result.Tax;
            }

            return total;
        }


        /// <summary>
        /// Returns total price lowered by other payments.
        /// </summary>
        private static decimal CalculateGrandTotal(CalculatorData calculationData)
        {
            return calculationData.Result.Total - calculationData.Result.OtherPayments;
        }


        private static bool GetPriceIncludesTax(int siteId)
        {
            var factory = Service.Resolve<ISettingServiceFactory>();
            var service = factory.GetSettingService(siteId);

            return service.GetBooleanValue(ECommerceSettings.INCLUDE_TAX_IN_PRICES);
        }
    }
}
