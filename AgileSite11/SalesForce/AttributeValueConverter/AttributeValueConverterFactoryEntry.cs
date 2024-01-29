using System;
using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.SalesForce
{
    internal sealed class AttributeValueConverterFactoryEntry
    {
        #region "Private members"

        private EntityAttributeValueType mAttributeType;
        private string[] mFieldTypes;
        private Func<EntityAttributeModel, FormFieldInfo, AttributeValueConverterBase> mActivator;

        #endregion

        #region "Public properties"

        public EntityAttributeValueType AttributeType
        {
            get
            {
                return mAttributeType;
            }
        }

        public string[] FieldTypes
        {
            get
            {
                return mFieldTypes;
            }
        }

        public Func<EntityAttributeModel, FormFieldInfo, AttributeValueConverterBase> Activator
        {
            get
            {
                return mActivator;
            }
        }

        #endregion

        #region "Constructors"

        public AttributeValueConverterFactoryEntry(EntityAttributeValueType attributeType, string fieldType, Func<EntityAttributeModel, FormFieldInfo, AttributeValueConverterBase> activator)
        {
            mAttributeType = attributeType;
            mFieldTypes = new string[] { fieldType };
            mActivator = activator;
        }

        public AttributeValueConverterFactoryEntry(EntityAttributeValueType attributeType, string[] fieldTypes, Func<EntityAttributeModel, FormFieldInfo, AttributeValueConverterBase> activator)
        {
            mAttributeType = attributeType;
            mFieldTypes = fieldTypes;
            mActivator = activator;
        }

        #endregion
    }
}