using System.Collections.Generic;
using System.Linq;

using SystemIO = System.IO;

namespace CIConsistencyChecker
{
    /// <summary>
    /// Represents comparator of all files (including files in subdirectories) and their content in two directories.
    /// </summary>
    internal class DirectoryContentComparator
    {
        private const string CURRENT_DIRECTORY = @".\";

        private readonly string mRepositoryPathWithNewFiles;

        private readonly string mRepositoryPathWithOriginalFiles;


        public DirectoryContentComparator(string repositoryPathWithOriginalFiles, string repositoryPathWithNewFiles)
        {
            mRepositoryPathWithOriginalFiles = repositoryPathWithOriginalFiles;
            mRepositoryPathWithNewFiles = repositoryPathWithNewFiles;
        }


        /// <summary>
        /// Compares all files.
        /// </summary>
        /// <returns>All differences in files' content.</returns>
        public IEnumerable<Issue> Compare()
        {
            var issues = new Issues();

            var originalFilesWithHashedContent = GetAllOriginalFilesWithHashedContent();
            string[] newFiles = GetAllNewFiles();

            foreach (var fileName in newFiles)
            {
                byte[] originalFileHashedContent;

                if (!HasHashForFile(originalFilesWithHashedContent, fileName, out originalFileHashedContent))
                {
                    // File is in new serialized repository, but there is no hash for original file of given name.
                    // That means the file is missing in the original repository.
                    issues.Add(fileName, "- Repository serialized from database contains extra file that was not part of original CI repository in VCS.");
                    continue;
                }

                if (!IsFileContentHashEqual(fileName, originalFileHashedContent))
                {
                    var errorMessage = GetFileDifferenceErrorMessage(fileName);
                    issues.Add(fileName, errorMessage);
                }

                // The file has been checked, removing it to see if any original-only files remain.
                originalFilesWithHashedContent.Remove(fileName);
            }

            // Files which did not get any match - original files which were not found in the new serialized repository.
            issues.AddRange(originalFilesWithHashedContent.Keys, "- File is not part of repository serialized from database and it is probably redundant.");

            return issues;
        }


        /// <summary>
        /// Returns collection of filenames from new serialized repository.
        /// </summary>
        private string[] GetAllNewFiles()
        {
            SystemIO.Directory.SetCurrentDirectory(mRepositoryPathWithNewFiles);
            var newFiles = SystemIO.Directory.GetFiles(CURRENT_DIRECTORY, "*.*", SystemIO.SearchOption.AllDirectories);

            return newFiles;
        }


        /// <summary>
        /// Returns file names from original CIRepository with their hashed content.
        /// </summary>
        private Dictionary<string, byte[]> GetAllOriginalFilesWithHashedContent()
        {
            SystemIO.Directory.SetCurrentDirectory(mRepositoryPathWithOriginalFiles);
            var originalFilesWithHashedContent = FileHashHelper.GetAllFilesWithHashedContent(CURRENT_DIRECTORY);

            return originalFilesWithHashedContent;
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="fileName"/> is in <paramref name="filesWithHashedContent"/> keys, otherwise <c>false</c>.
        /// </summary>
        private static bool HasHashForFile(Dictionary<string, byte[]> filesWithHashedContent, string fileName, out byte[] hashedContent)
        {
            return filesWithHashedContent.TryGetValue(fileName, out hashedContent);
        }


        private static bool IsFileContentHashEqual(string fileName, byte[] hash)
        {
            return hash.SequenceEqual(FileHashHelper.GetHashedContentOfFile(fileName));
        }


        private string GetFileDifferenceErrorMessage(string fileName)
        {
            var originalFileFullPath = SystemIO.Path.Combine(mRepositoryPathWithOriginalFiles, fileName);
            var originalFileContent = SystemIO.File.ReadAllLines(originalFileFullPath);

            var newFileFullPath = SystemIO.Path.Combine(mRepositoryPathWithNewFiles, fileName);
            var newFileContent = SystemIO.File.ReadAllLines(newFileFullPath);

            return StringArrayComparator.Compare(originalFileContent, newFileContent);
        }
    }
}
