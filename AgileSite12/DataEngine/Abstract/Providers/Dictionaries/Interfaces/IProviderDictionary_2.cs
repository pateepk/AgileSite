namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a generic implementation of provider dictionary.
    /// </summary>
    public interface IProviderDictionary<in TKey, TValue> : IProviderDictionary
    {
        /// <summary>
        /// Returns true if the table contains specified record.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="value">Returns the object value if the object is present</param>
        bool TryGetValue(TKey key, out TValue value);


        /// <summary>
        /// Returns true if the table contains specified record.
        /// </summary>
        /// <param name="key">Key to check</param>
        bool ContainsKey(TKey key);


        /// <summary>
        /// Removes the specified object.
        /// </summary>
        /// <param name="key">Key to remove</param>
        void Remove(TKey key);


        /// <summary>
        /// Adds the specified object.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value</param>
        void Add(TKey key, TValue value);
    }
}