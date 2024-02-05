using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Change type of cacheable item.
    /// </summary>
    public enum ChangeTypeEnum
    {
        /// <summary>
        /// Item was changed or modified.
        /// </summary>
        Modify = 0,

        /// <summary>
        /// Item was removed.
        /// </summary>
        Remove = 1,
    }

    /// <summary>
    /// Items cached in CurrentStateCache has to implement this interface.
    /// </summary>
    /// <typeparam name="TKey">Type of key of this item. Unique representation of one object.</typeparam>
    public interface IChatCacheableWithCurrentState<TKey> : IChatIncrementalCacheable
    {
        /// <summary>
        /// Type of change. If it is set to Modify, item with this PK will be added/modified in current state cache.
        /// </summary>
        ChangeTypeEnum ChangeType { get; }


        /// <summary>
        /// Primary key of this item.
        /// </summary>
        TKey PK { get; }
    }
}
