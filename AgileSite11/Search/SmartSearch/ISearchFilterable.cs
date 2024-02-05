namespace CMS.Search
{
    /// <summary>
    /// Interface for search filter.
    /// </summary>
    public interface ISearchFilterable
    {
        /// <summary>
        /// Seach condition.
        /// </summary>
        string FilterSearchCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Sorting of search.
        /// </summary>
        string FilterSearchSort
        {
            get;
            set;
        }


        /// <summary>
        /// Combine search condition and order value with current values. 
        /// </summary>
        /// <param name="searchCondition">Search condition</param>
        /// <param name="searchSort">Sorting of search</param>
        /// <param name="filterPostback">If true filter caused the postback which means that filter condition has been changed.</param>
        void ApplyFilter(string searchCondition, string searchSort, bool filterPostback);


        /// <summary>
        /// Add filter options (selected item) to filter. 
        /// </summary>
        /// <param name="searchWebpartID">Filter webpart id</param>
        /// <param name="options">Filter option</param>
        void AddFilterOptionsToUrl(string searchWebpartID, string options);
    }
}