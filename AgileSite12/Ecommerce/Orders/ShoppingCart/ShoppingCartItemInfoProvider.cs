using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing ShoppingCartItemInfo management.
    /// </summary>
    public class ShoppingCartItemInfoProvider : AbstractInfoProvider<ShoppingCartItemInfo, ShoppingCartItemInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all shopping cart items.
        /// </summary>
        public static ObjectQuery<ShoppingCartItemInfo> GetShoppingCartItems()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns shopping cart item with specified ID.
        /// </summary>
        /// <param name="itemId">Shopping cart item ID</param>        
        public static ShoppingCartItemInfo GetShoppingCartItemInfo(int itemId)
        {
            return ProviderObject.GetShoppingCartItemInfoInternal(itemId);
        }


        /// <summary>
        /// Returns shopping cart item with specified GUID.
        /// </summary>
        /// <param name="itemGuid">Shopping cart item GUID</param>        
        public static ShoppingCartItemInfo GetShoppingCartItemInfo(Guid itemGuid)
        {
            return ProviderObject.GetShoppingCartItemInfoInternal(itemGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified shopping cart item.
        /// </summary>
        /// <param name="itemObj">Shopping cart item to be set</param>
        public static void SetShoppingCartItemInfo(ShoppingCartItemInfo itemObj)
        {
            ProviderObject.SetShoppingCartItemInfoInternal(itemObj);
        }


        /// <summary>
        /// Deletes specified shopping cart item.
        /// </summary>
        /// <param name="itemObj">Shopping cart item to be deleted</param>
        public static void DeleteShoppingCartItemInfo(ShoppingCartItemInfo itemObj)
        {
            ProviderObject.DeleteShoppingCartItemInfoInternal(itemObj);
        }


        /// <summary>
        /// Deletes shopping cart item with specified ID.
        /// </summary>
        /// <param name="itemId">Shopping cart item ID</param>
        public static void DeleteShoppingCartItemInfo(int itemId)
        {
            var itemObj = GetShoppingCartItemInfo(itemId);
            DeleteShoppingCartItemInfo(itemObj);
        }


        /// <summary>
        /// Deletes shopping cart item with specified GUID.
        /// </summary>
        /// <param name="itemGuid">Shopping cart item GUID</param>
        public static void DeleteShoppingCartItemInfo(Guid itemGuid)
        {
            var itemObj = GetShoppingCartItemInfo(itemGuid);
            DeleteShoppingCartItemInfo(itemObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns shopping cart item created from the order item data.
        /// </summary>
        /// <param name="item">Order item</param>      
        /// <param name="sku">Current data of the product (SKU) which is associated with the order item</param>        
        /// <param name="shoppingCart">Shopping cart that should contain the shopping cart item</param>      
        public static ShoppingCartItemInfo GetShoppingCartItemInfo(OrderItemInfo item, SKUInfo sku, ShoppingCartInfo shoppingCart)
        {
            if (shoppingCart == null)
            {
                throw new ArgumentNullException(nameof(shoppingCart));
            }

            return ProviderObject.GetShoppingCartItemInfoInternal(item, sku, shoppingCart);
        }


        /// <summary>
        /// Returns DataSet with all shopping cart items data of the specified cart. DataSet includes both SKU data and shopping cart item data.
        /// </summary>
        /// <param name="cartId">Shopping cart ID</param>
        public static DataSet GetShoppingCartItems(int cartId)
        {
            return ProviderObject.GetShoppingCartItemsInternal(cartId);
        }


        /// <summary>
        /// Loads shopping cart items into the specified shopping cart. 
        /// If shopping cart is created from an order, cart items are loaded from the corresponding order items.
        /// </summary>
        /// <param name="cart">Shopping cart</param>        
        public static void LoadShoppingCartItems(ShoppingCartInfo cart)
        {
            if (cart != null)
            {
                if (cart.OrderId > 0)
                {
                    // Load items from order
                    ProviderObject.LoadItemsFromOrderInternal(cart);
                }
                else
                {
                    // Load items
                    ProviderObject.LoadItemsFromCartInternal(cart);
                }
            }
        }


        /// <summary>
        /// Checks shopping cart item in the shopping cart.
        /// The following conditions must be met to pass the check:
        /// 1)Shopping cart item is enabled 2)Max units in one order are not exceeded 
        /// 3)There is enough units in the inventory 4) Customer is registered, if it is a membership type product 
        /// 5)Product validity is valid, if it is a membership or e-product type product
        /// </summary>
        /// <param name="item">Shopping cart item to check</param>  
        [Obsolete("Use ValidateShoppingCartItem() instead.")]
        public static ShoppingCartItemCheckResult CheckShoppingCartItem(ShoppingCartItemInfo item)
        {
            return ProviderObject.CheckShoppingCartItemInternal(item);
        }


        /// <summary>
        /// Validates cart item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <returns>Validator containing results of the validation.</returns>
        public static ShoppingCartItemValidator ValidateShoppingCartItem(ShoppingCartItemInfo item)
        {
            return ProviderObject.ValidateShoppingCartItemInternal(item);
        }


        /// <summary>
        /// Returns sum of all units of the specified SKU in the order from which the shopping cart item is loaded.
        /// </summary>
        /// <param name="item">Shopping cart item which is loaded from the order (has order data).</param>        
        public static int GetSKUOrderUnits(ShoppingCartItemInfo item)
        {
            return ProviderObject.GetSKUOrderUnitsInternal(item);
        }


        /// <summary>
        /// Returns sum of all units of the specified SKU in the shopping cart to which the shopping cart item belongs. 
        /// </summary>
        /// <param name="item">Shopping cart item.</param>         
        public static int GetSKUShoppingCartUnits(ShoppingCartItemInfo item)
        {
            return ProviderObject.GetSKUShoppingCartUnitsInternal(item);
        }


        /// <summary>
        /// Returns initialized shopping item cart resolver.
        /// </summary>
        /// <param name="item">Shopping cart item data to be used for resolver initialization</param>        
        public static MacroResolver GetShoppingCartItemResolver(ShoppingCartItemInfo item)
        {
            return ProviderObject.GetShoppingCartItemResolverInternal(item);
        }


        /// <summary>
        /// Updates unit count of shopping cart item, related product option unit count or bundle item count is updated as well.
        /// </summary>
        /// <param name="item">Shopping cart item to updates unit count for.</param>      
        /// <param name="newCount">New number of cart item units.</param>  
        public static void UpdateShoppingCartItemUnits(ShoppingCartItemInfo item, int newCount)
        {
            ProviderObject.UpdateShoppingCartItemUnitsInternal(item, newCount);
        }

        #endregion


        #region "Public methods - Calculations"

        /// <summary>
        /// Calculates the weight of one shopping cart item unit. 'SKUWeight' field value of the corresponding SKU is returned by default.
        /// </summary>
        /// <param name="item">Shopping cart item</param>        
        public static double CalculateUnitWeight(ShoppingCartItemInfo item)
        {
            return ProviderObject.CalculateUnitWeightInternal(item);
        }


        /// <summary>
        /// Calculates the total weight of all shopping cart item units altogether.
        /// </summary>
        /// <param name="item">Shopping cart item</param>        
        public static double CalculateTotalWeight(ShoppingCartItemInfo item)
        {
            return ProviderObject.CalculateTotalWeightInternal(item);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns shopping cart item with specified ID.
        /// </summary>
        /// <param name="itemId">Shopping cart item ID</param>        
        protected virtual ShoppingCartItemInfo GetShoppingCartItemInfoInternal(int itemId)
        {
            return GetInfoById(itemId);
        }


        /// <summary>
        /// Returns shopping cart item with specified GUID.
        /// </summary>
        /// <param name="itemGuid">Shopping cart item GUID</param>        
        protected virtual ShoppingCartItemInfo GetShoppingCartItemInfoInternal(Guid itemGuid)
        {
            return GetInfoByGuid(itemGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified shopping cart item.
        /// </summary>
        /// <param name="itemObj">Shopping cart item to be set</param>        
        protected virtual void SetShoppingCartItemInfoInternal(ShoppingCartItemInfo itemObj)
        {
            if (itemObj == null)
            {
                throw new ArgumentNullException(nameof(itemObj));
            }

            // Set item 
            SetInfo(itemObj);
        }


        /// <summary>
        /// Deletes specified shopping cart item.
        /// </summary>
        /// <param name="itemObj">Shopping cart item to be deleted</param>        
        protected virtual void DeleteShoppingCartItemInfoInternal(ShoppingCartItemInfo itemObj)
        {
            if (itemObj == null)
            {
                return;
            }

            using (var tr = BeginTransaction())
            {
                // Delete product options
                foreach (var option in itemObj.ProductOptions)
                {
                    DeleteShoppingCartItemInfoInternal(option);
                }

                // Remove related bundle items
                foreach (var bundleItem in itemObj.BundleItems)
                {
                    DeleteShoppingCartItemInfoInternal(bundleItem);
                }

                // Delete product
                DeleteInfo(itemObj);

                tr.Commit();
            }
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ShoppingCartItemInfo info)
        {
            using (var transaction = BeginTransaction())
            {
                SaveCachedCart(info);

                base.SetInfo(info);

                transaction.Commit();
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns shopping cart item created from the order item data.
        /// </summary>
        /// <param name="item">Order item</param>        
        /// <param name="sku">Current data of the product (SKU) which is associated with the order item</param>
        /// <param name="shoppingCart">Shopping cart that should contain the shopping cart item</param>    
        protected virtual ShoppingCartItemInfo GetShoppingCartItemInfoInternal(OrderItemInfo item, SKUInfo sku, ShoppingCartInfo shoppingCart)
        {
            // Do not process
            if (item == null)
            {
                return null;
            }

            // Set main properties
            ShoppingCartItemInfo cartItem = new ShoppingCartItemInfo();

            // Item is not assigned to shopping cart yet
            cartItem.ShoppingCartID = 0;

            cartItem.CartItemGUID = item.OrderItemGUID;
            cartItem.CartItemParentGUID = item.OrderItemParentGUID;
            cartItem.CartItemBundleGUID = item.OrderItemBundleGUID;
            cartItem.CartItemUnits = item.OrderItemUnitCount;
            cartItem.UnitPrice = item.OrderItemUnitPrice;
            cartItem.SKUID = item.OrderItemSKUID;
            cartItem.CartItemValidTo = item.OrderItemValidTo;
            cartItem.CartItemText = item.OrderItemText;
            cartItem.OrderItem = item;

            // Set custom data
            cartItem.CartItemCustomData.LoadData(item.OrderItemCustomData.GetData());

            // Restore discount summaries
            cartItem.UnitDiscountSummary = new ValuesSummary(item.OrderItemProductDiscounts);
            cartItem.DiscountSummary = new ValuesSummary(item.OrderItemDiscountSummary);

            // Set custom fields
            foreach (var columnName in item.ColumnNames)
            {
                cartItem.SetValue(columnName, item.GetValue(columnName));
            }

            // Pre-set SKU data to avoid lazy load SKU from DB
            cartItem.SKU = sku;

            // Totals
            cartItem.UnitTotalDiscount = cartItem.UnitDiscountSummary.Sum(d => d.Value);
            cartItem.TotalPrice = item.OrderItemTotalPrice;
            cartItem.TotalDiscount = cartItem.UnitPrice * cartItem.CartItemUnits - cartItem.TotalPrice;


            return cartItem;
        }


        /// <summary>
        /// Returns DataSet with all shopping cart items data of the specified cart. DataSet includes both SKU data and shopping cart item data.
        /// </summary>
        /// <param name="cartId">Shopping cart ID</param>
        protected virtual DataSet GetShoppingCartItemsInternal(int cartId)
        {
            return new ObjectQuery<ShoppingCartItemInfo>()
                .From(new QuerySource("COM_ShoppingCartSKU")
                    .Join("COM_SKU", "COM_ShoppingCartSKU.SKUID", "COM_SKU.SKUID"))
                .Where("ShoppingCartID", QueryOperator.Equals, cartId);
        }


        /// <summary>
        /// Loads shopping cart items into the specified shopping cart. 
        /// Shopping cart items are loaded from standard shopping cart items data.       
        /// </summary>
        /// <param name="cart">Shopping cart</param>        
        protected virtual void LoadItemsFromCartInternal(ShoppingCartInfo cart)
        {
            // Do not process
            if (cart == null)
            {
                return;
            }

            // Remove old items
            cart.CartItems.Clear();

            // Get items data
            DataSet dsItems = GetShoppingCartItems(cart.ShoppingCartID);

            if (!DataHelper.DataSourceIsEmpty(dsItems))
            {
                foreach (DataRow dr in dsItems.Tables[0].Rows)
                {
                    // Create item
                    ShoppingCartItemInfo cartItem = new ShoppingCartItemInfo(dr);
                    cartItem.SKU = new SKUInfo(dr);

                    // Add item to the cart
                    ShoppingCartInfoProvider.AddShoppingCartItem(cart, cartItem);
                }

                InitShoppingCartItems(cart);
            }
        }


        /// <summary>
        /// Loads shopping cart items into the specified shopping cart. 
        /// Shopping cart items are loaded from items of the order from which the shopping cart is created.        
        /// </summary>
        /// <param name="cart">Shopping cart which is created from order data</param>        
        protected virtual void LoadItemsFromOrderInternal(ShoppingCartInfo cart)
        {
            // Do not process
            if (cart == null)
            {
                return;
            }

            // Remove old items
            cart.CartItems.Clear();

            // Get items data
            var items = OrderItemInfoProvider.GetOrderItems(cart.OrderId);

            foreach (var item in items)
            {
                // Create item
                ShoppingCartItemInfo cartItem = GetShoppingCartItemInfo(item, item.OrderItemSKU, cart);

                // Add item to the cart
                ShoppingCartInfoProvider.AddShoppingCartItem(cart, cartItem);
            }

            InitShoppingCartItems(cart);
        }


        /// <summary>
        /// Checks shopping cart item in the shopping cart. If check fails the specified error message is returned, otherwise an empty string is returned.
        /// The following conditions must be met to pass the check:
        /// 1)Shopping cart item is enabled 2)Max units in one order are not exceeded 
        /// 3)There is enough units in the inventory 4) Customer is registered, if it is a membership type product 
        /// 5)Product validity is valid, if it is a membership or e-product type product
        /// </summary>
        /// <param name="item">Shopping cart item to check</param>
        [Obsolete("Use ValidateShoppingCartItemInternal() instead.")]
        protected virtual ShoppingCartItemCheckResult CheckShoppingCartItemInternal(ShoppingCartItemInfo item)
        {
            ShoppingCartItemCheckResult result = new ShoppingCartItemCheckResult();

            // Do not process the check
            if (item?.ShoppingCart == null || (item.SKU == null))
            {
                return result;
            }

            result.CartItem = item;
            // Get up-to-date SKU due to possible inventory change by other users
            SKUInfo sku = SKUInfoProvider.GetSKUInfo(item.SKU.SKUID);

            // Check whether the product is enabled
            result.ProductDisabled = !sku.SKUEnabled;

            // Check whether parent of variant is enabled in case SKU is variant and it is enabled
            if ((!result.ProductDisabled) && (sku.IsProductVariant))
            {
                if (item.VariantParent != null)
                {
                    result.ProductDisabled = !item.VariantParent.SKUEnabled;
                }
            }

            // Check validity for e-products and memberships
            if ((sku.SKUValidUntil != DateTimeHelper.ZERO_TIME) &&
                (sku.SKUValidity == ValidityEnum.Until) &&
                (sku.SKUValidUntil < DateTime.Now) &&
                ((sku.SKUProductType == SKUProductTypeEnum.EProduct) ||
                    (sku.SKUProductType == SKUProductTypeEnum.Membership)))
            {
                result.ProductValidityExpired = true;
            }

            // Min and max units in one order limitations
            int minUnits = sku.SKUMinItemsInOrder;
            int maxUnits = sku.SKUMaxItemsInOrder;
            // Sum of all units of the SKU in the shopping cart
            int cartUnits = GetSKUShoppingCartUnitsInternal(item);

            // Ignore "Min/Max in order" check for bundle items with "remove only bundle" inventory type
            if (!item.IsBundleItem ||
                ((item.ParentBundle?.SKU != null) && (item.ParentBundle.SKU.SKUBundleInventoryType != BundleInventoryTypeEnum.RemoveBundle)))
            {
                // Check max units
                if ((minUnits > 0) && (cartUnits < minUnits))
                {
                    result.MarkMinUnitsFailed(minUnits);
                }

                // Check max units
                if ((maxUnits > 0) && (cartUnits > maxUnits))
                {
                    result.MarkMaxUnitsFailed(maxUnits);
                }
            }

            // Check registered customer for membership product
            if (sku.SKUProductType == SKUProductTypeEnum.Membership)
            {
                ShoppingCartInfo cart = item.ShoppingCart;
                // Public user is unable to purchase membership. Ignore this check for non-existing customer = steps before the one with customer detail
                if ((cart.Customer != null) && (cart.ShoppingCartUserID <= 0))
                {
                    // Get name of the parent bundle
                    if (item.IsBundleItem)
                    {
                        result.CartItem = item.ParentBundle;
                    }

                    result.RegisteredCustomerRequired = true;
                }
            }

            // Check inventory 
            bool checkInventory = false;

            if (sku.SKUProductType == SKUProductTypeEnum.Bundle)
            {
                if ((sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundle) ||
                    (sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                {
                    // Check for bundle
                    checkInventory = sku.SKUSellOnlyAvailable;
                }
            }
            else if (item.IsBundleItem)
            {
                if (item.ParentBundle?.SKU != null)
                {
                    BundleInventoryTypeEnum itemInventory = item.ParentBundle.SKU.SKUBundleInventoryType;

                    if ((itemInventory == BundleInventoryTypeEnum.RemoveProducts) ||
                        (itemInventory == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                    {
                        // Check for bundle item
                        checkInventory = sku.SKUSellOnlyAvailable;

                        result.CartItem = item.ParentBundle;
                    }
                }
            }
            else
            {
                // Check for all other product types
                checkInventory = sku.SKUSellOnlyAvailable;
            }

            // Check available items
            if (checkInventory)
            {
                // Sum of all units of the SKU in the edited order
                int orderUnits = GetSKUOrderUnitsInternal(item);

                // SKU available units
                int itemUnits = (sku.SKUAvailableItems > 0) ? sku.SKUAvailableItems : 0;

                // Sum of all available units
                int inventoryUnits = orderUnits + itemUnits;

                if (cartUnits > inventoryUnits)
                {
                    result.MarkNotEnoughUnits(inventoryUnits);
                }
            }

            return result;
        }


        /// <summary>
        /// Validates cart item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <returns>Validator containing results of the validation.</returns>
        protected virtual ShoppingCartItemValidator ValidateShoppingCartItemInternal(ShoppingCartItemInfo item)
        {
            var validator = new ShoppingCartItemValidator(item);

            validator.Validate();

            return validator;
        }


        /// <summary>
        /// Updates unit count of shopping cart item, related product option unit count or bundle item count is updated as well.
        /// </summary>
        /// <param name="item">Shopping cart item to updates unit count for.</param>      
        /// <param name="newCount">New number of cart item units.</param>  
        protected virtual void UpdateShoppingCartItemUnitsInternal(ShoppingCartItemInfo item, int newCount)
        {
            // All units are added by user
            item.CartItemAutoAddedUnits = 0;

            bool updateInDatabase = (item.ShoppingCart.ShoppingCartID > 0);

            // Updates count of main product
            UpdateItemCount(item, newCount, updateInDatabase);

            // Update units of the child product options
            foreach (var option in item.ProductOptions)
            {
                UpdateItemCount(option, newCount, updateInDatabase);
            }

            // Update units of child bundle items
            foreach (var bundleItem in item.BundleItems)
            {
                UpdateItemCount(bundleItem, newCount, updateInDatabase);
            }
        }


        /// <summary>
        /// Returns sum of all units of the specified SKU in the order from which the shopping cart item is loaded.
        /// </summary>
        /// <param name="item">Shopping cart item which is loaded from the order (has order data).</param>        
        protected virtual int GetSKUOrderUnitsInternal(ShoppingCartItemInfo item)
        {
            return GetCartUnitsCount(item, true);
        }


        /// <summary>
        /// Returns sum of all units of the specified SKU in the shopping cart to which the shopping cart item belongs.
        /// </summary>
        /// <param name="item">Shopping cart item.</param>    
        protected virtual int GetSKUShoppingCartUnitsInternal(ShoppingCartItemInfo item)
        {
            return GetCartUnitsCount(item, false);
        }


        private int GetCartUnitsCount(ShoppingCartItemInfo item, bool orderCounts)
        {
            if (item == null)
            {
                return 0;
            }
            // Include variants of same parent for "ByProduct" inventory tracking
            var includeVariantsOfSameParent = item.SKU.IsProductVariant && (item.SKU.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct);
            return GetCartUnitsCount(item, includeVariantsOfSameParent, orderCounts);
        }


        /// <summary>
        /// Gets the cart units count for specified cart item.
        /// </summary>
        /// <param name="item">The cart item.</param>
        /// <param name="includeVariantsOfSameParent">if set to true variants of same parent are included to result.</param>
        /// <param name="orderCounts">if set to true order counts are evaluating.</param>
        private int GetCartUnitsCount(ShoppingCartItemInfo item, bool includeVariantsOfSameParent, bool orderCounts)
        {
            // Do not calculate
            if (item?.ShoppingCart == null)
            {
                return 0;
            }

            int skuId = item.SKU.SKUID;
            int sumOfSKUUnits = 0;

            foreach (var cartItem in item.ShoppingCart.CartItems)
            {
                var isVariantOfSameParent = includeVariantsOfSameParent && (cartItem.VariantParent == item.VariantParent);
                var isSameSKU = cartItem.SKUID == skuId;

                if (isSameSKU || isVariantOfSameParent)
                {
                    // Do not count bundle item units if only parent bundle is removed from inventory
                    if (cartItem.IsBundleItem
                        && (cartItem.ParentBundle?.SKU != null)
                        && (cartItem.ParentBundle.SKU.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundle))
                    {
                        continue;
                    }

                    if (orderCounts)
                    {
                        // Get units from existing order item
                        var orderUnits = cartItem.OrderItem?.OrderItemUnitCount ?? 0;
                        // Add units
                        sumOfSKUUnits += orderUnits;
                    }
                    else
                    {
                        sumOfSKUUnits += cartItem.CartItemUnits;
                    }
                }
            }

            return sumOfSKUUnits;
        }


        /// <summary>
        /// Returns initialized shopping item cart resolver.
        /// </summary>
        /// <param name="item">Shopping cart item data to be used for resolver initialization</param>        
        protected virtual MacroResolver GetShoppingCartItemResolverInternal(ShoppingCartItemInfo item)
        {
            MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();
            resolver.SetNamedSourceData("ShoppingCartItem", item);

            return resolver;
        }

        #endregion


        #region "Internal methods - Calculations"

        /// <summary>
        /// Calculates the weight of one shopping cart item unit. 'SKUWeight' field value of the corresponding SKU is returned by default.
        /// </summary>
        /// <param name="item">Shopping cart item</param>        
        protected virtual double CalculateUnitWeightInternal(ShoppingCartItemInfo item)
        {
            if (item?.SKU != null)
            {
                // Get standard SKU unit weight
                return item.SKU.SKUWeight;
            }

            return 0d;
        }


        /// <summary>
        /// Calculates the total weight of all shopping cart item units altogether.
        /// </summary>
        /// <param name="item">Shopping cart item</param>        
        protected virtual double CalculateTotalWeightInternal(ShoppingCartItemInfo item)
        {
            return item.UnitWeight * item.CartItemUnits;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Updates number of units in shopping cart item.
        /// </summary>
        /// <param name="item">Item which units should be updated</param>
        /// <param name="count">Number of units</param>
        /// <param name="updateInDatabase">If true, item is updated in database</param>
        private static void UpdateItemCount(ShoppingCartItemInfo item, int count, bool updateInDatabase)
        {
            item.CartItemUnits = count;

            if (updateInDatabase)
            {
                SetShoppingCartItemInfo(item);
            }
        }


        /// <summary>
        /// Sets product options of each parent cart item and puts all cart items to the proper order (parent1-parent1 children, parent2- parent2 children,...).
        /// </summary>
        /// <param name="cart">Shopping cart</param> 
        private static void InitShoppingCartItems(ShoppingCartInfo cart)
        {
            var cartItems = cart?.CartItems;
            if (cartItems == null)
            {
                return;
            }

            var resultItems = new List<ShoppingCartItemInfo>();

            int i = 0;
            while (i < cartItems.Count)
            {
                // If shopping cart item is not a product option or bundle item
                if (!cartItems[i].IsProductOption && !cartItems[i].IsBundleItem)
                {
                    // Add parent product to the new list
                    resultItems.Add(cartItems[i]);

                    // Remove parent product from the original list
                    cartItems.Remove(cartItems[i]);
                }
                else
                {
                    i++;
                }
            }

            i = 0;
            while (i < resultItems.Count)
            {
                // Parent product
                ShoppingCartItemInfo parent = resultItems[i];

                if (parent.SKU == null)
                {
                    continue;
                }


                foreach (ShoppingCartItemInfo child in cartItems)
                {
                    // If shopping cart item is a product option or bundle item of the parent product
                    if ((child.CartItemParentGUID == parent.CartItemGUID) || (child.CartItemBundleGUID == parent.CartItemGUID))
                    {
                        // Insert product after the parent product
                        resultItems.Insert(++i, child);

                        // Assign product to the parent product
                        if (child.IsBundleItem)
                        {
                            parent.BundleItems.Add(child);
                        }
                        else
                        {
                            parent.ProductOptions.Add(child);
                        }
                    }
                }
                i++;
            }

            // Update original list of shopping cart items
            cartItems.Clear();
            cartItems.AddRange(resultItems);
        }


        private static void SaveCachedCart(ShoppingCartItemInfo info)
        {
            var cachedCart = Service.Resolve<IShoppingCartCache>().GetCart();
            if (cachedCart != null && info.Parent != null && cachedCart.Generalized.ObjectID == info.Parent.Generalized.ObjectID)
            {
                cachedCart.Generalized.SetObject();
            }
        }

        #endregion
    }
}