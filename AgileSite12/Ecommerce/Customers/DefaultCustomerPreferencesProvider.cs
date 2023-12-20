using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="ICustomerPreferencesProvider"/> taking customer's preferences 
    /// from the last order placed on respective site.
    /// </summary>
    public class DefaultCustomerPreferencesProvider : ICustomerPreferencesProvider
    {
        /// <summary>
        /// Gets customer's preferences on the site specified by <paramref name="site"/> parameter.
        /// </summary>
        /// <param name="customer">Customer for who the preferences are obtained.</param>
        /// <param name="site">Identifier of the site for which the preferences are obtained.</param>
        /// <returns>Customer's preferences on given site.</returns>
        public CustomerPreferences GetPreferences(CustomerInfo customer, SiteInfoIdentifier site)
        {
            if ((customer != null) && (customer.CustomerID != 0))
            {
                var lastOrder = OrderInfoProvider.GetOrders(site)
                    .WhereEquals("OrderCustomerID", customer.CustomerID)
                    .OrderByDescending("OrderDate")
                    .TopN(1)
                    .Columns("OrderCurrencyID", "OrderPaymentOptionID", "OrderShippingOptionID")
                    .FirstOrDefault();

                if (lastOrder != null)
                {
                    int? currencyID = lastOrder.OrderCurrencyID;
                    int? shippingID = lastOrder.OrderShippingOptionID;
                    int? paymentID = lastOrder.OrderPaymentOptionID;

                    return new CustomerPreferences(
                        currencyID > 0 ? currencyID : null, 
                        shippingID > 0 ? shippingID : null,
                        paymentID > 0 ? paymentID : null);
                }
            }

            return CustomerPreferences.Unknown;
        }
    }
}
