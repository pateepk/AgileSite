using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Base
{
    /// <summary>
    /// Collection of module usage data provided by module usage data source
    /// </summary>
    /// <remarks>
    /// This is default empty implementation which doesn't store any value. 
    /// Module usage tracking module registers its own implementation of the interface when installed.
    /// </remarks>
    public class DefaultModuleUsageDataCollection : IModuleUsageDataCollection
    {
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<IModuleUsageDataItem> GetEnumerator()
        {
            yield break;
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, string value)
        {
            return false;
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, byte[] value)
        {
            return false;
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, bool value)
        {
            return false;
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, DateTime value)
        {
            return false;
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, double value)
        {
            return false;
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, Guid value)
        {
            return false;
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, int value)
        {
            return false;
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, long value)
        {
            return false;
        }


        /// <summary>
        /// Gets item with specified key.
        /// </summary>
        /// <param name="key">Key of item to retrieve</param>
        /// <returns>Item if found, null otherwise</returns>
        public IModuleUsageDataItem Get(string key)
        {
            return null;
        }


        /// <summary>
        /// Remove item from collection.
        /// </summary>
        /// <param name="key">Key of item to remove</param>
        /// <returns>True when item was removed, false if item wasn't present in collection.</returns>
        public bool Remove(string key)
        {
            return false;
        }
    }
}
