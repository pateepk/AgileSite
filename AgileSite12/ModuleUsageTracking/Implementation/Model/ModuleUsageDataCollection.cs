using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using CMS.Base;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Collection of module usage data provided by module usage data source
    /// </summary>
    [Serializable]
    internal class ModuleUsageDataCollection : IModuleUsageDataCollection
    {
        #region "Variables"

        private readonly HashSet<ModuleUsageDataItem> mItems = new HashSet<ModuleUsageDataItem>();

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ModuleUsageDataCollection()
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
            return AddInternal(key, value);
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, byte[] value)
        {
            return AddInternal(key, value);
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, bool value)
        {
            return AddInternal(key, value);
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, DateTime value)
        {
            return AddInternal(key, value);
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, double value)
        {
            return AddInternal(key, value);
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, Guid value)
        {
            return AddInternal(key, value);
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, int value)
        {
            return AddInternal(key, value);
        }


        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        public bool Add(string key, long value)
        {
            return AddInternal(key, value);
        }


        /// <summary>
        /// Gets item with specified key.
        /// </summary>
        /// <param name="key">Key of item to retrieve</param>
        /// <returns>Item if found, null otherwise</returns>
        public IModuleUsageDataItem Get(string key)
        {
            var searchItem = new ModuleUsageDataItem(key);
            return mItems.FirstOrDefault(item => item.Equals(searchItem));
        }


        /// <summary>
        /// Remove item from collection.
        /// </summary>
        /// <param name="key">Key of item to remove</param>
        /// <returns>True if item was removed, false if item wasn't present in collection.</returns>
        public bool Remove(string key)
        {
            return mItems.Remove(new ModuleUsageDataItem(key));
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<IModuleUsageDataItem> GetEnumerator()
        {
            return mItems.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Adds usage data item to collection.
        /// </summary>
        /// <param name="key">Item identifier</param>
        /// <param name="value">Item value</param>
        /// <returns>True if the element is added, false if the element is already present.</returns>
        private bool AddInternal(string key, object value)
        {
            return mItems.Add(new ModuleUsageDataItem(key, value));
        }

        #endregion


        #region "Serialization methods"

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Items", mItems);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        public ModuleUsageDataCollection(SerializationInfo info, StreamingContext context)
        {
            mItems = (HashSet<ModuleUsageDataItem>)info.GetValue("Items", typeof(HashSet<ModuleUsageDataItem>));
        }

        #endregion
    }
}
