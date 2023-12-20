using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Order paid event handler
    /// </summary>
    public class OrderPaidHandler : SimpleHandler<OrderPaidHandler, OrderPaidEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="order">The paid order.</param>
        public OrderPaidEventArgs StartEvent(OrderInfo order)
        {
            var e = new OrderPaidEventArgs
            {
                Order = order,
            };

            StartEvent(e);

            return e;
        }
    }
}
