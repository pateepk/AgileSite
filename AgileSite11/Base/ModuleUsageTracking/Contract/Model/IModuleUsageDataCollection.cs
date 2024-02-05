using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Collection of module usage data
    /// </summary>
    public interface IModuleUsageDataCollection : IEnumerable<IModuleUsageDataItem>, ISerializable
    {
        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, string value);


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, byte[] value);


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, bool value);


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, DateTime value);


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, double value);


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, Guid value);


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, int value);


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        bool Add(string key, long value);


        /// <summary>
        /// Gets item with specified key.
        /// </summary>
        /// <param name="key">Key of item to retrieve</param>
        /// <returns>Item if found, null otherwise</returns>
        IModuleUsageDataItem Get(string key);


        /// <summary>
        /// Remove item from collection.
        /// </summary>
        /// <param name="key">Key of item to remove</param>
        /// <returns>True when item was removed, false if item wasn't present in collection.</returns>
        bool Remove(string key);
    }
}
