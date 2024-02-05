using System;

using CMS;
using CMS.Ecommerce;
using CMS.SiteProvider;

[assembly: RegisterCustomProvider(typeof(CustomOrderInfoProvider))]

/// <summary>
/// Sample order info provider. 
/// </summary>
public class CustomOrderInfoProvider : OrderInfoProvider
{
    #region "Example: Add specific amount of credit to each customer after he makes an order."

    /// <summary>
    /// Sets (updates or inserts) specified order.
    /// </summary>
    /// <param name="orderObj">Order to be set</param>  
    protected override void SetOrderInfoInternal(OrderInfo orderObj)
    {
        // Remember whether it is a new order
        bool newOrder = ((orderObj != null) && (orderObj.OrderID <= 0));

        // Add/Update the order
        base.SetOrderInfoInternal(orderObj);

        // Add credit to the customer for a new order with the total price higher than 1000 
        if (newOrder && (orderObj.OrderGrandTotalInMainCurrency > 1000))
        {
            // Initialize credit event
            CreditEventInfo extraCredit = new CreditEventInfo();
            extraCredit.EventName = string.Format("Credit for your order: {0}", orderObj.OrderID);
            extraCredit.EventDate = DateTime.Now;
            extraCredit.EventDescription = "Thank you for your order.";
            extraCredit.EventCustomerID = orderObj.OrderCustomerID;

            // Credit in site main currency which will be added to the customer
            extraCredit.EventCreditChange = 10;

            // Set credit as site credit or as global credit according to the settings
            extraCredit.EventSiteID = ECommerceHelper.GetSiteID(SiteContext.CurrentSiteID, ECommerceSettings.USE_GLOBAL_CREDIT);

            // Save credit event
            CreditEventInfoProvider.SetCreditEventInfo(extraCredit);
        }
    }

    #endregion
}