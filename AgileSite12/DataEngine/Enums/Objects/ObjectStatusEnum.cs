namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration of the object status.
    /// </summary>
    public enum ObjectStatusEnum : int
    {
        /// <summary>
        /// New object.
        /// </summary>
        New = 0,

        /// <summary>
        /// Object that didn't change.
        /// </summary>
        Unchanged = 1,

        /// <summary>
        /// Object that has changed.
        /// </summary>
        Changed = 2,

        /// <summary>
        /// Object that is flagged to be deleted.
        /// </summary>
        ToBeDeleted = 3,

        /// <summary>
        /// Object that has been deleted from the database
        /// </summary>
        WasDeleted = 6
    }
}