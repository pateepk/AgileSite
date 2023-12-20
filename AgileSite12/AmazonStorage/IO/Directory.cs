using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

using Amazon.S3;
using Amazon.S3.Model;

using CMS.IO;
using CMS.Base;

using Path = System.IO.Path;
using SearchOption = CMS.IO.SearchOption;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Implementation of Directory class for Amazon S3.
    /// </summary>
    public class Directory : AbstractDirectory
    {
        #region "Variables"

        private static string mCurrentDirectory;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns current directory. Value remains the same so it can be cached.
        /// </summary>
        internal static string CurrentDirectory
        {
            get
            {
                return mCurrentDirectory ?? (mCurrentDirectory = SystemContext.WebApplicationPhysicalPath.ToLowerInvariant());
            }
        }


        /// <summary>
        /// Returns S3Object provider.
        /// </summary>
        private IS3ObjectInfoProvider Provider
        {
            get
            {
                return S3ObjectFactory.Provider;
            }
        }

        #endregion


        #region "Public methods - overrides"

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">Path to test.</param>
        public override bool Exists(string path)
        {
            return ExistsInFileSystem(path) || ExistsInS3Storage(path);
        }


        /// <summary>
        /// Creates all directories and subdirectories as specified by path.
        /// </summary>
        /// <param name="path">Path to create.</param>
        public override IO.DirectoryInfo CreateDirectory(string path)
        {
            // Get valid path
            path = PathHelper.GetValidPath(path);

            // Check if directory already exists
            if (!Exists(path))
            {
                // Create empty S3 objects which represents the directory
                var obj = S3ObjectFactory.GetInfo(path);
                obj.Key = obj.Key + "/";
                Provider.CreateEmptyObject(obj);

                // When blob is created create directory info
                var info = new DirectoryInfo(path);

                // Fill informations
                info.CreationTime = DateTime.Now;
                info.Exists = true;
                info.FullName = path;
                info.LastWriteTime = info.CreationTime;
                info.Name = Path.GetFileName(path);

                return info;
            }

            // Directory already exist
            return new DirectoryInfo(path);
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            FileDebug.LogFileOperation(path, FileDebugOperation.ENUMERATE_FILES, -1, null, null, IOProviderName.Amazon);

            return EnumerateFilesCore(path, searchPattern);
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        private IEnumerable<string> EnumerateFilesCore(string path, string searchPattern)
        {
            IEnumerable<string> files = null;

            // Get files from file system
            if (ExistsInFileSystem(path))
            {
                files = System.IO.Directory.EnumerateFiles(path, searchPattern).Select(f => f.ToLowerInvariant());
            }

            path = PathHelper.GetValidPath(path);

            // Prepare search condition
            Func<string, bool> searchCondition = GetSearchCondition(searchPattern);

            // Get list of objects
            IEnumerable<string> s3FileList = Provider.GetObjectsList(path, ObjectTypeEnum.Files)
                .Where(f => searchCondition(IO.Path.GetFileName(f)))
                .Select(f => f.ToLowerInvariant());

            return files == null ? s3FileList : files.Union(s3FileList, StringComparer.Ordinal);
        }


        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern, or an empty array if no files are found.</returns>
        public override string[] GetFiles(string path, string searchPattern)
        {
            var files = EnumerateFilesCore(path, searchPattern).ToArray();

            FileDebug.LogFileOperation(path, FileDebugOperation.GET_FILES, -1, files.Length.ToString(), null, IOProviderName.Amazon);

            return files;
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern and option.</returns>
        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            FileDebug.LogFileOperation(path, FileDebugOperation.ENUMERATE_DIRECTORIES, -1, null, null, IOProviderName.Amazon);

            return EnumerateDirectoriesCore(path, searchPattern, searchOption);
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path,
        /// and optionally searches subdirectories.
        /// </summary>
        private IEnumerable<string> EnumerateDirectoriesCore(string path, string searchPattern, SearchOption searchOption)
        {
            IEnumerable<string> directories = null;

            // Get files from file system
            if (ExistsInFileSystem(path))
            {
                directories = System.IO.Directory.EnumerateDirectories(path, searchPattern, (System.IO.SearchOption)searchOption).Select(d => d.ToLowerInvariant());
            }

            path = PathHelper.GetValidPath(path);

            // Prepare search condition
            Func<string, bool> searchCondition = GetSearchCondition(searchPattern);

            // Get list of objects
            List<string> s3DirectoryList = Provider.GetObjectsList(path, ObjectTypeEnum.Directories, searchOption == SearchOption.AllDirectories);

            var s3Directories = s3DirectoryList
                .Select(d => d.TrimEnd('\\'))
                .Where(d => searchCondition(IO.Path.GetFileName(d)))
                .Select(d => d.ToLowerInvariant());

            return directories == null ? s3Directories : directories.Union(s3Directories, StringComparer.Ordinal);
        }


        /// <summary>
        /// Gets the names of the subdirectories (including their paths) that match the specified search pattern in the current directory,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>An array of the full names (including paths) of the subdirectories that match the specified criteria, or an empty array if no directories are found.</returns>
        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            var directories = EnumerateDirectoriesCore(path, searchPattern, searchOption).ToArray();

            FileDebug.LogFileOperation(path, FileDebugOperation.GET_DIRECTORIES, -1, directories.Length.ToString(), null, IOProviderName.Amazon);

            return directories;
        }


        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="recursive">Deletes all sub directories in given path.</param>
        public override void Delete(string path, bool recursive)
        {
            // Does path exist?
            if (ExistsInS3Storage(path))
            {
                if (recursive)
                {
                    DeleteInternal(path, true);
                }
                else
                {
                    // Check if directory is empty
                    List<string> objects = Provider.GetObjectsList(path, ObjectTypeEnum.FilesAndDirectories);
                    if (objects.Count == 0)
                    {
                        var obj = S3ObjectFactory.GetInfo(path);
                        Provider.DeleteObject(obj);
                    }
                    else
                    {
                        throw new InvalidOperationException("Directory is not empty.");
                    }
                }

                // Delete data from temp and cache directories
                if (path.StartsWith(CurrentDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(CurrentDirectory.Length);

                    try
                    {
                        System.IO.Directory.Delete(Path.Combine(PathHelper.TempPath, path));
                        System.IO.Directory.Delete(Path.Combine(PathHelper.CachePath, path));
                    }
                    catch (IOException)
                    {
                        // Operation is not crucial, in case that something went wrong (e.g. concurrent access, etc.) during the cleanup, suppress the exception
                    }
                }
            }
            else if (ExistsInFileSystem(path))
            {
                throw new InvalidOperationException("Cannot delete path '" + path + @"' because it's not in Amazon S3 storage and it exists only in local file system.
                    This exception typically occurs when file system is mapped to Amazon S3 storage after the file or directory
                    '" + path + "' was created in the local file system. To fix this issue remove specified file or directory.");
            }
            else
            {
                throw new DirectoryNotFoundException("Path '" + path + "' does not exist.");
            }
        }


        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public override void Delete(string path)
        {
            Delete(path, false);
        }


        /// <summary>
        /// Moves directory.
        /// </summary>
        /// <param name="sourceDirName">Source directory name.</param>
        /// <param name="destDirName">Destination directory name.</param>
        public override void Move(string sourceDirName, string destDirName)
        {
            Move(sourceDirName, destDirName, 0);
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified file.
        /// </summary>
        /// <param name="path">Path to directory.</param>
        public override DirectorySecurity GetAccessControl(string path)
        {
            // In Amazon S3 has user full control over the filesystem
            return new DirectorySecurity();
        }


        /// <summary>
        /// Prepares files for import. Converts them to lower case.
        /// </summary>
        /// <param name="path">Path.</param>
        public override void PrepareFilesForImport(string path)
        {
            path = PathHelper.GetValidPath(path);

            // Get all objects
            List<string> objects = Provider.GetObjectsList(path, ObjectTypeEnum.FilesAndDirectories, true, false);
            foreach (string obj in objects)
            {
                string key = PathHelper.GetObjectKeyFromPath(obj, false);
                var source = S3ObjectFactory.GetInfo(key, true);

                // Get lowered name
                string lowered = source.Key.ToLowerInvariant();

                // Should be renamed?
                if (lowered != source.Key)
                {
                    // Prepare objects
                    var sourceObj = S3ObjectFactory.GetInfo(source.Key, true);
                    var destObj = S3ObjectFactory.GetInfo(lowered, true);

                    // Rename object
                    Provider.CopyObjects(sourceObj, destObj);

                    // Delete source
                    Provider.DeleteObject(sourceObj);
                }
            }
        }


        /// <summary>
        /// Deletes all files in the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full path of the directory to delete</param>
        public override void DeleteDirectoryStructure(string path)
        {
            DeleteInternal(path, false);
        }

        #endregion


        #region "Public methods - other"

        /// <summary>
        /// Determines whether the given path refers to an existing directory on Amazon S3 storage.
        /// </summary>
        /// <param name="path">Path to test.</param>
        public static bool ExistsInS3Storage(string path)
        {
            path = PathHelper.GetValidPath(path);
            string key = PathHelper.GetObjectKeyFromPath(path);

            // Some directory - not the root
            if (!string.IsNullOrEmpty(key))
            {
                key = key + "/";
                IS3ObjectInfo obj = S3ObjectFactory.GetInfo(key, true);
                return S3ObjectFactory.Provider.ObjectExists(obj);
            }

            // Root directory exists always
            return true;
        }


        /// <summary>
        /// Determines whether the given path refers to an existing directory on file system
        /// </summary>
        /// <param name="path">Path to test.</param>
        public static bool ExistsInFileSystem(string path)
        {
            return System.IO.Directory.Exists(path);
        }


        /// <summary>
        /// Creates directory structure on disk for given path.
        /// </summary>
        /// <param name="path">Path with temporary file.</param>
        public static void CreateDiskDirectoryStructure(string path)
        {
            string directory = IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Moves directory.
        /// </summary>
        /// <param name="sourceDirName">Source directory name.</param>
        /// <param name="destDirName">Destination directory name.</param>
        /// <param name="level">Current nested level.</param>
        private void Move(string sourceDirName, string destDirName, int level)
        {
            sourceDirName = PathHelper.GetValidPath(sourceDirName);
            destDirName = PathHelper.GetValidPath(destDirName);
            destDirName = destDirName.Trim('\\');

            if (Exists(sourceDirName))
            {
                // Create empty directory if not exists
                if (!ExistsInS3Storage(destDirName))
                {
                    CreateDirectory(destDirName);
                }

                // Copy files
                string[] files = GetFiles(sourceDirName);
                foreach (string file in files)
                {
                    IO.File.Copy(file, destDirName + "\\" + IO.Path.GetFileName(file));
                }

                // Recursive Copy directories
                string[] dirs = GetDirectories(sourceDirName);
                foreach (string dir in dirs)
                {
                    Move(dir, destDirName + "\\" + IO.Path.GetFileName(dir), level + 1);
                }

                // Delete directories
                if (level == 0)
                {
                    Delete(sourceDirName, true);
                }
            }
            else
            {
                throw new Exception("Source path " + sourceDirName + " does not exist.");
            }
        }


        /// <summary>
        /// Deletes files and optionally directories in given path.
        /// </summary>
        /// <param name="path">Path to delete.</param>
        /// <param name="directories">Indicates, whether directories should be also deleted.</param>
        private void DeleteInternal(string path, bool directories)
        {
            path = PathHelper.GetValidPath(path);

            // Get Amazon storage objects
            var objects = Provider.GetObjectsList(path, directories ? ObjectTypeEnum.FilesAndDirectories : ObjectTypeEnum.Files, true, true, false)
                              .ConvertAll(p => new KeyVersion
                              {
                                  Key = PathHelper.GetObjectKeyFromPath(p, true)
                              })
                              .Batch(S3ObjectInfoProvider.MAX_OBJECTS_PER_REQUEST);

            foreach (var objectsBatch in objects)
            {
                var dor = new DeleteObjectsRequest
                {
                    BucketName = AccountInfo.Current.BucketName
                };
                dor.Objects = objectsBatch.ToList();

                try
                {
                    AccountInfo.Current.S3Client.DeleteObjects(dor);
                }
                catch (DeleteObjectsException ex)
                {
                    throw new IOException(string.Format("Some of the directory '{0}' underlying objects weren't deleted correctly.", path), ex);
                }
            }

            // Delete containing folder
            var obj = S3ObjectFactory.GetInfo(IO.Path.EnsureEndBackslash(path));
            Provider.DeleteObject(obj);
        }

        #endregion
    }
}