namespace CMS.Search
{
    /// <summary>
    /// Custom search index interface.
    /// </summary>
    public interface ICustomSearchIndex
    {
        /// <summary>
        /// Rebuild custom search index.
        /// </summary>
        /// <param name="srchInfo">Current index info</param>
        void Rebuild(SearchIndexInfo srchInfo);
    }
}