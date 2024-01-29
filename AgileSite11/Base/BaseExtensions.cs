using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace CMS.Base
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class BaseExtensions
    {
        /// <summary>
        /// Replaces a re
        /// </summary>
        /// <param name="regEx">Regular expression</param>
        /// <param name="input">Input text to replace</param>
        /// <param name="replacement">Replacement string</param>
        /// <param name="allowSubstitutions">If set to false, substitutions are not allowed within the replacement string</param>
        public static string Replace(this Regex regEx, string input, string replacement, bool allowSubstitutions)
        {
            if (!allowSubstitutions)
            {
                replacement = EscapeSubstitutions(replacement);
            }

            return regEx.Replace(input, replacement);
        }


        /// <summary>
        /// Escapes substitutions within the given string
        /// </summary>
        /// <param name="inputText">Input text</param>
        private static string EscapeSubstitutions(string inputText)
        {
            inputText = inputText.Replace("$", "$$");

            return inputText;
        }
        
        
        /// <summary>
        /// Returns input in batches of <paramref name="batchSize"/> size.
        /// </summary>
        /// <param name="collection">Collection to be batched</param>
        /// <param name="batchSize">Size of each chunk (except for the last one)</param>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new InvalidOperationException("[BaseExtensions.Batch]: batchSize must be a positive number.");
            }
            return BatchImpl(collection, batchSize);
        }


        /// <summary>
        /// Implementation of <see cref="Batch{T}"/> method.
        /// </summary>
        private static IEnumerable<IEnumerable<T>> BatchImpl<T>(this IEnumerable<T> collection, int batchSize)
        {
            var count = 0;
            T[] bucket = new T[batchSize];
            foreach (var item in collection)
            {
                bucket[count++] = item;

                if (count != batchSize)
                {
                    continue;
                }

                yield return bucket;

                bucket = new T[batchSize];
                count = 0;
            }

            if (count > 0)
            {
                yield return bucket.Take(count);
            }
        }


        /// <summary>
        /// Converts a DataRow to IDataContainer. Returns null if the original data row is null.
        /// </summary>
        /// <param name="dr">Data row</param>
        public static IDataContainer AsDataContainer(this DataRow dr)
        {
            if (dr == null)
            {
                return null;
            }

            return new DataRowContainer(dr);
        }
    }
}