using System;
using System.Linq;

using CMS.Activities;
using CMS.Core;
using CMS.DataEngine;
using CMS.Scheduler;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides an ITask interface to mark abandoned shopping carts.
    /// </summary>
    public class MarkCartAbandoned : ITask
    {
        /// <summary>
        /// Executes the publish action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                var cartsTo = DateTime.Now.AddHours(0 - ECommerceSettings.MarkShoppingCartAsAbandonedPeriod(task.TaskSiteID));
                var cartsFrom = (task.TaskLastRunTime == TaskInfoProvider.NO_TIME) ? TaskInfoProvider.NO_TIME : cartsTo.Subtract(DateTime.Now.Subtract(task.TaskLastRunTime));

                // Get all non-empty expired shopping carts
                var carts = ShoppingCartInfoProvider.GetShoppingCarts(task.TaskSiteID)
                                                    .WhereGreaterOrEquals("ShoppingCartLastUpdate", cartsFrom)
                                                    .WhereLessThan("ShoppingCartLastUpdate", cartsTo)
                                                    .WhereNotNull("ShoppingCartContactID")
                                                    .WhereIn("ShoppingCartID", new IDQuery<ShoppingCartItemInfo>("ShoppingCartID"));
                
                var activityLogService = Service.Resolve<IActivityLogService>();
                foreach (ShoppingCartInfo cart in FilterAbandonedCarts(carts).Where(IsShoppingCartAbandoned))
                {
                    activityLogService.LogWithoutModifiersAndFilters(new AbandonedShoppingCartActivityInitializer(task.TaskSiteID, cart));
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        /// <summary>
        /// Checks if the given shopping cart is marked as abandoned and the ShoppingCartAbandoned activity is logged.
        /// </summary>
        /// <param name="cart">Shopping cart object</param>
        protected virtual bool IsShoppingCartAbandoned(ShoppingCartInfo cart)
        {
            return true;
        }


        /// <summary>
        /// Allows filter abandoned carts used to log the ShoppingCartAbandoned activity.
        /// </summary>
        /// <param name="abandonedCarts">All non-empty expired shopping carts</param>
        protected virtual ObjectQuery<ShoppingCartInfo> FilterAbandonedCarts(ObjectQuery<ShoppingCartInfo> abandonedCarts)
        {
            return abandonedCarts;
        }
    }
}