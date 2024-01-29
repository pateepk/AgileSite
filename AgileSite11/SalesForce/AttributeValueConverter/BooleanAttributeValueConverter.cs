using System;

using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.SalesForce
{
    /// <summary>
    /// Provides an implementation of the Boolean value converter.
    /// </summary>
    public sealed class BooleanAttributeValueConverter : AttributeValueConverterBase
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the Boolean value converter using the specified attribute model and field info.
        /// </summary>
        /// <param name="attributeModel">The entity attribute model to convert value to.</param>
        /// <param name="fieldInfo">The form field info to convert value from.</param>
        public BooleanAttributeValueConverter(EntityAttributeModel attributeModel, FormFieldInfo fieldInfo) : base(attributeModel, fieldInfo)
        {

        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Assigns the value from the specified CMS object field to the specified SalesForce entity attribute.
        /// </summary>
        /// <param name="entity">A SalesForce entity.</param>
        /// <param name="baseInfo">A CMS object.</param>
        public override void Convert(Entity entity, BaseInfo baseInfo)
        {
            switch (mFieldInfo.DataType)
            {
                case FieldDataType.Boolean:
                    ConvertBoolean(entity, baseInfo);
                    break;
                default:
                    throw new Exception(GetUnsupportedFieldTypeMessage(mFieldInfo.DataType));
            }
        }

        #endregion

        #region "Private methods"

        private void ConvertBoolean(Entity entity, BaseInfo baseInfo)
        {
            object value = baseInfo.GetValue(mFieldInfo.Name);
            if (value != null || mAttributeModel.IsNullable)
            {
                entity[mAttributeModel.Name] = baseInfo.GetValue(mFieldInfo.Name);
            }
        }

        #endregion
    }
}