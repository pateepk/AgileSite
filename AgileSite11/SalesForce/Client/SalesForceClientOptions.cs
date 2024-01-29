namespace CMS.SalesForce
{

    /// <summary>
    /// SalesForce client commnad options.
    /// </summary>
    public sealed class SalesForceClientOptions
    {

        #region "Public properties"

        /// <summary>
        /// Gets or sets the value indicating whether the command operations will run in a transaction.
        /// </summary>
        public bool TransactionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the string values could be truncated.
        /// </summary>
        public bool AttributeTruncationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the changes will be tracked in feeds.
        /// </summary>
        public bool FeedTrackingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the list of most recently used items will be updated.
        /// </summary>
        public bool MruUpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the SOQL query will include deleted entities.
        /// </summary>
        public bool IncludeDeleted { get; set; }

        /// <summary>
        /// Gets or sets the string that identifies a client.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the string that identifies a developer namespace prefix.
        /// </summary>
        public string DefaultNamespace { get; set; }

        /// <summary>
        /// Gets or sets the language name of the labels returned.
        /// </summary>
        public string CultureName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of entities returned from one query command.
        /// </summary>
        public int BatchSize { get; set; }

        #endregion

    }

}