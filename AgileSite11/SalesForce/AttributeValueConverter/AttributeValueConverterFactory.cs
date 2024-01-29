using System;
using System.Collections.Generic;
using System.Linq;
using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides support for creating SalesForce entity attribute value converters.
    /// </summary>
    public sealed class AttributeValueConverterFactory
    {

        #region "Private members"

        private List<AttributeValueConverterFactoryEntry> mEntries;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the factory with default entries.
        /// </summary>
        public AttributeValueConverterFactory()
        {
            mEntries = new List<AttributeValueConverterFactoryEntry>();
            RegisterDefaultEntries();
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Registers a new factory entry.
        /// </summary>
        /// <param name="attributeType">The SalesForce entity attribute value type that the converter supports.</param>
        /// <param name="fieldType">The CMS object field value type that the converter supports.</param>
        /// <param name="activator">The delegate that creates a new instance of the SalesForce entity attribute value converter.</param>
        public void Register(EntityAttributeValueType attributeType, string fieldType, Func<EntityAttributeModel, FormFieldInfo, AttributeValueConverterBase> activator)
        {
            AttributeValueConverterFactoryEntry entry = new AttributeValueConverterFactoryEntry(attributeType, fieldType, activator);
            mEntries.Add(entry);
        }

        /// <summary>
        /// Registers a new factory entry.
        /// </summary>
        /// <param name="attributeType">The SalesForce entity attribute value type that the converter supports.</param>
        /// <param name="fieldTypes">The CMS object field value types that the converter supports.</param>
        /// <param name="activator">The delegate that creates a new instance of the SalesForce entity attribute value converter.</param>
        public void Register(EntityAttributeValueType attributeType, string[] fieldTypes, Func<EntityAttributeModel, FormFieldInfo, AttributeValueConverterBase> activator)
        {
            AttributeValueConverterFactoryEntry entry = new AttributeValueConverterFactoryEntry(attributeType, fieldTypes, activator);
            mEntries.Add(entry);
        }

        /// <summary>
        /// Creates a new instance of the SalesForce entity attribute value converter using the specified attribute model and field info, and returns it.
        /// </summary>
        /// <param name="attributeModel">The SalesForce entity attribute model.</param>
        /// <param name="fieldInfo">The CMS object field info.</param>
        /// <returns>A new instance of the SalesForce entity attribute value converter.</returns>
        public AttributeValueConverterBase CreateAttributeValueConverter(EntityAttributeModel attributeModel, FormFieldInfo fieldInfo)
        {
            AttributeValueConverterFactoryEntry entry = mEntries.LastOrDefault(x => x.AttributeType == attributeModel.Type && x.FieldTypes.Contains(fieldInfo.DataType));
            if (entry == null)
            {
                return null;
            }

            return entry.Activator.Invoke(attributeModel, fieldInfo);
        }

        #endregion

        #region "Private methods"

        private void RegisterDefaultEntries()
        {
            var numberTypes = new[] { FieldDataType.Double, FieldDataType.LongInteger, FieldDataType.Integer, FieldDataType.Decimal };
            var stringTypes = new[] { FieldDataType.Text, FieldDataType.LongText };
            
            Register(EntityAttributeValueType.Boolean, FieldDataType.Boolean, (attributeModel, fieldInfo) => new BooleanAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Date, FieldDataType.DateTime, (attributeModel, fieldInfo) => new DateTimeAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.DateTime, FieldDataType.DateTime, (attributeModel, fieldInfo) => new DateTimeAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Double, numberTypes, (attributeModel, fieldInfo) => new DoubleAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.EmailAddress, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.EncryptedString, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Integer, numberTypes, (attributeModel, fieldInfo) => new IntegerAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Percent, numberTypes, (attributeModel, fieldInfo) => new DoubleAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.PhoneNumber, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.String, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Textarea, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Time, FieldDataType.DateTime, (attributeModel, fieldInfo) => new DateTimeAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Url, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.Picklist, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
            Register(EntityAttributeValueType.MultiPicklist, stringTypes, (attributeModel, fieldInfo) => new StringAttributeValueConverter(attributeModel, fieldInfo));
        }

        #endregion

    }

}