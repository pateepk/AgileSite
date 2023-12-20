using System;
using System.Collections.Generic;
using System.Globalization;

using CMS.Base;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Search
{
    /// <summary>
    /// Structure to define the search parameters used for search.
    /// </summary>
    public class SearchParameters
    {
        /// <summary>
        /// Search query
        /// </summary>
        public string SearchFor
        {
            get;
            set;
        }


        /// <summary>
        /// Search sort expression
        /// </summary>
        public string SearchSort
        {
            get;
            set;
        }


        /// <summary>
        /// Path
        /// </summary>
        public string Path
        {
            get;
            set;
        }


        /// <summary>
        /// Class names separated by semicolon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Constrains the search to given class names only. Application of this parameter by search method is optional.
        /// See the search method documentation.
        /// </para>
        /// <para>
        /// Another option for constraining search by class names is <see cref="DocumentSearchCondition"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="DocumentSearchCondition"/>
        public string ClassNames
        {
            get;
            set;
        }


        /// <summary>
        /// Current culture
        /// </summary>
        public string CurrentCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Default culture
        /// </summary>
        public string DefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Whether combine with default culture
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether check permissions
        /// </summary>
        public bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether search in attachments
        /// </summary>
        public bool SearchInAttachments
        {
            get;
            set;
        }


        /// <summary>
        /// List of search index names to search in, separated by semicolon
        /// </summary>
        public string SearchIndexes
        {
            get;
            set;
        }


        /// <summary>
        /// Number of results which should be displayed
        /// </summary>
        public int DisplayResults
        {
            get;
            set;
        }


        /// <summary>
        /// Starting position in search results (paging)
        /// </summary>
        public int StartingPosition
        {
            get;
            set;
        }


        /// <summary>
        /// Number of processed results due to performance optimization
        /// </summary>
        public int NumberOfProcessedResults
        {
            get;
            set;
        }


        /// <summary>
        /// User info
        /// </summary>
        public IUserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Number of results for the search query.
        /// </summary>
        /// <remarks>
        /// Number of results can differ for different pages of search results due to increasing number of processed results.
        /// </remarks>
        public int NumberOfResults
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the maximum score value encountered within the search hits (among the NumberOfResults results).
        /// </summary>
        public float MaxScore
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition for attachments search
        /// </summary>
        public string AttachmentWhere
        {
            get;
            set;
        }

        /// <summary>
        /// Order by for attachments search
        /// </summary>
        public string AttachmentOrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// If true, performs the search only when the content part of the search expression is present.
        /// </summary>
        public bool BlockFieldOnlySearch
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true, if the search parameters search for all cultures
        /// </summary>
        public bool SearchAllCultures
        {
            get
            {
                return (CombineWithDefaultCulture || String.IsNullOrEmpty(CurrentCulture) || (CurrentCulture == "##ALL##"));
            }
        }


        /// <summary>
        /// <para>
        /// Creates a new instance of the <see cref="SearchParameters"/> class for a page search for given <paramref name="searchText"/>
        /// across indexes specified in <paramref name="searchIndexNames"/>. The <paramref name="user"/> parameter limits search result
        /// when performing permission checks in result filtering.
        /// </para>
        /// <para>
        /// Retrieves the result for the first page for invalid page numbers.
        /// </para>
        /// </summary>
        /// <param name="searchText">Text to search for.</param>
        /// <param name="searchIndexNames">Names of indexes to perform search on.</param>
        /// <param name="pageNumber">Number of a page for which to retrieve results, starting from 1.</param>
        /// <param name="pageSize">Size of a page.</param>
        /// <param name="user">User for whom to filter the result.</param>
        /// <param name="combineWithDefaultCulture">Indicates whether the search service uses site default language version of pages as a replacement for pages
        /// that are not translated into the language the search performed in (see the Remarks section on details of the search culture).</param>
        /// <remarks>
        /// This constructor passes <see cref="CultureInfo.Name"/> value of <see cref="CultureInfo.CurrentUICulture"/> as the name of culture
        /// to search in.
        /// </remarks>
        public static SearchParameters PrepareForPages(string searchText, IEnumerable<string> searchIndexNames, int pageNumber, int pageSize, IUserInfo user, bool combineWithDefaultCulture = false)
        {
            return PrepareForPages(searchText, searchIndexNames, pageNumber, pageSize, user, CultureInfo.CurrentUICulture.Name, combineWithDefaultCulture);
        }


        /// <summary>
        /// <para>
        /// Creates a new instance of the <see cref="SearchParameters"/> class for a page search for given <paramref name="searchText"/>
        /// across indexes specified in <paramref name="searchIndexNames"/>. The <paramref name="user"/> parameter limits search result
        /// when performing permission checks in result filtering.
        /// </para>
        /// <para>
        /// Retrieves the result for the first page for invalid page numbers.
        /// </para>
        /// </summary>
        /// <param name="searchText">Text to search for.</param>
        /// <param name="searchIndexNames">Names of indexes to perform search on.</param>
        /// <param name="pageNumber">Number of a page for which to retrieve results, starting from 1.</param>
        /// <param name="pageSize">Size of a page.</param>
        /// <param name="user">User for whom to filter the result.</param>
        /// <param name="cultureName">Name of culture to search in. Pass null to search in all cultures.</param>
        /// <param name="combineWithDefaultCulture">Indicates whether the search service uses site default language version of pages as a replacement for pages that are not translated into the language specified by <paramref name="cultureName"/>.</param>
        /// <remarks>
        /// The default culture is determined by <see cref="CultureHelper.GetDefaultCultureCode"/> call for <see cref="SiteContext.CurrentSiteName"/>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="searchIndexNames"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="searchText"/> is null or whitespace string.</exception>
        public static SearchParameters PrepareForPages(string searchText, IEnumerable<string> searchIndexNames, int pageNumber, int pageSize, IUserInfo user, string cultureName, bool combineWithDefaultCulture)
        {
            if (String.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text cannot be null or whitespace.", nameof(searchText));
            }
            if (searchIndexNames == null)
            {
                throw new ArgumentNullException(nameof(searchIndexNames));
            }

            pageNumber = Math.Max(pageNumber, 1);

            var defaultCulture = CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName);

            var documentCondition = new DocumentSearchCondition
            {
                Culture = cultureName,
                DefaultCulture = defaultCulture,
                CombineWithDefaultCulture = combineWithDefaultCulture
            };
            var condition = new SearchCondition(documentCondition: documentCondition);
            var searchExpression = SearchSyntaxHelper.CombineSearchCondition(searchText, condition);

            var parameters = new SearchParameters
            {
                SearchFor = searchExpression,
                Path = "/%",
                ClassNames = null,
                CurrentCulture = cultureName,
                DefaultCulture = defaultCulture,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                CheckPermissions = false,
                SearchInAttachments = false,
                User = user,
                SearchIndexes = searchIndexNames.Join(";"),
                StartingPosition = (pageNumber - 1) * pageSize,
                NumberOfResults = 0,
                NumberOfProcessedResults = 100,
                DisplayResults = pageSize
            };

            return parameters;
        }
    }
}
