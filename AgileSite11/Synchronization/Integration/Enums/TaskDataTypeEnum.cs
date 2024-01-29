namespace CMS.Synchronization
{
    /// <summary>
    /// Type of data contained in task to process.
    /// </summary>
    public enum TaskDataTypeEnum
    {
        /// <summary>
        /// Just main object.
        /// </summary>
        Simple = 0,

        /// <summary>
        /// Main object with translation information.
        /// </summary>
        SimpleSnapshot = 1,

        /// <summary>
        /// Object with child objects and bindings.
        /// </summary>
        Snapshot = 2
    }
}