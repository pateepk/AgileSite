using SystemIO = System.IO;

namespace CIConsistencyChecker
{
    /// <summary>
    /// Copies files including directory structure.
    /// </summary>
    public class FileCopier
    {
        private readonly string baseSourceDirectoryFullPath;
        private readonly string baseDestinationDirectoryFullPath;


        /// <summary>
        /// Creates instance of file copier.
        /// </summary>
        /// <param name="baseSourceDirectoryFullPath">Full path to base source directory to copy files from.</param>
        /// <param name="baseDestinationDirectoryFullPath">Full path to base destination directory to copy files to.</param>
        public FileCopier(string baseSourceDirectoryFullPath, string baseDestinationDirectoryFullPath)
        {
            this.baseSourceDirectoryFullPath = baseSourceDirectoryFullPath;
            this.baseDestinationDirectoryFullPath = baseDestinationDirectoryFullPath;
        }


        /// <summary>
        /// Copies the file specified by <paramref name="fileRelativePath"/> to destination directory.
        /// Directories which does not exists in destination path are created.
        /// If the file does not exists in source directory, no action is performed.
        /// </summary>
        /// <param name="fileRelativePath">Relative path from source directory to the file.</param>
        public void TryCopy(string fileRelativePath)
        {
            var sourceFileFullPath = SystemIO.Path.Combine(baseSourceDirectoryFullPath, fileRelativePath);
            if (SystemIO.File.Exists(sourceFileFullPath))
            {
                var destinationFileFullPath = SystemIO.Path.Combine(baseDestinationDirectoryFullPath, fileRelativePath);
                EnsureFileDirectoryExists(destinationFileFullPath);
                SystemIO.File.Copy(sourceFileFullPath, destinationFileFullPath);
            }
        }


        /// <summary>
        /// Ensures that destination directory tree exists for given <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">File path.</param>
        private static void EnsureFileDirectoryExists(string filePath)
        {
            SystemIO.Directory.CreateDirectory(SystemIO.Path.GetDirectoryName(filePath));
        }
    }
}
