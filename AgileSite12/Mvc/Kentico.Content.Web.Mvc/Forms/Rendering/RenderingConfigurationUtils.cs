using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Utility class for rendering configurations.
    /// </summary>
    internal static class RenderingConfigurationUtils
    {
        private static readonly StringComparer AttributeDictionaryComparer = StringComparer.Ordinal;


        /// <summary>
        /// Creates a new dictionary for storing rendering configuration attributes. The dictionary
        /// uses the <see cref="StringComparer.Ordinal"/> comparer (as the <see cref="TagBuilder.Attributes"/> dictionary).
        /// </summary>
        /// <param name="dictionary">Dictionary to optionally initialize the new dictionary from.</param>
        /// <returns>Creates a new dictionary optionally initialized with items from an existing dictionary.</returns>
        public static IDictionary<string, object> CreateAttributeDictionary(IDictionary<string, object> dictionary = null)
        {
            if (dictionary == null)
            {
                return new Dictionary<string, object>(AttributeDictionaryComparer);
            }
            return new Dictionary<string, object>(dictionary, AttributeDictionaryComparer);
        }
    }
}
