using System;
using System.Linq;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce.Internal;
using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

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

        private const string HTML_SEPARATOR = "<br />";

        #endregion


        #region "Variables"

        private int mContactID;

        private ShoppingCartInfo mShoppingCart;
        private IShoppingService mShoppingService;

        #endregion


        #region "Properties"

        /// <summary>
        /// Shopping service for the current shopping cart.
        /// </summary>
        protected IShoppingService ShoppingService
        {
            get
            {
                return mShoppingService ?? (mShoppingService = Service.Resolve<IShoppingService>());
            }
        }


        /// <summary>
        /// Current ShoppingCart
        /// </summary>
        public ShoppingCartInfo ShoppingCart
        {
            get
            {
                return mShoppingCart ?? (mShoppingCart = ShoppingService.GetCurrentShoppingCart());
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
                var currentUserId = ShoppingCart.User?.UserID ?? 0;
                // Get last known user saved in previous load of WP. In case there is no record in session current user id is retrieved.
                var userIdSessionObject = SessionHelper.GetValue("CheckoutUserID");
                var lastKnownUserID = ValidationHelper.GetInteger(userIdSessionObject, currentUserId);

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

                // Remember the hashcode of the current shopping cart from which order is going to be created because
                // shoppingService.CreateOrder() deletes shopping cart after creating order then new ShoppingCart is created when requested
                var orderHash = ShoppingCart.GetHashCode().ToString();
                
                // Create order
                var shoppingService = Service.Resolve<IShoppingService>();

                int orderId;

                try
                {
                    orderId = shoppingService.CreateOrder().OrderID;                  
                }
                catch (InvalidOperationException ex)
                {
                    e.CancelEvent = true;
                    ShowError(ex.Message);
                    e.SetValue(CHOP_FINALIZED_KEY, true);
                    return;
                }

                WindowHelper.Add(orderHash, orderId);

                // Create URL for payment page with order id hidden in hash
                e.FinalStepUrl = URLHelper.AddParameterToUrl(e.FinalStepUrl, "o", orderHash);
                
                DocumentWizardManager.ResetWizard();

                e.SetValue(CHOP_FINALIZED_KEY, true);
            }
        }

        #endregion


        #region "Methods"

        private void ShowError(string errorMessage)
        {
            // Try to show message through Message Panel web part
            var args = new CMSEventArgs<string>();
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
        

        /// <summary>
        /// Validates the shopping cart. In case of error user is able to go through CHOP again with the same cart object.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="errorMessage">The error message.</param>
        private bool ValidateShoppingCart(ShoppingCartInfo shoppingCart, out string errorMessage)
        {
            errorMessage = String.Empty;
            
            var validator = new CreateOrderValidator(shoppingCart);
            validator.Validate();

            if (!validator.IsValid)
            {
                errorMessage = validator.GetErrorMessages().Join(HTML_SEPARATOR);
            }

            return validator.IsValid;
        }


        /// <summary>
        /// Assigns all shopping cart addresses to customer and saves it to database.
        /// </summary>
        /// <param name="shoppingCart">Shopping cart which addresses will be saved.</param>
        protected void SaveCustomerAddresses(ShoppingCartInfo shoppingCart)
        {
            var billingAddress = shoppingCart.ShoppingCartBillingAddress;
            var shippingAddress = shoppingCart.ShoppingCartShippingAddress;
            
            if (billingAddress != null)
            {
                ShoppingService.SetBillingAddress(billingAddress);
            }

            if (shippingAddress != null)
            {
                ShoppingService.SetShippingAddress(shippingAddress);
            }

            shoppingCart.ShoppingCartCompanyAddress = SaveAddress(shoppingCart.ShoppingCartCompanyAddress, shoppingCart.Customer);

            // Update current contact's address
            MapContactAddress(billingAddress);
        }


        private AddressInfo SaveAddress(AddressInfo addressObject, CustomerInfo customer)
        {
            var address = addressObject;

            if (address == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(address.AddressPersonalName))
            {
                address.AddressPersonalName = AddressInfoProvider.GetAddressPersonalName(customer);
            }

            address.AddressCustomerID = customer.CustomerID;

            if (string.IsNullOrEmpty(address.AddressName))
            {
                address.AddressName = AddressInfoProvider.GetAddressName(address);
            }

            AddressInfoProvider.SetAddressInfo(address);

            return address;
        }


        /// <summary>
        /// Updates contact's address.
        /// </summary>
        /// <param name="address">Billing address</param>
        private void MapContactAddress(AddressInfo address)
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