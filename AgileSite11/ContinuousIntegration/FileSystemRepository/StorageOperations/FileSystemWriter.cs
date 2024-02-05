using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using CMS.Helpers;
using CMS.IO;
using CMS.ContinuousIntegration.Internal;

using Stream = System.IO.Stream;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Class designed to handle writing of data to a file and storing the resulting file hash in provided <see cref="RepositoryHashManager"/>.
    /// </summary>
    /// <remarks>Relative paths are relative to <see cref="FileSystemRepositoryConfiguration.RepositoryRootPath"/> provided in class constructor.</remarks>
    internal sealed class FileSystemWriter : FileSystemStorageOperationsBase, IFileSystemWriter
    {
        #region "Public methods"

        /// <summary>
        /// Creates a new file system writer with given repository configuration and hash manager.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <param name="hashManager">Hash manager.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> or <paramref name="hashManager"/> is null.</exception>
        public FileSystemWriter(FileSystemRepositoryConfiguration configuration, RepositoryHashManager hashManager)
            : base(configuration, hashManager)
        {
        }


        /// <summary>
        /// Stores binary data to a repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <param name="binaryData">Binary data to be stored.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public void WriteToFile(string relativePath, byte[] binaryData)
        {
            StoreToFile(relativePath, stream =>
            {
#pragma warning disable BH1014 // Do not use System.IO
                using (var writer = new System.IO.BinaryWriter(stream))
#pragma warning restore BH1014 // Do not use System.IO
                {
                    writer.Write(binaryData);
                }
            });
        }


        /// <summary>
        /// Stores text data to a repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <param name="textData">Content to be stored.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public void WriteToFile(string relativePath, string textData)
        {
            StoreToFile(relativePath, stream =>
            {
#pragma warning disable BH1014 // Do not use System.IO
                using (var writer = new System.IO.StreamWriter(stream, EncodingConfiguration.Encoding))
#pragma warning restore BH1014 // Do not use System.IO
                {
                    writer.Write(textData);
                }
            });
        }


        /// <summary>
        /// Stores given document to a repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <param name="document">Document to be stored.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public void WriteToFile(string relativePath, XmlDocument document)
        {
            StoreToFile(relativePath, stream => document.WriteFormattedXmlToStream(stream, encoding: EncodingConfiguration.Encoding));
        }


        /// <summary>
        /// Deletes repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path to the file within repository.</param>
        /// <param name="checkFileExistance">Indicates whether file's existence is checked before deleting it (to prevent <see cref="System.IO.DirectoryNotFoundException"/> from being thrown).</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        public void DeleteFile(string relativePath, bool checkFileExistance = false)
        {
            CheckRelativePath(relativePath);

            var absolutePath = RepositoryConfiguration.GetAbsolutePath(relativePath);
            if (!checkFileExistance || File.Exists(absolutePath))
            {
                try
                {
                    File.Delete(absolutePath);
                }
                catch (System.IO.PathTooLongException ex)
                {
                    throw new InvalidOperationException($"Cannot delete file '{absolutePath}' from current repository location because the file path is too long.", ex);
                }

                RepositoryHashManager.RemoveHash(relativePath);
            }
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Stores data to file using given action that writes to created file stream.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <param name="fileStreamWriteAction">Action that writes data to the given stream.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty.</exception>
        private void StoreToFile(string relativePath, Action<Stream> fileStreamWriteAction)
        {
            CheckRelativePath(relativePath);

            using (var hashAlgorithm = FileSystemCryptoHelper.GetHashAlgorithm())
            {
                using (var stream = GetFileCryptoStream(relativePath, hashAlgorithm))
                {
                    fileStreamWriteAction(stream);
                }
                RepositoryHashManager.SaveHash(hashAlgorithm, relativePath);
            }
        }


        /// <summary>
        /// Gets CryptoStream for writing to the file with given relative path.
        /// </summary>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <param name="hashAlgorithm">Hash algorithm that is used by CryptoStream.</param>
        /// <returns>CryptoStream for writing to the file with given relative path.</returns>
        private CryptoStream GetFileCryptoStream(string relativePath, HashAlgorithm hashAlgorithm)
        {
            var absolutePath = RepositoryConfiguration.GetAbsolutePath(relativePath);

            try
            {
                DirectoryHelper.CreateDirectory(Path.GetDirectoryName(absolutePath));

                var fileStream = File.Open(absolutePath, FileMode.Create, FileAccess.Write);

                return new CryptoStream(fileStream, hashAlgorithm, CryptoStreamMode.Write);
            }
            catch (System.IO.PathTooLongException ex)
            {
                throw new InvalidOperationException($"Cannot write file '{absolutePath}' into current repository location because the file path is too long.", ex);
            }
        }

        #endregion
    }
}
