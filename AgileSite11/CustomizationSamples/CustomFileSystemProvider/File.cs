using System;
using System.Text;
using System.Security.AccessControl;

using CMS.IO;

namespace CMS.CustomFileSystemProvider
{
    /// <summary>
    /// Sample of File class of CMS.IO provider.
    /// </summary>
    public class File : CMS.IO.AbstractFile
    {
        #region "Public properties"

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file.</param>  
        public override bool Exists(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override CMS.IO.StreamReader OpenText(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override void Delete(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>
        /// <param name="overwrite">If destination file should be overwritten.</param>
        public override void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>        
        public override void Copy(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override byte[] ReadAllBytes(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        public override CMS.IO.FileStream Create(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destFileName">Destination file name.</param>
        public override void Move(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        public override string ReadAllText(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        public override string ReadAllText(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write.</param>
        public override void WriteAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void WriteAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write.</param>
        public override void AppendAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write.</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void AppendAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="bytes">Bytes to write.</param>
        public override void WriteAllBytes(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override CMS.IO.FileStream OpenRead(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sets the specified FileAttributes  of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="fileAttributes">File attributes.</param>
        public override void SetAttributes(string path, CMS.IO.FileAttributes fileAttributes)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Opens a FileStream  on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        public override CMS.IO.FileStream Open(string path, CMS.IO.FileMode mode, CMS.IO.FileAccess access)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="lastWriteTimeUtc">Specified time.</param>
        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file.</param>      
        public override CMS.IO.StreamWriter CreateText(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override FileSecurity GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override DateTime GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="lastWriteTime">Last write time.</param>
        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns URL to file. If can be accessed directly then direct URL is generated else URL with GetFile page is generated.
        /// </summary>
        /// <param name="path">Virtual path starting with ~ or absolute path.</param>
        /// <param name="siteName">Site name.</param>
        public override string GetFileUrl(string path, string siteName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
