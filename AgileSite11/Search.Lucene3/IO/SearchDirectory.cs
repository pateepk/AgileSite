using CMS.IO;

using Lucene.Net.Store;

using Directory = Lucene.Net.Store.Directory;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Provides methods for manipulation with Lucene.NET index.
    /// </summary>
    internal class SearchDirectory : Directory
    {
        #region "Variables"

        private string directoryName = string.Empty;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates instance of directory class.
        /// </summary>
        /// <param name="dirName">Directory name.</param>
        public SearchDirectory(string dirName)
        {
            directoryName = dirName;
        }

        #endregion


        #region "Methods"

        protected override void Dispose(bool disposing)
        {
            directoryName = string.Empty;
        }


        /// <summary>
        /// Creates instance of IndexOutput for writing to file. 
        /// </summary>
        /// <param name="name">File name.</param>
        public override IndexOutput CreateOutput(string name)
        {
            string fullPath = DirectoryHelper.CombinePath(directoryName, name);

            var stream = FileStream.New(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            return new SearchIndexOutput(stream);
        }


        /// <summary>
        /// Deletes file.
        /// </summary>
        /// <param name="name">File name.</param>
        public override void DeleteFile(string name)
        {
            string fullPath = Path.Combine(directoryName, name);
            File.Delete(fullPath);
        }


        /// <summary>
        /// Returns list of files in directory.
        /// </summary>    
        public override string[] ListAll()
        {
            string[] files = IO.Directory.GetFiles(directoryName);
            for (int i = 0; i != files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

            return files;
        }

        /// <summary>
        /// Returns whether file exists.
        /// </summary>
        /// <param name="name">Filename.</param>
        public override bool FileExists(string name)
        {
            string fullPath = Path.Combine(directoryName, name);
            return File.Exists(fullPath);
        }


        /// <summary>
        /// Returns length of the file.
        /// </summary>
        /// <param name="name">File name.</param>
        public override long FileLength(string name)
        {
            string fullPath = DirectoryHelper.CombinePath(directoryName, name);
            FileInfo file = FileInfo.New(fullPath);
            return file.Length;
        }


        /// <summary>
        /// Returns when was file modified for last time.
        /// </summary>
        /// <param name="name">File name.</param>
        public override long FileModified(string name)
        {
            string fullPath = Path.Combine(directoryName, name);
            FileInfo file = FileInfo.New(fullPath);
            return file.LastWriteTime.ToFileTimeUtc();
        }


        /// <summary>
        /// Opens file for reading.
        /// </summary>
        /// <param name="name">File name.</param>
        public override IndexInput OpenInput(string name)
        {
            // Files in .zip folders has to be opened read-only
            var sharingPermission = FileShare.ReadWrite;
            if (ZipStorageProvider.IsZipFolderPath(directoryName))
            {
                sharingPermission = FileShare.Read;
            }

            string fullPath = Path.Combine(directoryName, name);

            var stream = FileStream.New(fullPath, FileMode.Open, FileAccess.Read, sharingPermission);

            return new SearchIndexInput(stream, BufferedIndexInput.BUFFER_SIZE, FSDirectory.DEFAULT_READ_CHUNK_SIZE);
        }


        /// <summary>
        /// Creates empty file.
        /// </summary>
        /// <param name="name">File name.</param>
        public override void TouchFile(string name)
        {
            string fullPath = Path.Combine(directoryName, name);
            FileStream fs = File.Create(fullPath);
            fs.Close();
        }


        /// <summary>
        /// Clears (releases) lock.
        /// </summary>
        /// <param name="name">Lock name.</param>
        public override void ClearLock(string name)
        {
            SearchLock l = new SearchLock(directoryName, name);
            l.Release();
        }


        /// <summary>
        /// Makes (creates) lock.
        /// </summary>
        /// <param name="name">Lock name.</param>
        public override Lock MakeLock(string name)
        {
            return new SearchLock(directoryName, name);
        }

        #endregion
    }
}