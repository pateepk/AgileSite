using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Search event arguments
    /// </summary>
    public class SearchEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Search parameters
        /// </summary>
        public SearchParameters Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Search results
        /// </summary>
        public SearchResults Results
        {
            get;
            set;
        }
    }
}