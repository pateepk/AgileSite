namespace CMS.IO
{
    /// <summary>
    /// Type of the IO operation (provider enumeration).
    /// </summary>
    public class IOProviderName
    {
        /// <summary>
        /// General IO operations.
        /// </summary>
        public const string FileSystem = "Local";

        /// <summary>
        /// Zip file operations
        /// </summary>
        public const string Zip = "Zip";

        /// <summary>
        /// Azure blob operations.
        /// </summary>
        public const string Azure = "Azure";

        /// <summary>
        /// Amazon S3 operations.
        /// </summary>
        public const string Amazon = "Amazon";
    }
}