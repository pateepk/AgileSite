using System;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of the Sales force entity attribute with string values.
    /// </summary>
    public class EntityStringAttributeModel : EntityAttributeModel
    {

        #region "Constructors"

        internal EntityStringAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type) : base(entityModel, source, type)
        {

        }

        #endregion

        #region "Methods"

        internal override object ConvertValue(object value)
        {
            return Convert.ToString(value);
        }

        internal override string SerializeValue(object value)
        {
            return (string)value;
        }

        internal override object DeserializeValue(string value)
        {
            return value;
        }

        #endregion

    }

}