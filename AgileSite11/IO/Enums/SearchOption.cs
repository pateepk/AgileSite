namespace CMS.IO
{
    /// <summary>
    /// Enum of options for IO search capabilities.
    /// </summary>
    public enum SearchOption
    {
        /// <summary>
        /// Includes only the current directory in a search.
        /// </summary>
        TopDirectoryOnly = 0,

        /// <summary>
        /// Includes the current directory and all the subdirectories in a search operation.
        /// This option includes reparse points like mounted drives and symbolic links
        /// in the search.
        /// </summary>
        AllDirectories = 1,
    }
}