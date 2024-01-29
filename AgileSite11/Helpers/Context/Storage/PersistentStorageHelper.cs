using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Persistent storage management.
    /// </summary>
    public static class PersistentStorageHelper
    {
        #region "Constants"

        /// <summary>
        /// Prefix used for unique identification of storage item locks.
        /// </summary>
        private const string ITEM_LOCK_OBJECTS_KEY_PREFIX = "PersistentStorageHelper-";


        /// <summary>
        /// Determines how long are the persistent items kept in persistent storage.
        /// </summary>
        private static readonly TimeSpan PERSISTENT_ITEMS_KEEP_INTERVAL = new TimeSpan(24, 0, 0);

        #endregion


        #region "Variables"

        /// <summary>
        /// Persistent storage manager used by this helper. Guarantees persistence for <see cref="PERSISTENT_ITEMS_KEEP_INTERVAL"/>.
        /// The storage operates within the default directory.
        /// </summary>
        private static readonly PersistentStorageManager mPersistentStorageManager = new PersistentStorageManager(ITEM_LOCK_OBJECTS_KEY_PREFIX, PERSISTENT_ITEMS_KEEP_INTERVAL);

        
        /// <summary>
        /// Cache for persisted items.
        /// </summary>
        private static readonly Dictionary<string, object> mValues = new Dictionary<string, object>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Persistent data directory.
        /// </summary>
        public static string PersistentDirectory
        {
            get
            {
                return mPersistentStorageManager.PersistentDirectory;
            }
            set
            {
                mPersistentStorageManager.PersistentDirectory = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the value of persistent item identified by <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Persistent item key</param>
        public static object GetValue(string key)
        {
            if (key == null)
            {
                return null;
            }
            key = key.ToLowerCSafe();

            // Get the value from file
            return mPersistentStorageManager.GetItemFromStorage(key, () => !mValues.ContainsKey(key), value => mValues[key] = value, () => mValues[key]);
        }


        /// <summary>
        /// Sets a persistent item identified by <paramref name="key"/>.
        /// Setting a null <paramref name="value"/> has the same effect as a call to <see cref="RemoveValue"/>.
        /// Does nothing when <paramref name="key"/> is null.
        /// </summary>
        /// <param name="key">Persistent item key</param>
        /// <param name="value">Persistent item value</param>
        public static void SetValue(string key, object value)
        {
            if (key == null)
            {
                return;
            }
            key = key.ToLowerCSafe();

            if (value == null)
            {
                // No value, delete the file
                mPersistentStorageManager.DeleteItemFromStorage(key, () => mValues.Remove(key));
            }
            else
            {
                // Save to the file and to the local table
                mPersistentStorageManager.SaveItemToStorage(key, value, () => mValues[key] = value);
                WebFarmHelper.CreateTask(CacheTaskType.RemovePersistentStorageKey, String.Empty, key);
            }

            mPersistentStorageManager.DeleteExpiredStorageItems(removedKey => mValues.Remove(removedKey));
        }


        /// <summary>
        /// Removes a persistent item identified by <paramref name="key"/> from storage.
        /// </summary>
        /// <param name="key">Persistent item key</param>
        public static void RemoveValue(string key)
        {
            SetValue(key, null);
        }


        /// <summary>
        /// Removes a key from <see cref="PersistentStorageHelper"/> internal cache, so the next request for value
        /// of given key will be taken from file system.
        /// </summary>
        /// <param name="key">Persistent item key</param>
        internal static void RemoveKeyFromInternalCache(string key)
        {
            if (key != null)
            {
                mValues.Remove(key);
            }
        }

        #endregion
    }
}