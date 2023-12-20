using System;

using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Basic file operations in repository
    /// </summary>
    internal interface IFileSystemStorageOperations
    {
        /// <summary>
        /// Configuration of a file system repository.
        /// </summary>
        FileSystemRepositoryConfiguration RepositoryConfiguration
        {
            get;
        }


        /// <summary>
        /// Collection of repository locations and hashes of the content of all parts of the object serialized or amended by a inheriting class method call.
        /// If hash value is null, the file in the corresponding location has been removed (typically occurs when base info uses separated fields
        /// with dynamic extension and the extension has changed, resulting in new file being created).
        /// </summary>
        /// <remarks>
        /// Repository location is a relative path starting from the repository root.
        /// <para>Repository is incremental (i.e. repeated invokes of jobs supporting multiple executions potentially results in growing
        /// of number of locations and hashes stored within the manager.</para>
        /// </remarks>
        RepositoryHashManager RepositoryHashManager
        {
            get;
        }


        /// <summary>
        /// Returns hash of file stored under <paramref name="relativePath"/> in the <see cref="IFileSystemStorageOperations.RepositoryHashManager"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        string GetFileHash(string relativePath);
    }
}