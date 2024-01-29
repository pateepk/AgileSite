using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of SalesForce entities.
    /// </summary>
    [Serializable]
    public sealed class EntityModel : ISerializable
    {

        #region "Private members"

        private WebServiceClient.DescribeSObjectResult mSource;
        private List<EntityAttributeModel> mAttributeModels;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the name of this entity model.
        /// </summary>
        public string Name
        {
            get
            {
                return mSource.name;
            }
        }

        /// <summary>
        /// Gets the collection of attribute models.
        /// </summary>
        public IEnumerable<EntityAttributeModel> AttributeModels
        {
            get
            {
                return mAttributeModels.AsEnumerable();
            }
        }

        #endregion

        #region "Constructors"

        internal EntityModel(WebServiceClient.DescribeSObjectResult source)
        {
            mSource = source;
            mAttributeModels = mSource.fields.Select(x => CreateEntityAttributeModel(x)).ToList();
        }

        private EntityModel(SerializationInfo info, StreamingContext context)
        {
            mSource = info.GetValue("Source", typeof(WebServiceClient.DescribeSObjectResult)) as WebServiceClient.DescribeSObjectResult;
            mAttributeModels = mSource.fields.Select(x => CreateEntityAttributeModel(x)).ToList();
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Searches for an attribute model with the specified name, and returns it.
        /// </summary>
        /// <param name="name">The name of the entity attribute to search for.</param>
        /// <returns>The attribute model with the specified name, if found; otherwise, null.</returns>
        public EntityAttributeModel GetAttributeModel(string name)
        {
            return mAttributeModels.SingleOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Creates a new instance of SalesForce entity using this model, and returns it.
        /// </summary>
        /// <returns>A new instance of SalesForce entity using this model.</returns>
        public Entity CreateEntity()
        {
            return new Entity(this);
        }

        /// <summary>
        /// This method supports the .NET Framework serialization process.
        /// </summary>
        /// <param name="info">An instance of the SerializationInfo class.</param>
        /// <param name="context">An instance of the StreamingContext class.</param>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Source", mSource);
        }

        #endregion

        #region "Private methods"

        private EntityAttributeModel CreateEntityAttributeModel(WebServiceClient.Field field)
        {
            switch (field.type)
            {
                case WebServiceClient.fieldType.anyType: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.Any);
                case WebServiceClient.fieldType.base64: return new EntityBinaryAttributeModel(this, field, EntityAttributeValueType.Base64);
                case WebServiceClient.fieldType.boolean: return new EntityBooleanAttributeModel(this, field, EntityAttributeValueType.Boolean);
                case WebServiceClient.fieldType.combobox: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.Combobox);
                case WebServiceClient.fieldType.currency: return new EntityDoubleAttributeModel(this, field, EntityAttributeValueType.Currency);
                case WebServiceClient.fieldType.datacategorygroupreference: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.DataCategoryGroupReference);
                case WebServiceClient.fieldType.date: return new EntityDateTimeAttributeModel(this, field, EntityAttributeValueType.Date);
                case WebServiceClient.fieldType.datetime: return new EntityDateTimeAttributeModel(this, field, EntityAttributeValueType.DateTime);
                case WebServiceClient.fieldType.@double: return new EntityDoubleAttributeModel(this, field, EntityAttributeValueType.Double);
                case WebServiceClient.fieldType.email: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.EmailAddress);
                case WebServiceClient.fieldType.encryptedstring: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.EncryptedString);
                case WebServiceClient.fieldType.id: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.Id);
                case WebServiceClient.fieldType.@int: return new EntityIntegerAttributeModel(this, field, EntityAttributeValueType.Integer);
                case WebServiceClient.fieldType.multipicklist: return new EntityMultiPicklistAttributeModel(this, field, EntityAttributeValueType.MultiPicklist);
                case WebServiceClient.fieldType.percent: return new EntityDoubleAttributeModel(this, field, EntityAttributeValueType.Percent);
                case WebServiceClient.fieldType.phone: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.PhoneNumber);
                case WebServiceClient.fieldType.picklist: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.Picklist);
                case WebServiceClient.fieldType.reference: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.Reference);
                case WebServiceClient.fieldType.@string: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.String);
                case WebServiceClient.fieldType.textarea: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.Textarea);
                case WebServiceClient.fieldType.time: return new EntityDateTimeAttributeModel(this, field, EntityAttributeValueType.Time);
                case WebServiceClient.fieldType.url: return new EntityStringAttributeModel(this, field, EntityAttributeValueType.Url);
            }
            throw new ArgumentException("[EntityModel.CreateEntityAttributeModel]: Invalid field type.");
        }

        #endregion

    }

}