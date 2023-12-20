using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Analyzes whether URLs come from some of the defined search engines and gets the search keywords from them. 
    /// Use it on referrer URL to get where the visitor came from.
    /// </summary>
    public static class SearchEngineAnalyzer
    {
        private struct SearchEngineDomainCachedItem
        {
            public string SearchEngineDomain;
            public int SearchEngineID;
        }


        /// <summary>
        /// Gets cached search engine domains with corresponding search engine IDs.
        /// </summary>
        private static IEnumerable<SearchEngineDomainCachedItem> GetCachedSearchEngineDomains()
        {
            var settings = new CacheSettings(TimeSpan.FromDays(1).TotalMinutes, SearchEngineInfo.OBJECT_TYPE, "cachedids", "domainnames")
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[]
                {
                    SearchEngineInfo.OBJECT_TYPE + "|all",
                })
            };

            return CacheHelper.Cache(GetSearchEngineDomains, settings);
        }


        /// <summary>
        /// Gets search engine domains with corresponding search engine IDs.
        /// </summary>
        private static IList<SearchEngineDomainCachedItem> GetSearchEngineDomains()
        {
            var result = new List<SearchEngineDomainCachedItem>();

            var searchEngines = SearchEngineInfoProvider.GetSearchEngines()
                                                        .Columns("SearchEngineID, SearchEngineDomainRule");


            // Use DataSet to go around InfoObjects to cut unnecessary overhead
            if (!DataHelper.DataSourceIsEmpty(searchEngines))
            {
                foreach (DataRow dr in searchEngines.Tables[0].Rows)
                {
                    var searchEngineID = dr["SearchEngineID"].ToInteger(0);
                    var domainName = dr["SearchEngineDomainRule"].ToString("");

                    result.Add(new SearchEngineDomainCachedItem
                    {
                        SearchEngineDomain = domainName,
                        SearchEngineID = searchEngineID
                    });
                }
            }

            return result;
        }


        /// <summary>
        /// Returns search engine name for specified URL.
        /// </summary>
        /// <param name="url">Absolute URL that can contain search keywords</param>
        /// <param name="searchKeyword">Search keyword that's in the URL</param>
        /// <returns><see cref="SearchEngineInfo"/> of search engine that corresponds to <paramref name="url"/>. Returns null if no search engine matches.</returns>
        public static SearchEngineInfo GetSearchEngineFromUrl(string url, out string searchKeyword)
        {
            // Initialize output parameter
            searchKeyword = String.Empty;

            // Check whether input URL is valid
            if (String.IsNullOrEmpty(url) || !ValidationHelper.IsURL(url))
            {
                return null;
            }

            var urlWithoutHttpAndWww = StripProtocolAndWww(url);

            // Ensure IDs collection
            var cachedDomains = GetCachedSearchEngineDomains();

            // Get matching search engines
            var matchingEngines = cachedDomains.Where(x => urlWithoutHttpAndWww.StartsWithCSafe(x.SearchEngineDomain, true))
                                               .Select(x => SearchEngineInfoProvider.GetSearchEngineInfo(x.SearchEngineID))
                                               .ToList();


            foreach (SearchEngineInfo sei in matchingEngines)
            {
                bool keyExists;

                // Try get search keyword
                searchKeyword = URLHelper.GetQueryValue(url, sei.SearchEngineKeywordParameter, out keyExists);
                if (keyExists)
                {
                    searchKeyword = HttpUtility.UrlDecode(searchKeyword);

                    // Query key exists, but there is no query value - http://google.com/?q=
                    if (String.IsNullOrEmpty(searchKeyword))
                    {
                        searchKeyword = "(not provided)";
                    }

                    return sei;
                }
            }

            // If at least one search engine found without query key, it could be something like http://google.com/#q=search,
            // but part after # is not considered a URI, browser doesn't send it to server so it is not accessible here.
            if (matchingEngines.Any())
            {
                searchKeyword = "(not provided)";
                return matchingEngines.First();
            }

            // Search engine not found
            return null;
        }


        /// <summary>
        /// Gets the URL for matching the search engine domain rule. Removes protocol and www. from the URL. 
        /// E.g. converts "http://www.google.com/abc?def" to "google.com/abc?def".
        /// </summary>
        /// <param name="url">URL to process</param>
        private static string StripProtocolAndWww(string url)
        {
            // Remove URL protocol
            url = URLHelper.RemoveProtocol(url);

            // Remove www prefix
            url = URLHelper.RemoveWWW(url);

            return url;
        }
    }
}