using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(CalculatorDataMethods), typeof(CalculatorData))]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Calculator data methods - wrapping methods for macro resolver.
    /// </summary>
    internal sealed class CalculatorDataMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if calculation data contains all or any product defined by product GUIDs depending on quantifier value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if calculation data contains all or any product defined by product GUIDs depending on quantifier value.", 3)]
        [MacroMethodParam(0, "data", typeof(CalculatorData), "Calculation data")]
        [MacroMethodParam(1, "productGUIDs", typeof(string), "Product GUIDs")]
        [MacroMethodParam(2, "quantifier", typeof(bool), "If true, all given products has to match. If false, just one given product has to match.")]
        public static object ContainsProducts(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return CalculationDataContainsProducts(parameters);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Evaluate if calculation data contains products (all by default).
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        private static bool CalculationDataContainsProducts(IReadOnlyList<object> parameters)
        {
            var data = parameters[0] as CalculatorData;
            var guids = ValidationHelper.GetString(parameters[1], null);
            var quantifier = ValidationHelper.GetBoolean(parameters[2], true);

            if (data?.Request == null || string.IsNullOrEmpty(guids))
            {
                return false;
            }

            var requestedProducts = GetProductGuids(guids).ToArray();
            var existingProducts = GetProductGuidsFromItems(data.Request.Items);

            if (!quantifier)
            {
                return existingProducts.Intersect(requestedProducts).Any();
            }

            return existingProducts.Intersect(requestedProducts).Count() == requestedProducts.Length;
        }


        private static IEnumerable<Guid> GetProductGuids(string stringWithGuids)
        {
            var guids = ValidationHelper.GetString(stringWithGuids, string.Empty).Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var productGuids = Array.ConvertAll(guids, Guid.Parse).Distinct().ToArray();

            return productGuids;
        }


        private static IEnumerable<Guid> GetProductGuidsFromItems(IEnumerable<CalculationRequestItem> items)
        {
            return items.Select(item => item.SKU.SKUGUID).Distinct().ToArray();
        }


        /// <summary>
        /// Returns true when calculated order is one of the first orders registered customer has placed.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true when calculated order is one of the first orders registered customer has placed.", 2)]
        [MacroMethodParam(0, "data", typeof(CalculatorData), "Calculation data")]
        [MacroMethodParam(1, "numberOfOrders", typeof(int), "Number of existing orders")]
        public static object OrderIsOneOfTheFirstOrdersPlacedByRegisteredCustomer(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return OrderIsOneOfTheFirstOrdersPlacedByRegisteredCustomer(parameters);

                default:
                    throw new NotSupportedException();
            }
        }


        private static bool OrderIsOneOfTheFirstOrdersPlacedByRegisteredCustomer(IReadOnlyList<object> parameters)
        {
            var data = parameters[0] as CalculatorData;
            var numberOfOrders = ValidationHelper.GetInteger(parameters[1], 0);
            var customer = data?.Request?.Customer;

            if (customer == null || numberOfOrders <= 0)
            {
                return false;
            }

            if (!customer.CustomerIsRegistered && customer.Orders.Count == 0)
            {
                return ECommerceHelper.IsCustomerRegisteredAfterCheckout(customer);
            }

            if (customer.Orders.Count < numberOfOrders)
            {
                return true;
            }

            if (data.Request.OrderId > 0)
            {
                var firstNumberOfOrders = customer.Orders.OrderBy(i => i.OrderDate).Take(numberOfOrders).Select(i => i.OrderID);
                return firstNumberOfOrders.Contains(data.Request.OrderId);
            }

            return false;
        }
    }
}
