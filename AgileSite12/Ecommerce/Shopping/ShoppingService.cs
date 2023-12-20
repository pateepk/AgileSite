using System;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;

[assembly: RegisterImplementation(typeof(IShoppingService), typeof(ShoppingService), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides interaction with shopping cart.
    /// </summary>
    public class ShoppingService : IShoppingService
    {
        private readonly IEcommerceActivityLogger mEcommerceActivityLogger;
        private readonly ICartItemChecker mCartItemChecker;
        private readonly ICurrentShoppingCartService mCurrentShoppingCartService;
        private readonly ICustomerShoppingService mCustomerShoppingService;
        private readonly IShoppingCartAdapterService mShoppingCartAdapterService;
        private readonly IShippingPriceService mShippingPriceService;


        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingService"/> class with supplied activity logger.
        /// </summary>
        /// <param name="ecommerceActivityLogger">Activity logger to be used for logging E-commerce activities.</param>
        /// <param name="cartItemChecker">Cart item checker to be used for product being added to the cart.</param>
        /// <param name="currentShoppingCartService">Service to be used for current shopping cart retrieval.</param>
        /// <param name="customerShoppingService">Customer shopping service used for user registration.</param>
        /// <param name="shoppingCartAdapterService">Shopping cart adapter service used for calculating shipping price.</param>
        /// <param name="shippingPriceService">Shipping price service used for calculating shipping price.</param>
        public ShoppingService(IEcommerceActivityLogger ecommerceActivityLogger, ICartItemChecker cartItemChecker, ICurrentShoppingCartService currentShoppingCartService, ICustomerShoppingService customerShoppingService,
            IShoppingCartAdapterService shoppingCartAdapterService, IShippingPriceService shippingPriceService)
        {
            mEcommerceActivityLogger = ecommerceActivityLogger;
            mCartItemChecker = cartItemChecker;
            mCurrentShoppingCartService = currentShoppingCartService;
            mCustomerShoppingService = customerShoppingService;
            mShoppingCartAdapterService = shoppingCartAdapterService;
            mShippingPriceService = shippingPriceService;
        }


        /// <summary>
        /// Gets the shopping cart used by the <see cref="IShoppingService"/> instance. The shopping cart is retrieved
        /// using the <see cref="ICurrentShoppingCartService"/> implementation this instance was initialized with.
        /// This member is a backing method of the <see cref="ECommerceContext.get_CurrentShoppingCart"/> property.
        /// </summary>
        /// <returns><see cref="ShoppingCartInfo"/> object representing the shopping cart on the current site.</returns>
        public ShoppingCartInfo GetCurrentShoppingCart()
        {
            var user = MembershipContext.AuthenticatedUser;
            var siteId = SiteContext.CurrentSiteID;

            return mCurrentShoppingCartService.GetCurrentShoppingCart(user, siteId);
        }


        /// <summary>
        /// Gets the current customer.
        /// </summary>
        /// <returns><see cref="CustomerInfo"/> object representing the current customer. Returns <c>null</c> if there is not any current customer.</returns>
        /// <seealso cref="SetCustomer(CustomerInfo)"/>
        /// <seealso cref="CustomerHelper.MapToCustomer(UserInfo)"/>
        public virtual CustomerInfo GetCurrentCustomer()
        {
            var customer = GetCurrentShoppingCart()?.Customer;
            return customer?.CustomerID > 0 ? customer : null;
        }


        /// <summary>
        /// Sets the given <paramref name="customer"/> to the current shopping cart and persists changes in the database.
        /// </summary>
        /// <param name="customer">Customer to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="customer"/> is null.</exception>
        /// <seealso cref="GetCurrentCustomer"/>
        /// <seealso cref="CustomerHelper.MapToCustomer(UserInfo)"/>
        public virtual void SetCustomer(CustomerInfo customer)
        {
            SetCustomerInternal(customer, true);
        }


        /// <summary>
        /// Sets the given <paramref name="customer"/> to the current shopping cart and persists changes in the database
        /// if the <paramref name="persistShoppingCart" /> is true.
        /// </summary>
        protected virtual void SetCustomerInternal(CustomerInfo customer, bool persistShoppingCart)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            var shoppingCart = GetCurrentShoppingCart();

            if (!customer.CustomerIsRegistered)
            {
                customer.CustomerSiteID = shoppingCart.ShoppingCartSiteID;
            }

            CustomerInfoProvider.SetCustomerInfo(customer);
            customer.UpdateLinkedContact();

            shoppingCart.Customer = customer;

            if (persistShoppingCart)
            {
                ShoppingCartInfoProvider.SetShoppingCartInfo(shoppingCart);
            }
        }


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
        public virtual ShoppingCartItemInfo AddItemToCart(int skuId, int quantity)
        {
            if (quantity <= 0)
            {
                return null;
            }

            var parameters = new ShoppingCartItemParameters
            {
                SKUID = skuId,
                Quantity = quantity
            };

            return AddItemToCart(parameters);
        }


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
        public virtual ShoppingCartItemInfo AddItemToCart(ShoppingCartItemParameters itemParameters)
        {
            if (itemParameters == null)
            {
                throw new ArgumentNullException(nameof(itemParameters));
            }

            if (itemParameters.Quantity <= 0)
            {
                return null;
            }

            var shoppingCart = GetCurrentShoppingCart();

            ValidateAddItemToCart(shoppingCart, itemParameters);

            if (!mCartItemChecker.CheckNewItem(itemParameters, shoppingCart))
            {
                return null;
            }

            if (shoppingCart.ShoppingCartID == 0)
            {
                SaveCart();
            }

            var cartItem = ShoppingCartInfoProvider.SetShoppingCartItem(shoppingCart, itemParameters);

            cartItem.CartItemAutoAddedUnits = 0;
            SetCartItem(cartItem);

            shoppingCart.Evaluate();

            LogProductAddedToCartActivity(cartItem);

            EcommerceEvents.ProductAddedToShoppingCart.StartEvent(cartItem);

            return cartItem;
        }


        /// <summary>
        /// Validates adding an item to the shopping <paramref name="cart"/> via <see cref="AddItemToCart(ShoppingCartItemParameters)"/>.
        /// The method validates whether an item being added belongs to the same site as current shopping cart.
        /// Throws an exception when validation fails.
        /// </summary>
        /// <param name="cart"><see cref="ShoppingCartInfo"/> to add the item to.</param>
        /// <param name="itemParameters">Parameters specifying the item to be added.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        /// <seealso cref="AddItemToCartValidator"/>
        /// <seealso cref="SKUFromDifferentSiteValidationError"/>
        protected virtual void ValidateAddItemToCart(ShoppingCartInfo cart, ShoppingCartItemParameters itemParameters)
        {
            var validator = new AddItemToCartValidator(cart, itemParameters);
            if (!validator.Validate())
            {
                if (validator.Contains<SKUFromDifferentSiteValidationError>())
                {
                    throw new InvalidOperationException($"Cannot add an item to a cart from a different site.");
                }
                throw new InvalidOperationException($"Validation of adding an item to cart failed with the following error: {validator.FirstErrorType().Name}");
            }
        }


        /// <summary>
        /// Sets or updates cart item, its product options and bundle items to the database.
        /// </summary>
        /// <param name="cartItem"><see cref="ShoppingCartItemInfo"/> to be set.</param>
        protected virtual void SetCartItem(ShoppingCartItemInfo cartItem)
        {
            var cart = GetCurrentShoppingCart();

            cartItem.ShoppingCart = cart;
            cartItem.ShoppingCartID = cart.ShoppingCartID;
            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(cartItem);

            // Update product options and bundle items in database
            cartItem.ProductOptions.ForEach(ShoppingCartItemInfoProvider.SetShoppingCartItemInfo);
            cartItem.BundleItems.ForEach(ShoppingCartItemInfoProvider.SetShoppingCartItemInfo);
        }


        /// <summary>
        /// Removes item with specified <paramref name="itemId"/> from the current shopping cart.
        /// Also removes all children and bundle items from the cart, logs the activity and evaluates the cart.
        /// </summary>
        /// <param name="itemId">ID of the item to be removed.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when item with the specified <paramref name="itemId"/> is not in the cart.</remarks>
        public virtual void RemoveItemFromCart(int itemId)
        {
            var shoppingCartInfo = GetCurrentShoppingCart();
            var shoppingCartItemInfo = shoppingCartInfo.CartItems.Find(scii => scii.CartItemID == itemId);

            if (shoppingCartItemInfo == null)
            {
                return;
            }

            RemoveItemFromCartInternal(shoppingCartInfo, shoppingCartItemInfo);
        }


        /// <summary>
        /// Removes item specified by <paramref name="shoppingCartItemInfo"/> from the current shopping cart.
        /// Also removes all children and bundle items from the cart, logs the activity and evaluates the cart.
        /// </summary>
        /// <param name="shoppingCartItemInfo">Item to be removed.</param>     
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shoppingCartItemInfo"/> is null.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when the specified <paramref name="shoppingCartItemInfo"/> is not in the cart.</remarks>
        public virtual void RemoveItemFromCart(ShoppingCartItemInfo shoppingCartItemInfo)
        {
            if (shoppingCartItemInfo == null)
            {
                throw new ArgumentNullException(nameof(shoppingCartItemInfo));
            }

            var shoppingCartInfo = GetCurrentShoppingCart();

            // Ensure that the object instance is from the current cart
            var currentCartItemInfo = shoppingCartInfo.CartItems.Find(scii => scii.CartItemID == shoppingCartItemInfo.CartItemID);

            if (currentCartItemInfo == null)
            {
                return;
            }

            RemoveItemFromCartInternal(shoppingCartInfo, currentCartItemInfo);
        }


        private void RemoveItemFromCartInternal(ShoppingCartInfo shoppingCartInfo, ShoppingCartItemInfo shoppingCartItemInfo)
        {
            ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(shoppingCartItemInfo);
            ShoppingCartInfoProvider.RemoveShoppingCartItem(shoppingCartInfo, shoppingCartItemInfo);

            shoppingCartInfo.Evaluate();

            LogProductRemovedFromCartActivity(shoppingCartItemInfo);

            // Make sure that in-memory changes persist (unsaved address, etc.)
            mCurrentShoppingCartService.SetCurrentShoppingCart(shoppingCartInfo);
        }


        /// <summary>
        /// Updates the quantity of a shopping cart item specified by <paramref name="itemId"/> to <paramref name="quantity"/>.
        /// Logs the activity resulting from the actual amount of items added or removed and evaluates the cart.
        /// </summary>
        /// <param name="itemId">ID of the shopping cart item.</param>
        /// <param name="quantity">New number of the product units.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when <paramref name="quantity"/> less than zero or item with the specified <paramref name="itemId"/> is not in the cart.</remarks>
        public virtual void UpdateItemQuantity(int itemId, int quantity)
        {
            if (quantity < 0)
            {
                return;
            }

            var shoppingCartInfo = GetCurrentShoppingCart();
            var shoppingCartItemInfo = shoppingCartInfo.CartItems.FirstOrDefault(i => i.CartItemID == itemId);

            if (shoppingCartItemInfo == null)
            {
                return;
            }

            UpdateItemQuantityInternal(shoppingCartInfo, shoppingCartItemInfo, quantity);
        }


        /// <summary>
        /// Updates the quantity of a shopping cart item specified by <paramref name="shoppingCartItemInfo"/> to <paramref name="quantity"/>.
        /// Logs the activity resulting from the actual amount of items added or removed and evaluates the cart.
        /// </summary>
        /// <param name ="shoppingCartItemInfo">Item the quantity of which is to be changed.</param>
        /// <param name="quantity">New number of the product units.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shoppingCartItemInfo"/> is null.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <remarks>This method does nothing when <paramref name="quantity"/> less than zero or the specified <paramref name="shoppingCartItemInfo"/> is not in the cart.</remarks>
        public virtual void UpdateItemQuantity(ShoppingCartItemInfo shoppingCartItemInfo, int quantity)
        {
            if (shoppingCartItemInfo == null)
            {
                throw new ArgumentNullException(nameof(shoppingCartItemInfo));
            }

            if (quantity < 0)
            {
                return;
            }

            var shoppingCartInfo = GetCurrentShoppingCart();

            // Ensure that the object instance is from the current cart
            var currentCartItemInfo = shoppingCartInfo.CartItems.Find(scii => scii.CartItemID == shoppingCartItemInfo.CartItemID);

            if (currentCartItemInfo == null)
            {
                return;
            }

            UpdateItemQuantityInternal(shoppingCartInfo, currentCartItemInfo, quantity);
        }


        private void UpdateItemQuantityInternal(ShoppingCartInfo shoppingCartInfo, ShoppingCartItemInfo shoppingCartItemInfo, int quantity)
        {
            if (quantity == 0)
            {
                RemoveItemFromCartInternal(shoppingCartInfo, shoppingCartItemInfo);

                return;
            }

            var originalItemQuantity = shoppingCartItemInfo.CartItemUnits;

            ShoppingCartItemInfoProvider.UpdateShoppingCartItemUnits(shoppingCartItemInfo, quantity);

            shoppingCartInfo.Evaluate();

            // Log activity depending on whether the quantity was increased or decreased
            if (originalItemQuantity > quantity)
            {
                LogProductRemovedFromCartActivity(shoppingCartItemInfo, originalItemQuantity - quantity);
            }
            else
            {
                LogProductAddedToCartActivity(shoppingCartItemInfo, quantity - originalItemQuantity);
            }

            // Make sure that in-memory changes persist (unsaved address, etc.)
            mCurrentShoppingCartService.SetCurrentShoppingCart(shoppingCartInfo);
        }


        /// <summary>
        /// Removes all items from the current shopping cart.
        /// Also removes all children and bundle items from the cart, logs the activity and evaluates the cart.
        /// </summary>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        public virtual void RemoveAllItemsFromCart()
        {
            foreach (ShoppingCartItemInfo cartItem in GetCurrentShoppingCart().CartProducts)
            {
                LogProductRemovedFromCartActivity(cartItem);
            }

            ShoppingCartInfo shoppingCartInfo = GetCurrentShoppingCart();
            ShoppingCartInfoProvider.EmptyShoppingCart(shoppingCartInfo);

            shoppingCartInfo.Evaluate();
        }


        /// <summary>
        /// Applies the specified coupon code to the current shopping cart and evaluates the cart.
        /// </summary>
        /// <param name="couponCode">Coupon code to apply.</param>
        /// <returns>True if the <paramref name="couponCode"/> is applied, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="couponCode"/> is null or empty.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        public virtual bool AddCouponCode(string couponCode)
        {
            return GetCurrentShoppingCart().AddCouponCode(couponCode);
        }


        /// <summary>
        /// Removes the specified coupon code from the current shopping cart and evaluates the cart.
        /// </summary>
        /// <param name="couponCode">Coupon code to remove.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="couponCode"/> is null or empty.</exception>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        public virtual void RemoveCouponCode(string couponCode)
        {
            GetCurrentShoppingCart().RemoveCouponCode(couponCode);
        }


        /// <summary>
        /// Returns the billing address for the current shopping cart.
        /// </summary>
        /// <remarks>
        /// Address has to be set and assigned in the database to the current shopping cart.
        /// </remarks>
        /// <seealso cref="SetBillingAddress(AddressInfo)"/>
        public AddressInfo GetBillingAddress()
        {
            var address = GetCurrentShoppingCart().ShoppingCartBillingAddress;
            return address?.AddressID > 0 ? address : null;
        }


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
        public virtual void SetBillingAddress(AddressInfo billingAddressInfo)
        {
            SetBillingAddressInternal(billingAddressInfo, true);
        }


        /// <summary>
        /// Sets billing address to the current shopping cart, evaluates the cart and persists changes in the database
        /// if the <paramref name="persistShoppingCart"/> is true.
        /// </summary>
        protected virtual void SetBillingAddressInternal(AddressInfo billingAddressInfo, bool persistShoppingCart)
        {
            if (billingAddressInfo == null)
            {
                throw new ArgumentNullException(nameof(billingAddressInfo));
            }

            var cart = GetCurrentShoppingCart();

            ValidateSetBillingAddress(billingAddressInfo);
            SetAddress(billingAddressInfo);
            cart.ShoppingCartBillingAddress = billingAddressInfo;
            cart.Evaluate();

            if (persistShoppingCart)
            {
                ShoppingCartInfoProvider.SetShoppingCartInfo(cart);
            }
        }


        /// <summary>
        /// Validates billing address which is to be set to the current shopping cart.
        /// </summary>
        /// <param name="billingAddressInfo">Address for billing purposes.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        /// <seealso cref="CustomerAddressValidator"/>
        /// <seealso cref="CountryNotSetValidationError"/>
        /// <seealso cref="StateNotFromCountryValidationError"/>
        protected virtual void ValidateSetBillingAddress(AddressInfo billingAddressInfo)
        {
            var validator = new CustomerAddressValidator(billingAddressInfo);
            if (!validator.Validate())
            {
                var stateNotFromCountryValidationError = validator.Errors.OfType<StateNotFromCountryValidationError>().FirstOrDefault();
                if (stateNotFromCountryValidationError != null)
                {
                    throw new InvalidOperationException($"Cannot set billing address with incorrectly assigned state '{stateNotFromCountryValidationError.State}' to country '{stateNotFromCountryValidationError.Country}'.");
                }
                throw new InvalidOperationException($"Validation of billing address failed with the following error: {validator.FirstErrorType().Name}.");
            }
        }


        /// <summary>
        /// Validates and creates order from current shopping cart. If order creation was successful empties shopping cart.
        /// If order requires an user account, handles its creation.
        /// Also tracks order conversion, sends order notifications and logs purchase activities.
        /// </summary>
        /// <returns>Returns the <see cref="OrderInfo"/> of newly created order.</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails, or when strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so or when strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so (<see cref="SKUInfo.SKUSellOnlyAvailable"/> is true).</exception>
        /// <seealso cref="CreateOrderValidator"/>
        public virtual OrderInfo CreateOrder()
        {
            var cart = GetCurrentShoppingCart();

            // Remove the shipping option if it is not needed
            if (!cart.IsShippingNeeded && (cart.ShippingOption != null))
            {
                SetShippingOptionInternal(0, true);
            }

            cart.ShoppingCartCulture = LocalizationContext.PreferredCultureCode;

            ValidateCreateOrder(cart);

            using (var scope = new CMSTransactionScope())
            {
                ShoppingCartInfoProvider.SetOrder(cart);

                mCustomerShoppingService.AutoRegisterCustomer(cart.Customer);

                scope.Commit();
            }

            TrackOrderConversion(cart);
            SendOrderNotifications(cart);
            LogPurchaseActivities(cart);

            ShoppingCartInfoProvider.DeleteShoppingCartInfo(cart.ShoppingCartID);
            mCurrentShoppingCartService.SetCurrentShoppingCart(null);

            return cart.Order;
        }


        /// <summary>
        /// Validates <paramref name="cart"/> for which order is being created.
        /// </summary>
        /// <param name="cart">Cart to be validated.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        /// <seealso cref="CreateOrderValidator"/>
        protected virtual void ValidateCreateOrder(ShoppingCartInfo cart)
        {
            var validator = new CreateOrderValidator(cart);
            if (!validator.Validate())
            {
                throw new InvalidOperationException($"Order creation failed with following validation error: {validator.FirstErrorType().Name}.");
            }
        }


        /// <summary>
        /// Returns the shipping address for the current shopping cart.
        /// </summary>
        /// <remarks>
        /// Address has to be set and assigned in the database to the current shopping cart.
        /// </remarks>
        /// <seealso cref="SetShippingAddress(AddressInfo)"/>
        public AddressInfo GetShippingAddress()
        {
            var address = GetCurrentShoppingCart().ShoppingCartShippingAddress;
            return address?.AddressID > 0 ? address : null;
        }


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
        public virtual void SetShippingAddress(AddressInfo shippingAddressInfo)
        {
            SetShippingAddressInternal(shippingAddressInfo, true);
        }


        /// <summary>
        /// Sets shipping address to the current shopping cart, evaluates the cart and persists changes in the database
        /// if the <paramref name="persistShoppingCart"/> is true.
        /// </summary>
        protected virtual void SetShippingAddressInternal(AddressInfo shippingAddressInfo, bool persistShoppingCart)
        {
            if (shippingAddressInfo == null)
            {
                throw new ArgumentNullException(nameof(shippingAddressInfo));
            }

            var cart = GetCurrentShoppingCart();

            ValidateSetShippingAddress(shippingAddressInfo);
            SetAddress(shippingAddressInfo);
            cart.ShoppingCartShippingAddress = shippingAddressInfo;
            cart.Evaluate();

            if (persistShoppingCart)
            {
                ShoppingCartInfoProvider.SetShoppingCartInfo(cart);
            }
        }


        /// <summary>
        /// Validates shipping address which is to be set to the current shopping cart.
        /// </summary>
        /// <param name="shippingAddressInfo">Address for shipping purposes.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        /// <seealso cref="CustomerAddressValidator"/>
        /// <seealso cref="CountryNotSetValidationError"/>
        /// <seealso cref="StateNotFromCountryValidationError"/>
        protected virtual void ValidateSetShippingAddress(AddressInfo shippingAddressInfo)
        {
            var validator = new CustomerAddressValidator(shippingAddressInfo);
            if (!validator.Validate())
            {
                var stateNotFromCountryValidationError = validator.Errors.OfType<StateNotFromCountryValidationError>().FirstOrDefault();
                if (stateNotFromCountryValidationError != null)
                {
                    throw new InvalidOperationException($"Cannot set shipping address with incorrectly assigned state '{stateNotFromCountryValidationError.State}' to country '{stateNotFromCountryValidationError.Country}'.");
                }
                throw new InvalidOperationException($"Validation of shipping address failed with the following error: {validator.FirstErrorType().Name}");
            }
        }


        /// <summary>
        /// Validates whether a payment option is assigned to the same site as the current shopping cart or the payment option is a global object.
        /// </summary>
        /// <param name="paymentOptionInfo">Payment option.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        /// <seealso cref="PaymentOptionFromDifferentSiteValidationError"/>
        protected virtual void ValidateSetPaymentOption(PaymentOptionInfo paymentOptionInfo)
        {
            var validator = new PaymentOptionValidator(paymentOptionInfo, GetCurrentShoppingCart());
            if (!validator.Validate())
            {
                var paymentOptionFromDifferentSiteValidationError = validator.Errors.OfType<PaymentOptionFromDifferentSiteValidationError>().FirstOrDefault();
                if(paymentOptionFromDifferentSiteValidationError != null)
                {
                    throw new InvalidOperationException($"Cannot set payment option '{paymentOptionInfo.PaymentOptionName}' to a shopping cart assigned to a different site.");
                }
                throw new InvalidOperationException($"Validation of payment option failed with the following error: {validator.FirstErrorType().Name}");
            }
        }


        /// <summary>
        /// Validates whether a shipping option is assigned to the same site as the current shopping cart or the shipping option is a global object.
        /// </summary>
        /// <param name="shippingOptionInfo">Shipping option.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        /// <seealso cref="ShippingOptionFromDifferentSiteValidationError"/>
        protected virtual void ValidateShippingOption(ShippingOptionInfo shippingOptionInfo)
        {
            var validator = new ShippingOptionValidator(shippingOptionInfo, GetCurrentShoppingCart());
            if (!validator.Validate())
            {
                var paymentOptionFromDifferentSiteValidationError = validator.Errors.OfType<ShippingOptionFromDifferentSiteValidationError>().FirstOrDefault();
                if (paymentOptionFromDifferentSiteValidationError != null)
                {
                    throw new InvalidOperationException($"Cannot set shipping option '{shippingOptionInfo.ShippingOptionName}' to a shopping cart assigned to a different site.");
                }
                throw new InvalidOperationException($"Validation of shipping option failed with the following error: {validator.FirstErrorType().Name}");
            }
        }


        /// <summary>
        /// Sets <paramref name="address"/> to the database. Assigns <see cref="AddressInfo.AddressCustomerID"/>/>.
        /// Sets <see cref="AddressInfo.AddressName"/> to <paramref name="address"/> if not already set.
        /// </summary>
        /// <param name="address">Address to set.</param>
        /// <seealso cref="AddressInfoProvider.GetAddressName(AddressInfo)"/>
        protected virtual void SetAddress(AddressInfo address)
        {
            var currentCustomer = GetCurrentCustomer();
            if (currentCustomer == null)
            {
                throw new InvalidOperationException($"Customer is not set for the current shopping cart. Set customer via '{nameof(SetCustomer)}' method.");
            }

            if (address.AddressCustomerID == 0)
            {
                address.AddressCustomerID = currentCustomer.CustomerID;
            }

            if (String.IsNullOrEmpty(address.AddressPersonalName))
            {
                address.AddressPersonalName = AddressInfoProvider.GetAddressPersonalName(currentCustomer);
            }

            if (String.IsNullOrEmpty(address.AddressName))
            {
                address.AddressName = AddressInfoProvider.GetAddressName(address);
            }

            AddressInfoProvider.SetAddressInfo(address);
        }


        /// <summary>
        /// Returns the selected shipping option for the current shopping cart.
        /// </summary>
        /// <seealso cref="ShippingOptionInfo"/>
        /// <seealso cref="SetShippingOption"/>
        public virtual int GetShippingOption()
        {
            var shippingOption = GetCurrentShoppingCart().ShippingOption;

            return shippingOption?.ShippingOptionID ?? 0;
        }


        /// <summary>
        /// Sets the selected shipping option to the current shopping cart, evaluates it and persists changes in the database.
        /// Does nothing if <paramref name="shippingOptionId"/> is the same as the cart already has.
        /// </summary>
        /// <param name="shippingOptionId">Shipping option ID to set.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <seealso cref="ShippingOptionInfo"/>
        /// <seealso cref="GetShippingOption"/>
        public virtual void SetShippingOption(int shippingOptionId)
        {
            SetShippingOptionInternal(shippingOptionId, true);
        }


        /// <summary>
        /// Sets the selected shipping option to the current shopping cart, evaluates it and persists changes in the database
        /// if the <paramref name="persistShoppingCart"/> is true.
        /// Does nothing if <paramref name="shippingOptionId"/> is the same as the cart already has.
        /// </summary>
        protected virtual void SetShippingOptionInternal(int shippingOptionId, bool persistShoppingCart)
        {
            var cart = GetCurrentShoppingCart();
            var shippingOption = ShippingOptionInfoProvider.GetShippingOptionInfo(shippingOptionId);

            if (cart.ShoppingCartShippingOptionID != shippingOptionId && shippingOption != null)
            {
                ValidateShippingOption(shippingOption);

                cart.ShoppingCartShippingOptionID = shippingOptionId;
                cart.Evaluate();

                if (persistShoppingCart)
                {
                    ShoppingCartInfoProvider.SetShoppingCartInfo(cart);
                }
            }
        }


        /// <summary>
        /// Returns price of the given shipping option based on the current shopping cart.
        /// </summary>
        /// <param name="shippingOptionInfo">Shipping option for which the price is calculated.</param>
        public decimal CalculateShippingOptionPrice(ShippingOptionInfo shippingOptionInfo)
        {
            var cart = GetCurrentShoppingCart();

            if (shippingOptionInfo == null || (cart.Currency == null))
            {
                return 0m;
            }

            var request = mShoppingCartAdapterService.GetCalculationRequest(cart);
            request.ShippingOption = shippingOptionInfo;

            var result = mShoppingCartAdapterService.GetCalculationResult(cart);

            return mShippingPriceService.GetShippingPrice(new CalculatorData(request, result), cart.TotalItemsPrice).Price;
        }


        /// <summary>
        /// Returns the selected payment option for the current shopping cart.
        /// </summary>
        /// <seealso cref="PaymentOptionInfo"/>
        /// <seealso cref="SetPaymentOption"/>
        public virtual int GetPaymentOption()
        {
            var paymentOption = GetCurrentShoppingCart().PaymentOption;

            return paymentOption?.PaymentOptionID ?? 0;
        }


        /// <summary>
        /// Sets the selected payment option to the current shopping cart, and evaluates and saves it.
        /// Does nothing if <paramref name="paymentOptionId"/> is the same as the cart already has.
        /// </summary>
        /// <param name="paymentOptionId">Payment option ID to set.</param>
        /// <seealso cref="ShoppingCartInfo.Evaluate"/>
        /// <seealso cref="PaymentOptionInfo"/>
        /// <seealso cref="GetPaymentOption"/>
        public virtual void SetPaymentOption(int paymentOptionId)
        {
            SetPaymentOptionInternal(paymentOptionId, true);
        }


        /// <summary>
        /// Sets the selected payment option to the current shopping cart, evaluates it and persists changes in the database
        /// if the <paramref name="persistShoppingCart"/> is true.
        /// Does nothing if <paramref name="paymentOptionId"/> is the same as the cart already has.
        /// </summary>
        protected virtual void SetPaymentOptionInternal(int paymentOptionId, bool persistShoppingCart)
        {
            var cart = GetCurrentShoppingCart();
            var paymentOption = PaymentOptionInfoProvider.GetPaymentOptionInfo(paymentOptionId);

            if (cart.ShoppingCartPaymentOptionID != paymentOptionId && paymentOption != null)
            {
                ValidateSetPaymentOption(paymentOption);

                cart.ShoppingCartPaymentOptionID = paymentOptionId;
                cart.Evaluate();

                if (persistShoppingCart)
                {
                    ShoppingCartInfoProvider.SetShoppingCartInfo(cart);
                }
            }
        }


        /// <summary>
        /// Validates and saves the shopping cart into the database.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when validation of shopping cart, its items, customer addresses, payment or shipping option fails.</exception>
        /// <remarks>
        /// It is not necessary to call this method after modifying a shopping cart through the other methods of the <see cref="ShoppingService"/>
        /// (adding or removing items, setting the customer, addresses, shipping and payment options, etc.). These methods save the shopping cart automatically.
        /// The method is useful when you need to save a shopping cart to the database after it gets modified outside of the <see cref="ShoppingService"/>.
        /// For example, call the method after validating the cart prior to checkout (e.g. this could remove cart items which are sold out).
        /// </remarks>
        public virtual void SaveCart()
        {
            var cart = GetCurrentShoppingCart();
            ValidateShoppingCart(cart);

            var customer = GetCurrentCustomer();
            if (customer != null)
            {
                SetCustomerInternal(customer, false);
            }

            var shippingAddress = GetShippingAddress();
            if (shippingAddress != null)
            {
                SetShippingAddressInternal(shippingAddress, false);
            }

            var billingAddress = GetBillingAddress();
            if (billingAddress != null)
            {
                SetBillingAddressInternal(billingAddress, false);
            }

            SetShippingOptionInternal(GetShippingOption(), false);
            SetPaymentOptionInternal(GetPaymentOption(), false);

            ShoppingCartInfoProvider.SetShoppingCartInfo(cart);
            foreach (var cartItem in cart.CartItems)
            {
                SetCartItem(cartItem);
            }
        }


        /// <summary>
        /// Validates the shopping cart and its items.
        /// </summary>
        /// <param name="cart"><see cref="ShoppingCartInfo"/> to be validated.</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        protected virtual void ValidateShoppingCart(ShoppingCartInfo cart)
        {
            var cartValidator = new ShoppingCartValidator(cart);
            if (!cartValidator.Validate())
            {
                throw new InvalidOperationException($"Validation of shopping cart and its items failed with the following error: {cartValidator.FirstErrorType().Name}");
            }
        }


        /// <summary>
        /// Sends notifications about created order to administrator and customer.
        /// </summary>
        /// <param name="cart">Specifies cart to be logged</param>
        /// <seealso cref="CreateOrder"/>
        protected virtual void SendOrderNotifications(ShoppingCartInfo cart)
        {
            if (ECommerceSettings.SendOrderNotification(cart.SiteName))
            {
                OrderInfoProvider.SendOrderNotificationToAdministrator(cart);
                OrderInfoProvider.SendOrderNotificationToCustomer(cart);
            }
        }


        /// <summary>
        /// Tracks order conversion of the provided <paramref name="cart"/>.
        /// </summary>
        /// <param name="cart">Specifies the cart to be tracked.</param>
        /// <seealso cref="CreateOrder"/>
        protected virtual void TrackOrderConversion(ShoppingCartInfo cart)
        {
            var name = ECommerceSettings.OrderConversionName(cart.SiteName);
            ECommerceHelper.TrackOrderConversion(cart, name);
            ECommerceHelper.TrackOrderItemsConversions(cart);
        }


        /// <summary>
        /// Logs all items in <paramref name="cart"/> as purchased.
        /// </summary>
        /// <param name="cart">Specifies cart to be logged.</param>
        /// <seealso cref="CreateOrder"/>
        protected virtual void LogPurchaseActivities(ShoppingCartInfo cart)
        {
            var orderInfo = cart.Order;
            var mainCurrency = CurrencyInfoProvider.GetMainCurrency(SiteContext.CurrentSiteID);
            var priceString = String.Format(mainCurrency.CurrencyFormatString, orderInfo.OrderTotalPriceInMainCurrency);

            foreach (var product in cart.CartItems)
            {
                mEcommerceActivityLogger.LogPurchasedProductActivity(product.SKU, product.CartItemUnits);
            }

            mEcommerceActivityLogger.LogPurchaseActivity(orderInfo.OrderID, orderInfo.OrderTotalPriceInMainCurrency, priceString, false);
        }


        /// <summary>
        /// Logs the add to shopping cart activity.
        /// </summary>
        /// <param name="item">Item added into shopping cart.</param>
        /// <param name="quantity">Amount of items added to the shopping cart. If null, the <see cref="ShoppingCartItemInfo.CartItemUnits"/> of <paramref name="item"/> is logged.</param>
        protected virtual void LogProductAddedToCartActivity(ShoppingCartItemInfo item, int? quantity = null)
        {
            mEcommerceActivityLogger.LogProductAddedToShoppingCartActivity(item.SKU, quantity ?? item.CartItemUnits);
        }


        /// <summary>
        /// Logs the remove from shopping cart activity.
        /// </summary>
        /// <param name="item">Item removed from shopping cart.</param>
        /// <param name="quantity">Amount of items removed from the shopping cart. If null, the <see cref="ShoppingCartItemInfo.CartItemUnits"/> of <paramref name="item"/> is logged.</param>
        protected virtual void LogProductRemovedFromCartActivity(ShoppingCartItemInfo item, int? quantity = null)
        {
            mEcommerceActivityLogger.LogProductRemovedFromShoppingCartActivity(item.SKU, quantity ?? item.CartItemUnits, GetCurrentContactId());
        }


        /// <summary>
        /// Gets ID of the current contact.
        /// </summary>
        /// <returns>Current contact ID.</returns>
        protected virtual int GetCurrentContactId()
        {
            return ModuleCommands.OnlineMarketingGetCurrentContactID();
        }
    }
}
