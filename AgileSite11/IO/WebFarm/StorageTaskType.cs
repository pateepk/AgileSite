namespace CMS.IO
{
    /// <summary>
    /// Web farm task types for IO operations
    /// </summary>
    public class StorageTaskType
    {
        /// <summary>
        /// Delete physical folder.
        /// </summary>
        public const string DeleteFolder = "DELETEFOLDER";

        /// <summary>
        /// Update physical file
        /// </summary>
        public const string UpdatePhysicalFile = "UPDATEPHYSICALFILE";

        /// <summary>
        /// Delete physical file
        /// </summary>
        public const string DeletePhysicalFile = "DELETEPHYSICALFILE";
    }
}
