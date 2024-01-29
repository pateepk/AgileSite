using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Holds registered instances of <see cref="IInfoProvider"/> by object type. 
    /// </summary>
    /// <seealso cref="InfoProviderLoader"/>
    internal static class InfoProviderCache
    {
        // Cache of all loaded provider objects. [objectType] => [provider]
        private static readonly CMSStatic<ConcurrentDictionary<string, IInfoProvider>> providers = new CMSStatic<ConcurrentDictionary<string, IInfoProvider>>(() => new ConcurrentDictionary<string, IInfoProvider>(StringComparer.OrdinalIgnoreCase));


        /// <summary>
        /// Gets collection of registered providers.
        /// </summary>
        internal static IEnumerable<IInfoProvider> RegisteredProviders
        {
            get
            {
                return providers.Value.Values;
            }
        }


        /// <summary>
        /// Returns <see cref="IInfoProvider"/> retrieved from provider cache by given object type.
        /// </summary>
        public static IInfoProvider GetInfoProvider(string objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            IInfoProvider result;
            providers.Value.TryGetValue(objectType, out result);
            return result;
        }


        /// <summary>
        /// Returns <see cref="IInfoProvider"/> retrieved from provider cache by given object type.
        /// </summary>
        public static TProvider GetInfoProvider<TProvider>(string objectType)
            where TProvider : class
        {
            return GetInfoProvider(objectType) as TProvider;
        }


        /// <summary>
        /// Registers specified provider in providers cache
        /// </summary>
        internal static void Register(string objectType, IInfoProvider provider)
        {
            if (string.IsNullOrEmpty(objectType) || objectType == ObjectTypeInfo.VALUE_UNKNOWN)
            {
                return;
            }

            providers.Value.AddOrUpdate(objectType, provider, (objectTypeKey, existingProvider) => provider);
        }
    }
}
