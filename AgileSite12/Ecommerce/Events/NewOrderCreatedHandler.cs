
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// New order created (through checkout process) event handler
    /// </summary>
    public class NewOrderCreatedHandler : SimpleHandler<NewOrderCreatedHandler, NewOrderCreatedEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="order">The newly created order.</param>
        public NewOrderCreatedEventArgs StartEvent(OrderInfo order)
        {
            var e = new NewOrderCreatedEventArgs
            {
                NewOrder = order
            };

            StartEvent(e);

            return e;
        }
    }
}
