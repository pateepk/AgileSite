using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class calculating missed discount opportunities and adding discounted items to the shopping cart automatically.
    /// </summary>
    public class MultiBuyDiscountsAutoAdder : IMultiBuyDiscountsApplicator
    {
        #region "Properties"

        /// <summary>
        /// Dictionary with products which could be added to the shopping cart for free in format [SKUID, number of its units].
        /// </summary>
        public Dictionary<int, int> ProductsToBeAutoAdded
        {
            get;
        } = new Dictionary<int, int>();


        /// <summary>
        /// Auto-add candidates with information about suitability for auto-adding. 
        /// Key is SKUID and value is a tuple with SKUSellOnlyIfAvailable information and number of available units on stock.
        /// </summary>
        private Dictionary<int, Tuple<bool, int>> AvailableProducts
        {
            get;
        } = new Dictionary<int, Tuple<bool, int>>();

        #endregion


        #region "IMultiBuyDiscountApplicator implementation"

        /// <summary>
        /// Cart items to find auto add triggers.
        /// </summary>
        public IEnumerable<ShoppingCartItemInfo> Items
        {
            get;
            set;
        }


        /// <summary>
        /// Resets auto adder to its initial state.
        /// </summary>
        public void Reset()
        {
            if (Items == null)
            {
                return;
            }

            foreach (var item in Items)
            {
                // Nothing is provided for free
                item.CartItemUnits -= item.CartItemAutoAddedUnits;
                item.CartItemAutoAddedUnits = 0;
            }
        }


        /// <summary>
        /// Applies discount to given number of unit of given item.
        /// </summary>
        /// <param name="discount">Discount to be applied.</param>
        /// <param name="itemToBeDiscounted">Cart item to apply discount on.</param>
        /// <param name="units">Number of unit to be discounted.</param>
        public void ApplyDiscount(IMultiBuyDiscount discount, MultiBuyItem itemToBeDiscounted, int? units = null)
        {
        }


        /// <summary>
        /// Notifies the applicator that discount was nearly applied. 
        /// It could be applied on given products if present in cart.
        /// </summary>
        /// <param name="discount">Nearly applied discount.</param>
        /// <param name="missedApplications">Missed application count.</param>
        public bool AcceptsMissedDiscount(IMultiBuyDiscount discount, int missedApplications)
        {
            if (discount.AutoAddEnabled)
            {
                var autoAddCandidateID = discount.GetMissingProducts().FirstOrDefault(ProductCanBeAutoAdded);

                if (autoAddCandidateID > 0)
                {
                    MarkYToBeAutoAdded(autoAddCandidateID, missedApplications);
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region "Shopping cart methods"

        /// <summary>
        /// Inserts/updates shopping cart items which user can get for free, but he did not insert these items to the cart manually.
        /// </summary>
        /// <param name="cart">Shopping cart to add free items to.</param>
        internal void UpdateAutoAddedItemsInShoppingCart(ShoppingCartInfo cart)
        {
            foreach (var autoAddCandidate in ProductsToBeAutoAdded)
            {
                int autoAddedSKUID = autoAddCandidate.Key;
                int autoAddedSKUUnits = autoAddCandidate.Value;

                ShoppingCartItemInfo item = CreateShoppingCartItem(cart, autoAddedSKUID, autoAddedSKUUnits);

                // Set cart item in database, bundle items are set as well if auto added product is a bundle
                if ((item != null) && (cart.ShoppingCartID > 0))
                {
                    // Update shopping cart item in database
                    ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(item);

                    // Update bundle items in database
                    foreach (ShoppingCartItemInfo bundleItem in item.BundleItems)
                    {
                        ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(bundleItem);
                    }
                }
            }

            // Clear items which was automatically added to the cart previously, but right now, there is not enough discount triggers
            ClearAutoAddedItemsInShoppingCart(cart);
        }


        /// <summary>
        /// Creates shopping cart item to be auto added.
        /// </summary>
        /// <param name="cart">Shopping cart to add auto added item.</param>
        /// <param name="skuID">SKUID of item which will be auto added.</param>
        /// <param name="units">Count of auto added units.</param>
        private ShoppingCartItemInfo CreateShoppingCartItem(ShoppingCartInfo cart, int skuID, int units)
        {
            ShoppingCartItemInfo cartItem = null;

            ShoppingCartItemParameters itemParams = new ShoppingCartItemParameters(skuID, units);

            // Get requested SKU info object from database
            SKUInfo skuObj = SKUInfoProvider.GetSKUInfo(skuID);
            if (skuObj != null)
            {
                // Set item in the shopping cart object
                cartItem = ShoppingCartInfoProvider.SetShoppingCartItem(cart, itemParams);
                cartItem.CartItemAutoAddedUnits = units;
            }

            return cartItem;
        }


        /// <summary>
        /// Removes auto added items which unit count is 0 from shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart to add free items to.</param>
        private void ClearAutoAddedItemsInShoppingCart(ShoppingCartInfo cart)
        {
            var itemsToRemove = cart.CartItems.FindAll(item => item.CartItemUnits < 1);

            foreach (var item in itemsToRemove)
            {
                // Delete the CartItem form the shopping cart object (session)
                ShoppingCartInfoProvider.RemoveShoppingCartItem(cart, item.CartItemGUID);

                if (cart.ShoppingCartID > 0)
                {
                    // Deletes the CartItem from the database
                    ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(item.CartItemGUID);
                }
            }
        }


        /// <summary>
        /// Checks if product does not have options and is available on stock if stock availability is required.
        /// </summary>
        /// <param name="skuID">SKUID to be checked.</param>
        protected virtual bool ProductCanBeAutoAdded(int skuID)
        {
            if (skuID < 1)
            {
                return false;
            }

            if (!AvailableProducts.ContainsKey(skuID))
            {
                // Find out if product can be auto added
                AvailableProducts.Add(skuID, GetUnitsWhichCanBeAutoAdded(skuID));
            }

            // Product with options cannot be added
            if (AvailableProducts[skuID] == null)
            {
                return false;
            }

            int productToBeAutoAddedUnits = ProductsToBeAutoAdded.ContainsKey(skuID) ? ProductsToBeAutoAdded[skuID] : 0;
            // Find cart item for this sku (ignore bundles)
            var cartItem = Items.FirstOrDefault(item => !item.IsBundleItem && (item.SKUID == skuID));
            var cartItemUnits = cartItem?.CartItemUnits ?? 0;

            // Returns true if availability on stock is not required or there is enough items on stock
            return !AvailableProducts[skuID].Item1 || ((AvailableProducts[skuID].Item2 - cartItemUnits) > productToBeAutoAddedUnits);
        }

        #endregion
        

        #region "Helper methods"

        private void MarkYToBeAutoAdded(int getYID, int units)
        {
            if (!ProductsToBeAutoAdded.ContainsKey(getYID))
            {
                ProductsToBeAutoAdded.Add(getYID, 0);
            }

            ProductsToBeAutoAdded[getYID] += units;
        }


        /// <summary>
        /// Returns null if auto add candidate SKUID is a product with options otherwise Tuple with SKUSellOnlyAvailable and number of available units.
        /// </summary>
        /// <param name="skuID">ID of SKU to check suitability for.</param>
        private static Tuple<bool, int> GetUnitsWhichCanBeAutoAdded(int skuID)
        {
            var sku = SKUInfoProvider.GetSKUInfo(skuID);

            if ((sku == null) || !sku.SKUEnabled || (SKUInfoProvider.HasSKUEnabledOptions(skuID)))
            {
                return null;
            }

            return new Tuple<bool, int>(sku.SKUSellOnlyAvailable, sku.SKUAvailableItems);
        }

        #endregion
    }
}
