using System;
using System.Linq;

using CMS.Base.Web.UI;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;


/// <summary>
/// Payment selector web part
/// </summary>
public partial class CMSWebParts_Ecommerce_Checkout_Selectors_PaymentMethodSelection : CMSCheckoutWebPart
{
    #region "Event handling"

    /// <summary>
    /// OnInit event handler.
    /// </summary>
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        // Subscribe to the wizard events
        SubscribeToWizardEvents();
    }


    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        SetupControl();
    }


    /// <summary>
    /// Handles the payment option change and rises the <see cref="CMSCheckoutWebPart.SHOPPING_CART_CHANGED"/>.
    /// </summary>
    protected void PaymentSelected(object sender, EventArgs e)
    {
        // Only if selection is different
        if (ShoppingCart.ShoppingCartPaymentOptionID != drpPayment.SelectedID)
        {
            ShoppingCart.ShoppingCartPaymentOptionID = drpPayment.SelectedID;
            ShoppingCart.Evaluate();
            ShoppingCartInfoProvider.SetShoppingCartInfo(ShoppingCart);

            // Make sure that in-memory changes persist (unsaved address, etc.)
            ECommerceContext.CurrentShoppingCart = ShoppingCart;

            // Raise the change event for all subscribed web parts
            ComponentEvents.RequestEvents.RaiseEvent(this, e, SHOPPING_CART_CHANGED);
        }
    }


    /// <summary>
    /// Updates the web part according to the new shopping cart values.
    /// </summary>
    private void Update(object sender, EventArgs e)
    {
        if (sender != this)
        {
            LoadControlData(true);
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Validates the data.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">The StepEventArgs instance containing the event data.</param>
    protected override void ValidateStepData(object sender, StepEventArgs e)
    {
        base.ValidateStepData(sender, e);

        bool valid = true;
        lblError.Visible = false;
        string message;

        if (drpPayment.SelectedID == 0)
        {
            message = ResHelper.GetString("com.checkoutprocess.paymentneeded");
            valid = false;
        }
        // Check whether payment option is valid for user.
        else if (!CheckPaymentOptionIsValidForUser(ShoppingCart, out message))
        {
            valid = false;
        }

        if (!valid)
        {
            e.CancelEvent = true;
            lblError.Text = message;
            lblError.Visible = true;
        }
    }


    /// <summary>
    /// Subscribes the web part to the wizard events.
    /// </summary>
    private void SubscribeToWizardEvents()
    {
        ComponentEvents.RequestEvents.RegisterForEvent(SHOPPING_CART_CHANGED, Update);
    }


    /// <summary>
    /// Control initialization.
    /// </summary>
    private void SetupControl()
    {
        if (!StopProcessing)
        {
            // Set up empty record text; the macro ResourcePrefix + .empty represents empty record value
            drpPayment.UniSelector.ResourcePrefix = "com.livesiteselector";

            // Set shopping cart to determine available payment options
            drpPayment.ShoppingCart = ShoppingCart;

            LoadControlData();
        }
    }


    private void LoadControlData(bool forceReload = false)
    {
        var paymentOptions = PaymentOptionInfoProvider.GetPaymentOptions(ShoppingCart.ShoppingCartSiteID, true)
                            .Column("PaymentOptionID")
                            .OrderBy("PaymentOptionDisplayName");

        if (ShoppingCart.ShippingOption == null || !ShoppingCart.IsShippingNeeded)
        {
            paymentOptions.WhereTrue("PaymentOptionAllowIfNoShipping");
        }

        var paymentIds = paymentOptions.GetListResult<int>();

        // If there is only one payment method set it
        if (paymentIds.Count == 1)
        {
            ShoppingCart.ShoppingCartPaymentOptionID = paymentIds.First();
            drpPayment.SelectedID = ShoppingCart.ShoppingCartPaymentOptionID;
        }

        drpPayment.DisplayOnlyAllowedIfNoShipping = (ShoppingCart.ShoppingCartShippingOptionID <= 0) || !ShoppingCart.IsShippingNeeded;

        if (!RequestHelper.IsPostBack() || forceReload || ((ShoppingCart.ShoppingCartPaymentOptionID != 0) && !PaymentOptionInfoProvider.IsPaymentOptionApplicable(ShoppingCart, ShoppingCart.PaymentOption)))
        {
            // Reset selector on shipping changed event if selected payment is not allowed for current shipping (zero shipping id is Please select state).
            drpPayment.Reload();
            drpPayment.SelectedID = ShoppingCart.ShoppingCartPaymentOptionID;
        }

        // Evaluate cart if payment was pre-selected
        if (ShoppingCart.ItemChanged("ShoppingCartPaymentOptionID"))
        {
            ShoppingCart.Evaluate();
            ShoppingCartInfoProvider.SetShoppingCartInfo(ShoppingCart);

            // Make sure that in-memory changes persist (unsaved address, etc.)
            ECommerceContext.CurrentShoppingCart = ShoppingCart;
        }
    }

    #endregion
}