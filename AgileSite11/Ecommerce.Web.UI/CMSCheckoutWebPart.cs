using System;
using System.Collections.Generic;
using System.Text;

using CMS.Activities;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce.Internal;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.Protection;
using CMS.SiteProvider;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base class for checkout web parts.
    /// </summary>
    public class CMSCheckoutWebPart : CMSAbstractWebPart
    {
        #region "Event names constants"

        /// <summary>
        /// Name of the event raised when shopping cart has been changed.
        /// </summary>
        public const string SHOPPING_CART_CHANGED = "ShoppingCartChanged";


        /// <summary>
        /// Name of the event raised when error message has been shown.
        /// </summary>
        public const string MESSAGE_RAISED = "ShoppingCartMessageRaised";

        #endregion


        #region "Constants"

        private const string CHOP_FINALIZED_KEY = "ChopFinalized";
        private const string LOG_OFF_VALIDATION = "LogoffValidation";

        private const string EVENT_CODE_VALIDATION = "VALIDATION";
        private const string EVENT_CODE_SAVING = "SAVEOBJ";
        private const string EVENT_CODE_EXCEPTION = "EXCEPTION";

        private const string EVENT_SOURCE = "Checkout";

        private const string HTML_SEPARATOR = "<br />";

        #endregion


        #region "Variables"

        private int mContactID;

        private readonly List<Tuple<string, string>> loggedErrors = new List<Tuple<string, string>>();
        private readonly List<Exception> loggedExceptions = new List<Exception>();
        private string registrationBanned = String.Empty;
        private readonly EcommerceActivityLogger mActivityLogger = new EcommerceActivityLogger();

        #endregion


        #region "Properties"

        /// <summary>
        /// Current ShoppingCart
        /// </summary>
        public ShoppingCartInfo ShoppingCart
        {
            get
            {
                return ECommerceContext.CurrentShoppingCart;
            }
        }


        /// <summary>
        /// Current customer.
        /// </summary>
        public CustomerInfo Customer
        {
            get
            {
                return ShoppingCart.Customer;
            }
        }


        /// <summary>
        /// Contact ID passed between shopping cart steps.
        /// </summary>
        public int ContactID
        {
            get
            {
                if (mContactID <= 0)
                {
                    mContactID = ModuleCommands.OnlineMarketingGetCurrentContactID();
                }
                return mContactID;
            }
            set
            {
                mContactID = value;
            }
        }

        #endregion


        #region "Wizard methods"

        /// <summary>
        /// Loads the step.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The StepEventArgs instance containing the event data.</param>
        protected override void LoadStep(object sender, StepEventArgs e)
        {
            base.LoadStep(sender, e);

            // Ensure log off check only once
            if (e.GetValue(LOG_OFF_VALIDATION) == null)
            {
                e.SetValue(LOG_OFF_VALIDATION, true);
                // Get current shopping cart user id, 0 for null (public user)
                int currentUserId = ShoppingCart.User?.UserID ?? 0;
                // Get last known user saved in previous load of WP. In case there is no record in session current user id is retrieved.
                object userIdSessionObject = SessionHelper.GetValue("CheckoutUserID");
                int lastKnownUserID = ValidationHelper.GetInteger(userIdSessionObject, currentUserId);

                // Reset checkout process in case of different users. Possible log-off or login as another user
                if (lastKnownUserID != currentUserId)
                {
                    DocumentWizardManager.ResetWizard();

                    SessionHelper.Remove("CheckoutUserID");
                    // Refresh page for wizard to jump at first step
                    URLHelper.Redirect(RequestContext.CurrentURL);
                }

                // Set user id to session for non public users. Change from public user to authorized does not reset checkout process.
                if (currentUserId != 0)
                {
                    SessionHelper.SetValue("CheckoutUserID", currentUserId);
                }
                // Clean session entry (if there was one) for public user in case of log-off action to remove previous user id.
                else if (userIdSessionObject != null)
                {
                    SessionHelper.Remove("CheckoutUserID");
                }
            }
        }


        /// <summary>
        /// Validates shopping cart and stores order.
        /// </summary>
        /// <param name="e">The StepEventArgs instance containing the event data.</param>
        /// <param name="sender">Sender</param>
        protected override void StepFinished(object sender, StepEventArgs e)
        {
            base.StepFinished(sender, e);
            // We are on last step and checkout process has not been finalized yet by any web part
            if (e.IsLastStep && (e.GetValue(CHOP_FINALIZED_KEY) == null))
            {
                // Evaluate the cart and check if it is up to date
                if (!ShoppingCart.IsRecent())
                {
                    e.CancelEvent = true;
                    ComponentEvents.RequestEvents.RaiseEvent(this, e, SHOPPING_CART_CHANGED);
                    ShowError(ResHelper.GetString("ecommerce.orderpreview.errorsomethinghaschanged"));
                    e.SetValue(CHOP_FINALIZED_KEY, true);
                    return;
                }

                string validationMessage;
                // Validate cart; in case of failure user is able to go through checkout process and fix errors
                if (!ValidateShoppingCart(ShoppingCart, out validationMessage))
                {
                    e.CancelEvent = true;
                    ShowError(validationMessage);
                    e.SetValue(CHOP_FINALIZED_KEY, true);
                    return;
                }

                if (FinalizeCheckout())
                {
                    int orderId = ShoppingCart.OrderId;
                    string orderHash = ShoppingCart.GetHashCode().ToString();
                    WindowHelper.Add(orderHash, orderId);
                    // Create URL for payment page with order id hidden in hash
                    e.FinalStepUrl = URLHelper.AddParameterToUrl(e.FinalStepUrl, "o", orderHash);
                }
                else
                {
                    // Log events created in transaction
                    foreach (Tuple<string, string> error in loggedErrors)
                    {
                        EventLogProvider.LogEvent(EventType.ERROR, EVENT_SOURCE, error.Item2, error.Item1);
                    }

                    foreach (Exception ex in loggedExceptions)
                    {
                        EventLogProvider.LogException(EVENT_SOURCE, EVENT_CODE_EXCEPTION, ex);
                    }

                    e.CancelEvent = true;

                    // Get error text
                    string errorMessage = HTMLHelper.HTMLEncode(ResHelper.GetString("ecommerce.orderpreview.errorordersave"));

                    if (!string.IsNullOrEmpty(registrationBanned))
                    {
                        errorMessage += HTMLHelper.HTMLEncode(Environment.NewLine + registrationBanned);
                    }

                    ShowError(errorMessage);
                }

                CleanUpShoppingCart();
                DocumentWizardManager.ResetWizard();

                e.SetValue(CHOP_FINALIZED_KEY, true);
            }
        }

        #endregion


        #region "Methods"

        private void ShowError(string errorMessage)
        {
            // Try to show message through Message Panel web part
            CMSEventArgs<string> args = new CMSEventArgs<string>();
            args.Parameter = errorMessage;
            ComponentEvents.RequestEvents.RaiseEvent(this, args, MESSAGE_RAISED);

            // If Message Panel web part is not present (Parameter is cleared by web part after successful handling), show message through alert script
            if (!string.IsNullOrEmpty(args.Parameter))
            {
                errorMessage = errorMessage.Replace(HTML_SEPARATOR, Environment.NewLine);
                ScriptHelper.Alert(Page, errorMessage);
            }
        }


        /// <summary>
        /// Hides the user-defined content of the web part (envelope, title...).
        /// </summary>
        protected void HideWebPartContent()
        {
            ContentBefore = string.Empty;
            ContentAfter = string.Empty;
            ContainerTitle = string.Empty;
            ContainerName = string.Empty;
        }


        private bool FinalizeCheckout()
        {
            using (var scope = new CMSTransactionScope())
            {
                ShoppingCartInfo currentShoppingCart = ShoppingCart;

                // Validate breaking errors (No recovery)
                if (!CheckBreakingErrors(currentShoppingCart))
                {
                    return false;
                }
                // Create and save Order
                if (!CreateOrder(currentShoppingCart))
                {
                    return false;
                }

                if (!HandleAutoRegistration(currentShoppingCart))
                {
                    return false;
                }

                HandleOrderNotification(currentShoppingCart);
                scope.Commit();
            }

            return true;
        }


        /// <summary>
        /// Validates the shopping cart. In case of error user is able to go through CHOP again with the same cart object.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="errorMessage">The error message.</param>    
        private bool ValidateShoppingCart(ShoppingCartInfo shoppingCart, out string errorMessage)
        {
            bool valid = true;
            StringBuilder sb = new StringBuilder();

            // Check shopping cart items.
            // The following conditions must be met to pass the check:
            // 1) All shopping cart items are enabled
            // 2) Max units in one order are not exceeded 
            // 3) There is enough units in the inventory
            // 4) Customer is registered, if there is a membership type product in the cart
            // 5) Product validity is valid, if there is a membership or e-product type product in the cart
            var result = ShoppingCartInfoProvider.CheckShoppingCart(shoppingCart);
            if (result.CheckFailed || shoppingCart.IsEmpty)
            {
                valid = false;
                sb.Append(result.GetHTMLFormattedMessage());
                sb.Append(HTML_SEPARATOR);
            }

            // Check PaymentOption
            if (shoppingCart.PaymentOption == null)
            {
                valid = false;
                sb.Append(GetString("com.checkout.paymentoptionnotselected"));
                sb.Append(HTML_SEPARATOR);
            }

            // Check whether payment option is valid for user.
            string message;
            if (!CheckPaymentOptionIsValidForUser(shoppingCart, out message))
            {
                valid = false;
                sb.Append(message);
                sb.Append(HTML_SEPARATOR);
            }

            // If there is at least one product that needs shipping and shipping is not selected
            if (shoppingCart.IsShippingNeeded && (shoppingCart.ShippingOption == null))
            {
                valid = false;
                sb.Append(GetString("com.checkoutprocess.shippingneeded"));
                sb.Append(HTML_SEPARATOR);
            }

            // Check selected payment and shipping
            if (!CheckShippingAndPayment(shoppingCart, out message))
            {
                valid = false;
                sb.Append(message);
                sb.Append(HTML_SEPARATOR);
            }

            errorMessage = TextHelper.TrimEndingWord(sb.ToString(), HTML_SEPARATOR);
            return valid;
        }


        private bool CheckBreakingErrors(ShoppingCartInfo shoppingCart)
        {
            bool valid = true;
            // Check currency
            if (shoppingCart.Currency == null)
            {
                valid = false;
                LogError("Missing currency", EVENT_CODE_VALIDATION);
            }

            // Check customer
            if (shoppingCart.Customer == null)
            {
                valid = false;
                LogError("Missing customer", EVENT_CODE_VALIDATION);
            }

            // Check BillingAddress
            if (shoppingCart.ShoppingCartBillingAddress == null)
            {
                valid = false;
                LogError("Missing billing address", EVENT_CODE_VALIDATION);
            }

            return valid;
        }


        /// <summary>
        /// Assigns all shopping cart addresses to customer and saves it to database.
        /// </summary>
        /// <param name="shoppingCart">Shopping cart which addresses will be saved.</param>
        protected void SaveCustomerAddresses(ShoppingCartInfo shoppingCart)
        {
            shoppingCart.ShoppingCartBillingAddress = SaveAddress(shoppingCart.ShoppingCartBillingAddress, shoppingCart.Customer);
            shoppingCart.ShoppingCartShippingAddress = SaveAddress(shoppingCart.ShoppingCartShippingAddress, shoppingCart.Customer);
            shoppingCart.ShoppingCartCompanyAddress = SaveAddress(shoppingCart.ShoppingCartCompanyAddress, shoppingCart.Customer);

            // Update current contact's address
            MapContactAddress(shoppingCart.ShoppingCartBillingAddress);
        }


        private IAddress SaveAddress(IAddress addressObject, CustomerInfo customer)
        {
            AddressInfo address = addressObject as AddressInfo;

            if (address == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(address.AddressPersonalName))
            {
                address.AddressPersonalName = TextHelper.LimitLength($"{customer.CustomerFirstName} {customer.CustomerLastName}", 200);
            }

            address.AddressCustomerID = customer.CustomerID;

            if (string.IsNullOrEmpty(address.AddressName))
            {
                address.AddressName = AddressInfoProvider.GetAddressName(address);
            }

            AddressInfoProvider.SetAddressInfo(address);

            return address;
        }


        private bool CreateOrder(ShoppingCartInfo shoppingCart)
        {
            try
            {
                // Set order culture
                shoppingCart.ShoppingCartCulture = LocalizationContext.PreferredCultureCode;

                // Create order
                ShoppingCartInfoProvider.SetOrder(shoppingCart);
            }
            catch (Exception ex)
            {
                // Log exception
                loggedExceptions.Add(ex);
                return false;
            }

            if (shoppingCart.OrderId > 0)
            {
                // Track order conversion        
                var name = ECommerceSettings.OrderConversionName(shoppingCart.SiteName);
                ECommerceHelper.TrackOrderConversion(shoppingCart, name);
                ECommerceHelper.TrackOrderItemsConversions(shoppingCart);
                // Log purchase activity
                if (ActivitySettingsHelper.ActivitiesEnabledForThisUser(MembershipContext.AuthenticatedUser))
                {
                    var mainCurrency = CurrencyInfoProvider.GetMainCurrency(shoppingCart.ShoppingCartSiteID);
                    var grandTotalInMainCurrency = CurrencyConverter.Convert(shoppingCart.GrandTotal, shoppingCart.Currency.CurrencyCode, mainCurrency.CurrencyCode, shoppingCart.ShoppingCartSiteID);
                    var formattedPrice = CurrencyInfoProvider.GetFormattedPrice(grandTotalInMainCurrency, mainCurrency);

                    TrackActivityPurchasedProducts(shoppingCart, ContactID);
                    TrackActivityPurchase(shoppingCart.OrderId,
                                          ContactID,
                                          grandTotalInMainCurrency,
                                          formattedPrice);
                }

                return true;
            }

            LogError("Save order action failed", EVENT_CODE_SAVING);
            return false;
        }


        private static void HandleOrderNotification(ShoppingCartInfo shoppingCart)
        {
            if (ECommerceSettings.SendOrderNotification(shoppingCart.SiteName))
            {
                // Send order notification emails
                OrderInfoProvider.SendOrderNotificationToAdministrator(shoppingCart);
                OrderInfoProvider.SendOrderNotificationToCustomer(shoppingCart);
            }
        }


        private bool HandleAutoRegistration(ShoppingCartInfo currentShoppingCart)
        {
            registrationBanned = string.Empty;
            var customer = currentShoppingCart.Customer;

            if ((customer == null) || customer.CustomerIsRegistered)
            {
                return true;
            }

            var repository = Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(SiteContext.CurrentSiteID);

            if (repository.IsCustomerRegisteredAfterCheckout)
            {
                // Ban IP addresses which are blocked for registration
                var registrationBan = !BannedIPInfoProvider.IsAllowed(currentShoppingCart.SiteName, BanControlEnum.Registration);
                var allUserActionBan = !BannedIPInfoProvider.IsAllowed(currentShoppingCart.SiteName, BanControlEnum.AllNonComplete);

                if (registrationBan || allUserActionBan)
                {
                    registrationBanned = GetString("banip.ipisbannedregistration");
                    LogError(registrationBanned, EVENT_CODE_VALIDATION);
                    return false;
                }

                // Auto-register user and send mail notification
                CustomerInfoProvider.RegisterAndNotify(customer, repository.RegisteredAfterCheckoutTemplate);

                repository.Clear();
            }

            return true;
        }


        private void LogError(string errorMessage, string code)
        {
            loggedErrors.Add(new Tuple<string, string>(errorMessage, code));
        }


        /// <summary>
        /// Removes current shopping cart data from database and from session.
        /// </summary>
        private void CleanUpShoppingCart()
        {
            if (ShoppingCart != null)
            {
                ShoppingCartInfoProvider.DeleteShoppingCartInfo(ShoppingCart.ShoppingCartID);
                ECommerceContext.CurrentShoppingCart = null;
            }
        }


        /// <summary>
        /// Checks whether the payment option is valid for current user.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="message">The message in case of failure.</param>    
        protected bool CheckPaymentOptionIsValidForUser(ShoppingCartInfo shoppingCart, out string message)
        {
            message = string.Empty;
            CMSPaymentGatewayProvider provider = CMSPaymentGatewayProvider.GetPaymentGatewayProvider<CMSPaymentGatewayProvider>(shoppingCart.ShoppingCartPaymentOptionID);

            if ((provider != null) && (!provider.IsUserAuthorizedToFinishPayment(shoppingCart.User, shoppingCart)))
            {
                message = provider.ErrorMessage;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks whether selected shipping option and payment method are valid for given cart.
        /// </summary>
        protected bool CheckShippingAndPayment(ShoppingCartInfo cart, out string message)
        {
            message = String.Empty;
            if (!ShippingOptionInfoProvider.IsShippingOptionApplicable(cart, cart.ShippingOption))
            {
                message = GetString("com.checkout.shippingoptionnotapplicable");
                return false;
            }
            if (!PaymentOptionInfoProvider.IsPaymentOptionApplicable(cart, cart.PaymentOption))
            {
                message = GetString("com.checkout.paymentoptionnotapplicable");
                return false;
            }
            if (!cart.IsShippingNeeded && (cart.ShippingOption != null))
            {
                // Just remove the shipping option if shipping is not needed 
                cart.ShoppingCartShippingOptionID = 0;
            }
            if (!cart.IsShippingNeeded && (cart.PaymentOption != null) && !cart.PaymentOption.PaymentOptionAllowIfNoShipping)
            {
                // The selected payment is not applicable with no shipping
                message = GetString("com.checkout.paymentnoshipping");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Logs activity "purchase" for all items.
        /// </summary>
        /// <param name="shoppingCartInfoObj">Shopping cart</param>
        /// <param name="contactId">Contact ID</param>
        private void TrackActivityPurchasedProducts(ShoppingCartInfo shoppingCartInfoObj, int contactId)
        {
            // Check if shopping contains any items
            if ((shoppingCartInfoObj == null) || (shoppingCartInfoObj.IsEmpty))
            {
                return;
            }
            // Loop through all products and log activity
            foreach (ShoppingCartItemInfo cartItem in shoppingCartInfoObj.CartProducts)
            {
                mActivityLogger.LogPurchasedProductActivity(cartItem.SKU, cartItem.CartItemUnits, contactId);
            }
        }


        /// <summary>
        /// Logs activity "purchase".
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="totalPrice">Total price</param>
        /// <param name="totalPriceAsString">Total price user friendly formatted</param>
        private void TrackActivityPurchase(int orderId, int contactId, decimal totalPrice, string totalPriceAsString)
        {
            mActivityLogger.LogPurchaseActivity(orderId, totalPrice, totalPriceAsString, false, contactId);
        }


        /// <summary>
        /// Creates customer - current contact binding if does not exist and logs customer registration activity.
        /// </summary>
        /// <param name="customer">Customer to assign to the current contact.</param>
        private void AssignCustomerToContact(CustomerInfo customer)
        {
            Service.Resolve<IContactRelationAssigner>().Assign(customer.CustomerID, MemberTypeEnum.EcommerceCustomer, ContactID, new CustomerContactDataPropagationChecker());
        }


        /// <summary>
        /// Saves customer object. 
        /// </summary>
        /// <param name="customer">Customer object.</param>
        /// <param name="userID">ID of registered user, to connect newly created customer.</param>
        /// <param name="siteID">ID of the site, where customer was created.</param>
        protected CustomerInfo SaveCustomer(CustomerInfo customer, int userID, int siteID)
        {
            var newCustomer = (customer.CustomerID < 1);

            if (newCustomer && (userID > 0))
            {
                // Connect newly created customer with registered user -> if user returns to the site, customer`s data will be filled 
                customer.CustomerUserID = userID;
            }

            customer.CustomerSiteID = siteID;
            CustomerInfoProvider.SetCustomerInfo(customer);

            if (newCustomer)
            {
                AssignCustomerToContact(customer);
            }

            return customer;
        }


        /// <summary>
        /// Updates contact's address.
        /// </summary>
        /// <param name="address">Billing address</param>
        private void MapContactAddress(IAddress address)
        {
            try
            {
                Service.Resolve<IContactDataInjector>().Inject(address, ContactID, new AddressContactDataMapper(), new CustomerContactDataPropagationChecker());
            }
            catch (Exception ex)
            {
                // Exception could happen when max length of contact parameters is exceeded
                EventLogProvider.LogException("ShoppingCartOrderAddresses.MapContactAddress", "UPDATECONTACT", ex);
            }
        }

        #endregion
    }
}