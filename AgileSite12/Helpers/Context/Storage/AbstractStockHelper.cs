using System;
using System.Collections;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Abstract class for the stock helpers
    /// </summary>
    public abstract class AbstractStockHelper<THelper>
        where THelper : AbstractStockHelper<THelper>, new()
    {
        #region "Properties"

        /// <summary>
        /// Helper instance
        /// </summary>
        protected static THelper Instance = new THelper();


        /// <summary>
        /// Current items collection.
        /// </summary>
        protected abstract IDictionary CurrentItems
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static AbstractStockHelper()
        {
            TypeManager.RegisterGenericType(typeof(AbstractStockHelper<THelper>));
        }


        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static void Add(string key, object value, bool caseSensitive = false)
        {
            ItemsFunctions.Add(Instance.CurrentItems, key, value, caseSensitive);
        }


        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static void Remove(string key, bool caseSensitive = false)
        {
            ItemsFunctions.Remove(Instance.CurrentItems, key, caseSensitive);
        }


        /// <summary>
        /// Check if stock contain item.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static bool Contains(string key, bool caseSensitive = false)
        {
            return ItemsFunctions.Contains(Instance.CurrentItems, key, caseSensitive);
        }


        /// <summary>
        /// Returns object which matches by key.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static object GetItem(string key, bool caseSensitive = false)
        {
            return ItemsFunctions.GetItem(Instance.CurrentItems, key, caseSensitive);
        }


        /// <summary>
        /// Ensures the object in request items
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the object key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static ObjectType EnsureObject<ObjectType>(string key, bool caseSensitive = true)
            where ObjectType : new()
        {
            ObjectType data = (ObjectType)RequestStockHelper.GetItem(key, caseSensitive);
            if (data == null)
            {
                data = new ObjectType();

                RequestStockHelper.Add(key, data, caseSensitive);
            }

            return data;
        }

        #endregion


        #region "Storage methods"

        /// <summary>
        /// Executes the given action while ensuring the empty storage under the given key for the time being of the action.
        /// Restores original storage after the action.
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="key">Storage key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static void ExecuteWithEmptyStorage(Action action, string key, bool caseSensitive = true)
        {
            key = GetFullStorageKey(key);
            var originalStorage = GetItem(key, caseSensitive);
            DropStorage(key, caseSensitive);

            try
            {
                action();
            }
            finally
            {
                Add(key, originalStorage, caseSensitive);
            }
        }


        /// <summary>
        /// Gets the storage
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        /// <param name="ensure">If true, the storage is ensured if it doesn't exist</param>
        public static SafeDictionary<string, object> GetStorage(string key, bool caseSensitive = true, bool ensure = true)
        {
            key = GetFullStorageKey(key);

            // Try get storage (use same case sensitivity)
            var result = (SafeDictionary<string, object>)GetItem(key, caseSensitive);
            if ((result == null) && ensure)
            {
                // Ensure new storage with same case sensitivity
                result = new SafeDictionary<string, object>();
                Add(key, result, caseSensitive);
            }

            return result;
        }


        /// <summary>
        /// Gets the storage key
        /// </summary>
        /// <param name="key">Storage key</param>
        private static string GetFullStorageKey(string key)
        {
            return "storage|" + key;
        }


        /// <summary>
        /// Drops the storage
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static void DropStorage(string key, bool caseSensitive = true)
        {
            key = GetFullStorageKey(key);

            // Drop storage
            Remove(key, caseSensitive);
        }


        /// <summary>
        /// Adds item to specific storage specified by <paramref name="storageKey"/>.
        /// </summary>
        /// <param name="storageKey">Storage key</param>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static void AddToStorage(string storageKey, string key, object value, bool caseSensitive = true)
        {
            var storage = GetStorage(storageKey, caseSensitive);

            ItemsFunctions.Add(storage, key, value, caseSensitive);
        }


        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="storageKey">Storage key</param>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static void Remove(string storageKey, string key, bool caseSensitive = true)
        {
            var storage = GetStorage(storageKey, caseSensitive, false);
            if (storage == null)
            {
                return;
            }

            ItemsFunctions.Remove(storage, key, caseSensitive);
        }


        /// <summary>
        /// Check if stock contain item.
        /// </summary>
        /// <param name="storageKey">Storage key</param>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static bool Contains(string storageKey, string key, bool caseSensitive = true)
        {
            var storage = GetStorage(storageKey, caseSensitive, false);
            if (storage == null)
            {
                return false;
            }

            return ItemsFunctions.Contains(storage, key, caseSensitive);
        }


        /// <summary>
        /// Returns object which matches by key.
        /// </summary>
        /// <param name="storageKey">Storage key</param>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive, otherwise key is converted to lowercase before use</param>
        public static object GetItem(string storageKey, string key, bool caseSensitive = true)
        {
            var storage = GetStorage(storageKey, caseSensitive, false);
            if (storage == null)
            {
                return null;
            }

            return ItemsFunctions.GetItem(storage, key, caseSensitive);
        }

        #endregion
    }
}