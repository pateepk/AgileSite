using System;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

using CMS.Helpers;
using CMS.Globalization;
using CMS.IO;
using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Implementation of File class for Azure.
    /// </summary>
    public class File : AbstractFile
    {
        private const int FILE_NAME_MAX_LENGTH = 259;

        private readonly IDateTimeNowService mDateTimeNowService;


        /// <summary>
        /// Initializes a new instance of the <see cref="File"/> class, which provides operations with files.
        /// </summary>
        public File() : this(Service.Resolve<IDateTimeNowService>()) { }


        /// <summary>
        /// Internal constructor for testing purposes.
        /// </summary>
        internal File(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
        }


        #region "Public methods"

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file.</param>  
        public override bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // Check if exists in application directory
            if (System.IO.File.Exists(path))
            {
                return true;
            }

            try
            {
                path = Directory.GetValidPath(path);
                BlobInfo blob = new BlobInfo(Directory.GetBlobPathFromPath(path));
                return BlobInfoProvider.BlobExists(blob);
            }
            // According to MSDN, the File.Exists method is not supposed to generate exceptions for any input
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path"/> does not exist</exception>
        public override StreamReader OpenText(string path)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);
            ThrowOnNonexistentFile(path);

            path = Directory.GetValidPath(path);

            // Init blob                
            BlobInfo blobInfo = new BlobInfo(path);

            if (!BlobInfoProvider.BlobExists(blobInfo))
            {
                System.IO.StreamReader systemReader = System.IO.File.OpenText(path);
                StreamReader reader = StreamReader.New(systemReader);
                return reader;
            }

            // Get content from blob storage
            var stream = BlobInfoProvider.GetBlobContent(blobInfo);
            if (stream != null)
            {
                StreamReader reader = StreamReader.New(stream);
                return reader;
            }

            return null;
        }


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        public override void Delete(string path)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);

            if (!Exists(path))
            {
                return;
            }

            BlobInfo blobInfo = new BlobInfo(path);

            if (!BlobInfoProvider.BlobExists(blobInfo))
            {
                System.IO.File.Delete(path);
                return;
            }

            // Delete
            BlobInfoProvider.DeleteBlob(blobInfo);

            // Set parent directory's write time
            string directory = Path.GetDirectoryName(path);
            if (IsPathDirectoryPlaceholder(path))
            {
                // Path leads to directory, access its parent
                directory = Path.GetDirectoryName(directory);
            }

            var dirBlobPath = Path.Combine(directory, BlobInfoProvider.DIRECTORY_BLOB);
            BlobInfo dirBlob = new BlobInfo(dirBlobPath);

            if (BlobInfoProvider.BlobExists(dirBlob))
            {
                SetBlobLastWriteTimeToNow(dirBlob);
                return;
            }

            // In case that directory blob of parent directory doesn't exist create one
            Create(dirBlobPath);
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>
        /// <param name="overwrite">If destination file should be overwritten.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is too long</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="sourceFileName"/> does not exist</exception>
        /// <exception cref="System.IO.IOException"><paramref name="destFileName"/> already exists and <paramref name="overwrite"/> is set to false</exception>
        public override void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            ThrowOnNullPath(sourceFileName);
            ThrowOnInvalidPath(sourceFileName);
            ThrowOnTooLongFilePath(sourceFileName);
            ThrowOnNonexistentFile(sourceFileName);

            ThrowOnNullPath(destFileName);
            ThrowOnInvalidPath(destFileName);
            ThrowOnTooLongFilePath(destFileName);
        
            var exists = IO.File.Exists(destFileName);
            if (exists && !overwrite)
            {
                throw new System.IO.IOException($"Destination path {destFileName} already exists.");
            }

            if (!StorageHelper.IsSameStorageProvider(sourceFileName, destFileName))
            {
                StorageHelper.CopyFileAcrossProviders(sourceFileName, destFileName);
                return;
            }

            ContainerInfo srcContainer = ContainerInfoProvider.GetRootContainerInfo(sourceFileName);
            ContainerInfo dstContainer = ContainerInfoProvider.GetRootContainerInfo(destFileName);

            BlobInfo sourceBlob = new BlobInfo(srcContainer, sourceFileName);
            BlobInfo destBlob = new BlobInfo(dstContainer, destFileName);

            if (BlobInfoProvider.BlobExists(destBlob))
            {
                BlobInfoProvider.DeleteBlob(destBlob);
            }

            if (BlobInfoProvider.BlobExists(sourceBlob))
            {
                BlobInfoProvider.CopyBlobs(sourceBlob, destBlob);
            }
            else
            {
                // Put file system file to blob storage
                BlobInfoProvider.PutFileToBlob(destBlob, sourceFileName);
            }

            string directory = Path.GetDirectoryName(destFileName);

            BlobInfo folderBlob = new BlobInfo(dstContainer, Path.Combine(directory, BlobInfoProvider.DIRECTORY_BLOB));
            if (!BlobInfoProvider.BlobExists(folderBlob))
            {
                BlobInfoProvider.CreateEmptyBlob(folderBlob);
            }

            var now = mDateTimeNowService.GetDateTimeNow();
            SetBlobLastWriteTimeToNow(folderBlob, now);

            SetBlobLastWriteTimeToNow(destBlob, now);
            SetBlobCreationTimeToNow(destBlob, now);
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is too long</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="sourceFileName"/> does not exist</exception>
        /// <exception cref="System.IO.IOException"><paramref name="destFileName"/> already exists</exception>        
        public override void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }


        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override byte[] ReadAllBytes(string path)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);
            ThrowOnNonexistentFile(path);

            BlobInfo blobInfo = new BlobInfo(path);
            if (!BlobInfoProvider.BlobExists(blobInfo))
            {
                return System.IO.File.ReadAllBytes(path);
            }

            var stream = BlobInfoProvider.GetBlobContent(blobInfo);
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.Read(buffer, 0, ValidationHelper.GetInteger(stream.Length, 0));
            stream.Close();
            return buffer;
        }


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        public override IO.FileStream Create(string path)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);

            if (!IsPathDirectoryPlaceholder(path))
            {
                FileStream stream = new FileStream(path, FileMode.Create, mDateTimeNowService);
                return stream;
            }

            BlobInfo blobInfo = new BlobInfo(path);
            BlobInfoProvider.CreateEmptyBlob(blobInfo);

            blobInfo.Blob = null;
            BlobInfoProvider.InitBlobInfo(blobInfo);

            var now = mDateTimeNowService.GetDateTimeNow();

            SetBlobCreationTimeToNow(blobInfo, now);
            SetBlobLastWriteTimeToNow(blobInfo, now);

            return null;
        }


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destFileName">Destination file name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is too long</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="sourceFileName"/> does not exist</exception>
        /// <exception cref="System.IO.IOException">Both <paramref name="sourceFileName"/> and <paramref name="destFileName"/> lead to the same file</exception>
        public override void Move(string sourceFileName, string destFileName)
        {
            ThrowOnNullPath(sourceFileName);
            ThrowOnInvalidPath(sourceFileName);
            ThrowOnTooLongFilePath(sourceFileName);
            ThrowOnNonexistentFile(sourceFileName);

            ThrowOnNullPath(destFileName);
            ThrowOnInvalidPath(destFileName);
            ThrowOnTooLongFilePath(destFileName);

            if (Directory.GetValidPath(sourceFileName).Equals(Directory.GetValidPath(destFileName), StringComparison.OrdinalIgnoreCase))
            {
                throw new System.IO.IOException("Cannot move file to the same location.");
            }

            // If the destination storage provider is different, use helper method to copy the file
            if (!StorageHelper.IsSameStorageProvider(sourceFileName, destFileName))
            {
                StorageHelper.MoveFileAcrossProviders(sourceFileName, destFileName);
                return;
            }

            Copy(sourceFileName, destFileName, false);
            Delete(sourceFileName);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path"/> does not exist</exception>
        public override string ReadAllText(string path)
        {
            return ReadAllText(path, null);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path"/> does not exist</exception>
        public override string ReadAllText(string path, Encoding encoding)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);
            ThrowOnNonexistentFile(path);

            BlobInfo blobInfo = new BlobInfo(path);

            if (!BlobInfoProvider.BlobExists(blobInfo))
            {
                return System.IO.File.ReadAllText(path);
            }

            var stream = BlobInfoProvider.GetBlobContent(blobInfo);

            if (encoding == null)
            {
                using (StreamReader reader = StreamReader.New(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            using (StreamReader reader = StreamReader.New(stream, encoding))
            {
                return reader.ReadToEnd();
            }
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        public override void WriteAllText(string path, string contents)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);

            BlobInfo blobInfo = new BlobInfo(path);

            BlobInfoProvider.PutTextToBlob(blobInfo, contents);

            SetBlobLastWriteTimeToNow(blobInfo);
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        public override void AppendAllText(string path, string contents)
        {
            AppendAllText(path, contents, null);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write.</param>
        /// <param name="encoding">The character encoding to use</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        public override void AppendAllText(string path, string contents, Encoding encoding)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);

            // Init storage                
            BlobInfo blobInfo = new BlobInfo(path);

            BlobInfoProvider.AppendTextToBlob(blobInfo, contents, encoding);

            SetBlobLastWriteTimeToNow(blobInfo);
        }


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="bytes">Bytes to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>
        public override void WriteAllBytes(string path, byte[] bytes)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);

            path = Directory.GetValidPath(path);

            BlobInfo blobInfo = new BlobInfo(path);
            BlobInfoProvider.PutByteArrayToBlob(blobInfo, bytes);

            SetBlobLastWriteTimeToNow(blobInfo);
        }


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception>        
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path"/> does not exist</exception>
        public override IO.FileStream OpenRead(string path)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);
            ThrowOnNonexistentFile(path);

            return new FileStream(path, FileMode.Open, FileAccess.Read, mDateTimeNowService);
        }


        /// <summary>
        /// Sets the specified FileAttributes  of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="fileAttributes">File attributes.</param>
        public override void SetAttributes(string path, FileAttributes fileAttributes)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);
            ThrowOnNonexistentFile(path);

            BlobInfo blobInfo = new BlobInfo(path);

            // Set attributes to blob storage
            if (BlobInfoProvider.BlobExists(blobInfo))
            {
                SetBlobLastWriteTimeToNow(blobInfo);
                blobInfo.SetMetadata(ContainerInfoProvider.ATTRIBUTES, ValidationHelper.GetString(ValidationHelper.GetInteger(fileAttributes, 0), string.Empty), false);
            }
            else
            {
                System.IO.File.SetAttributes(path, (System.IO.FileAttributes)fileAttributes);
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
            return new FileStream(path, mode, access, mDateTimeNowService);
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
        /// Creates or overwrites a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file.</param>      
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception> 
        public override StreamWriter CreateText(string path)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);

            BlobInfo blobInfo = new BlobInfo(path);
            BlobInfoProvider.CreateEmptyBlob(blobInfo);

            // Create stream and stream writer objects
            FileStream fileStream = new FileStream(path, FileMode.Create, mDateTimeNowService);
            return StreamWriter.New(fileStream);
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override FileSecurity GetAccessControl(string path)
        {
            // In Microsoft Azure has user full control over file system
            return new FileSecurity();
        }


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception> 
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path"/> does not exist</exception>
        public override DateTime GetLastWriteTime(string path)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);
            ThrowOnNonexistentFile(path);
            
            BlobInfo blobInfo = new BlobInfo(path);

            // Set attributes to blob storage
            if (BlobInfoProvider.BlobExists(blobInfo))
            {
                return ValidationHelper.GetDateTime(blobInfo.GetMetadata(ContainerInfoProvider.LAST_WRITE_TIME), DateTimeHelper.ZERO_TIME);
            }
            else
            {
                return System.IO.File.GetLastAccessTime(path);
            }
        }


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="lastWriteTime">Last write time.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains invalid characters</exception>
        /// <exception cref="System.IO.PathTooLongException"><paramref name="path"/> is too long</exception> 
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path"/> does not exist</exception>
        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            ThrowOnNullPath(path);
            ThrowOnInvalidPath(path);
            ThrowOnTooLongFilePath(path);
            ThrowOnNonexistentFile(path);

            BlobInfo blobInfo = new BlobInfo(path);

            if (BlobInfoProvider.BlobExists(blobInfo))
            {
                blobInfo.SetMetadata(ContainerInfoProvider.LAST_WRITE_TIME, ValidationHelper.GetString(lastWriteTime, string.Empty));
            }
            else
            {
                System.IO.File.SetLastWriteTime(path, lastWriteTime);
            }
        }


        /// <summary>
        /// Returns URL to file. If can be accessed directly then direct URL is generated else URL with GetFile page is generated.
        /// </summary>
        /// <param name="path">Virtual path starting with ~ or absolute path.</param>
        /// <param name="siteName">Site name.</param>
        public override string GetFileUrl(string path, string siteName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string relativePath = path;
            if (relativePath.StartsWithCSafe("~"))
            {
                relativePath = relativePath.Substring(1);
            }

            BlobInfo blobInfo = new BlobInfo(relativePath);

            if (!BlobInfoProvider.BlobExists(blobInfo))
            {
                return null;
            }

            string blobName = Directory.GetBlobPathFromPath(relativePath);

            // If runs on Azure or uses Azure for a storage and files should be generated directly to Azure storage 
            if (ContainerInfoProvider.IsContainerPublic(path))
            {
                // Remove starting "/"
                return String.Format("{0}{1}/{2}", AzureHelper.BlobUrl, ContainerInfoProvider.GetRootContainer(path).ToLowerCSafe(), Directory.GetCaseValidPath(blobName));
            }

            string relative = "~\\" + Path.EnsureBackslashes(blobName);
            string url = AzureHelper.GetDownloadPath(relative);
            string hash = ValidationHelper.GetHashString(URLHelper.GetQuery(url), new HashSettings(""));
            url = URLHelper.AddParameterToUrl(url, "hash", hash);
            return URLHelper.ResolveUrl(url);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Throws <see cref="NullReferenceException"/> if <paramref name="path"/> is null.
        /// </summary>
        private void ThrowOnNullPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
        }


        /// <summary>
        /// Throws <see cref="ArgumentException"/> if <paramref name="path"/> is empty, 
        /// contains only white space or contains invalid characters.
        /// </summary>
        private void ThrowOnInvalidPath(string path)
        {
            if (path != null && (String.IsNullOrWhiteSpace(path) || IsPathToFileContainingInvalidChars(path)))
            {
                throw new ArgumentException($"Path '{path}' is invalid.");
            }
        }


        /// <summary>
        /// Throws <see cref="System.IO.FileNotFoundException"/> if file referenced by <paramref name="path"/>
        /// does not exist.
        /// </summary>
        private void ThrowOnNonexistentFile(string path)
        {
            if (!Exists(path))
            {
                throw new System.IO.FileNotFoundException($"Path '{path}' does not exist.");
            }
        }


        /// <summary>
        /// Throws <see cref="System.IO.PathTooLongException"/> if the path leading to a file exceeds maximum length.
        /// </summary>
        private void ThrowOnTooLongFilePath(string path)
        {
            var normalizedPath = Directory.GetValidPath(path);
            if (normalizedPath.Length > FILE_NAME_MAX_LENGTH)
            {
                throw new System.IO.PathTooLongException($"Path '{path}' is too long.");
            }
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="path"/> contains some invalid char.
        /// </summary>
        private bool IsPathToFileContainingInvalidChars(string path)
        {
            var invalidPathChars = new string(System.IO.Path.GetInvalidPathChars());
            var invalidFileChars = new string(System.IO.Path.GetInvalidFileNameChars());
            var fileName = Path.GetFileName(path);

            return path.Intersect(invalidPathChars).Count() != 0 || fileName.Intersect(invalidFileChars).Count() != 0;
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="path"/> has format of Azure directory placeholder.
        /// </summary> 
        private bool IsPathDirectoryPlaceholder(string path)
        {
            if (path != null && path.EndsWithCSafe(BlobInfoProvider.DIRECTORY_BLOB))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Set blob's last write time to the current time.
        /// </summary>
        private void SetBlobLastWriteTimeToNow(BlobInfo blobInfo, DateTime? now = null)
        {
            if (now == null)
            {
                now = mDateTimeNowService.GetDateTimeNow();
            }

            blobInfo.SetMetadata(ContainerInfoProvider.LAST_WRITE_TIME, ValidationHelper.GetString(now, String.Empty));
        }


        /// <summary>
        /// Set blob's last write time to the current time.
        /// </summary>
        private void SetBlobCreationTimeToNow(BlobInfo blobInfo, DateTime? now = null)
        {
            if (now == null)
            {
                now = mDateTimeNowService.GetDateTimeNow();
            }
            blobInfo.SetMetadata(ContainerInfoProvider.CREATION_TIME, ValidationHelper.GetString(now, String.Empty));
        }

        #endregion
    }
}
