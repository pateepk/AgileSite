using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

using CMS.Helpers;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Collection of files for processing.
    /// </summary>
    [Serializable]
    public class FileOperationCollection : ISerializable
    {
        #region "Variables"

        private readonly List<FileOperation> mOperationList = new List<FileOperation>();
        private readonly HashSet<string> mOperationKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Properties"
        
        /// <summary>
        /// Count of the file operations.
        /// </summary>
        public int Count
        {
            get
            {
                return mOperationList.Count;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the FileOperationCollection class.
        /// </summary>
        public FileOperationCollection()
        {

        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds the file operation to the list.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destPath">Destination path</param>
        /// <param name="operation">Operation</param>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="parameter">Operation parameter</param>
        /// <returns>True if the operation was added.</returns>
        public bool Add(string objectType, string sourcePath, string destPath, FileOperationEnum operation, FileOperationParamaterTypeEnum parameterType = FileOperationParamaterTypeEnum.None, string parameter = null)
        {
            var op = new FileOperation(objectType, sourcePath, destPath, operation, parameterType, parameter);

            return Add(op);
        }


        /// <summary>
        /// Adds the file operation to the list.
        /// </summary>
        /// <param name="op">Operation to add</param>
        public bool Add(FileOperation op)
        {
            if (mOperationKeys.Add(op.Key))
            {
                mOperationList.Add(op);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Removes the file operation from the list.
        /// </summary>
        /// <param name="operation">File operation</param>
        public void Remove(FileOperation operation)
        {
            mOperationList.Remove(operation);
            mOperationKeys.Remove(operation.Key);
        }


        /// <summary>
        /// Indicates if the operation already exists.
        /// </summary>
        /// <param name="operation">File operation</param>
        public bool Contains(FileOperation operation)
        {
            return mOperationKeys.Contains(operation.Key);
        }


        /// <summary>
        /// Sets the file operation result.
        /// </summary>
        /// <param name="operation">File operation</param>
        /// <param name="result">Operation result</param>
        public void SetResult(FileOperation operation, FileResultEnum result)
        {
            // Change result
            operation.Result = result;

            // Add file operation
            Add(operation);
        }


        /// <summary>
        /// Gets the file operation result list.
        /// </summary>
        /// <param name="result">Operation result</param>
        public string GetResultList(FileResultEnum result)
        {
            string message = null;

            foreach (var operation in mOperationList)
            {
                // Filter operations
                if ((operation.Result == result) || (result == FileResultEnum.Unknown))
                {
                    switch (operation.Operation)
                    {
                        case FileOperationEnum.CopyFile:
                            // Log permissions error
                            if (operation.Result == FileResultEnum.PermissionsError)
                            {
                                message += String.Format(ResHelper.GetAPIString("ImportSite.FileCopyWarning", "File '{0}' to '{1}'."), operation.SourcePath, operation.DestinationPath) + "\n";
                            }
                            break;

                        case FileOperationEnum.CopyDirectory:
                            // Log permissions error
                            if (operation.Result == FileResultEnum.PermissionsError)
                            {
                                message += String.Format(ResHelper.GetAPIString("ImportSite.DirectoryCopyWarning", "Folder '{0}' to '{1}'."), operation.SourcePath, operation.DestinationPath) + "\n";
                            }
                            break;
                    }
                }
            }
            return message;
        }


        /// <summary>
        /// Clear the collection
        /// </summary>
        public void Clear()
        {
            mOperationList.Clear();
            mOperationKeys.Clear();
        }


        /// <summary>
        /// Gets the operation on specific index.
        /// </summary>
        /// <param name="index">Operation index to get</param>
        public FileOperation this[int index]
        {
            get
            {
                // Out of range
                if ((index < 0) || (index >= mOperationList.Count))
                {
                    return null;
                }

                return mOperationList[index];
            }
        }

        #endregion


        #region "Serialization"

        /// <summary>
        /// Initializes a new instance of the FileOperationCollection class using the specified serialization info.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        public FileOperationCollection(SerializationInfo info, StreamingContext context)
        {
            mOperationList = (List<FileOperation>)info.GetValue("Operations", typeof(List<FileOperation>));

            // Rebuild the keys from the list of operations
            mOperationKeys = new HashSet<string>(mOperationList.Select(op => op.Key), StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Serializes this object using the specified serialization info.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Operations", mOperationList);
        }

        #endregion
    }
}