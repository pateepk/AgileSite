namespace CMS.Search.Azure
{
    /// <summary>
    /// Encapsulates information about Azure Search service necessary for connecting to the service's endpoint.
    /// </summary>
    public class SearchService
    {
        /// <summary>
        /// Azure Search service name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Azure Search service admin API key.
        /// </summary>
        public string AdminApiKey
        {
            get;
            set;
        }


        /// <summary>
        /// Creates a new <see cref="SearchService"/> from specified <paramref name="name"/> and <paramref name="adminApiKey"/>.
        /// </summary>
        /// <param name="name">Azure Search service name.</param>
        /// <param name="adminApiKey">Azure Search admin API key.</param>
        /// <returns>Search service object initialized from given parameters.</returns>
        public static SearchService FromAdminApiKey(string name, string adminApiKey)
        {
            return new SearchService
            {
                Name = name,
                AdminApiKey = adminApiKey
            };
        }
    }
}
