using System;

using CMS.Membership;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Creates entity attribute value converters corresponding to entity attribute models.
    /// </summary>
    public sealed class EntityAttributeValueConverterFactory : IEntityAttributeValueConverterFactory
    {
        #region "Public methods"

        /// <summary>
        /// Creates an entity attribute value converter that corresponds to the specified entity attribute model.
        /// </summary>
        /// <param name="attributeModel">The entity attribute model.</param>
        /// <returns>An entity attribute value converter that corresponds to the specified entity attribute model, if applicable; otherwise, null.</returns>
        public EntityAttributeValueConverterBase CreateConverter(EntityAttributeModel attributeModel)
        {
            if (attributeModel.AttributeValueConverterType != null)
            {
                return Activator.CreateInstance(attributeModel.AttributeValueConverterType, attributeModel) as EntityAttributeValueConverterBase;
            }

            Type type = attributeModel.PropertyInfo.PropertyType;

            // Unwrap nullable type
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }
            
            // Check supported types
            if (type == typeof(Boolean))
            {
                return new EntityBooleanAttributeValueConverter(attributeModel);
            }
            if (type == typeof(Int32))
            {
                return new EntityIntegerAttributeValueConverter(attributeModel);
            }
            if (type == typeof(DateTime))
            {
                return new EntityDateTimeAttributeValueConverter(attributeModel);
            }
            if (type == typeof(String))
            {
                return new EntityTextAttributeValueConverter(attributeModel);
            }
            if (type == typeof(UserGenderEnum))
            {
                return new EntityGenderAttributeValueConverter(attributeModel);
            }
            
            return null;
        }

        #endregion
    }

}