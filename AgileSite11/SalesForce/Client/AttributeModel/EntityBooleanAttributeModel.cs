using System;
using System.Xml;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of the Sales force entity attribute with boolean values.
    /// </summary>
    public class EntityBooleanAttributeModel : EntityAttributeModel
    {

        #region "Constructors"

        internal EntityBooleanAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type) : base(entityModel, source, type)
        {

        }

        #endregion

        #region "Methods"

        internal override object ConvertValue(object value)
        {
            return Convert.ToBoolean(value);
        }

        internal override string SerializeValue(object value)
        {
            return ((bool)value) ? "1" : "0";
        }

        internal override object DeserializeValue(string value)
        {
            return XmlConvert.ToBoolean(value);
        }

        #endregion

    }

}