using System;
using System.Linq;
using System.Text;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Global e-commerce events
    /// </summary>
    public static class EcommerceEvents
    {
        /// <summary>
        /// Fired when new order was created through checkout process.
        /// </summary>
        public static NewOrderCreatedHandler NewOrderCreated = new NewOrderCreatedHandler { Name = "EcommerceEvents.NewOrderCreated" };

        /// <summary>
        /// Fired when existing order has been paid.
        /// </summary>
        public static OrderPaidHandler OrderPaid = new OrderPaidHandler { Name = "EcommerceEvents.OrderPaid" };
    }
}
