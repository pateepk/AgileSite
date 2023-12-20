using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(ShoppingCartMethods), typeof(ShoppingCartInfo))]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Shopping cart methods - wrapping methods for macro resolver.
    /// </summary>
    internal class ShoppingCartMethods : MacroMethodContainer
    {
        #region "Private methods"

        /// <summary>
        /// Evaluate if Shopping cart contains products (all by default).
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="containsAny">if set to true, check if contains any of products; all otherwise</param>
        private static bool ShoppingCartContainsProducts(object[] parameters, bool containsAny = false)
        {
            var cart = (ShoppingCartInfo)parameters[0];
            var selectedGuids = GetProductGuids(parameters[1]);

            if (cart == null)
            {
                return false;
            }

            var itemGuids = GetGuidListFromItems(cart.CartItems);

            if (containsAny)
            {
                return selectedGuids.Any(itemGuids.Contains);
            }

            return selectedGuids.All(itemGuids.Contains);
        }

        #endregion


        #region "Helper methods"

        private static IEnumerable<Guid> GetProductGuids(object stringWithGuids)
        {
            var guids = ValidationHelper.GetString(stringWithGuids, String.Empty).Split(';');
            var productGuids = Array.ConvertAll(guids, Guid.Parse).ToList();

            return productGuids;
        }


        private static IEnumerable<Guid> GetGuidListFromItems(IEnumerable<ShoppingCartItemInfo> list)
        {
            return list.Select(item => item.SKU.SKUGUID).ToList();
        }

        #endregion
    }
}