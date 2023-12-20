
using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides interface for interaction with shopping cart.
    /// </summary>
    public interface IShoppingService
    {
        /// <summary>
        /// Gets the shopping cart used by the <see cref="IShoppingService"/> instance.
        /// This member is a backing method of the <see cref="ECommerceContext.get_CurrentShoppingCart"/> property.
        /// </summary>
        /// <returns><see cref="ShoppingCartInfo"/> object representing the shopping cart on the current site.</returns>
        ShoppingCartInfo GetCurrentShoppingCart();


        /// <summary>
        /// Gets the current customer.
        /// </summary>
        /// <returns><see cref="CustomerInfo"/> object representing the current customer. Returns <c>null</c> if there is not any current customer.</returns>
        CustomerInfo GetCurrentCustomer();


        /// <summary>
        /// Sets the given <paramref name="customer"/> to the current shopping cart and persists changes in the database.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="customer"/> is null.</exception>
        /// <seealso cref="GetCurrentCustomer"/>
        /// <seealso cref="CustomerHelper.MapToCustomer(Membership.UserInfo)"/>
        void SetCustomer(CustomerInfo customer);


        /// <summary>
        /// Returns the billing address for the current shopping cart.
        /// </summary>
        /// <remarks>
        /// Address has to be set and assigned in the database to the current shopping cart.
        /// </remarks>
        /// <seealso cref="SetBillingAddress(AddressInfo)"/>
        AddressInfo GetBillingAddress();


        /// <summary>
        /// Sets billing address to the current shopping cart and evaluates the cart.
        /// </summary>
        /// <param name="billingAddressInfo">Address for billing purposes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="billingAddressInfo"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="billingAddressInfo"/> is in invalid state.</exception>
        /// <seealso cref="GetBillingAddress"/>
        /// <seealso cref="CountryNotSetValidationError"/>
        /// <seealso cref="StateNotFromCountryValidationError"/>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        void SetBillingAddress(AddressInfo billingAddressInfo);


        /// <summary>
        /// Returns the shipping address for the current shopping cart.
        /// </summary>
        /// <remarks>
        /// Address has to be set and assigned in the database to the current shopping cart.
        /// </remarks>
        /// <seealso cref="SetShippingAddress(AddressInfo)"/>
        AddressInfo GetShippingAddress();


        /// <summary>
        /// Sets shipping address to the current shopping cart and evaluates the cart.
        /// </summary>
        /// <param name="shippingAddressInfo">Address for shipping purposes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shippingAddressInfo"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="shippingAddressInfo"/> is in invalid state.</exception>
        /// <seealso cref="GetShippingAddress"/>
        /// <seealso cref="CountryNotSetValidationError"/>
        /// <seealso cref="StateNotFromCountryValidationError"/>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        void SetShippingAddress(AddressInfo shippingAddressInfo);


        /// <summary>
        /// Adds an item to the current shopping cart and logs the activity. Saves the shopping cart if it has not been saved yet.
        /// If an item in such configuration is already present in the cart, its quantity is increased by <paramref name="quantity"/>.
        /// Saves all product options and bundle items after adding and evaluates the cart.
        /// </summary>
        /// <param name="skuId">Id of SKU item to be added.</param>
        /// <param name="quantity">Quantity of the item to be added.</param>
        /// <returns>Returns the resulting item added/updated.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="skuId"/> specifies a product from a different site than current shopping cart belongs to.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>
        /// <para>The method does nothing when <paramref name="quantity"/> is less than 1 and returns null.</para>
        /// <para>This override is effectively the same as encapsulating given arguments in a new <see cref="ShoppingCartItemParameters"/> object and calling <see cref="AddItemToCart(ShoppingCartItemParameters)"/>.</para>
        /// </remarks>
        ShoppingCartItemInfo AddItemToCart(int skuId, int quantity);


        /// <summary>
        /// Adds an item to the current shopping cart and logs the activity. Saves the shopping cart if it has not been saved yet.
        /// If an item in such configuration is already present in the cart, its quantity is increased by <see cref="ShoppingCartItemParameters.Quantity"/>.
        /// Saves all product options and bundle items after adding and evaluates the cart.
        /// </summary>
        /// <param name="itemParameters">Parameters specifying the item to be added.</param>
        /// <returns>Returns the resulting item added/updated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="itemParameters"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="itemParameters"/> specify a product's <see cref="ShoppingCartItemParameters.SKUID"/> from a different site than current shopping cart belongs to.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>
        /// The method does nothing when <see cref="ShoppingCartItemParameters.Quantity"/> is less than 1 and returns null.
        /// </remarks>
        ShoppingCartItemInfo AddItemToCart(ShoppingCartItemParameters itemParameters);


        /// <summary>
        /// Removes item with specified <paramref name="itemId"/> from the current shopping cart.
        /// Also removes all children and bundle items from the cart, logs the activity and evaluates the cart.
        /// </summary>
        /// <param name="itemId">ID of the item to be removed.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when item with the specified <paramref name="itemId"/> is not in the cart.</remarks>
        void RemoveItemFromCart(int itemId);


        /// <summary>
        /// Removes item specified by <paramref name="shoppingCartItemInfo"/> from the current shopping cart.
        /// Also removes all children and bundle items from the cart, logs the activity and evaluates the cart.
        /// </summary>
        /// <param name="shoppingCartItemInfo">Item to be removed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shoppingCartItemInfo"/> is null.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when the specified <paramref name="shoppingCartItemInfo"/> is not in the cart.</remarks>
        void RemoveItemFromCart(ShoppingCartItemInfo shoppingCartItemInfo);


        /// <summary>
        /// Updates the quantity of a shopping cart item specified by <paramref name="itemId"/> to <paramref name="quantity"/>.
        /// Logs the activity resulting from the actual amount of items added or removed and evaluates the cart.
        /// </summary>
        /// <param name="itemId">ID of the shopping cart item.</param>
        /// <param name="quantity">New number of the product units.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when <paramref name="quantity"/> less than zero or item with the specified <paramref name="itemId"/> is not in the cart.</remarks>
        void UpdateItemQuantity(int itemId, int quantity);


        /// <summary>
        /// Updates the quantity of a shopping cart item specified by <paramref name="shoppingCartItemInfo"/> to <paramref name="quantity"/>.
        /// Logs the activity resulting from the actual amount of items added or removed and evaluates the cart.
        /// </summary>
        /// <param name ="shoppingCartItemInfo">Item the quantity of which is to be changed.</param>
        /// <param name="quantity">New number of the product units.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shoppingCartItemInfo"/> is null.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when <paramref name="quantity"/> less than zero or the specified <paramref name="shoppingCartItemInfo"/> is not in the cart.</remarks>
        void UpdateItemQuantity(ShoppingCartItemInfo shoppingCartItemInfo, int quantity);


        /// <summary>
        /// Removes all items from the current shopping cart.
        /// Also removes all children and bundle items from the cart, logs the activity and evaluates the cart.
        /// </summary>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        void RemoveAllItemsFromCart();


        /// <summary>
        /// Applies the specified coupon code to the current shopping cart and evaluates the cart.
        /// </summary>
        /// <param name="couponCode">Coupon code to apply.</param>
        /// <returns>True if the <paramref name="couponCode"/> is applied, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="couponCode"/> is null or empty.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        bool AddCouponCode(string couponCode);


        /// <summary>
        /// Removes the specified coupon code from the current shopping cart and evaluates the cart.
        /// </summary>
        /// <param name="couponCode">Coupon code to remove.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="couponCode"/> is null or empty.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        void RemoveCouponCode(string couponCode);


        /// <summary>
        /// Returns the selected shipping option for the current shopping cart.
        /// </summary>
        /// <seealso cref="ShippingOptionInfo"/>
        /// <seealso cref="SetShippingOption"/>
        int GetShippingOption();


        /// <summary>
        /// Sets the selected shipping option to the current shopping cart, and evaluates and saves it.
        /// Does nothing if <paramref name="shippingOptionId"/> is the same as the cart already has.
        /// </summary>
        /// <param name="shippingOptionId">Shipping option ID to set.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <seealso cref="ShippingOptionInfo"/>
        /// <seealso cref="GetShippingOption"/>
        void SetShippingOption(int shippingOptionId);


        /// <summary>
        /// Returns the selected payment option for the current shopping cart.
        /// </summary>
        /// <seealso cref="PaymentOptionInfo"/>
        /// <seealso cref="SetPaymentOption"/>
        int GetPaymentOption();


        /// <summary>
        /// Sets the selected payment option to the current shopping cart, and evaluates and saves it.
        /// Does nothing if <paramref name="paymentOptionId"/> is the same as the cart already has.
        /// </summary>
        /// <param name="paymentOptionId">Payment option ID to set.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <seealso cref="PaymentOptionInfo"/>
        /// <seealso cref="GetPaymentOption"/>
        void SetPaymentOption(int paymentOptionId);


        /// <summary>
        /// Validates and saves the shopping cart into the database.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when validation of shopping cart, its items, customer addresses, payment or shipping option fails.</exception>
        void SaveCart();


        /// <summary>
        /// Validates and creates order from current shopping cart. If order creation was successful empties shopping cart.
        /// If order requires an user account, handles its creation.
        /// Also tracks order conversion, sends order notifications and logs purchase activities.
        /// </summary>
        /// <returns>Returns the <see cref="OrderInfo"/> of newly created order.</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        /// <seealso cref="CreateOrderValidator"/>
        OrderInfo CreateOrder();


        /// <summary>
        /// Returns price of the given shipping option.
        /// </summary>
        /// <param name="shippingOptionInfo">Shipping option for which the price is calculated.</param>
        decimal CalculateShippingOptionPrice(ShippingOptionInfo shippingOptionInfo);
    }
}
