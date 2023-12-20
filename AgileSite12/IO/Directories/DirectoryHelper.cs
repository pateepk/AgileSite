using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Core;
using CMS.Base;
using CMS.Helpers;

using IOExceptions = System.IO;

namespace CMS.IO
{
    /// <summary>
    /// Directory management methods.
    /// </summary>
    public class DirectoryHelper : AbstractHelper<DirectoryHelper>
    {
        #region "Variables"

        /// <summary>
        /// Whether IO permissions are checked.
        /// </summary>
        private static bool? mAllowCheckIOPermissions = null;

        /// <summary>
        /// Whether web root is writable.
        /// </summary>
        private static bool? mIsWebRootWritable = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Allow checking IO permissions?
        /// </summary>
        public static bool AllowCheckIOPermissions
        {
            get
            {
                if (mAllowCheckIOPermissions == null)
                {
                    mAllowCheckIOPermissions = CoreServices.AppSettings["CMSAllowCheckIOPermissions"].ToBoolean(true);
                }

                return mAllowCheckIOPermissions.Value;
            }
            set
            {
                mAllowCheckIOPermissions = value;
            }
        }


        /// <summary>
        /// Returns whether web root is writable.
        /// </summary>
        public static bool IsWebRootWritable
        {
            get
            {
                if (mIsWebRootWritable == null)
                {
                    try
                    {
                        var filePath = Path.Combine(SystemContext.WebApplicationPhysicalPath, Guid.NewGuid().ToString("N"));
                        using (var stream = FileStream.New(filePath, FileMode.Create, FileAccess.Write))
                        {
                            stream.WriteByte(0);
                        }
                        File.Delete(filePath);
                        mIsWebRootWritable = true;
                    }
                    catch
                    {
                        mIsWebRootWritable = false;
                    }
                }

                return mIsWebRootWritable.Value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates directory. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the new directory</param>
        /// <returns>Returns DirectoryInfo representing the new directory.</returns>
        public static DirectoryInfo CreateDirectory(string path)
        {
            return HelperObject.CreateDirectoryInternal(path);
        }


        /// <summary>
        /// Deletes the directory. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the directory to delete</param>
        public static void DeleteDirectory(string path)
        {
            HelperObject.DeleteDirectoryInternal(path);
        }


        /// <summary>
        /// Deletes the directory. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the directory to delete</param>
        /// <param name="recursive">If true, the directory is deleted recursively</param>
        public static void DeleteDirectory(string path, bool recursive)
        {
            HelperObject.DeleteDirectoryInternal(path, recursive);
        }


        /// <summary>
        /// Moves specified directory to new path.
        /// </summary>
        /// <param name="sourcePath">Full disk path of the source folder</param>
        /// <param name="targetPath">New full disk path of the moved folder including its name</param>
        public static void MoveDirectory(string sourcePath, string targetPath)
        {
            HelperObject.MoveDirectoryInternal(sourcePath, targetPath);
        }


        /// <summary>
        /// Copies specified directory including its subdirectories and all underlying files.
        /// </summary>
        /// <param name="sourcePath">Full disk path of the source directory</param>
        /// <param name="targetPath">Full disk path of the new copy of the directory including its name</param>
        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            HelperObject.CopyDirectoryInternal(sourcePath, targetPath);
        }


        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target directory</param>
        public static void CopyFile(string sourcePath, string targetPath)
        {
            HelperObject.CopyFileInternal(sourcePath, targetPath);
        }


        /// <summary>
        /// Deletes the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the directory to delete</param>
        public static void DeleteDirectoryStructure(string path)
        {
            HelperObject.DeleteDirectoryStructureInternal(path);
        }


        /// <summary>
        /// Checks if all folders of the given path exist and if not, it creates them.
        /// </summary>
        /// <param name="path">Full disk path of the file including file name</param>
        /// <param name="startingPath">Starting path (subset of file path) that will not be checked</param>
        public static void EnsureDiskPath(string path, string startingPath)
        {
            HelperObject.EnsureDiskPathInternal(path, startingPath);
        }


        /// <summary>
        /// Check permissions (checks read and write permissions).
        /// </summary>
        /// <param name="path">Directory path</param>
        public static bool CheckPermissions(string path)
        {
            return HelperObject.CheckPermissionsInternal(path, true, true, false, false);
        }


        /// <summary>
        /// Check write permissions.
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="checkRead">If read permissions should be checked</param>
        /// <param name="checkWrite">If write permissions should be checked</param>
        /// <param name="checkModify">If modify permissions should be checked</param>
        /// <param name="checkDelete">If delete permissions should be checked</param>
        public static bool CheckPermissions(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            return HelperObject.CheckPermissionsInternal(path, checkRead, checkWrite, checkModify, checkDelete);
        }


        /// <summary>
        /// Combines parts of one path together, handles back slashes. Path ends without backslash.
        /// </summary>
        /// <param name="paths">Parts to combine</param>
        public static string CombinePath(params string[] paths)
        {
            return HelperObject.CombinePathInternal(paths);
        }


        /// <summary>
        /// This method ensures that path will end with one backslash.
        /// </summary>
        /// <param name="path">Path to ensure</param>
        public static string EnsurePathBackSlash(string path)
        {
            return HelperObject.EnsurePathBackSlashInternal(path);
        }

        /// <summary>
        /// Returns search condition delegate.
        /// </summary>
        /// <param name="searchPattern">Can be a combination of literal and wildcard characters, but doesn't support regular expressions. Supports only <c>*</c> and <c>?</c></param>
        internal static Func<string, bool> GetSearchCondition(string searchPattern)
        {
            return HelperObject.GetSearchConditionInternal(searchPattern);
        }


        /// <summary>
        /// Test if the right exists within the given rights.
        /// </summary>
        /// <param name="right">Right to check</param>
        /// <param name="rule">File access rule</param>
        private static bool Contains(FileSystemRights right, FileSystemAccessRule rule)
        {
            return (((int)right & (int)rule.FileSystemRights) == (int)right);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Creates directory. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the new directory</param>
        /// <returns>Returns DirectoryInfo representing the new directory.</returns>
        protected virtual DirectoryInfo CreateDirectoryInternal(string path)
        {
            return Directory.CreateDirectory(path);
        }


        /// <summary>
        /// Deletes the directory. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the directory to delete</param>
        protected virtual void DeleteDirectoryInternal(string path)
        {
            Directory.Delete(path);
        }


        /// <summary>
        /// Deletes the directory. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the directory to delete</param>
        /// <param name="recursive">If true, the directory is deleted recursively</param>
        protected virtual void DeleteDirectoryInternal(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }


        /// <summary>
        /// Moves specified directory to new path.
        /// </summary>
        /// <param name="sourcePath">Full disk path of the source folder</param>
        /// <param name="targetPath">New full disk path of the moved folder including its name</param>
        protected virtual void MoveDirectoryInternal(string sourcePath, string targetPath)
        {
            Directory.Move(sourcePath, targetPath);
        }


        /// <summary>
        /// Copies specified directory including its subdirectories and all underlying files.
        /// </summary>
        /// <param name="sourcePath">Full disk path of the source directory</param>
        /// <param name="targetPath">Full disk path of the new copy of the directory including its name</param>
        protected virtual void CopyDirectoryInternal(string sourcePath, string targetPath)
        {
            DirectoryInfo sourceFolder = DirectoryInfo.New(sourcePath);

            // Create the directory if not exists
            if (!Directory.Exists(targetPath))
            {
                CreateDirectoryInternal(targetPath);
            }

            // Copy subfolders
            foreach (DirectoryInfo subFolder in sourceFolder.GetDirectories())
            {
                CopyDirectoryInternal(EnsurePathBackSlash(sourcePath) + subFolder.Name, EnsurePathBackSlash(targetPath) + subFolder.Name);
            }

            // Copy files
            foreach (FileInfo sourceFile in sourceFolder.GetFiles())
            {
                CopyFileInternal(sourceFile.FullName, CombinePath(targetPath, sourceFile.Name));
            }
        }


        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target directory</param>
        protected virtual void CopyFileInternal(string sourcePath, string targetPath)
        {
            // Copy the file
            FileInfo sourceFile = FileInfo.New(sourcePath);
            sourceFile.CopyTo(targetPath, true);

            // Set attributes to normal
            File.SetAttributes(targetPath, FileAttributes.Normal);
            FileInfo f = FileInfo.New(targetPath);
            f.IsReadOnly = false;
            f.Attributes = FileAttributes.Normal;
        }


        /// <summary>
        /// Deletes the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full disk path of the directory to delete</param>
        protected virtual void DeleteDirectoryStructureInternal(string path)
        {
            Directory.DeleteDirectoryStructure(path);
        }


        /// <summary>
        /// Checks if all folders of the given path exist and if not, it creates them.
        /// </summary>
        /// <param name="path">Full disk path of the file including file name</param>
        /// <param name="startingPath">Starting path (subset of file path) that will not be checked</param>
        protected virtual void EnsureDiskPathInternal(string path, string startingPath)
        {
            string folderPath = null;
            int folderIndex = 0;
            string currentPath = null;
            string[] pathArray = null;
            int startingIndex = 0;

            // Prepare the starting path
            if (startingPath == null)
            {
                startingPath = "";
            }
            if (startingPath.EndsWith("\\", StringComparison.Ordinal))
            {
                startingPath = startingPath.Substring(0, startingPath.Length - 1);
            }
            // If path outside of the application folder, ignore the starting path
            if (!path.StartsWith(startingPath, StringComparison.OrdinalIgnoreCase))
            {
                startingPath = "";
            }

            bool networkDirectory = path.StartsWith("\\\\", StringComparison.Ordinal);

            // Remove file name from the path
            folderPath = path.Substring(0, path.LastIndexOf(@"\", StringComparison.Ordinal));
            pathArray = folderPath.Split('\\');
            currentPath = pathArray[0];

            // If starting path available, get starting index
            if ((startingPath != "") && folderPath.Trim().StartsWith(startingPath.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                startingIndex = startingPath.Split('\\').GetUpperBound(0);
            }

            for (folderIndex = 1; folderIndex <= pathArray.GetUpperBound(0); folderIndex++)
            {
                currentPath += @"\" + pathArray[folderIndex];
                if ((startingIndex < folderIndex) && (!networkDirectory || (folderIndex > 2)))
                {
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
                }
            }
        }


        /// <summary>
        /// Check write permissions.
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="checkRead">If read permissions should be checked</param>
        /// <param name="checkWrite">If write permissions should be checked</param>
        /// <param name="checkModify">If modify permissions should be checked</param>
        /// <param name="checkDelete">If delete permissions should be checked</param>
        protected virtual bool CheckPermissionsInternal(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            // If checking not allowed, always pass
            if (!AllowCheckIOPermissions)
            {
                return true;
            }

            // If the given path is handled by external storage, always pass
            if (StorageHelper.IsExternalStorage(path))
            {
                return true;
            }

            if (CMSHttpContext.Current != null)
            {
                // Try convert path to the full path
                try
                {
                    if (!path.StartsWith("\\\\", StringComparison.Ordinal))
                    {
                        path = CMSHttpContext.Current.Server.MapPath(path);
                    }
                }
                catch
                {
                    // Path is already full path
                }
            }

            // Get closest existing parent
            while (!Directory.Exists(path))
            {
                string parentPath = Path.GetDirectoryName(path);
                if (parentPath == path)
                {
                    break;
                }
                else
                {
                    path = parentPath;
                }
            }

            // Check the permissions
            bool denyRead = false;
            bool denyWrite = false;
            bool denyModify = false;
            bool denyDelete = false;
            bool allowRead = false;
            bool allowWrite = false;
            bool allowModify = false;
            bool allowDelete = false;

            WindowsIdentity principal = WindowsIdentity.GetCurrent();

            bool? result = null;
            AuthorizationRuleCollection acl = null;
            try
            {
                acl = Directory.GetAccessControl(path).GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            catch
            {
                // If the access control is not available (e.g., in medium trust environments), check whether the application root folder is writable.
                if (IsWebRootWritable)
                {
                    result = true;
                }
                else
                {
                    result = false;
                    CoreServices.EventLog.LogEvent("E", "Security", "Exception", "Unable to get the file system access rules due to insufficient user permissions. Grant user the permissions or disable the file system security check by setting the 'CMSAllowCheckIOPermissions' key in web.config file to false.");
                }
            }
            // For some mysterious reasons when there were two return statements in the catch block, the last one was always executed.
            if (result.HasValue)
            {
                return result.Value;
            }

            try
            {
                foreach (FileSystemAccessRule rule in acl)
                {
                    // If the rule is for current user
                    if (principal.User.Equals(rule.IdentityReference))
                    {
                        // First check deny rules
                        if (AccessControlType.Deny.Equals(rule.AccessControlType))
                        {
                            if (Contains(FileSystemRights.Delete, rule))
                            {
                                denyDelete = true;
                            }
                            if (Contains(FileSystemRights.Modify, rule))
                            {
                                denyModify = true;
                            }
                            if (Contains(FileSystemRights.Read, rule))
                            {
                                denyRead = true;
                            }
                            if (Contains(FileSystemRights.Write, rule))
                            {
                                denyWrite = true;
                            }
                        }
                        // Check allow rules
                        else if (AccessControlType.Allow.Equals(rule.AccessControlType))
                        {
                            if (Contains(FileSystemRights.Delete, rule))
                            {
                                allowDelete = true;
                            }
                            if (Contains(FileSystemRights.Modify, rule))
                            {
                                allowModify = true;
                            }
                            if (Contains(FileSystemRights.Read, rule))
                            {
                                allowRead = true;
                            }
                            if (Contains(FileSystemRights.Write, rule))
                            {
                                allowWrite = true;
                            }
                        }
                    }
                }

                // Check groups
                IdentityReferenceCollection groups = principal.Groups;
                foreach (IdentityReference group in groups)
                {
                    foreach (FileSystemAccessRule rule in acl)
                    {
                        if (group.Equals(rule.IdentityReference))
                        {
                            // Check deny rules
                            if (AccessControlType.Deny.Equals(rule.AccessControlType))
                            {
                                if (Contains(FileSystemRights.Delete, rule))
                                {
                                    denyDelete = true;
                                }
                                if (Contains(FileSystemRights.Modify, rule))
                                {
                                    denyModify = true;
                                }
                                if (Contains(FileSystemRights.Read, rule))
                                {
                                    denyRead = true;
                                }
                                if (Contains(FileSystemRights.Write, rule))
                                {
                                    denyWrite = true;
                                }
                            }
                            // Check deny rules
                            else if (AccessControlType.Allow.Equals(rule.AccessControlType))
                            {
                                if (Contains(FileSystemRights.Delete, rule))
                                {
                                    allowDelete = true;
                                }
                                if (Contains(FileSystemRights.Modify, rule))
                                {
                                    allowModify = true;
                                }
                                if (Contains(FileSystemRights.Read, rule))
                                {
                                    allowRead = true;
                                }
                                if (Contains(FileSystemRights.Write, rule))
                                {
                                    allowWrite = true;
                                }
                            }
                        }
                    }
                }

                // Particular permissions
                bool canDelete = !denyDelete && allowDelete;
                bool canModify = !denyModify && allowModify;
                bool canRead = !denyRead && allowRead;
                bool canWrite = !denyWrite && allowWrite;

                // Global permission
                bool globalPermission = true;

                if (checkRead)
                {
                    globalPermission = globalPermission && canRead;
                }

                if (checkWrite)
                {
                    globalPermission = globalPermission && canWrite;
                }

                if (checkModify)
                {
                    globalPermission = globalPermission && canModify;
                }

                if (checkDelete)
                {
                    globalPermission = globalPermission && canDelete;
                }

                return globalPermission;
            }
            catch (IOExceptions.IOException)
            {
            }
            catch (Exception ex)
            {
                throw new Exception("[DirectoryHelper.CheckPermissions]: " + ex.Message);
            }

            return false;
        }


        /// <summary>
        /// Combines parts of one path together, handles back slashes.
        /// </summary>
        /// <param name="paths">Parts to combine</param>
        protected virtual string CombinePathInternal(params string[] paths)
        {
            if ((paths != null) && (paths.Length > 0))
            {
                StringBuilder sb = new StringBuilder();
                foreach (string path in paths)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        sb.Append(path.TrimEnd('\\'));
                        sb.Append("\\");
                    }
                }

                // Remove last backslash
                return sb.ToString().TrimEnd('\\');
            }

            return null;
        }


        /// <summary>
        /// This method ensures that path will end with one backslash.
        /// </summary>
        /// <param name="path">Path to ensure</param>
        protected virtual string EnsurePathBackSlashInternal(string path)
        {
            if (path == null)
            {
                return null;
            }

            return path.TrimEnd('\\') + "\\";
        }


        /// <summary>
        /// Returns search condition delegate.
        /// </summary>
        /// <param name="searchPattern">Can be a combination of literal and wildcard characters, but doesn't support regular expressions. Supports only <c>*</c> and <c>?</c></param>
        internal virtual Func<string, bool> GetSearchConditionInternal(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }

            if (searchPattern.Equals(".", StringComparison.Ordinal) || searchPattern.Equals("*", StringComparison.Ordinal))
            {
                return (s) => true;
            }

            searchPattern = searchPattern.Trim();

            CheckSearchPattern(searchPattern);

            string pattern = Regex
                .Escape(searchPattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".?");
            pattern = "^" + pattern + "$";
            
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).IsMatch;
        }


        /// <summary>
        /// Checks whether pattern search contains invalid "..". Similar behavior as default .NET implementation.
        /// </summary>
        /// <remarks>
        /// ".." can only be used if it is specified as a part of a valid File/Directory name. It is not allowed to move up to directories.
        /// </remarks>
        private static void CheckSearchPattern(string searchPattern)
        {
            int index = 0;
            while ((index = searchPattern.IndexOf("..", index,  StringComparison.Ordinal)) != -1)
            {
                if (index + 2 == searchPattern.Length)
                {
                    throw new ArgumentException("Files names cannot end in '..'.");
                }

                if ((searchPattern[index + 2] == System.IO.Path.DirectorySeparatorChar) || 
                    (searchPattern[index + 2] == System.IO.Path.AltDirectorySeparatorChar))
                {
                    throw new ArgumentException("Invalid pattern search.");
                }

                index += 2;
            }
        }

        #endregion
    }
}