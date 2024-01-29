using System;
using System.Collections.Generic;

using CMS.Ecommerce;

namespace APIExamples
{
    /// <summary>
    /// Holds shopping cart API examples.
    /// </summary>
    /// <pageTitle>Shopping carts</pageTitle>
    internal class ShoppingCarts
    {
        /// <heading>Adding products to a shopping cart</heading>
        private void AddProductToShoppingCart()
        {
            // Gets a product to add to the shopping cart
            SKUInfo product = SKUInfoProvider.GetSKUs()
                                                .WhereEquals("SKUName", "NewProduct")
                                                .WhereNull("SKUOptionCategoryID")
                                                .FirstObject;

            if (product != null)
            {
                // Gets the current shopping cart
                ShoppingCartInfo cart = ECommerceContext.CurrentShoppingCart;

                // Creates the shopping cart in the database if it does not exist yet
                if (cart.ShoppingCartID == 0)
                {
                    ShoppingCartInfoProvider.SetShoppingCartInfo(cart);
                }

                // Prepares a shopping cart item representing 1 unit of the product
                ShoppingCartItemParameters parameters = new ShoppingCartItemParameters(product.SKUID, 1);
                ShoppingCartItemInfo cartItem = ShoppingCartInfoProvider.SetShoppingCartItem(cart, parameters);

                // Saves the shopping cart item to the shopping cart
                ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(cartItem);

                // Evaluates and recalculates the shopping cart (discounts, shipping, price totals, etc).
                cart.Evaluate();
            }
        }


        /// <heading>Updating the unit count of a shopping cart item</heading>
        private void UpdateShoppingCartItemUnits()
        {
            // Gets the product
            SKUInfo product = SKUInfoProvider.GetSKUs()
                                                .WhereEquals("SKUName", "NewProduct")
                                                .WhereNull("SKUOptionCategoryID")
                                                .FirstObject;

            if (product != null)
            {
                // Prepares the shopping cart item
                ShoppingCartItemInfo item = null;

                // Gets the current shopping cart
                ShoppingCartInfo cart = ECommerceContext.CurrentShoppingCart;

                // Loops through the items in the shopping cart
                foreach (ShoppingCartItemInfo cartItem in cart.CartItems)
                {
                    // Gets the first shopping cart item matching the specified product
                    if (cartItem.SKUID == product.SKUID)
                    {
                        item = cartItem;
                        break;
                    }
                }

                if (item != null)
                {
                    // Updates the unit count of the shopping cart item to 3
                    ShoppingCartItemInfoProvider.UpdateShoppingCartItemUnits(item, 3);

                    // Evaluates and recalculates the shopping cart (discounts, shipping, price totals, etc).
                    cart.Evaluate();
                }
            }
        }


        /// <heading>Adding coupon codes to a shopping cart</heading>
        private void AddCouponCodeToShoppingCart()
        {
            // Gets the current shopping cart
            ShoppingCartInfo cart = ECommerceContext.CurrentShoppingCart;

            // Adds the 'SAVE10' coupon code to the cart, if it is related to a valid discount for the current shopping cart content
            // Automatically evaluates and recalculates the shopping cart
            cart.AddCouponCode("SAVE10");
        }


        /// <heading>Removing products from a shopping cart</heading>
        private void RemoveProductFromShoppingCart()
        {
            // Gets the product
            SKUInfo product = SKUInfoProvider.GetSKUs()
                                                .WhereEquals("SKUName", "NewProduct")
                                                .WhereNull("SKUOptionCategoryID")
                                                .FirstObject;

            if (product != null)
            {
                // Prepares the shopping cart item
                ShoppingCartItemInfo item = null;

                // Gets the current shopping cart
                ShoppingCartInfo cart = ECommerceContext.CurrentShoppingCart;

                // Loops through the items in the shopping cart
                foreach (ShoppingCartItemInfo cartItem in cart.CartItems)
                {
                    // Gets the first shopping cart item matching the specified product
                    if (cartItem.SKUID == product.SKUID)
                    {
                        item = cartItem;
                        break;
                    }
                }

                if (item != null)
                {
                    // Removes the item from the shopping cart
                    ShoppingCartInfoProvider.RemoveShoppingCartItem(cart, item.CartItemID);

                    // Removes the item from the database
                    ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(item);

                    // Evaluates and recalculates the shopping cart (discounts, shipping, price totals, etc).
                    cart.Evaluate();
                }
            }
        }        
    }
}
