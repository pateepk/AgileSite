using System;
using System.Security.AccessControl;
using System.Text;

using CMS.IO;
using CMS.Base;

namespace CMS.FileSystemStorage
{
    /// <summary>
    /// Envelope for System.IO.File class.
    /// </summary>
    public class File : AbstractFile
    {
        /// <summary>
        /// Raised when the <see cref="GetFileUrl" /> method is called.
        /// </summary>
        /// <remarks>This API supports the framework infrastructure and is not intended to be used directly from your code.</remarks>
        public static readonly SimpleHandler<GetFileUrlEventArgs> GetFileUrlForPath = new SimpleHandler<GetFileUrlEventArgs>();


        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file</param>  
        public override bool Exists(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return System.IO.File.Exists(path);
        }


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override StreamReader OpenText(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.StreamReader sr = System.IO.File.OpenText(path);

            // Create stream reader
            StreamReader reader = StreamReader.New(sr);
            return reader;
        }


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override void Delete(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.Delete(path);

            StorageSynchronization.LogDeleteFileTask(path);
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file</param>
        /// <param name="destFileName">Path to destination file</param>
        /// <param name="overwrite">If destination file should be overwritten</param>
        public override void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
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

            sourceFileName = AbstractStorageProvider.GetTargetPhysicalPath(sourceFileName);
            destFileName = AbstractStorageProvider.GetTargetPhysicalPath(destFileName);

            System.IO.File.Copy(sourceFileName, destFileName, overwrite);

            StorageSynchronization.LogUpdateFileTask(destFileName);
        }


        /// <summary>
        /// Copies an existing file to a new file.
        /// </summary>
        /// <param name="sourceFileName">Path to source file</param>
        /// <param name="destFileName">Path to destination file</param>        
        public override void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }


        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override byte[] ReadAllBytes(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return System.IO.File.ReadAllBytes(path);
        }


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public override IO.FileStream Create(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.FileStream fsStream = System.IO.File.Create(path);
            IO.FileStream stream = new FileStream(fsStream);
            return stream;
        }


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name</param>
        /// <param name="destFileName">Destination file name</param>
        public override void Move(string sourceFileName, string destFileName)
        {
            // If the destination storage provider is different, use helper method to copy the file
            if (!StorageHelper.IsSameStorageProvider(sourceFileName, destFileName))
            {
                StorageHelper.MoveFileAcrossProviders(sourceFileName, destFileName);
                return;
            }

            sourceFileName = AbstractStorageProvider.GetTargetPhysicalPath(sourceFileName);
            destFileName = AbstractStorageProvider.GetTargetPhysicalPath(destFileName);
            System.IO.File.Move(sourceFileName, destFileName);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public override string ReadAllText(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return System.IO.File.ReadAllText(path);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        public override string ReadAllText(string path, Encoding encoding)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return System.IO.File.ReadAllText(path, encoding);
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        public override void WriteAllText(string path, string contents)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.WriteAllText(path, contents);

            StorageSynchronization.LogUpdateFileTask(path);
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void WriteAllText(string path, string contents, Encoding encoding)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.WriteAllText(path, contents, encoding);

            StorageSynchronization.LogUpdateFileTask(path);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        public override void AppendAllText(string path, string contents)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.AppendAllText(path, contents);

            StorageSynchronization.LogUpdateFileTask(path);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void AppendAllText(string path, string contents, Encoding encoding)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.AppendAllText(path, contents, encoding);

            StorageSynchronization.LogUpdateFileTask(path);
        }


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="bytes">Bytes to write</param>
        public override void WriteAllBytes(string path, byte[] bytes)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.WriteAllBytes(path, bytes);

            StorageSynchronization.LogUpdateFileTask(path);
        }


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override IO.FileStream OpenRead(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            // Get System.IO.FileStream
            System.IO.FileStream fStream = System.IO.File.OpenRead(path);

            // Create  
            FileStream stream = new FileStream(fStream);
            return stream;
        }


        /// <summary>
        /// Sets the specified FileAttributes  of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="fileAttributes">File attributes</param>
        public override void SetAttributes(string path, FileAttributes fileAttributes)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.SetAttributes(path, (System.IO.FileAttributes)fileAttributes);
        }


        /// <summary>
        /// Opens a FileStream  on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        public override IO.FileStream Open(string path, FileMode mode, FileAccess access)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.FileStream fs = System.IO.File.Open(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access);
            return new FileStream(fs);
        }


        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="lastWriteTimeUtc">Specified time</param>
        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        }


        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file</param>        
        public override StreamWriter CreateText(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return StreamWriter.New(System.IO.File.CreateText(path));
        }


        /// <summary>
        /// Gets a DirectorySecurity  object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public override FileSecurity GetAccessControl(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return System.IO.File.GetAccessControl(path);
        }


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override DateTime GetLastWriteTime(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return System.IO.File.GetLastWriteTime(path);
        }


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="lastWriteTime">Last write time</param>
        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            System.IO.File.SetLastWriteTime(path, lastWriteTime);
        }


        /// <summary>
        /// Returns URL to file. 
        /// </summary>
        /// <param name="path">Virtual path starting with ~ or absolute path.</param>
        /// <param name="siteName">Site name.</param>
        /// <remarks>
        /// Uses <see cref="GetFileUrlForPath"/> event to get web application related path. Returns null if executed outside the web application.
        /// </remarks>
        public override string GetFileUrl(string path, string siteName)
        {
            using (var eventArgs = new GetFileUrlEventArgs(path, siteName))
            {
                GetFileUrlForPath.StartEvent(eventArgs);
                return eventArgs.Url;
            }
        }
    }
}