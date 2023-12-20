using System.Collections;

using CMS.Helpers;

namespace CMS.Base
{
    /// <summary>
    /// Inserts items into HttpContext.Current.Items.
    /// </summary>
    public static class RequestItems
    {
        #region "Properties"

        /// <summary>
        /// Gets the current items.
        /// </summary>
        public static IDictionary CurrentItems
        {
            get
            {
                if (CMSHttpContext.Current != null)
                {
                    return CMSHttpContext.Current.Items;
                }

                return ThreadItems.CurrentItems;
            }
        }
        

        /// <summary>
        /// Gets the current items or null if they don't exist
        /// </summary>
        public static IDictionary ReadOnlyCurrentItems
        {
            get
            {
                if (CMSHttpContext.Current != null)
                {
                    return CMSHttpContext.Current.Items;
                }

                return ThreadItems.ReadOnlyCurrentItems;
            }
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        internal static void Add(string key, object value, bool caseSensitive)
        {
            ItemsFunctions.Add(CurrentItems, key, value, caseSensitive);
        }
        

        /// <summary>
        /// Returns object which matches by key.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        internal static object GetItem(string key, bool caseSensitive)
        {
            return ItemsFunctions.GetItem(ReadOnlyCurrentItems, key, caseSensitive);
        }


        /// <summary>
        /// Remove items which start with selected name.
        /// </summary>
        /// <param name="startsWith">Starting string of the objects to be removed</param>
        public static void Clear(string startsWith)
        {
            ItemsFunctions.Clear(ReadOnlyCurrentItems, startsWith);
        }


        /// <summary>
        /// Ensures the item with the given key
        /// </summary>
        /// <param name="key">Item key</param>
        internal static T Ensure<T>(string key)
            where T : class, new()
        {
            T result = (T)GetItem(key, true);
            if (result == null)
            {
                result = new T();
                Add(key, result, true);
            }

            return result;
        }
        
        #endregion
    }
}