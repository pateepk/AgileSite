using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.SalesForce
{
    /// <summary>
    /// Provides an implementation of the String value converter.
    /// </summary>
    public sealed class StringAttributeValueConverter : AttributeValueConverterBase
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the String value converter using the specified attribute model and field info.
        /// </summary>
        /// <param name="attributeModel">The entity attribute model to convert value to.</param>
        /// <param name="fieldInfo">The form field info to convert value from.</param>
        public StringAttributeValueConverter(EntityAttributeModel attributeModel, FormFieldInfo fieldInfo) : base(attributeModel, fieldInfo)
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
            if (!DataTypeManager.IsString(TypeEnum.Field, mFieldInfo.DataType))
            {
                throw new Exception(GetUnsupportedFieldTypeMessage(mFieldInfo.DataType));
            }
            
            ConvertText(entity, baseInfo);
        }

        /// <summary>
        /// Creates a list of compatibility problems that might occur during the value conversion, and returns it.
        /// </summary>
        /// <returns>A list of compatibility problems that might occur during the value conversion.</returns>
        public override List<string> GetCompatibilityWarnings()
        {
            List<string> warnings = base.GetCompatibilityWarnings();
            if (mFieldInfo.Size > mAttributeModel.Length)
            {
                string format = ResHelper.GetString("sf.attributecompatibility.tooshort");
                warnings.Add(String.Format(format, mFieldInfo.Size, mAttributeModel.Length));
            }

            return warnings;
        }

        #endregion

        #region "Private methods"

        private void ConvertText(Entity entity, BaseInfo baseInfo)
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