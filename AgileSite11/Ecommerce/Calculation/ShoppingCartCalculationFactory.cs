using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="IShoppingCartCalculationFactory"/>.
    /// </summary>
    internal sealed class ShoppingCartCalculationFactory : IShoppingCartCalculationFactory
    {
        private static readonly Lazy<IShoppingCartCalculator> DefaultCalculator = new Lazy<IShoppingCartCalculator>(() => new ShoppingCartCalculatorCollection(GetCalculationSteps()));


        private static IEnumerable<IShoppingCartCalculator> GetCalculationSteps()
        {
            yield return new UnitPriceCalculator();
            yield return new CartItemDiscountCalculator();
            yield return new TotalValuesCalculator();
            yield return new OrderDiscountsCalculator();
            yield return new TotalValuesCalculator();
            yield return new ShippingCalculator();
            yield return new TaxCalculator();
            yield return new TotalValuesCalculator();
            yield return new OtherPaymentsCalculator();
            yield return new TotalValuesCalculator();
        }


        /// <summary>
        /// Returns calculator implementation based on given <see cref="SiteInfoIdentifier"/>.
        /// </summary>
        public IShoppingCartCalculator GetCalculator(SiteInfoIdentifier siteIdentifier)
        {
            return DefaultCalculator.Value;
        }
    }
}