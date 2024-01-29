using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Compares user-agent with ones that use crawlers from defined search engines.
    /// </summary>
    public static class SearchEngineCrawlerAnalyzer
    {
        /// <summary>
        /// Struct for expressing cached crawler name with search engine ID.
        /// </summary>
        private struct CrawlerCachedItem
        {
            public string CrawlerName;
            public int SearchEngineID;
        }


        /// <summary>
        /// Cached cache settings.
        /// </summary>
        private static readonly string[] cacheDependency = { SearchEngineInfo.OBJECT_TYPE + "|all" };
        private static readonly string cacheName = CacheHelper.BuildCacheItemName(new[] { SearchEngineInfo.OBJECT_TYPE, "cachedids", "crawlers" });
        private static readonly double cacheTimeout = TimeSpan.FromDays(1).TotalMinutes;


        /// <summary>
        /// Gets all crawler names associated with search engine IDs.
        /// </summary>
        private static IEnumerable<CrawlerCachedItem> GetCachedCrawlerItems()
        {
            var settings = new CacheSettings(cacheTimeout, cacheName)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(cacheDependency)
            };

            return CacheHelper.Cache(GetCrawlerItems, settings);
        }


        /// <summary>
        /// Gets all crawler names associated with search engine IDs.
        /// </summary>
        private static IList<CrawlerCachedItem> GetCrawlerItems()
        {
            var result = new List<CrawlerCachedItem>();

            var searchEngines = SearchEngineInfoProvider.GetSearchEngines()
                                                        .Columns("SearchEngineID, SearchEngineCrawler")
                                                        .WhereNotEmpty("SearchEngineCrawler");

            // Use DataSet to go around InfoObjects to cut unnecessary overhead
            if (!DataHelper.DataSourceIsEmpty(searchEngines))
            {
                foreach (DataRow dr in searchEngines.Tables[0].Rows)
                {
                    var searchEngineID = dr["SearchEngineID"].ToInteger(0);
                    var crawlerName = dr["SearchEngineCrawler"].ToString("");

                    result.Add(new CrawlerCachedItem
                    {
                        CrawlerName = crawlerName,
                        SearchEngineID = searchEngineID
                    });
                }
            }

            return result;
        }


        /// <summary>
        /// Returns <see cref="SearchEngineInfo"/> for crawlers identified by user agent.
        /// </summary>
        /// <param name="userAgent">User agent string that might contain crawler identifier</param>
        /// <returns><see cref="SearchEngineInfo"/> of search engine that has given crawler or null if no search engine matches</returns>
        public static SearchEngineInfo GetSearchEngineFromUserAgent(string userAgent)
        {
            if (!string.IsNullOrEmpty(userAgent))
            {
                var cachedCrawlers = GetCachedCrawlerItems();

                foreach (var crawlerItem in cachedCrawlers)
                {
                    // Check if rule matches
                    if (userAgent.IndexOfCSafe(crawlerItem.CrawlerName, true) >= 0)
                    {
                        var sei = SearchEngineInfoProvider.GetSearchEngineInfo(crawlerItem.SearchEngineID);
                        if (sei != null)
                        {
                            return sei;
                        }
                    }
                }
            }

            return null;
        }
    }
}