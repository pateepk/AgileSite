using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Scheduler;

using IOExceptions = System.IO;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Cleans Azure Temp and Cache folders on local file system.
    /// </summary>
    public class AzureStorageCacheCleaner : ITask
    {
        #region "Properties"

        /// <summary>
        /// The limit in bytes. The scheduled task starts to delete files after reaching this limit.
        /// </summary>
        private long Threshold
        {
            get
            {
                return ((long)CoreServices.AppSettings["CMSAzureStorageCacheCleanerThreshold"].ToInteger(45)) * 1073741824;
            }
        }


        /// <summary>
        /// How many bytes should be kept on the disk after the cleanup.
        /// </summary>
        private long KeepLimit
        {
            get
            {
                return ((long)CoreServices.AppSettings["CMSAzureStorageCacheCleanerKeepLimit"].ToInteger(10)) * 1073741824;
            }
        }


        /// <summary>
        /// Paths that are excluded from the deletion.
        /// </summary>
        private IEnumerable<string> ExcludedPaths
        {
            get
            {
                return CoreServices.AppSettings["CMSAzureStorageCacheCleanerExcludedPaths"].ToString(String.Empty).Split(new[]{';'}, StringSplitOptions.RemoveEmptyEntries);
            }
        }


        /// <summary>
        /// Directories that are guarded by this scheduled task.
        /// </summary>
        private IEnumerable<string> DirsToSearch
        {
            get
            {
                return new[]
                {
                    PathHelper.CachePath,
                    PathHelper.TempPath
                };
            }
        } 

        #endregion


        #region "Methods"

        /// <summary>
        /// Executes task.
        /// </summary>
        /// <param name="task">TaskInfo</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                var files = DirsToSearch.SelectMany(x => new IOExceptions.DirectoryInfo(x).EnumerateFiles("*", IOExceptions.SearchOption.AllDirectories)).ToList();
                var consumedStorage = files.Sum(fi => fi.Length);

                // Do not continue if we are bellow the storage limit to start deleting files
                if (consumedStorage < Threshold)
                {
                    return String.Empty;
                }

                // Order the files from oldest to newest by creation time, skip excluded files
                var orderedFiles = new Queue<IOExceptions.FileInfo>(files.Where(fi => !IsFileExcluded(fi.FullName)).OrderBy(fi => fi.CreationTimeUtc));

                while ((consumedStorage > KeepLimit) && orderedFiles.Any())
                {
                    // Get the first file in the queue
                    var fi = orderedFiles.Dequeue();

                    try
                    {
                        // Delete the file from file system
                        fi.Delete();

                        // Decrement consumed storage
                        consumedStorage -= fi.Length;
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("AzureStorageCacheCleaner", "DeleteFile", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return String.Empty;
        }


        /// <summary>
        /// Checks if the file should be excluded from the deletion.
        /// </summary>
        /// <param name="path">Full path to the file.</param>
        private bool IsFileExcluded(string path)
        {
            var relativePath = path.Substring(DirsToSearch.Where(path.StartsWith).First().Length + 1);

            return ExcludedPaths.Any(relativePath.StartsWith);
        }

        #endregion
    }
}
