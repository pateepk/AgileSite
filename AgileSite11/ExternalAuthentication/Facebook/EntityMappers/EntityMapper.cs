using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Provides mapping of Facebook API entities to CMS objects.
    /// </summary>
    /// <typeparam name="TEntity">The type of Facebook API entity.</typeparam>
    /// <typeparam name="TInfo">The type of CMS object.</typeparam>
    public sealed class EntityMapper<TEntity, TInfo> : IEntityMapper<TEntity, TInfo> where TEntity : class where TInfo : BaseInfo
    {

        #region "Private members"

        /// <summary>
        /// The model of Facebook API entity.
        /// </summary>
        private EntityModel mEntityModel;
        
        
        /// <summary>
        /// The object that provides form info objects suitable for mapping.
        /// </summary>
        private IFormInfoProvider mFormInfoProvider;
        
        
        /// <summary>
        /// The object that creates entity attribute value converters.
        /// </summary>
        private IEntityAttributeValueConverterFactory mAttributeValueConverterFactory;
        
        
        /// <summary>
        /// The objects that provides formatting of entity attribute values.
        /// </summary>
        private IEntityAttributeValueFormatter mAttributeValueFormatter;


        /// <summary>
        /// A collection of form info objects with object type names as keys.
        /// </summary>
        private Dictionary<string, FormInfo> mFormInfoCache = new Dictionary<string, FormInfo>();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityMapper class.
        /// </summary>
        /// <param name="entityModel">The model of Facebook API entity.</param>
        /// <param name="formInfoProvider">The object that provides form info objects suitable for mapping.</param>
        /// <param name="attributeValueConverterFactory">The object that creates entity attribute value converters.</param>
        /// <param name="attributeValueFormatter">The objects that provides formatting of entity attribute values.</param>
        public EntityMapper(EntityModel entityModel, IFormInfoProvider formInfoProvider, IEntityAttributeValueConverterFactory attributeValueConverterFactory, IEntityAttributeValueFormatter attributeValueFormatter)
        {
            mEntityModel = entityModel;
            mFormInfoProvider = formInfoProvider;
            mAttributeValueConverterFactory = attributeValueConverterFactory;
            mAttributeValueFormatter = attributeValueFormatter;
        }

        #endregion


        #region "IEntityMapper methods"

        /// <summary>
        /// Updates CMS object with data from Facebook API entity.
        /// </summary>
        /// <param name="entity">The Facebook API entity.</param>
        /// <param name="info">The CMS object.</param>
        /// <param name="mapping">The mapping between a Facebook API entity and a CMS object.</param>
        public void Map(TEntity entity, TInfo info, EntityMapping mapping)
        {
            foreach (EntityMappingItem mappingItem in mapping.Items)
            {
                EntityMappingTask task = CreateEntityMappingTask(mappingItem, info);
                if (task != null)
                {
                    object currentValue = info.GetValue(task.FieldInfo.Name);
                    if (IsEmptyValue(currentValue))
                    {
                        EntityAttributeValueConverterBase attributeValueConverter = mAttributeValueConverterFactory.CreateConverter(task.AttributeModel);
                        if (attributeValueConverter != null)
                        {
                            object value = attributeValueConverter.GetFormFieldValue(task.FieldInfo, entity, mAttributeValueFormatter);
                            if (value != null)
                            {
                                SetValue(task, value);
                            }
                        }
                    }
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates a task that contains information required for mapping, and returns it.
        /// </summary>
        /// <param name="item">The mapping item.</param>
        /// <param name="info">The CMS object.</param>
        /// <returns>A task that contains information required for mapping, if found; otherwise, null.</returns>
        private EntityMappingTask CreateEntityMappingTask(EntityMappingItem item, BaseInfo info)
        {
            try
            {
                string fieldName = item.FieldName;
                if (fieldName.Contains('.'))
                {
                    string[] tokens = fieldName.Split('.');
                    for (int i = 0; i < tokens.Length - 1; i++)
                    {
                        PropertyInfo propertyInfo = info.GetType().GetProperty(tokens[i], BindingFlags.Instance | BindingFlags.Public);
                        info = propertyInfo.GetValue(info, null) as BaseInfo;
                    }
                    fieldName = tokens.Last();
                }
                FormInfo formInfo = GetFormInfo(info);
                FormFieldInfo fieldInfo = formInfo.GetFormField(fieldName);
                EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(item.AttributeName);
                if (fieldInfo == null || attributeModel == null)
                {
                    return null;
                }

                return new EntityMappingTask
                {
                    Info = info,
                    FieldInfo = fieldInfo,
                    AttributeModel = attributeModel
                };
            }
            catch
            {
                return null;
            }
        }

        
        /// <summary>
        /// Retrieves an instance of form info suitable for mapping, and returns it.
        /// </summary>
        /// <param name="info">The CMS object to retrieve form info for.</param>
        /// <returns>An instance of form info suitable for mapping, if found; otherwise, null.</returns>
        private FormInfo GetFormInfo(BaseInfo info)
        {
            FormInfo formInfo = null;
            if (!mFormInfoCache.TryGetValue(info.TypeInfo.ObjectType, out formInfo))
            {
                formInfo = mFormInfoProvider.GetFormInfo(info);
                mFormInfoCache.Add(info.TypeInfo.ObjectType, formInfo);
            }

            return formInfo;
        }

        
        /// <summary>
        /// Sets the CMS object value.
        /// </summary>
        /// <param name="task">A task that contains information required for mapping.</param>
        /// <param name="value">A CMS object value.</param>
        private void SetValue(EntityMappingTask task, object value)
        {
            if (DataTypeManager.IsString(TypeEnum.Field, task.FieldInfo.DataType))
            {
                string text = value.ToString();
                if (task.FieldInfo.Size > 0 && text.Length > task.FieldInfo.Size)
                {
                    text = text.Substring(0, task.FieldInfo.Size);
                }
                task.Info.SetValue(task.FieldInfo.Name, text);
            }
            else
            {
                task.Info.SetValue(task.FieldInfo.Name, value);
            }
        }


        /// <summary>
        /// Indicates whether the given value is null or empty.
        /// </summary>
        /// <param name="value">A value to test.</param>
        private bool IsEmptyValue(object value)
        {
            return DataHelper.IsEmpty(value) || DateTime.MinValue.Equals(value);
        }

        #endregion


        #region "Private classes"

        /// <summary>
        /// Represents information required for mapping between a single Facebook API entity attribute and CMS object field.
        /// </summary>
        private class EntityMappingTask
        {

            /// <summary>
            /// The CMS object field info.
            /// </summary>
            public FormFieldInfo FieldInfo;
            

            /// <summary>
            /// The CMS object.
            /// </summary>
            public BaseInfo Info;
            

            /// <summary>
            /// The Facebook API entity attribute model.
            /// </summary>
            public EntityAttributeModel AttributeModel;

        }

        #endregion

    }

}