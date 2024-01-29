namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Performance counter names
    /// </summary>
    public static class CounterName
    {
        /// <summary>
        /// Key of counter 'Allocated memory'.
        /// </summary>
        public const string ALLOCATED_MEMORY = "allocatedmemory";

        /// <summary>
        /// Key of counter 'Content page views/sec'
        /// </summary>
        public const string VIEW_OF_CONTENT_PAGES_PER_SECOND = "viewofcontentpagespersecond";

        /// <summary>
        /// Key of counter 'Pending requests/sec'
        /// </summary>
        public const string PENDING_REQUESTS_PER_SECOND = "pendingrequestspersecond";

        /// <summary>
        /// Key of counter 'File downloads/sec'
        /// </summary>
        public const string FILE_DOWNLOADS_AND_VIEWS_PER_SECOND = "filedownloadsandviewspersecond";

        /// <summary>
        /// Key of counter 'Pages not found/sec'
        /// </summary>
        public const string NOT_FOUND_PAGES_PER_SECOND = "notfoundpagespersecond";

        /// <summary>
        /// Key of counter 'Robots.txt views/sec'
        /// </summary>
        public const string ROBOT_TXT_PER_SECOND = "viewsofrobottxtpagepersecond";

        /// <summary>
        /// Key of counter 'System page views/sec'
        /// </summary>
        public const string VIEW_OF_SYSTEM_PAGES_PER_SECOND = "viewsofsystempagespersecond";

        /// <summary>
        /// Key of counter 'Non-page requests/sec'
        /// </summary>
        public const string NON_PAGES_REQUESTS_PER_SECOND = "nonpagesrequestspersecond";

        /// <summary>
        /// Key of counter 'Cache removed items/sec'
        /// </summary>
        public const string CACHE_REMOVED_ITEMS_PER_SECOND = "cacheremoveditemspersecond";

        /// <summary>
        /// Key of counter 'Cache underused items/sec'
        /// </summary>
        public const string CACHE_UNDERUSED_ITEMS_PER_SECOND = "cacheunderuseditemspersecond";

        /// <summary>
        /// Key of counter 'Cache expired items/sec'
        /// </summary>
        public const string CACHE_EXPIRED_ITEMS_PER_SECOND = "cacheexpireditemspersecond";

        /// <summary>
        /// Key of counter 'Online users - total'.
        /// </summary>
        public const string ONLINE_USERS = "onlineusers";

        /// <summary>
        /// Key of counter 'Online users - authenticated'.
        /// </summary>
        public const string AUTHENTICATED_USERS = "authenticatedusers";

        /// <summary>
        /// Key of counter 'Online users - anonymous'.
        /// </summary>
        public const string ANONYMOUS_VISITORS = "anonymousvisitors";

        /// <summary>
        /// Key of counter 'Running threads'.
        /// </summary>
        public const string RUNNING_THREADS = "runningthreads";

        /// <summary>
        /// Key of counter 'Running SQL queries'.
        /// </summary>
        public const string RUNNING_SQL_QUERIES = "runningsqlqueries";

        /// <summary>
        /// Key of counter 'Warnings'.
        /// </summary>
        public const string EVENTLOG_WARNINGS = "eventlogwarnings";

        /// <summary>
        /// Key of counter 'Errors'.
        /// </summary>
        public const string EVENTLOG_ERRORS = "eventlogerrors";

        /// <summary>
        /// Key of counter 'No. of running tasks'.
        /// </summary>
        public const string RUNNING_TASKS = "runningtasks";

        /// <summary>
        /// Key of counter 'Scheduled tasks in queue'.
        /// </summary>
        public const string TASKS_IN_QUEUE = "tasksinqueue";

        /// <summary>
        /// Key of counter 'E-mails in queue'.
        /// </summary>
        public const string ALL_EMAILS_IN_QUEUE = "allemailsinqueue";

        /// <summary>
        /// Key of counter 'Error e-mails in queue'.
        /// </summary>
        public const string ERROR_EMAILS_IN_QUEUE = "erroremailsinqueue";
    }
}
