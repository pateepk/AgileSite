using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Event arguments for "new order created" event
    /// </summary>
    public class NewOrderCreatedEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Newly created order.
        /// </summary>        
        public OrderInfo NewOrder
        {
            get;
            set;
        }
    }
}
