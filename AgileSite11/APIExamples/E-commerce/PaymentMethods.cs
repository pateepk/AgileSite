using System;

using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds payment method API examples.
    /// </summary>
    /// <pageTitle>Payment methods</pageTitle>
    internal class PaymentMethodsMain
    {
        /// <summary>
        /// Holds payment method API examples.
        /// </summary>
        /// <groupHeading>Payment methods</groupHeading>
        private class PaymentMethods
        {
            /// <heading>Creating a payment method</heading>
            private void CreatePaymentMethod()
            {
                // Creates a new payment method object
                PaymentOptionInfo newMethod = new PaymentOptionInfo();

                // Sets the payment method properties
                newMethod.PaymentOptionDisplayName = "New method";
                newMethod.PaymentOptionName = "NewMethod";
                newMethod.PaymentOptionSiteID = SiteContext.CurrentSiteID;
                newMethod.PaymentOptionEnabled = true;

                // Saves the payment method to the database
                PaymentOptionInfoProvider.SetPaymentOptionInfo(newMethod);
            }


            /// <heading>Updating a payment method</heading>
            private void GetAndUpdatePaymentMethod()
            {
                // Gets the payment method
                PaymentOptionInfo updateMethod = PaymentOptionInfoProvider.GetPaymentOptionInfo("NewMethod", SiteContext.CurrentSiteName);

                if (updateMethod != null)
                {
                    // Updates the payment method properties
                    updateMethod.PaymentOptionDisplayName = updateMethod.PaymentOptionDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    PaymentOptionInfoProvider.SetPaymentOptionInfo(updateMethod);
                }
            }


            /// <heading>Updating multiple payment methods</heading>
            private void GetAndBulkUpdatePaymentMethods()
            {
                // Gets all payment methods on the current site whose code name starts with 'New'
                var methods = PaymentOptionInfoProvider.GetPaymentOptions()
                                                          .OnSite(SiteContext.CurrentSiteID)
                                                          .WhereStartsWith("PaymentOptionName", "New");

                // Loops through the payment methods
                foreach (PaymentOptionInfo method in methods)
                {
                    // Updates the payment method properties
                    method.PaymentOptionDisplayName = method.PaymentOptionDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    PaymentOptionInfoProvider.SetPaymentOptionInfo(method);
                }
            }


            /// <heading>Deleting a payment method</heading>
            private void DeletePaymentMethod()
            {
                // Gets the payment method
                PaymentOptionInfo deleteMethod = PaymentOptionInfoProvider.GetPaymentOptionInfo("NewMethod", SiteContext.CurrentSiteName);

                if (deleteMethod != null)
                {
                    // Deletes the payment method from the database
                    PaymentOptionInfoProvider.DeletePaymentOptionInfo(deleteMethod.PaymentOptionID);
                }
            }
        }

        /// <summary>
        /// Holds customer credit API examples.
        /// </summary>
        /// <groupHeading>Customer credit</groupHeading>
        private class CustomerCredit
        {
            /// <heading>Creating a customer credit event</heading>
            private void CreateCreditEvent()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;
                
                if (customer != null)
                {
                    // Creates a new credit event object and sets up its properties
                    CreditEventInfo newEvent = new CreditEventInfo
                    {
                        EventName = "New event",
                        EventCreditChange = 500,
                        EventDate = DateTime.Now,
                        EventDescription = "Credit event description.",
                        EventCustomerID = customer.CustomerID,
                        EventSiteID = SiteContext.CurrentSiteID
                    };

                    // Saves the credit event to the database
                    CreditEventInfoProvider.SetCreditEventInfo(newEvent);
                }
            }


            /// <heading>Updating a customer credit event</heading>
            private void GetAndUpdateCreditEvent()
            {
                // Gets the credit event
                CreditEventInfo updateCredit = CreditEventInfoProvider.GetCreditEvents()
                                                            .WhereEquals("EventName", "NewEvent")
                                                            .FirstObject;

                if (updateCredit != null)
                {
                    // Updates the customer credit event properties
                    updateCredit.EventName = updateCredit.EventName.ToLowerCSafe();

                    // Saves the changes to the database
                    CreditEventInfoProvider.SetCreditEventInfo(updateCredit);
                }
            }


            /// <heading>Updating multiple customer credit events</heading>
            private void GetAndBulkUpdateCreditEvents()
            {
                // Gets all customer credit events whose name starts with 'New'
                var credits = CreditEventInfoProvider.GetCreditEvents().WhereStartsWith("EventName", "New");

                // Loops through the credit events
                foreach (CreditEventInfo updateCredit in credits)
                {
                    // Updates the credit event properties
                    updateCredit.EventName = updateCredit.EventName.ToUpperCSafe();

                    // Saves the changes to the database
                    CreditEventInfoProvider.SetCreditEventInfo(updateCredit);
                }
            }


            /// <heading>Getting a customer's total credit</heading>
            private void GetTotalCredit()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's total credit on the current site
                    decimal totalCredit = CreditEventInfoProvider.GetTotalCredit(customer.CustomerID, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Deleting a customer credit event</heading>
            private void DeleteCreditEvent()
            {
                // Gets the credit event
                CreditEventInfo deleteCredit = CreditEventInfoProvider.GetCreditEvents()
                                                                .WhereStartsWith("EventName", "New")
                                                                .FirstObject;

                if (deleteCredit != null)
                {
                    // Deletes the credit event from the database
                    CreditEventInfoProvider.DeleteCreditEventInfo(deleteCredit);
                }
            }
        }
    }
}
