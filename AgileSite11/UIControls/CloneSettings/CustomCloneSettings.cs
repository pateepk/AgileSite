using System;
using System.Collections.Concurrent;

namespace CMS.UIControls
{
    /// <summary>
    /// Collection of clone settings control type used in clone settings dialog.
    /// </summary>
    public static class CustomCloneSettings
    {
        private static readonly ConcurrentDictionary<string, Type> mCustomCloneSettings = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Adds the type to the collection if does not already exist, or to update if the type already exists
        /// </summary>
        /// <param name="objectType">Info object type.</param>
        /// <param name="type">Control type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objectType"/></exception>
        public static void AddOrUpdate(string objectType, Type type)
        {
            mCustomCloneSettings[objectType] = type;
        }


        /// <summary>
        /// Gets the registered control type for specified object type.
        /// </summary>
        /// <param name="objectType">Info object type.</param>
        /// <param name="type">Control type.</param>
        /// <returns><c>true</c> if control type was found for specified <paramref name="objectType"/>;otherwise <c>false</c>.</returns>
        public static bool TryGetControl(string objectType, out Type type)
        {
            return mCustomCloneSettings.TryGetValue(objectType, out type);
        }
    }
}
