using System;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider string indexed dictionary. Represent the case-insensitive object storage optimized for reading.
    /// </summary>
    [Obsolete("Use ProviderDictionary<string, T> instead.")]
    public class ProviderStringValueDictionary : ProviderStringValueDictionary<object>
    {
        /// <summary>
        /// Creates new instance of <see cref="ProviderStringValueDictionary"/>.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        /// <param name="customNameSuffix">Suffix used for name identifier. Suffix is required in cases where <paramref name="objectType"/> and <paramref name="columnNames"/> are used for more instances of <see cref="ProviderDictionary{TKey, TValue}"/>.</param>
        internal ProviderStringValueDictionary(string objectType, string columnNames, string customNameSuffix)
            : base(objectType, columnNames, customNameSuffix)
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="ProviderStringValueDictionary"/>.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        public ProviderStringValueDictionary(string objectType, string columnNames)
            : this(objectType, columnNames, null)
        {
        }
    }


    /// <summary>
    /// Provider string indexed dictionary. Represent the case-insensitive object storage optimized for reading.
    /// </summary>
    [Obsolete("Use ProviderDictionary<string, T> instead.")]
    public class ProviderStringValueDictionary<T> : ProviderDictionary<string, T>
    {
        /// <summary>
        /// Creates new instance of <see cref="ProviderStringValueDictionary{T}"/>.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        /// <param name="customNameSuffix">Suffix used for name identifier. Suffix is required in cases where <paramref name="objectType"/> and <paramref name="columnNames"/> are used for more instances of <see cref="ProviderDictionary{TKey, TValue}"/>.</param>
        internal ProviderStringValueDictionary(string objectType, string columnNames, string customNameSuffix)
            : base(objectType, columnNames, StringComparer.InvariantCultureIgnoreCase, customNameSuffix, allowNulls: true)
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="ProviderStringValueDictionary{T}"/>.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        public ProviderStringValueDictionary(string objectType, string columnNames)
            : this(objectType, columnNames, null)
        {
        }


        /// <summary>
        /// Returns true if the table contains specified record.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="value">Returns the object value if the object is present</param>
        public bool Contains(string key, out string value)
        {
            T outValue;
            bool result = TryGetValue(key, out outValue);

            value = ValidationHelper.GetString(outValue, null);
            return result;
        }
    }
}