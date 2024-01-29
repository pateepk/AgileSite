using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.IO.Zip;


namespace CMS.IO
{
    /// <summary>
    /// Represents virtual storage provider over zip file.
    /// </summary>
    /// <remarks>
    /// Provider is read-only virtual file system, allowing to get files as <see cref="ZipFileStream"/>.
    /// </remarks>
    public class ZipStorageProvider : AbstractStorageProvider, IDisposable
    {
        #region "Variables"

        private static readonly ConcurrentDictionary<string, bool> mMappedZipProviders = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        // Limit size for the caching of the file data (bytes), 10 kB by default
        private int mCacheDataLimit = -1;
        private bool disposed;

        private readonly Dictionary<string, ZipArchiveEntry> mEntries;
        private readonly Dictionary<string, ZipFileInfo> mFiles;
        private readonly Dictionary<string, ZipDirectoryInfo> mDirectories;

        private ConcurrentDictionary<string, byte[]> mData;

        private FileStream mFileStream;
        private ZipArchive zipArchive;

        private readonly object zipArchiveSubStreamLock = new object();

        /// <summary>
        /// Starting character for the zip archive within path
        /// </summary>
        public const char ZIP_START = '[';


        /// <summary>
        /// Ending character for the zip archive within path
        /// </summary>
        public const char ZIP_END = ']';

        #endregion


        #region "Properties"

        /// <summary>
        /// Provider name
        /// </summary>
        public override string Name
        {
            get
            {
                return IOProviderName.Zip;
            }
        }


        /// <summary>
        /// If true, the loaded data is cached
        /// </summary>
        public bool CacheData
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Limit size for the caching of the file data (bytes)
        /// </summary>
        public int CacheDataLimit
        {
            get
            {
                if (mCacheDataLimit < 0)
                {
                    // Load from settings, 10 kB default
                    mCacheDataLimit = CoreServices.AppSettings["CMSZipCacheDataLimit"].ToInteger(10 * 1024);
                }

                return mCacheDataLimit;
            }
            set
            {
                mCacheDataLimit = value;
            }
        }


        /// <summary>
        /// Zip file info object
        /// </summary>
        public FileInfo ZipFile
        {
            get;
            protected set;
        }


        /// <summary>
        /// Zip file path
        /// </summary>
        public string FilePath
        {
            get;
            protected set;
        }


        /// <summary>
        /// Root directory
        /// </summary>
        public ZipDirectoryInfo RootDirectory
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zipFilePath">Path to the zip file</param>
        /// <param name="mappedPath">Mapped path</param>
        /// <param name="parentProvider">Parent storage provider</param>
        public ZipStorageProvider(string zipFilePath, string mappedPath, AbstractStorageProvider parentProvider)
        {
            FilePath = StorageHelper.GetMappingPath(zipFilePath);
            MappedPath = StorageHelper.GetMappingPath(mappedPath);
            ParentStorageProvider = parentProvider;

            mEntries = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
            mFiles = new Dictionary<string, ZipFileInfo>(StringComparer.OrdinalIgnoreCase);
            mDirectories = new Dictionary<string, ZipDirectoryInfo>(StringComparer.OrdinalIgnoreCase);
            mData = new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            LoadContents();

            mMappedZipProviders[MappedPath] = true;
        }


        /// <summary>
        /// Returns true if the given file path is a zip file
        /// </summary>
        /// <param name="path">File path</param>
        public static bool IsZipFile(string path)
        {
            return String.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ZipStorageProvider"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by the current instance of the <see cref="ZipStorageProvider"/> class, and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release unmanaged resources only.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                mMappedZipProviders[MappedPath] = false;

                zipArchive?.Dispose();
                zipArchive = null;

                mFileStream?.Dispose();
                mFileStream = null;

                mData = null;
            }

            disposed = true;
        }


        /// <summary>
        /// Disposes the storage provider for the given path
        /// </summary>
        public static void Dispose(string path)
        {
            // Cut the path to the last zip file path
            int endZipIndex = path.LastIndexOf(ZIP_END);
            if (endZipIndex >= 0)
            {
                path = path.Substring(0, endZipIndex + 1);
            }

            // Unmap the provider if mapped
            var provider = StorageHelper.UnMapStoragePath(path) as IDisposable;
            provider?.Dispose();
        }


        /// <summary>
        /// Disposes the zip storage providers
        /// </summary>
        /// <param name="pathCondition">Path condition</param>
        public static void DisposeAll(Func<string, bool> pathCondition = null)
        {
            var disposePaths = mMappedZipProviders
                                    .Where(x => x.Value && (pathCondition == null || pathCondition(x.Key)))
                                    .Select(x => x.Key)
                                    .OrderByDescending(p => p);

            foreach (string path in disposePaths)
            {
                Dispose(path);
            }
        }


        /// <summary>
        /// Gets the zip file path from the given zip folder path. Returns null if the path isn't a zip folder path
        /// </summary>
        public static bool IsZipFolderPath(string path)
        {
            // Check for the zip archive on the way
            int zipIndex = path.IndexOf(ZIP_START);
            if (zipIndex >= 0)
            {
                int endZipIndex = path.IndexOf(ZIP_END, zipIndex);
                if ((endZipIndex >= 0) && (zipIndex < endZipIndex))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Gets the zip file name with the container brackets
        /// </summary>
        /// <param name="fileName">File name</param>
        public static string GetZipFileName(string fileName)
        {
            return ZIP_START + fileName + ZIP_END;
        }


        /// <summary>
        /// Throws a not supported exception informing that zip file content is read-only
        /// </summary>
        internal static void ThrowReadOnly()
        {
            throw new NotSupportedException("The content of zipped files is read-only.");
        }


        /// <summary>
        /// Gets a file stream for the given file
        /// </summary>
        /// <param name="fileName">File name</param>
        public Stream GetFileStream(string fileName)
        {
            var data = GetFileData(fileName);

            return data != null ? new MemoryStream(data, false) : null;
        }


        /// <summary>
        /// Gets the stream for the entry in zipped file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Returns the data size</param>
        private Stream GetZippedFileStream(string fileName, out long size)
        {
            ZipArchiveEntry entry;
            if (mEntries.TryGetValue(fileName, out entry))
            {
                lock (zipArchiveSubStreamLock)
                {
                    var stream = entry.Open();
                    size = entry.Length;
                    return stream;
                }
            }

            size = 0;
            return null;
        }


        /// <summary>
        /// Gets the particular file data
        /// </summary>
        /// <param name="fileName">File name</param>
        public byte[] GetFileData(string fileName)
        {
            byte[] data;
            if (CacheData && mData.TryGetValue(fileName, out data))
            {
                return data;
            }

            // Get the internal stream
            long size;
            using (var stream = GetZippedFileStream(fileName, out size))
            {
                if (stream != null)
                {
                    if (size > Int32.MaxValue)
                    {
                        throw new InvalidOperationException($"File '{fileName}' cannot be read. Its size {size} bytes exceeds the limit of {Int32.MaxValue} bytes.");
                    }

                    // Load data from the stream
                    data = new byte[size];
                    lock (zipArchiveSubStreamLock)
                    {
                        int offset = 0;
                        int bytesRead;
                        while ((bytesRead = stream.Read(data, offset, (int)size - offset)) > 0)
                        {
                            offset += bytesRead;
                        }
                    }

                    if (CacheData && CacheDataLimit >= size)
                    {
                        mData[fileName] = data;
                    }

                    return data;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the particular file data
        /// </summary>
        /// <param name="fileName">File name</param>
        internal ZipArchiveEntry GetFileEntry(string fileName)
        {
            ZipArchiveEntry entry;
            mEntries.TryGetValue(fileName, out entry);
            return entry;
        }


        /// <summary>
        /// Loads the contents of the zip file
        /// </summary>
        protected void LoadContents()
        {
            ZipFile = FileInfo.New(FilePath);

            // Ensure the root directory
            RootDirectory = EnsureDirectory(MappedPath);

            mFileStream = File.OpenRead(FilePath);
            zipArchive = new ZipArchive(mFileStream, ZipArchiveMode.Read, true);

            foreach (var theEntry in zipArchive.Entries)
            {
                string fullFileName = Path.EnsureBackslashes(theEntry.FullName);
                fullFileName = GetFullPath(fullFileName);

                string fileName = Path.GetFileName(fullFileName);

                if (!string.IsNullOrEmpty(fileName))
                {
                    string directoryName = Path.GetDirectoryName(fullFileName);
                    var dir = EnsureDirectory(directoryName);

                    ZipFileInfo file = new ZipFileInfo(this, fullFileName);

                    mFiles[fullFileName] = file;
                    mEntries[fullFileName] = theEntry;

                    dir.RegisterFile(file);
                }
                else
                {
                    string directoryName = fullFileName.TrimEnd('\\');
                    EnsureDirectory(directoryName);
                }
            }
        }


        /// <summary>
        /// Ensures the given directory
        /// </summary>
        /// <param name="directoryName">Directory name</param>
        private ZipDirectoryInfo EnsureDirectory(string directoryName)
        {
            // Ensure the directory
            ZipDirectoryInfo dir = (ZipDirectoryInfo)GetDirectoryInfo(directoryName);
            if (dir == null)
            {
                dir = new ZipDirectoryInfo(this, directoryName);
                mDirectories[directoryName] = dir;

                // Ensure parent directory
                if (!String.IsNullOrEmpty(directoryName) && (directoryName.Length > MappedPath.Length))
                {
                    int slashIndex = directoryName.LastIndexOf('\\');
                    if (slashIndex >= 0)
                    {
                        string parentName = directoryName.Substring(0, slashIndex);
                        var parentDir = EnsureDirectory(parentName);

                        // Register the directory within parent
                        parentDir.RegisterDirectory(dir);
                    }
                    else
                    {
                        // Register to root directory
                        RootDirectory.RegisterDirectory(dir);
                    }
                }
            }

            return dir;
        }


        /// <summary>
        /// Returns read-only stream for given <paramref name="path"/> inside the zip file.
        /// </summary>
        /// <remarks>
        /// Other parameters are checked only to determine whether the read only exception is thrown or not.
        /// </remarks>
        private ZipFileStream GetZipFileStream(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
        {
            if (mode != FileMode.Open)
            {
                ThrowReadOnly();
            }

            if (access != FileAccess.Read)
            {
                ThrowReadOnly();
            }

            if (share != FileShare.None && share != FileShare.Read)
            {
                ThrowReadOnly();
            }

            return new ZipFileStream(this, path);
        }


        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        protected override AbstractFile CreateFileProviderObject()
        {
            return new Zip.ZipFile(this);
        }


        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        protected override AbstractDirectory CreateDirectoryProviderObject()
        {
            return new ZipDirectory(this);
        }


        /// <summary>
        /// Returns new instance of FileInfo object.
        /// </summary>
        /// <param name="fileName">File name</param>
        public override FileInfo GetFileInfo(string fileName)
        {
            ZipFileInfo zipInfo;
            mFiles.TryGetValue(fileName, out zipInfo);
            return zipInfo;
        }


        /// <summary>
        /// Returns new instance of directory info.
        /// </summary>
        /// <param name="path">Path</param>        
        public override DirectoryInfo GetDirectoryInfo(string path)
        {
            path = path.TrimEnd('\\');

            ZipDirectoryInfo dirInfo;
            mDirectories.TryGetValue(path, out dirInfo);
            return dirInfo;
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>        
        public override FileStream GetFileStream(string path, FileMode mode)
        {
            return GetZipFileStream(path, mode);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access)
        {
            return GetZipFileStream(path, mode, access);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>        
        /// <param name="share">Sharing permissions</param>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return GetZipFileStream(path, mode, access, share);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="share">Sharing permissions</param>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            return GetZipFileStream(path, mode, access, share);
        }

        #endregion
    }
}
