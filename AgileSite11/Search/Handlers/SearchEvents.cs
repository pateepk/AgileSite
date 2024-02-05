namespace CMS.Search
{
    /// <summary>
    /// Search events.
    /// </summary>
    public static class SearchEvents
    {
        /// <summary>
        /// Fires when the Smart search searches the results.
        /// </summary>
        public static readonly SearchHandler Search = new SearchHandler { Name = "SearchEvents.Search" };

        
        /// <summary>
        /// Fires when the search task is going to be created.
        /// </summary>
        public static readonly SearchTaskCreationHandler SearchTaskCreationHandler = new SearchTaskCreationHandler { Name = "SearchEvents.SearchTaskCreation" };
    }
}