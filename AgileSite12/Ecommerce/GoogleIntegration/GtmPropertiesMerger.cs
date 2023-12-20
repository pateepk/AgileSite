using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Utility class for merging properties of two objects.
    /// </summary>
    /// <seealso cref="GtmData"/>
    public class GtmPropertiesMerger : AbstractHelper<GtmPropertiesMerger>
    {
        /// <summary>
        /// <para>
        /// Merges <paramref name="gtmData"/> and <paramref name="additionalData"/> to form new <see cref="GtmData"/>.
        /// </para>
        /// <para>
        /// If <paramref name="additionalData"/> can be enumerated as <see cref="KeyValuePair{TKey, TValue}"/> items, then the key-value pairs
        /// are merged. Otherwise, the public properties and their values are merged.
        /// </para>
        /// </summary>
        /// <param name="gtmData">Source object to be merged with <paramref name="additionalData"/> object.</param>
        /// <param name="additionalData">Properties of this object are merged with <paramref name="gtmData"/> object.</param>
        /// <param name="overwrite">
        /// Decides whether values of properties with the same name are substituted for values of <paramref name="additionalData"/> object.
        /// </param>
        /// <remarks>
        /// Objects properties are merged only on the first level.
        /// </remarks>
        /// <example>
        /// <para>Source object: new GtmData { a = 1, b = 2 }</para>
        /// <para>Merge object: new { a = 3, c = 4 }</para>
        /// <para>Rewrite equals false: new GtmData { a = 1, b = 2, c = 4 }</para>
        /// <para>Rewrite equals false: new GtmData { a = 3, b = 2, c = 4 }</para>
        /// </example>
        /// <returns>
        /// New <see cref="GtmData"/> with combined properties and values of original <paramref name="gtmData"/> and <paramref name="additionalData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gtmData"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="additionalData"/> can be enumerated as <see cref="KeyValuePair{TKey, TValue}"/> items, but a null key is found.</exception>
        public static GtmData Merge(GtmData gtmData, object additionalData, bool overwrite = false)
        {
            return HelperObject.MergeInternal(gtmData, additionalData, overwrite);
        }


        /// <summary>
        /// <para>
        /// Merges <paramref name="gtmData"/> and <paramref name="additionalData"/> to form new <see cref="GtmData"/>.
        /// </para>
        /// <para>
        /// If <paramref name="additionalData"/> can be enumerated as <see cref="KeyValuePair{TKey, TValue}"/> items, then the key-value pairs
        /// are merged. Otherwise, the public properties and their values are merged.
        /// </para>
        /// </summary>
        /// <param name="gtmData">Source object to be merged with <paramref name="additionalData"/> object.</param>
        /// <param name="additionalData">Properties of this object are merged with <paramref name="gtmData"/> object.</param>
        /// <param name="overwrite">
        /// Decides whether values of properties with the same name are substituted for values of <paramref name="additionalData"/> object.
        /// </param>
        /// <remarks>
        /// Objects properties are merged only on the first level.
        /// </remarks>
        /// <example>
        /// <para>Source object: new GtmData { a = 1, b = 2 }</para>
        /// <para>Merge object: new { a = 3, c = 4 }</para>
        /// <para>Rewrite equals false: new GtmData { a = 1, b = 2, c = 4 }</para>
        /// <para>Rewrite equals false: new GtmData { a = 3, b = 2, c = 4 }</para>
        /// </example>
        /// <returns>
        /// New <see cref="GtmData"/> with combined properties and values of original <paramref name="gtmData"/> and <paramref name="additionalData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gtmData"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="additionalData"/> can be enumerated as <see cref="KeyValuePair{TKey, TValue}"/> items, but a null key is found.</exception>
        protected virtual GtmData MergeInternal(GtmData gtmData, object additionalData, bool overwrite = false)
        {
            if (gtmData == null)
            {
                throw new ArgumentNullException(nameof(gtmData));
            }

            var mergedData = new GtmData();

            foreach (var key in gtmData.Keys)
            {
                mergedData.Add(key, gtmData[key]);
            }

            IEnumerable additionalDataAsEnumerable = additionalData as IEnumerable;
            if (additionalDataAsEnumerable != null && TryMergeKeyValuePairType(mergedData, additionalDataAsEnumerable, overwrite))
            {
                return mergedData;
            }

            var propertiesToMerge = additionalData?.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToDictionary(p => p.Name, p => p.GetValue(additionalData, null));
            foreach (var key in propertiesToMerge?.Keys ?? Enumerable.Empty<string>())
            {
                if (!mergedData.ContainsKey(key) || overwrite)
                {
                    mergedData[key] = propertiesToMerge[key];
                }
            }

            return mergedData;
        }


        /// <summary>
        /// Tries to enumerate <paramref name="additionalData"/> as <see cref="KeyValuePair{TKey, TValue}"/> items and merge them into <paramref name="result"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="additionalData"/> can be enumerated as <see cref="KeyValuePair{TKey, TValue}"/> items, but a null key is found.</exception>
        private bool TryMergeKeyValuePairType(GtmData result, IEnumerable additionalData, bool overwrite)
        {
            MethodInfo getEnumeratorMethod;
            var enumeratedType = GetEnumeratedType(additionalData.GetType(), out getEnumeratorMethod);

            if (enumeratedType == null || !IsGenericKeyValuePairType(enumeratedType))
            {
                return false;
            }

            var keyProperty = enumeratedType.GetProperty("Key");
            var valueProperty = enumeratedType.GetProperty("Value");

            IEnumerator additionalDataEnumerator = null;
            try
            {
                additionalDataEnumerator = (IEnumerator) getEnumeratorMethod.Invoke((object) additionalData, (object[]) null);
                while(additionalDataEnumerator.MoveNext())
                {
                    var key = keyProperty.GetValue(additionalDataEnumerator.Current)?.ToString();
                    if (key == null)
                    {
                        throw new InvalidOperationException("Additional data cannot contain null value as a key.");
                    }
                    var value = valueProperty.GetValue(additionalDataEnumerator.Current);

                    if (!result.ContainsKey(key) || overwrite)
                    {
                        result[key] = value;
                    }
                }

            }
            finally
            {
                var disposible = additionalDataEnumerator as IDisposable;
                disposible?.Dispose();
            }

            return true;
        }


        private Type GetEnumeratedType(Type enumerableType, out MethodInfo getEnumerator)
        {
            var getEnumeratorMethod = enumerableType.GetMethod(nameof(IEnumerable.GetEnumerator));
            var currentProperty = getEnumeratorMethod?.ReturnType.GetProperty(nameof(IEnumerator.Current));
            var enumeratedType = currentProperty?.GetGetMethod()?.ReturnType;

            getEnumerator = getEnumeratorMethod;

            return enumeratedType;
        }


        private bool IsGenericKeyValuePairType(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>));
        }
    }
}
