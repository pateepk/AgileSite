using System.Collections.Concurrent;

namespace CMS.Membership
{
    /// <summary>
    /// Class containing extension methods for <see cref="ConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Attempts to remove value that has the specified key from the <see cref="ConcurrentDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="concurrentDictionary"/>.</typeparam>
        /// <typeparam name="TValue">Type of the value returned by <paramref name="concurrentDictionary"/>.</typeparam>
        /// <param name="concurrentDictionary"><see cref="ConcurrentDictionary{TKey, TValue}"/> from which to remove value with the associated <paramref name="key"/>.</param>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns>Returns true if the object was removed successfully. False, otherwise.</returns>
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary, TKey key)
        {
            TValue value;
            return concurrentDictionary.TryRemove(key, out value);
        }
    }
}
