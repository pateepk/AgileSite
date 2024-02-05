using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Scheduler;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides an ITask interface to delete old shopping carts.
    /// </summary>
    public class ShoppingCartCleaner : ITask
    {
        /// <summary>
        /// Executes the action deleting old shopping carts from database.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                // Get all expired shopping carts
                var carts = GetExpiredShoppingCarts(task.TaskSiteID);
                foreach (var cart in carts)
                {
                    // Get customer 
                    var customer = CustomerInfoProvider.GetCustomerInfo(cart.ShoppingCartCustomerID);

                    // Delete shopping cart
                    cart.Delete();

                    // Delete anonymous customers without any order    
                    if ((customer != null) && !customer.CustomerIsRegistered)
                    {
                        // Get customer's orders
                        var orders = OrderInfoProvider.GetOrders()
                            .WhereEquals("OrderCustomerID", customer.CustomerID)
                            .TopN(1);

                        // No orders -> Delete the customer
                        if (!orders.Any())
                        {
                            customer.Delete();
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        /// <summary>
        /// Returns shopping carts that are expired on given site.
        /// </summary>
        /// <param name="siteId">ID of the site where shopping carts are examined.</param> 
        protected virtual List<ShoppingCartInfo> GetExpiredShoppingCarts(int siteId)
        {
            // Get expiration date
            var expirationDate = DateTime.Now.AddDays(0 - ECommerceSettings.ShoppingCartExpirationPeriod(siteId));

            return ShoppingCartInfoProvider.GetShoppingCarts(siteId)
                .WhereLessThan("ShoppingCartLastUpdate", expirationDate)
                .ToList();
        }
    }
}