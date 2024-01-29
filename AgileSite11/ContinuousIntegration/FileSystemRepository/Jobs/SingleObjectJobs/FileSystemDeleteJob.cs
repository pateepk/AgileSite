using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.IO;

using IOExceptions = System.IO;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class designated for removal of serialized BaseInfo objects from the file system.
    /// </summary>
    public class FileSystemDeleteJob : AbstractSingleObjectJob
    {
        /// <summary>
        /// Creates a new file system delete job with given repository configuration.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public FileSystemDeleteJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Deletes given <paramref name="baseInfo"/> from the repository.
        /// </summary>
        /// <param name="baseInfo">Base info which will be deleted.</param>
        /// <remarks>Provided <paramref name="baseInfo"/> is never <see langword="null"/>.</remarks>
        protected override void RunInternal(BaseInfo baseInfo)
        {
            // Delete files
            var deletedFilesRelativePaths = SelectFilesToDelete(baseInfo)
                .Select(DeleteFile)
                .Where(deletedFilePath => !String.IsNullOrEmpty(deletedFilePath))
                .ToHashSet();

            // Delete empty folders from the deepest ones
            var deletedDirectories = deletedFilesRelativePaths
                .Select(Path.GetDirectoryName)
                .ToHashSet()
                .OrderByDescending(dir => dir.Length);

            foreach (var relativeDirectoryPath in deletedDirectories)
            {
                FileSystemRepositoryHelper.DeleteRepositoryDirectoryIfEmpty(relativeDirectoryPath, RepositoryConfiguration.RepositoryRootPath);
            }
        }


        /// <summary>
        /// Returns text that is shown in <see cref="ObjectSerializationException"/> thrown when execution of <see cref="RunInternal(BaseInfo)"/> fails with an exception.
        /// </summary>
        /// <param name="baseInfo">Object that was passed to the <see cref="RunInternal(BaseInfo)"/> method.</param>
        /// <remarks>Provided <paramref name="baseInfo"/> is never <see langword="null"/>.</remarks>
        protected override string ObjectSerializationExceptionMessage(BaseInfo baseInfo)
        {
            return "Deletion of the object " + baseInfo + " has failed.";
        }


        /// <summary>
        /// Selects files to be deleted in order to delete given <paramref name="baseInfo"/> from the repository.
        /// </summary>
        /// <param name="baseInfo">Base info which will be deleted.</param>
        /// <returns>Collection of file paths to deleted.</returns>
        /// <remarks>Passed <paramref name="baseInfo"/> is never null.</remarks>
        protected virtual IEnumerable<string> SelectFilesToDelete(BaseInfo baseInfo)
        {
            // For current operation there is no benefit in storing current object in translation helper.
            // However translation helper might be shared on global level and subsequent operations might benefit from it.
            RegisterTranslationRecord(baseInfo);

            var relativePaths = RepositoryPathHelper
                .GetExistingSerializationFiles(baseInfo)
                .SelectMany(location => location)
                .ToHashSet();

            if (!baseInfo.TypeInfo.IsBinding)
            {
                // Type is not binding, no special processing required (just delete the file(s))
                return relativePaths;
            }

            var relativePath = relativePaths.SingleOrDefault();
            if (relativePath == default(string))
            {
                // Infos consisting of multiple files are not supported for bindings (just delete the files)
                return relativePaths;
            }

            // Process bindings
            var document = BindingsProcessor.RemoveBinding(baseInfo, relativePath);
            if (document == FileSystemBindingsProcessor.NO_BINDINGS)
            {
                // No bindings left after removal of current one, so the file can be deleted
                return relativePaths;
            }

            // Some bindings left after removal of current one, so the file must be updated (and not removed)
            BindingsProcessor.WriteBindings(relativePath, document);

            // Current file doesn't need to be removed anymore
            relativePaths.Remove(relativePath);

            return relativePaths;
        }


        /// <summary>
        /// Deletes the file specified by its relative path within the repository from the file system.
        /// </summary>
        /// <param name="relativePath">Relative path to the file which will be deleted.</param>
        /// <exception cref="ObjectSerializationException">Thrown when deleting the file failed</exception>
        /// <returns>Returns relative path if deletion succeeded, <see cref="String.Empty"/> if file already did not exist.</returns>
        private string DeleteFile(string relativePath)
        {
            try
            {
                FileSystemWriter.DeleteFile(relativePath);
                return relativePath;
            }
            catch (IOExceptions.DirectoryNotFoundException)
            {
                // File is not in the repository anymore
                return String.Empty;
            }
            catch (Exception ex)
            {
                throw new ObjectSerializationException("Deleting of serialized object has failed.", ex);
            }
        }
    }
}
