using System.Xml;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Describes data writing operations in repository
    /// </summary>
    internal interface IFileSystemWriter : IFileSystemStorageOperations
    {
        /// <summary>
        /// Stores binary data to a repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <param name="binaryData">Binary data to be stored.</param>
        void WriteToFile(string relativePath, byte[] binaryData);


        /// <summary>
        /// Stores text data to a repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <param name="textData">Content to be stored.</param>
        void WriteToFile(string relativePath, string textData);


        /// <summary>
        /// Stores given document to a repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <param name="document">Document to be stored.</param>
        void WriteToFile(string relativePath, XmlDocument document);


        /// <summary>
        /// Deletes repository file identified by its repository <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">Relative path to the file within repository.</param>
        /// <param name="checkFileExistance">Indicates whether file's existence is checked before deleting it (to prevent <see cref="System.IO.DirectoryNotFoundException"/> from being thrown).</param>
        void DeleteFile(string relativePath, bool checkFileExistance = false);
    }
}