using System;
using System.Collections.Generic;

using CMS.Base;

using MappedPath = System.Collections.Generic.KeyValuePair<string, string>;

namespace CMS.IO
{
    /// <summary>
    /// Performs operations on String  instances that contain file or directory path information.
    /// </summary>
    public static class Path
    {
        #region "Variables"

        /// <summary>
        /// Valid slashes
        /// </summary>
        private static char[] mSlashes = { '\\', '/' };

        /// <summary>
        /// List of registered mapped folders
        /// </summary>
        private static List<MappedPath> mMappedPaths;

        /// <summary>
        /// Locking object
        /// </summary>
        private static object lockObject = new Object();

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns the physical file path that corresponds to the specified virtual path on the web server.
        /// </summary>
        /// <param name="path">File virtual path</param>
        private static string GetPhysicalPath(string path)
        {
            CheckPath(path);

            return SystemContext.WebApplicationPhysicalPath + EnsureBackslashes(path.Substring(1));
        }
         

        /// <summary>
        /// Checks the given path whether it is supported by the mapping
        /// </summary>
        /// <param name="path">Path to check</param>
        private static void CheckPath(string path)
        {
            if (String.IsNullOrEmpty(path) || !path.StartsWith("~/", StringComparison.Ordinal))
            {
                throw new NotSupportedException("[Path.CheckPath]: Only paths starting with '~/' are supported by mapping. Path given: " + path);
            }
        }


        /// <summary>
        /// Finds the existing folder based on the given relative path
        /// </summary>
        /// <param name="path">Relative path to folder</param>  
        /// <param name="tryMappedPath">If true, the check tries also the mapped path</param>
        public static bool FindExistingDirectory(ref string path, bool tryMappedPath)
        {
            // Try the original path
            string physicalPath = GetPhysicalPath(path);
            if (Directory.Exists(physicalPath))
            {
                return true;
            }

            // Try to map the path
            if (tryMappedPath)
            {
                string mappedPath = path;

                if (GetMappedPath(ref mappedPath))
                {
                    // Check if mapped directory exists
                    string mappedPhysicalPath = GetPhysicalPath(mappedPath);
                    if (Directory.Exists(mappedPhysicalPath))
                    {
                        path = mappedPath;

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Finds the existing file based on the given relative path
        /// </summary>
        /// <param name="path">Relative path to file</param>  
        /// <param name="tryMappedPath">If true, the check tries also the mapped path</param>
        public static bool FindExistingFile(ref string path, bool tryMappedPath)
        {
            // Try the original path
            string physicalPath = GetPhysicalPath(path);
            if (File.Exists(physicalPath))
            {
                return true;
            }

            if (tryMappedPath)
            {
                // Try to map the path
                string mappedPath = path;

                if (GetMappedPath(ref mappedPath))
                {
                    // Check if mapped file exists
                    string mappedPhysicalPath = GetPhysicalPath(mappedPath);
                    if (File.Exists(mappedPhysicalPath))
                    {
                        path = mappedPath;

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Registers folder which is mapped to a zip file
        /// </summary>
        /// <param name="path">Folder path to map to a zip file</param>
        /// <param name="zipFileName">Zip file name stored in the folder containing the folder contents</param>
        public static void RegisterMappedZippedFolder(string path, string zipFileName)
        {
            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            // Register mapped paths
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path += "/";
            }

            // Check if the zip file exists
            string zipPath = path + zipFileName;

            if (File.ExistsRelative(zipPath))
            {
                // Map the path
                zipPath = path + ZipStorageProvider.GetZipFileName(zipFileName) + "/";

                RegisterMappedPath(path, zipPath);
            }
        }


        /// <summary>
        /// Maps the folder to another folder
        /// </summary>
        /// <param name="path">Mapped folder</param>
        /// <param name="targetPath">Target folder</param>
        public static void RegisterMappedPath(string path, string targetPath)
        {
            CheckPath(path);
            CheckPath(targetPath);

            // Ensure mapped folders
            if (mMappedPaths == null)
            {
                lock (lockObject)
                {
                    if (mMappedPaths == null)
                    {
                        mMappedPaths = new List<MappedPath>();
                    }
                }
            }

            // Register the mapped path
            mMappedPaths.Add(new MappedPath(path, targetPath));
        }


        /// <summary>
        /// Returns true, if the given path is a mapped path
        /// </summary>
        /// <param name="path">Path to check</param>
        public static bool IsPathMapped(string path)
        {
            return GetMappedPath(ref path);
        }


        /// <summary>
        /// Gets the mapped path for the given path. Returns true if the path was mapped
        /// </summary>
        /// <param name="path">Path to convert</param>
        public static bool GetMappedPath(ref string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            if (mMappedPaths == null)
            {
                return false;
            }

            foreach (var map in mMappedPaths)
            {
                // If path maps correctly, translate the path
                if (path.StartsWith(map.Key, StringComparison.OrdinalIgnoreCase))
                {
                    path = map.Value + path.Substring(map.Key.Length);

                    return true;
                }
            }

            // No mapping matched
            return false;
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Gets a value indicating whether the specified path string contains absolute or relative path information.
        /// </summary>
        /// <param name="path">Path</param>
        public static bool IsPathRooted(string path)
        {
            return System.IO.Path.IsPathRooted(path);
        }


        /// <summary>
        /// Returns the extension of the specified path string.
        /// </summary>
        /// <param name="path">Path</param> 
        public static string GetExtension(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            int slashIndex = path.LastIndexOfAny(mSlashes);
            int dotIndex = path.LastIndexOf('.');

            if ((dotIndex < 0) || (slashIndex > dotIndex))
            {
                return String.Empty;
            }

            return path.Substring(dotIndex);
        }


        /// <summary>
        /// Gets the file name from the given path
        /// </summary>
        /// <param name="path">Input path</param>
        public static string GetFileName(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            // Get name
            int slashIndex = path.LastIndexOfAny(mSlashes);
            if (slashIndex >= 0)
            {
                path = path.Substring(slashIndex + 1);
            }

            return path;
        }


        /// <summary>
        /// Gets the directory name from the given path
        /// </summary>
        /// <param name="path">Input path</param>
        public static string GetDirectoryName(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            // Get name
            int slashIndex = path.LastIndexOfAny(mSlashes);
            if (slashIndex >= 0)
            {
                path = path.Substring(0, slashIndex);
            }
            else
            {
                path = "";
            }

            return path;
        }


        /// <summary>
        /// Combines two strings into a path.
        /// </summary>
        /// <param name="path1">First path</param>
        /// <param name="path2">Second path</param>
        public static string Combine(string path1, string path2)
        {
            return System.IO.Path.Combine(path1, path2);
        }


        /// <summary>
        /// Combines an array of strings into a path.
        /// </summary>
        /// <param name="paths">An array of parts of the path.</param>
        /// <exception cref="T:System.ArgumentException">One of the strings in the array contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars"/>. </exception>
        /// <exception cref="T:System.ArgumentNullException">One of the strings in the array is null. </exception>
        public static string Combine(params string[] paths)
        {
            return System.IO.Path.Combine(paths);
        }


        /// <summary>
        /// Returns the file name of the specified path string without the extension.
        /// </summary>
        /// <param name="path">Path</param> 
        public static string GetFileNameWithoutExtension(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }


        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">Path</param>
        public static string GetFullPath(string path)
        {
            return System.IO.Path.GetFullPath(path);
        }


        /// <summary>
        /// Determines whether a path includes a file name extension.
        /// </summary>
        /// <param name="path">Path</param>        
        public static bool HasExtension(string path)
        {
            return System.IO.Path.HasExtension(path);
        }

        #endregion

        /// <summary>
        /// Ensures that path contains backslashes '\'
        /// </summary>
        /// <param name="path">Path to convert</param>
        /// <param name="trimEnd">If true, the end of path is trimmed for backslash</param>
        public static string EnsureBackslashes(string path, bool trimEnd = false)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            path = path.Replace('/', '\\');
            if (trimEnd)
            {
                path = path.TrimEnd('\\');
            }

            return path;
        }


        /// <summary>
        /// Ensures that path ends with backslash '\'
        /// </summary>
        /// <param name="path">Path to convert</param>
        public static string EnsureEndBackslash(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            return path.TrimEnd('\\') + '\\';
        }
        

        /// <summary>
        /// Ensures that path contains slashes '/'
        /// </summary>
        /// <param name="path">Path to convert</param>
        /// <param name="trimEnd">If true, the end of path is trimmed for slash</param>
        public static string EnsureSlashes(string path, bool trimEnd = false)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            path = path.Replace('\\', '/');
            if (trimEnd)
            {
                path = path.TrimEnd('/');
            }

            return path;
        }
    }
}