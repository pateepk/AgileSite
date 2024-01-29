using System;

using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Structure to define the search parameters used for search.
    /// </summary>
    public class SearchParameters
    {
        #region "Properties"
        
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

        #endregion
    }
}
