using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using CMS.IO;
using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Class designed to handle reading of data from a file and storing the file hash in provided <see cref="RepositoryHashManager"/>.
    /// </summary>
    /// <remarks>Relative paths are relative to <see cref="FileSystemRepositoryConfiguration.RepositoryRootPath"/> provided in class constructor.</remarks>
    internal abstract class FileSystemReader : FileSystemStorageOperationsBase, IFileSystemReader
    {
        /// <summary>
        /// Creates a new file system writer with given repository configuration and hash manager.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <param name="hashManager">Hash manager.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> or <paramref name="hashManager"/> is null.</exception>
        protected FileSystemReader(FileSystemRepositoryConfiguration configuration, RepositoryHashManager hashManager)
            : base(configuration, hashManager)
        {
        }


        /// <summary>
        /// Reads given file from the repository and computes its hash.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <returns>Array of bytes representing file's content.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public virtual byte[] ReadBytes(string relativePath)
        {
            CheckRelativePath(relativePath);

            string absolutePath = RepositoryConfiguration.GetAbsolutePath(relativePath);

            try
            {
                byte[] content;
                using (var hashAlgorithm = FileSystemCryptoHelper.GetHashAlgorithm())
                {
                    using (var fileStream = File.OpenRead(absolutePath))
                    {
                        using (var stream = fileStream)
                        {
                            using (var hashStream = new CryptoStream(stream, hashAlgorithm, CryptoStreamMode.Read))
                            {
#pragma warning disable BH1014 // Do not use System.IO
                                using (var reader = new System.IO.BinaryReader(hashStream))
#pragma warning restore BH1014 // Do not use System.IO
                                {
                                    content = reader.ReadBytes((int)fileStream.Length);
                                }
                            }
                        }
                    }
                    RepositoryHashManager.SaveHash(hashAlgorithm, relativePath);
                }

                return content;
            }
            catch (System.IO.PathTooLongException ex)
            {
                throw new InvalidOperationException($"Cannot read file '{absolutePath}' from current repository location because the file path is too long.", ex);
            }
        }


        /// <summary>
        /// Reads given file from the repository and computes its hash.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <returns>Unicode string representing file's content.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public virtual string ReadString(string relativePath)
        {
            var bytes = ReadBytes(relativePath);
            
            // This approach for reading the bytes removes BOM (if present)
            using (var stream = new System.IO.MemoryStream(bytes))
            {
                using (var reader = StreamReader.New(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }


        /// <summary>
        /// Removes cached content of each path in <paramref name="relativePaths"/> collection
        /// from internal caches and also from <see cref="RepositoryHashManager"/>.
        /// </summary>
        /// <param name="relativePaths">Collection of relative path of the file.</param>
        /// <remarks>Only non-empty and not-null paths are processed.</remarks>
        public virtual void RemoveFromCache(IEnumerable<string> relativePaths)
        {
        }
    }
}
