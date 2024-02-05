using System;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Membership;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Converts gender values of entity attributes to values compatible with form fields.
    /// </summary>
    public sealed class EntityGenderAttributeValueConverter : EntityAttributeValueConverterBase
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityGenderAttributeValueConverterBase.
        /// </summary>
        /// <param name="attributeModel">The model of an entity attribute.</param>
        public EntityGenderAttributeValueConverter(EntityAttributeModel attributeModel) : base(attributeModel)
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
                Nullable<UserGenderEnum> value = GetValue<UserGenderEnum>(entity);
                if (value != null)
                {
                    switch (fieldInfo.DataType)
                    {
                        case FieldDataType.Integer:
                        case FieldDataType.LongInteger:
                            return value.Value;

                        case FieldDataType.Text:
                        case FieldDataType.LongText:
                            return attributeValueFormatter.Format(value.Value);
                    }
                }
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
                case FieldDataType.Integer:
                case FieldDataType.LongInteger:
                case FieldDataType.Text:
                case FieldDataType.LongText:
                    return true;
            }

            return false;
        }

        #endregion
    }

}