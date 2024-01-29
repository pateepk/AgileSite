using System;
using System.Linq;
using System.Collections.Generic;

namespace CMS.IO
{
    /// <summary>
    /// List of constants for file debugging operations
    /// </summary>
    public static class FileDebugOperation
    {
        #region "Write operation constants"

        /// <summary>
        /// Delete
        /// </summary>
        public const string DELETE = "Delete";

        /// <summary>
        /// Copy
        /// </summary>
        public const string COPY = "Copy";

        /// <summary>
        /// Create
        /// </summary>
        public const string CREATE = "Create";

        /// <summary>
        /// Move
        /// </summary>
        public const string MOVE = "Move";

        /// <summary>
        /// Write all text
        /// </summary>
        public const string WRITE_ALL_TEXT = "WriteAllText";

        /// <summary>
        /// Append all text
        /// </summary>
        public const string APPEND_ALL_TEXT = "AppendAllText";

        /// <summary>
        /// Write all bytes
        /// </summary>
        public const string WRITE_ALL_BYTES = "WriteAllBytes";

        /// <summary>
        /// Write
        /// </summary>
        public const string WRITE = "Write";


        /// <summary>
        /// Asynchronous write
        /// </summary>
        public const string WRITE_ASYNC = "WriteAsync";

        /// <summary>
        /// Delete directory structure
        /// </summary>
        public const string DELETE_DIR_STRUCTURE = "DeleteDirectoryStructure";

        /// <summary>
        /// Move directory
        /// </summary>
        public const string MOVE_DIR = "MoveDir";

        /// <summary>
        /// Delete directory
        /// </summary>
        public const string DELETE_DIR = "DeleteDir";
        
        /// <summary>
        /// Create directory
        /// </summary>
        public const string CREATE_DIR = "CreateDir";
        
        #endregion


        #region "Read-only operations constants"

        /// <summary>
        /// Directory exists
        /// </summary>
        public const string DirExists = "DirExists";

        /// <summary>
        /// Get current directory
        /// </summary>
        public const string GET_CURRENT_DIR = "GetCurrentDir";

        /// <summary>
        /// Read
        /// </summary>
        public const string READ = "Read";


        /// <summary>
        /// Asynchronous read
        /// </summary>
        public const string READ_ASYNC = "ReadAsync";

        /// <summary>
        /// Read all text
        /// </summary>
        public const string READ_ALL_TEXT = "ReadAllText";

        /// <summary>
        /// Read all bytes
        /// </summary>
        public const string READ_ALL_BYTES = "ReadAllBytes";

        /// <summary>
        /// Enumerate files in a directory.
        /// </summary>
        public const string ENUMERATE_FILES = "EnumerateFiles";

        /// <summary>
        /// Enumerate subdirectories in a directory.
        /// </summary>
        public const string ENUMERATE_DIRECTORIES = "EnumerateDirectories";

        /// <summary>
        /// Exists
        /// </summary>
        public const string EXISTS = "Exists";

        /// <summary>
        /// GetFiles
        /// </summary>
        public const string GET_FILES = "GetFiles";

        /// <summary>
        /// GetDirectories
        /// </summary>
        public const string GET_DIRECTORIES = "GetDirectories";

        /// <summary>
        /// GetAccessControl
        /// </summary>
        public const string GET_ACCESS_CONTROL = "GetAccessControl";

        /// <summary>
        /// Open
        /// </summary>
        public const string OPEN = "Open";

        /// <summary>
        /// Directory exists
        /// </summary>
        public const string DIR_EXISTS = "DirExists";

        /// <summary>
        /// Close
        /// </summary>
        public const string CLOSE = "Close";

        #endregion


        #region "Read-only operations list"

        private static List<string> customReadOnlyOperations = null;
        private static readonly object lockObject = new object();


        /// <summary>
        /// List of known read-only file system operations.
        /// </summary>
        private static Lazy<IReadOnlyCollection<string>> mReadOnlyOperations = new Lazy<IReadOnlyCollection<string>>(() => new List<string>
        {
            READ,
            EXISTS,
            GET_FILES, 
            GET_DIRECTORIES, 
            GET_ACCESS_CONTROL,
            OPEN, 
            DIR_EXISTS,
            CLOSE,
            READ_ALL_BYTES,
            READ_ALL_TEXT,
            DIR_EXISTS,
            GET_CURRENT_DIR
        });


        /// <summary>
        /// Collection of known read-only file system operations.
        /// </summary>
        /// <remarks>
        /// Use <see cref="RegisterReadOnlyOperation"/> to extend this list.
        /// </remarks>
        public static IEnumerable<string> ReadOnlyOperations
        {
            get
            {
                var customOperations = customReadOnlyOperations;

                if ((customOperations != null) && (customOperations.Count > 0))
                {
                    return customOperations.Concat(mReadOnlyOperations.Value);
                }

                return mReadOnlyOperations.Value;
            }
        }


        /// <summary>
        /// Registers the given operation as read-only. Extends the list <see cref="ReadOnlyOperations"/>
        /// </summary>
        /// <param name="operation">Operation to register</param>
        public static void RegisterReadOnlyOperation(string operation)
        {
            lock (lockObject)
            {
                var customOperations = (customReadOnlyOperations == null) ? new List<string>() : new List<string>(customReadOnlyOperations);
                customOperations.Add(operation);

                customReadOnlyOperations = customOperations;
            }
        }

        #endregion
    }
}
