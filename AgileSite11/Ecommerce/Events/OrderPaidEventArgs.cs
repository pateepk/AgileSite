using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Event arguments for "order paid" event
    /// </summary>
    public class OrderPaidEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Paid order.
        /// </summary>        
        public OrderInfo Order
        {
            get;
            set;
        }
    }
}
