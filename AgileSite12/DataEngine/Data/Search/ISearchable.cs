namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for search.
    /// </summary>
    public interface ISearchable
    {
        /// <summary>
        /// Gets or sets the search object type.
        /// </summary>
        string SearchType
        {
            get;
        }


        /// <summary>
        /// Returns search document for index.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <returns>Returns search document</returns>
        SearchDocument GetSearchDocument(ISearchIndexInfo index);


        /// <summary>
        /// Returns search fields collection. When existing collection is passed as argument, fields will be added to that collection.
        /// When collection is not passed, new collection will be created and return.
        /// Collection will contain field values only when collection with StoreValues property set to true is passed to the method.
        /// When method creates new collection, it is created with StoreValues property set to false.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        ISearchFields GetSearchFields(ISearchIndexInfo index, ISearchFields searchFields = null);


        /// <summary>
        /// Returns URL for a search result item image.
        /// </summary>
        /// <param name="image">Image</param>
        string GetSearchImageUrl(string image);


        /// <summary>
        /// Gets the search item unique id.
        /// </summary>
        string GetSearchID();


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object GetValue(string columnName);


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param>
        bool SetValue(string columnName, object value);
    }
}