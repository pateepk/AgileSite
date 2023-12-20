using System.Collections.Generic;
using System.ComponentModel;

namespace CMS.DataEngine.CollectionExtensions
{
    /// <summary>
    /// Collection extension methods
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CollectionExtensions
    {
        /// <summary>
        /// Converts the list of objects to a hash set of distinct values
        /// </summary>
        /// <param name="objects">List of objects to convert</param>
        /// <param name="comparer">Comparer</param>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public static HashSet<T> ToHashSetCollection<T>(this IEnumerable<T> objects, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(objects, comparer);
        }


        /// <summary>
        /// Adds range of items to hashset.
        /// </summary>
        /// <param name="instance">Hashset to add to.</param>
        /// <param name="range">List of items to add.</param>
        public static void AddRangeToSet<T>(this ISet<T> instance, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                instance.Add(item);
            }
        }
    }
}
