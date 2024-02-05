using System.Collections.Generic;
using System.Security.Cryptography;

using SystemIO = System.IO;

namespace CIConsistencyChecker
{
    /// <summary>
    /// Helper for obtaining hashes of files.
    /// </summary>
    internal static class FileHashHelper
    {
        private static readonly SHA1 Sha1 = SHA1.Create();

        /// <summary>
        /// Gets all files from given directory and hashes of their content.
        /// </summary>
        /// <param name="path">Path to the directory.</param>
        /// <returns>Pair of file name with its hashed content.</returns>
        public static Dictionary<string, byte[]> GetAllFilesWithHashedContent(string path)
        {
            var result = new Dictionary<string, byte[]>();

            string[] files = SystemIO.Directory.GetFiles(path, "*.*", SystemIO.SearchOption.AllDirectories);

            foreach (string file in files)
            {
                var hash = GetHashedContentOfFile(file);

                result.Add(file, hash);
            }

            return result;
        }


        /// <summary>
        /// Returns content hash of given file.
        /// </summary>
        public static byte[] GetHashedContentOfFile(string fileName)
        {
            using (var fileStream = SystemIO.File.OpenRead(fileName))
            {
                return Sha1.ComputeHash(fileStream);
            }
        }
    }
}
