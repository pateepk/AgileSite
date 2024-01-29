using System;
using System.Collections.Generic;

using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// <see cref="BaseInfo"/> dictionary.
    /// </summary>
    /// <remarks>Uses <see cref="StringComparer.InvariantCultureIgnoreCase"/> as default comparer for <c>string</c> types.</remarks>
    public class ProviderInfoDictionary<TKey> : ProviderDictionary<TKey, BaseInfo>
    {
        private static readonly Type currentType = typeof(TKey);

        /// <summary>
        /// Adds the specified object.
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value</param>
        /// <param name="logTask">If true, web farm task is logged</param>
        public override void Add(TKey key, BaseInfo value, bool logTask)
        {
            // Mark object as cached when inserted
            if (value != null)
            {
                value.Generalized.IsCachedObject = true;
            }

            base.Add(key, value, logTask);
        }


        /// <summary>
        /// Returns true if the internal dictionary contains specified record.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="value">Returns the object value if the object is present</param>
        protected override bool TryGetInternal(TKey key, out BaseInfo value)
        {
            var result = base.TryGetInternal(key, out value);

            // Check the validity of the object
            if (result && (value != null)
                && value.TypeInfo.SupportsInvalidation && !value.Generalized.IsObjectValid)
            {
                // Remove from dictionary if not valid, and pretend that it does not exist in the dictionary
                RemoveInternal(key);

                value = null;
                result = false;
            }

            return result;
        }


        /// <summary>
        /// Converts the key to a specific type
        /// </summary>
        /// <param name="key">Key to convert</param>
        /// <exception cref="InvalidOperationException">Thrown when unsupported type is used.</exception>
        protected override TKey ConvertKey(object key)
        {
            Func<object, object> conversionFunction;
            if (!ProviderInfoDictionaryConversions.Values.TryGetValue(currentType, out conversionFunction))
            {
                throw new InvalidOperationException($"Type '{currentType.Name}' is not supported for {nameof(ProviderInfoDictionary<TKey>)}.");
            }

            return (TKey)conversionFunction(key);
        }


        /// <summary>
        /// Returns <see cref="StringComparer.InvariantCultureIgnoreCase"/> if comparer is not defined explicitly and type is string.
        /// </summary>
        private static IEqualityComparer<TKey> TryGetDefaultStringComparer()
        {
            if (typeof(TKey) == typeof(string))
            {
                return (IEqualityComparer<TKey>)StringComparer.InvariantCultureIgnoreCase;
            }

            return null;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        /// <param name="comparer">Equality comparer for the items</param>
        /// <param name="allowNulls">Indicates whether null value will be considered as valid and will be cached.</param>
        /// <param name="useWeakReferences">Indicates whether cache item can be removed from cache in case of insufficient memory.</param>
        public ProviderInfoDictionary(string objectType, string columnNames, IEqualityComparer<TKey> comparer = null, bool allowNulls = false, bool useWeakReferences = false)
            : base(objectType, columnNames, comparer ?? TryGetDefaultStringComparer(), allowNulls, useWeakReferences)
        {
        }


        /// <summary>
        /// Static constructor
        /// </summary>
        static ProviderInfoDictionary()
        {
            TypeManager.RegisterGenericType(typeof(ProviderInfoDictionary<TKey>));
        }
    }


    // Collection of supported types for conversion
    internal static class ProviderInfoDictionaryConversions
    {
        public static readonly Dictionary<Type, Func<object, object>> Values = new Dictionary<Type, Func<object, object>>
        {
            {  typeof(int), (key) => ValidationHelper.GetInteger(key, 0) },
            {  typeof(string), (key) => ValidationHelper.GetString(key, String.Empty) },
            {  typeof(Guid), (key) => ValidationHelper.GetGuid(key, Guid.Empty) }
        };
    }
}
