namespace CMS.Synchronization
{
    /// <summary>
    /// Type of processing integration tasks. 
    /// Determines whether the tasks are being processed asynchronously or synchronously with full or partial data.
    /// The types are ordered by priority (highest to lowest).
    /// </summary>
    public enum TaskProcessTypeEnum
    {
        /// <summary>
        /// Synchronous processing with complete object/document data (child objects etc.)
        /// Runs in context of the application allowing retrieving any additional data.
        /// This type is the first priority during processing.
        /// </summary>
        SyncSnapshot = 0,

        /// <summary>
        /// Asynchronous processing with complete object/document data (child objects etc.) including all translations.
        /// This type is the second priority during processing.
        /// </summary>
        AsyncSnapshot = 1,

        /// <summary>
        /// Asynchronous processing with data of object/document itself (including its translations).
        /// This type is the third priority during processing.
        /// </summary>
        AsyncSimpleSnapshot = 2,

        /// <summary>
        /// Asynchronous processing with data of object/document itself (without any translations).
        /// This type is the fourth priority during processing.
        /// </summary>
        AsyncSimple = 3
    }
}