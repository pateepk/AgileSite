using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Window objects management.
    /// </summary>
    public static class WindowHelper
    {
        #region "Methods"
        
        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static void Add(string key, object value, bool caseSensitive = false)
        {
            if (key == null)
            {
                return;
            }

            key = GetInternalKey(key, caseSensitive);

            SessionHelper.SetValue(key, value, true);
        }


        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static void Remove(string key, bool caseSensitive = false)
        {
            Add(key, null, caseSensitive);
        }


        /// <summary>
        /// Check if stock contain item.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static bool Contains(string key, bool caseSensitive)
        {
            if (key == null)
            {
                return false;
            }

            key = GetInternalKey(key, caseSensitive);

            return SessionHelper.GetValue(key) != null;
        }


        /// <summary>
        /// Gets the internal key representation for the given key
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        private static string GetInternalKey(string key, bool caseSensitive)
        {
            if ((key != null) && !caseSensitive)
            {
                key = key.ToLowerCSafe();
            }

            return "window_" + key;
        }


        /// <summary>
        /// Returns object which matches by key.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static object GetItem(string key, bool caseSensitive = false)
        {
            if (key == null)
            {
                return null;
            }

            key = GetInternalKey(key, caseSensitive);

            return SessionHelper.GetValue(key);
        }


        /// <summary>
        /// Clears the session content starting with given string
        /// </summary>
        /// <param name="startsWith">If null, removes all session items, if set, remove only items starting with given string</param>
        public static void Clear(string startsWith = null)
        {
            startsWith = GetInternalKey(startsWith, true);

            SessionHelper.Clear(startsWith);
        }

        #endregion
    }
}