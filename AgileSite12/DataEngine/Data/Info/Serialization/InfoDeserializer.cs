using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using CMS.Helpers;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// De-serialization support for info objects.
    /// </summary>
    internal class InfoDeserializer
    {
        #region "Variables"

        // String representation of the CDATA closing tag
        private const string CDATA_END_TAG = "]]>";

        // Character sets defining new lines in text document. Order is important.
        // We don't want to detect shorter string when there actually is more complex one.
        private readonly string[] mNewLineStrings = { "\r\n", "\n" };

        // Translations
        private TranslationHelper mTranslationHelper;

        #endregion


        #region "Properties"

        /// <summary>
        /// Provides object translation. Used during deserialization to optimize database calls.
        /// </summary>
        /// <remarks>
        /// The <see cref="DataEngine.TranslationHelper.TranslationTable"/> property contains data records that correspond with the database.        
        /// For translation records identified using partially correct parameters (either the code name or GUID is different than the value in the database), 
        /// the actual values are loaded from the database for all fields.        
        /// An index key referencing the correct data is then created from the partial parameters. The index key is used to obtain the record in future calls.
        /// </remarks>
        internal TranslationHelper TranslationHelper
        {
            get
            {
                return mTranslationHelper ?? (mTranslationHelper = new TranslationHelper());
            }
            set
            {
                mTranslationHelper = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates info object from given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">XML element containing all the object data.</param>
        /// <remarks>
        /// <para>
        ///     Deserialized object is then matched with existing object in the database and reference are translated.
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 If matching object exists in the database, it is returned as <see cref="DeserializationResult.DeserializedInfo"/> with values and references
        ///                 that are no excluded (see <see cref="SerializationSettings.ExcludedFieldNames"/>) set based on data provided in <paramref name="element"/>.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 If field serialization failed (see <see cref="DeserializationResultBase.FailedFields"/>) or no object was found,
        ///                 <see cref="DeserializationResult.DeserializedInfo"/>'s ID is set to 0.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     Data that could not be read and references that could not be translated are stored in
        ///     <see cref="DeserializationResultBase.FailedFields"/> and <see cref="DeserializationResultBase.FailedMappings"/> respectively.
        /// </para>
        /// </remarks>
        public DeserializationResult DeserializeObject(XmlElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var infoObject = ModuleManager.GetObject(element.Name, true);
            var serializationSettings = infoObject.TypeInfo.SerializationSettings;
            var deserializationResult = new DeserializationResult(infoObject);

            // Get column names without the excluded ones and prepare deserialization field wrappers
            var fieldWrappers = infoObject.ColumnNames
                                        .Where(name => !serializationSettings.IsExcludedField(name))
                                        .Select(columnName => new DeserializationFieldWrapper<XmlElement>(deserializationResult)
                                        {
                                            FieldName = columnName,
                                            FieldData = element[columnName]
                                        });

            foreach (var field in fieldWrappers)
            {
                object value;

                if (IsForeignKeyField(field, infoObject.TypeInfo))
                {
                    // Handle foreign key translations.
                    value = ReadForeignKey(field);
                }
                else
                {
                    // Handle regular fields.
                    value = ReadValue(field);
                }

                field.SetFieldValue(value);
            }

            // Reset object changes to correctly return original values and detect changes against the existing object. 
            infoObject.Generalized.ResetChanges();

            // If deserialization of the object failed on any field, no matching for existing object needs to be performed (deserialized object is in incorrect state),
            // otherwise attempt to merge (eventually) existing and deserialized object is made.
            return deserializationResult.FailedFields.Any()
                ? deserializationResult
                : MergeDeserializedInfoWithExistingObject(deserializationResult);
        }


        /// <summary>
        /// If info object does not exist in database, <paramref name="deserializedInfoResult"/> is returned without any change.
        /// <para>
        /// Otherwise, existing info object is retrieved from the database and data from <see cref="DeserializationResult.DeserializedInfo"/> property of <paramref name="deserializedInfoResult"/> are copies into the object.
        /// Existing object (with changed data and same <see cref="DeserializationResultBase.FailedFields"/> and <see cref="DeserializationResultBase.FailedMappings"/>) is returned in this case.
        /// </para>
        /// </summary>
        /// <param name="deserializedInfoResult">DeserializationResult obtained by deserialization of the XML on the file system.</param>
        private DeserializationResult MergeDeserializedInfoWithExistingObject(DeserializationResult deserializedInfoResult)
        {
            // Get existing object from the database
            var deserializedInfo = deserializedInfoResult.DeserializedInfo;
            var existingInfo = deserializedInfo.Generalized.GetExisting();

            if (existingInfo == null)
            {
                // New object is being added, nothing to merge it with
                return deserializedInfoResult;
            }

            // Register translation information of this object for future use.
            // Record must be registered before merge. Otherwise incorrect data would be registered (deserialized data are not saved yet).
            RegisterExistingInfo(existingInfo);

            CopySerializableFieldsFromOneInfoToAnother(deserializedInfo, existingInfo);          

            // Create new deserialization result for the existing info
            return new DeserializationResult(existingInfo)
            {
                FailedFields = deserializedInfoResult.FailedFields,
                FailedMappings = deserializedInfoResult.FailedMappings
            };
        }

        /// <summary>
        /// Copies data except the excluded fields (specified in <see cref="ObjectTypeInfo.SerializationSettings"/>)
        /// from <paramref name="sourceInfo"/> to <paramref name="destinationInfo"/>.
        /// </summary>
        private void CopySerializableFieldsFromOneInfoToAnother(BaseInfo sourceInfo, BaseInfo destinationInfo)
        {
            var serializationSettings = sourceInfo.Generalized.TypeInfo.SerializationSettings;
            destinationInfo
                .ColumnNames
                .Where(fieldName => !serializationSettings.IsExcludedField(fieldName))
                .ToList()
                .ForEach(fieldName => destinationInfo.SetValue(fieldName, sourceInfo.GetValue(fieldName)));
        }


        /// <summary>
        /// Registers record for given <paramref name="info"/> into <see cref="TranslationHelper"/>.
        /// </summary>
        private void RegisterExistingInfo(BaseInfo info)
        {
            var generalizedInfo = info.Generalized;
            var typeInfo = info.TypeInfo;
            var objectId = generalizedInfo.ObjectID;

            var record = TranslationHelper.GetRecord(typeInfo.ObjectType, objectId);
            if (record == null)
            {
                TranslationHelper.RegisterRecord(info);
            }
        }


        /// <summary>
        /// Returns true if the field holds reference to another object.
        /// </summary>
        /// <param name="field">Field to check</param>
        /// <param name="typeInfo">Object type info of the deserialized object</param>
        private static bool IsForeignKeyField(DeserializationFieldWrapper<XmlElement> field, ObjectTypeInfo typeInfo)
        {
            var objectType = field.GetReferencedObjectTypeName();
            if (!String.IsNullOrEmpty(objectType))
            {
                return true;
            }

            var dependency = typeInfo.GetDependencyForColumn(field.FieldName);
            return dependency != null;
        }


        /// <summary>
        /// Returns true if the field holds required reference to another object.
        /// </summary>
        /// <param name="field">Field to check</param>
        private static bool IsRequiredDependency(DeserializationFieldWrapper<XmlElement> field)
        {
            var typeInfo = field.DeserializationResult.DeserializedInfo.TypeInfo;
            var dependency = typeInfo.GetDependencyForColumn(field.FieldName);
            if (dependency != null)
            {
                return dependency.DependencyType == ObjectDependencyEnum.Required;
            }

            var referencedObjectType = field.GetReferencedObjectTypeName();

            // Parent, group and site do not have dependency type
            return !String.IsNullOrEmpty(referencedObjectType);
        }


        /// <summary>
        /// Reads regular or common value (including CDATA)
        /// </summary>
        /// <param name="field">Carries all necessary information required for deserialization of the value of <see cref="DeserializationFieldWrapper{XmlElement}.FieldName"/>.</param>
        private object ReadValue(DeserializationFieldWrapper<XmlElement> field)
        {
            var element = field.FieldData;
            if (element == null)
            {
                // Null fields are not serialized.
                return null;
            }

            // Structured field
            if (element.ChildNodes.OfType<XmlElement>().Any())
            {
                // Recursively normalize all CDATA nodes of inner XML
                RemoveCDataWrappers(element);

                // Structured field is just a pretty XML
                return element.InnerXml;
            }

            var cdatas = element.ChildNodes.OfType<XmlCDataSection>().ToArray();
            if (cdatas.Any())
            {
                // Content of CDATA section(s)
                return GetCDataContent(cdatas);
            }

            // Try to convert simple value into a proper type
            Type dataType = field.GetFieldType();
            if (dataType == null)
            {
                throw new InvalidOperationException(String.Format("Data type for field '{0}' could not be recognized.", field.FieldName));
            }

            object result = null;
            try
            {
                result = ConvertToProperType(element.InnerText, dataType);
            }
            catch (InvalidCastException)
            {
                field.FieldDeserializationFailed(element.InnerText);
            }

            return result;
        }


        /// <summary>
        /// Reads foreign key that stored as <see cref="TranslationReference"/>.
        /// </summary>
        /// <remarks>In case the reference is stored as plain integer ID, no DB is check performed.</remarks>
        /// <param name="field">Carries all necessary information required for deserialization of the value of <see cref="DeserializationFieldWrapper{XmlElement}.FieldName"/>.</param>
       /// <returns>ID if the value provided is reference to an object that exists in the DB (reference) or valid integer greater than 0 (plain ID), null otherwise.</returns>
        private int? ReadForeignKey(DeserializationFieldWrapper<XmlElement> field)
        {
            var element = field.FieldData;
            if (element == null)
            {
                // Field element was not provided
                return null;
            }

            if (!element.ChildNodes.OfType<XmlElement>().Any())
            {
                if (ValidationHelper.IsInteger(element.InnerText))
                {
                    // Field element was foreign object ID so no translation is needed
                    return ValidationHelper.GetInteger(element.InnerText, 0);
                }

                if (IsRequiredDependency(field))
                {
                    // Field element was empty or untranslated during serialization
                    field.FieldDeserializationFailed(element.InnerText);
                }

                return null;
            }

            // Field element was translated into a reference during serialization
            int? id = TranslationHelper.TranslateReferenceIntoForeignKey(field);

            return id;
        }


        /// <summary>
        /// Ensures format of inner CDATA sections for element values with complex content.
        /// </summary>
        /// <param name="element">Root of element sub-tree to process</param>
        private void RemoveCDataWrappers(XmlElement element)
        {
            var hasChildElements = false;

            if (element.ChildNodes.Count > 0)
            {
                foreach (var childElement in element.ChildNodes.OfType<XmlElement>())
                {
                    RemoveCDataWrappers(childElement);
                    hasChildElements = true;
                }
            }

            if (!hasChildElements)
            {
                var cdatas = element.ChildNodes.OfType<XmlCDataSection>().ToArray();
                if (cdatas.Any())
                {
                    // Content of CDATA section(s)
                    string content = GetCDataContent(cdatas);

                    // Clear all child nodes
                    element.IsEmpty = true;

                    element.InnerText = content;
                }
            }
        }


        /// <summary>
        /// Returns string with a content of CDATA section(s).
        /// </summary>
        /// <param name="cdatas">Collection of CDATA sections</param>
        private string GetCDataContent(IEnumerable<XmlCDataSection> cdatas)
        {
            var parts = cdatas.Select(cdata =>
            {
                string text = cdata.InnerText;

                // Trim new line at the beginning
                string trimStart = mNewLineStrings.FirstOrDefault(newLineString => text.StartsWith(newLineString, StringComparison.Ordinal));
                if (trimStart != null)
                {
                    text = text.Substring(trimStart.Length);
                }

                // Trim new line at the end
                string trimEnd = mNewLineStrings.FirstOrDefault(newLineString => text.EndsWith(newLineString, StringComparison.Ordinal));
                if (trimEnd != null)
                {
                    text = text.Substring(0, text.Length - trimEnd.Length);
                }

                return text;
            });

            //  Multiple CDATA parts should be joined with CDATA end tag to restore nested CDATA sections
            return String.Join(CDATA_END_TAG, parts);
        }


        /// <summary>
        /// Converts given string value into a proper type.
        /// </summary>
        /// <param name="value">String value for conversion</param>
        /// <param name="type">Data type the <paramref name="value"/> is converted to</param>
        private object ConvertToProperType(string value, Type type)
        {
            object result = value;
            if ((result == null) || (type == typeof(string)))
            {
                return result;
            }

            var dataType = DataTypeManager.GetDataType(type);
            if (dataType != null)
            {
                // Convert string value to a proper type
                result = StringConverter.GetValue(value, dataType);
            }

            return result;
        }

        #endregion
    }
}
