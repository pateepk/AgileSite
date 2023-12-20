using System;
using System.Linq;

using CMS.Core;
using CMS.EventLog;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default <see cref="IShoppingCartEvaluator"/> implementation.
    /// </summary>
    internal sealed class DefaultShoppingCartEvaluator : IShoppingCartEvaluator
    {
        private readonly IMultiBuyDiscountSource mMultiBuyDiscountSource;
        private readonly IShoppingCartAdapterService mCartAdapter;
        private readonly IShoppingCartCalculationFactory mCalculationFactory;


        public DefaultShoppingCartEvaluator(IMultiBuyDiscountSource multiBuyDiscountSource, IShoppingCartAdapterService cartAdapter, IShoppingCartCalculationFactory calculationFactory)
        {
            mCartAdapter = cartAdapter;
            mCalculationFactory = calculationFactory;
            mMultiBuyDiscountSource = multiBuyDiscountSource;
        }


        /// <summary>
        /// Evaluates discounts and calculates the price, shipping, taxes information on the given <paramref name="cart"/>.
        /// </summary>
        public void Evaluate(ShoppingCartInfo cart)
        {
            if (cart.Currency == null)
            {
                EventLogProvider.LogException("CartEvaluation", "NOCURRENCY", null, cart.ShoppingCartSiteID, "ShoppingCart currency is not set.");
                return;
            }

            AutoAddMultiBuyItems(cart);

            var request = mCartAdapter.GetCalculationRequest(cart);
            var result = mCartAdapter.GetCalculationResult(cart);

            var calculator = mCalculationFactory.GetCalculator(cart.ShoppingCartSiteID);
            calculator.Calculate(new CalculatorData(request, result));

            mCartAdapter.ApplyCalculationResult(cart, result);
        }


        /// <summary>
        /// Evaluates active discounts and automatically adds items which can be for free to the shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart where items will be automatically added in case they could be given for free.</param>
        private void AutoAddMultiBuyItems(ShoppingCartInfo cart)
        {
            var adder = new MultiBuyDiscountsAutoAdder
            {
                Items = cart.CartItems
            };

            var parameters = GetAutoAddParameters(cart);
            var discounts = mMultiBuyDiscountSource.GetDiscounts(parameters);

            // Reset cart's auto added items before the evaluator is created
            adder.Reset();

            var multiBuyItems = cart.CartProducts.Select(item => new MultiBuyItem(item));
            var evaluator = Service.Resolve<IMultiBuyDiscountsEvaluator>();
            evaluator.EvaluateDiscounts(discounts, multiBuyItems, adder);

            adder.UpdateAutoAddedItemsInShoppingCart(cart);
        }


        private static DiscountsParameters GetAutoAddParameters(ShoppingCartInfo cart)
        {
            var param = new DiscountsParameters
            {
                SiteID = cart.ShoppingCartSiteID,
                DueDate = cart.IsCreatedFromOrder ? cart.Order.OrderDate : DateTime.Now,
                Enabled = true,
                User = cart.User,
                Currency = cart.Currency,
                CouponCodes = cart.CouponCodes
            };

            return param;
        }
    }
}