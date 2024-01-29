using System;
using System.Linq;
using System.Xml;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Provides methods for working with separated fields in the file system repository.
    /// </summary>
    internal class SeparatedFieldProcessor : ICustomProcessor
    {
        #region "Variables and Properties"

        /// <summary>
        /// Object for working with file paths in file system repository.
        /// </summary>
        protected RepositoryPathHelper RepositoryPathHelper
        {
            get;
            private set;
        }


        /// <summary>
        /// Object for reading content from the file system.
        /// </summary>
        protected IFileSystemReader FileSystemReader
        {
            get;
            private set;
        }


        /// <summary>
        /// Object for writing content to the file system.
        /// </summary>
        protected IFileSystemWriter FileSystemWriter
        {
            get;
            private set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates new instance of the <see cref="SeparatedFieldProcessor"/>.
        /// </summary>
        /// <param name="repositoryPathHelper">Object for working with file paths in file system repository.</param>
        /// <param name="fileSystemReader">Object for reading content from the file system.</param>
        /// <param name="fileSystemWriter">Object for writing content to the file system.</param>
        public SeparatedFieldProcessor(RepositoryPathHelper repositoryPathHelper, IFileSystemReader fileSystemReader, IFileSystemWriter fileSystemWriter)
        {
            if (repositoryPathHelper == null)
            {
                throw new ArgumentNullException("repositoryPathHelper");
            }

            if (fileSystemReader == null)
            {
                throw new ArgumentNullException("fileSystemReader");
            }

            if (fileSystemReader == null)
            {
                throw new ArgumentNullException("fileSystemWriter");
            }

            RepositoryPathHelper = repositoryPathHelper;
            FileSystemReader = fileSystemReader;
            FileSystemWriter = fileSystemWriter;
        }


        /// <summary>
        /// Merges separated non-binary fields back into the XML document (<paramref name="serializedInfo"/>).
        /// </summary>
        /// <param name="processorResult">Operation result, containing object's type info.</param>
        /// <param name="serializedInfo">Original serialized info. Separated fields are added as new elements to this XML document.</param>
        /// <param name="structuredLocation">Location of object's main file and its additional parts in repository.</param>
        public void PreprocessDeserializedDocument(CustomProcessorResult processorResult, XmlDocument serializedInfo, StructuredLocation structuredLocation)
        {
            var document = serializedInfo.DocumentElement;
            if (document == null)
            {
                return;
            }

            var nonBinaryFields = processorResult
                .TypeInfo
                .ContinuousIntegrationSettings
                .SeparatedFields.Where(x => !x.IsBinaryField);

            foreach (var field in nonBinaryFields)
            {
                var fieldFileName = GetFieldFileName(structuredLocation, field.FieldName);
                if (String.IsNullOrEmpty(fieldFileName))
                {
                    continue;
                }

                var fieldContent = FileSystemReader.ReadString(fieldFileName);
                var existingNode = document[field.FieldName];
                if (existingNode != null)
                {
                    document.RemoveChild(existingNode);
                }

                var node = serializedInfo.CreateElement(field.FieldName);
                node.InnerXml = fieldContent;
                document.AppendChild(node);
            }
        }


        /// <summary>
        /// Merges separated binary fields' data back into the <paramref name="deserializedInfo"/>.
        /// </summary>
        /// <param name="deserializedInfo">Deserialized info.</param>
        /// <param name="structuredLocation">Location of object's main file and its additional parts in repository.</param>
        public void PostprocessDeserializedDocument(BaseInfo deserializedInfo, StructuredLocation structuredLocation)
        {
            var separatedFields = deserializedInfo.TypeInfo.ContinuousIntegrationSettings.SeparatedFields;
            foreach (var field in separatedFields.Where(x => x.IsBinaryField))
            {
                var fieldFile = RepositoryPathHelper.GetSeparatedFieldPath(deserializedInfo, structuredLocation.MainLocation, field);
                if (!structuredLocation.Contains(fieldFile))
                {
                    continue;
                }

                deserializedInfo.SetValue(field.FieldName, FileSystemReader.ReadBytes(fieldFile));
            }
        }


        /// <summary>
        /// Stores content of separated fields to additional files and removes the fields from serialized object's XML.
        /// </summary>
        /// <param name="baseInfo">Base info which will be stored.</param>
        /// <param name="infoRelativePath">Relative path to file in which the given base info is to be stored.</param>
        /// <param name="serializedObject">Serialized <paramref name="baseInfo"/>.</param>
        public void PostprocessSerializedObject(BaseInfo baseInfo, string infoRelativePath, XmlElement serializedObject)
        {
            var separatedFields = baseInfo.TypeInfo.ContinuousIntegrationSettings.SeparatedFields;

            var filesToDelete = from separatedField in separatedFields
                                let relativePath = RepositoryPathHelper.GetSeparatedFieldPath(baseInfo, infoRelativePath, separatedField)
                                let fieldContent = RemoveSeparatedFieldFromSerializedObject(separatedField, serializedObject)
                                let oldFileDeleted = HasSeparatedFieldVaryingExtension(separatedField)
                                                     && DeleteSeparatedFieldFileWithDifferentExtension(relativePath)
                                let newFileWritten = separatedField.IsBinaryField
                                    ? StoreBinaryData(baseInfo, separatedField.FieldName, relativePath)
                                    : StoreFieldContent(relativePath, fieldContent)
                                where !newFileWritten && !oldFileDeleted
                                select relativePath;

            foreach (var relativePath in filesToDelete)
            {
                FileSystemWriter.DeleteFile(relativePath, true);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns file name with extension that belongs to the <paramref name="fieldName"/>.
        /// If location of the <paramref name="fieldName"/> cannot be found within the <see cref="StructuredLocation"/>,
        /// <see langword="null"/> is returned.
        /// </summary>
        private string GetFieldFileName(StructuredLocation structuredLocation, string fieldName)
        {
            var fieldFileNameWithoutExtension = RepositoryPathHelper.GetSeparatedFieldPath(structuredLocation.MainLocation, fieldName);
            return structuredLocation.FirstOrDefault(x => x.StartsWith(fieldFileNameWithoutExtension + ".", StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Removes separated field's content from given serialized object. Removed content is returned.
        /// </summary>
        /// <param name="separatedField">Separated field to remove.</param>
        /// <param name="serializedObject">Serialized object.</param>
        /// <returns>Returns removed separated field's content or null if given field is missing in serialized object.</returns>
        private static string RemoveSeparatedFieldFromSerializedObject(SeparatedField separatedField, XmlElement serializedObject)
        {
            var node = serializedObject[separatedField.FieldName];
            if (node == null)
            {
                return null;
            }

            var fieldContent = node.InnerXml;
            node.RemoveAll();

            if (serializedObject.OwnerDocument != null)
            {
                var comment = serializedObject.OwnerDocument.CreateComment("Content of this field is serialized in separate file.");
                node.AppendChild(comment);
            }

            return fieldContent;
        }


        /// <summary>
        /// Returns <paramref langword="true"/> if <paramref name="separatedField"/> is set to be stored in a file whose extension is determined dynamically (can differ).
        /// </summary>
        /// <param name="separatedField">Separated field for which to examine the extension.</param>
        private static bool HasSeparatedFieldVaryingExtension(SeparatedField separatedField)
        {
            return !String.IsNullOrEmpty(separatedField.FileExtensionFieldName) && (separatedField.FileExtensionFieldName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
        }


        /// <summary>
        /// Deletes separated field file, if <paramref name="newRelativePath"/> of the field within repository
        /// differs from the existing one (i.e. the <paramref name="newRelativePath"/> has different extension than
        /// the existing one).
        /// </summary>
        /// <param name="newRelativePath">Relative repository path of the new separated field file.</param>
        /// <returns>True if file with different extension existed and had to be deleted, false otherwise.</returns>
        private bool DeleteSeparatedFieldFileWithDifferentExtension(string newRelativePath)
        {
            var newExtension = Path.GetExtension(newRelativePath);
            var extensionlessRelativePath = newRelativePath.Substring(0, newRelativePath.Length - newExtension.Length);
            var existingExtension = RepositoryPathHelper.GetSerializationFileExtension(extensionlessRelativePath);
            if ((existingExtension == null) || existingExtension.Equals(newExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                // either no files exist or obtained extension equals current, no deletion needed
                return false;
            }

            FileSystemWriter.DeleteFile(extensionlessRelativePath + existingExtension, true);
            return true;
        }


        /// <summary>
        /// Stores content of separated field to file.
        /// </summary>
        /// <param name="fieldContent">Content to be stored.</param>
        /// <param name="relativePath">Relative path of the file.</param>
        private bool StoreFieldContent(string relativePath, string fieldContent)
        {
            var textContent = fieldContent;
            if (textContent == null)
            {
                return false;
            }

            FileSystemWriter.WriteToFile(relativePath, fieldContent);
            return true;
        }


        /// <summary>
        /// Stores binary content of separated field to file.
        /// </summary>
        /// <param name="baseInfo">Info object the <paramref name="fieldName"/> of should be stored to the <paramref name="relativePath"/>.</param>
        /// <param name="fieldName">Binary data to be stored.</param>
        /// <param name="relativePath">Relative path of the file.</param>
        private bool StoreBinaryData(BaseInfo baseInfo, string fieldName, string relativePath)
        {
            var binaryContent = baseInfo.GetValue(fieldName) as byte[];
            if (binaryContent == null)
            {
                // No data obtained, no file written
                return false;
            }

            FileSystemWriter.WriteToFile(relativePath, binaryContent);
            return true;
        }

        #endregion
    }
}
