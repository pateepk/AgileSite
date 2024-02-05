using System;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides tax calculation.
    /// </summary>
    public class TaxCalculator : IShoppingCartCalculator
    {
        /// <summary>
        /// Runs shopping cart calculation based on given calculation related data.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        /// <remarks>
        /// Calculates <see cref="CalculationResult.Tax"/> value.
        /// </remarks>
        public void Calculate(CalculatorData calculationData)
        {
            var request = calculationData.Request;
            var result = calculationData.Result;

            if (request.Site == null)
            {
                throw new ArgumentException("Calculation request site has to be assigned.", nameof(calculationData));
            }

            // Prepare context for tax calculation
            var taxCalculationRequest = new TaxCalculationRequest
            {
                Shipping = request.ShippingOption,
                ShippingPrice = result.Shipping,
                TaxParameters = new TaxCalculationParameters
                {
                    Customer = request.Customer,
                    Currency = request.Currency,
                    CalculationDate = request.CalculationDate,
                    BillingAddress = request.BillingAddress,
                    ShippingAddress = request.ShippingAddress,
                    SiteID = request.Site
                },
                Discount = result.OrderDiscount
            };

            // Create tax calculation items
            if ((request.Items != null) && (result.Items != null))
            {
                var taxItems = request.Items.Join(result.Items,
                req => req.ItemGuid,
                res => res.ItemGuid,
                (req, res) => new TaxItem
                {
                    SKU = req.SKU,
                    Price = res.LineSubtotal,
                    Quantity = req.Quantity
                });

                foreach (var taxItem in taxItems)
                {
                    taxCalculationRequest.Items.Add(taxItem);
                }
            }

            // Calculate tax
            var taxCalculator = Service.Resolve<ITaxCalculationServiceFactory>().GetTaxCalculationService(request.Site);
            var taxes = taxCalculator.CalculateTaxes(taxCalculationRequest);

            result.Tax = taxes.TotalTax;
            result.TaxSummary.Merge(taxes.Summary);
        }
    }
}
