using System;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class representing file operation.
    /// </summary>
    [Serializable]
    public class FileOperation
    {
        #region "Variables"

        private string mKey;

        /// <summary>
        /// Object type.
        /// </summary>
        public readonly string ObjectType = null;

        /// <summary>
        /// Source file.
        /// </summary>
        public readonly string SourcePath = null;

        /// <summary>
        /// Destination file.
        /// </summary>
        public readonly string DestinationPath = null;

        /// <summary>
        /// Operation to process.
        /// </summary>
        public readonly FileOperationEnum Operation = FileOperationEnum.None;

        /// <summary>
        /// Result of the operation.
        /// </summary>
        public FileResultEnum Result = FileResultEnum.Unknown;

        /// <summary>
        /// Parameter type
        /// </summary>
        public readonly FileOperationParamaterTypeEnum ParameterType = FileOperationParamaterTypeEnum.None;

        /// <summary>
        /// Operation parameter.
        /// </summary>
        public readonly string Parameter = null;


        /// <summary>
        /// Operation key to detect duplicities
        /// </summary>
        public string Key
        {
            get
            {
                return mKey ?? (mKey = GetKey());
            }
        }
        
        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates the file operation.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destPath">Destination path</param>
        /// <param name="operation">File operation</param>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="parameter">Operation parameter</param>
        public FileOperation(string objectType, string sourcePath, string destPath, FileOperationEnum operation, FileOperationParamaterTypeEnum parameterType = FileOperationParamaterTypeEnum.None, string parameter = null)
        {
            ObjectType = objectType;
            SourcePath = sourcePath;
            DestinationPath = destPath;
            Operation = operation;
            ParameterType = parameterType;
            Parameter = parameter;
        }

        #endregion


        #region "Private methods"
        
        /// <summary>
        /// Gets a unique key to identify the operation
        /// </summary>
        private string GetKey()
        {
            return String.Join("|", ObjectType ?? String.Empty, SourcePath, DestinationPath, Operation, Parameter, ParameterType);
        }

        #endregion
    }
}