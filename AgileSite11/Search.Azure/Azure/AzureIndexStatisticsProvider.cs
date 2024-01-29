using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Provides statistics of Azure Search indexes.
    /// </summary>
    public class AzureIndexStatisticsProvider : IIndexStatisticsProvider
    {
        /// <summary>
        /// Gets statistics of an Azure index.
        /// </summary>
        /// <param name="indexInfo">Index to retrieve statistics for.</param>
        /// <returns>Statistics of given index, or null when statistics are not available.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="SearchIndexInfo.IndexProvider"/> of given <paramref name="indexInfo"/> is not Azure.</exception>
        /// <remarks>
        /// This method returns null if index does not exist within Azure Search service or the index's service name and admin API key are not valid.
        /// </remarks>
        /// <seealso cref="SearchIndexInfo.AZURE_SEARCH_PROVIDER"/>
        public IIndexStatistics GetStatistics(SearchIndexInfo indexInfo)
        {
            if (indexInfo == null)
            {
                throw new ArgumentNullException(nameof(indexInfo));
            }
            if (!indexInfo.IsAzureIndex())
            {
                throw new ArgumentException("The given index is not an Azure index.");
            }

            var ssm = new SearchServiceManager(SearchService.FromAdminApiKey(indexInfo.IndexSearchServiceName, indexInfo.IndexAdminKey));
            IndexGetStatisticsResult statisticsResult;
            if (!TryGetStatistics(ssm, indexInfo.IndexName, out statisticsResult))
            {
                return null;
            }

            return new AzureIndexStatistics(statisticsResult.DocumentCount, statisticsResult.StorageSize);
        }


        private bool TryGetStatistics(SearchServiceManager searchServiceManager, string indexName, out IndexGetStatisticsResult statisticsResult)
        {
            IndexGetStatisticsResult result = null;
            try
            {
                if (searchServiceManager.IndexExists(indexName))
                {
                    result = searchServiceManager.GetStatistics(indexName);

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                statisticsResult = result;
            }
        }
    }
}
