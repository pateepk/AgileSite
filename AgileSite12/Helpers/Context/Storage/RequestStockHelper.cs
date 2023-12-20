using System.Collections;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Inserts items into RequestItems.Items.
    /// </summary>
    public class RequestStockHelper : AbstractStockHelper<RequestStockHelper>
    {
        /// <summary>
        /// Gets the current items.
        /// </summary>
        protected override IDictionary CurrentItems
        {
            get
            {
                return RequestItems.CurrentItems;
            }
        }
    }
}