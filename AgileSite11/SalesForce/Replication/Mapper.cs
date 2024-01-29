using System;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Base;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides transformation of CMS objects to SalesForce entities using the specified mapping.
    /// </summary>
    public abstract class Mapper
    {

        #region "Protected members"

        /// <summary>
        /// A factory that creates instances of entity attribute value converters.
        /// </summary>
        protected AttributeValueConverterFactory mAttributeValueConverterFactory;
        
        /// <summary>
        /// A mapping of CMS objects to SalesForce entities.
        /// </summary>
        protected Mapping mMapping;
        
        /// <summary>
        /// A model of SalesForce entity.
        /// </summary>
        protected EntityModel mEntityModel;
        
        /// <summary>
        /// A form info of CMS object.
        /// </summary>
        protected FormInfo mFormInfo;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the ContactMapper class.
        /// </summary>
        /// <param name="attributeValueConverterFactory">A factory that creates instances of entity attribute value converters.</param>
        /// <param name="mapping">A mapping of CMS objects to SalesForce entities.</param>
        /// <param name="entityModel">A model of SalesForce entity.</param>
        /// <param name="formInfo">A form info of CMS object.</param>
        public Mapper(AttributeValueConverterFactory attributeValueConverterFactory, Mapping mapping, EntityModel entityModel, FormInfo formInfo)
        {
            mAttributeValueConverterFactory = attributeValueConverterFactory;
            mMapping = mapping;
            mEntityModel = entityModel;
            mFormInfo = formInfo;
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Creates a new instance of the SalesForce entity from the specified CMS object, and returns it.
        /// </summary>
        /// <param name="baseInfo">The object to transform.</param>
        /// <returns>A new instance of the SalesForce entity.</returns>
        public Entity Map(BaseInfo baseInfo)
        {
            Entity entity = mEntityModel.CreateEntity();
            foreach (MappingItem item in mMapping.Items)
            {
                switch (item.SourceType)
                {
                    case MappingItemSourceTypeEnum.MetaField:
                        MapMetaField(item, baseInfo, entity);
                        break;
                    case MappingItemSourceTypeEnum.Field :
                        MapField(item, baseInfo, entity);
                        break;
                    case MappingItemSourceTypeEnum.PicklistEntry:
                        MapPicklistValue(item, baseInfo, entity);
                        break;
                    default :
                        throw new Exception("[Mapper.Map]: Unsupported source type.");
                }
            }
            return entity;
        }

        #endregion

        #region "Abstract methods"

        /// <summary>
        /// Maps meta property of CMS object to SalesForce entity field.
        /// </summary>
        /// <param name="item">An item that maps meta property of CMS object.</param>
        /// <param name="baseInfo">The object to transform.</param>
        /// <param name="entity">The SalesForce entity to update.</param>
        protected abstract void MapMetaField(MappingItem item, BaseInfo baseInfo, Entity entity);

        #endregion

        #region "Private methods"

        private void MapField(MappingItem item, BaseInfo baseInfo, Entity entity)
        {
            EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(item.AttributeName);
            FormFieldInfo fieldInfo = mFormInfo.GetFormField(item.SourceName);
            if (attributeModel != null && fieldInfo != null)
            {
                AttributeValueConverterBase attributeValueConverter = mAttributeValueConverterFactory.CreateAttributeValueConverter(attributeModel, fieldInfo);
                if (attributeValueConverter != null)
                {
                    attributeValueConverter.Convert(entity, baseInfo);
                }
            }
        }

        private void MapPicklistValue(MappingItem item, BaseInfo baseInfo, Entity entity)
        {
            EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(item.AttributeName);
            if (attributeModel != null)
            {
                entity[attributeModel.Name] = item.SourceName;
            }
        }

        #endregion

    }

}