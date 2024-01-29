using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Collection of customer orders represented by OrderInfo objects
    /// </summary>
    public class OrdersCollection : InfoObjectCollection<OrderInfo>
    {
        /// <summary>
        /// Total volume of all customer orders. Value is expressed in site main currency for site orders. 
        /// Result is in global main currency when collection contains orders from all sites.
        /// </summary>
        [RegisterProperty]
        public decimal TotalVolume => GetTotalVolume();


        /// <summary>
        /// The ID of customer whose orders should be loaded
        /// </summary>
        private int CustomerID
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor - creates collection of orders for the given customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="siteId">ID of the site from which the customer orders should be loaded. Use 0 to fill collection with customer orders from all sites.</param>
        public OrdersCollection(int customerId, int siteId)
        {
            CustomerID = customerId;
            SiteID = siteId;
            OrderByColumns = "OrderDate";
        }


        private decimal GetTotalVolume()
        {
            decimal total = 0;
            var exRateTables = new Dictionary<int, decimal>();

            // Sum all totals from underlying orders
            foreach (var order in this)
            {
                var orderTotal = order.OrderGrandTotalInMainCurrency;

                // Apply exchange rates when collection contains orders from different sites
                if (SiteID <= 0)
                {
                    decimal rate = 1;
                    var orderSiteID = order.OrderSiteID;

                    // Get exchange rate from dictionary if present
                    if (exRateTables.ContainsKey(orderSiteID))
                    {
                        rate = exRateTables[orderSiteID];
                    }
                    else
                    {
                        // Get exchange rate
                        CurrencyConverter.TryGetExchangeRate(true, CurrencyInfoProvider.GetMainCurrencyCode(orderSiteID), orderSiteID, ref rate);
                        exRateTables.Add(orderSiteID, rate);
                    }

                    // Apply exchange rate
                    orderTotal = CurrencyConverter.ApplyExchangeRate(orderTotal, 1 / rate);
                }

                total += orderTotal;
            }

            return total;
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public override IInfoObjectCollection<OrderInfo> Clone()
        {
            // Create new instance and copy over the properties
            var result = new OrdersCollection(CustomerID, SiteID);
            CopyPropertiesTo(result);

            return result;
        }


        /// <summary>
        /// Gets the complete where condition for the collection
        /// </summary>
        public override WhereCondition GetCompleteWhereCondition()
        {
            var baseWhere = base.GetCompleteWhereCondition();

            return baseWhere.WhereEquals("OrderCustomerID", CustomerID);
        }


        /// <summary>
        /// Copies the properties of this collection to the other collection
        /// </summary>
        /// <param name="col">Target collection</param>
        protected override void CopyPropertiesTo(IInfoObjectCollection col)
        {
            base.CopyPropertiesTo(col);

            var ordersCollection = col as OrdersCollection;
            if (ordersCollection != null)
            {
                ordersCollection.CustomerID = CustomerID;
            }
        }
    }
}