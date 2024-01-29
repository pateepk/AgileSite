using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

using CMS.Helpers;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Class provides extensions for <see cref="TranslationHelper"/>, enabling it to work with <see cref="TranslationReference"/>,
    /// <see cref="TranslationReference"/> serialized to XML (through <see cref="DeserializationFieldWrapper{TDataType}"/>)
    /// and unsuccessful <see cref="TranslationReference"/>s stored in <see cref="DeserializationResult"/>
    /// </summary>
    public static class TranslationHelperForeignKeyExtensions
    {
        internal const string STRING_ID_FIELD_SEPARATOR = ",";

        /// <summary>
        /// Returns true when ID was obtained or reference was null.
        /// When returned value is false, then requested reference object is missing and it's ID can't be obtained.
        /// </summary>
        /// <remarks>
        /// The <see cref="TranslationHelper.TranslationTable"/> property contains data records that correspond with the database.        
        /// For translation records identified using partially correct parameters (either the code name or GUID is different than the value in the database), 
        /// the actual values are loaded from the database for all fields.        
        /// An index key referencing the correct data is then created from the partial parameters. The index key is used to obtain the record in future calls.
        /// </remarks>
        /// <param name="translationHelper"><see cref="TranslationHelper"/> to work with.</param>
        /// <param name="reference">Information necessary to obtain the ID of an object.</param>
        /// <param name="id">ID of specified object.</param>
        internal static bool TryGetForeignID(this TranslationHelper translationHelper, TranslationReference reference, out int id)
        {
            id = translationHelper.GetForeignID(reference);
            return (reference == null) || (id != 0);
        }


        /// <summary>
        /// Returns the ID of specified object.
        /// </summary>
        /// <remarks>
        /// The <see cref="TranslationHelper.TranslationTable"/> property contains data records that correspond with the database.        
        /// For translation records identified using partially correct parameters (either the code name or GUID is different than the value in the database), 
        /// the actual values are loaded from the database for all fields.        
        /// An index key referencing the correct data is then created from the partial parameters. The index key is used to obtain the record in future calls.
        /// </remarks>
        /// <param name="translationHelper"><see cref="TranslationHelper"/> to work with.</param>
        /// <param name="reference">Information necessary to obtain the ID of an object.</param>
        public static int GetForeignID(this TranslationHelper translationHelper, TranslationReference reference)
        {
            if (reference == null)
            {
                return 0;
            }

            // Recursively translate parent and  group and site ID
            int parentId, groupId, siteId;
            if (!translationHelper.TryGetForeignID(reference.Parent, out parentId)
                || !translationHelper.TryGetForeignID(reference.Group, out groupId)
                || !translationHelper.TryGetForeignID(reference.Site, out siteId))
            {
                return 0;
            }

            var parameters = new TranslationParameters(reference.ObjectType)
            {
                Guid = reference.GUID,
                CodeName = reference.CodeName,
                ParentId = parentId,
                GroupId = groupId
            };

            int id = translationHelper.GetIDWithFallback(parameters, siteId);
            return id;
        }


        /// <summary>
        /// Translates reference of an object specified in <see cref="DeserializationFieldWrapper{XmlElement}.FieldData"/> to ID of the object present in DB,
        /// provided the element containing serialized <see cref="TranslationReference"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="TranslationHelper.TranslationTable"/> property contains data records that correspond with the database.        
        /// For translation records identified using partially correct parameters (either the code name or GUID is different than the value in the database), 
        /// the actual values are loaded from the database for all fields.        
        /// An index key referencing the correct data is then created from the partial parameters. The index key is used to obtain the record in future calls.
        /// </remarks>
        /// <param name="translationHelper"><see cref="TranslationHelper"/> to work with.</param>
        /// <param name="field">All information necessary to interpret an XML element.</param>
        /// <returns>An integer greater than 0 if referenced object found, null otherwise.</returns>
        internal static int? TranslateReferenceIntoForeignKey(this TranslationHelper translationHelper, DeserializationFieldWrapper<XmlElement> field)
        {
            // Field element is translated
            var reference = translationHelper.TranslationReferenceLoader.LoadFromElement(field.FieldData);

            if (reference == null)
            {
                // Field element could not be deserialized into valid reference
                field.FieldDeserializationFailed(field.FieldData.InnerText);
                return null;
            }

            var referenceField = new DeserializationFieldWrapper<TranslationReference>(field.DeserializationResult)
            {
                FieldName = field.FieldName,
                FieldData = reference
            };
            return translationHelper.TranslateReferenceIntoForeignKey(referenceField);
        }


        internal static DataRow GetTranslationRecord(this TranslationHelper translationHelper, string objectType, int objectId)
        {
            if (translationHelper == null)
            {
                throw new ArgumentNullException("translationHelper");
            }

            var record = translationHelper.GetRecord(objectType, objectId);

            if (record != null)
            {
                // Translation record already registered
                return record;
            }

            var ids = new List<int> { objectId };

            translationHelper.RegisterRecords(ids, objectType, TranslationHelper.AUTO_SITENAME);

            record = translationHelper.GetRecord(objectType, objectId);

            return record;
        }


        /// <summary>
        /// Translates reference of an object specified in <see cref="DeserializationFieldWrapper{TranslationReference}.FieldData"/> to ID of the object present in DB.
        /// </summary>
        /// <param name="translationHelper"><see cref="TranslationHelper"/> to work with.</param>
        /// <param name="field">All information necessary to interpret a reference.</param>
        /// <returns>An integer greater than 0 if referenced object found, null otherwise.</returns>
        private static int? TranslateReferenceIntoForeignKey(this TranslationHelper translationHelper, DeserializationFieldWrapper<TranslationReference> field)
        {
            int id = translationHelper.GetForeignID(field.FieldData);
            if (id > 0)
            {
                // Valid ID of existing object obtained
                return id;
            }

            // Invalid ID obtained
            field.FieldMappingFailed(field.FieldData);
            return null;
        }


        /// <summary>
        /// Appends <paramref name="id"/> to the <paramref name="field"/> that is of <see cref="String"/> type.
        /// <para>If there is empty or <see langword="null"/> value present in the field, new <paramref name="id"/> set as single value.</para>
        /// <para>If there is non-empty value present in the field, <paramref name="separator"/> and new <paramref name="id"/> is appended.</para>
        /// </summary>
        /// <param name="field">The field the <paramref name="id"/> was resolved for.</param>
        /// <param name="id">Resolved ID that should be appended into the <paramref name="field"/>.</param>
        /// <param name="separator">Separates individual <paramref name="id"/>s in the <paramref name="field"/>.</param>
        private static void SetStringId(DeserializationFieldWrapper<TranslationReference> field, int id, string separator = STRING_ID_FIELD_SEPARATOR)
        {
            var currentValue = ValidationHelper.GetString(field.GetFieldValue(), String.Empty);
            if (String.IsNullOrEmpty(currentValue))
            {
                // First item for the field
                field.SetFieldValue(id);
            }
            else
            {
                // Any other item to be appended
                field.SetFieldValue(currentValue + separator + id);
            }
        }


        /// <summary>
        /// Creates <see cref="DeserializationFieldWrapper{TDataType}"/> for each <see cref="DeserializationResultBase.FailedMappings"/> present
        /// in <paramref name="originalResult"/> and returns them as a collection. Created wrappers reference <paramref name="newResult"/>, so
        /// any operation upon the wrapper will be reflected in the <paramref name="newResult"/> (and not the <paramref name="originalResult"/>).
        /// </summary>
        /// <param name="originalResult">Deserialization result provided by deserialization result.</param>
        /// <param name="newResult">Newly created result where only newly failed fields will be reflected.</param>
        private static IEnumerable<DeserializationFieldWrapper<TranslationReference>> GetFieldWrappersForNewResult(DeserializationResult originalResult, DeserializationResult newResult)
        {
            var fieldWrappers = originalResult
                .FailedMappings
                .Select(failedMapping => new DeserializationFieldWrapper<TranslationReference>(newResult)
                {
                    FieldName = failedMapping.FieldName,
                    FieldData = failedMapping.TranslationReference
                });

            return fieldWrappers;
        }


        /// <summary>
        /// Store provided <paramref name="id"/> in <paramref name="field"/> wrapper (that stores in the deserialized <see cref="BaseInfo"/>).
        /// </summary>
        /// <param name="field">Wrapper that provides operation on <see cref="DeserializationResult.DeserializedInfo"/> in context of <see cref="DeserializationFieldWrapper{TDataType}.FieldName"/>.</param>
        /// <param name="id">Identifier of object that was carried through serialization process only as a reference and was identified at the destination location.</param>
        private static void StoreValidId(DeserializationFieldWrapper<TranslationReference> field, int id)
        {
            if (field.GetFieldType() == typeof(string))
            {
                SetStringId(field, id);
            }
            else
            {
                field.SetFieldValue(id);
            }
        }


        /// <summary>
        /// Tries to obtain ID for given <paramref name="field"/>'s reference. If succeeds, stores ID in resulting info object (see <see cref="StoreValidId"/>),
        /// otherwise appends field into <paramref name="field"/>'s <see cref="DeserializationFieldWrapper{TDataType}.DeserializationResult"/>.
        /// </summary>
        /// <param name="translationHelper">Instance of translation helper (in ideal case pre-filled with all existing objects of referenced type).</param>
        /// <param name="field">Wrapper that provides operation on <see cref="DeserializationResult"/> and carries <see cref="TranslationReference"/> in context of <see cref="DeserializationFieldWrapper{TDataType}.FieldName"/>.</param>
        private static void TryTranslateField(TranslationHelper translationHelper, DeserializationFieldWrapper<TranslationReference> field)
        {
            // Failed reference is stored in field's Deserialization results
            int? id = translationHelper.TranslateReferenceIntoForeignKey(field);

            if (id.HasValue)
            {
                // Translation from reference to ID was successful
                StoreValidId(field, id.Value);
            }
        }


        /// <summary>
        /// Returns new <see cref="DeserializationResult"/> with <see cref="DeserializationResult.DeserializedInfo"/> and <see cref="DeserializationResultBase.FailedFields"/> 
        /// from the <paramref name="originalResult"/>, yet brand new collection of <see cref="DeserializationResultBase.FailedMappings"/> where there are only fields which
        /// translation failed again.
        /// </summary>
        /// <param name="translationHelper"><see cref="TranslationHelper"/> to work with.</param>
        /// <param name="originalResult">Existing <see cref="DeserializationResult"/>.</param>
        internal static DeserializationResult TranslateFailedReferences(this TranslationHelper translationHelper, DeserializationResult originalResult)
        {
            // Create new result, copying existing failed fields only
            var newResult = new DeserializationResult(originalResult.DeserializedInfo, originalResult.FailedFields);

            var fieldWrappers = GetFieldWrappersForNewResult(originalResult, newResult);
            foreach (var field in fieldWrappers)
            {
                TryTranslateField(translationHelper, field);
            }

            return newResult;
        }
    }
}
