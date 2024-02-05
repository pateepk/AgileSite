using System;

using CMS.FormEngine;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Converts values of entity attributes to values compatible with form fields.
    /// </summary>
    public abstract class EntityAttributeValueConverterBase
    {
        #region "Protected members"

        /// <summary>
        /// The model of an entity attribute.
        /// </summary>
        protected EntityAttributeModel mAttributeModel;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityAttributeValueConverterBase.
        /// </summary>
        /// <param name="attributeModel">The model of an entity attribute.</param>
        public EntityAttributeValueConverterBase(EntityAttributeModel attributeModel)
        {
            mAttributeModel = attributeModel;
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
        public abstract object GetFormFieldValue(FormFieldInfo fieldInfo, object entity, IEntityAttributeValueFormatter attributeValueFormatter);
        

        /// <summary>
        /// Determines whether this converter is compatible with the specified form field.
        /// </summary>
        /// <param name="fieldInfo">The target of value conversion.</param>
        /// <returns>True, if this mapping is compatible with the specified form field; otherwise, false.</returns>
        public abstract bool IsCompatibleWithFormField(FormFieldInfo fieldInfo);

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets the attribute value of the specified entity regardless of value nullability, and returns it.
        /// </summary>
        /// <typeparam name="T">The type of entity attribute value.</typeparam>
        /// <param name="entity">The entity to get the value from.</param>
        /// <returns>The attribute value of the specified entity regardless of value nullability.</returns>
        protected Nullable<T> GetValue<T>(object entity) where T : struct
        {
            object value = mAttributeModel.PropertyInfo.GetValue(entity, null);
            if (mAttributeModel.PropertyInfo.PropertyType == typeof(Nullable<T>))
            {
                return value as Nullable<T>;
            }

            return (T)value;
        }

        #endregion
    }
}