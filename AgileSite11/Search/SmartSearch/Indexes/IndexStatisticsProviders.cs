using System;
using System.Collections.Concurrent;

namespace CMS.Search
{
    /// <summary>
    /// Maintains registered <see cref="IIndexStatisticsProvider"/>s.
    /// </summary>
    /// <seealso cref="SearchIndexInfo.IndexProvider"/>
    public class IndexStatisticsProviders
    {
        private static IndexStatisticsProviders mInstance = new IndexStatisticsProviders();
        private readonly ConcurrentDictionary<string, IIndexStatisticsProvider> providers = new ConcurrentDictionary<string, IIndexStatisticsProvider>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Gets the <see cref="IndexStatisticsProviders"/> instance.
        /// </summary>
        public static IndexStatisticsProviders Instance
        {
            get
            {
                return mInstance;
            }
            internal set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// Registers a new <see cref="IIndexStatisticsProvider"/> for given <paramref name="indexProviderName"/>, or overwrites an existing one.
        /// </summary>
        /// <param name="indexProviderName">Name of index provider for which statistics provider is being registered.</param>
        /// <param name="statisticsProvider">Statistics provider to be registered.</param>
        /// <seealso cref="SearchIndexInfo.IndexProvider"/>
        public void Register(string indexProviderName, IIndexStatisticsProvider statisticsProvider)
        {
            if (indexProviderName == null)
            {
                throw new ArgumentNullException(nameof(indexProviderName));
            }

            providers[indexProviderName] = statisticsProvider;
        }


        /// <summary>
        /// Gets an <see cref="IIndexStatisticsProvider"/> for given <paramref name="indexProviderName"/>.
        /// </summary>
        /// <param name="indexProviderName">Name of index provider to retrieve <see cref="IIndexStatisticsProvider"/> implementation for.</param>
        /// <returns>Index statistics provider for <paramref name="indexProviderName"/>.</returns>
        /// <seealso cref="SearchIndexInfo.IndexProvider"/>
        public IIndexStatisticsProvider Get(string indexProviderName)
        {
            if (indexProviderName == null)
            {
                throw new ArgumentNullException(nameof(indexProviderName));
            }

            IIndexStatisticsProvider result;
            if (providers.TryGetValue(indexProviderName, out result))
            {
                return result;
            }
            
            throw new ArgumentException($"No index statistics provider is registered for index provider name '{indexProviderName}'.", nameof(indexProviderName));
        }
    }
}
