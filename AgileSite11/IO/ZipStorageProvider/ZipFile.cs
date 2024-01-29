using System;
using System.Security.AccessControl;
using System.Text;

namespace CMS.IO.Zip
{
    /// <summary>
    /// Zip file operation provider
    /// </summary>
    public class ZipFile : AbstractFile
    {
        #region "Variables"

        /// <summary>
        /// Parent provider
        /// </summary>
        ZipStorageProvider mProvider = null;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Parent provider</param>
        public ZipFile(ZipStorageProvider provider)
        {
            mProvider = provider;
        }


        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file</param>  
        public override bool Exists(string path)
        {
            return (mProvider.GetFileEntry(path) != null);
        }


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override StreamReader OpenText(string path)
        {
            var s = OpenRead(path);
            var reader = StreamReader.New(s);

            return reader;
        }


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override void Delete(string path)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>
        /// <param name="overwrite">If destination file should be overwritten.</param>
        public override void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            // If the destination storage provider is different, use helper method to copy the file
            if (!StorageHelper.IsSameStorageProvider(sourceFileName, destFileName))
            {
                StorageHelper.CopyFileAcrossProviders(sourceFileName, destFileName);
                return;
            }

            if (Exists(sourceFileName))
            {
                // Check whether destination file exist
                bool isExist = File.Exists(destFileName);
                if (isExist && !overwrite)
                {
                    return;
                }

                // Read the data from source file
                byte[] data = File.ReadAllBytes(sourceFileName);

                // Write the data to destination file
                File.WriteAllBytes(destFileName, data);
            }
            else
            {
                throw new Exception("[File.Copy]: Source path " + sourceFileName + " does not exist.");
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
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override byte[] ReadAllBytes(string path)
        {
            return mProvider.GetFileData(path);
        }


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public override FileStream Create(string path)
        {
            ZipStorageProvider.ThrowReadOnly();
            return null;
        }


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name</param>
        /// <param name="destFileName">Destination file name</param>
        public override void Move(string sourceFileName, string destFileName)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        public override string ReadAllText(string path)
        {
            using (var reader = OpenText(path))
            {
                return reader.ReadToEnd();
            }
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        public override string ReadAllText(string path, Encoding encoding)
        {
            using (var reader = StreamReader.New(OpenRead(path), encoding))
            {
                return reader.ReadToEnd();
            }
        }



        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        public override void WriteAllText(string path, string contents)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void WriteAllText(string path, string contents, Encoding encoding)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        public override void AppendAllText(string path, string contents)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void AppendAllText(string path, string contents, Encoding encoding)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="bytes">Bytes to write</param>
        public override void WriteAllBytes(string path, byte[] bytes)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override FileStream OpenRead(string path)
        {
            return new ZipFileStream(mProvider, path);
        }


        /// <summary>
        /// Sets the specified FileAttributes  of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="fileAttributes">File attributes</param>
        public override void SetAttributes(string path, FileAttributes fileAttributes)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Opens a FileStream  on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        public override FileStream Open(string path, FileMode mode, FileAccess access)
        {
            return new ZipFileStream(mProvider, path);
        }


        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="lastWriteTimeUtc">Specified time</param>
        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file</param>        
        public override StreamWriter CreateText(string path)
        {
            ZipStorageProvider.ThrowReadOnly();
            return null;
        }


        /// <summary>
        /// Gets a DirectorySecurity  object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public override FileSecurity GetAccessControl(string path)
        {
            return new FileSecurity();
        }


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override DateTime GetLastWriteTime(string path)
        {
            return mProvider.ZipFile.LastWriteTime;
        }


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="lastWriteTime">Last write time</param>
        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            ZipStorageProvider.ThrowReadOnly();
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