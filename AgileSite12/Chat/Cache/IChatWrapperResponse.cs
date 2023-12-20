using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// This interface is returned from Chat cache wrappers when collection of items and its last modification time are needed.
    /// </summary>
    /// <typeparam name="TData">Type of one item in collection</typeparam>
    public interface ICacheWrapperResponse<TData>
    {
        /// <summary>
        /// Collection of items.
        /// </summary>
        ICollection<TData> Items { get; }


        /// <summary>
        /// DateTime of last modification time of Items.
        /// </summary>
        DateTime LastChange { get; }
    }
}
