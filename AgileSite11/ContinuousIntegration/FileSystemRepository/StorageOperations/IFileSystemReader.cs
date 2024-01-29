namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Describes data reading operations in repository
    /// </summary>
    internal interface IFileSystemReader : IFileSystemStorageOperations
    {
        /// <summary>
        /// Reads given file from the repository and computes its hash.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <returns>Array of bytes representing file's content.</returns>
        byte[] ReadBytes(string relativePath);


        /// <summary>
        /// Reads given file from the repository and computes its hash.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <returns>Unicode string representing file's content.</returns>
        string ReadString(string relativePath);
    }
}