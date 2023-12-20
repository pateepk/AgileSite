using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing ShoppingCartInfo management.
    /// </summary>
    public class ShoppingCartInfoProvider : AbstractInfoProvider<ShoppingCartInfo, ShoppingCartInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all shopping carts.
        /// </summary>
        public static ObjectQuery<ShoppingCartInfo> GetShoppingCarts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns shopping cart with specified ID.
        /// </summary>
        /// <param name="cartId">Shopping cart ID</param>
        public static ShoppingCartInfo GetShoppingCartInfo(int cartId)
        {
            return ProviderObject.GetShoppingCartInfoInternal(cartId);
        }


        /// <summary>
        /// Returns shopping cart with specified GUID.
        /// </summary>
        /// <param name="cartGuid">Shopping cart GUID</param>
        public static ShoppingCartInfo GetShoppingCartInfo(Guid cartGuid)
        {
            return ProviderObject.GetShoppingCartInfoInternal(cartGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart to be set</param>
        public static void SetShoppingCartInfo(ShoppingCartInfo cart)
        {
            ProviderObject.SetShoppingCartInfoInternal(cart);
        }


        /// <summary>
        /// Deletes specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart to be deleted</param>
        public static void DeleteShoppingCartInfo(ShoppingCartInfo cart)
        {
            ProviderObject.DeleteShoppingCartInfoInternal(cart);
        }


        /// <summary>
        /// Deletes shopping cart with specified ID.
        /// </summary>
        /// <param name="cartId">Shopping cart ID</param>
        public static void DeleteShoppingCartInfo(int cartId)
        {
            var cart = GetShoppingCartInfo(cartId);
            DeleteShoppingCartInfo(cart);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all shopping carts for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<ShoppingCartInfo> GetShoppingCarts(int siteId)
        {
            return ProviderObject.GetShoppingCartsInternal(siteId);
        }


        /// <summary>
        /// Returns shopping cart of the specified user.
        /// </summary>
        /// <param name="userId">ID of the user who is the owner of the shopping cart</param>
        /// <param name="siteName">Name of the site to which the shopping cart belongs</param>
        public static ShoppingCartInfo GetShoppingCartInfo(int userId, string siteName)
        {
            return ProviderObject.GetShoppingCartInfoInternal(userId, siteName);
        }


        /// <summary>
        /// Returns ShoppingCartInfo object created from the order data.
        /// </summary>
        /// <param name="orderId">ID of the order from which the shopping cart should be created</param>
        public static ShoppingCartInfo GetShoppingCartInfoFromOrder(int orderId)
        {
            return ProviderObject.GetShoppingCartInfoFromOrderInternal(orderId);
        }


        /// <summary>
        /// Deletes shopping cart with specified GUID.
        /// </summary>
        /// <param name="cartGuid">Shopping cart GUID</param>
        public static void DeleteShoppingCartInfo(Guid cartGuid)
        {
            var cart = GetShoppingCartInfo(cartGuid);
            DeleteShoppingCartInfo(cart);
        }


        /// <summary>
        /// Deletes shopping cart of the specified user.
        /// </summary>
        /// <param name="userId">ID of the user who is the owner of the shopping cart</param>
        /// <param name="siteName">Name of the site to which the shopping cart belongs</param>
        public static void DeleteShoppingCartInfo(int userId, string siteName)
        {
            var cart = GetShoppingCartInfo(userId, siteName);
            DeleteShoppingCartInfo(cart);
        }


        /// <summary>
        /// Deletes all items of the specified shopping cart.
        /// </summary>
        /// <param name="cartId">Shopping cart ID</param>
        public static void DeleteShoppingCartItems(int cartId)
        {
            ProviderObject.DeleteShoppingCartItemsInternal(cartId);
        }


        /// <summary>
        /// Removes all items from specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart object</param>
        public static void EmptyShoppingCart(ShoppingCartInfo cart)
        {
            ProviderObject.EmptyShoppingCartInternal(cart);
        }


        /// <summary>
        /// Creates or updates order based on the data of the specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when ShoppingCartCustomerID and/or ShoppingCartCurrencyID not set,
        /// or when strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so (<see cref="SKUInfo.SKUSellOnlyAvailable"/> is true).
        /// </exception>
        public static void SetOrder(ShoppingCartInfo cart)
        {
            ProviderObject.SetOrderInternal(cart, true);
        }


        /// <summary>
        /// Creates or updates order based on the data of the specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="generateInvoice">Indicates if new invoice should be generated from the new order data. It is True by default.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when ShoppingCartCustomerID and/or ShoppingCartCurrencyID not set,
        /// or when strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so (<see cref="SKUInfo.SKUSellOnlyAvailable"/> is true).
        /// </exception>
        public static void SetOrder(ShoppingCartInfo cart, bool generateInvoice)
        {
            ProviderObject.SetOrderInternal(cart, generateInvoice);
        }


        /// <summary>
        /// Creates or updates order items based on the items of the specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        public static void SetOrderItems(ShoppingCartInfo cart)
        {
            ProviderObject.SetOrderItemsInternal(cart);
        }


        /// <summary>
        /// Checks shopping cart items.
        /// The following conditions must be met to pass the check:
        /// 1)All shopping cart items are enabled 2)Max units in one order are not exceeded
        /// 3)There is enough units in the inventory 4) Customer is registered, if there is a membership type product in the cart
        /// 5)Product validity is valid, if there is a membership or e-product type product in the cart
        /// </summary>
        /// <param name="cart">Shopping cart to check</param>
        [Obsolete("Use ValidateShoppingCart() instead.")]
        public static ShoppingCartCheckResult CheckShoppingCart(ShoppingCartInfo cart)
        {
            return ProviderObject.CheckShoppingCartInternal(cart);
        }


        /// <summary>
        /// Validates shopping cart and all its items. Returns an empty collection in case validation passes.
        /// </summary>
        /// <param name="cart">Shopping cart to validate.</param>
        /// <returns>Returns collection of validation errors that occured.</returns>
        public static IEnumerable<IValidationError> ValidateShoppingCart(ShoppingCartInfo cart)
        {
            return ProviderObject.ValidateShoppingCartInternal(cart);
        }

        #endregion


        #region "Public methods - Calculations"

        /// <summary>
        /// Recalculates shopping cart content table.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        public static IEnumerable<ShoppingCartLine> EvaluateContent(ShoppingCartInfo cart)
        {
            lock (cart)
            {
                return ProviderObject.EvaluateContentInternal(cart);
            }
        }


        /// <summary>
        /// Calculates total weight of all shopping cart items.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        public static double CalculateTotalItemsWeight(ShoppingCartInfo cart)
        {
            return ProviderObject.CalculateTotalItemsWeightInternal(cart);
        }

        #endregion


        #region "Public methods - Cart items actions"

        /// <summary>
        /// Adds the product (and its product options) specified in the <paramref name="itemParams"/> to the specified <paramref name="cart"/> object.
        /// If such product configuration already exists in the shopping cart,
        /// only the <see cref="ShoppingCartItemInfo.CartItemUnits"/> property is increased by <see cref="ShoppingCartItemParameters.Quantity"/>.
        /// The added/updated shopping cart item is returned.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="itemParams">Parameters of the shopping cart item which should be used to perform the adding or updating.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cart"/> or <paramref name="itemParams"/> are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="itemParams"/> specify a non-positive <see cref="ShoppingCartItemParameters.SKUID"/>.</exception>
        public static ShoppingCartItemInfo SetShoppingCartItem(ShoppingCartInfo cart, ShoppingCartItemParameters itemParams)
        {
            return ProviderObject.SetShoppingCartItemInternal(cart, itemParams);
        }


        /// <summary>
        /// Returns shopping cart item from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartItemId">Shopping cart item ID</param>
        public static ShoppingCartItemInfo GetShoppingCartItem(ShoppingCartInfo cart, int cartItemId)
        {
            return ProviderObject.GetShoppingCartItemInternal(cart, cartItemId);
        }


        /// <summary>
        /// Returns shopping cart item from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartItemGuid">Shopping cart item GUID</param>
        public static ShoppingCartItemInfo GetShoppingCartItem(ShoppingCartInfo cart, Guid cartItemGuid)
        {
            return ProviderObject.GetShoppingCartItemInternal(cart, cartItemGuid);
        }


        /// <summary>
        /// Removes shopping cart item and its product options from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartItemId">Shopping cart item ID</param>
        public static void RemoveShoppingCartItem(ShoppingCartInfo cart, int cartItemId)
        {
            // Get item
            var cartItem = ProviderObject.GetShoppingCartItemInternal(cart, cartItemId);

            // Remove item
            ProviderObject.RemoveShoppingCartItemInternal(cart, cartItem);
        }


        /// <summary>
        /// Removes shopping cart item and its product options from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartItemGuid">Shopping cart item GUID</param>
        public static void RemoveShoppingCartItem(ShoppingCartInfo cart, Guid cartItemGuid)
        {
            // Get item
            var cartItem = ProviderObject.GetShoppingCartItemInternal(cart, cartItemGuid);

            // Remove item
            ProviderObject.RemoveShoppingCartItemInternal(cart, cartItem);
        }


        /// <summary>
        /// Removes shopping cart item, related bundle items and its product options from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="item">Shopping cart item</param>
        public static void RemoveShoppingCartItem(ShoppingCartInfo cart, ShoppingCartItemInfo item)
        {
            ProviderObject.RemoveShoppingCartItemInternal(cart, item);
        }


        /// <summary>
        /// Adds new initialized item to the shopping cart items collection.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="item">Shopping cart item</param>
        public static void AddShoppingCartItem(ShoppingCartInfo cart, ShoppingCartItemInfo item)
        {
            ProviderObject.AddShoppingCartItemInternal(cart, item);
        }


        /// <summary>
        /// Returns sum of all shopping cart items' units, bundle items and product options are not included.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        public static int GetTotalUnits(ShoppingCartInfo cart)
        {
            return ProviderObject.GetTotalUnitsInternal(cart);
        }


        /// <summary>
        /// Updates shopping cart with shopping cart from order. If such product configuration already exists in the shopping cart, only  its quantity is updated.
        /// Returns true if all ordered items were successfully added to cart.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="orderID">ID of order which should be added to shopping cart.</param>
        public static bool UpdateShoppingCartFromOrder(ShoppingCartInfo cart, int orderID)
        {
            return ProviderObject.UpdateShoppingCartFromOrderInternal(cart, orderID);
        }


        /// <summary>
        /// Updates destination shopping cart with source shopping cart.
        /// <para>If such product configuration already exists in the shopping cart, only its quantity is updated.</para>
        /// </summary>
        /// <param name="sourceCart">Original shopping cart</param>
        /// <param name="destinationCart">The cart where to copy items</param>
        /// <returns>Returns true if all ordered items were successfully added to cart.</returns>
        public static bool CopyShoppingCartItems(ShoppingCartInfo sourceCart, ShoppingCartInfo destinationCart)
        {
            return ProviderObject.CopyShoppingCartItemsInternal(sourceCart, destinationCart);
        }

        #endregion


        #region "Public methods - Resolver"

        /// <summary>
        /// Returns initialized shopping cart resolver.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        public static MacroResolver GetShoppingCartResolver(ShoppingCartInfo cart)
        {
            return ProviderObject.GetShoppingCartResolverInternal(cart);
        }


        /// <summary>
        /// Returns the HTML code for the order invoice.
        /// </summary>
        /// <param name="cart">Shopping cart data used for generating invoice</param>
        public static string GetOrderInvoice(ShoppingCartInfo cart)
        {
            return ProviderObject.GetOrderInvoiceInternal(cart);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns shopping cart with specified ID.
        /// </summary>
        /// <param name="cartId">Shopping cart ID</param>
        protected virtual ShoppingCartInfo GetShoppingCartInfoInternal(int cartId)
        {
            // Get shopping cart
            var cart = GetInfoById(cartId);

            // Load cart items
            ShoppingCartItemInfoProvider.LoadShoppingCartItems(cart);

            return cart;
        }


        /// <summary>
        /// Returns shopping cart with specified GUID.
        /// </summary>
        /// <param name="cartGuid">Shopping cart GUID</param>
        protected virtual ShoppingCartInfo GetShoppingCartInfoInternal(Guid cartGuid)
        {
            // Get cart
            var cart = GetInfoByGuid(cartGuid);

            // Load cart items
            ShoppingCartItemInfoProvider.LoadShoppingCartItems(cart);

            return cart;
        }


        /// <summary>
        /// Sets (updates or inserts) specified shopping cart.
        /// </summary>
        /// <param name="cartObj">Shopping cart to be set</param>
        protected virtual void SetShoppingCartInfoInternal(ShoppingCartInfo cartObj)
        {
            if (cartObj == null)
            {
                throw new ArgumentNullException(nameof(cartObj));
            }

            // If shopping cart object was restored from session and in the meantime was user deleted.
            if (cartObj.User == null)
            {
                cartObj.SetValue("ShoppingCartUserID", DBNull.Value);
            }

            if (cartObj.Customer == null)
            {
                cartObj.SetValue("ShoppingCartCustomerID", DBNull.Value);
            }

            // Assign current contact if not set
            if (cartObj.ShoppingCartContactID <= 0)
            {
                cartObj.ShoppingCartContactID = ModuleCommands.OnlineMarketingGetCurrentContactID();
            }

            // Set cart
            SetInfo(cartObj);
        }


        /// <summary>
        /// Deletes specified shopping cart.
        /// </summary>
        /// <param name="cartObj">Shopping cart to be deleted</param>
        protected virtual void DeleteShoppingCartInfoInternal(ShoppingCartInfo cartObj)
        {
            DeleteInfo(cartObj);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all shopping carts for specified site matching the specified parameters.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<ShoppingCartInfo> GetShoppingCartsInternal(int siteId)
        {
            return GetShoppingCarts().OnSite(siteId);
        }


        /// <summary>
        /// Returns shopping cart of the specified user.
        /// </summary>
        /// <param name="userId">ID of the user who is the owner of the shopping cart</param>
        /// <param name="siteName">Name of the site to which the shopping cart belongs</param>
        protected virtual ShoppingCartInfo GetShoppingCartInfoInternal(int userId, string siteName)
        {
            // Get cart
            var condition = new WhereCondition()
                .WhereEquals("ShoppingCartUserID", userId)
                .WhereEquals("ShoppingCartSiteID", SiteInfoProvider.GetSiteID(siteName));

            var cart = GetObjectQuery().TopN(1).Where(condition).OrderByDescending("ShoppingCartLastUpdate").FirstOrDefault();

            // Load cart items
            ShoppingCartItemInfoProvider.LoadShoppingCartItems(cart);

            return cart;
        }


        /// <summary>
        /// Returns ShoppingCartInfo object created from the order data.
        /// </summary>
        /// <param name="orderId">ID of the order from which the shopping cart should be created</param>
        protected virtual ShoppingCartInfo GetShoppingCartInfoFromOrderInternal(int orderId)
        {
            // Get order data
            var orderObj = OrderInfoProvider.GetOrderInfo(orderId);

            if (orderObj == null)
            {
                return null;
            }

            // Create shopping cart
            var cart = ShoppingCartFactory.CreateCart(orderObj.OrderSiteID);

            cart.IsCreatedFromOrder = true;

            // Set main properties
            cart.OrderId = orderObj.OrderID;
            cart.Order = orderObj;

            // Set coupon code and currency before cart items are loaded
            cart.CouponCodes = CouponCodeCollection.Deserialize(orderObj.OrderCouponCodes);
            cart.ShoppingCartCurrencyID = orderObj.OrderCurrencyID;

            // Load cart items before shipping tax is calculated
            ShoppingCartItemInfoProvider.LoadShoppingCartItems(cart);

            var addressConverter = Service.Resolve<IAddressConverter>();

            cart.ShoppingCartBillingAddress = addressConverter.Convert(orderObj.OrderBillingAddress);
            cart.ShoppingCartShippingAddress = addressConverter.Convert(orderObj.OrderShippingAddress);
            cart.ShoppingCartCompanyAddress = addressConverter.Convert(orderObj.OrderCompanyAddress);
            cart.ShoppingCartCustomerID = orderObj.OrderCustomerID;
            cart.ShoppingCartPaymentOptionID = orderObj.OrderPaymentOptionID;
            cart.ShoppingCartShippingOptionID = orderObj.OrderShippingOptionID;
            cart.ShoppingCartNote = orderObj.OrderNote;
            cart.ShoppingCartSiteID = orderObj.OrderSiteID;
            cart.ShoppingCartCulture = orderObj.OrderCulture;

            // Totals
            cart.GrandTotal = orderObj.OrderGrandTotal;
            cart.TotalPrice = orderObj.OrderTotalPrice;
            cart.TotalShipping = orderObj.OrderTotalShipping;
            cart.TotalTax = orderObj.OrderTotalTax;

            if (cart.Customer != null)
            {
                cart.User = UserInfoProvider.GetUserInfo(cart.Customer.CustomerUserID);
            }

            // Set custom fields
            foreach (var columnName in orderObj.ColumnNames)
            {
                cart.SetValue(columnName, orderObj.GetValue(columnName));
            }

            // Set custom data
            cart.ShoppingCartCustomData.LoadData(orderObj.OrderCustomData.GetData());

            // Set summary data
            cart.OtherPaymentsSummary = new ValuesSummary(orderObj.OrderOtherPayments);
            cart.TaxSummary = new ValuesSummary(orderObj.OrderTaxSummary);
            cart.OrderDiscountSummary = new ValuesSummary(orderObj.OrderDiscounts);

            // Calculate values missing in the order
            cart.TotalItemsPrice = cart.CartItems.Sum(item => item.TotalPrice);
            cart.OtherPayments = cart.OtherPaymentsSummary.Sum(x => x.Value);
            cart.OrderDiscount = cart.OrderDiscountSummary.Sum(x => x.Value);

            return cart;
        }


        /// <summary>
        /// Deletes items of the specified shopping cart.
        /// </summary>
        /// <param name="cartId">Shopping cart ID</param>
        protected virtual void DeleteShoppingCartItemsInternal(int cartId)
        {
            ShoppingCartItemInfoProvider.GetShoppingCartItems()
                .WhereEquals("ShoppingCartID", cartId)
                .ToList()
                .ForEach(ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo);
        }


        /// <summary>
        /// Removes all items from specified shopping cart object and from the shopping cart in database.
        /// </summary>
        /// <param name="cart">Shopping cart object</param>
        protected virtual void EmptyShoppingCartInternal(ShoppingCartInfo cart)
        {
            if (cart == null)
            {
                return;
            }

            cart.CouponCodes = null;

            while (!cart.IsEmpty)
            {
                // Get first item
                var item = cart.CartItems[0];

                // Remove product and its product option
                RemoveShoppingCartItem(cart, item.CartItemGUID);

                if (item.CartItemID > 0)
                {
                    // Delete item from database
                    ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(item);
                }
            }
        }


        /// <summary>
        /// Creates or updates order based on the data of the specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="generateInvoice">Indicates if new invoice should be generated from the new order data. It is True by default.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when ShoppingCartCustomerID and/or ShoppingCartCurrencyID not set,
        /// or when strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so (<see cref="SKUInfo.SKUSellOnlyAvailable"/> is true).
        /// </exception>
        protected virtual void SetOrderInternal(ShoppingCartInfo cart, bool generateInvoice)
        {
            if (cart.ShoppingCartCustomerID == 0)
            {
                throw new InvalidOperationException("[ShoppingCartInfoProvider.SetOrderInternal]: Shopping cart customer not set.");
            }

            if (cart.ShoppingCartCurrencyID <= 0)
            {
                throw new InvalidOperationException("[ShoppingCartInfoProvider.SetOrderInternal]: Shopping cart currency not set.");
            }

            bool newOrder = (cart.OrderId <= 0);
            var status = newOrder ? OrderStatusInfoProvider.GetFirstEnabledStatus(cart.ShoppingCartSiteID) : null;

            // Get order object
            OrderInfo order = OrderInfoProvider.GetOrderInfo(cart.OrderId) ?? new OrderInfo();

            // Set order base parameters
            order.OrderCustomerID = cart.ShoppingCartCustomerID;
            order.OrderCurrencyID = cart.ShoppingCartCurrencyID;
            order.OrderPaymentOptionID = cart.ShoppingCartPaymentOptionID;
            order.OrderShippingOptionID = cart.ShoppingCartShippingOptionID;
            order.OrderSiteID = cart.ShoppingCartSiteID;
            order.OrderNote = cart.ShoppingCartNote;
            order.OrderCouponCodes = cart.CouponCodes.Serialize();
            order.OrderCustomData.LoadData(cart.ShoppingCartCustomData.GetData());
            order.OrderCulture = cart.ShoppingCartCulture;

            // Exchange rate from cart currency to main currency
            var mainCurrency = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrency(cart.ShoppingCartSiteID);

            // Set order totals
            order.OrderGrandTotal = cart.GrandTotal;
            order.OrderTotalPrice = cart.TotalPrice;
            order.OrderTotalShipping = cart.TotalShipping;
            order.OrderTotalTax = cart.TotalTax;
            order.OrderTaxSummary = cart.TaxSummary?.GetSummaryXml();

            // Store some totals in the main currency
            var roundingService = GetRoundingService(cart.ShoppingCartSiteID);
            var rateToMainCurrency = CurrencyConverter.GetExchangeRate(cart.Currency.CurrencyCode, mainCurrency.CurrencyCode, cart.ShoppingCartSiteID);

            order.OrderGrandTotalInMainCurrency = roundingService.Round(CurrencyConverter.ApplyExchangeRate(cart.GrandTotal, rateToMainCurrency), mainCurrency);
            order.OrderTotalPriceInMainCurrency = roundingService.Round(CurrencyConverter.ApplyExchangeRate(cart.TotalPrice, rateToMainCurrency), mainCurrency);

            // Set order custom parameters
            foreach (var columnName in cart.ColumnNames)
            {
                order.SetValue(columnName, cart.GetValue(columnName));
            }

            if (newOrder)
            {
                // Set order user
                order.OrderCompletedByUserID = cart.ShoppingCartUserID;
            }

            using (var tr = BeginTransaction())
            {
                cart.CouponCodes.Apply();

                // Other payments
                order.OrderCouponCodes = cart.CouponCodes.Serialize();
                order.OrderOtherPayments = cart.OtherPaymentsSummary?.GetSummaryXml();
                order.OrderDiscounts = cart.OrderDiscountSummary?.GetSummaryXml();

                // Create order
                OrderInfoProvider.SetOrderInfo(order);

                // Update order ID in shopping cart
                cart.OrderId = order.OrderID;
                cart.Order = order;

                // Create order items
                SetOrderItems(cart);

                if (newOrder)
                {
                    // Create order addresses
                    SetOrderAddresses(cart);

                    // Set invoice number (default invoice number contains OrderID -> order is set before invoice number is generated)
                    order.OrderInvoiceNumber = OrderInfoProvider.GenerateInvoiceNumber(cart);

                    // Set order status after creation of order items to ensure that status changed notification email has all data
                    order.OrderStatusID = status?.StatusID ?? 0;
                }

                if (generateInvoice)
                {
                    order.OrderInvoice = GetOrderInvoice(cart);
                }

                // Save invoice changes
                if (newOrder || generateInvoice)
                {
                    OrderInfoProvider.SetOrderInfo(order);
                }

                // Update inventory
                SKUInfoProvider.UpdateInventory(cart);

                // Commit changes
                tr.Commit();
            }

            // Fire event for newly saved order
            if (newOrder && (order.OrderID > 0))
            {
                EcommerceEvents.NewOrderCreated.StartEvent(order);
            }
        }


        /// <summary>
        /// Creates or updates order items based on the items of the specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        protected virtual void SetOrderItemsInternal(ShoppingCartInfo cart)
        {
            // Do not process
            if ((cart == null) || (cart.OrderId <= 0))
            {
                return;
            }

            // Get dataset with all order items
            var items = OrderItemInfoProvider.GetOrderItems(cart.OrderId);

            if (!DataHelper.DataSourceIsEmpty(items))
            {
                foreach (var orderItem in items)
                {
                    // Get delete order item
                    bool updateInventory = false;

                    // Check if item was totally removed from shopping cart
                    if (!cart.CartItems.Exists(ci => ci.CartItemGUID == orderItem.OrderItemGUID))
                    {
                        var sku = SKUInfoProvider.GetSKUInfo(orderItem.OrderItemSKUID);
                        if (sku != null)
                        {
                            // Check inventory settings for bundle products
                            if (sku.SKUProductType == SKUProductTypeEnum.Bundle)
                            {
                                if ((sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundle) ||
                                    (sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                                {
                                    // Update bundle
                                    updateInventory = true;
                                }
                            }
                            // Check inventory settings for bundle items
                            else if (orderItem.OrderItemBundleGUID != Guid.Empty)
                            {
                                // Find parent bundle product
                                var parentBundle = items.Where(i => i.OrderItemGUID == orderItem.OrderItemBundleGUID).ToList();

                                if (parentBundle.Any())
                                {
                                    var parentBundleSKU = parentBundle.First().OrderItemSKU;

                                    // Check inventory settings for parent bundle product
                                    if ((parentBundleSKU.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveProducts) ||
                                        (parentBundleSKU.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                                    {
                                        // Update bundle item
                                        updateInventory = true;
                                    }

                                }
                            }
                            else
                            {
                                // Track if this sku is variant and track by variants is enabled
                                bool trackForThisVariant = sku.IsProductVariant && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByVariants);
                                // Track if this sku is product and track by products is enabled
                                bool trackForThisProduct = !sku.IsProductVariant && !sku.IsProductOption && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct);
                                // Track if this sku is variant and track by variants is enabled
                                bool trackForParentProduct = sku.IsProductVariant && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct);
                                // Track if this sku is accessory and track by product is enabled
                                bool trackForAccessory = sku.IsAccessoryProduct && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct);

                                if (trackForThisVariant || trackForThisProduct || trackForParentProduct || trackForAccessory)
                                {
                                    updateInventory = true;
                                }
                            }
                        }
                    }

                    // Clear old order items
                    OrderItemInfoProvider.DeleteOrderItemInfo(orderItem, updateInventory);
                }
            }

            // Create fresh order items from cart again
            foreach (var cartItem in cart.CartItems)
            {
                // Create order item from cart item
                var orderItem = OrderItemInfoProvider.GetOrderItemInfo(cartItem);

                // Save order item
                OrderItemInfoProvider.SetOrderItemInfo(orderItem);
            }
        }


        /// <summary>
        /// Checks shopping cart items.
        /// The following conditions must be met to pass the check:
        /// 1)All shopping cart items are enabled 2)Max units in one order are not exceeded
        /// 3)There is enough units in the inventory 4) Customer is registered, if there is a membership type product in the cart
        /// 5)Product validity is valid, if there is a membership or e-product type product in the cart
        /// </summary>
        /// <param name="cart">Shopping cart to check</param>
        [Obsolete("Use ValidateShoppingCartInternal() instead.")]
        protected virtual ShoppingCartCheckResult CheckShoppingCartInternal(ShoppingCartInfo cart)
        {
            var cartResult = new ShoppingCartCheckResult();

            // Check each item
            foreach (var item in cart.CartItems)
            {
                var itemResult = ShoppingCartItemInfoProvider.CheckShoppingCartItem(item);

                // Add cart item result to car result
                cartResult.AddCartItemResult(itemResult);
            }

            // Return check result
            return cartResult;
        }


        /// <summary>
        /// Validates shopping cart and all its items. Returns an empty collection in case validation passes.
        /// </summary>
        /// <param name="cart">Shopping cart to validate.</param>
        /// <returns>Returns collection of validation errors that occured.</returns>
        protected virtual IEnumerable<IValidationError> ValidateShoppingCartInternal(ShoppingCartInfo cart)
        {
            var validator = new ShoppingCartValidator(cart);

            validator.Validate();

            return validator.Errors;
        }

        #endregion


        #region "Internal methods - Calculations"

        /// <summary>
        /// Recalculates shopping cart content table.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        protected virtual IEnumerable<ShoppingCartLine> EvaluateContentInternal(ShoppingCartInfo cart)
        {
            return cart.CartContentItems.Select(item => new ShoppingCartLine(item)).ToList();
        }


        /// <summary>
        /// Calculates total weight of all shopping cart items.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        protected virtual double CalculateTotalItemsWeightInternal(ShoppingCartInfo cart)
        {
            // Get sum of all total weights
            return cart.CartItems.Sum(item => item.TotalWeight);
        }

        #endregion


        #region "Internal methods - Cart items actions"

        /// <summary>
        /// Adds the product (and its product options) specified in the <paramref name="itemParams"/> to the specified <paramref name="cart"/> object.
        /// If such product configuration already exists in the shopping cart,
        /// only the <see cref="ShoppingCartItemInfo.CartItemUnits"/> property is increased by <see cref="ShoppingCartItemParameters.Quantity"/>.
        /// The added/updated shopping cart item is returned.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="itemParams">Parameters of the shopping cart item which should be used to perform the adding or updating.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cart"/> or <paramref name="itemParams"/> are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="itemParams"/> specify a non-positive <see cref="ShoppingCartItemParameters.SKUID"/>.</exception>
        protected virtual ShoppingCartItemInfo SetShoppingCartItemInternal(ShoppingCartInfo cart, ShoppingCartItemParameters itemParams)
        {
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }
            if (itemParams == null)
            {
                throw new ArgumentNullException(nameof(itemParams));
            }
            if (itemParams.SKUID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemParams), $"The {nameof(itemParams.SKUID)} of an item being added must be greater than 0.");
            }

            if (itemParams.Quantity <= 0)
            {
                return null;
            }

            // Try to find specified product with such product options in cart
            ShoppingCartItemInfo product = GetShoppingCartItemInternal(cart, itemParams);

            // Update product when it is not bundle item
            if (product?.SKU != null)
            {
                UpdateShoppingCartItemInternal(product, itemParams);
            }
            // Add product
            else
            {
                product = AddShoppingCartItemInternal(cart, itemParams);
            }

            // Return updated or created shopping cart item
            return product;
        }


        /// <summary>
        /// Returns shopping cart item from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartItemId">Shopping cart item ID</param>
        protected virtual ShoppingCartItemInfo GetShoppingCartItemInternal(ShoppingCartInfo cart, int cartItemId)
        {
            return cart?.CartItems.FirstOrDefault(item => item.CartItemID == cartItemId);
        }


        /// <summary>
        /// Returns shopping cart item from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartItemGuid">Shopping cart item GUID</param>
        protected virtual ShoppingCartItemInfo GetShoppingCartItemInternal(ShoppingCartInfo cart, Guid cartItemGuid)
        {
            return cart?.CartItems.FirstOrDefault(item => item.CartItemGUID == cartItemGuid);
        }


        /// <summary>
        /// Removes shopping cart item, related bundle items and its product options from the specified shopping cart object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartItem">Shopping cart item</param>
        protected virtual void RemoveShoppingCartItemInternal(ShoppingCartInfo cart, ShoppingCartItemInfo cartItem)
        {
            if ((cart != null) && (cartItem != null))
            {
                // Remove related product options
                foreach (ShoppingCartItemInfo option in cartItem.ProductOptions)
                {
                    cart.CartItems.Remove(option);
                }

                // Remove related bundle items
                foreach (ShoppingCartItemInfo bundleItem in cartItem.BundleItems)
                {
                    cart.CartItems.Remove(bundleItem);
                }

                // Remove cart item from list
                cart.CartItems.Remove(cartItem);
            }
        }


        /// <summary>
        /// Adds new initialized item to the shopping cart items collection.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="item">Shopping cart item</param>
        protected virtual void AddShoppingCartItemInternal(ShoppingCartInfo cart, ShoppingCartItemInfo item)
        {
            if ((cart == null) || (item == null))
            {
                return;
            }

            // Reference item to the shopping cart
            item.ShoppingCartID = cart.ShoppingCartID;
            item.ShoppingCart = cart;

            // Add item to the shopping cart
            cart.CartItems.Add(item);
        }


        /// <summary>
        /// Adds new item to the shopping cart object and returns its object.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="itemParams">Parameters from which the new shopping cart item is initialized.</param>
        protected virtual ShoppingCartItemInfo AddShoppingCartItemInternal(ShoppingCartInfo cart, ShoppingCartItemParameters itemParams)
        {
            if (cart == null)
            {
                return null;
            }

            // Create new shopping cart item with initialized item GUID
            ShoppingCartItemInfo product = new ShoppingCartItemInfo();

            // Initialize product properties
            product.SKUID = itemParams.SKUID;
            product.CartItemUnits = itemParams.Quantity;
            // Prevent endless loop
            product.ShoppingCart = cart;

            if (product.SKU == null)
            {
                return null;
            }

            // Add product to the cart items
            AddShoppingCartItemInternal(cart, product);

            // Add product options
            foreach (ShoppingCartItemParameters optionParams in itemParams.ProductOptions)
            {
                if (optionParams.SKUID > 0)
                {
                    // Create new shopping cart item with initialized item GUID
                    ShoppingCartItemInfo option = new ShoppingCartItemInfo
                    {
                        SKUID = optionParams.SKUID,
                        CartItemUnits = itemParams.Quantity,
                        CartItemParentGUID = product.CartItemGUID,
                        CartItemText = optionParams.Text
                    };

                    // Add product option to the list
                    product.ProductOptions.Add(option);

                    // Add product to the cart items
                    AddShoppingCartItemInternal(cart, option);
                }
            }

            // Get related bundle items
            var bundleItems = SKUInfoProvider.GetBundleItems(itemParams.SKUID).OrderBy("SKUID");

            // Add bundle items
            foreach (var bundleItem in bundleItems)
            {
                // Create new shopping cart item
                var bundleCartItem = new ShoppingCartItemInfo
                {
                    SKU = bundleItem,
                    SKUID = bundleItem.SKUID,
                    CartItemUnits = itemParams.Quantity,
                    CartItemBundleGUID = product.CartItemGUID
                };

                // Add bundle item to shopping cart item
                product.BundleItems.Add(bundleCartItem);

                // Add bundle item to shopping cart
                AddShoppingCartItemInternal(cart, bundleCartItem);
            }

            // Return new cart item which was added to the shopping cart
            return product;
        }


        /// <summary>
        /// Updates shopping cart item in the specified shopping cart object. By default only units of the specified shopping cart item and units of its product options are updated.
        /// </summary>
        /// <param name="item">Shopping cart item to be updated.</param>
        /// <param name="itemParams">Parameters of the shopping cart item which should be used to perform the update.
        /// By default only Quantity parameter is used.</param>
        protected virtual void UpdateShoppingCartItemInternal(ShoppingCartItemInfo item, ShoppingCartItemParameters itemParams)
        {
            if (item == null)
            {
                return;
            }

            const int maxUnits = 999999999;
            int units = itemParams.Quantity;

            // Do not allow to add more than 'Max units'
            if ((maxUnits - item.CartItemUnits) >= units)
            {
                // Update units of the product
                item.CartItemUnits += units;

                // Update units of the product options
                foreach (ShoppingCartItemInfo option in item.ProductOptions)
                {
                    option.CartItemUnits += units;
                }

                // Update units of related bundle items
                foreach (ShoppingCartItemInfo bundleItem in item.BundleItems)
                {
                    bundleItem.CartItemUnits += units;
                }
            }
        }


        /// <summary>
        /// Returns specified shopping cart item from the given shopping cart object. If such item is not found, null is returned.
        /// By default shopping cart item is searched according to the product SKUID and SKUIDs of its product options.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="itemParams">Parameters of the shopping cart item which should be used to perform the search.</param>
        protected virtual ShoppingCartItemInfo GetShoppingCartItemInternal(ShoppingCartInfo cart, ShoppingCartItemParameters itemParams)
        {
            foreach (ShoppingCartItemInfo cartItem in cart.CartContentItems)
            {
                // Shopping cart item with specified SKUID found, skip bundle item
                if (cartItem.SKUID == itemParams.SKUID)
                {
                    // Looking for product with product options
                    if (itemParams.ProductOptions.Count > 0)
                    {
                        int optionCount = 0;
                        int foundOptionCount = 0;

                        foreach (ShoppingCartItemParameters option in itemParams.ProductOptions)
                        {
                            int optionSKUId = option.SKUID;

                            if (optionSKUId > 0)
                            {
                                optionCount++;

                                // Try to find product options
                                foreach (ShoppingCartItemInfo cartOption in cartItem.ProductOptions)
                                {
                                    if ((cartOption.SKUID == optionSKUId) && ((cartOption.SKU.SKUProductType != SKUProductTypeEnum.Text) || (cartOption.CartItemText == option.Text)))
                                    {
                                        foundOptionCount++;
                                        break;
                                    }
                                }
                            }
                        }

                        // Product with such product options found
                        if ((optionCount == foundOptionCount) && (cartItem.ProductOptions.Count == foundOptionCount))
                        {
                            return cartItem;
                        }
                    }
                    // Looking for product without product options
                    else if (cartItem.ProductOptions.Count == 0)
                    {
                        // Product without product options found
                        return cartItem;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns sum of all shopping cart items' units, bundle items and product options are not included.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        protected virtual int GetTotalUnitsInternal(ShoppingCartInfo cart)
        {
            // Do not process
            if (cart == null)
            {
                return 0;
            }

            // Count units without bundle items and product options
            return cart.CartProducts.Sum(item => item.CartItemUnits);
        }


        /// <summary>
        /// Updates shopping cart with shopping cart from order. If such product configuration already exists in the shopping cart, only  its quantity is updated.
        /// Returns true if all ordered items were successfully added to cart.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="orderID">ID of order which should be added to shopping cart.</param>
        protected virtual bool UpdateShoppingCartFromOrderInternal(ShoppingCartInfo cart, int orderID)
        {
            // Get order object
            OrderInfo order = OrderInfoProvider.GetOrderInfo(orderID);

            if (cart != null && order != null)
            {
                // Get shopping cart from order
                ShoppingCartInfo cartFromOrder = GetShoppingCartInfoFromOrder(orderID);

                return CopyShoppingCartItems(cartFromOrder, cart);
            }

            return false;
        }


        /// <summary>
        /// Updates destination shopping cart with source shopping cart.
        /// <para>If such product configuration already exists in the shopping cart, only its quantity is updated.</para>
        /// </summary>
        /// <param name="sourceCart">Original shopping cart</param>
        /// <param name="destinationCart">The cart where to copy items</param>
        /// <returns>Returns true if all ordered items were successfully added to cart.</returns>
        protected virtual bool CopyShoppingCartItemsInternal(ShoppingCartInfo sourceCart, ShoppingCartInfo destinationCart)
        {
            if (sourceCart == null || destinationCart == null)
            {
                return false;
            }

            bool success = true;

            // Get products of shopping cart from order
            foreach (var item in sourceCart.CartProducts)
            {
                var cartItemParams = new ShoppingCartItemParameters(item);

                // Check product/options combination
                if (Service.Resolve<ICartItemChecker>().CheckNewItem(cartItemParams, destinationCart))
                {
                    // Set item to shopping cart
                    SetShoppingCartItem(destinationCart, cartItemParams);
                }
                else
                {
                    success = false;
                }
            }

            destinationCart.Evaluate();

            return success;
        }

        #endregion


        #region "Internal methods - Resolver"

        /// <summary>
        /// Returns initialized shopping cart resolver
        /// </summary>
        /// <param name="cart">Shopping cart data to be used for resolver initialization</param>
        protected virtual MacroResolver GetShoppingCartResolverInternal(ShoppingCartInfo cart)
        {
            MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();

            // Set culture
            resolver.Culture = String.IsNullOrEmpty(cart.ShoppingCartCulture) ? LocalizationContext.PreferredCultureCode : cart.ShoppingCartCulture;

            // Set source object
            resolver.SourceObject = cart;

            // Set source data
            resolver.SetAnonymousSourceData(cart);

            // Get resolver data
            var data = GetShoppingCartResolverData(cart, resolver.Culture);

            // Init named sources
            foreach (var item in data)
            {
                resolver.SetNamedSourceData(item.Key, item.Value);
            }

            return resolver;
        }


        /// <summary>
        /// Returns the HTML code for the order invoice.
        /// </summary>
        /// <param name="cart">Shopping cart data used for generating invoice</param>
        protected virtual string GetOrderInvoiceInternal(ShoppingCartInfo cart)
        {
            // Do not process
            if (String.IsNullOrEmpty(cart?.SiteName))
            {
                return "";
            }

            // Get invoice template
            bool useGlobalInvoice = ECommerceSettings.UseGlobalInvoice(cart.SiteName);
            string template = ECommerceSettings.InvoiceTemplate(useGlobalInvoice ? "" : cart.SiteName);

            // Store original shopping cart culture
            string origCartCulture = cart.ShoppingCartCulture;

            // Use default culture for generating invoice
            cart.ShoppingCartCulture = CultureHelper.GetDefaultCultureCode(cart.SiteName);

            // Resolve macros
            MacroResolver mr = GetShoppingCartResolver(cart);

            string invoice = mr.ResolveMacros(template);

            // Restore shopping cart culture
            cart.ShoppingCartCulture = origCartCulture;

            return invoice;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Calculates remaining amount for free shipping.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// Returns remaining amount for free shipping.
        /// Method returns 0 if the cart is null, without currency or if there is no valid discount or if free shipping is already applied.
        /// </returns>
        internal static decimal CalculateRemainingAmountForFreeShipping(ShoppingCartInfo cart)
        {
            if (cart?.Currency == null)
            {
                return 0m;
            }

            var request = Service.Resolve<IShoppingCartAdapterService>().GetCalculationRequest(cart);
            var result = Service.Resolve<IShoppingCartAdapterService>().GetCalculationResult(cart);

            return Service.Resolve<IShippingDiscountSource>().GetRemainingAmountForFreeShipping(new CalculatorData(request, result), cart.TotalItemsPrice - cart.OrderDiscount);
        }


        /// <summary>
        /// Gets the data for the shopping cart resolver. (selector -> data object) of the shopping cart objects.
        /// </summary>
        /// <param name="cart">Shopping cart from which the data are loaded</param>
        /// <param name="resolverCulture">Culture used for macro resolving</param>
        private IDictionary<string, object> GetShoppingCartResolverData(ShoppingCartInfo cart, string resolverCulture)
        {
            var result = new Dictionary<string, object>();

            // Shopping cart data
            result["ShoppingCart"] = cart;

            // Order data
            var order = OrderInfoProvider.GetOrderInfo(cart.OrderId);
            if (order != null)
            {
                result["Order"] = order;

                // Order status data
                var status = OrderStatusInfoProvider.GetOrderStatusInfo(order.OrderStatusID);
                if (status != null)
                {
                    result["OrderStatus"] = status;
                }
            }

            // Billing address data
            if (cart.ShoppingCartBillingAddress != null)
            {
                result["BillingAddress"] = cart.ShoppingCartBillingAddress;
            }

            // Shipping address data
            if (cart.ShoppingCartShippingAddress != null || cart.ShoppingCartBillingAddress != null)
            {
                result["ShippingAddress"] = cart.ShoppingCartShippingAddress ?? cart.ShoppingCartBillingAddress;
            }

            // Shipping option data
            if (cart.ShippingOption != null)
            {
                result["ShippingOption"] = cart.ShippingOption;
            }

            // Company address data
            if (cart.ShoppingCartCompanyAddress != null)
            {
                result["CompanyAddress"] = cart.ShoppingCartCompanyAddress;
            }

            // Customer data
            result["Customer"] = cart.Customer;

            // Payment option data
            if (cart.PaymentOption != null)
            {
                result["PaymentOption"] = cart.PaymentOption;
            }

            // Currency data
            if (cart.Currency != null)
            {
                result["Currency"] = cart.Currency;
            }

            // Shopping cart content data
            if (!cart.IsEmpty)
            {
                result["ContentTable"] = cart.ContentTable;
            }

            if (cart.OrderId > 0)
            {
                // Add table with e-products if any
                var hasEproducts = cart.CartItems.Any(item => (item.SKU != null) && (item.SKU.SKUProductType == SKUProductTypeEnum.EProduct));
                if (hasEproducts)
                {
                    DataSet ds = OrderItemSKUFileInfoProvider.GetOrderItemSKUFiles(cart.OrderId);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        result["EproductsTable"] = ds.Tables[0].Rows;
                    }
                }
            }

            // Totals data
            result["TotalPrice"] = ValidationHelper.GetDecimal(cart.TotalPrice, 0m, resolverCulture);
            result["TotalShipping"] = ValidationHelper.GetDecimal(cart.TotalShipping, 0m, resolverCulture);
            result["GrandTotal"] = ValidationHelper.GetDecimal(cart.GrandTotal, 0m, resolverCulture);

            return result;
        }


        /// <summary>
        /// Returns rounding service based on shopping cart site identifier.
        /// </summary>
        private static IRoundingService GetRoundingService(int siteId)
        {
            var roundingServiceFactory = Service.Resolve<IRoundingServiceFactory>();
            return roundingServiceFactory.GetRoundingService(siteId);
        }


        private static void SetOrderAddresses(ShoppingCartInfo cart)
        {
            if ((cart == null) || (cart.OrderId <= 0))
            {
                return;
            }

            SetOrderAddress(cart.ShoppingCartBillingAddress, cart.OrderId, AddressType.Billing);

            if ((cart.ShoppingCartShippingAddress != null) || cart.IsShippingNeeded)
            {
                // Clone billing address and set to shipping address if shipping is needed and wasn't specified
                SetOrderAddress(cart.ShoppingCartShippingAddress ?? cart.ShoppingCartBillingAddress, cart.OrderId, AddressType.Shipping);
            }

            if (cart.ShoppingCartCompanyAddress != null)
            {
                SetOrderAddress(cart.ShoppingCartCompanyAddress, cart.OrderId, AddressType.Company);
            }
        }


        private static void SetOrderAddress(AddressInfo address, int orderId, AddressType addressType)
        {
            var addressConverter = Service.Resolve<IAddressConverter>();
            var orderAddress = addressConverter.Convert(address, orderId, addressType);

            OrderAddressInfoProvider.SetAddressInfo(orderAddress);
        }

        #endregion
    }
}