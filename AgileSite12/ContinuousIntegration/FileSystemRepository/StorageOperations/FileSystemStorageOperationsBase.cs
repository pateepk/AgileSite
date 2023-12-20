using System;

using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Base class to handle reading/writing of data to/from a file and storing the file hash in provided <see cref="RepositoryHashManager"/>.
    /// </summary>
    /// <remarks>Relative paths are relative to <see cref="FileSystemRepositoryConfiguration.RepositoryRootPath"/> provided in class constructor.</remarks>
    internal abstract class FileSystemStorageOperationsBase : IFileSystemStorageOperations
    {
        #region "Properties"

        /// <summary>
        /// Configuration of a file system repository.
        /// </summary>
        public FileSystemRepositoryConfiguration RepositoryConfiguration
        {
            get;
            private set;
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
        /// <seealso cref="FileSystemStoreJob"/>
        /// <seealso cref="FileSystemDeleteJob"/>
        /// <seealso cref="FileSystemUpsertObjectsByTypeJob"/>
        /// <seealso cref="FileSystemDeleteObjectsByTypeJob"/>
        /// <seealso cref="FileSystemBindingsProcessor"/>
        public RepositoryHashManager RepositoryHashManager
        {
            get;
            private set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates new instance of <see cref="FileSystemStorageOperationsBase"/>.
        /// </summary>
        /// <param name="repositoryConfiguration">Repository configuration.</param>
        /// <param name="repositoryHashManager">Collection of repository locations and hashes of the content of all parts of the object serialized or amended by a inheriting class method call.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="repositoryConfiguration"/> or <paramref name="repositoryHashManager"/> is null.</exception>
        protected FileSystemStorageOperationsBase(FileSystemRepositoryConfiguration repositoryConfiguration, RepositoryHashManager repositoryHashManager)
        {
            if (repositoryHashManager == null)
            {
                throw new ArgumentNullException("repositoryHashManager");
            }
            if (repositoryConfiguration == null)
            {
                throw new ArgumentNullException("repositoryConfiguration");
            }

            RepositoryConfiguration = repositoryConfiguration;

            RepositoryHashManager = repositoryHashManager;
        }


        /// <summary>
        /// Returns hash of file stored under <paramref name="relativePath"/> in the <see cref="RepositoryHashManager"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public virtual string GetFileHash(string relativePath)
        {
            CheckRelativePath(relativePath);

            return RepositoryHashManager.GetHash(relativePath);
        }

        #endregion


        #region "Protected methods"
        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the provided <paramref name="relativePath"/> is null or empty (or white spaced).
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        protected static void CheckRelativePath(string relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Relative path cannot be null or empty.", "relativePath");
            }
        }

        #endregion
    }
}
