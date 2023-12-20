using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.IO;


namespace CMS.Helpers
{
    /// <summary>
    /// Encapsulates management of persistent storage items.
    /// </summary>
    internal class PersistentStorageManager
    {
        #region "Constants"

        /// <summary>
        /// File extension of persisted items.
        /// </summary>
        private const string PERSISTENT_ITEM_FILE_EXTENSION = ".dat";
        
        internal const string OPERATION_FILE_READ = "PersistentStorageFileRead";

        private const string OPERATION_FILE_WRITE = "PersistentStorageFileWrite";
        private const string OPERATION_FILE_DELETE = "PersistentStorageFileDelete";
        private const string OPERATION_EXPIRED_FILE_DELETE = "PersistentStorageExpiredFileDelete";

        #endregion


        #region "Variables"

        /// <summary>
        /// Prefix to uniquely identify locks for storage items (each lock is identified by this prefix and storage item key).
        /// </summary>
        private readonly string mItemLockObjectsKeyPrefix;


        /// <summary>
        /// Defines how long the persisted items are guaranteed to exist.
        /// </summary>
        private readonly TimeSpan mPersistentItemsKeepInterval;
        private string mPersistentDirectory;

        #endregion


        #region "Properties"

        /// <summary>
        /// Directory for storing persistent items.
        /// If not set, or set to null, default directory is used.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default persistence directory is determined by app setting <c>CMSPersistentDirectory</c>.
        /// If not set, directory <c>Persistent</c> within <c>App_Data</c> is used.
        /// </para>
        /// <para>
        /// If multiple <see cref="PersistentStorageManager"/> instances operate within the same directory,
        /// the behavior is undefined.
        /// </para>
        /// </remarks>
        public string PersistentDirectory
        {
            get
            {
                if (mPersistentDirectory == null)
                {
                    mPersistentDirectory = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSPersistentDirectory"], SystemContext.WebApplicationPhysicalPath + "\\App_Data\\Persistent");
                    Directory.CreateDirectory(mPersistentDirectory);
                }              

                return mPersistentDirectory;
            }
            set
            {
                if (value != null)
                {
                    Directory.CreateDirectory(value);
                    mPersistentDirectory = value;
                }
                else
                {
                    mPersistentDirectory = null;
                }
            }
        }


        /// <summary>
        /// Gets prefix used for unique identification of storage item locks.
        /// </summary>
        public string ItemLockObjectsKeyPrefix
        {
            get
            {
                return mItemLockObjectsKeyPrefix;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new persistent storage manager.
        /// </summary>
        /// <param name="itemLockObjectsKeyPrefix">Prefix used for unique identification of storage item locks.</param>
        /// <param name="persistentItemsKeepInterval">Interval specifying how long (at least) the storage items must exist.</param>
        public PersistentStorageManager(string itemLockObjectsKeyPrefix, TimeSpan persistentItemsKeepInterval)
        {
            mItemLockObjectsKeyPrefix = itemLockObjectsKeyPrefix;
            mPersistentItemsKeepInterval = persistentItemsKeepInterval;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets the specified item from the persistent storage.
        /// The value is loaded from the storage only if <paramref name="loadCondition"/> is met. Otherwise the result of <paramref name="notLoadedReturnFunc"/> is returned.
        /// The evaluation of condition and execution of either load action or return function are performed atomically.
        /// </summary>
        /// <param name="key">Persistent item key.</param>
        /// <param name="loadCondition">Condition evaluated atomically before the storage read operation. If the condition is evaluated to true (or is not provided),
        /// the read operation and <paramref name="loadAction"/> are performed.</param>
        /// <param name="loadAction">Action executed atomically when the <paramref name="loadCondition"/> is met. The value read from the storage is passed as the action argument.
        /// If the storage item does not exist, null value is passed as the action argument.
        /// </param>
        /// <param name="notLoadedReturnFunc">Function whose result is returned when <paramref name="loadCondition"/> is not met. If it is not provided, it is treated as if it returned null.</param>
        /// <returns>Item value loaded from the storage if the <paramref name="loadCondition"/> is met and the storage item exists.
        /// Result of <paramref name="notLoadedReturnFunc"/> when the <paramref name="loadCondition"/> is not met (if return function is not provided, it is treated as if it returns null).
        /// Null if <paramref name="loadCondition"/> is met, but the storage item does not exist.</returns>
        public object GetItemFromStorage(string key, Func<bool> loadCondition = null, Action<object> loadAction = null, Func<object> notLoadedReturnFunc = null)
        {
            object loadedValue = null;

            try
            {
                string filePath = GetFilePath(key);
                if (filePath != null)
                {
                    LockObject keyLock = GetLockForKey(key);
                    lock (keyLock)
                    {
                        // Perform the load when condition is null or is met
                        if ((loadCondition == null) || loadCondition())
                        {
                            bool performLoadAction = false;
                            try
                            {
                                if (File.Exists(filePath))
                                {
                                    using (var fs = FileStream.New(filePath, FileMode.Open, FileAccess.Read))
                                    {
                                        var bf = new BinaryFormatter();

                                        loadedValue = bf.Deserialize(fs);
                                        performLoadAction = true;

                                        FileDebug.LogFileOperation(filePath, OPERATION_FILE_READ, (int)fs.Length);
                                    }

                                    return loadedValue;
                                }
                                else
                                {
                                    performLoadAction = true;
                                }
                            }
                            finally
                            {
                                if (loadAction != null && performLoadAction)
                                {
                                    // Execute load action if no error occurred while deserializing the value, or the value does not exist
                                    loadAction(loadedValue);
                                }
                            }
                        }
                        else
                        {
                            return (notLoadedReturnFunc != null) ? notLoadedReturnFunc() : null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("PersistentStorageHelper", "GetItemFromStorage", ex);

                throw;
            }

            return null;
        }


        /// <summary>
        /// Saves the item to the persistent storage.
        /// </summary>
        /// <param name="key">Persistent item key.</param>
        /// <param name="value">Persistent item value.</param>
        /// <param name="saveAction">Action executed atomically with the storage write operation.</param>
        public void SaveItemToStorage(string key, object value, Action saveAction = null)
        {
            try
            {
                // Save the file
                string filePath = GetFilePath(key);
                if (filePath != null)
                {
                    LockObject keyLock = GetLockForKey(key);
                    lock (keyLock)
                    {
                        using (var fs = FileStream.New(filePath, FileMode.Create))
                        {
                            var bf = new BinaryFormatter();
                            bf.Serialize(fs, value);

                            try
                            {
                                // Log the file operation first, but guarantee the execution of saveAction
                                FileDebug.LogFileOperation(filePath, OPERATION_FILE_WRITE, (int)fs.Length);
                            }
                            finally
                            {
                                if (saveAction != null)
                                {
                                    saveAction();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("PersistentStorageHelper", "SaveItemToStorage", ex);

                throw;
            }
        }


        /// <summary>
        /// Deletes the specified item from the persistent storage.
        /// If the storage item does not exist, no action is performed.
        /// </summary>
        /// <param name="key">Storage item key.</param>
        /// <param name="deleteAction">Action executed atomically with the storage delete operation.</param>
        public void DeleteItemFromStorage(string key, Action deleteAction = null)
        {
            try
            {
                string filePath = GetFilePath(key);
                if (filePath != null)
                {
                    LockObject keyLock = GetLockForKey(key);
                    lock (keyLock)
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);

                            try
                            {
                                // Log the file operation first, but guarantee the execution of deleteAction
                                FileDebug.LogFileOperation(filePath, OPERATION_FILE_DELETE);
                            }
                            finally 
                            {
                                if (deleteAction != null)
                                {
                                    deleteAction();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("PersistentStorageHelper", "DeleteItemFromStorage", ex);

                throw;
            }
        }


        /// <summary>
        /// Deletes storage items which are expired.
        /// </summary>
        /// <param name="deleteAction">Action executed atomically with the storage delete operation of each expired item. The deleted item key is passed as the action argument.</param>
        public void DeleteExpiredStorageItems(Action<string> deleteAction = null)
        {
            List<string> possiblyExpiredFiles = new List<string>();
            try
            {
                string dir = PersistentDirectory;
                if ((dir != null) && Directory.Exists(dir))
                {
                    DirectoryInfo dirInfo = DirectoryInfo.New(dir);
                    FileInfo[] files = dirInfo.GetFiles();
                    if (files != null)
                    {
                        foreach (FileInfo file in files)
                        {
                            if (file.Name.EndsWithCSafe(PERSISTENT_ITEM_FILE_EXTENSION) && IsFileExpired(file.LastWriteTime))
                            {
                                // The file seems expired but since no lock is obtained for the storage item, it can change any time
                                possiblyExpiredFiles.Add(file.Name);
                            }
                        }
                        possiblyExpiredFiles.ForEach(fileName => DeletePossiblyExpiredStorageItem(fileName, deleteAction));
                    }
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("PersistentStorageHelper", "DeleteExpiredStorageItems", ex);

                throw;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Deletes a storage item if it is expired.
        /// The check whether item is expired an its removal are performed atomically.
        /// </summary>
        /// <param name="fileName">Name of the file within persistent storage (with extension).</param>
        /// <param name="deleteAction">Action executed atomically with the delete operation of expired item. The deleted item key is passed as the action argument.</param>
        private void DeletePossiblyExpiredStorageItem(string fileName, Action<string> deleteAction = null)
        {
            var key = GetKey(fileName);

            string filePath = GetFilePath(key);
            if (filePath != null)
            {
                LockObject keyLock = GetLockForKey(key);
                lock (keyLock)
                {
                    if (File.Exists(filePath))
                    {
                        FileInfo fi = FileInfo.New(filePath);
                        if (fi != null && IsFileExpired(fi.LastWriteTime))
                        {
                            fi.Delete();

                            try
                            {
                                // Log the file operation first, but guarantee the execution of deleteAction
                                FileDebug.LogFileOperation(filePath, OPERATION_EXPIRED_FILE_DELETE);
                            }
                            finally 
                            {
                                if (deleteAction != null)
                                {
                                    deleteAction(key);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets lock object for storage item identified by its key.
        /// </summary>
        /// <param name="key">Key of the storage item.</param>
        /// <returns>Lock object for synchronization of access to storage item.</returns>
        private LockObject GetLockForKey(string key)
        {
            key = mItemLockObjectsKeyPrefix + key;

            return LockHelper.GetLockObject(key);
        }


        /// <summary>
        /// Tells you whether file representing a persistent storage item has expired.
        /// </summary>
        /// <param name="lastWriteTime">Date and time of last write to the file.</param>
        /// <returns>True if the file has expired, false otherwise.</returns>
        private bool IsFileExpired(DateTime lastWriteTime)
        {
            return lastWriteTime < DateTime.Now.Add(-mPersistentItemsKeepInterval);
        }


        /// <summary>
        /// Gets the file path for specified key, or null if <see cref="PersistentDirectory"/> is null.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <returns>Path for the specified key, or null if <see cref="PersistentDirectory"/> is null.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty string.</exception>
        private string GetFilePath(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key can not be null or empty string.", "key");
            }

            if (PersistentDirectory == null)
            {
                return null;
            }

            // Create the path
            string path = DirectoryHelper.CombinePath(PersistentDirectory, key) + PERSISTENT_ITEM_FILE_EXTENSION;

            return path;
        }


        /// <summary>
        /// Gets persistent item key from file name.
        /// </summary>
        /// <param name="fileName">File name (without extension) for which to return the persistent item key.</param>
        /// <returns>Key for the file name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when file name is null.</exception>
        /// <exception cref="ArgumentException">Thrown when file name does not contain extension <see cref="PERSISTENT_ITEM_FILE_EXTENSION"/>.</exception>
        private static string GetKey(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName", "File name can not be null.");
            }
            if (!fileName.EndsWithCSafe(PERSISTENT_ITEM_FILE_EXTENSION))
            {
                throw new ArgumentException(String.Format("File '{0}' does not contain the expected extension '{1}'. It is not a persistent storage item.", fileName, PERSISTENT_ITEM_FILE_EXTENSION));
            }

            // Trim the extension
            return fileName.Remove(fileName.Length - PERSISTENT_ITEM_FILE_EXTENSION.Length);
        }

        #endregion        
    }
}
