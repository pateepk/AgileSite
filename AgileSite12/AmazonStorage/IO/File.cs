using System;
using System.IO;
using System.Security.AccessControl;
using System.Text;

using CMS.Helpers;
using CMS.Globalization;
using CMS.IO;
using CMS.Base;

using FileAccess = CMS.IO.FileAccess;
using FileAttributes = CMS.IO.FileAttributes;
using FileMode = CMS.IO.FileMode;
using MemoryStream = System.IO.MemoryStream;
using Path = CMS.IO.Path;
using StreamReader = CMS.IO.StreamReader;
using StreamWriter = CMS.IO.StreamWriter;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Implementation of File class for Amazon S3.
    /// </summary>
    public class File : AbstractFile
    {
        #region "Variables"

        static IS3ObjectInfoProvider mProvider = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Get or set S3ObjectInfoProvider.
        /// </summary>
        public static IS3ObjectInfoProvider Provider
        {
            get
            {
                if (mProvider == null)
                {
                    mProvider = S3ObjectFactory.Provider;
                }

                return mProvider;
            }
            set
            {
                mProvider = value;
            }
        }       

        #endregion


        #region "Public methods - overrides"

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        public override bool Exists(string path)
        {
            return ExistsInFileSystem(path) || ExistsInS3Storage(path);
        }


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override StreamReader OpenText(string path)
        {
            // S3 storage
            if (ExistsInS3Storage(path))
            {
                // Get content from S3 storage
                IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);
                var stream = Provider.GetObjectContent(obj);
                if (stream != null)
                {
                    StreamReader reader = StreamReader.New(stream);
                    return reader;
                }

                return null;
            }

            // File system
            if (ExistsInFileSystem(path))
            {
                System.IO.StreamReader systemReader = System.IO.File.OpenText(path);
                StreamReader reader = StreamReader.New(systemReader);
                return reader;
            }

            throw GetFileNotFoundException(path);
        }


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override void Delete(string path)
        {
            if (!Exists(path))
            {
                throw GetFileNotFoundException(path);
            }

            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);

            if (Provider.ObjectExists(obj))
            {
                // Delete
                Provider.DeleteObject(obj);

                // Set last write time to directory
                string directory = Path.GetDirectoryName(path);

                IS3ObjectInfo dirObj = S3ObjectFactory.GetInfo(directory);
                dirObj.Key = dirObj.Key + "/";

                // In case that directory object doesn't exist create one
                if (!Provider.ObjectExists(dirObj))
                {
                    // Create
                    Provider.CreateEmptyObject(dirObj);
                }

                // Set last write time
                dirObj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(DateTime.Now));
            }

            // File exists only on file system - throw exception
            else
            {
                throw new InvalidOperationException("File '" + path + @"' cannot be deleted because it exists only in application file system. 
                    This exception typically occurs when file system is mapped to Amazon S3 storage after the file or directory
                    '" + path + "' was created in the local file system. To fix this issue remove specified file or directory.");
            }
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>        
        public override void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>
        /// <param name="overwrite">If destination file should be overwritten.</param>
        public override void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (!Exists(sourceFileName))
            {
                throw GetFileNotFoundException(sourceFileName);
            }

            // Check whether destination file exist
            var exists = IO.File.Exists(destFileName);
            if (exists && !overwrite)
            {
                return;
            }

            // If the destination storage provider is different, use helper method to copy the file
            if (!StorageHelper.IsSameStorageProvider(sourceFileName, destFileName))
            {
                StorageHelper.CopyFileAcrossProviders(sourceFileName, destFileName);
                return;
            }

            // Get objects                   
            IS3ObjectInfo sourceObj = S3ObjectFactory.GetInfo(sourceFileName);
            IS3ObjectInfo destObj = S3ObjectFactory.GetInfo(destFileName);

            // Delete old object
            if (exists)
            {
                Provider.DeleteObject(destObj);
            }

            // Copy in Amazon S3 storage
            if (Provider.ObjectExists(sourceObj))
            {
                // Copy
                Provider.CopyObjects(sourceObj, destObj);
            }
            // Upload from file system to Amazon S3
            else
            {
                Provider.PutFileToObject(destObj, sourceFileName);
            }

            // Set last write time to directory
            string directory = Path.GetDirectoryName(destFileName);

            // Create directory entry if not exist
            IS3ObjectInfo folderObj = S3ObjectFactory.GetInfo(directory);
            folderObj.Key = folderObj.Key + "/";
            if (!Provider.ObjectExists(folderObj))
            {
                Provider.CreateEmptyObject(folderObj);
            }

            // Set times
            DateTime now = DateTime.Now;
            folderObj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(now));

            destObj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(now), false);
            destObj.SetMetadata(S3ObjectInfoProvider.CREATION_TIME, S3ObjectInfoProvider.GetDateTimeString(now));
        }


        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override byte[] ReadAllBytes(string path)
        {
            if (!Exists(path))
            {
                throw GetFileNotFoundException(path);
            }

            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);
            if (Provider.ObjectExists(obj))
            {
                var stream = Provider.GetObjectContent(obj);
                byte[] buffer = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, ValidationHelper.GetInteger(stream.Length, 0));
                stream.Close();
                return buffer;
            }

            // File exists in file system
            return System.IO.File.ReadAllBytes(path);
        }


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        public override IO.FileStream Create(string path)
        {
            return GetFileStream(path, FileMode.Create);
        }


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destFileName">Destination file name.</param>
        public override void Move(string sourceFileName, string destFileName)
        {
            // If the destination storage provider is different, use helper method to copy the file
            if (!StorageHelper.IsSameStorageProvider(sourceFileName, destFileName))
            {
                StorageHelper.MoveFileAcrossProviders(sourceFileName, destFileName);
                return;
            }

            // Copy
            Copy(sourceFileName, destFileName, false);

            // Delete source                
            Delete(sourceFileName);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        public override string ReadAllText(string path)
        {
            if (!Exists(path))
            {
                throw GetFileNotFoundException(path);
            }

            // Get object content                    
            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);
            if (Provider.ObjectExists(obj))
            {
                var stream = Provider.GetObjectContent(obj);

                // Put content of stream to path
                using (StreamReader reader = StreamReader.New(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            return System.IO.File.ReadAllText(path);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        public override string ReadAllText(string path, Encoding encoding)
        {
            return ReadAllText(path);
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write.</param>
        public override void WriteAllText(string path, string contents)
        {
            // Check that path exists
            var directoryPath = Path.GetDirectoryName(path);
            if (!IO.Directory.Exists(directoryPath))
            {
                throw GetDirectoryNotFoundException(directoryPath);
            }

            // Init storage                
            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);

            // Put text to the object
            Provider.PutTextToObject(obj, contents);
            obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(DateTime.Now));
        }


       


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void WriteAllText(string path, string contents, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(contents);
            WriteAllBytes(path, bytes);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write.</param>
        public override void AppendAllText(string path, string contents)
        {
            // Check that path exist.           
            var directoryPath = Path.GetDirectoryName(path);
            if (!IO.Directory.Exists(directoryPath))
            {
                throw GetDirectoryNotFoundException(directoryPath);
            }

            // Init storage                
            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);
            
            // Put text to the object
            Provider.AppendTextToObject(obj, contents);
            obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(DateTime.Now));
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write.</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void AppendAllText(string path, string contents, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(contents);
            WriteAllBytes(path, bytes);
        }


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="bytes">Bytes to write.</param>
        public override void WriteAllBytes(string path, byte[] bytes)
        {
            // Check that path exist.           
            var directoryPath = Path.GetDirectoryName(path);
            if (!IO.Directory.Exists(directoryPath))
            {
                throw GetDirectoryNotFoundException(directoryPath);
            }

            MemoryStream memory = new MemoryStream(bytes);
            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);
            Provider.PutDataFromStreamToObject(obj, memory);

            obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(DateTime.Now));
        }


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override IO.FileStream OpenRead(string path)
        {
            if (!Exists(path))
            {
                throw GetFileNotFoundException(path);
            }

            return GetFileStream(path, FileMode.Open, FileAccess.Read);
        }


        /// <summary>
        /// Sets the specified FileAttributes  of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="fileAttributes">File attributes.</param>
        public override void SetAttributes(string path, FileAttributes fileAttributes)
        {
            if (!Exists(path))
            {
                throw GetFileNotFoundException(path);
            }

            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);

            // Set attributes to S3 storage
            if (Provider.ObjectExists(obj))
            {
                obj.SetMetadata(S3ObjectInfoProvider.ATTRIBUTES, ValidationHelper.GetString(ValidationHelper.GetInteger(fileAttributes, 0), string.Empty), false);
                obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(DateTime.Now));
            }
            else
            {
                throw new InvalidOperationException("Cannot set attributes to file '" + path + @"' because it exists only in application file system. 
                    This exception typically occurs when file system is mapped to Amazon S3 storage after the file or directory
                    '" + path + "' was created in the local file system. To fix this issue move given file to Amazon S3 storage.");
            }
        }


        /// <summary>
        /// Opens a FileStream  on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        public override IO.FileStream Open(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(path, mode, access);
        }


        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="lastWriteTimeUtc">Specified time.</param>
        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            SetLastWriteTime(path, TimeZoneHelper.ConvertTimeToUTC(lastWriteTimeUtc, TimeZoneHelper.ServerTimeZone));
        }


        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override StreamWriter CreateText(string path)
        {
            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);            

            // Create stream and stream writer objects
            IO.FileStream fs = GetFileStream(path, FileMode.Create);
            return StreamWriter.New(fs);
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override FileSecurity GetAccessControl(string path)
        {
            // In Amazon S3 has user full control over file system
            return new FileSecurity();
        }


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override DateTime GetLastWriteTime(string path)
        {
            if (!Exists(path))
            {
                throw GetFileNotFoundException(path);
            }

            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);

            // Set attributes to S3 storage
            if (Provider.ObjectExists(obj))
            {
                return S3ObjectInfoProvider.GetStringDateTime(obj.GetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME));
            }

            return System.IO.File.GetLastAccessTime(path);
        }


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="lastWriteTime">Last write time.</param>
        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            if (!Exists(path))
            {
                throw GetFileNotFoundException(path);
            }

            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);

            // Set attributes to S3 storage
            if (Provider.ObjectExists(obj))
            {
                obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(lastWriteTime));
            }
            else
            {
                throw new InvalidOperationException("Cannot last write time to file '" + path + @"' because is located only in application file system. 
                    This exception typically occurs when file system is mapped to Amazon S3 storage after the file or directory
                    '" + path + "' was created in the local file system. To fix this issue move given file to Amazon S3 storage.");
            }
        }


        /// <summary>
        /// Returns URL to file. If can be accessed directly then direct URL is generated else URL with GetFile page is generated.
        /// </summary>
        /// <param name="path">Virtual path starting with ~ or absolute path.</param>
        /// <param name="siteName">Site name.</param>
        public override string GetFileUrl(string path, string siteName)
        {            
            if (!ExistsInS3Storage(path))
            {
                return null;
            }

            string objKey = PathHelper.GetObjectKeyFromPath(path);
            if (AmazonHelper.PublicAccess)
            {
                // Remove starting "/"
                return String.Format("{0}/{1}", AmazonHelper.EndPoint, objKey).ToLowerCSafe();
            }

            string relative = "~\\" + Path.EnsureBackslashes(objKey);

            string url = AmazonHelper.GetDownloadPath(relative);
            string hash = GetHashString(URLHelper.GetQuery(url));
            url = URLHelper.AddParameterToUrl(url, "hash", hash);

            return ResolveUrl(url);
        }

        #endregion


        #region "Non public methods"

        private static FileNotFoundException GetFileNotFoundException(string path)
        {
            return new FileNotFoundException("Path " + path + " does not exist.");
        }


        private static DirectoryNotFoundException GetDirectoryNotFoundException(string directoryPath)
        {
            return new DirectoryNotFoundException("Directory '" + directoryPath + "' does not exist.");
        }


        /// <summary>
        /// Returns whether given file exist in file system.
        /// </summary>
        /// <param name="path">Path to given file.</param>
        protected virtual bool ExistsInFileSystem(string path)
        {
            return System.IO.File.Exists(path);
        }


        /// <summary>
        /// Returns whether given file exists in S3 storage.
        /// </summary>
        /// <param name="path">Path to file.</param>
        private bool ExistsInS3Storage(string path)
        {
            IS3ObjectInfo obj = S3ObjectFactory.GetInfo(path);
            return Provider.ObjectExists(obj);
        }


        /// <summary>
        /// Returns new instance of FileStream class. 
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="mode">File mode.</param>
        protected virtual IO.FileStream GetFileStream(string path, FileMode mode)
        {
            return new FileStream(path, mode);
        }


        /// <summary>
        /// Returns new instance of FileStream class. 
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        protected virtual IO.FileStream GetFileStream(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(path, mode, access);
        }


        /// <summary>
        /// Returns Hashed string from specified query string. Hashed string is not user specific.
        /// </summary>
        /// <param name="query">Query to hash.</param>
        protected virtual string GetHashString(string query)
        {
            return ValidationHelper.GetHashString(query, new HashSettings(""));
        }


        /// <summary>
        /// Returns resolved URL.
        /// </summary>
        /// <param name="url">URL to resolve.</param>
        protected virtual string ResolveUrl(string url)
        {
            return URLHelper.ResolveUrl(url);
        }

        #endregion
    }
}
