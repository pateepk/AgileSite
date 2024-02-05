using System;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Class provides parameters that are sufficient for a single field deserialization.
    /// </summary>
    /// <remarks>Class is meant for internal use only.</remarks>
    /// <typeparam name="TDataType">Type of object the field's data are stored in (e.g. <see cref="System.Xml.XmlElement"/> or <see cref="TranslationReference"/>).</typeparam>
    internal class DeserializationFieldWrapper<TDataType>
    {
        /// <summary>
        /// Name of the field the in question.
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }


        /// <summary>
        /// <see cref="BaseInfo"/> object containing the field encapsulated with failed deserialization/mapping collections.
        /// </summary>
        public DeserializationResult DeserializationResult
        {
            get;
            private set;
        }


        /// <summary>
        /// Object that holds data for the <see cref="FieldName"/> field.
        /// It might be e.g. <see cref="System.Xml.XmlElement"/> or <see cref="TranslationReference"/> or any other representation of raw data that need to be processed
        /// before it can be stored in the <see cref="FieldName"/> field of the <see cref="Serialization.DeserializationResult.DeserializedInfo"/>.
        /// </summary>
        public TDataType FieldData
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor of the <see cref="DeserializationFieldWrapper{TDataType}"/>.
        /// </summary>
        /// <param name="deserializationResult">Carries both (partially) deserialized info object and results of eventual failures.</param>
        public DeserializationFieldWrapper(DeserializationResult deserializationResult)
        {
            if (deserializationResult == null)
            {
                throw new ArgumentNullException("deserializationResult");
            }
            DeserializationResult = deserializationResult;
        }


        /// <summary>
        /// Stores information about unsuccessful deserialization of <paramref name="rawValue"/> into <see cref="FieldName"/> field.
        /// </summary>
        /// <param name="rawValue">Value that was impossible to convert. Can be null if no value read.</param>
        public void FieldDeserializationFailed(string rawValue)
        {
            DeserializationResult.FailedFields.Add(FieldName, rawValue);
        }


        /// <summary>
        /// Stores information about unsuccessful mapping of the <paramref name="reference"/> into <see cref="FieldName"/> field.
        /// </summary>
        /// <param name="reference">Reference (usually read from the <see cref="FieldData"/>) that the object it references to was not found in the DB.</param>
        public void FieldMappingFailed(TranslationReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }
            
            DeserializationResult.FailedMappings.Add(FieldName, reference);
        }


        /// <summary>
        /// Sets given <paramref name="value"/> to the <see cref="FieldName"/> field of <see cref="Serialization.DeserializationResult.DeserializedInfo"/>.
        /// </summary>
        /// <param name="value">Value to be set to the <see cref="Serialization.DeserializationResult.DeserializedInfo"/>.</param>
        public void SetFieldValue(object value)
        {
            DeserializationResult.DeserializedInfo.SetValue(FieldName, value);
        }


        /// <summary>
        /// Gets value of the <see cref="FieldName"/> field of <see cref="Serialization.DeserializationResult.DeserializedInfo"/>.
        /// </summary>
        public object GetFieldValue()
        {
            return DeserializationResult.DeserializedInfo.GetValue(FieldName);
        }


        /// <summary>
        /// Returns object type of the <see cref="FieldName"/> field.
        /// </summary>
        public Type GetFieldType()
        {
            return DeserializationResult.DeserializedInfo.Generalized.GetColumnType(FieldName);
        }


        /// <summary>
        /// Returns name of the object type that is referenced by the <see cref="FieldName"/> field.
        /// If the field does not hold reference to another object, null is returned.
        /// </summary>
        public string GetReferencedObjectTypeName()
        {
            return DeserializationResult.DeserializedInfo.TypeInfo.GetObjectTypeForColumn(FieldName);
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return String.Format(
                "FieldName: {0}, ObjectType: {1}, Field data: {2}",
                FieldName,
                DeserializationResult.DeserializedInfo.TypeInfo.ObjectType,
                FieldData);
        }
    }
}
