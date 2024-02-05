using System;
using System.Security.AccessControl;
using System.Text;

namespace CMS.IO
{
    /// <summary>
    /// Abstract class for file providers.
    /// </summary>
    public abstract class AbstractFile
    {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file</param>  
        public abstract bool Exists(string path);


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public abstract StreamReader OpenText(string path);


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        public abstract void Delete(string path);


        /// <summary>
        /// Copies an existing file to a new file.
        /// </summary>
        /// <param name="sourceFileName">Path to source file</param>
        /// <param name="destFileName">Path to destination file</param>        
        public abstract void Copy(string sourceFileName, string destFileName);


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file</param>
        /// <param name="destFileName">Path to destination file</param>
        /// <param name="overwrite">If destination file should be overwritten</param>
        public abstract void Copy(string sourceFileName, string destFileName, bool overwrite);


        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param>
        public abstract byte[] ReadAllBytes(string path);


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public abstract FileStream Create(string path);


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name</param>
        /// <param name="destFileName">Destination file name</param>
        public abstract void Move(string sourceFileName, string destFileName);


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public abstract string ReadAllText(string path);


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        public abstract string ReadAllText(string path, Encoding encoding);


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        public abstract void WriteAllText(string path, string contents);


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public abstract void WriteAllText(string path, string contents, Encoding encoding);


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        public abstract void AppendAllText(string path, string contents);


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public abstract void AppendAllText(string path, string contents, Encoding encoding);       


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="bytes">Bytes to write</param>
        public abstract void WriteAllBytes(string path, byte[] bytes); 


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public abstract FileStream OpenRead(string path);


        /// <summary>
        /// Sets the specified FileAttributes  of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="fileAttributes">File attributes</param>
        public abstract void SetAttributes(string path, FileAttributes fileAttributes);


        /// <summary>
        /// Opens a FileStream  on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        public abstract FileStream Open(string path, FileMode mode, FileAccess access);


        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="lastWriteTimeUtc">Specified time</param>
        public abstract void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);


        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file</param>        
        public abstract StreamWriter CreateText(string path);


        /// <summary>
        /// Gets a DirectorySecurity  object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public abstract FileSecurity GetAccessControl(string path);


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        public abstract DateTime GetLastWriteTime(string path);


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="lastWriteTime">Last write time</param>
        public abstract void SetLastWriteTime(string path, DateTime lastWriteTime);


        /// <summary>
        /// Returns URL to file. If can be accessed directly then direct URL is generated else URL with GetFile page is generated.
        /// </summary>
        /// <param name="path">Virtual path starting with ~ or absolute path.</param>
        /// <param name="siteName">Site name.</param>
        public abstract string GetFileUrl(string path, string siteName);
    }
}