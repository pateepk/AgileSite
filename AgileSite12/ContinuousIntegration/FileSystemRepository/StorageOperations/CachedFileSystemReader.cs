using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Class designed to handle repetitive reading of data from a file and storing the file hash in provided <see cref="RepositoryHashManager"/>.
    /// </summary>
    /// <remarks>Relative paths are relative to <see cref="FileSystemRepositoryConfiguration.RepositoryRootPath"/> provided in class constructor.</remarks>
    internal sealed class CachedFileSystemReader : FileSystemReader, ICachedFileSystemReader
    {
        #region "Properties and variables"

        private MemoryCache<byte[]> mCachedBytes;
        private MemoryCache<string> mCachedContents;


        /// <summary>
        /// Provides content bytes of files that were already read.
        /// </summary>
        private MemoryCache<byte[]> CachedBytes
        {
            get
            {
                return mCachedBytes ?? (mCachedBytes = new MemoryCache<byte[]>());
            }
        }


        /// <summary>
        /// Provides content strings of files that were already read.
        /// </summary>
        private MemoryCache<string> CachedContents
        {
            get
            {
                return mCachedContents ?? (mCachedContents = new MemoryCache<string>());
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates new instance of the <see cref="CachedFileSystemReader"/>.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <param name="hashManager">Hash manager.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> or <paramref name="hashManager"/> is null.</exception>
        public CachedFileSystemReader(FileSystemRepositoryConfiguration configuration, RepositoryHashManager hashManager)
            : base(configuration, hashManager)
        {
        }


        /// <summary>
        /// Read content bytes of <paramref name="relativePath"/> from internal caches
        /// or file system repository and computes its hash.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <returns>Array of bytes representing file's content.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public override byte[] ReadBytes(string relativePath)
        {
            CheckRelativePath(relativePath);

            var bytes = CachedBytes.FetchItem(relativePath, () => base.ReadBytes(relativePath));

            return bytes;
        }


        /// <summary>
        /// Read content string of <paramref name="relativePath"/> from internal caches
        /// or file system repository and computes its hash.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <returns>Unicode string representing file's content.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public override string ReadString(string relativePath)
        {
            CheckRelativePath(relativePath);

            var content = CachedContents.FetchItem(relativePath, () => base.ReadString(relativePath));

            return content;
        }


        /// <summary>
        /// Returns hash of file stored under <paramref name="relativePath"/>
        /// in the <see cref="RepositoryHashManager"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public override string GetFileHash(string relativePath)
        {
            CheckRelativePath(relativePath);

            // Ensure the file content is cached (and hash is read)
            ReadBytes(relativePath);
            return RepositoryHashManager.GetHash(relativePath);
        }


        /// <summary>
        /// Removes cached content of each path in <paramref name="relativePaths"/> collection
        /// from internal caches and also from <see cref="RepositoryHashManager"/>.
        /// </summary>
        /// <param name="relativePaths">Collection of relative path of the file.</param>
        /// <remarks>Only non-empty and not-null paths are processed.</remarks>
        public override void RemoveFromCache(IEnumerable<string> relativePaths)
        {
            foreach (var relativePath in relativePaths.Where(path => !String.IsNullOrEmpty(path)))
            {
                var removedFromACache = false;

                removedFromACache |= CachedBytes.RemoveItem(relativePath) != null;

                removedFromACache |= CachedContents.RemoveItem(relativePath) != null;

                if (removedFromACache)
                {
                    RepositoryHashManager.RemoveHash(relativePath);
                }
            }
        }

        #endregion
    }
}
