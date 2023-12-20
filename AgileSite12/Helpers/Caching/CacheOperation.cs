namespace CMS.Helpers
{
    /// <summary>
    /// Cache operation names
    /// </summary>
    public class CacheOperation
    {
        /// <summary>
        /// Touch operation.
        /// </summary>
        public const string TOUCH = "TOUCH";


        /// <summary>
        /// Add operation.
        /// </summary>
        public const string ADD = "ADD";


        /// <summary>
        /// Add persistent operation.
        /// </summary>
        public const string ADD_PERSISTENT = "ADDPERSISTENT";


        /// <summary>
        /// Get operation.
        /// </summary>
        public const string GET = "GET";


        /// <summary>
        /// Get persistent operation.
        /// </summary>
        public const string GET_PERSISTENT = "GETPERSISTENT";


        /// <summary>
        /// Remove operation.
        /// </summary>
        public const string REMOVE = "REMOVE";


        /// <summary>
        /// Remove persistent operation.
        /// </summary>
        public const string REMOVE_PERSISTENT = "REMOVEPERSISTENT";


        /// <summary>
        /// Dependency callback
        /// </summary>
        public const string DEPENDENCY_CALLBACK = "DEPENDENCYCALLBACK";
    }
}
