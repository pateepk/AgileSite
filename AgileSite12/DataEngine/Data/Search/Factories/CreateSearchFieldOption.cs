namespace CMS.DataEngine
{
    /// <summary>
    /// Represents creation option of a system search field.
    /// </summary>
    /// <seealso cref="SearchFieldFactory"/>
    public enum CreateSearchFieldOption
    {
        /// <summary>
        /// Denotes a field which must be possible to search for and retrieve from an index.
        /// </summary>
        SearchableAndRetrievable = 3, // Included and retrievable


        /// <summary>
        /// Denotes a field which must be possible to search for in an index. It is not necessary for the value to be retrievable from the index.
        /// Further processing of the field value, which provides 'smart' search (e.g. tokenization) is desired.
        /// </summary>
        SearchableWithTokenizer = 5, // Included in index and tokenized


        /// <summary>
        /// Denotes a field which must be possible to search for and retrieve from an index. Further processing of the field value, which provides 'smart' search (e.g. tokenization) is desired.
        /// </summary>
        SearchableAndRetrievableWithTokenizer = 7 //  Included and retrievable and tokenized - SearchableAndRetrievableWithTokenizer
    }
}
