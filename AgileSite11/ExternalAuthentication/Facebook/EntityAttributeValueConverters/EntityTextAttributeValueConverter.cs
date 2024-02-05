using System;

using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Converts string values of entity attributes to values compatible with form fields.
    /// </summary>
    public sealed class EntityTextAttributeValueConverter : EntityAttributeValueConverterBase
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityTextAttributeValueConverterBase.
        /// </summary>
        /// <param name="attributeModel">The model of an entity attribute.</param>
        public EntityTextAttributeValueConverter(EntityAttributeModel attributeModel) : base(attributeModel)
        {

        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Computes a value compatible with the specified form field, and returns it.
        /// </summary>
        /// <param name="fieldInfo">The target of value conversion.</param>
        /// <param name="entity">The Facebook API entity</param>
        /// <param name="attributeValueFormatter">The object that provides formatting of entity attribute values.</param>
        /// <returns>A value compatible with the specified form field, if applicable; otherwise, null.</returns>
        public override object GetFormFieldValue(FormFieldInfo fieldInfo, object entity, IEntityAttributeValueFormatter attributeValueFormatter)
        {
            if (IsCompatibleWithFormField(fieldInfo))
            {
                return mAttributeModel.PropertyInfo.GetValue(entity, null) as string;
            }

            return null;
        }


        /// <summary>
        /// Determines whether this converter is compatible with the specified form field.
        /// </summary>
        /// <param name="fieldInfo">The target of value conversion.</param>
        /// <returns>True, if this mapping is compatible with the specified form field; otherwise, false.</returns>
        public override bool IsCompatibleWithFormField(FormFieldInfo fieldInfo)
        {
            switch (fieldInfo.DataType)
            {
                case FieldDataType.Text:
                case FieldDataType.LongText:
                    return true;
            }

            return false;
        }

        #endregion
    }

}