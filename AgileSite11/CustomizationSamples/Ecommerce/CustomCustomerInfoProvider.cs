using System;

using CMS;
using CMS.Ecommerce;
using CMS.SiteProvider;

[assembly: RegisterCustomProvider(typeof(CustomCustomerInfoProvider))]

/// <summary>
/// Sample customer info provider. 
/// </summary>
public class CustomCustomerInfoProvider : CustomerInfoProvider
{
    #region "Example: Add extra credit for each new customer"

    /// <summary>
    /// Sets (updates or inserts) specified customer.
    /// </summary>
    /// <param name="customerObj">Customer to be set</param>   
    protected override void SetCustomerInfoInternal(CustomerInfo customerObj)
    {
        // Remember whether it is a new customer
        bool newCustomer = ((customerObj != null) && (customerObj.CustomerID <= 0));

        // Update or insert customer
        base.SetCustomerInfoInternal(customerObj);

        // Add extra credit for each new registered customer 
        if (newCustomer && customerObj.CustomerIsRegistered)
        {
            // Initialize credit event
            CreditEventInfo extraCredit = new CreditEventInfo();
            extraCredit.EventName = "Extra credit for a new customer";
            extraCredit.EventDate = DateTime.Now;
            extraCredit.EventDescription = "This is a starting credit for a new customer.";
            extraCredit.EventCustomerID = customerObj.CustomerID;

            // Credit event value in site main currency
            extraCredit.EventCreditChange = 100;

            // Set credit as site credit or as global credit according to the settings
            extraCredit.EventSiteID = ECommerceHelper.GetSiteID(SiteContext.CurrentSiteID, ECommerceSettings.USE_GLOBAL_CREDIT);

            // Save credit event
            CreditEventInfoProvider.SetCreditEventInfo(extraCredit);
        }
    }

    #endregion
}