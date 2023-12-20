using System;
using System.Security.AccessControl;
using System.Text;

namespace CMS.IO
{
    /// <summary>
    /// Envelope for File classes
    /// </summary>
    public static class File
    {
        #region "Public methods"

        /// <summary>
        /// Gets the file provider object for given path
        /// </summary>
        /// <param name="path">Input path, output is path relative to the returned storage provider</param>
        public static AbstractFile GetFileObject(string path)
        {
            var provider = StorageHelper.GetStorageProvider(path);

            return provider.FileProviderObject;
        }


        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file</param>  
        public static bool ExistsRelative(string path)
        {
            path = StorageHelper.GetFullFilePhysicalPath(path);

            return Exists(path);
        }


        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param> 
        /// <returns>true if the caller has the required permissions and path contains the name of an existing file; otherwise, false. This method also returns false if path is null, an invalid path, or a zero-length string. 
        /// If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns false regardless of the existence of path.
        /// </returns>
        public static bool Exists(string path)
        {
            FileDebug.LogFileOperation(path, FileDebugOperation.EXISTS);

            try
            {
                var file = GetFileObject(path);
                return file.Exists(path);
            }
            catch (StorageProviderException)
            {
                throw;
            }
            catch
            {
                // Other exceptions are not propagated (similar behavior as native implementation of File.Exists method)
            }

            return false;
        }


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static StreamReader OpenText(string path)
        {
            var file = GetFileObject(path);
            return file.OpenText(path);
        }


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static void Delete(string path)
        {
            using (var h = IOEvents.DeleteFile.StartEvent(path))
            {
                var file = GetFileObject(path);
                file.Delete(path);

                FileDebug.LogFileOperation(path, FileDebugOperation.DELETE);

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file</param>
        /// <param name="destFileName">Path to destination file</param>
        /// <param name="overwrite">If destination file should be overwritten</param>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            var file = GetFileObject(sourceFileName);
            file.Copy(sourceFileName, destFileName, overwrite);

            FileDebug.LogFileOperation(sourceFileName + "|" + destFileName, FileDebugOperation.COPY);
        }


        /// <summary>
        /// Copies an existing file to a new file.
        /// </summary>
        /// <param name="sourceFileName">Path to source file</param>
        /// <param name="destFileName">Path to destination file</param>        
        public static void Copy(string sourceFileName, string destFileName)
        {
            var file = GetFileObject(sourceFileName);
            file.Copy(sourceFileName, destFileName);

            FileDebug.LogFileOperation(sourceFileName + "|" + destFileName, FileDebugOperation.COPY);
        }


        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static byte[] ReadAllBytes(string path)
        {
            var file = GetFileObject(path);

            byte[] retval = file.ReadAllBytes(path);

            FileDebug.LogFileOperation(path, FileDebugOperation.READ_ALL_BYTES, retval.Length);

            return retval;
        }


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public static FileStream Create(string path)
        {
            var file = GetFileObject(path);

            FileStream retval = file.Create(path);
            FileDebug.LogFileOperation(path, FileDebugOperation.CREATE);

            return retval;
        }


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name</param>
        /// <param name="destFileName">Destination file name</param>
        public static void Move(string sourceFileName, string destFileName)
        {
            var file = GetFileObject(sourceFileName);

            file.Move(sourceFileName, destFileName);

            FileDebug.LogFileOperation(sourceFileName + "|" + destFileName, FileDebugOperation.MOVE);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public static string ReadAllText(string path)
        {
            var file = GetFileObject(path);

            string retval = file.ReadAllText(path);
            FileDebug.LogFileOperation(path, FileDebugOperation.READ_ALL_TEXT, Encoding.Default.GetBytes(retval).Length);

            return retval;
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        public static string ReadAllText(string path, Encoding encoding)
        {
            var file = GetFileObject(path);

            string retval = file.ReadAllText(path, encoding);
            FileDebug.LogFileOperation(path, FileDebugOperation.READ_ALL_TEXT, encoding.GetBytes(retval).Length);

            return retval;
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        public static void WriteAllText(string path, string contents)
        {
            var file = GetFileObject(path);

            file.WriteAllText(path, contents);

            FileDebug.LogFileOperation(path, FileDebugOperation.WRITE_ALL_TEXT, Encoding.Default.GetBytes(contents).Length, contents);
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            var file = GetFileObject(path);

            file.WriteAllText(path, contents, encoding);

            FileDebug.LogFileOperation(path, FileDebugOperation.WRITE_ALL_TEXT, encoding.GetBytes(contents).Length, contents);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        public static void AppendAllText(string path, string contents)
        {
            AppendAllText(path, contents, true);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public static void AppendAllText(string path, string contents, Encoding encoding)
        {
            AppendAllText(path, contents, encoding, true);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        /// <param name="logOperation">If true file operation is logged to File log</param>
        public static void AppendAllText(string path, string contents, bool logOperation)
        {
            var file = GetFileObject(path);

            file.AppendAllText(path, contents);

            if (logOperation)
            {
                FileDebug.LogFileOperation(path, FileDebugOperation.APPEND_ALL_TEXT, Encoding.Default.GetBytes(contents).Length, contents);
            }
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        /// <param name="logOperation">If true file operation is logged to File log</param>
        public static void AppendAllText(string path, string contents, Encoding encoding, bool logOperation)
        {
            var file = GetFileObject(path);

            file.AppendAllText(path, contents, encoding);
            if (logOperation)
            {
                FileDebug.LogFileOperation(path, FileDebugOperation.APPEND_ALL_TEXT, encoding.GetBytes(contents).Length, contents);
            }
        }


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="bytes">Bytes to write</param>
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            var file = GetFileObject(path);

            file.WriteAllBytes(path, bytes);

            FileDebug.LogFileOperation(path, FileDebugOperation.WRITE_ALL_BYTES, bytes.Length);
        }


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static FileStream OpenRead(string path)
        {
            var file = GetFileObject(path);

            return file.OpenRead(path);
        }


        /// <summary>
        /// Sets the specified FileAttributes of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="fileAttributes">File attributes</param>
        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            var file = GetFileObject(path);

            file.SetAttributes(path, fileAttributes);
        }


        /// <summary>
        /// Opens a FileStream  on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        public static FileStream Open(string path, FileMode mode, FileAccess access)
        {
            var file = GetFileObject(path);

            FileDebug.LogFileOperation(path, FileDebugOperation.OPEN, -1, "", access.ToString());

            return file.Open(path, mode, access);
        }


        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="lastWriteTimeUtc">Specified time</param>
        public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            var file = GetFileObject(path);

            file.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        }


        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file</param>        
        public static StreamWriter CreateText(string path)
        {
            var file = GetFileObject(path);

            return file.CreateText(path);
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static FileSecurity GetAccessControl(string path)
        {
            var file = GetFileObject(path);

            return file.GetAccessControl(path);
        }


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static DateTime GetLastWriteTime(string path)
        {
            var file = GetFileObject(path);

            return file.GetLastWriteTime(path);
        }


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="lastWriteTime">Last write time</param>
        public static void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            var file = GetFileObject(path);

            file.SetLastWriteTime(path, lastWriteTime);
        }


        /// <summary>
        /// Returns URL to file. If can be accessed directly then direct URL is generated else URL with GetFile page is generated.
        /// </summary>
        /// <param name="path">Virtual path starting with ~ or absolute path.</param>
        /// <param name="siteName">Site name.</param>
        public static string GetFileUrl(string path, string siteName)
        {
            var file = GetFileObject(path);
            return file.GetFileUrl(path, siteName);
        }

        #endregion
    }
}